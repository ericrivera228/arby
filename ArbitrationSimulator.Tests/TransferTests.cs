using System;
using System.Collections.Generic;
using System.Data;
using ArbitrationSimulator.Exceptions;
using BitcoinExchanges;
using DatabaseLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitrationSimulator.Tests
{
    [TestClass]
    public class TransferTests
    {
        [TestMethod]
        public void TransferInsertTest()
        {
            BaseExchange sellExchange = new Kraken();
            BaseExchange buyExchange = new Bitstamp();
            int MaxId = 0;

            sellExchange.AvailableBtc = 1.0234234234m;
            buyExchange.AvailableBtc = 2.4534534m;
            decimal transferAmount = 0.6319841998m;
                                    
            //First need to create an arbitration run and opportunity
            ArbitrationRun testRun = TestsHelper.CreateArbitrationRun();
            ArbitrationOpportunity testOpportunity = TestsHelper.CreateArbitrationOpportunity(buyExchange, sellExchange, testRun.Id.Value);

            //Before the test can continue, ensure the arbitraiton opportunity was properly inserted
            Assert.IsTrue(testOpportunity.Id != null);

            Transfer testTransfer = new Transfer(buyExchange, sellExchange, transferAmount);

            //Get the max id from TRANSFER table
            Object result = DatabaseManager.ExecuteQuery("select MaxId = max(id) from TRANSFER").Rows[0]["MaxId"];

            if (result != DBNull.Value)
            {
                MaxId = (int)result;
            }

            //Now save the TRANSFER to the db
            testTransfer.PersistToDb();

            //Select the transfer from the db
            result = DatabaseManager.ExecuteQuery(string.Format("select id from TRANSFER where id = {0} AND ORIGIN_EXCHANGE = '{1}' and DESTINATION_EXCHANGE = '{2}' AND AMOUNT = {3}", testTransfer.Id, buyExchange.Name, sellExchange.Name, transferAmount)).Rows[0][0];

            //Clean up before testing result
            if (result != DBNull.Value)
            {
                //Remove the test transfer
                DatabaseManager.ExecuteNonQuery(String.Format("delete from TRANSFER where id = {0}", testTransfer.Id));
            }

            TestsHelper.DeleteArbitrationTrade(testOpportunity.Id.Value);
            TestsHelper.DeleteArbitrationRun(testRun.Id.Value);

            //The above query should return the transfer that was created.
            Assert.IsTrue(result != DBNull.Value);
        }

        [TestMethod]
        public void CompleteTransfersTest()
        {
            Btce buyExchange = new Btce();
            Kraken sellExchange = new Kraken();
            decimal initialBuyExchangeBTCBalance = 10;
            decimal initialSellExchangeBTCBalance = 10;

            buyExchange.AvailableBtc = initialBuyExchangeBTCBalance;
            sellExchange.AvailableBtc = initialSellExchangeBTCBalance;

            List<Transfer> transferList = new List<Transfer>();
            Transfer transfer1 = TransferManager.OnTimeTransfer_Simulate(buyExchange, sellExchange, 2.01m);;
            Transfer transfer2 = TransferManager.OnTimeTransfer_Simulate(buyExchange, sellExchange, 1.022m);
            Transfer transfer3 = TransferManager.OnTimeTransfer_Simulate(buyExchange, sellExchange, 0.654968519m);
            Transfer transfer4 = TransferManager.OnTimeTransfer_Simulate(buyExchange, sellExchange, 3.6549871m);

            try
            {
                transfer1.CompleteDateTime = DateTime.Now.AddHours(-1);
                transfer4.CompleteDateTime = DateTime.Now.AddHours(-1);

                //Calcualte expected balances
                decimal expectedBuyExchangeBtcBalance = initialBuyExchangeBTCBalance - transfer1.Amount - transfer2.Amount - transfer3.Amount - transfer4.Amount;
                decimal expectedSellExchangeBtcTransfer = transfer1.Amount - transfer1.OriginExchange.BtcTransferFee + transfer2.Amount - transfer2.OriginExchange.BtcTransferFee + transfer3.Amount - transfer3.OriginExchange.BtcTransferFee + transfer4.Amount - transfer4.OriginExchange.BtcTransferFee;
                decimal expectedSellExchangeBtcBalance = initialSellExchangeBTCBalance + transfer1.Amount - transfer1.OriginExchange.BtcTransferFee + transfer4.Amount - transfer4.OriginExchange.BtcTransferFee;
                decimal expectedSellBtcInTransferAfterTransferCompletion = transfer2.Amount - transfer2.OriginExchange.BtcTransferFee + transfer3.Amount - transfer3.OriginExchange.BtcTransferFee;

                //Make sure the btce has been placed in transit and removed from the origin exchange
                Assert.IsTrue(buyExchange.AvailableBtc == expectedBuyExchangeBtcBalance);
                Assert.IsTrue(sellExchange.BTCInTransfer == expectedSellExchangeBtcTransfer);

                transferList.Add(transfer1);
                transferList.Add(transfer2);
                transferList.Add(transfer3);
                transferList.Add(transfer4);

                TransferManager.CompleteTransfers(transferList);

                Assert.IsTrue(transferList.Count == 2);
                Assert.IsTrue(sellExchange.AvailableBtc == expectedSellExchangeBtcBalance);
                Assert.IsTrue(sellExchange.BTCInTransfer == expectedSellBtcInTransferAfterTransferCompletion);
            }
            finally
            {
                DatabaseManager.ExecuteNonQuery(String.Format("delete from TRANSFER where ID in ({0}, {1}, {2}, {3})", DatabaseManager.FormatNullableIntegerForDb((transfer1.Id)), DatabaseManager.FormatNullableIntegerForDb((transfer2.Id)), DatabaseManager.FormatNullableIntegerForDb((transfer3.Id)), DatabaseManager.FormatNullableIntegerForDb((transfer4.Id))));
            }
        }

        //[TestMethod]
        //public void TransferRollupTest()
        //{
        //    //1. Create a couple arbitration trades
        //    //2. Execute transferManager
        //    //3. Check values at exchanges; should match manually calculated
            
        //    //Purposefully give Kraken less btc than the total amount of btc in trades. Trade 3 should not be executed anyways, so it won't matter. If there is a
        //    //low btc balance error, then this test fails.
        //    BaseExchange kraken = new Kraken();
        //    decimal krakenOriginalBalance = 3.69347m;
        //    kraken.AvailableBtc = krakenOriginalBalance;

        //    BaseExchange bitstamp = new Bitstamp();
        //    decimal bitstampOriginalBalance = 0.0m;
        //    bitstamp.AvailableBtc = bitstampOriginalBalance;

        //    BaseExchange itBit = new ItBit();
        //    decimal itBitOriginalBalance = 79.2423423423312312m;
        //    itBit.AvailableBtc = itBitOriginalBalance;

        //    BaseExchange anx = new Anx();
        //    decimal anxOriginalBalance = 123m;
        //    anx.AvailableBtc = anxOriginalBalance;

        //    decimal trade1Amount = 1.32m;
        //    decimal trade2Amount = 2.3147m;
        //    decimal trade3Amount = 2.28m;
        //    decimal trade4Amount = 5.78m;
        //    decimal trade5Amount = 8.72m;
        //    decimal trade6Amount = 0.21489784m;

        //    List<BaseExchange> exchangeList = new List<BaseExchange>();
        //    exchangeList.Add(kraken);
        //    exchangeList.Add(bitstamp);
        //    exchangeList.Add(itBit);
        //    exchangeList.Add(anx);

        //    int rollupNumber = 2;

        //    //First need to create an arbitration run and a couple opportunities
        //    ArbitrationRun testRun = TestsHelper.CreateArbitrationRun();

        //    ArbitrationOpportunity arbitrationTrade_1 = TestsHelper.CreateArbitrationOpportunity(kraken, bitstamp, testRun.Id.Value, trade1Amount);
        //    ArbitrationOpportunity arbitrationTrade_2 = TestsHelper.CreateArbitrationOpportunity(kraken, bitstamp, testRun.Id.Value, trade2Amount);
        //    ArbitrationOpportunity arbitrationTrade_3 = TestsHelper.CreateArbitrationOpportunity(kraken, anx, testRun.Id.Value, trade3Amount);
        //    ArbitrationOpportunity arbitrationTrade_4 = TestsHelper.CreateArbitrationOpportunity(itBit, bitstamp, testRun.Id.Value, trade4Amount);
        //    ArbitrationOpportunity arbitrationTrade_5 = TestsHelper.CreateArbitrationOpportunity(itBit, bitstamp, testRun.Id.Value, trade5Amount);
        //    ArbitrationOpportunity arbitrationTrade_6 = TestsHelper.CreateArbitrationOpportunity(itBit, anx, testRun.Id.Value, trade6Amount);
        //    List<Transfer> transfers = new List<Transfer>();

        //    try
        //    {
        //        //Transfers for trades 1, 2, 4, and 5 should be made. Transfers for trades 3 and 6 should not be executed.
        //        transfers = TransferManager.DetectAndExecuteRollupTransfers_Simulate(rollupNumber, testRun.Id.Value, exchangeList);

        //        //Ensure balances are correct.
        //        decimal amountTransferredtoBitstampAfterFees = trade1Amount + trade2Amount + trade4Amount + trade5Amount; 

        //        //There was one transfer from each exchange; take into account those fees
        //        amountTransferredtoBitstampAfterFees = amountTransferredtoBitstampAfterFees - arbitrationTrade_1.BuyExchange.BtcTransferFee - arbitrationTrade_4.BuyExchange.BtcTransferFee;

        //        Assert.IsTrue(bitstamp.AvailableBtc + bitstamp.BTCInTransfer ==  bitstampOriginalBalance + amountTransferredtoBitstampAfterFees);
        //        Assert.IsTrue(bitstamp.BTCInTransfer == amountTransferredtoBitstampAfterFees);
        //        Assert.IsTrue(kraken.AvailableBtc == krakenOriginalBalance - trade1Amount - trade2Amount);
        //        Assert.IsTrue(itBit.AvailableBtc == itBitOriginalBalance - trade4Amount - trade5Amount);
        //        Assert.IsTrue(anx.AvailableBtc == anxOriginalBalance);

        //        //Ensure two transfers were executed
        //        Assert.IsTrue(transfers.Count == 2);

        //        //Ensure the trades were updated with the transfer ids; both transfers should be tied to 2 arbitration trades
        //        foreach (Transfer transfer in transfers)
        //        {
        //            DataTable resultSet = DatabaseManager.ExecuteQuery(string.Format("SELECT ID FROM ARBITRATION_TRADE WHERE TRANSFER_ID = {0} AND ARBITRATION_RUN_ID = {1} AND BUY_EXCHANGE = '{2}' AND SELL_EXCHANGE = '{3}'", transfer.Id, testRun.Id, transfer.OriginExchange.Name, transfer.DestinationExchange.Name));

        //            Assert.IsTrue(resultSet.Rows.Count == 2);
        //        }
        //    }
        //    finally 
        //    {
        //        TestsHelper.DeleteArbitrationTrade(arbitrationTrade_1.Id.Value);
        //        TestsHelper.DeleteArbitrationTrade(arbitrationTrade_2.Id.Value);
        //        TestsHelper.DeleteArbitrationTrade(arbitrationTrade_3.Id.Value);
        //        TestsHelper.DeleteArbitrationTrade(arbitrationTrade_4.Id.Value);
        //        TestsHelper.DeleteArbitrationTrade(arbitrationTrade_5.Id.Value);
        //        TestsHelper.DeleteArbitrationTrade(arbitrationTrade_6.Id.Value);

        //        foreach (Transfer transfer in transfers)
        //        {
        //            TestsHelper.DeleteTransfer(transfer.Id.Value);
        //        }

        //        TestsHelper.DeleteArbitrationRun(testRun.Id.Value);
        //    }
        //}

        /// <summary>
        /// This test case covers the scenario discovered in the 'choices' round of simulations. That is, the rollup method produces a transfer that is valid,
        /// but because previous transfers have not completed yet, a 'NotEnoughBtcException' is generated. This is ok; the proper way to handle is to just 
        /// swallow the error and try again later. This test ensures that happens.
        /// </summary>
        [TestMethod]
        public void NotEnoughtBtcExceptionAreSwallowedCorrectly()
        {
            ArbitrationRun testRun = null;
            ArbitrationOpportunity opportunity1 = null;
            ArbitrationOpportunity opportunity2 = null;
            ArbitrationOpportunity opportunity3 = null;

            try
            {
                //Set up a test run
                testRun = TestsHelper.CreateArbitrationRun();

                //Set up the buy and sell exchanges.
                Bitstamp bitstamp = new Bitstamp();
                Kraken kraken = new Kraken();
                List<BaseExchange> exchangeList = new List<BaseExchange>();
                exchangeList.Add(bitstamp);
                exchangeList.Add(kraken);

                bitstamp.AvailableBtc = 0.2m;
                bitstamp.AvailableFiat = 100m;
                kraken.AvailableBtc = 0.2m;
                kraken.AvailableFiat = 30m;


                //Initial trad; bitstamp will need to transfer 0.1 btc to kraken for this. It will be found in the rollup later.
                //Bitstamp now has 0.4 btc.
                opportunity1 = TestsHelper.CreateArbitrationOpportunity(bitstamp, kraken, testRun.Id.Value, 0.1m);

                //Made up numbers; they don't really matter for this test
                opportunity1.TotalBuyCost = 10m;                                                                                    
                opportunity1.TotalSellCost = 10m;

                bitstamp.SimulatedBuy(opportunity1.BuyAmount, opportunity1.TotalBuyCost);
                kraken.SimulatedSell(opportunity1.BuyAmount, opportunity1.TotalSellCost);
                Assert.IsTrue(bitstamp.AvailableBtc == 0.3m);

                //Now a couple a trade where btc is sold off at Bitstamp; enough to make the rollup transfer fail
                //Bitstamp now has 0.1
                opportunity2 = TestsHelper.CreateArbitrationOpportunity(kraken, bitstamp, testRun.Id.Value, 0.3m);

                //Made up numbers; they don't really matter for this test
                opportunity2.TotalBuyCost = 30m;
                opportunity2.TotalSellCost = 30m;

                kraken.SimulatedBuy(opportunity2.BuyAmount, opportunity2.TotalBuyCost);
                bitstamp.SimulatedSell(opportunity2.BuyAmount, opportunity2.TotalSellCost);
                Assert.IsTrue(bitstamp.AvailableBtc == 0.0m);

                //Now assume this trigger a transfer, so lets say 0.5btc is in transfer to bitstamp from kraken
                bitstamp.BTCInTransfer = 0.5m;

                //Now another buy at bitstamp; this will trigger a transfer rollup from bitstamp to kraken
                opportunity3 = TestsHelper.CreateArbitrationOpportunity(bitstamp, kraken, testRun.Id.Value, 0.15m);

                //Made up numbers; they don't really matter for this test
                opportunity3.TotalBuyCost = 15m;
                opportunity3.TotalSellCost = 15m;

                bitstamp.SimulatedBuy(opportunity3.BuyAmount, opportunity3.TotalBuyCost);
                kraken.SimulatedSell(opportunity3.BuyAmount, opportunity3.TotalSellCost);
                Assert.IsTrue(bitstamp.AvailableBtc == 0.15m);

                //This should detect a transfer, but not actually create it
                List<Transfer> transferFound = TransferManager.DetectAndExecuteRollupTransfers_Simulate(3, testRun.Id.Value, exchangeList);
                Assert.IsTrue(transferFound == null);

                //Erase btc in transfer and look for transfers again, this time the TransferManager should throw an exception because
                //a bad transfer has been found (it's amount is > bitstamp total btc)
                bool errorFound = false;
                bitstamp.BTCInTransfer = 0m;

                try
                {
                    TransferManager.DetectAndExecuteRollupTransfers_Simulate(2, testRun.Id.Value, exchangeList);
                }

                catch (NotEnoughBtcException e)
                {
                    errorFound = true;
                }

                Assert.IsTrue(errorFound, "TransferManager did not throw NotEnoughBtcException.");
            }

            //Clean up test data
            finally
            {
                if (opportunity1 != null)
                {
                    TestsHelper.DeleteArbitrationTrade(opportunity1.Id.Value);
                }

                if (opportunity2 != null)
                {
                    TestsHelper.DeleteArbitrationTrade(opportunity2.Id.Value);
                }

                if (opportunity3 != null)
                {
                    TestsHelper.DeleteArbitrationTrade(opportunity3.Id.Value);
                }

                if (testRun != null)
                {
                    TestsHelper.DeleteArbitrationTrade(testRun.Id.Value);
                }
            }

        }
    }
}
