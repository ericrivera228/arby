using System;
using System.Linq;
using ArbitrationUtilities.EnumerationObjects;
using ArbitrationUtilities.OrderObjects;
using BitcoinExchanges;
using CommonFunctions;
using DatabaseLayer;

namespace ArbitrationSimulator
{
    public class ArbitrationOpportunity : IDomainObject
    {
        private int? _id;
        private int? _arbitrationRunId;
        private int? _transferId;
        private decimal _buyAmount;
        private decimal _sellAmount;
        private decimal _buyPrice;
        private decimal _totalBuyCost;
        private string _buyOrderId;
        private decimal _sellPrice;
        private decimal _totalSellCost;
        private string _sellOrderId;
        private decimal _profit;
        private OrderList _buyOrderList = new OrderList();         //List of the component buy orders which made up thise arbitration opportunity.
        private OrderList _sellOrderList = new OrderList();         //List of the component sell orders which made up thise arbitration opportunity.
        private BaseExchange _buyExchange;
        private BaseExchange _sellExchange;
        private OrderBook _buyExchangeOrderBook;
        private OrderBook _sellExchangeOrderBook;
        private DateTime? _executionDateTime;
        private DateTime? _createDateTime;
        private DateTime? _lastModifiedDateTime;
    
        #region Property Getters and Setters

        /// <summary>
        /// Id of the record in the Arbitration_Trade table; null if it doesn't exist in the table
        /// </summary>
        public int? Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
            }
        }

        /// <summary>
        /// The id of the arbitration run this particular object belongs to. Note, because a lot of works is done with caclulating opportunities
        /// before it is actually stored in the db, this is allowed to be null. 
        /// </summary>
        public int? ArbitrationRunId
        {
            get
            {
                return _arbitrationRunId;
            }

            set
            {
                _arbitrationRunId = value;
            }
        }

        /// <summary>
        /// The id of the transfer that is executed at least in part because of this arbitration trade. Note, this is allowed to be null
        /// because arbitration trades are executed and persisted to the db before the transfer takes place.
        /// </summary>
        public int? TransferId
        {
            get
            {
                return _transferId;
            }

            set
            {
                _transferId = value;
            }
        }

        /// <summary>
        /// Total amount of BTC to for the buy side of this arbitration opportunity. Some exchanges apply the buy fee to bitcoins, and thus
        /// the buy and sell amounts may be different.
        /// </summary>
        public decimal BuyAmount
        {
            get
            {
                return _buyAmount;
            }

            set
            {
                _buyAmount = value;
            }
        }

        /// <summary>
        /// Total amount of BTC to for the sell side of this arbitration opportunity. Some exchanges apply the buy fee to bitcoins, and thus
        /// the buy and sell amounts may be different.
        /// </summary>
        public decimal SellAmount
        {
            get
            {
                return _sellAmount;
            }

            set
            {
                _sellAmount = value;
            }
        }

        /// <summary>
        /// Highest buy price to fulfill for this arbitration opportunity.
        /// </summary>
        public decimal BuyPrice
        {
            get
            {
                return _buyPrice;
            }

            set
            {
                _buyPrice = value;
            }
        }

        /// <summary>
        /// Total amount of money to be spent for this arbitration opportunity.
        /// </summary>
        public decimal TotalBuyCost
        {
            get
            {
                return _totalBuyCost;
            }

            set
            {
                _totalBuyCost = value;
            }
        }

        /// <summary>
        /// Unique identifier for the buy order; string.
        /// </summary>
        public string BuyOrderId
        {
            get
            {
                return _buyOrderId;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception("Buy order id cannot be a null/empty string.");
                }

                _buyOrderId = value;
            }
        }

        /// <summary>
        /// Lowest sell price to fulfill for this arbitration opportunity.
        /// </summary>
        public decimal SellPrice
        {
            get
            {
                return _sellPrice;
            }

            set
            {
                _sellPrice = value;
            }
        }

        /// <summary>
        /// Total amount of money to be earned for this arbitration opportunity.
        /// </summary>
        public decimal TotalSellCost
        {
            get
            {
                return _totalSellCost;
            }

            set
            {
                _totalSellCost = value;
            }
        }

        /// <summary>
        /// Unique identifier for the sell order.
        /// </summary>
        public string SellOrderId
        {
            get
            {
                return _sellOrderId;
            }

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    throw new Exception("Sell order id cannot be null/empty");
                }

                _sellOrderId = value;
            }
        }

        /// <summary>
        /// Total profit of money to be earned for this arbitration opportunity.
        /// </summary>
        public decimal Profit
        {
            get
            {
                return _profit;
            }

            set
            {
                _profit = value;
            }
        }

        /// <summary>
        /// The exchange to buy from for this arbitration opportunity.
        /// </summary>
        public BaseExchange BuyExchange
        { 
            get
            {
                return _buyExchange;
            }

            set
            {
                _buyExchange = value;
            }
        }

        /// <summary>
        /// The exchange sell sell at for this arbitration opportunity.
        /// </summary>
        public BaseExchange SellExchange
        {
            get
            {
                return _sellExchange;
            }

            set
            {
                _sellExchange = value;
            }
        }

        /// <summary>
        /// Order book of the buy exchange for this arbitration opportunity.
        /// </summary>
        public OrderBook BuyExchangeOrderBook
        {
            get
            {
                return _buyExchangeOrderBook;
            }

            set
            {
                _buyExchangeOrderBook = value;
            }
        }

        /// <summary>
        /// Order book of the sell exchange for this arbitration opportunity.
        /// </summary>
        public OrderBook SellExchangeOrderBook
        {
            get
            {
                return _sellExchangeOrderBook;
            }

            set
            {
                _sellExchangeOrderBook = value;
            }
        }

        /// <summary>
        /// Time this arbitration trade was executed
        /// </summary>
        public DateTime? ExecutionDateTime
        {
            get
            {
                return _executionDateTime;
            }

            set
            {
                _executionDateTime = value;
            }
        }

        /// <summary>
        /// Time this arbitration trade was put into the db.
        /// </summary>
        public DateTime? CreateDateTime
        {
            get
            {
                return _createDateTime;
            }

            set
            {
                _createDateTime = value;
            }
        }

        /// <summary>
        /// Time this arbitration trade was last updated.
        /// </summary>
        public DateTime? LastModifiedDateTime
        {
            get
            {
                return _lastModifiedDateTime;
            }

            set
            {
                _lastModifiedDateTime = value;
            }
        }
        #endregion

        public ArbitrationOpportunity(BaseExchange buyExchange, BaseExchange sellExchange)
        {
            _buyExchange = buyExchange;
            _sellExchange = sellExchange;
            _arbitrationRunId = ArbitrationRunId;
            _buyAmount = 0.0m;
            _buyPrice = 0.0m;
            _totalBuyCost = 0.0m;
            _sellPrice = 0.0m;
            _totalSellCost = 0.0m;
            _profit = 0.0m;
        }

        /// <summary>
        /// Legacy construtor that sets both buy and sell amount to the given amount. Leaving this constructor in here mosty for legacy purposes.
        /// </summary>
        /// <param name="amount">Sets both buy amount and sell amount.</param>
        public ArbitrationOpportunity(BaseExchange buyExchange, BaseExchange sellExchange, decimal amount, decimal buyPrice, decimal totalBuyCost, decimal sellPrice, decimal totalSellCost, decimal profit, int arbitrationRunId)
        {
            _buyExchange = buyExchange;
            _sellExchange = sellExchange;
            _arbitrationRunId = arbitrationRunId;
            _buyAmount = amount;
            _buyPrice = buyPrice;
            _totalBuyCost = totalBuyCost;
            _sellPrice = sellPrice;
            _totalSellCost = totalSellCost;
            _profit = profit;
        }

        public ArbitrationOpportunity(BaseExchange buyExchange, BaseExchange sellExchange, decimal buyAmount, decimal sellAmount, decimal buyPrice, decimal totalBuyCost, decimal sellPrice, decimal totalSellCost, decimal profit, int arbitrationRunId)
        {
            _buyExchange = buyExchange;
            _sellExchange = sellExchange;
            _arbitrationRunId = arbitrationRunId;
            _buyAmount = buyAmount;
            _sellAmount = sellAmount;
            _buyPrice = buyPrice;
            _totalBuyCost = totalBuyCost;
            _sellPrice = sellPrice;
            _totalSellCost = totalSellCost;
            _profit = profit;
        }

        /// <summary>
        /// Adds a buy order to the list of buy orders which comprise the buying side of this arbitration opportunity.
        /// </summary>
        /// <param name="order"></param>
        public void AddBuyOrder(Order ask)
        {
            _buyOrderList.Add(ask);
        }

        /// <summary>
        /// Adds a sell order to the list of sell orders which comprise the selling side of this arbitration opportunity.
        /// </summary>
        /// <param name="order"></param>
        public void AddSellOrder(Order bid)
        {
            _sellOrderList.Add(bid);
        }

        public void CalculateArbitrationOpportunityCosts()
        {
            //Round the amount to 4 decimal places, a requirement for most exchanges.
            RoundAmounts();

            //If all the buy or sell orders were removed, this arbitration opportunity is not valid, so set everything to zero
            if (_buyOrderList.Count <= 0 || _sellOrderList.Count <= 0)
            {
                TotalBuyCost = 0m;
                TotalSellCost = 0m;
            }

            else
            {
                //Set the buy and sell prices to the price other order on the bottom of each respective list.
                BuyPrice = _buyOrderList.Last().Price;
                SellPrice = _sellOrderList.Last().Price;

                //Calculate the total buy and sell costs
                TotalBuyCost = BuyExchange.ApplyFeeToBuyCost(CalculateTotalWorthOfOrderList(_buyOrderList));
                TotalSellCost = SellExchange.ApplyFeeToSellCost(CalculateTotalWorthOfOrderList(_sellOrderList));
            }

            //Calculate profit from total costs
            Profit = TotalSellCost - TotalBuyCost;
        }

        private decimal CalculateTotalWorthOfOrderList(OrderList orderList)
        {
            decimal returnWorth = 0.0m;

            foreach (Order order in orderList)
            {
                returnWorth += order.Worth;
            }

            return returnWorth;
        }

        private void RoundAmounts()
        {
            //In the case of fee being applied to fiat, then the buy and sell amounts are the same. Thus, round them
            //to the same number of decimal places.
            if (BuyExchange.CurrencyTypeBuyFeeIsAppliedTo == CurrencyType.Fiat)
            {
                //Round the trade amount to the lowest of the two two exchanges
                int decimalPlaces = Math.Min(BuyExchange.AmountDecimalPlace, SellExchange.AmountDecimalPlace);

                //Round the buy amount.
                BuyAmount = RoundAmount(BuyAmount, decimalPlaces, _buyOrderList, "floor");
                SellAmount = RoundAmount(SellAmount, decimalPlaces, _sellOrderList, "floor");
            }

            //In the case of fee being applied to btc
            else
            {
                //Sell exchange has less decimal places
                if (SellExchange.AmountDecimalPlace < BuyExchange.AmountDecimalPlace)
                {
                    //Round the sell amount
                    SellAmount = RoundAmount(SellAmount, SellExchange.AmountDecimalPlace, _sellOrderList, "floor");

                    //Recalculate the buy amount
                    decimal newBuyAmount = Decimal.Multiply(SellAmount, (Decimal.Add(1, BuyExchange.TradeFeeAsDecimal)));

                    BuyAmount = RoundAmount(BuyAmount, newBuyAmount, BuyExchange.AmountDecimalPlace, _buyOrderList, "regular");
                }

                else
                {
                    //Round the buy amount
                    BuyAmount = RoundAmount(BuyAmount, BuyExchange.AmountDecimalPlace, _buyOrderList, "floor");

                    //Recaluate the sell amount based upon the new buy amount
                    decimal newSellAmount = Decimal.Multiply(BuyAmount, (Decimal.Subtract(1, BuyExchange.TradeFeeAsDecimal)));

                    SellAmount = RoundAmount(SellAmount, newSellAmount, SellExchange.AmountDecimalPlace, _sellOrderList, "regular");
                }
            }
        }

        private decimal RoundAmount(decimal originalAmount, int decimalPlaces, OrderList orderList, string roundMethod)
        {
            return RoundAmount(originalAmount, originalAmount, decimalPlaces, orderList, roundMethod);
        }

        private decimal RoundAmount(decimal originalAmount, decimal newAmountToRound, int decimalPlaces, OrderList orderList, string roundMethod)
        {
            decimal newAmount;

            if (roundMethod == "floor")
            {
                newAmount = MathHelpers.FloorRound(newAmountToRound, decimalPlaces);    
            }
            else
            {
                newAmount = Math.Round(newAmountToRound, decimalPlaces);    
            }
            
            //Determine how much btc amount needs to be left off this order
            decimal choppedAmount = originalAmount - newAmount;

            //If there is an amount of btc to be lost from this trade because of the new calculation, remove it 
            if (choppedAmount > 0m)
            {
                RemoveAmountFromOrderList(choppedAmount, orderList);
            }
            else if (choppedAmount < 0m)
            {
                //Add the chopped amount to the last part of the order
                orderList[orderList.Count - 1].Amount = orderList[orderList.Count - 1].Amount + (-1*choppedAmount);
            }

            return newAmount;
        }

        /// <summary>
        /// Removes the specified amount of btc from the components orders of the given order list, starting from the bottom of the lists.
        /// </summary>
        /// <param name="amountToRemove">Amount of btc to remove from each side of this arbitration trade.</param>
        /// <param name="orderList">List of orders to remove the specified amount from.</param>
        private void RemoveAmountFromOrderList(decimal amountToRemove, OrderList orderList)
        {
            //Loop through the list of order components and remove the amount that is being chopped
            //from the round.
            while (amountToRemove > 0)
            {
                //Get the last order from the list
                Order lastOrder = orderList.Last();

                //Remove choppedAmount from the buy and sell sides

                //If the amount to be lost is greater than the order at the bottom of the list, 
                //remove that order.
                //Comparison is >= so that when the amount to be chopped is the same as the amount of the order, that order is removed.
                if (amountToRemove >= lastOrder.Amount)
                {
                    amountToRemove -= lastOrder.Amount;
                    orderList.RemoveAt(orderList.Count - 1);
                }

                //Order at the bottom of the tier is larger than the amount that is lost due to rounding;
                //remove chopped amount from only order.
                else
                {
                    lastOrder.Amount -= amountToRemove;

                    //Recalculate the worth for the order with the trade fee multiplier
                    //lastOrder.Worth = lastOrder.Amount*lastOrder.Price*tradeFeeMultiplier;
                    lastOrder.Worth = lastOrder.Amount * lastOrder.Price;

                    //Amount has been subtracted, break from the loop
                    break;
                }
            }
        }
        
        /// <summary>
        /// Saves this arbitration trade to the database. If this atribtration trade does not have an id, then an INSERT statement 
        /// is used. Otherwise, the record in the ARBITRATION_TRADE with the corresponding id is updated. This method throws any 
        /// that could occur when saving to the db.
        /// </summary>
        /// <returns>The id of this arbitration trade in the ARBITRATION_TRADE table. (If this is an update, id is unchanged).</returns>
        public int? PersistToDb()
        {
            string insertSql;
            string updateSql;

            //Make sure this object is tied to an arbitration run before saving it to the db
            if(_arbitrationRunId == null)
            {
                throw new ArgumentNullException("Cannot persist an arbitration trade until it has an arbitration run id.");
            }

            //Whether this is an update or an insert, the last modified date time will need to be updated.
            _lastModifiedDateTime = DateTime.Now;

            string buyExchangeName = DatabaseManager.FormatStringForDb(_buyExchange != null ? _buyExchange.Name : null);
            string sellExchangeName = DatabaseManager.FormatStringForDb(_sellExchange != null ? _sellExchange.Name : null);
            string buyOrderIdString = DatabaseManager.FormatStringForDb(_buyOrderId);
            string sellOrderIdString = DatabaseManager.FormatStringForDb(_sellOrderId);
            string executionDateTimeString = DatabaseManager.FormatDateTimeForDb(_executionDateTime);
            string lastModifiedDateTimeString = DatabaseManager.FormatDateTimeForDb(_lastModifiedDateTime);
            string transferIdString = DatabaseManager.FormatNullableIntegerForDb(_transferId);
        
            if(_id == null)
            {
                //If we are inserting a new record to the db, set CREATE_DATETIME
                _createDateTime = DateTime.Now;
                string createDateTimeString = DatabaseManager.FormatDateTimeForDb(_createDateTime);

                insertSql = String.Format("" +
                    "INSERT INTO ARBITRATION_TRADE( " +
                    "    BUY_AMOUNT, " +
                    "    BUY_EXCHANGE, " +
                    "    SELL_EXCHANGE, " +
                    "    BUY_PRICE, " +
                    "    TOTAL_BUY_COST, " +
                    "    BUY_ORDER_ID, " +
                    "    SELL_PRICE, " +
                    "    SELL_AMOUNT, " + 
                    "    TOTAL_SELL_COST, " +
                    "    SELL_ORDER_ID, " +
                    "    PROFIT, " +
                    "    EXECUTION_DATETIME, " +
                    "    CREATE_DATETIME, " +
                    "    LAST_MODIFIED_DATETIME, " +
                    "    ARBITRATION_RUN_ID, " + 
                    "    TRANSFER_ID " +
                    ") " +
                    "OUTPUT INSERTED.ID " +
                    "values( " +
                    "    {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}" +
                    ") ", _buyAmount, buyExchangeName, sellExchangeName, _buyPrice, _totalBuyCost, buyOrderIdString, _sellPrice, _sellAmount, _totalSellCost, sellOrderIdString, _profit, executionDateTimeString, createDateTimeString, lastModifiedDateTimeString, _arbitrationRunId.Value, transferIdString);

                _id = DatabaseManager.ExecuteInsert(insertSql);    
            } 

            else
            {
                updateSql = String.Format("" +
                    "UPDATE " +
                    "   ARBITRATION_TRADE " +
                    "SET " +
                    "    BUY_AMOUNT = {0}, " +
                    "    BUY_EXCHANGE = {1}, " +
                    "    SELL_EXCHANGE = {2}, " +
                    "    BUY_PRICE = {3}, " +
                    "    TOTAL_BUY_COST = {4}, " +
                    "    SELL_PRICE = {5}, " +
                    "    TOTAL_SELL_COST = {6}, " +
                    "    PROFIT = {7}, " +
                    "    EXECUTION_DATETIME = {8}, " +
                    "    LAST_MODIFIED_DATETIME = {9}, " +
                    "    ARBITRATION_RUN_ID = {10}, " +
                    "    TRANSFER_ID = {11}, " +
                    "    BUY_ORDER_ID = {12}, " + 
                    "    SELL_ORDER_ID = {13}, " +
                    "    SELL_AMOUNT = {14} " + 
                    "WHERE " +
                    "    ID = {15} ", _buyAmount, buyExchangeName, sellExchangeName, _buyPrice, _totalBuyCost, _sellPrice, _totalSellCost, _profit, executionDateTimeString, lastModifiedDateTimeString, _arbitrationRunId.Value, transferIdString, buyOrderIdString, sellOrderIdString, _sellAmount, _id.Value);

                DatabaseManager.ExecuteNonQuery(updateSql);
            }

            return _id;
        }
    }
}
