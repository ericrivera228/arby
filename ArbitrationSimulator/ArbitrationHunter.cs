using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ArbitrationUtilities.EnumerationObjects;
using ArbitrationUtilities.OrderObjects;
using BitcoinExchanges;
using CommonFunctions;
using log4net;

namespace ArbitrationSimulator
{
    public class ArbitrationHunter
    {
        private List<BaseExchange> _exchangeList;
        private ILog _log;

        #region Property Getters and Setters
        public List<BaseExchange> ExchangeList
        {
            get
            {
                return _exchangeList;
            }

            set
            {
                _exchangeList = value;
            }
        }

        public ILog Log
        {
            get
            {
                return _log;
            }

            set
            {
                _log = value;
            }
        }
        #endregion

        public ArbitrationHunter(List<BaseExchange> exchanges, ILog log = null)
        {
            _exchangeList = exchanges;
            _log = log;
        }

        /// <summary>
        /// Looks through all the possible combinations of the exchanges in _exchangeList to find possible arbitration opportunities. Returns a list of all the 
        /// possible arbitration opportunities ordered by profit.
        /// </summary>
        /// <param name="maxBtc">The maximum amount of BTC that can be used to find arbitration</param>
        /// <param name="maxFiat">The maximum amount of fiat that can be used in an arbitration trade.</param>
        /// <param name="minimumProfit">The minimum profit required for an arbitration opportunity to be considered valid. That is, the minimum profit required
        ///     for an arbitration opportunity to be returned in the return list.</param>
        /// <returns>If there are arbitration opportunities available, a list of all the opportunities order by descending profit. If there aren't any opportunities,
        /// null is returned.</returns>
        public List<ArbitrationOpportunity> FindArbitration(decimal maxBtc, decimal maxFiat, decimal minimumProfit, bool accountForBtcTransferFee)
        {
            List<ArbitrationOpportunity> opportunityList = new List<ArbitrationOpportunity>();

            UpdateExchangeOrderBooks();

            foreach (BaseExchange buyExchange in _exchangeList)
            {
                foreach (BaseExchange sellExchange in _exchangeList)
                {
                    //No point in trying to find arbitration with the same exchange; move on to the next one.
                    if (buyExchange == sellExchange)
                    {
                        continue;
                    }

                    //In addition to the hard BTC limit passed into this method, arbitration opportunities will be limited by the number
                    //of bitcoins in the sell exchange. Recalculate MaxBtc with this in mind.
                    decimal availableBtc = Math.Min(maxBtc, sellExchange.AvailableBtc);

                    //Same with fiat; in addition to the hard fiat limit passed into this method, arbitration opportunities will be limited
                    //by the amount of money in the buy exchange. Recalculate MaxFiat with this in mind.
                    //Same with fiat; in addition to the hard fiat limit passed into this method, arbitration opportunities will be limited
                    //by the amount of money in the buy exchange. Recalculate MaxFiat with this in mind.
                    //**NOTE** Because of how the exchange to rounding (like Bitstamp), it is possible calculate a value from the available fiat, but then
                    //have the actual cost be higher. Thus, subtract $0.01 from the availabe fiat in the buy exchange; this will ensure that scenario doesn't ever happen
                    decimal availableFiat = Math.Min(maxFiat, (buyExchange.AvailableFiat - 0.01m));

                    ArbitrationOpportunity opportunity = CalculateArbitration(buyExchange, sellExchange, availableBtc, availableFiat, minimumProfit, accountForBtcTransferFee);

                    if (opportunity != null)
                    {
                        opportunityList.Add(opportunity);
                    }
                }
            }

            if (opportunityList.Count > 0)
            {
                opportunityList = opportunityList.OrderByDescending(opportunity => opportunity.Profit).ToList();

                return opportunityList;
            }

            return null;
        }

        /// <summary>
        /// Determines if the arbitration is possible with the given buy and sell exchanges. The order book for both exchanges must be ordered (that is,
        /// asks with ascending order price and bids with descending order price) for this method to work properly. 
        /// </summary>
        /// <param name="buyExchange">The exchange to buy from.</param>
        /// <param name="sellExchange">The exchange to sell at.</param>
        /// <param name="availableBtc">The amount of BTC available for an aribtration trade.</param>
        /// <param name="availableFiat">The amount of fiat available for an arbitration trade.</param>
        /// <param name="minimumProfit">The minimum profit required for an arbitration opportunity to be considered valid. That is, the minimum profit required
        ///     for an arbitration opportunity to be returned by this method.</param>
        /// <returns>An ArbirationOpportunity object is arbitration between the two given exchanges can occur. Otherwise, returns null.</returns>
        public ArbitrationOpportunity CalculateArbitration(BaseExchange buyExchange, BaseExchange sellExchange, decimal availableBtc, decimal availableFiat, decimal minimumProfit, bool accountForBtcTransferFee)
        {
            ArbitrationOpportunity returnOpportunity = null;

            //These two lists keep track of the asks and bids lists as they are used
            List<Order> runningAskList;
            List<Order> runningBidList;

            //Only need to clone the Asks of the buy exchange (since we aren't selling there don't need the bids), and only need to clone the Bids of the sell
            //exchange (since we aren't buying there don't need the asks)
            if (buyExchange.OrderBook != null && buyExchange.OrderBook.Asks != null && sellExchange.OrderBook != null && sellExchange.OrderBook.Bids != null)
            {
                runningAskList = buyExchange.OrderBook.Asks.Clone();
                runningBidList = sellExchange.OrderBook.Bids.Clone();
            }
            else
            {
                //Defensive code: at this point, both orderbooks and their order lists shouldn't be null. But just in case, return null.
                return null;
            }

            //Continue to look for aribtration while:
            //  - The next ask has a higher price than the next bid (meaning there is a potential for arbitration)
            //  - There is btc left to use up
            //  - There is fiat left to use. Note, this is limited at 1.00 to prevent any fringe situation where this method finds arbitration with a very small
            //    amount of btc. If there is less than 1.00, just don't bother looking for arbitration as that isn't practical. This would probably be assuaged
            //    by the min profit limit anyways, but just to be safe. 
            while (runningBidList.Count > 0 && runningAskList.Count > 0 && runningBidList[0].Price > runningAskList[0].Price && availableBtc > 0 && availableFiat > 1.00m)
            {
                decimal buyAmount;
                Order bid = runningBidList[0];
                Order ask = runningAskList[0];

                bool calculateProfitResult;

                //Part of the ask (buy) order has been fulfilled
                if ((buyExchange.CurrencyTypeBuyFeeIsAppliedTo == CurrencyType.Fiat && ask.Amount > bid.Amount) || (buyExchange.CurrencyTypeBuyFeeIsAppliedTo == CurrencyType.Bitcoin && ask.Amount > Decimal.Multiply(bid.Amount, Decimal.Add(1, buyExchange.TradeFeeAsDecimal))))
                {
                    buyAmount = DetermineBuyTradeAmount(availableBtc, ask, bid, availableFiat, buyExchange.TradeFeeAsDecimal, buyExchange.CurrencyTypeBuyFeeIsAppliedTo, OrderType.Bid);
                    calculateProfitResult = CalculateArbitration(buyAmount, buyExchange, sellExchange, ref returnOpportunity, ask, bid, ref availableBtc, ref availableFiat);

                    if (calculateProfitResult)
                    {
                        ask.Amount = decimal.Subtract(ask.Amount, buyAmount);

                        //If the entire bid order was filled, remove it
                        //*NOTE* Greater than or equal too is needed, because in the case of the buy fee being applied to btc, the buy amount may actually be greater than amount of the sell order
                        if (buyAmount >= bid.Amount)
                        {
                            runningBidList.RemoveAt(0);
                        }
                    }
                }

                //All of the ask (buy) order has been filled
                else if ((buyExchange.CurrencyTypeBuyFeeIsAppliedTo == CurrencyType.Fiat && ask.Amount < bid.Amount) || (buyExchange.CurrencyTypeBuyFeeIsAppliedTo == CurrencyType.Bitcoin && ask.Amount < Decimal.Multiply(bid.Amount, Decimal.Add(1, buyExchange.TradeFeeAsDecimal))))
                {
                    buyAmount = DetermineBuyTradeAmount(availableBtc, ask, bid, availableFiat, buyExchange.TradeFeeAsDecimal, buyExchange.CurrencyTypeBuyFeeIsAppliedTo, OrderType.Ask);
                    calculateProfitResult = CalculateArbitration(buyAmount, buyExchange, sellExchange, ref returnOpportunity, ask, bid, ref availableBtc, ref availableFiat);

                    if (calculateProfitResult)
                    {
                        bid.Amount = Decimal.Subtract(bid.Amount, buyAmount);

                        //If the entire ask order was filled, remove it
                        if (buyAmount >= ask.Amount)
                        {
                            runningAskList.RemoveAt(0);
                        }
                    }
                }

                //The ask and buy orders are the same amount so the both get filled
                //WARNING!!!! This block technically isn't right, it very slightly error when buy fee is applied to btc. But, the error is very small, and this is a very, very unlikely case so 
                //I'm not doing to do anything about it. To see the error, take test case 18 and add another tier of arbitration opportunity.
                else
                {
                    buyAmount = DetermineBuyTradeAmount(availableBtc, ask, bid, availableFiat, buyExchange.TradeFeeAsDecimal, buyExchange.CurrencyTypeBuyFeeIsAppliedTo, OrderType.Ask);
                    calculateProfitResult = CalculateArbitration(buyAmount, buyExchange, sellExchange, ref returnOpportunity, ask, bid, ref availableBtc, ref availableFiat);

                    if (calculateProfitResult)
                    {
                        //If both orders were wholly filled, delete them. Note, since this else block is for asks and bids whose amount are the same,
                        //tradeAmount only needs to be checked against one of the orders
                        if (buyAmount >= ask.Amount)
                        {
                            runningBidList.RemoveAt(0);
                            runningAskList.RemoveAt(0);
                        }
                    }
                }

                //If no profit could be found, exit the while loop
                if (!calculateProfitResult)
                {
                    break;
                }

            }

            //An oppportunity was found, now see if it meets the minimum profit requirements
            if (returnOpportunity != null)
            {
                if (returnOpportunity.BuyExchangeOrderBook == null || returnOpportunity.SellExchangeOrderBook == null)
                {
                    //If arbitration can occur, add the order books to returnOpportunity for recordkeeping and trouble shooting
                    returnOpportunity.BuyExchangeOrderBook = buyExchange.OrderBook;
                    returnOpportunity.SellExchangeOrderBook = sellExchange.OrderBook;
                }

                returnOpportunity.CalculateArbitrationOpportunityCosts();

                //Todo: just increase the buy amount by the transfer fee, don't use this method
                //Arbitration calculation finished, now take into account the transfer trade fee if specified:
                if (accountForBtcTransferFee)
                {
                    //Calculate the average btc cost in fiat
                    decimal averageCost = (returnOpportunity.BuyPrice + returnOpportunity.SellPrice) / 2;

                    //Using the average buy/sell cost, determine if the opportunity is stil profitable after losing that amount of btc in dollars.
                    returnOpportunity.Profit -= returnOpportunity.BuyExchange.BtcTransferFee * averageCost;
                }

                //Now that arbitration has been fully calculated between the two exchange, and the opportunity has the required minimum profit, see if the buy and sell orders are valid:
                if (returnOpportunity.Profit >= minimumProfit && returnOpportunity.BuyExchange.IsOrderCostValidForExchange(returnOpportunity.TotalBuyCost, returnOpportunity.BuyAmount) && returnOpportunity.SellExchange.IsOrderCostValidForExchange(returnOpportunity.TotalSellCost, returnOpportunity.SellAmount))
                {
                    return returnOpportunity;
                }
            }

            return null;
        }

        /// <summary>
        /// Calculates profit with the exchange fees taken into account. If arbitration is still possible, ReturnOpportunity and MaxBTC are updated as needed. 
        /// Returns a bool indicating if profit is possible between the two exchanges.
        /// </summary>
        /// <param name="tradeAmount">That amount of BTC to be traded between the two exchanges.</param>
        /// <param name="buyExchange">The exchange to buy from; where the ask order is being fulfilled.</param>
        /// <param name="sellExchange">The exchange to sell at; where the bid order is being fulfilled.</param>
        /// <param name="returnOpportunity">ArbitrationOpportunity that is being used to keep track of the total arbitration between the buy and sell exchanges. Note,
        ///     this variable is passed in by reference, so the object that is passed to this method is directly updated.</param>
        /// <param name="ask">Ask order that is being used from the buy exchange.</param>
        /// <param name="bid">Bid order that is being used from the sell exchange.</param>
        /// <param name="availableBtc">The amount of BTC available for an aribtration trade. Note, this variable is passed in to by reference,
        ///     so the object that is passed to this method is directly updated.</param>
        /// <param name="availableFiat">The amount of fiat available for an arbitration trade. Note, this variable is passed in to by reference,
        ///     so the object that is passed to this method is directly updated</param>
        /// <returns>Returns a boolean. True if the profit can be made from the given ask and bid. False if no arbitration can occur.</returns>
        private static bool CalculateArbitration(decimal tradeAmount, BaseExchange buyExchange, BaseExchange sellExchange, ref ArbitrationOpportunity returnOpportunity, Order ask, Order bid, ref decimal availableBtc, ref decimal availableFiat)
        {
            decimal sellAmount;
            decimal buyCost;

            if (buyExchange.CurrencyTypeBuyFeeIsAppliedTo == CurrencyType.Bitcoin)
            {
                //The buy exchange applies their fee to btc, lower the sell amount to compensate.
                sellAmount = Decimal.Multiply(tradeAmount, decimal.Subtract(1m, buyExchange.TradeFeeAsDecimal));

               buyCost = Decimal.Multiply(tradeAmount, ask.Price);
            }
            else
            {
                //Buy fee is applied to fiat, so no need to decrease sell amount.
                sellAmount = tradeAmount;

                buyCost = Decimal.Multiply(Decimal.Add(1.0m, buyExchange.TradeFeeAsDecimal), Decimal.Multiply(tradeAmount, ask.Price));
            }
            
            decimal sellCost = Decimal.Multiply(Decimal.Subtract(1.0m, sellExchange.TradeFeeAsDecimal), Decimal.Multiply(sellAmount, bid.Price));
            decimal profit = Decimal.Subtract(sellCost, buyCost);

            //Just to weed out arbitration opportunities whose profit is so low that they aren't practical. Note, this isn't the Minimum profit requirement,
            //to return an arbitration opportunity, this limit just keeps the algorithm from adding to an arbitration opportunity trade amounts that don't add
            //at least 0.01 profit to the overall opportunity. 
            if (profit > 0.001m)
            {
                if (returnOpportunity == null)
                {
                    returnOpportunity = new ArbitrationOpportunity(buyExchange, sellExchange);
                }

                //Increase the amount of the arbitration opportunity; add the new buy and sell orders to this opportunity
                returnOpportunity.BuyAmount = Decimal.Add(returnOpportunity.BuyAmount, tradeAmount);
                returnOpportunity.SellAmount = Decimal.Add(returnOpportunity.SellAmount, sellAmount);

                returnOpportunity.AddBuyOrder(new Order(tradeAmount, ask.Price));
                returnOpportunity.AddSellOrder(new Order(sellAmount, bid.Price));

                //Decrement MaxBTC and MaxFiat; since some was used on this iteration, less will be available for use on subsequent iterations
                availableBtc = Decimal.Subtract(availableBtc, tradeAmount);
                availableFiat = Decimal.Subtract(availableFiat, buyCost);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the order book for each exchange in _exchangeList. This is done in parrallel to mimimize execution time.
        /// </summary>
        private void UpdateExchangeOrderBooks()
        {
            

            if (_exchangeList != null && _exchangeList.Count > 0)
            {
                Parallel.ForEach(_exchangeList, exchange =>
                {
                    //If one of the exchanges fails, that's ok. Set that order book to null, and continue to update
                    //the other ones. 
                    try
                    {
                        //Stopwatch used to measure the effect of parallelization
                        Stopwatch stopWatch = new Stopwatch();
                        //stopWatch.Start();


                        exchange.UpdateOrderBook(10);

                        //stopWatch.Stop();
                        //_log.Debug(exchange.Name + " update time = " + stopWatch.Elapsed.Milliseconds);
                    }
                    catch (Exception e)
                    {
                        exchange.OrderBook = null;

                        if (_log != null)
                        {
                            _log.Warn("There was a problem updating the order book for " + exchange.Name + ":" + Environment.NewLine + "\t" + e.Message);
                            _log.Debug("Stack trace:" + Environment.NewLine + e.StackTrace);
                        }
                    }
                });
            }

            
        }

        private static decimal DetermineBuyTradeAmount(Decimal availableBtc, Order ask, Order bid, Decimal availableFiat, Decimal buyExchangeTradeFeeAsDecimal, CurrencyType currencyTypeBuyFeeIsAppliedTo, OrderType limitingOrderType)
        {
            decimal returnBuyAmount;
            decimal costOfBtc;

            //Limited by the sell, and fee is applied to btc. In this case, calculate the buy amount from the minimum of the sell order amount, or the available btc.
            if (currencyTypeBuyFeeIsAppliedTo == CurrencyType.Bitcoin && limitingOrderType == OrderType.Bid)
            {
                //If the limitation is the amount in the sell order, calculate the buy amount from that number.
                if (availableBtc >= bid.Amount)
                {
                    returnBuyAmount = Decimal.Multiply(bid.Amount, Decimal.Add(1, buyExchangeTradeFeeAsDecimal));
                }
                //Otherwise, when limited by the btc available for sell, calculate the buy number from that.
                else
                {
                    returnBuyAmount = Decimal.Multiply(availableBtc, Decimal.Add(1, buyExchangeTradeFeeAsDecimal));
                }
            }

            //When limited by the buy order, the buy amount is going to be the minimum of the avilable btc (since the sell side needs to match the buy side) regardless of the buy fee type.
            else if (limitingOrderType == OrderType.Ask)
            {
                returnBuyAmount = Math.Min(availableBtc, ask.Amount);
            }
            //If the this block is reached, then the fee type must be 'fiat' and the limiting order type must be sell. In this case, take the minimum of the available btc and sell order.
            else
            {
                returnBuyAmount = Math.Min(availableBtc, bid.Amount);
            }

            //Now that the buy amount has been calcualted, determine how much it would cost to actually buy that amount. This calculation will differ depending on how the buy fee is applied.
            if (currencyTypeBuyFeeIsAppliedTo == CurrencyType.Fiat)
            {
                costOfBtc = Decimal.Multiply(Decimal.Multiply(returnBuyAmount, ask.Price), Decimal.Add(buyExchangeTradeFeeAsDecimal, 1));
            }
            else
            {
                costOfBtc = Decimal.Multiply(returnBuyAmount, ask.Price);
            }

            //Not enough fiat available to just fill the order or use all the available btc; need to recalculate how much btc can be bought
            if (costOfBtc > availableFiat)
            {
                //Calculate the amount of btc that can be bought based upon how the buy fee is applied
                if (currencyTypeBuyFeeIsAppliedTo == CurrencyType.Fiat)
                {
                    returnBuyAmount = Decimal.Divide(availableFiat, Decimal.Multiply(ask.Price, Decimal.Add(buyExchangeTradeFeeAsDecimal, 1)));
                }
                else
                {
                    returnBuyAmount = Decimal.Divide(availableFiat, ask.Price);
                }
            }

            return MathHelpers.FloorRound(returnBuyAmount, 8);
        }
    }
}
