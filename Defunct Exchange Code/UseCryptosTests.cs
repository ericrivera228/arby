using ArbitrationSimulator.Exchanges;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitrationSimulator.Tests.Exchanges
{
    [TestClass]
    public class UseCryptosTests
    {
        [TestMethod]
        public void UseCryptosUpdateOrderBookTest()
        {
            UseCryptos useCryptos = new UseCryptos();

            //If there is any problem connecting to the exchange, this will throw an error.
            useCryptos.UpdateOrderBook(1);

            //This assumes that the exchange has at least 1 order on each side. Technically this can give a false negative, but that is unlikely.
            Assert.IsTrue(useCryptos.OrderBook.Asks.Count == 1);
            Assert.IsTrue(useCryptos.OrderBook.Bids.Count == 1);
        }

        [TestMethod]
        public void UseCryptosAccountInformationUpdateTest()
        {
            UseCryptos useCryptos = new UseCryptos();

            //If there is any problem connecting to the exchange, this will throw an error.
            useCryptos.UpdateAvailableBalances();
        }

        //TODO: Finish!
        [TestMethod]
        public void UseCryptosBuySellQueryDeleteTest()
        {
            UseCryptos useCryptos = new UseCryptos();
            string buyOrderId;
            string sellorderId;

            //First, insert a buy and sell order. If there are any errors with either of these operations, 
            //an exception will be thrown.

            //Buy at a really low price to the order doesn't actually get executed.
            buyOrderId = useCryptos.Buy(1.00m, 0.01m);

            //Sell at a really high price so the order doesn't actually get executed.
            sellorderId = useCryptos.Sell(useCryptos.MinimumBitcoinOrderAmount, 9999m);

            //Both orders should still be open
            Assert.IsFalse(useCryptos.IsOrderFulfilled(buyOrderId));
            Assert.IsFalse(useCryptos.IsOrderFulfilled(sellorderId));

            //Now delete both orders. If there are any errors with either of these operations, 
            //an exception will be thrown.
            useCryptos.DeleteOrder(buyOrderId);
            useCryptos.DeleteOrder(sellorderId);
        }
    }
}
