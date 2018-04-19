using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArbitrationSimulator.Exchanges;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitrationSimulator.Tests
{
    [TestClass]
    public class ExhangeApiTests
    {
        [TestMethod]
        public void BitstampAccountInformationUpdateTest()
        {
            Bitstamp bitstamp = new Bitstamp();

            //If there is any problem connecting to the exchange, this will throw an error.
            bitstamp.UpdateAccountInformation();

            //Only need to ensure UpdateAccountInformation updates trade fee, as btc and fiat balances are allowed to be zero.
            Assert.IsTrue(bitstamp.TradeFee > 0.0m);
        }

        [TestMethod]
        public void KrakenAccountInformationUpdateTest()
        {
            Kraken kraken = new Kraken();

            //If there is any problem connecting to the exchange, this will throw an error.
            kraken.UpdateAccountInformation();

            //Only need to ensure UpdateAccountInformation updates trade fee, as btc and fiat balances are allowed to be zero.
            Assert.IsTrue(kraken.TradeFee > 0.0m);
        }
    }
}
