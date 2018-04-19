using System;
using System.Collections.Generic;
using ArbitrationUtilities.EnumerationObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitcoinExchanges.Tests
{
    [TestClass]
    public class KrakenTests
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            //No idea why this line is needed; it just is to properly connect to Kraken
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        [TestMethod]
        public void KrakenAccountInformationUpdateTest()
        {
            Kraken kraken = new Kraken();

            //If there is any problem connecting to the exchange, this will throw an error.
            kraken.UpdateBalances();
        }

        [TestMethod]
        public void KrakenGetOpenOrdersTest()
        {
            Kraken kraken = new Kraken();

            //If there is any problem connecting to the exchange, this will throw an error.
            List<Dictionary<string, dynamic>> openOrderList = kraken.GetAllOpenOrders();
        }

        [TestMethod]
        public void KrakenUpdateTradeFeeTest()
        {
            Kraken kraken = new Kraken();

            //If there is any problem connecting to the exchange, this will throw an error.
            kraken.SetTradeFee();

            Assert.IsTrue(kraken.TradeFee > 0);
        }

        [TestMethod]
        public void KrakenUpdateOrderBookTest()
        {
            Kraken kraken = new Kraken();

            //If there is any problem connecting to the exchange, this will throw an error.
            kraken.UpdateOrderBook(1);

            //This assumes that the exchange has at least 1 order on each side. Technically this can give a false negative, but it's unlikely.
            Assert.IsTrue(kraken.OrderBook.Asks.Count == 1);
            Assert.IsTrue(kraken.OrderBook.Bids.Count == 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void KrakenMinimumTransferTest()
        {
            Kraken kraken = new Kraken();

            //Give less than the minimum amount to ensure we can an error
            //Address isn't needed here, just give a random string
            kraken.Transfer(kraken.MinimumBitcoinWithdrawalAmount - 1, "fakeAddress");
        }

        [TestMethod]
        public void KrakenTransferTest()
        {
            Kraken kraken = new Kraken();

            //Ensure that connection to the api can be made.
            try
            {
                //Give a fake deposit address so it errors on purpose
                kraken.Transfer(kraken.MinimumBitcoinWithdrawalAmount, "fakeAddress");
            }
            catch (Exception e)
            {
                //If this is in the error message, then connecting to the api was ok (this error is on purpose; don't want to be sending a 
                //transfer everytime this test is run).
                Assert.IsTrue(e.Message.Contains("Unknown withdraw key"));
            }
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void KrakenMinimumSellTest_Amount()
        {
            Kraken kraken = new Kraken();

            //Price doesn't do anything in this situation; just picked an absurdely high price to sell at
            kraken.Sell(kraken.MinimumBitcoinOrderAmount-1, 99999m);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void KrakenMinimumSellTest_Price()
        {
            Kraken kraken = new Kraken();

            //Amount doesn't do anything in this situation; just picked the minimum amount to be safe
            kraken.Sell(kraken.MinimumBitcoinOrderAmount, -1m);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void KrakenMinimumBuyTest_Amount()
        {
            Kraken kraken = new Kraken();

            //Price doesn't do anything in this situation; just picked an absurdely low price to buy at
            kraken.Buy(kraken.MinimumBitcoinOrderAmount - 1, 1m);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void KrakenMinimumBuyTest_Price()
        {
            Kraken kraken = new Kraken();

            //Amount doesn't do anything in this situation; just picked the minimum amount to be safe
            kraken.Buy(kraken.MinimumBitcoinOrderAmount, -1m);
        }

        [TestMethod]
        public void KrakenBuySellQueryDeleteTest()
        {
            Kraken kraken = new Kraken(FiatType.Usd);
            string buyOrderId;
            string sellorderId;

            //First, insert a buy and sell order. If there are any errors with either of these operations, 
            //an exception will be thrown.

            //Buy at a really low price to the order doesn't actually get executed. Note, with this amount of btc, need to sell at at least $10 to meet the minimum order requirements.
            buyOrderId = kraken.Buy(kraken.MinimumBitcoinOrderAmount, 10m);

            //Sell at a really high price so the order doesn't actually get executed.
            sellorderId = kraken.Sell(kraken.MinimumBitcoinOrderAmount, 9999m);

            //Both orders should still be open
            Assert.IsFalse(kraken.IsOrderFulfilled(buyOrderId));
            Assert.IsFalse(kraken.IsOrderFulfilled(sellorderId));

            //Now delete both orders. If there are any errors with either of these operations, 
            //an exception will be thrown.
            kraken.DeleteOrder(buyOrderId);
            kraken.DeleteOrder(sellorderId);
        }

        [TestMethod]
        public void KrakenQueryOrderTest()
        {
            Kraken kraken = new Kraken();
            
            //If there is a problem,  this will throw an error.
            kraken.GetOrderInformation("O6HJVS-6SU7A-EX5TVD");
        }

        //-----------------Obsolete test methods, leaving them here just in case they prove useful in the future-----------------\\

        //[TestMethod]
        //public void KrakenDeleteTest()
        //{
        //    Kraken kraken = new Kraken();

        //    kraken.DeleteOrder("OEHO7K-ALOQF-BBHE6T");
        //}

        //[TestMethod]
        //public void KrakenSellTest()
        //{
        //    Kraken kraken = new Kraken(FiatType.Usd);

        //    //Sell at a really high price so the order doesn't actually get executed.
        //    string result = kraken.Sell(kraken.MinimumBitcoinOrderAmount, 999.99999m);

        //    //YOU MUST DELETE THE ORDER MANUALLY AFTER THIS TEST!!!
        //}

        //[TestMethod]
        //public void KrakenBuyTest()
        //{
        //    Kraken kraken = new Kraken();

        //    //Buy at a really low price to the order doesn't actually get executed.
        //    string result = kraken.Buy(kraken.MinimumBitcoinOrderAmount, 1m);

        //    //YOU MUST DELETE THE ORDER MANUALLY AFTER THIS TEST!!!
        //}
   }
}
