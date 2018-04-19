using ArbitrationSimulator.Exchanges;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitrationSimulator.Tests.Exchanges
{
    [TestClass]
    public class BtcXeTests
    {
        [TestMethod]
        public void BtcXeUpdateOrderBookTest()
        {
            BtcXe btcXe = new BtcXe();

            //If there is any problem connecting to the exchange, this will throw an error.
            btcXe.UpdateOrderBook(1);

            //This assumes that the exchange has at least 1 order on each side. Technically this can give a false negative, but it's unlikely.
            Assert.IsTrue(btcXe.OrderBook.Asks.Count == 1);
            Assert.IsTrue(btcXe.OrderBook.Bids.Count == 1);
        }

        [TestMethod]
        public void BtcXeAccountInformationUpdateTest()
        {
            BtcXe btcXe = new BtcXe();

            //If there is any problem connecting to the exchange, this will throw an error.
            btcXe.UpdateAvailableBalances();
        }
    }
}
