using System;
using BitcoinExchanges;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitrationSimulator.Tests
{
    [TestClass]
    public class ExchangeTests
    {
        //Test case: Buy Btc when there is no balance
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SimulatedBuyTestWithNotBtc()
        {
            Btce btce = new Btce();
            btce.SimulatedBuy(2.0m, 234.00m);  
        }

        //Test case: Simuated buy btc happy path, ensures btc and fiat balances are updated appropriately
        [TestMethod]
        public void SimulateBuyTestHappyPath()
        {
            decimal initialPaymiumBtcBalance = 9.98763156454m;
            decimal initialPaymiumFiatBalance = 245.63m;
            decimal buyBtcAmount = 0.5687489732163m;
            decimal totalBuyCost = 45.21m;

            Btce btce = new Btce();
            btce.AvailableBtc = initialPaymiumBtcBalance;
            btce.AvailableFiat = initialPaymiumFiatBalance;
            btce.SimulatedBuy(buyBtcAmount, totalBuyCost);

            Assert.IsTrue(btce.AvailableBtc == initialPaymiumBtcBalance + buyBtcAmount);
            Assert.IsTrue(btce.AvailableFiat == initialPaymiumFiatBalance - totalBuyCost);
        }

        //Test Case: Sell btc when there isn't any btc in the exchange
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SimulatedSellTestWithNoFiat()
        {
            Btce btce = new Btce();
            btce.SimulatedSell(2.0m, 234.00m);
        }

        //Test case: Simulated sell btc happy path, ensures btc and fiat balances are updated appropriately
        [TestMethod]
        public void SimulateSellTestHappyPath()
        {
            decimal initialPaymiumBtcBalance = 4.689812874641m;
            decimal initialPaymiumFiatBalance = 148.28m;
            decimal sellBtcAmount = 0.987316397m;
            decimal totalSellCost = 45.21m;

            Btce btce = new Btce();
            btce.AvailableBtc = initialPaymiumBtcBalance;
            btce.AvailableFiat = initialPaymiumFiatBalance;
            btce.SimulatedSell(sellBtcAmount, totalSellCost);

            Assert.IsTrue(btce.AvailableBtc == initialPaymiumBtcBalance - sellBtcAmount);
            Assert.IsTrue(btce.AvailableFiat == initialPaymiumFiatBalance + totalSellCost);
        }

        /// <summary>
        /// Ensures that when the tradeFee of an exchange is modified, the tradeFeeAsDecimal is updated as well.
        /// </summary>
        [TestMethod]
        public void TradeFeeUpdate()
        {
            Kraken kraken = new Kraken {TradeFee = 0.2m};
            Assert.IsTrue(kraken.TradeFeeAsDecimal == 0.002m);

            kraken.TradeFee = 0.4m;
            Assert.IsTrue(kraken.TradeFeeAsDecimal == 0.004m);
        }




        //TODO: This this hack!
        //Just an lazy, convenient way to get the total exchanges until I take the time to make a better solution
        [TestMethod]
        public void GetTotalBalances()
        {
            ItBit itbit = new ItBit();
            Anx anx = new Anx();
            Bitstamp bitstamp = new Bitstamp();
            Kraken kraken = new Kraken();
            //Btce btce = new Btce();
            Bitfinex bitfinex = new Bitfinex();

            itbit.UpdateBalances();
            anx.UpdateBalances();
            bitstamp.UpdateBalances();
            bitfinex.UpdateBalances();

            //TODO: Fix these! Should be calls to UpdateTotalBalances!
            kraken.UpdateBalances();
            //btce.UpdateBalances();

            Decimal itBitBtcAmount = itbit.TotalBtc;
            Decimal itBitFiatAmount = itbit.TotalFiat;
            Decimal anxBtcAmount = anx.TotalBtc;
            Decimal anxFiatAmount = anx.TotalFiat;
            Decimal bitstampBtcAmount = bitstamp.TotalBtc;
            Decimal bitstampFiatoAmount = bitstamp.TotalFiat;
            Decimal bitfinexBtcAmount = bitfinex.TotalBtc;
            Decimal bitfinexFiatAmount = bitfinex.TotalFiat;

            //TODO Fix these! They should be the total amounts!
            Decimal krakenBtcAmount = kraken.AvailableBtc;
            Decimal krakenEurAmount = kraken.AvailableFiat;
            //Decimal btceBtcAmount = btce.AvailableBtc;
            //Decimal btceEuroAmount = btce.AvailableFiat;
        }
    }
}
