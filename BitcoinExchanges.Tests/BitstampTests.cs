using System;
using ArbitrationUtilities.EnumerationObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitcoinExchanges.Tests
{
    [TestClass]
    public class BitstampTests
    {
        [TestMethod]
        public void BitstampSellCostRoundTest()
        {
            Bitstamp bitstamp = new Bitstamp(FiatType.Usd);

            //Need to set the trade fee for this test to work
            bitstamp.TradeFee = 0.25m;

            Assert.IsTrue(bitstamp.ApplyFeeToSellCost(20.09m) == 20.03m);
            Assert.IsTrue(bitstamp.ApplyFeeToSellCost(90.95m) == 90.72m);

            //Sell 0.04831514 @ 415.5 = 20.07494067 = 20.07
            //Fee price = 20.07 * 0.0025 = 0.050175 = 0.06
            //Total cost = 20.07 - 0.06 = 20.01
            Assert.IsTrue(bitstamp.ApplyFeeToSellCost(Decimal.Multiply(0.04831514m, 415.5m)) == 20.01m);

            //Sell 0.05513812 @ 365 = 20.1254138 = 20.13
            //Fee price = 20.13 * 0.0025 = 0.050325 = 0.06
            //Total cost = 20.13 - 0.06 = 20.07
            Assert.IsTrue(bitstamp.ApplyFeeToSellCost(Decimal.Multiply(0.05513812m, 365.0m)) == 20.07m);
        }

        [TestMethod]
        public void BitstampBuyCostRoundTest()
        {
            Bitstamp bitstamp = new Bitstamp(FiatType.Usd);

            //Need to set the trade fee for this test to work
            bitstamp.TradeFee = 0.25m;

            Assert.IsTrue(bitstamp.ApplyFeeToBuyCost(20.09m) == 20.15m);
            Assert.IsTrue(bitstamp.ApplyFeeToBuyCost(90.95m) == 91.18m);

            //Buy 0.02 @ 395.65 = 7.91
            //Fee price = 7.91 * 0.0025 = 0.019775 = 0.02
            //Total cost = 7.91 + 0.02 = 7.93
            Assert.IsTrue(bitstamp.ApplyFeeToBuyCost(Decimal.Multiply(395.65m, 0.02m)) == 7.93m);
        }

        [TestMethod]
        public void BitstampGetOpenOrdersTest()
        {
            Bitstamp bitstamp = new Bitstamp(FiatType.Usd);

            //If there is any problem connecting to the exchange, this will throw an error.
            bitstamp.GetAllOpenOrders();
        }

        [TestMethod]
        public void BitstampAvailableBalanceUpdateTest()
        {
            Bitstamp bitstamp = new Bitstamp(FiatType.Usd);

            //If there is any problem connecting to the exchange, this will throw an error.
            bitstamp.UpdateBalances();

            //Only need to ensure UpdateBalances updates trade fee, as btc and fiat balances are allowed to be zero.
            Assert.IsTrue(bitstamp.TradeFee > 0.0m);
        }

		[TestMethod]
		public void BitstampUpdateOrderBookTest()
		{
			Bitstamp bitstamp = new Bitstamp();

			//If there is any problem connecting to the exchange, this will throw an error.
			bitstamp.UpdateOrderBook(1);

			//This assumes that the exchange has at least 1 order on each side. Technically this can give a false negative, but it's unlikely.
			Assert.IsTrue(bitstamp.OrderBook.Asks.Count == 1);
			Assert.IsTrue(bitstamp.OrderBook.Bids.Count == 1);
		}

        [TestMethod]
        public void BitstampGetUsdEurConversionRateTest()
        {
            Bitstamp bitstamp = new Bitstamp();
            decimal conversionRate = bitstamp.GetUsdEurConversionRate();

            //As long as this returned a number, the call to the api was successful
            Assert.IsTrue(conversionRate > 0);
        }

        [TestMethod]
        public void BitstampBuySellQueryDeleteTest()
        {
            Bitstamp bitstamp = new Bitstamp(FiatType.Usd);
            string buyOrderId;
            string sellorderId;

            //First, insert a buy and sell order. If there are any errors with either of these operations, 
            //an exception will be thrown.

            //This is the lowest possible buy order you can put in bitstamp
            buyOrderId = bitstamp.Buy(1, 5.00m);

            //Set the sell price at an absurdely high value so the sell order doesn't actully get executed.
            sellorderId = bitstamp.Sell(bitstamp.MinimumBitcoinOrderAmount, 99999m);

            //Both orders should still be open
            Assert.IsFalse(bitstamp.IsOrderFulfilled(buyOrderId));
            Assert.IsFalse(bitstamp.IsOrderFulfilled(sellorderId));

            //Now delete both orders. If there are any errors with either of these operations, 
            //an exception will be thrown.
            bitstamp.DeleteOrder(buyOrderId);
            bitstamp.DeleteOrder(sellorderId);
        }

        //[TestMethod]
        //public void BistampTransferTest()
        //{
        //    Bitstamp bitstamp = new Bitstamp();
        //    string result = bitstamp.Transfer(bitstamp.MinimumBitcoinWithdrawalAmount, "19bRxKDKWcyGzXhzQVbZLqsRQ15YGi4Bki");
        //}



        //-----------------Obsolete test methods, leaving them here just in case they prove useful in the future-----------------\\

        //[TestMethod]
        //public void BitstampBuyTest()
        //{
        //    Bitstamp bitstamp = new Bitstamp();

        //    //This is the lowest possible buy order you can put in bitstamp
        //    string resultString = bitstamp.Buy(1, 5.00m);

        //    //YOU MUST DELETE THE ORDER MANUALLY AFTER THIS TEST!!!
        //}

        //[TestMethod]
        //public void BitstampSellTest()
        //{
        //    Bitstamp bitstamp = new Bitstamp();

        //    //Set the sell price at an absurdely high value so the sell order doesn't actully get executed.
        //    string sellOrderId = bitstamp.Sell(bitstamp.MinimumBitcoinOrderAmount, 99999m);

        //    bitstamp.DeleteOrder(sellOrderId);
        //}

        //[TestMethod]
        //public void BitstampDeleteTest()
        //{
        //    Bitstamp bitstamp = new Bitstamp();
        
        //    bitstamp.DeleteOrder("78077013");
        //}

        //[TestMethod]
        //public void BitstampQueryOrderTest()
        //{
        //    //Order that is finished: 78077630
        //    //Order that is open: 78077013

        //    Bitstamp bitstamp = new Bitstamp();

        //    Assert.IsTrue(bitstamp.IsOrderFulfilled("78077630"));
        //    Assert.IsFalse(bitstamp.IsOrderFulfilled("78077013"));
        //}
    }
}