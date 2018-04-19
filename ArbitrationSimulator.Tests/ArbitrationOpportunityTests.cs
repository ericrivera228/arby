using System;
using System.Data;
using System.Diagnostics;
using BitcoinExchanges;
using DatabaseLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitrationSimulator.Tests
{
    [TestClass]
    public class ArbitrationOpportunityTests
    {
        /// <summary>
        /// Trys to save an arbitration trade without setting the arbitration run id, which should
        /// result in a ArgumentNullException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersistArbitrationOpportunityWithoutRunId()
        {
            ArbitrationOpportunity testArbitrationTrade = new ArbitrationOpportunity(new Bitstamp(), new Kraken());

            //This should result in an error since testArbitrationTrade.ArbitrationRunId was not set.
            testArbitrationTrade.PersistToDb();
        }

        /// <summary>
        /// Ensures an arbitration trades is inserted and updated to the database correctly.
        /// </summary>
        [TestMethod]
        public void PersistArbitrationOpportunity()
        {
            //Arbitraty test values
            decimal buyAmount = 351.654316m;
            decimal sellAmount = 351.654316m;
            decimal buyPrice = 409.21m;
            decimal sellPrice = 410.87m;
            decimal totalBuyCost = 1201.14m;
            decimal totalSellCost = 1301.54m;
            decimal profit = totalSellCost - totalBuyCost;
            string buyOrderId = "Abc-123";
            string sellOrderId = "123-Abc";
            DateTime executionDateTime = DateTime.Now;

            Bitstamp buyExchange = new Bitstamp();
            Btce sellExchange = new Btce();
            ArbitrationRun testRun = null;
            ArbitrationRun updateTestRun = null;
            ArbitrationOpportunity testArbitrationTrade = null;

            try
            {
                testRun = TestsHelper.CreateArbitrationRun();
                updateTestRun = TestsHelper.CreateArbitrationRun();
                testArbitrationTrade = new ArbitrationOpportunity(buyExchange, sellExchange);
                
                testArbitrationTrade.BuyAmount = buyAmount;
                testArbitrationTrade.SellAmount = sellAmount;
                testArbitrationTrade.ArbitrationRunId = testRun.Id.Value;
                testArbitrationTrade.BuyPrice = buyPrice;
                testArbitrationTrade.ExecutionDateTime = executionDateTime;
                testArbitrationTrade.Profit = profit;
                testArbitrationTrade.SellPrice = sellPrice;
                testArbitrationTrade.TotalBuyCost = totalBuyCost;
                testArbitrationTrade.TotalSellCost = totalSellCost;
                testArbitrationTrade.BuyOrderId = buyOrderId;
                testArbitrationTrade.SellOrderId = sellOrderId;

                //Now that the arbitration trade has been set up, save it to the db.
                testArbitrationTrade.PersistToDb();

                //Ensure an id was set after the initial insert
                Assert.IsTrue(testArbitrationTrade.Id != null);

                string insertFetchSql = string.Format("" +
                        "select " +
                        "   ID, " +
                        "   CREATE_DATETIME, " +
                        "   LAST_MODIFIED_DATETIME " +
                        "from " +
                        "   ARBITRATION_TRADE " +
                        "where " +
                        "   ID = {0} AND " +
                        "   BUY_AMOUNT = {1} AND " +
                        "   BUY_PRICE = {2} AND " +
                        "   SELL_PRICE = {3} AND" +
                        "   TOTAL_BUY_COST = {4} AND " +
                        "   TOTAL_SELL_COST = {5} AND " +
                        "   PROFIT = {6} AND " +
                        "   EXECUTION_DATETIME = '{7}' AND " +
                        "   SELL_EXCHANGE = '{8}' AND " +
                        "   BUY_EXCHANGE = '{9}' AND " +
                        "   BUY_ORDER_ID = '{10}' AND " +
                        "   SELL_ORDER_ID = '{11}' AND " +
                        "   ARBITRATION_RUN_ID = {12} AND " + 
                        "   SELL_AMOUNT = {13}", testArbitrationTrade.Id.Value, buyAmount, buyPrice, sellPrice, totalBuyCost, totalSellCost, profit, executionDateTime.ToString("yyy-MM-dd HH:mm:ss"), sellExchange.Name, buyExchange.Name, buyOrderId, sellOrderId, testRun.Id.Value, sellAmount);

                //Ensure all the values were properly inserted.
                DataTable result = DatabaseManager.ExecuteQuery(insertFetchSql);
                Assert.IsTrue(result != null && Convert.ToInt32(result.Rows[0]["ID"]) == testArbitrationTrade.Id.Value);

                //Create some bogus dates for CREATE_DATETIME and LAST_MODIFIED_DATETIME
                DateTime createDateTime = new DateTime(2014, 1, 1, 13, 25, 05);
                DateTime lastModifiedDateTime = new DateTime(2014, 1, 1, 13, 25, 05);

                //In order to test that CREATE_DATETIME and LAST_MODIFIED_DATETIME behave properly on updates, need to put some bogus data in there:
                DatabaseManager.ExecuteNonQuery(string.Format("update ARBITRATION_TRADE set CREATE_DATETIME = '{0}', LAST_MODIFIED_DATETIME = '{1}' where ID = {2}", createDateTime.ToString("yyy-MM-dd HH:mm:ss"), lastModifiedDateTime.ToString("yyy-MM-dd HH:mm:ss"), testArbitrationTrade.Id.Value));

                //Update properties with arbitraty test values.
                buyAmount = 103264798.175785743216m;
                sellAmount = 103264798.175785743216m;
                buyPrice = 1.02m;
                sellPrice = 10.02m;
                totalBuyCost = 2.01m;
                totalSellCost = 18.01m;
                profit = totalSellCost - totalBuyCost;
                executionDateTime = DateTime.Now;
                buyOrderId = "Choctaw-47";
                sellOrderId = "Apache-48";
                
                //Update arbitration trade to have new values
                testArbitrationTrade.BuyAmount = buyAmount;
                testArbitrationTrade.SellAmount = sellAmount;
                testArbitrationTrade.ArbitrationRunId = updateTestRun.Id.Value;
                testArbitrationTrade.BuyPrice = buyPrice;
                testArbitrationTrade.ExecutionDateTime = executionDateTime;
                testArbitrationTrade.Profit = profit;
                testArbitrationTrade.SellPrice = sellPrice;
                testArbitrationTrade.TotalBuyCost = totalBuyCost;
                testArbitrationTrade.TotalSellCost = totalSellCost;
                testArbitrationTrade.BuyOrderId = buyOrderId;
                testArbitrationTrade.SellOrderId = sellOrderId;

                testArbitrationTrade.PersistToDb();

                string updateFetchSql = string.Format("" +
                        "select " +
                        "   ID, " +
                        "   CREATE_DATETIME, " +
                        "   LAST_MODIFIED_DATETIME " +
                        "from " +
                        "   ARBITRATION_TRADE " +
                        "where " +
                        "   ID = {0} AND " +
                        "   BUY_AMOUNT = {1} AND " +
                        "   BUY_PRICE = {2} AND " +
                        "   SELL_PRICE = {3} AND" +
                        "   TOTAL_BUY_COST = {4} AND " +
                        "   TOTAL_SELL_COST = {5} AND " +
                        "   PROFIT = {6} AND " +
                        "   EXECUTION_DATETIME = '{7}' AND " +
                        "   SELL_EXCHANGE = '{8}' AND " +
                        "   BUY_EXCHANGE = '{9}' AND " +
                        "   BUY_ORDER_ID = '{10}' AND " +
                        "   SELL_ORDER_ID = '{11}' AND " + 
                        "   ARBITRATION_RUN_ID = {12} AND " + 
                        "   SELL_AMOUNT = {13}", testArbitrationTrade.Id.Value, buyAmount, buyPrice, sellPrice, totalBuyCost, totalSellCost, profit, executionDateTime.ToString("yyy-MM-dd HH:mm:ss"), sellExchange.Name, buyExchange.Name, buyOrderId, sellOrderId, updateTestRun.Id.Value, sellAmount);

                result = DatabaseManager.ExecuteQuery(updateFetchSql);
                
                //Ensure a record was found with all the updated values
                Assert.IsTrue(result != null);

                //Ensure the CREATE_DATETIME is the same, but the LAST_MODIFIED_DATETIME is different
                Assert.IsTrue(createDateTime == (DateTime)result.Rows[0]["CREATE_DATETIME"]);
                Assert.IsTrue(lastModifiedDateTime != (DateTime)result.Rows[0]["LAST_MODIFIED_DATETIME"]);

            }
            finally
            {
                //Remove test data from the database
                if (testArbitrationTrade.Id != null)
                {
                    TestsHelper.DeleteArbitrationTrade(testArbitrationTrade.Id.Value);
                }

                if (testRun.Id != null)
                {
                    TestsHelper.DeleteArbitrationRun(testRun.Id.Value);
                }

                if (updateTestRun.Id != null)
                {
                    TestsHelper.DeleteArbitrationRun(updateTestRun.Id.Value);
                }
            }
        }
    }
}
