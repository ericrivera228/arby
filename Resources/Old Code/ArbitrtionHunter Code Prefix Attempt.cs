private static decimal DetermineBuyTradeAmount(Decimal availableBtc, Order order, Decimal availableFiat, Decimal buyExchangeTradeFeeAsDecimal, CurrencyType currencyTypeBuyFeeIsAppliedTo)
{
	decimal returnAmount = Math.Min(availableBtc, order.Amount);
	decimal costOfBtc;

	if (currencyTypeBuyFeeIsAppliedTo == CurrencyType.Fiat)
	{
		costOfBtc = Decimal.Multiply(Decimal.Multiply(returnAmount, order.Price), Decimal.Add(buyExchangeTradeFeeAsDecimal, 1));
	}
	else
	{
		costOfBtc = Decimal.Multiply(returnAmount, order.Price);
	}

	//Not enough fiat available to just fill the order or use all the available btc; calculate how much btc can be bought
	if (costOfBtc > availableFiat)
	{
		//Calculate the amount of btc that can be bought based upon how the buy fee is applied
		if (currencyTypeBuyFeeIsAppliedTo == CurrencyType.Fiat)
		{
			returnAmount = Decimal.Divide(availableFiat, Decimal.Multiply(order.Price, Decimal.Add(buyExchangeTradeFeeAsDecimal, 1)));
		}
		else
		{
			returnAmount = Decimal.Divide(availableFiat, order.Price);
		}
	}

	return returnAmount;
}

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

                //Part of the ask (buy) order has been fulfilled and the fee is applied to fiat at the buy exchange
                if (ask.Amount > bid.Amount)
                {
                    buyAmount = DetermineBuyTradeAmount(availableBtc, bid, availableFiat, buyExchange.TradeFeeAsDecimal, buyExchange.CurrencyTypeBuyFeeIsAppliedTo);
                    calculateProfitResult = CalculateArbitration(buyAmount, buyExchange, sellExchange, ref returnOpportunity, ask, bid, ref availableBtc, ref availableFiat);

                    if (calculateProfitResult)
                    {
                        ask.Amount = decimal.Subtract(ask.Amount, buyAmount);

                        //If the entire bid order was filled, remove it
                        if (buyAmount == bid.Amount)
                        {
                            runningBidList.RemoveAt(0);
                        }
                    }
                }

                //All of the ask (buy) order has been filled
                else if (ask.Amount < bid.Amount)
                {
                    buyAmount = DetermineBuyTradeAmount(availableBtc, ask, availableFiat, buyExchange.TradeFeeAsDecimal, buyExchange.CurrencyTypeBuyFeeIsAppliedTo);
                    calculateProfitResult = CalculateArbitration(buyAmount, buyExchange, sellExchange, ref returnOpportunity, ask, bid, ref availableBtc, ref availableFiat);

                    if (calculateProfitResult)
                    {
                        bid.Amount = Decimal.Subtract(bid.Amount, buyAmount);

                        //If the entire ask order was filled, remove it
                        if (buyAmount == ask.Amount)
                        {
                            runningAskList.RemoveAt(0);
                        }
                    }
                }

                //The ask and buy orders are the same amount so the both get filled
                else
                {
                    buyAmount = DetermineBuyTradeAmount(availableBtc, ask, availableFiat, buyExchange.TradeFeeAsDecimal, buyExchange.CurrencyTypeBuyFeeIsAppliedTo);
                    calculateProfitResult = CalculateArbitration(buyAmount, buyExchange, sellExchange, ref returnOpportunity, ask, bid, ref availableBtc, ref availableFiat);

                    if (calculateProfitResult)
                    {
                        //If both orders were wholly filled, delete them. Note, since this else block is for asks and bids whose amount are the same,
                        //tradeAmount only needs to be checked against one of the orders
                        if (buyAmount == ask.Amount)
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

                //Arbitration calculation finished, now take into account the transfer trade fee if specified:
                if (accountForBtcTransferFee)
                {
                    //Calculate the average btc cost in fiat
                    decimal averageCost = (returnOpportunity.BuyPrice + returnOpportunity.SellPrice) / 2;

                    //Using the average buy/sell cost, determine if the opportunity is stil profitable after losing that amount of btc in dollars.
                    returnOpportunity.Profit -= returnOpportunity.BuyExchange.BtcTransferFee * averageCost;
                }

                //Now that arbitration has been fully calculated between the two exchange, and the opportunity has the required minimum profit, see if the buy and sell orders are valid:
                if (returnOpportunity.Profit >= minimumProfit)
                {
                    Order buyOrder = new Order(returnOpportunity.BuyAmount, returnOpportunity.BuyPrice);
                    Order sellOrder = new Order(returnOpportunity.BuyAmount, returnOpportunity.SellPrice);

                    if (returnOpportunity.BuyExchange.IsOrderValidForExchange(buyOrder) && returnOpportunity.SellExchange.IsOrderValidForExchange(sellOrder))
                    {
                        return returnOpportunity;
                    }
                }
            }

            return null;
        }