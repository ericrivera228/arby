using System.Collections.Generic;
using ArbitrationUtilities.EnumerationObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitcoinExchanges.Tests
{
    [TestClass]
    public class CoinbaseTests
    {
        [TestMethod]
        public void CoinbaseUpdateOrderBookTest()
        {
            Coinbase coinbase = new Coinbase();

            //If there is any problem connecting to the exchange, this will throw an error.
            coinbase.UpdateOrderBook(1);

            //This assumes that the exchange has at least 1 order on each side. Technically this can give a false negative, but it's unlikely.
            Assert.IsTrue(coinbase.OrderBook.Asks.Count == 1);
            Assert.IsTrue(coinbase.OrderBook.Bids.Count == 1);
        }

        [TestMethod]
        public void CoinbaseBalanceUpdateTest()
        {
            Coinbase coinbase = new Coinbase();

            //If there is any problem connecting to the exchange, this will throw an error.
            coinbase.UpdateBalances();
        }

        [TestMethod]
        public void CoinbaseGetAllOpenOrdersTest()
        {
            Coinbase coinbase = new Coinbase();

            //If there is any problem connecting to the exchange, this will throw an error.
            List<Dictionary<string, dynamic>> openOrderList = coinbase.GetAllOpenOrders();
        }

        [TestMethod]
        public void CoinbaseBuySellQueryDeleteTest()
        {
            Coinbase coinbase = new Coinbase(FiatType.Usd);
            string buyOrderId;
            string sellorderId;

            //First, insert a buy and sell order. If there are any errors with either of these operations, 
            //an exception will be thrown.

            //Buy at a really low price to the order doesn't actually get executed.
            buyOrderId = coinbase.Buy(coinbase.MinimumBitcoinOrderAmount, 10m);

            //Sell at a really high price so the order doesn't actually get executed.
            sellorderId = coinbase.Sell(coinbase.MinimumBitcoinOrderAmount, 9999m);

            //Both orders should still be open
            Assert.IsFalse(coinbase.IsOrderFulfilled(buyOrderId));
            Assert.IsFalse(coinbase.IsOrderFulfilled(sellorderId));

            //Now delete both orders. If there are any errors with either of these operations, 
            //an exception will be thrown.
            coinbase.DeleteOrder(buyOrderId);
            coinbase.DeleteOrder(sellorderId);
        }

        //-----------------Obsolete test methods, leaving them here just in case they prove useful in the future-----------------\\

        //[TestMethod]
        //public void CoinbaseOrderQueryTest()
        //{
        //    Coinbase coinbase = new Coinbase();

        //    Dictionary<string, dynamic> orderInformation = coinbase.GetOrderInformation("646fca2a-2169-41c2-a455-db18fed25432");
        //}

        //[TestMethod]
        //public void CoinbaseOrderFulfilledTest()
        //{
        //    Coinbase coinbase = new Coinbase();

        //    Assert.IsFalse(coinbase.IsOrderFulfilled("646fca2a-2169-41c2-a455-db18fed25432"));
        //}

        //[TestMethod]
        //public void CoinbaseDeleteOrderTest()
        //{
        //    Coinbase coinbase = new Coinbase();

        //    coinbase.DeleteOrder("974ef71e-cef5-495d-a329-e18e56c8e770");
        //}

        //[TestMethod]
        //public void CoinbaseSellTest()
        //{
        //    Coinbase coinbase = new Coinbase();

        //    coinbase.Sell(coinbase.MinimumBitcoinOrderAmount, 999.99m);
        //}
    }
}
