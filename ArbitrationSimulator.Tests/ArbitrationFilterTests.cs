using System;
using System.Collections.Generic;
using BitcoinExchanges;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitrationSimulator.Tests
{
    [TestClass]
    public class ArbitrationFilterTests
    {
        [TestInitialize()]
        public void Setup()
        {
        }

        /// <summary>
        /// Ensures ArbitrationFilter.MostProfitableTrade null when opportunity list is null or empty.
        /// </summary>
        [TestMethod]
        public void MostProfitableTrade_Rule_OpportunityList_Null_Or_Empty()
        {
            ArbitrationOpportunity result = ArbitrationFilter.MostProfitableTrade(null);
            Assert.IsTrue(result == null);

            result = ArbitrationFilter.MostProfitableTrade(new List<ArbitrationOpportunity>());
            Assert.IsTrue(result == null);
        }

        [TestMethod]
        public void MostProfitableTrade_Rule_Validate()
        {
            List<BaseExchange> exchangeList = CreateExchangeList(new Anx(), new Bitstamp(), new Btce(), new ItBit(), new Kraken());
            List<ArbitrationOpportunity> opportunityList = null;
            ArbitrationOpportunity result;

            result = ArbitrationFilter.MostProfitableTrade(opportunityList);
            Assert.IsTrue(result == null);

            opportunityList = new List<ArbitrationOpportunity>();

            result = ArbitrationFilter.MostProfitableTrade(opportunityList);
            Assert.IsTrue(result == null);

            ArbitrationOpportunity opportunity1 = new ArbitrationOpportunity(exchangeList[0], exchangeList[1]);
            opportunity1.Profit = 7.87m;

            ArbitrationOpportunity opportunity2 = new ArbitrationOpportunity(exchangeList[3], exchangeList[2]);
            opportunity2.Profit = 7.48m;

            ArbitrationOpportunity opportunity3 = new ArbitrationOpportunity(exchangeList[4], exchangeList[3]);
            opportunity3.Profit = 4.57m;

            ArbitrationOpportunity opportunity4 = new ArbitrationOpportunity(exchangeList[1], exchangeList[4]);
            opportunity4.Profit = 87.00m;

            opportunityList.Add(opportunity1);
            opportunityList.Add(opportunity2);
            opportunityList.Add(opportunity3);
            opportunityList.Add(opportunity4);

            result = ArbitrationFilter.MostProfitableTrade(opportunityList);
            Assert.IsTrue(result == opportunity4);

            opportunity2.Profit = 87.01m;
            result = ArbitrationFilter.MostProfitableTrade(opportunityList);
            Assert.IsTrue(result == opportunity2);

            opportunity1.Profit = 1000.00m;
            result = ArbitrationFilter.MostProfitableTrade(opportunityList);
            Assert.IsTrue(result == opportunity1);
        }

        /// <summary>
        /// Ensures ArbitrationFilter.TradeForExchangeWithLowestBtc throws an argument exception when the OpportunityList
        /// paramter is null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TradeForExchangeWithLowestBtc_Rule_OpportunityList_Null()
        {
            ArbitrationOpportunity result = ArbitrationFilter.TradeForExchangeWithLowestBtc(null, new List<BaseExchange>());
        }

        /// <summary>
        /// Ensures ArbitrationFilter.TradeForExchangeWithLowestBtc throws an argument exception when the ExchangeList
        /// paramter is null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TradeForExchangeWithLowestBtc_Rule_ExchangeList_Null()
        {
            ArbitrationOpportunity result = ArbitrationFilter.TradeForExchangeWithLowestBtc(new List<ArbitrationOpportunity>(), null);
        }

        /// <summary>
        /// Ensures ArbitrationFilter.TradeForExchangeWithLowestBtc returns null when an empty opportunity list 
        /// is passed.
        /// </summary>
        [TestMethod]
        public void TradeForExchangeWithLowestBtc_Rule_OpportunityList_Empty()
        {
            List<BaseExchange> exchangeList = CreateExchangeList(new Anx(), new Btce(), new Bitstamp());

            //Pass in a non empty exchange list
            ArbitrationOpportunity result = ArbitrationFilter.TradeForExchangeWithLowestBtc(new List<ArbitrationOpportunity>(), exchangeList);
            Assert.IsTrue(result == null); 
        }

        /// <summary>
        /// Ensures ArbitrationFilter.TradeForExchangeWithLowestBtc returns null when an empty exchange list 
        /// is passed.
        /// </summary>
        [TestMethod]
        public void TradeForExchangeWithLowestBtc_Rule_ExchangeList_Empty()
        {
            //Create a list of opportunities with at least 1 opportunity
            List<ArbitrationOpportunity> opportunityList = new List<ArbitrationOpportunity>();
            ArbitrationOpportunity opportunity1 = new ArbitrationOpportunity(new Anx(), new Bitstamp());
            opportunityList.Add(opportunity1);

            ArbitrationOpportunity result = ArbitrationFilter.TradeForExchangeWithLowestBtc(opportunityList, new List<BaseExchange>());
            Assert.IsTrue(result == null);
        }

        /// <summary>
        /// Tests ArbitrationFilter.TradeForExchangeWithLowestBtc
        /// </summary>
        [TestMethod]
        public void TradeForExchangeWithLowestBtc_Rule_Validate()
        {
            List<BaseExchange> exchangeList = CreateExchangeList(new Anx(), new Bitstamp(), new Btce(), new ItBit(), new Btce(), new Kraken(), new Anx());
            List<ArbitrationOpportunity> opportunityList = new List<ArbitrationOpportunity>();
            ArbitrationOpportunity result;

            exchangeList[0].AvailableBtc = 0.6524987m;
            exchangeList[1].AvailableBtc = 0.9654689m;
            exchangeList[2].AvailableBtc = 1.069509478320m;
            exchangeList[3].AvailableBtc = 8.657849810m;
            exchangeList[4].AvailableBtc = 98.14651m;
            exchangeList[5].AvailableBtc = 9.98163521m;
            exchangeList[6].AvailableBtc = 0.0m;

            ArbitrationOpportunity opportunity1 = new ArbitrationOpportunity(exchangeList[0], exchangeList[1]);
            opportunity1.Profit = 7.87m;
            opportunity1.SellExchange = exchangeList[1];
            opportunityList.Add(opportunity1);

            //Test case: only 1 opportunity in list
            result = ArbitrationFilter.MostProfitableTrade(opportunityList);
            Assert.IsTrue(result == opportunity1);

            ArbitrationOpportunity opportunity2 = new ArbitrationOpportunity(exchangeList[3], exchangeList[2]);
            opportunity2.Profit = 7.48m;
            opportunityList.Add(opportunity2);

            ArbitrationOpportunity opportunity3 = new ArbitrationOpportunity(exchangeList[5], exchangeList[0]);
            opportunity3.Profit = 4.57m;
            opportunityList.Add(opportunity3);

            ArbitrationOpportunity opportunity4 = new ArbitrationOpportunity(exchangeList[1], exchangeList[4]);
            opportunity4.Profit = 87.00m;
            opportunityList.Add(opportunity4);

            //Test case: no exchanges in the exchangelist map to any sell exchanges for an opportunity
            List<BaseExchange> exchangeList2 = CreateExchangeList(new ItBit(), new Kraken(), new ItBit(), new Kraken());
            result = ArbitrationFilter.TradeForExchangeWithLowestBtc(opportunityList, exchangeList2);
            Assert.IsTrue(result == null);

            //Test case: Correct opportunity is in the middle of the opportunity list
            result = ArbitrationFilter.TradeForExchangeWithLowestBtc(opportunityList, exchangeList);
            Assert.IsTrue(result == opportunity3);

            //Test case: All opportunities have the same profit, opportunity for UseCryptos is returned because it has the least balance
            ArbitrationOpportunity opportunity5 = new ArbitrationOpportunity(exchangeList[1], exchangeList[6]);
            opportunity4.Profit = 87.00m;
            opportunityList.Add(opportunity5);
            result = ArbitrationFilter.TradeForExchangeWithLowestBtc(opportunityList, exchangeList);
            Assert.IsTrue(result == opportunity5);
        }

        /// <summary>
        /// Tests ArbitrationFilter.MostProfitableTradeWithPercentRestriction
        /// </summary>
        [TestMethod]
        public void MostProfitableTradeWithPercentRestriction_Rule_Validate()
        {
            List<BaseExchange> exchangeList = CreateExchangeList(new Anx(), new Bitstamp(), new Btce(), new ItBit(), new Kraken(), new Anx());
            List<ArbitrationOpportunity> opportunityList = new List<ArbitrationOpportunity>();
            ArbitrationOpportunity result;

            //Ensure an exception is thrown when exchangeList is null
            try
            {
                ArbitrationFilter.MostProfitableTradeWithPercentRestriction(opportunityList, null, 0.20m);
            }
            catch (ArgumentNullException) {}

            //Ensure an exception is thrown when opportunityList is null
            try
            {
                ArbitrationFilter.MostProfitableTradeWithPercentRestriction(null, exchangeList, 0.20m);
            }
            catch (ArgumentNullException) { }

            //Ensure an exception is thrown when percentRestrictionAsDecimal is not valid
            try
            {
                ArbitrationFilter.MostProfitableTradeWithPercentRestriction(opportunityList, exchangeList, -0.0000001m);
            }
            catch (ArgumentException) { }

            try
            {
                ArbitrationFilter.MostProfitableTradeWithPercentRestriction(opportunityList, exchangeList, 1.00000001m);
            }
            catch (ArgumentException) { }

            //Ensure an empty excchange list returns null
            List<ArbitrationOpportunity> throwAwayOpportunityList = new List<ArbitrationOpportunity>();
            throwAwayOpportunityList.Add(new ArbitrationOpportunity(exchangeList[0], exchangeList[1]));
            result = ArbitrationFilter.MostProfitableTradeWithPercentRestriction(throwAwayOpportunityList, new List<BaseExchange>(), 0.45m);
            Assert.IsTrue(result == null);

            //Give all the test exchanges some money
            //Have a total of = $500.00
            exchangeList[0].AvailableFiat = 50.0m;    //10%
            exchangeList[1].AvailableFiat = 125.0m;   //25%
            exchangeList[2].AvailableFiat = 225.0m;    //45%
            exchangeList[3].AvailableFiat = 25.0m;    //5%
            exchangeList[4].AvailableFiat = 40.0m;    //8%
            exchangeList[5].AvailableFiat = 35.0m;    //7%

            //Ensure an empty opportunity list returns null
            result = ArbitrationFilter.MostProfitableTradeWithPercentRestriction(opportunityList, exchangeList, 0.45m);
            Assert.IsTrue(result == null);


            //Buy @ Anx, sell @ Bitstamp. Would give Bitstamp 30.2% of the total money
            ArbitrationOpportunity opportunity1 = new ArbitrationOpportunity(exchangeList[0], exchangeList[1]);
            opportunity1.TotalSellCost = 26.0m;
            opportunity1.Profit = 0.81m;
            opportunityList.Add(opportunity1);

            //Ensure in a list of 1, where the opportunity is valid, that opportunity is returned.
            result = ArbitrationFilter.MostProfitableTradeWithPercentRestriction(opportunityList, exchangeList, 0.45m);
            Assert.IsTrue(result == opportunity1);

            //Ensure in a list of 1, where the opportunity is not valid, null is returned
            result = ArbitrationFilter.MostProfitableTradeWithPercentRestriction(opportunityList, exchangeList, 0.25m);
            Assert.IsTrue(result == null);

            //Buy @ Bitstamp, sell @ btce. Would give Btce %53
            ArbitrationOpportunity opportunity2 = new ArbitrationOpportunity(exchangeList[1], exchangeList[2]);
            opportunity2.TotalSellCost = 40.00m;
            opportunity2.Profit = 1.07m;
            opportunityList.Add(opportunity2);

            //In an out-of-order list of 2, the most profitable is still returned
            
            result = ArbitrationFilter.MostProfitableTradeWithPercentRestriction(opportunityList, exchangeList, 0.53m);
            Assert.IsTrue(result == opportunity2);

            //Buy @ JustCoin, sell @ Kraken. Would give Kraken 8.4%
            ArbitrationOpportunity opportunity3 = new ArbitrationOpportunity(exchangeList[3], exchangeList[4]);
            opportunity3.TotalSellCost = 2.0m;
            opportunity3.Profit = 0.01m;
            opportunityList.Add(opportunity3);
            
            //List of  three, least profitable is the only valid
            result = ArbitrationFilter.MostProfitableTradeWithPercentRestriction(opportunityList, exchangeList, 0.3019m);
            Assert.IsTrue(result == opportunity3);

            //List of  two, second most profitable is valid
            result = ArbitrationFilter.MostProfitableTradeWithPercentRestriction(opportunityList, exchangeList, 0.47982m);
            Assert.IsTrue(result == opportunity1);
        }


        private List<BaseExchange> CreateExchangeList(params BaseExchange[] exchanges)
        {
            List<BaseExchange> exchangeList = new List<BaseExchange>();

            for(int counter = 0; counter < exchanges.Length; counter++)
            {
                exchangeList.Add(exchanges[counter]);
            }

            return exchangeList;
        }
    }
}
