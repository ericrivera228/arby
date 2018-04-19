using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitcoinExchanges.Tests
{
    [TestClass]
    public class OkCoinTests
    {
        [TestMethod]
        public void OkCoinSellCostRoundTest()
        {
            OkCoin okcoin = new OkCoin();

            Assert.IsTrue(okcoin.ApplyFeeToSellCost(0.01m * 430.63m) == 4.2976m);
            Assert.IsTrue(okcoin.ApplyFeeToSellCost(0.014m * 371.76m) == 5.1941m);
        }

        [TestMethod]
        public void OkCoinGetAllOpenOrdersTest()
        {
            OkCoin okcoin = new OkCoin();

            object result = okcoin.GetAllOpenOrders();
        }

        [TestMethod]
        public void OkCoinUpdateOrderBookTest()
        {
            OkCoin okCoin = new OkCoin();

            //If there is any problem connecting to the exchange, this will throw an error.
            okCoin.UpdateOrderBook(1);

            //This assumes that the exchange has at least 1 order on each side. Technically this can give a false negative, but it's unlikely.
            Assert.IsTrue(okCoin.OrderBook.Asks.Count == 1);
            Assert.IsTrue(okCoin.OrderBook.Bids.Count == 1);
        }

        [TestMethod]
        public void OkCoinBalanceUpdateTest()
        {
            OkCoin okCoin = new OkCoin();

            //If there is any problem connecting to the exchange, this will throw an error.
            okCoin.UpdateBalances();
        }

        [TestMethod]
        public void OkCoinSellQueryDeleteTest()
        {
            OkCoin okCoin = new OkCoin();
            string sellorderId;

            //First, insert a sell order. If there are any errors with either of these operations, 
            //an exception will be thrown.

            //Buy at a really low price to the order doesn't actually get executed.
            sellorderId = okCoin.Sell(okCoin.MinimumBitcoinOrderAmount, 999m);

            //Both orders should still be open
            Assert.IsFalse(okCoin.IsOrderFulfilled(sellorderId));
            
            //Now delete the order. If there is an error, an exception will be thrown.
            okCoin.DeleteOrder(sellorderId);
        }

        //-----------------Obsolete test methods, leaving them here just in case they prove useful in the future-----------------\\


        //[TestMethod]
        //public void OkCoinOrderQueryTest()
        //{
        //    OkCoin okCoin = new OkCoin();

        //    //If there is any problem connecting to the exchange, this will throw an error.
        //    //(passing -1 pulls the first unfulfilled order)
        //    Dictionary<string, dynamic> order = okCoin.GetOrderInformation("-1");
        //}

        //[TestMethod]
        //public void OkCoinDeleteOrderTest()
        //{
        //    OkCoin okCoin = new OkCoin();

        //    okCoin.DeleteOrder("171638113");
        //}

        //[TestMethod]
        //public void OkCoinIsOrderFulfilledTest()
        //{
        //    OkCoin okcoin = new OkCoin();

        //    bool result = okcoin.IsOrderFulfilled("171879197");
        //}

        //[TestMethod]
        //public void OkCoinSellTest()
        //{
        //    OkCoin okCoin = new OkCoin();

        //    //Sell at a really high price so the order doesn't actually get executed.
        //    string sellOrderId = okCoin.Sell(okCoin.MinimumBitcoinOrderAmount, 999m);
        //}

        //[TestMethod]
        //public void OkCoinBuyTest()
        //{
        //    OkCoin okCoin = new OkCoin();

        //    //Buy at a really low price to the order doesn't actually get executed.
        //    string buyOrderId = okCoin.Buy(0.0121m, 20m);
        //}
    }
}
