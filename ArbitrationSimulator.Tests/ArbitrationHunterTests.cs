using ArbitrationUtilities.OrderObjects;
using BitcoinExchanges;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitrationSimulator.Tests
{
    [TestClass]
    public class ArbitrationHunterTests
    {
        [TestMethod]
        public void CalculateArbitrationTests_01()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Kraken kraken = new Kraken();
            ArbitrationOpportunity opportunity;
            Bitstamp bitstamp = new Bitstamp();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            kraken.OrderBook = askBook;
            bitstamp.OrderBook = bidBook;

            //Test case 1: Part of ask order is fulfilled, no trade fees to account for
            kraken.TradeFee = 0.0m;
            bitstamp.TradeFee = 0.0m;

            askBook.Asks.Add(new Order(1.1m, 400.0m));
            askBook.Asks.Add(new Order(0.8m, 402.0m));
            askBook.Asks.Add(new Order(1.0m, 410.0m));

            bidBook.Bids.Add(new Order(0.6m, 401.0m));
            bidBook.Bids.Add(new Order(1.0m, 399m));
            bidBook.Bids.Add(new Order(2.8m, 398m));

            opportunity = hunter.CalculateArbitration(kraken, bitstamp, 99m, 999999m, 0.58m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == kraken);
            Assert.IsTrue(opportunity.SellExchange == bitstamp);
            Assert.IsTrue(opportunity.BuyAmount == 0.6m);
            Assert.IsTrue(opportunity.SellAmount == 0.6m);
            Assert.IsTrue(opportunity.Profit == 0.6m);
            Assert.IsTrue(opportunity.BuyPrice == 400m);
            Assert.IsTrue(opportunity.SellPrice == 401m);
            Assert.IsTrue(opportunity.TotalBuyCost == 240m);
            Assert.IsTrue(opportunity.TotalSellCost == 240.60m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_02()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Kraken kraken = new Kraken();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            kraken.OrderBook = askBook;
            anx.OrderBook = bidBook;

            //Test case 2: All of ask order is fulfilled across multiple bids
            kraken.TradeFee = 0.25m;
            anx.TradeFee = 0.25m;

            askBook.Asks.Add(new Order(1.2m, 398m));
            askBook.Asks.Add(new Order(0.8m, 402m));
            askBook.Asks.Add(new Order(1.0m, 410m));

            bidBook.Bids.Add(new Order(0.6m, 402m));
            bidBook.Bids.Add(new Order(1.0m, 401m));
            bidBook.Bids.Add(new Order(2.8m, 398m));

            opportunity = hunter.CalculateArbitration(kraken, anx, 99m, 999999m, 1.79m, false);
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == kraken);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyAmount == 1.2m);
            Assert.IsTrue(opportunity.SellAmount == 1.2m);
            Assert.IsTrue(opportunity.Profit == 1.8015m);
            Assert.IsTrue(opportunity.BuyPrice == 398m);
            Assert.IsTrue(opportunity.SellPrice == 401m);
            Assert.IsTrue(opportunity.TotalBuyCost == 478.794m);
            Assert.IsTrue(opportunity.TotalSellCost == 480.5955m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_03()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Kraken kraken = new Kraken();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            kraken.OrderBook = askBook;
            anx.OrderBook = bidBook;

            //Test case 3: Multiple asks orders are fulfilled across one bid
            kraken.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(0.2m, 399m));
            askBook.Asks.Add(new Order(0.4m, 401m));
            askBook.Asks.Add(new Order(1.0m, 410m));

            bidBook.Bids.Add(new Order(0.6m, 405m));
            bidBook.Bids.Add(new Order(1.0m, 401m));
            bidBook.Bids.Add(new Order(2.8m, 398m));

            opportunity = hunter.CalculateArbitration(kraken, anx, 99m, 999999m, 1.348m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == kraken);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyAmount == 0.6m);
            Assert.IsTrue(opportunity.SellAmount == 0.6m);
            Assert.IsTrue(opportunity.Profit == 1.349m);
            Assert.IsTrue(opportunity.BuyPrice == 401);
            Assert.IsTrue(opportunity.SellPrice == 405);
            Assert.IsTrue(opportunity.TotalBuyCost == 240.8005m);
            Assert.IsTrue(opportunity.TotalSellCost == 242.1495m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_04()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            ItBit itbit = new ItBit();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            itbit.OrderBook = askBook;
            anx.OrderBook = bidBook;

            //Test Case 4: Multiple asks orders are fulfilled across multiple bids
            itbit.TradeFee = 0.25m;
            anx.TradeFee = 0.3m;

            askBook.Asks.Add(new Order(0.5m, 397m));
            askBook.Asks.Add(new Order(0.4m, 399m));
            askBook.Asks.Add(new Order(1.0m, 410m));

            bidBook.Bids.Add(new Order(0.2m, 405m));
            bidBook.Bids.Add(new Order(0.5m, 403m));
            bidBook.Bids.Add(new Order(0.4m, 402m));
            bidBook.Bids.Add(new Order(2.8m, 398m));

            opportunity = hunter.CalculateArbitration(itbit, anx, 99m, 999999m, 2.80m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == itbit);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyAmount == 0.9m);
            Assert.IsTrue(opportunity.SellAmount == 0.9m);
            Assert.IsTrue(opportunity.Profit == 2.81605m);
            Assert.IsTrue(opportunity.BuyPrice == 399m);
            Assert.IsTrue(opportunity.SellPrice == 402m);
            Assert.IsTrue(opportunity.TotalBuyCost == 358.99525m);
            Assert.IsTrue(opportunity.TotalSellCost == 361.8113m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_05()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            ItBit itBit = new ItBit();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            itBit.OrderBook = askBook;
            anx.OrderBook = bidBook;

            //Test case 5: Multiple asks orders are not filled by all the bids
            itBit.TradeFee = 0.25m;
            anx.TradeFee = 0.3m;

            askBook.Asks.Add(new Order(0.5m, 397m));
            askBook.Asks.Add(new Order(1.0m, 399m));
            askBook.Asks.Add(new Order(1.0m, 410m));

            bidBook.Bids.Add(new Order(0.2m, 405m));
            bidBook.Bids.Add(new Order(0.3m, 403m));
            bidBook.Bids.Add(new Order(0.4m, 402m));

            opportunity = hunter.CalculateArbitration(itBit, anx, 99m, 999999m, 2.610m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == itBit);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyAmount == 0.9m);
            Assert.IsTrue(opportunity.SellAmount == 0.9m);
            Assert.IsTrue(opportunity.Profit == 2.61665m);
            Assert.IsTrue(opportunity.BuyPrice == 399m);
            Assert.IsTrue(opportunity.SellPrice == 402m);
            Assert.IsTrue(opportunity.TotalBuyCost == 358.99525m);
            Assert.IsTrue(opportunity.TotalSellCost == 361.6119m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_06()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Kraken kraken = new Kraken();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            kraken.OrderBook = askBook;
            anx.OrderBook = bidBook;

            //Test Case 6: Max amount is lower than possible arbitration
            kraken.TradeFee = 0.25m;
            anx.TradeFee = 0.25m;

            askBook.Asks.Add(new Order(1.2m, 398m));
            askBook.Asks.Add(new Order(0.8m, 402m));
            askBook.Asks.Add(new Order(1.0m, 410m));

            bidBook.Bids.Add(new Order(0.6m, 402m));
            bidBook.Bids.Add(new Order(1.0m, 401m));
            bidBook.Bids.Add(new Order(2.8m, 398m));

            opportunity = hunter.CalculateArbitration(kraken, anx, 1.0m, 999999m, 1.5m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == kraken);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyAmount == 1.0m);
            Assert.IsTrue(opportunity.SellAmount == 1.0m);
            Assert.IsTrue(opportunity.Profit == 1.601m);
            Assert.IsTrue(opportunity.BuyPrice == 398m);
            Assert.IsTrue(opportunity.SellPrice == 401m);
            Assert.IsTrue(opportunity.TotalBuyCost == 398.995m);
            Assert.IsTrue(opportunity.TotalSellCost == 400.596m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_07()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Kraken kraken = new Kraken();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            kraken.OrderBook = askBook;
            anx.OrderBook = bidBook;

            //Test Case 7: Max amount is lower than possible arbitration
            kraken.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(0.2m, 399m));
            askBook.Asks.Add(new Order(0.4m, 401m));
            askBook.Asks.Add(new Order(1.0m, 410m));

            bidBook.Bids.Add(new Order(0.6m, 405m));
            bidBook.Bids.Add(new Order(1.0m, 401m));
            bidBook.Bids.Add(new Order(2.8m, 398m));

            opportunity = hunter.CalculateArbitration(kraken, anx, 0.4m, 999999m, 0.032m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == kraken);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyAmount == 0.4m);
            Assert.IsTrue(opportunity.SellAmount == 0.4m);
            Assert.IsTrue(opportunity.Profit == 1.033m);
            Assert.IsTrue(opportunity.BuyPrice == 401m);
            Assert.IsTrue(opportunity.SellPrice == 405m);
            Assert.IsTrue(opportunity.TotalBuyCost == 160.4m);
            Assert.IsTrue(opportunity.TotalSellCost == 161.433m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_08()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Kraken kraken = new Kraken();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();
            decimal maxFiatForTrade;
            decimal maxBtcForTrade;

            kraken.OrderBook = askBook;
            anx.OrderBook = bidBook;

            kraken.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            //Test Case 8: Cannot take full advantage of arbitration due to max USD limitation
            maxFiatForTrade = 100m;
            maxBtcForTrade = 10.0m;
            kraken.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(1.0m, 405m));

            bidBook.Bids.Add(new Order(1.0m, 410m));

            opportunity = hunter.CalculateArbitration(kraken, anx, maxBtcForTrade, maxFiatForTrade, 0.01m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == kraken);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyAmount == 0.24629783m);
            Assert.IsTrue(opportunity.SellAmount == 0.24629783m);
            Assert.IsTrue(opportunity.BuyPrice == 405m);
            Assert.IsTrue(opportunity.SellPrice == 410m);
            Assert.IsTrue(opportunity.TotalSellCost == 100.62867291395m);
            
            //Note, because Kraken rounds to 4 decimal places, the total buy cost and profit will be different that what is calculated on the sheet
            Assert.IsTrue(opportunity.TotalBuyCost == 100m);
            Assert.IsTrue(opportunity.Profit == 0.62867291395m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_09()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Kraken kraken = new Kraken();
            ArbitrationOpportunity opportunity;
            Anx bitstamp = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();
            decimal maxFiatForTrade;
            decimal maxBtcForTrade;

            kraken.OrderBook = askBook;
            bitstamp.OrderBook = bidBook;

            kraken.TradeFee = 0.25m;
            bitstamp.TradeFee = 0.35m;

            //Test Case 9: Cannot take full advantage of arbitration due to max USD limitation
            maxFiatForTrade = 100m;
            maxBtcForTrade = 10.0m;
            kraken.TradeFee = 0.25m;
            bitstamp.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(0.2m, 399m));
            askBook.Asks.Add(new Order(0.4m, 401m));
            askBook.Asks.Add(new Order(1.0m, 410m));

            bidBook.Bids.Add(new Order(0.6m, 405m));
            bidBook.Bids.Add(new Order(1.0m, 401m));
            bidBook.Bids.Add(new Order(2.8m, 398m));

            opportunity = hunter.CalculateArbitration(kraken, bitstamp, maxBtcForTrade, maxFiatForTrade, 0.01m, false);

            //Make sure opportunity was found correctly
            //Note, C# carries out more digits than when I do manual calculations, so manual calculations will be a little different than what
            //is calculated by the hunter. In these checks, I make sure the numbers returned by the hunter round to the manual calculations.
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == kraken);
            Assert.IsTrue(opportunity.SellExchange == bitstamp);
            Assert.IsTrue(opportunity.BuyAmount == 0.24975217m);
            Assert.IsTrue(opportunity.SellAmount == 0.24975217m);
            Assert.IsTrue(opportunity.BuyPrice == 401m);
            Assert.IsTrue(opportunity.SellPrice == 405m);
            Assert.IsTrue(opportunity.TotalSellCost == 100.795605149025m);

            //Note, because Kraken rounds to 4 decimal places, the total buy cost and profit will be different that what is calculated on the sheet
            Assert.IsTrue(opportunity.TotalBuyCost == 100m);
            Assert.IsTrue(opportunity.Profit == 0.795605149025m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_10()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Kraken kraken = new Kraken();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();
            decimal maxFiatForTrade;
            decimal maxBtcForTrade;

            kraken.OrderBook = askBook;
            anx.OrderBook = bidBook;

            kraken.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            //Test Case 10: Cannot take full advantage of arbitration due to max USD limitation
            maxFiatForTrade = 200m;
            maxBtcForTrade = 10.0m;
            kraken.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(0.2m, 418m));
            askBook.Asks.Add(new Order(0.1m, 419m));
            askBook.Asks.Add(new Order(1.0m, 420m));

            bidBook.Bids.Add(new Order(0.2m, 430m));
            bidBook.Bids.Add(new Order(0.24m, 429m));
            bidBook.Bids.Add(new Order(1.0m, 425m));

            opportunity = hunter.CalculateArbitration(kraken, anx, maxBtcForTrade, maxFiatForTrade, 0.01m, false);

            //Make sure opportunity was found correctly
            //Note, C# carries out more digits than when I do manual calculations, so manual calculations will be a little different than what
            //is calculated by the hunter. In these checks, I make sure the numbers returned by the hunter round to the manual calculations.
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == kraken);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyAmount == 0.47619344m);
            Assert.IsTrue(opportunity.SellAmount == 0.47619344m);
            Assert.IsTrue(opportunity.BuyPrice == 420m);
            Assert.IsTrue(opportunity.SellPrice == 425m);
            Assert.IsTrue(opportunity.TotalSellCost == 203.627014258m);

            //Note, because Kraken rounds to 4 decimal places, the total buy cost and profit will be different that what is calculated on the sheet
            Assert.IsTrue(opportunity.TotalBuyCost == 200.00m);
            Assert.IsTrue(opportunity.Profit == 3.627014258m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_11()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Kraken kraken = new Kraken();
            ArbitrationOpportunity opportunity;
            Bitstamp bitstamp = new Bitstamp();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            kraken.OrderBook = askBook;
            bitstamp.OrderBook = bidBook;

            //Test case 11: Part of ask order is fulfilled, taking into account transfer fee
            //IMPORTANT NOTE! This test assumes Kraken has a transfer fee of 0.0005, if that changes, this test will fail!
            kraken.TradeFee = 0.0m;
            bitstamp.TradeFee = 0.0m;

            askBook.Asks.Add(new Order(1.1m, 400.0m));
            askBook.Asks.Add(new Order(0.8m, 402.0m));
            askBook.Asks.Add(new Order(1.0m, 410.0m));

            bidBook.Bids.Add(new Order(0.6m, 401.0m));
            bidBook.Bids.Add(new Order(1.0m, 399m));
            bidBook.Bids.Add(new Order(2.8m, 398m));

            opportunity = hunter.CalculateArbitration(kraken, bitstamp, 99m, 999999m, 0.01m, true);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == kraken);
            Assert.IsTrue(opportunity.SellExchange == bitstamp);
            Assert.IsTrue(opportunity.BuyAmount == 0.6m);
            Assert.IsTrue(opportunity.SellAmount == 0.6m);
            Assert.IsTrue(opportunity.Profit == 0.39975m);
            Assert.IsTrue(opportunity.BuyPrice == 400m);
            Assert.IsTrue(opportunity.SellPrice == 401m);
            Assert.IsTrue(opportunity.TotalBuyCost == 240m);
            Assert.IsTrue(opportunity.TotalSellCost == 240.60m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_12()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Kraken kraken = new Kraken();
            ArbitrationOpportunity opportunity;
            Bitstamp bitstamp = new Bitstamp();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            kraken.OrderBook = askBook;
            bitstamp.OrderBook = bidBook;

            //Test case 12: Part of ask order is fulfilled, not profitable after taking into account transfer fee
            //IMPORTANT NOTE! This test assumes Kraken has a trade fee of 0.0005, if that changes, this test *MIGHT* fail!
            kraken.TradeFee = 0.0m;
            bitstamp.TradeFee = 0.0m;

            askBook.Asks.Add(new Order(1.1m, 400.0m));
            askBook.Asks.Add(new Order(0.8m, 402.0m));
            askBook.Asks.Add(new Order(1.0m, 410.0m));

            bidBook.Bids.Add(new Order(0.6m, 400.25m));
            bidBook.Bids.Add(new Order(1.0m, 399m));
            bidBook.Bids.Add(new Order(2.8m, 398m));

            opportunity = hunter.CalculateArbitration(kraken, bitstamp, 99m, 999999m, 0.01m, true);

            //Make sure opportunity was not found, as it is not profitable when taking into account transfer fee
            Assert.IsTrue(opportunity == null);

            //Calculate arbitration again, but this time, don't take into account the transfer fee. An opportunity should be found this time.
            opportunity = hunter.CalculateArbitration(kraken, bitstamp, 99m, 999999m, 0.01m, false);
            Assert.IsTrue(opportunity != null);
        }

        [TestMethod]
        public void CalculateArbitrationTests_13()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Kraken kraken = new Kraken();
            ArbitrationOpportunity opportunity;
            ItBit itBit = new ItBit();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            kraken.OrderBook = askBook;
            itBit.OrderBook = bidBook;

            //Test case 13: Amount is floor rounded to the minimum of the two exchanges
            kraken.TradeFee = 0.0m;
            itBit.TradeFee = 0.0m;

            askBook.Asks.Add(new Order(1.1m, 400.0m));
            askBook.Asks.Add(new Order(0.8m, 402.0m));
            askBook.Asks.Add(new Order(1.0m, 410.0m));

            bidBook.Bids.Add(new Order(0.65471204m, 401.0m));
            bidBook.Bids.Add(new Order(1.0m, 399m));
            bidBook.Bids.Add(new Order(2.8m, 398m));

            opportunity = hunter.CalculateArbitration(kraken, itBit, 99m, 999999m, 0.58m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == kraken);
            Assert.IsTrue(opportunity.SellExchange == itBit);
            Assert.IsTrue(opportunity.BuyAmount == 0.6547m);
            Assert.IsTrue(opportunity.SellAmount == 0.6547m);
            Assert.IsTrue(opportunity.Profit ==  0.6547m);
            Assert.IsTrue(opportunity.BuyPrice == 400m);
            Assert.IsTrue(opportunity.SellPrice == 401m);
            Assert.IsTrue(opportunity.TotalBuyCost == 261.88m);
            Assert.IsTrue(opportunity.TotalSellCost == 262.5347m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_14()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Btce btce = new Btce();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            btce.OrderBook = askBook;
            anx.OrderBook = bidBook;

            //Test case 13: Amount is floor rounded to the minimum of the two exchanges
            btce.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(1.1m, 398.0m));
            askBook.Asks.Add(new Order(0.8m, 402.0m));
            askBook.Asks.Add(new Order(1.0m, 410.0m));

            bidBook.Bids.Add(new Order(0.65479204m, 401.0m));
            bidBook.Bids.Add(new Order(1.0m, 399m));
            bidBook.Bids.Add(new Order(2.8m, 398m));

            opportunity = hunter.CalculateArbitration(btce, anx, 99m, 999999m, 0.30m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == btce);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyPrice == 398m);
            Assert.IsTrue(opportunity.SellPrice == 401m);
            Assert.IsTrue(opportunity.BuyAmount == 0.65642902m);
            Assert.IsTrue(opportunity.SellAmount == 0.65478795m);
            Assert.IsTrue(opportunity.TotalBuyCost == 261.25874996m);
            Assert.IsTrue(opportunity.TotalSellCost == 261.650973062175m);
            Assert.IsTrue(opportunity.Profit == 0.392223102175m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_15()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Btce btce = new Btce();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            btce.OrderBook = askBook;
            anx.OrderBook = bidBook;

            btce.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(1.0m, 405.0m));

            bidBook.Bids.Add(new Order(1.04m, 410.0m));

            opportunity = hunter.CalculateArbitration(btce, anx, 99m, 10.9872m, 0.01m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == btce);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyPrice == 405m);
            Assert.IsTrue(opportunity.SellPrice == 410m);
            Assert.IsTrue(opportunity.BuyAmount == 0.02712888m);
            Assert.IsTrue(opportunity.SellAmount == 0.02706106m);
            Assert.IsTrue(opportunity.TotalBuyCost == 10.9871964m);
            Assert.IsTrue(opportunity.TotalSellCost == 11.0562019789m);
            Assert.IsTrue(opportunity.Profit == 0.0690055789m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_16()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Btce btce = new Btce();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            btce.OrderBook = askBook;
            anx.OrderBook = bidBook;
            
            btce.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(1.1m, 405.0m));

            bidBook.Bids.Add(new Order(1.0m, 410.0m));

            opportunity = hunter.CalculateArbitration(btce, anx, 0.45127846m, 200m, 0.01m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == btce);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyPrice == 405m);
            Assert.IsTrue(opportunity.SellPrice == 410m);
            Assert.IsTrue(opportunity.BuyAmount == 0.45240665m);
            Assert.IsTrue(opportunity.SellAmount == 0.45127563m);
            Assert.IsTrue(opportunity.TotalBuyCost == 183.22469325m);
            Assert.IsTrue(opportunity.TotalSellCost == 184.37542777095m);
            Assert.IsTrue(opportunity.Profit == 1.15073452095m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_17()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Btce btce = new Btce();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            btce.OrderBook = askBook;
            anx.OrderBook = bidBook;

            btce.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(1.002m, 405.0m));

            bidBook.Bids.Add(new Order(1.0m, 410.0m));

            opportunity = hunter.CalculateArbitration(btce, anx, 99m, 500m, 0.01m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == btce);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyPrice == 405m);
            Assert.IsTrue(opportunity.SellPrice == 410m);
            Assert.IsTrue(opportunity.BuyAmount == 1.002m);
            Assert.IsTrue(opportunity.SellAmount == 0.999495m);
            Assert.IsTrue(opportunity.TotalBuyCost == 405.81m);
            Assert.IsTrue(opportunity.TotalSellCost == 408.358674675m);
            Assert.IsTrue(opportunity.Profit == 2.548674675m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_18()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Btce btce = new Btce();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            btce.OrderBook = askBook;
            anx.OrderBook = bidBook;

            btce.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(0.99999375m, 405.0m));

            bidBook.Bids.Add(new Order(0.9975m, 410.0m));

            opportunity = hunter.CalculateArbitration(btce, anx, 99m, 1000m, 0.01m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == btce);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyPrice == 405m);
            Assert.IsTrue(opportunity.SellPrice == 410m);
            Assert.IsTrue(opportunity.BuyAmount == 0.99999375m);
            Assert.IsTrue(opportunity.SellAmount == 0.99749377m);
            Assert.IsTrue(opportunity.TotalBuyCost == 404.99746875m);
            Assert.IsTrue(opportunity.TotalSellCost == 407.54104214005m);
            Assert.IsTrue(opportunity.Profit == 2.54357339005m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_19()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Btce btce = new Btce();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            btce.OrderBook = askBook;
            anx.OrderBook = bidBook;

            btce.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(1.0m, 405.0m));

            bidBook.Bids.Add(new Order(0.5m, 410.0m));

            opportunity = hunter.CalculateArbitration(btce, anx, 99m, 50m, 0.01m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == btce);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyPrice == 405m);
            Assert.IsTrue(opportunity.SellPrice == 410m);
            Assert.IsTrue(opportunity.BuyAmount == 0.12345679m);
            Assert.IsTrue(opportunity.SellAmount == 0.12314815m);
            Assert.IsTrue(opportunity.TotalBuyCost == 49.99999995m);
            Assert.IsTrue(opportunity.TotalSellCost == 50.31402390475m);
            Assert.IsTrue(opportunity.Profit == 0.31402395475m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_20()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Kraken kraken = new Kraken();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            kraken.OrderBook = askBook;
            anx.OrderBook = bidBook;

            kraken.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(1.0m, 405.0m));

            bidBook.Bids.Add(new Order(1.024m, 410.0m));

            opportunity = hunter.CalculateArbitration(kraken, anx, 0.084m, 20.20m, 0.01m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == kraken);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyPrice == 405m);
            Assert.IsTrue(opportunity.SellPrice == 410m);
            Assert.IsTrue(opportunity.BuyAmount == 0.04975216m);
            Assert.IsTrue(opportunity.SellAmount == 0.04975216m);
            Assert.IsTrue(opportunity.TotalSellCost == 20.3269912504m);
            
            //Note, because Kraken rounds to 4 decimal places, the total buy cost and profit will be different that what is calculated on the sheet
            Assert.IsTrue(opportunity.TotalBuyCost == 20.2m);
            Assert.IsTrue(opportunity.Profit == 0.1269912504m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_21()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Kraken kraken = new Kraken();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            kraken.OrderBook = askBook;
            anx.OrderBook = bidBook;

            kraken.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(1.0m, 405.0m));

            bidBook.Bids.Add(new Order(0.78m, 410.0m));

            opportunity = hunter.CalculateArbitration(kraken, anx, 0.87m, 20.20m, 0.01m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == kraken);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyPrice == 405m);
            Assert.IsTrue(opportunity.SellPrice == 410m);
            Assert.IsTrue(opportunity.BuyAmount == 0.04975216m);
            Assert.IsTrue(opportunity.SellAmount == 0.04975216m);
            Assert.IsTrue(opportunity.TotalSellCost == 20.3269912504m);
            
            //Note, because Kraken rounds to 4 decimal places, the total buy cost and profit will be different that what is calculated on the sheet
            Assert.IsTrue(opportunity.TotalBuyCost == 20.20m);
            Assert.IsTrue(opportunity.Profit == 0.1269912504m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_22()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Kraken kraken = new Kraken();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            kraken.OrderBook = askBook;
            anx.OrderBook = bidBook;

            kraken.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(1.0m, 405.0m));

            bidBook.Bids.Add(new Order(0.78m, 410.0m));

            opportunity = hunter.CalculateArbitration(kraken, anx, 0.09m, 228m, 0.01m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == kraken);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyPrice == 405m);
            Assert.IsTrue(opportunity.SellPrice == 410m);
            Assert.IsTrue(opportunity.BuyAmount == 0.09m);
            Assert.IsTrue(opportunity.SellAmount == 0.09m);
            Assert.IsTrue(opportunity.TotalSellCost == 36.77085m);
            
            //Note, because Kraken rounds to 4 decimal places, the total buy cost and profit will be different that what is calculated on the sheet
            Assert.IsTrue(opportunity.TotalBuyCost == 36.5411m);
            Assert.IsTrue(opportunity.Profit == 0.22975m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_23()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Kraken kraken = new Kraken();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            kraken.OrderBook = askBook;
            anx.OrderBook = bidBook;

            kraken.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(1.0m, 405.0m));

            bidBook.Bids.Add(new Order(0.78m, 410.0m));

            opportunity = hunter.CalculateArbitration(kraken, anx, 0.09m, 30.01m, 0.01m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == kraken);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyPrice == 405m);
            Assert.IsTrue(opportunity.SellPrice == 410m);
            Assert.IsTrue(opportunity.BuyAmount == 0.07391398m);
            Assert.IsTrue(opportunity.SellAmount == 0.07391398m);
            Assert.IsTrue(opportunity.TotalSellCost == 30.1986652387m);
            
            //Note, because Kraken rounds to 4 decimal places, the total buy cost and profit will be different that what is calculated on the sheet
            Assert.IsTrue(opportunity.TotalBuyCost == 30.01m);
            Assert.IsTrue(opportunity.Profit == 0.1886652387m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_24()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Btce btce = new Btce();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            btce.OrderBook = askBook;
            anx.OrderBook = bidBook;

            btce.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(0.21482587m, 405.0m));

            bidBook.Bids.Add(new Order(0.35417808m, 410.0m));

            opportunity = hunter.CalculateArbitration(btce, anx, 1000m, 2000m, 0.01m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == btce);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyPrice == 405m);
            Assert.IsTrue(opportunity.SellPrice == 410m);
            Assert.IsTrue(opportunity.BuyAmount == 0.21482587m);
            Assert.IsTrue(opportunity.SellAmount == 0.21428881m);
            Assert.IsTrue(opportunity.TotalBuyCost == 87.00447735m);
            Assert.IsTrue(opportunity.TotalSellCost == 87.55090765765m);
            Assert.IsTrue(opportunity.Profit == 0.54643030765m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_25()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Btce btce = new Btce();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            btce.OrderBook = askBook;
            anx.OrderBook = bidBook;

            btce.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(0.21482587m, 405.0m));

            bidBook.Bids.Add(new Order(0.35417808m, 410.0m));

            opportunity = hunter.CalculateArbitration(btce, anx, 0.12478543m, 2000m, 0.01m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == btce);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyPrice == 405m);
            Assert.IsTrue(opportunity.SellPrice == 410m);
            Assert.IsTrue(opportunity.BuyAmount == 0.12478543m);
            Assert.IsTrue(opportunity.SellAmount == 0.12447347m);
            Assert.IsTrue(opportunity.TotalBuyCost == 50.53809915m);
            Assert.IsTrue(opportunity.TotalSellCost == 50.85550327055m);
            Assert.IsTrue(opportunity.Profit == 0.31740412055m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_26()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Btce btce = new Btce();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            btce.OrderBook = askBook;
            anx.OrderBook = bidBook;

            btce.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(0.21482587m, 405.0m));

            bidBook.Bids.Add(new Order(0.35417808m, 410.0m));

            opportunity = hunter.CalculateArbitration(btce, anx, 0.12478543m, 25.01m, 0.01m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == btce);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyPrice == 405m);
            Assert.IsTrue(opportunity.SellPrice == 410m);
            Assert.IsTrue(opportunity.BuyAmount == 0.06175308m);
            Assert.IsTrue(opportunity.SellAmount == 0.06159870m);
            Assert.IsTrue(opportunity.TotalBuyCost == 25.0099974m);
            Assert.IsTrue(opportunity.TotalSellCost == 25.1670728655m);
            Assert.IsTrue(opportunity.Profit == 0.1570754655m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_27()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Btce btce = new Btce();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            btce.OrderBook = askBook;
            anx.OrderBook = bidBook;

            btce.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(0.91482587m, 405.0m));

            bidBook.Bids.Add(new Order(0.35417808m, 410.0m));

            opportunity = hunter.CalculateArbitration(btce, anx, 0.35987104m, 25.01m, 0.01m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == btce);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyPrice == 405m);
            Assert.IsTrue(opportunity.SellPrice == 410m);
            Assert.IsTrue(opportunity.BuyAmount == 0.06175308m);
            Assert.IsTrue(opportunity.SellAmount == 0.06159870m);
            Assert.IsTrue(opportunity.TotalBuyCost == 25.0099974m);
            Assert.IsTrue(opportunity.TotalSellCost == 25.1670728655m);
            Assert.IsTrue(opportunity.Profit == 0.1570754655m);
        }

        [TestMethod]
        public void CalculateArbitrationTests_28()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Btce btce = new Btce();
            ArbitrationOpportunity opportunity;
            Anx anx = new Anx();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            btce.OrderBook = askBook;
            anx.OrderBook = bidBook;

            btce.TradeFee = 0.25m;
            anx.TradeFee = 0.35m;

            askBook.Asks.Add(new Order(0.91482587m, 405.0m));

            bidBook.Bids.Add(new Order(0.35417808m, 410.0m));

            opportunity = hunter.CalculateArbitration(btce, anx, 0.299999999m, 25.01m, 0.01m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
            Assert.IsTrue(opportunity.BuyExchange == btce);
            Assert.IsTrue(opportunity.SellExchange == anx);
            Assert.IsTrue(opportunity.BuyPrice == 405m);
            Assert.IsTrue(opportunity.SellPrice == 410m);
            Assert.IsTrue(opportunity.BuyAmount == 0.06175308m);
            Assert.IsTrue(opportunity.SellAmount == 0.06159870m);
            Assert.IsTrue(opportunity.TotalBuyCost == 25.0099974m);
            Assert.IsTrue(opportunity.TotalSellCost == 25.1670728655m);
            Assert.IsTrue(opportunity.Profit == 0.1570754655m);
        }

        [TestMethod]
        public void Current()
        {
            ArbitrationHunter hunter = new ArbitrationHunter(null);
            Btce btce = new Btce();
            ArbitrationOpportunity opportunity;
            ItBit itBit = new ItBit();
            OrderBook askBook = new OrderBook();
            OrderBook bidBook = new OrderBook();

            btce.OrderBook = askBook;
            itBit.OrderBook = bidBook;

            btce.TradeFee = 0.2m;
            itBit.TradeFee = 0.2m;

            askBook.Asks.Add(new Order(43.01161324m, 400.00m));

            bidBook.Bids.Add(new Order(50m, 403.00m));

            opportunity = hunter.CalculateArbitration(btce, itBit, 0.1m, 20.00m, 0.01m, false);

            //Make sure opportunity was found correctly
            Assert.IsTrue(opportunity != null);
        }
    }
}
