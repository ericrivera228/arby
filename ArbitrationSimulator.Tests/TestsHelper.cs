using System;
using BitcoinExchanges;
using DatabaseLayer;

namespace ArbitrationSimulator.Tests
{
    public static class TestsHelper
    {
        public static ArbitrationRun CreateArbitrationRun()
        {
            ArbitrationRun returnRun = new ArbitrationRun();
            returnRun.PersistToDb();

            //If there was a problem saving the opportunity to the db.
            if (returnRun.Id == null)
            {
                throw new Exception();
            }

            return returnRun;
        }

        public static ArbitrationOpportunity CreateArbitrationOpportunity(BaseExchange BuyExchange, BaseExchange SellExchange, int ArbitrationRunId, decimal amount = 0.0m)
        {
            ArbitrationOpportunity returnOpportunity = new ArbitrationOpportunity(BuyExchange, SellExchange);
            returnOpportunity.ArbitrationRunId = ArbitrationRunId;
            returnOpportunity.BuyAmount = amount;
            returnOpportunity.PersistToDb();

            //If there was a problem saving the opportunity to the db.
            if (returnOpportunity.Id == null)
            {
                throw new Exception();
            }

            return returnOpportunity;
        }

        public static void DeleteArbitrationRun(int ArbitrationRunId)
        {
            //Remove the test run
            DatabaseManager.ExecuteNonQuery(String.Format("delete from ARBITRATION_RUN where id = {0}", ArbitrationRunId));
        }

        public static void DeleteArbitrationTrade(int ArbitrationTradeId)
        {
            //Remove the test trade
            DatabaseManager.ExecuteNonQuery(String.Format("delete from ARBITRATION_TRADE where id = {0}", ArbitrationTradeId));
        }

        public static void DeleteTransfer(int TransferId)
        {
            //Remove the test transfer
            DatabaseManager.ExecuteNonQuery(String.Format("delete from TRANSFER where id = {0}", TransferId));
        }
    }
}
