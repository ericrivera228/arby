using System;
using System.Data;
using ArbitrationUtilities.EnumerationObjects;
using DatabaseLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitrationSimulator.Tests
{
    [TestClass]
    public class ArbitrationRunTests
    {
        [TestMethod]
        public void PersistArbitrationRun()
        {
            //Variables for arbitrary arbitration run values
            bool useAnx = false;
            bool useBitfinex = true;
            bool useBitstamp = false;
            bool useBitX = false;
            bool useBtce = false;
            bool useItBit = true;
            bool useKraken = true;
            bool useOkCoin = false;
            bool useCoinbase = true;
            decimal minProfit = 0.12m;
            OpportunitySelectionType opportunitySelectionMethod = OpportunitySelectionType.MostProfitableWithPercentRestriction;
            ArbitrationMode mode = ArbitrationMode.Simulation;
            TransferMode transferMode = TransferMode.OnTime;
            FiatType fiatType = FiatType.Eur;
            int? rollupTradeNumber = null;
            decimal? rollupHours = null;
            decimal? exchangeBaseCurrencyPercentRestriction = 0.34m;
            int searchInterval = 20;
            int roundsRequiredForValidation = 5;
            decimal maxBtc = 47.49684435m;
            decimal maxFiat = 984.41m;
            string logFile = "C:\\temp\\hunterOutput.csv";
            DateTime startTime = DateTime.Now;

            ArbitrationRun testRun = new ArbitrationRun();

            try
            {
                testRun.UseAnx = useAnx;
                testRun.UseBitfinex = useBitfinex;
                testRun.UseBitstamp = useBitstamp;
                testRun.UseBitX = useBitX;
                testRun.UseBtce = useBtce;
                testRun.UseCoinbase = useCoinbase;
                testRun.UseItBit = useItBit;
                testRun.UseKraken = useKraken;
                testRun.UseOkCoin = useOkCoin;
                testRun.MinimumProfit = minProfit;
                testRun.SeachIntervalMilliseconds = searchInterval;
                testRun.RoundsRequiredForValidation = roundsRequiredForValidation;
                testRun.MaxBtcTradeAmount = maxBtc;
                testRun.MaxFiatTradeAmount = maxFiat;
                testRun.LogFileName = logFile;
                testRun.StartDateTime = startTime;
                testRun.ExchangeBaseCurrencyPercentageRestriction = exchangeBaseCurrencyPercentRestriction;
                testRun.OpportunitySelectionMethod = opportunitySelectionMethod;
                testRun.ArbitrationMode = mode;
                testRun.TransferMode = transferMode;
                testRun.RollupNumber = rollupTradeNumber;
                testRun.RollupHours = rollupHours;
                testRun.FiatType = fiatType;

                //Now that the arbitration run has been set up, save it to the db.
                testRun.PersistToDb();

                //Ensure an id was set after the initial insert
                Assert.IsTrue(testRun.Id != null);

                string insertFetchSql = string.Format("" +
                        "select " +
                        "   ID, " +
                        "   CREATE_DATETIME, " +
                        "   LAST_MODIFIED_DATETIME " +
                        "from " +
                        "   ARBITRATION_RUN " +
                        "where " +
                        "   ID = {0} and " +
                        "   USE_ANX = {1} and " +
                        "   USE_BITFINEX = {2} and " + 
                        "   USE_BITSTAMP = {3} and " +
                        "   USE_BITX = {4} and " +
                        "   USE_BTCE = {5} and " +
                        "   USE_COINBASE = {6} and " + 
                        "   USE_ITBIT = {7} and " +
                        "   USE_KRAKEN = {8} and " +
                        "   USE_OKCOIN = {9} and " +
                        "   MINIMUM_PROFIT_FOR_TRADE = {10} and " +
                        "   SEARCH_INTERVAL_MILLISECONDS = {11} and " +
                        "   MAX_BTC_FOR_TRADE = {12} and " +
                        "   MAX_FIAT_FOR_TRADE = {13} and " +
                        "   LOG_FILE = '{14}' and " +
                        "   START_DATETIME = '{15}' and " +
                        "   OPPORTUNITY_SELECTION_METHOD = '{16}' and " +
                        "   MODE = '{17}' and " + 
                        "   EXCHANGE_BASE_CURRENCY_PERCENTAGE_RESTRICTION = {18} and " + 
                        "   TRANSFER_MODE = '{19}' and " + 
                        "   ROUNDS_REQUIRED_FOR_VALIDATION = {20} and " +
                        "   ROLLUP_TRADE_NUMBER IS NULL and " +
                        "   ROLLUP_HOURS IS NULL and " +
                        "   FIAT_TYPE = '{21}'", testRun.Id.Value, useAnx ? 1 : 0, useBitfinex ? 1 : 0, useBitstamp ? 1 : 0, useBitX ? 1 : 0, useBtce ? 1 : 0, useCoinbase ? 1 : 0, useItBit ? 1 : 0, useKraken ? 1 : 0, useOkCoin ? 1 : 0, minProfit, searchInterval, maxBtc, maxFiat, logFile, startTime.ToString("yyy-MM-dd HH:mm:ss"), opportunitySelectionMethod, mode, exchangeBaseCurrencyPercentRestriction, transferMode, roundsRequiredForValidation, fiatType);

                //Ensure all the values were properly inserted.
                DataTable result = DatabaseManager.ExecuteQuery(insertFetchSql);
                Assert.IsTrue(result != null && Convert.ToInt32(result.Rows[0]["ID"]) == testRun.Id.Value);

                //Create some bogus dates for CREATE_DATETIME and LAST_MODIFIED_DATETIME
                DateTime createDateTime = new DateTime(2014, 1, 1, 13, 25, 05);
                DateTime lastModifiedDateTime = new DateTime(2014, 1, 1, 13, 25, 05);

                //In order to test that CREATE_DATETIME and LAST_MODIFIED_DATETIME behave properly on updates, need to put some bogus data in there:
                DatabaseManager.ExecuteNonQuery(string.Format("update ARBITRATION_RUN set CREATE_DATETIME = '{0}', LAST_MODIFIED_DATETIME = '{1}' where ID = {2}", createDateTime.ToString("yyy-MM-dd HH:mm:ss"), lastModifiedDateTime.ToString("yyy-MM-dd HH:mm:ss"), testRun.Id.Value));

                useAnx = true;
                useBitstamp = true;
                useBtce = true;
                useItBit = false;
                useBitfinex = false;
                useBitX = true;
                useKraken = true;
                useOkCoin = false;
                useCoinbase = false;
                minProfit = 98.15m;
                searchInterval = 85;
                maxBtc = 228m;
                maxFiat = 5412m;
                startTime = DateTime.Now;
                opportunitySelectionMethod = OpportunitySelectionType.MostProfitableOpportunity;
                exchangeBaseCurrencyPercentRestriction = null;
                mode = ArbitrationMode.Live;
                transferMode = TransferMode.RollupOnTrades;
                rollupTradeNumber = 3;
                rollupHours = 2.54m;
                roundsRequiredForValidation = 8;
                fiatType = FiatType.Usd;

                testRun.UseAnx = useAnx;
                testRun.UseBitfinex = useBitfinex;
                testRun.UseBitstamp = useBitstamp;
                testRun.UseBitX = useBitX;
                testRun.UseBtce = useBtce;
                testRun.UseCoinbase = useCoinbase;
                testRun.UseItBit = useItBit;
                testRun.UseKraken = useKraken;
                testRun.UseOkCoin = useOkCoin;
                testRun.MinimumProfit = minProfit;
                testRun.SeachIntervalMilliseconds = searchInterval;
                testRun.MaxBtcTradeAmount = maxBtc;
                testRun.MaxFiatTradeAmount = maxFiat;
                testRun.LogFileName = logFile;
                testRun.StartDateTime = startTime;
                testRun.ExchangeBaseCurrencyPercentageRestriction = exchangeBaseCurrencyPercentRestriction;
                testRun.OpportunitySelectionMethod = opportunitySelectionMethod;
                testRun.ArbitrationMode = mode;
                testRun.TransferMode = transferMode;
                testRun.RollupNumber = rollupTradeNumber;
                testRun.RollupHours = rollupHours;
                testRun.RoundsRequiredForValidation = roundsRequiredForValidation;
                testRun.FiatType = fiatType;

                testRun.PersistToDb();

                string updateFetchSql = string.Format("" +
                        "select " +
                        "   ID, " +
                        "   CREATE_DATETIME, " +
                        "   LAST_MODIFIED_DATETIME " +
                        "from " +
                        "   ARBITRATION_RUN " +
                        "where " +
                        "   ID = {0} and " +
                        "   USE_ANX = {1} and " +
                        "   USE_BITFINEX = {2} and " +
                        "   USE_BITSTAMP = {3} and " +
                        "   USE_BITX = {4} and " +
                        "   USE_BTCE = {5} and " +
                        "   USE_COINBASE = {6} and " +
                        "   USE_ITBIT = {7} and " +
                        "   USE_KRAKEN = {8} and " +
                        "   USE_OKCOIN = {9} and " +
                        "   MINIMUM_PROFIT_FOR_TRADE = {10} and " +
                        "   SEARCH_INTERVAL_MILLISECONDS = {11} and " +
                        "   MAX_BTC_FOR_TRADE = {12} and " +
                        "   MAX_FIAT_FOR_TRADE = {13} and " +
                        "   LOG_FILE = '{14}' and " +
                        "   START_DATETIME = '{15}' and " +
                        "   OPPORTUNITY_SELECTION_METHOD = '{16}' and " +
                        "   MODE = '{17}' and " + 
                        "   EXCHANGE_BASE_CURRENCY_PERCENTAGE_RESTRICTION is NULL and " +
                        "   TRANSFER_MODE = '{18}' and " + 
                        "   ROLLUP_TRADE_NUMBER = {19} and " +
                        "   ROLLUP_HOURS = {20} and " +
                        "   ROUNDS_REQUIRED_FOR_VALIDATION = {21} and " +
                        "   FIAT_TYPE = '{22}'", testRun.Id.Value, useAnx ? 1 : 0, useBitfinex ? 1 : 0, useBitstamp ? 1 : 0, useBitX ? 1 : 0, useBtce ? 1 : 0, useCoinbase ? 1 : 0, useItBit ? 1 : 0, useKraken ? 1 : 0, useOkCoin ? 1 : 0, minProfit, searchInterval, maxBtc, maxFiat, logFile, startTime.ToString("yyy-MM-dd HH:mm:ss"), opportunitySelectionMethod, mode, transferMode, rollupTradeNumber, rollupHours, roundsRequiredForValidation, fiatType);

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
                if (testRun.Id != null)
                {
                    TestsHelper.DeleteArbitrationRun(testRun.Id.Value);
                }
            }
        }

        [TestMethod]
        public void ArbitrationRunHydrateTest()
        {
            ArbitrationRun testRun = new ArbitrationRun();

            //Arbitraty values for test run
            bool useAnx = true;
            bool useBtce = true;
            bool useBitstamp = true;
            bool useBitX = true;
            bool useCoinbase = true;
            bool useItBit = true;
            bool useKraken = true;
            bool useOkCoin = true;
            bool useBitfinex = true;
            decimal minimumProfit = 0.11m;
            int seachIntervalMilliseconds = 28;
            decimal maxBtcTradeAmount = 0.123654m;
            decimal maxFiatTradeAmount = 120.28m;
            OpportunitySelectionType opportunitySelectionMethod = OpportunitySelectionType.MostProfitableWithPercentRestriction;
            decimal? exchangeBaseCurrencyPercentageRestriction = 0.15m;
            string logFileName = "blahblahblah.csv";
            ArbitrationMode arbitrationMode = ArbitrationMode.Simulation;
            TransferMode transferMode = TransferMode.RollupOnTrades;
            FiatType fiatType = FiatType.Eur;
            int? rollupNumber = 2;
            int? rollupHours = 2;
            int roundsRequiredForValidation = 2;
            DateTime startDateTime = new DateTime(2015, 08, 02, 14, 36, 42);
            DateTime endDateTime = new DateTime(2015, 08, 02, 14, 36, 42);

            try
            {
                //Create and save an arbitary arbitration run
                testRun.UseAnx = useAnx;
                testRun.UseBitfinex = useBitfinex;
                testRun.UseBitstamp = useBitstamp;
                testRun.UseBitX = useBitX;
                testRun.UseBtce = useBtce;
                testRun.UseCoinbase = useCoinbase;
                testRun.UseItBit = useItBit;
                testRun.UseKraken = useKraken;
                testRun.UseOkCoin = useOkCoin;
                testRun.MinimumProfit = minimumProfit;
                testRun.SeachIntervalMilliseconds = seachIntervalMilliseconds;
                testRun.MaxBtcTradeAmount = maxBtcTradeAmount;
                testRun.MaxFiatTradeAmount = maxFiatTradeAmount;
                testRun.OpportunitySelectionMethod = opportunitySelectionMethod;
                testRun.ExchangeBaseCurrencyPercentageRestriction = exchangeBaseCurrencyPercentageRestriction;
                testRun.LogFileName = logFileName;
                testRun.ArbitrationMode = arbitrationMode;
                testRun.TransferMode = transferMode;
                testRun.RollupNumber = rollupNumber;
                testRun.RollupHours = rollupHours;
                testRun.StartDateTime = startDateTime;
                testRun.RoundsRequiredForValidation = roundsRequiredForValidation;
                testRun.EndDateTime = endDateTime;
                testRun.FiatType = fiatType;
                testRun.PersistToDb();

                //Hyrdate the run that was just created
                ArbitrationRun hydratedRun = ArbitrationRun.GetArbitrationRunFromDbById(testRun.Id.Value);

                Assert.IsTrue(hydratedRun.UseAnx == useAnx);
                Assert.IsTrue(hydratedRun.UseBitfinex == useBitfinex);
                Assert.IsTrue(hydratedRun.UseBitstamp == useBitstamp);
                Assert.IsTrue(hydratedRun.UseBitX == useBitX);
                Assert.IsTrue(hydratedRun.UseBtce == useBtce);
                Assert.IsTrue(hydratedRun.UseCoinbase == useCoinbase);
                Assert.IsTrue(hydratedRun.UseItBit == useItBit);
                Assert.IsTrue(hydratedRun.UseKraken == useKraken);
                Assert.IsTrue(hydratedRun.UseOkCoin == useOkCoin);
                Assert.IsTrue(hydratedRun.MinimumProfit == minimumProfit);
                Assert.IsTrue(hydratedRun.SeachIntervalMilliseconds == seachIntervalMilliseconds);
                Assert.IsTrue(hydratedRun.MaxBtcTradeAmount == maxBtcTradeAmount);
                Assert.IsTrue(hydratedRun.MaxFiatTradeAmount == maxFiatTradeAmount);
                Assert.IsTrue(hydratedRun.OpportunitySelectionMethod == opportunitySelectionMethod);
                Assert.IsTrue(hydratedRun.ExchangeBaseCurrencyPercentageRestriction == exchangeBaseCurrencyPercentageRestriction);
                Assert.IsTrue(hydratedRun.LogFileName == logFileName);
                Assert.IsTrue(hydratedRun.ArbitrationMode == arbitrationMode);
                Assert.IsTrue(hydratedRun.TransferMode == transferMode);
                Assert.IsTrue(hydratedRun.RollupNumber == rollupNumber);
                Assert.IsTrue(hydratedRun.RollupHours == rollupHours);
                Assert.IsTrue(hydratedRun.RoundsRequiredForValidation == roundsRequiredForValidation);
                Assert.IsTrue(hydratedRun.FiatType == fiatType);
                Assert.IsTrue(DateTime.Compare(hydratedRun.StartDateTime.Value, startDateTime) == 0);
                Assert.IsTrue(DateTime.Compare(hydratedRun.EndDateTime.Value, endDateTime) == 0);

                //This fields are populated in the persist method, so they don't have a predetermiend value. Just ensure that they have any value
                Assert.IsTrue(hydratedRun.CreateDateTime != null);
                Assert.IsTrue(hydratedRun.LastModifiedDateTime != null);

                //Test that the nullable fields hyrdate correctly when null:
                exchangeBaseCurrencyPercentageRestriction = null;
                rollupNumber = null;
                rollupHours = null;

                testRun.ExchangeBaseCurrencyPercentageRestriction = exchangeBaseCurrencyPercentageRestriction;
                testRun.RollupNumber = rollupNumber;
                testRun.RollupHours = rollupHours;
                testRun.PersistToDb();

                hydratedRun = ArbitrationRun.GetArbitrationRunFromDbById(testRun.Id.Value);
                Assert.IsTrue(hydratedRun.ExchangeBaseCurrencyPercentageRestriction == exchangeBaseCurrencyPercentageRestriction);
                Assert.IsTrue(hydratedRun.RollupNumber == rollupNumber);
                Assert.IsTrue(hydratedRun.RollupHours == rollupHours);
            }
            
            finally
            {
                //Remove test data from the database
                if (testRun.Id != null)
                {
                    TestsHelper.DeleteArbitrationRun(testRun.Id.Value);
                }
            }
        }
    }
}
