using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitcoinExchanges.Tests
{
    [TestClass]
    public class BitXTests
    {
        [TestMethod]
        public void BitXUpdateOrderBookTest()
        {
            BitX bitX = new BitX();

            //If there is any problem connecting to the exchange, this will throw an error.
            bitX.UpdateOrderBook(1);

            //This assumes that the exchange has at least 1 order on each side. Technically this can give a false negative, but it's unlikely.
            Assert.IsTrue(bitX.OrderBook.Asks.Count == 1);
            Assert.IsTrue(bitX.OrderBook.Bids.Count == 1);
        }

    }
}
