using ArbitrationUtilities.EnumerationObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitcoinExchanges.Tests
{
    [TestClass]
    public class ItBitTests
    {
        [TestMethod]
        public void ItBitUpdateOrderBookTest()
        {
            ItBit itBit = new ItBit();

            //If there is any problem connecting to the exchange, this will throw an error.
            itBit.UpdateOrderBook(1);

            //This assumes that the exchange has at least 1 order on each side. Technically this can give a false negative, but it's unlikely.
            Assert.IsTrue(itBit.OrderBook.Asks.Count == 1);
            Assert.IsTrue(itBit.OrderBook.Bids.Count == 1);
        }

        [TestMethod]
        public void ItBitBalancesUpdateTest()
        {
            ItBit itBit = new ItBit(FiatType.Usd);

            //If there is any problem connecting to the exchange, this will throw an error.
            itBit.UpdateBalances();
        }

        /// <summary>
        /// Since ItBit trade fee must be set manually, this test ensures it is pulled from the config settings
        /// </summary>
        [TestMethod]
        public void ItBitTradeFeeIsSet()
        {
            ItBit itBit = new ItBit();

            Assert.IsTrue(itBit.TradeFee > 0);
            Assert.IsTrue(itBit.TradeFeeAsDecimal > 0);
        }

        [TestMethod]
        public void ItBitBuySellQueryDeleteTest()
        {
            ItBit itBit = new ItBit();
            string buyOrderId;
            string sellorderId;

            //First, insert a buy and sell order. If there are any errors with either of these operations, 
            //an exception will be thrown.

            //Buy at a really low price to the order doesn't actually get executed.
            buyOrderId = itBit.Buy(1.00m, 0.01m);

            //Sell at a really high price so the order doesn't actually get executed.
            sellorderId = itBit.Sell(itBit.MinimumBitcoinOrderAmount, 9999m);

            //Both orders should still be open
            Assert.IsFalse(itBit.IsOrderFulfilled(buyOrderId));
            Assert.IsFalse(itBit.IsOrderFulfilled(sellorderId));

            //Now delete both orders. If there are any errors with either of these operations, 
            //an exception will be thrown.
            itBit.DeleteOrder(buyOrderId);
            itBit.DeleteOrder(sellorderId);
        }

        //[TestMethod]
        //public void ItBitOrderQueryTest()
        //{
        //    ItBit itBit = new ItBit();

        //    //Bad order
        //    //object test = itBit.GetOrderInformation("3f379f7e-fcbe-409c-a85a-6273c023b55c");
        //    object test = itBit.GetOrderInformation("323717d6-910b-4d6c-8179-fe4db4845c34");

        //    //Good order
        //    //object test = itBit.GetOrderInformation("5279d43c-44e2-454f-9c98-408ec8fccb23");
            
            
        //    //Assert.IsFalse(itBit.IsOrderFulfilled("a5206668-5bef-4353-9738-7555a9deb13d"));
        //}

        //[TestMethod]
        //public void ItBitTransferTest()
        //{
        //    ItBit itbit = new ItBit();

        //    //Transfering to Kraken
        //    string result = itbit.Transfer(itbit.MinimumBitcoinWithdrawalAmount, "19bRxKDKWcyGzXhzQVbZLqsRQ15YGi4Bki");
        //}








        //-----------------Obsolete test methods, leaving them here just in case they prove useful in the future-----------------\\

        //[TestMethod]
        //public void ItBitOrderQueryTest()
        //{
        //    ItBit itBit = new ItBit();

        //    Assert.IsFalse(itBit.IsOrderFulfilled("fbf18ce7-c270-47ec-8430-aea9c9eb814f"));
        //}

        //[TestMethod]
        //public void ItBitDeleteOrderTest()
        //{
        //    ItBit itBit = new ItBit();

        //    itBit.DeleteOrder("fbf18ce7-c270-47ec-8430-aea9c9eb814g");
        //}

        [TestMethod]
        public void ItBitSellTest()
        {
            ItBit itBit = new ItBit();

            //Buy at a really low price to the order doesn't actually get executed.
            //This operation will error out if there is any problem
            string orderId = itBit.Sell(0.0538m, 999.99m);

            itBit.DeleteOrder(orderId);
        }

        //[TestMethod]
        //public void ItBitBuyTest()
        //{
        //    ItBit itBit = new ItBit();

        //    //Buy at a really low price to the order doesn't actually get executed.
        //    //This operation will error out if there is any problem
        //    string orderId = itBit.Buy(itBit.MinimumBitcoinOrderAmount, 10.00m);

        //    itBit.DeleteOrder(orderId);
        //}
    }
}
