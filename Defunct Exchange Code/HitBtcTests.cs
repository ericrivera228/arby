using ArbitrationSimulator.Exchanges;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitrationSimulator.Tests.Exchanges
{
    [TestClass]
    public class HitBtcTests
    {
        [TestMethod]
        public void HitBtcUpdateOrderBookTest()
        {
            HitBtc hitBtc = new HitBtc();

            //If there is any problem connecting to the exchange, this will throw an error.
            hitBtc.UpdateOrderBook(1);

            //This assumes that the exchange has at least 1 order on each side. Technically this can give a false negative, but it's unlikely.
            Assert.IsTrue(hitBtc.OrderBook.Asks.Count == 1);
            Assert.IsTrue(hitBtc.OrderBook.Bids.Count == 1);
        }

        [TestMethod]
        public void HitBtcAccountInformationUpdateTest()
        {
            HitBtc hitBtc = new HitBtc();

            //If there is any problem connecting to the exchange, this will throw an error.
            hitBtc.UpdateAvailableBalances();
        }

        [TestMethod]
        public void HitBtcBuySellQueryDeleteTest()
        {
            HitBtc hitBtc = new HitBtc();
            string buyOrderId;
            string sellorderId;

            //First, insert a buy and sell order. If there are any errors with either of these operations, 
            //an exception will be thrown.

            //Buy at a really low price to the order doesn't actually get executed. Note, with this amount of btc, need to sell at at least $10 to meet the minimum order requirements.
            buyOrderId = hitBtc.Buy(hitBtc.MinimumBitcoinOrderAmount, 10m);

            ////Sell at a really high price so the order doesn't actually get executed.
            //sellorderId = hitBtc.Sell(hitBtc.MinimumBitcoinOrderAmount, 9999m);

            ////Both orders should still be open
            //Assert.IsFalse(hitBtc.IsOrderFulfilled(buyOrderId));
            //Assert.IsFalse(hitBtc.IsOrderFulfilled(sellorderId));

            ////Now delete both orders. If there are any errors with either of these operations, 
            ////an exception will be thrown.
            //hitBtc.DeleteOrder(buyOrderId);
            //hitBtc.DeleteOrder(sellorderId);
        }
    }
}
