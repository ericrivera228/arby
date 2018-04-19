using System.Collections;
using System.Collections.Generic;
using ArbitrationUtilities.EnumerationObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitcoinExchanges.Tests
{
    [TestClass]
    public class AnxTests
    {
        [TestMethod]
        public void AnxUpdateOrderBookTest()
        {
            Anx anx = new Anx();

            //If there is any problem connecting to the exchange, this will throw an error.
            anx.UpdateOrderBook(1);

            //This assumes that the exchange has at least 1 order on each side. Technically this can give a false negative, but it's unlikely.
            Assert.IsTrue(anx.OrderBook.Asks.Count == 1);
            Assert.IsTrue(anx.OrderBook.Bids.Count == 1);
        }

        [TestMethod]
        public void AnxBalanceUpdateTest()
        {
            Anx anx = new Anx(FiatType.Usd);

            //If there is any problem connecting to the exchange, this will throw an error.
            anx.UpdateBalances();
        }

        /// <summary>
        /// Since Anx trade fee must be set manually, this test ensures it is pulled from the config settings
        /// </summary>
        [TestMethod]
        public void AnxTradeFeeIsSet()
        {
            Anx anx = new Anx();
            
            Assert.IsTrue(anx.TradeFee > 0);
            Assert.IsTrue(anx.TradeFeeAsDecimal > 0);
        }

        [TestMethod]
        public void AnxGetTradeListTest()
        {
            Anx anx = new Anx();

            //If there is any problem connecting to the exchange, this will throw an error.
            ArrayList tradeList = anx.GetTradeList();
        }

        [TestMethod]
        public void AnxGetAllOpenOrdersTest()
        {
            Anx anx = new Anx();

            List<Dictionary<string, dynamic>> openOrders = anx.GetAllOpenOrders();
        }

        [TestMethod]
        public void AnxBuySellQueryDeleteTest()
        {
            Anx anx = new Anx(FiatType.Usd);
            string buyOrderId;
            string sellorderId;

            //First, insert a buy and sell order. If there are any errors with either of these operations, 
            //an exception will be thrown.

            //Buy at a really low price to the order doesn't actually get executed.
            buyOrderId = anx.Buy(anx.MinimumBitcoinOrderAmount, 10m);

            //Sell at a really high price so the order doesn't actually get executed.
            sellorderId = anx.Sell(anx.MinimumBitcoinOrderAmount, 9999m);

            //Both orders should still be open
            Assert.IsFalse(anx.IsOrderFulfilled(buyOrderId));
            Assert.IsFalse(anx.IsOrderFulfilled(sellorderId));

            //Now delete both orders. If there are any errors with either of these operations, 
            //an exception will be thrown.
            anx.DeleteOrder(buyOrderId);
            anx.DeleteOrder(sellorderId);
        }

        //-----------------Obsolete test methods, leaving them here just in case they prove useful in the future-----------------\\

        //[TestMethod]
        //public void AnxOrderFulfilledTest()
        //{
        //    Anx anx = new Anx();
        //    bool orderFulfilled;

        //    //This is an order that is permanently open on the account; it is a but for 0.01 btc at 10.28.
        //    //This ensures an opewn order can be retrieved.
        //    orderFulfilled = anx.IsOrderFulfilled("813ed3c2-9a6e-4cd2-ab01-3b5c934d850e");
        //    Assert.IsFalse(orderFulfilled, "Anx IsOrderFulfilled test failed when looking at status of an open order.");

        //    //This is an order that was already fulfilled. Ensures the order query method works for closed orders.
        //    orderFulfilled = anx.IsOrderFulfilled("485d1ee2-e52c-4618-a519-13b3a798cb7e");
        //    Assert.IsTrue(orderFulfilled, "Anx IsOrderFulfilled test failed when looking at status of a closed order.");

        //    bool errorThrown = false;

        //    try
        //    {
        //        //The IsOrderFulfilled method should return an error when a nonexistent order is queried.
        //        //This part ensures this happens.
        //        anx.IsOrderFulfilled("813ed3c2-0b7e-4cd2-ab01-3b5c934d850e");
        //    }
        //    catch
        //    {
        //        errorThrown = true;
        //    }

        //    Assert.IsTrue(errorThrown, "Anx IsOrderFulfilled test failed for a non-existant order.");
        //}

        //[TestMethod]
        //public void AnxOrderQueryTest()
        //{
        //    Anx anx = new Anx();

        //    //This is an order that is permanently open on the account; it is a but for 0.01 btc at 10.28.
        //    //This ensures an open order can be retrieved.
        //    object openOrder = anx.GetOrderInformation("813ed3c2-9a6e-4cd2-ab01-3b5c934d850e");
        //    Assert.IsTrue(openOrder != null, "Anx order query test could did not work for open orders");

        //    //This is an order that was already fulfilled. Ensures the order query method works for closed orders.
        //    object closedOrder = anx.GetOrderInformation("485d1ee2-e52c-4618-a519-13b3a798cb7e");
        //    Assert.IsTrue(closedOrder != null, "Anx order query test could did not work for closed orders");

        //    bool errorThrown = false;

        //    try
        //    {
        //        //The GetOrderInformation method should return an error when a nonexistent order is queried.
        //        //This part ensures this happens.
        //        anx.GetOrderInformation("813ed3c2-0b7e-4cd2-ab01-3b5c934d850e");
        //    }
        //    catch
        //    {
        //        errorThrown = true;
        //    }

        //    Assert.IsTrue(errorThrown, "Anx order query test failed for a non-existant order.");
        //}

        //[TestMethod]
        //public void AnxSellTest()
        //{
        //    Anx anx = new Anx();

        //    //Sell at a really high price so the order doesn't actually get executed.
        //    string result = anx.Sell(anx.MinimumBitcoinOrderAmount, 9999m);
        //}

        //[TestMethod]
        //public void AnxBuyTest()
        //{
        //    Anx anx = new Anx();

        //    //Buy at a really low price to the order doesn't actually get executed.
        //    //string result = anx.Buy(anx.MinimumBitcoinOrderAmount, 10m);

        //    string result = anx.Buy(0.01m, 200m);
        //}

        //[TestMethod]
        //public void AnxDeleteOrderTest()
        //{
        //    Anx anx = new Anx();

        //    anx.DeleteOrder("782e88fc-2638-4636-9bd0-490a81eca7e8");
        //}

        //[TestMethod]
        //public void AnxTransferTest()
        //{
        //    Anx anx = new Anx();
        //    Kraken kraken = new Kraken();
        //    string result = anx.Transfer(0.1492m, kraken.BitcoinDepositAddress);
        //}
    }
}
