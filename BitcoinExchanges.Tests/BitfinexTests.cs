using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitcoinExchanges.Tests
{
    [TestClass]
    public class BitfinexTests
    {
        [TestMethod]
        public void BitfinexUpdateOrderBookTest()
        {
            Bitfinex bitfinex = new Bitfinex();

            //If there is any problem connecting to the exchange, this will throw an error.
            bitfinex.UpdateOrderBook(1);

            //This assumes that the exchange has at least 1 order on each side. Technically this can give a false negative, but it's unlikely.
            Assert.IsTrue(bitfinex.OrderBook.Asks.Count == 1);
            Assert.IsTrue(bitfinex.OrderBook.Bids.Count == 1);
        }

        [TestMethod]
        public void BitfinexBalanceUpdateTest()
        {
            Bitfinex bitfinex = new Bitfinex();

            //If there is any problem connecting to the exchange, this will throw an error.
            bitfinex.UpdateBalances();
        }

        [TestMethod]
        public void BitfinexGetAllOpenOrdersTest()
        {
            Bitfinex bitfinex = new Bitfinex();

            //If there is any problem connecting to the exchange, this will throw an error.
            List<Dictionary<string, dynamic>> openOrderList = bitfinex.GetAllOpenOrders();
        }

        [TestMethod]
        public void BitfinexGetTradeFeeTest()
        {
            Bitfinex bitfinex = new Bitfinex();

            bitfinex.SetTradeFee();

            Assert.IsTrue(bitfinex.TradeFee > 0);
        }

        [TestMethod]
        public void BitfinexBuySellQueryDeleteTest()
        {
            Bitfinex bitfinex = new Bitfinex();
            string buyOrderId;
            string sellorderId;

            //First, insert a buy and sell order. If there are any errors with either of these operations, 
            //an exception will be thrown.

            //Buy at a really low price to the order doesn't actually get executed.
            buyOrderId = bitfinex.Buy(bitfinex.MinimumBitcoinOrderAmount, 10m);

            //Sell at a really high price so the order doesn't actually get executed.
            sellorderId = bitfinex.Sell(bitfinex.MinimumBitcoinOrderAmount, 9999m);

            //Both orders should still be open
            Assert.IsFalse(bitfinex.IsOrderFulfilled(buyOrderId));
            Assert.IsFalse(bitfinex.IsOrderFulfilled(sellorderId));

            //Now delete both orders. If there are any errors with either of these operations, 
            //an exception will be thrown.
            bitfinex.DeleteOrder(buyOrderId);
            bitfinex.DeleteOrder(sellorderId);
        }

        //-----------------Obsolete test methods, leaving them here just in case they prove useful in the future-----------------\\

        //[TestMethod]
        //public void BitfinexSellTest()
        //{
        //    Bitfinex bitfinex = new Bitfinex();
        //    string orderId = bitfinex.Sell(bitfinex.MinimumBitcoinOrderAmount, 999.99m);
        //}

        //[TestMethod]
        //public void BitfinexBuyTest()
        //{
        //    Bitfinex bitfinex = new Bitfinex();
        //    string orderId = bitfinex.Buy(bitfinex.MinimumBitcoinOrderAmount, 9.99m);
        //}

        //[TestMethod]
        //public void BitfinexOrderQueryTest()
        //{
        //    Bitfinex bitfinex = new Bitfinex();

        //    Dictionary<string, dynamic> order = bitfinex.GetOrderInformation("619813549");
        //}

        //[TestMethod]
        //public void BitfinexIsOrderFulilledTest()
        //{
        //    Bitfinex bitfinex = new Bitfinex();

        //    bool result = bitfinex.IsOrderFulfilled("15617527");
        //}

        //[TestMethod]
        //public void BitfinexDeleteOrderTest()
        //{
        //    Bitfinex bitfinex = new Bitfinex();

        //    bitfinex.DeleteOrder("613707977");
        //}
    }
}
