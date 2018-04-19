using System;
using System.Collections.Generic;
using ArbitrationUtilities.EnumerationObjects;
using BitcoinExchanges;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitrationSimulator.Tests
{
    [TestClass]
    public class OpportunityValidatorTests
    {
        [TestMethod]
        public void OpportunityListValidatesProperly()
        {
            int requiredRoundsForValidarion = 3;
            Anx anx = new Anx();
            Bitstamp bitstamp = new Bitstamp();
            Kraken kraken = new Kraken();
            ItBit itBit = new ItBit();
            List<ArbitrationOpportunity> opportunityList = new List<ArbitrationOpportunity>();
            List<ArbitrationOpportunity> validatedList;
            List<BaseExchange> exchangeList = new List<BaseExchange>();

            exchangeList.Add(anx);
            exchangeList.Add(bitstamp);
            exchangeList.Add(kraken);
            exchangeList.Add(itBit);

            OpportunityValidator opportunityValidator = new OpportunityValidator(requiredRoundsForValidarion, exchangeList);

            //Build of a list of opportunities:
            ArbitrationOpportunity krakenItBitopportunity = new ArbitrationOpportunity(kraken, itBit) {Profit = 2.23m};
            ArbitrationOpportunity krakenBitstampopportunity = new ArbitrationOpportunity(kraken, bitstamp) { Profit = 0.50m };
            ArbitrationOpportunity anxItBitopportunity = new ArbitrationOpportunity(anx, itBit) { Profit = 2.01m };
            ArbitrationOpportunity anxItBitopportunity2 = new ArbitrationOpportunity(anx, itBit) { Profit = 1.98m };
            ArbitrationOpportunity anxItBitopportunity3 = new ArbitrationOpportunity(anx, itBit) { Profit = 0.27m };

            opportunityList.Add(krakenItBitopportunity);
            opportunityList.Add(krakenBitstampopportunity);
            opportunityList.Add(anxItBitopportunity);

            //Round one:
            validatedList = opportunityValidator.ValidateOpportunities(opportunityList);
            Assert.IsTrue(validatedList.Count == 0);

            //Round two:
            opportunityList.Remove(krakenBitstampopportunity);
            validatedList = opportunityValidator.ValidateOpportunities(opportunityList);
            Assert.IsTrue(validatedList.Count == 0);

            //Round three: An oppportunity between Anx and ItBit has survived until now (alebit with a different profit)
            //Add the Kraken - Bitstamp opportunity.
            //Remove Krakken - ItBit opportunity; it only lasts two rounds.
            opportunityList.Add(krakenBitstampopportunity);
            opportunityList.Remove(krakenItBitopportunity);

            //Remove the Anx - ItBit opportunity, than add another opportunity with those exchanges but a less profit
            opportunityList.Remove(anxItBitopportunity);
            opportunityList.Add(anxItBitopportunity2);

            validatedList = opportunityValidator.ValidateOpportunities(opportunityList);
            Assert.IsTrue(validatedList.Count == 1);
            Assert.IsTrue(validatedList[0].Profit == 1.98m);

            //Round 4: anxItBitopportunity should still be in the returned list
            //Remove the Anx - ItBit opportunity, than add another opportunity with those exchanges but a less profit
            opportunityList.Remove(anxItBitopportunity);
            opportunityList.Add(anxItBitopportunity3);
            validatedList = opportunityValidator.ValidateOpportunities(opportunityList);
            Assert.IsTrue(validatedList.Count == 1);
            Assert.IsTrue(validatedList[0].Profit == 0.27m);
            
            //Round 5: anxItBitopportunity should still be in the returned list, krakenBitstampopportunity should have been added
            validatedList = opportunityValidator.ValidateOpportunities(opportunityList);
            Assert.IsTrue(validatedList.Count == 2);
            Assert.IsTrue(validatedList[0].Profit == 0.50m);
            Assert.IsTrue(validatedList[1].Profit == 0.27m);
        }

        [TestMethod]
        public void OpportunityProfitValidatesProperly()
        {
            int requiredRoundsForValidarion = 3;
            Anx anx = new Anx();
            Bitstamp bitstamp = new Bitstamp();
            Kraken kraken = new Kraken();
            ItBit itBit = new ItBit();
            List<BaseExchange> exchangeList = new List<BaseExchange>();
            decimal amount = 0.2m;
            decimal buyPrice = 150m;
            decimal totalBuyCost = 30m;
            decimal sellPrice = 200m;
            decimal totalSellCost = 40m;
            decimal profit = 10m; 


            exchangeList.Add(anx);
            exchangeList.Add(bitstamp);
            exchangeList.Add(kraken);
            exchangeList.Add(itBit);

            OpportunityValidator opportunityValidator = new OpportunityValidator(requiredRoundsForValidarion, exchangeList);

            //Note, can use a fake run id here because the trade is never persisted to the db.
            ArbitrationOpportunity testOpportunity = new ArbitrationOpportunity(anx, bitstamp, amount, buyPrice, totalBuyCost, sellPrice, totalSellCost, profit, 1);

            //First, set the balances for all the exchanges.
            anx.AvailableFiat = 100m;
            anx.AvailableBtc = 100m;
            bitstamp.AvailableFiat = 100m;
            bitstamp.AvailableBtc = 100m;
            kraken.AvailableFiat = 100m;
            kraken.AvailableBtc = 100m;
            itBit.AvailableFiat = 100m;
            itBit.AvailableBtc = 100m;

            //Scenario 1; happy path. Buy and selling works as expected.

            //Prep the validator
            opportunityValidator.SetFiatAndBitcoinBalanceBeforeArbitrationTrade();

            //Simulate the trade going through
            anx.AvailableFiat -= totalBuyCost;
            anx.AvailableBtc += amount;
            bitstamp.AvailableFiat += totalSellCost;
            bitstamp.AvailableBtc -= amount;
            
            //If there was as error, this will throw it.
            opportunityValidator.ValidateExchangeBalancesAfterTrade(testOpportunity);
            
            //Scenario 2: Buy never happens. Validator should throw an error
            bool errorThrown = false;

            anx.AvailableFiat = 100m;
            anx.AvailableBtc = 100m;
            bitstamp.AvailableFiat = 100m;
            bitstamp.AvailableBtc = 100m;
            kraken.AvailableFiat = 100m;
            kraken.AvailableBtc = 100m;
            itBit.AvailableFiat = 100m;
            itBit.AvailableBtc = 100m;

            //Prep the validator
            opportunityValidator.SetFiatAndBitcoinBalanceBeforeArbitrationTrade();

            //Simulate the trade going through, but buy never happens. This should cause the validator to throw an error.
            bitstamp.AvailableFiat += totalSellCost;
            bitstamp.AvailableBtc -= amount;

            try
            {
                opportunityValidator.ValidateExchangeBalancesAfterTrade(testOpportunity);
            }
            catch (Exception e)
            {
                errorThrown = true;
            }

            Assert.IsTrue(errorThrown);
        }

        [TestMethod]
        public void BtceOrdersDoNotGetCheckedForFulfillment()
        {
            int requiredRoundsForValidarion = 3;
            Btce btce = new Btce(); 
            Kraken kraken = new Kraken();
            List<BaseExchange> exchangeList = new List<BaseExchange>();
            
            exchangeList.Add(btce);
            exchangeList.Add(kraken);

            OpportunityValidator opportunityValidator = new OpportunityValidator(requiredRoundsForValidarion, exchangeList);
            ArbitrationOpportunity testOpportunity = new ArbitrationOpportunity(btce, btce, 0.1m, 10.22m, 1.022m, 20.00m, 2.00m, 78m, 1);

            //Set the buy and sell order id to 0 to mimic the behavior of immediately filled orders for Btce.
            testOpportunity.BuyOrderId = "0";
            testOpportunity.SellOrderId = "0";

            //This will throw an error if it is not correct.
            opportunityValidator.ValidateArbitrationTradeOrderExecution(testOpportunity);
        }

        //[TestMethod]
        //public void ValidateTradeExecutionTest()
        //{
        //    Bitstamp bitstamp = new Bitstamp(FiatType.Usd);
        //    OkCoin okcoin = new OkCoin(FiatType.Usd);

        //    List<BaseExchange> exchangeList = new List<BaseExchange>();
        //    exchangeList.Add(bitstamp);
        //    exchangeList.Add(okcoin);
            
        //    ArbitrationOpportunity testOpportunity = new ArbitrationOpportunity(bitstamp, okcoin);
        //    testOpportunity.BuyAmount = 0.055m;
        //    testOpportunity.SellAmount = 0.055m;
        //    testOpportunity.BuyOrderId = "105743727";
        //    testOpportunity.SellOrderId = "171879197";

        //    OpportunityValidator validator = new OpportunityValidator(1, exchangeList);
        //    validator.ValidateArbitrationTradeOrderExecution(testOpportunity);
        //}
    }
}
