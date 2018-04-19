using System.Collections.Generic;
using ArbitrationUtilities.EnumerationObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitcoinExchanges.Tests
{
    [TestClass]
    public class BtceTests
    {
        [TestMethod]
        public void BtceUpdateOrderBookTest()
        {
            Btce btce = new Btce();

            //If there is any problem connecting to the exchange, this will throw an error.
            btce.UpdateOrderBook(1);

            //This assumes that the exchange has at least 1 order on each side. Technically this can give a false negative, but it's unlikely.
            Assert.IsTrue(btce.OrderBook.Asks.Count == 1);
            Assert.IsTrue(btce.OrderBook.Bids.Count == 1);
        }

        [TestMethod]
        public void BtceBalanceUpdateTest()
        {
            Btce btce = new Btce(FiatType.Usd);

            //If there is any problem connecting to the exchange, this will throw an error.
            btce.UpdateBalances();
        }

        /// <summary>
        /// Since Btce trade fee must be set manually, this test ensures it is pulled from the config settings
        /// </summary>
        [TestMethod]
        public void BtceTradeFeeIsSet()
        {
            Btce btce = new Btce();

            Assert.IsTrue(btce.TradeFee > 0);
            Assert.IsTrue(btce.TradeFeeAsDecimal > 0);
        }

        [TestMethod]
        public void BtceGetTradeListTest()
        {
            Btce btce = new Btce();

            //If there is any problem connecting to the exchange, this will throw an error. The returning dictionary should never be null,
            //as there is a history on the account.
            Assert.IsNotNull(btce.GetTradeList(), "List of past trades could not be pulled from BTC-E.");
        }
        
        [TestMethod]
        public void BtceIsOrderFulfilledReturnsTrueForCompletedOrder()
        {
            Btce btce = new Btce();

            Assert.IsTrue(btce.IsOrderFulfilled("957745724"));
        }

        [TestMethod]
        public void BtceGetAllOpenOrdersTest()
        {
            Btce btce = new Btce();

            List<Dictionary<string, dynamic>> openOrders = btce.GetAllOpenOrders();
        }

        //[TestMethod]
        //public void BtceOrderQueryTest()
        //{
        //    string openOrderId = "858221183";
        //    string closedOrderId = "62114972";
        //    Btce btce = new Btce();

        //    //This is an order that is permanently open on the account
        //    //This ensures an open order can be retrieved.
        //    object openOrder = btce.GetOrderInformation(openOrderId);
        //    Assert.IsTrue(openOrder != null, "Btce order query test could did not work for open orders");

        //    //Now make sure that IsOrderFulfilled method works for open orders
        //    Assert.IsFalse(btce.IsOrderFulfilled(openOrderId), "Btce IsOrderFulfilled failed for open order.");

        //    //This is an order that was already fulfilled. Ensures the order query method works for closed orders.
        //    object closedOrder = btce.GetOrderInformation(closedOrderId);
        //    Assert.IsTrue(closedOrder != null, "Btce order query test could did not work for closed orders");

        //    //Now make sure that IsOrderFulfilled method works for closed orders
        //    Assert.IsTrue(btce.IsOrderFulfilled(closedOrderId), "Btce IsOrderFulfilled failed for closed order.");

        //    bool errorThrown = false;

        //    try
        //    {
        //        //The IsOrderFulfilled method should return an error when a nonexistent order is queried.
        //        //This part ensures this happens.
        //        btce.GetOrderInformation("0000002");
        //    }
        //    catch
        //    {
        //        errorThrown = true;
        //    }

        //    Assert.IsTrue(errorThrown, "Btce order query test failed for a non-existant order.");

        //    //Reset errorThrown for the next test
        //    errorThrown = false;

        //    try
        //    {
        //        //The IsOrderFulfilled method should return an error when a nonexistent order is queried.
        //        //This part ensures this happens.
        //        btce.IsOrderFulfilled("0000002");
        //    }
        //    catch
        //    {
        //        errorThrown = true;
        //    }

        //    Assert.IsTrue(errorThrown, "Btce IsOrderFulfilled test failed for a non-existant order.");
        //}

        //[TestMethod]
        //public void BtceBuyTest()
        //{
        //    Btce btce = new Btce();

        //    //This is the lowest possible buy order you can put in btce
        //    string buyOrderId = btce.Buy(0.01m, 5.00m);

        //    Assert.IsFalse(btce.IsOrderFulfilled(buyOrderId), "Btce order query test could did not work for closed orders");

        //    //Now delete the order
        //    btce.DeleteOrder(buyOrderId);
        //}

        //[TestMethod]
        //public void BtceSellTest()
        //{
        //    Btce btce = new Btce(FiatType.Usd);

        //    //Set the sell price at an absurdely high value so the sell order doesn't actully get executed.
        //    string sellOrderId = btce.Sell(btce.MinimumBitcoinOrderAmount, 3000m);

        //    //Now delete the order
        //    btce.DeleteOrder(sellOrderId);

        //}
    }
}
