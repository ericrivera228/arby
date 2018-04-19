using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using ArbitrationSimulator.Exceptions;
using BitcoinExchanges;

namespace ArbitrationSimulator
{
    /// <summary>
    /// Class used to validate that arbitration opportunities exist through multiple rounds of arbitration searching. B
    /// Every time the ValidateOpportunities method is called, it looks to see if opporunitiesi in the given list
    /// have existed for the required number of rounds.
    /// </summary>
    public class OpportunityValidator
    {
        public readonly int RoundsRequiredForValidaton;
        private ExchangeBalanceInfo _exchangeBalanceInfoBeforeArbitrationTrade;
        private readonly List<BaseExchange> _exchangeList; 
        private readonly Dictionary<string, OpportunityValidationInfo> _opportunityDictionary;

        public OpportunityValidator(int roundsRequiredForValidation, List<BaseExchange> exchangeList)
        {
            if (roundsRequiredForValidation <= 0)
            {
                throw new ArgumentException("roundsRequiredForValidation must be greater than zero.");
            }

            RoundsRequiredForValidaton = roundsRequiredForValidation;
            _opportunityDictionary = new Dictionary<string, OpportunityValidationInfo>();
            _exchangeList = exchangeList;
        }

        public void SetFiatAndBitcoinBalanceBeforeArbitrationTrade()
        {
            _exchangeBalanceInfoBeforeArbitrationTrade = CalculateFiatAndBitcoinTotals();
        }

        public string BalanceInfoBeforeTradeString()
        {
            return _exchangeBalanceInfoBeforeArbitrationTrade.ToString();
        }

        /// <summary>
        /// Given a list of opportunities, returns a list of opportunities whose buy/sell exchange pair have had
        /// an arbitration opportunity for the required number of rounds.
        /// </summary>
        /// <param name="opportunityList"></param>
        /// <returns></returns>
        public List<ArbitrationOpportunity> ValidateOpportunities(List<ArbitrationOpportunity> opportunityList)
        {
            FindAndUpdateMatchingOpportunityInRunningList(opportunityList);
            List<ArbitrationOpportunity> returnList = CleanRunningDictionaryAndBuildValidateOpportunityList();

            return returnList;
        }

        public string ValidateExchangeBalancesAfterTrade(ArbitrationOpportunity arbitrationTrade)
        {
            ExchangeBalanceInfo postArbitrationTradebalanceInfo = CalculateFiatAndBitcoinTotals();
            decimal realizedProfit = postArbitrationTradebalanceInfo.TotalFiatBalance - _exchangeBalanceInfoBeforeArbitrationTrade.TotalFiatBalance;
            string errorMessage = "";
            string balanceString = "";
            balanceString += "\tBalances after trade:" + Environment.NewLine + postArbitrationTradebalanceInfo.ToString() + Environment.NewLine;
            balanceString += "\tDifferences: " + Environment.NewLine + BuildDifferenceString(_exchangeBalanceInfoBeforeArbitrationTrade, postArbitrationTradebalanceInfo);

            if (realizedProfit <= arbitrationTrade.Profit * 0.98m)
            {
                errorMessage = "Realized profit for arbitration trade " + arbitrationTrade.Id + " was more than 2% less than the expected profit. Expected profit = " + arbitrationTrade.Profit + ", realized profit = " + realizedProfit + ".";
            }

            else if (realizedProfit >= arbitrationTrade.Profit * 1.02m)
            {
                errorMessage = "Realized profit for arbitration trade " + arbitrationTrade.Id + " was more than 2% greater than the expected profit. Expected profit = " + arbitrationTrade.Profit + ", realized profit = " + realizedProfit + ".";
            }

            if (postArbitrationTradebalanceInfo.TotalBitcoinBalance <= _exchangeBalanceInfoBeforeArbitrationTrade.TotalBitcoinBalance * 0.98m)
            {
                errorMessage += "Bitcoin balance after arbitration trade " + arbitrationTrade.Id + " decreased by more than 2%. Bitcoin balance before trade = " + _exchangeBalanceInfoBeforeArbitrationTrade.TotalBitcoinBalance + ", bitcoin balance after trade = " + postArbitrationTradebalanceInfo.TotalBitcoinBalance + ".";
            }

            else if (postArbitrationTradebalanceInfo.TotalBitcoinBalance >=  _exchangeBalanceInfoBeforeArbitrationTrade.TotalBitcoinBalance * 1.02m)
            {
                errorMessage += "Bitcoin balance after arbitration trade " + arbitrationTrade.Id + " increased by more than 2%. Bitcoin balance before trade = " + _exchangeBalanceInfoBeforeArbitrationTrade.TotalBitcoinBalance + ", bitcoin balance after trade = " + postArbitrationTradebalanceInfo.TotalBitcoinBalance + ".";
            }

            //If there is text in erroMessage, the trade did not validate
            if (!String.IsNullOrWhiteSpace(errorMessage))
            {
                throw new ArbitrationTradeValidationException(errorMessage, balanceString);
            }

            return balanceString;
        }

        /// <summary>
        /// Given two ExchangeBalanceInfo objects, subtracts the pre trade balance from the post trade balance and builds a of the results.
        /// </summary>
        /// <param name="exchangeBalanceInfoBeforeArbitrationTrade">Balances of the exchanges before the arbitration trade.</param>
        /// <param name="postArbitrationTradebalanceInfo">Balances of the exchanges after the arbitration trade.</param>
        /// <returns></returns>
        private string BuildDifferenceString(ExchangeBalanceInfo exchangeBalanceInfoBeforeArbitrationTrade, ExchangeBalanceInfo postArbitrationTradebalanceInfo)
        {
            string returnString = "";

            foreach (KeyValuePair<string, decimal[]> preExchangeBalanceDetailInfo in exchangeBalanceInfoBeforeArbitrationTrade.exchangeBalanceDictionary)
            {
                //Grab the balance info from the post balance object as well
                KeyValuePair<string, decimal[]> postExchangeBalanceDetailInfo = postArbitrationTradebalanceInfo.exchangeBalanceDictionary.First(x => x.Key == preExchangeBalanceDetailInfo.Key);

                if (returnString.Length > 0)
                {
                    returnString += Environment.NewLine;
                }

                returnString += "\t\t" + preExchangeBalanceDetailInfo.Key + Environment.NewLine;
                returnString += "\t\t\tFiat: " + (postExchangeBalanceDetailInfo.Value[0] - preExchangeBalanceDetailInfo.Value[0]) + Environment.NewLine;
                returnString += "\t\t\tBtc: " + (postExchangeBalanceDetailInfo.Value[1] - preExchangeBalanceDetailInfo.Value[1]);
            }
            
            return returnString;
        }

        public void ValidateArbitrationTradeOrderExecution(ArbitrationOpportunity arbitrationTrade)
        {
            bool tradeFailed = false;
            StringBuilder errorMessage = new StringBuilder("Arbitration trade " + CommonFunctions.StringManipulation.NullIntString(arbitrationTrade.Id) + " did not execute. ");

            //Note, btce is special. If orders are fulfilled immediately, it just returns zero, not the actual order id. So if the exchange is btce,
            //and the order id is 0, no need to do a check.

            //Check to see if both the buy and sell orders were executed; check for btce order fulfillment first, to keep an error getting thrown on the order fulfillment check because of a non-existent '0' order id.
            if (!(arbitrationTrade.SellExchange.GetType() == typeof(Btce) && arbitrationTrade.SellOrderId == "0") && !arbitrationTrade.SellExchange.IsOrderFulfilled(arbitrationTrade.SellOrderId))
            {
                errorMessage.Append("Sell order did not get filled. ");
                tradeFailed = true;
            }

            if (!(arbitrationTrade.BuyExchange.GetType() == typeof(Btce) && arbitrationTrade.BuyOrderId == "0") && !arbitrationTrade.BuyExchange.IsOrderFulfilled(arbitrationTrade.BuyOrderId))
            {
                errorMessage.Append("Buy order did not get filled. ");
                tradeFailed = true;
            }

            if (tradeFailed)
            {
                ExchangeBalanceInfo postArbitrationTradebalanceInfo = CalculateFiatAndBitcoinTotals();
                string balanceString = "";
                balanceString += "Balances after trade:" + Environment.NewLine + postArbitrationTradebalanceInfo.ToString() + Environment.NewLine;
                balanceString += "Differences: " + Environment.NewLine + BuildDifferenceString(_exchangeBalanceInfoBeforeArbitrationTrade, postArbitrationTradebalanceInfo);

                throw new ArbitrationTradeValidationException(errorMessage.ToString(), balanceString);
            }
        }

        private ExchangeBalanceInfo CalculateFiatAndBitcoinTotals()
        {
            decimal totalFiat = 0.0m;
            decimal totalBitcoin = 0.0m;
            string balanceString = "";

            ExchangeBalanceInfo returnInfo = new ExchangeBalanceInfo();

            foreach (BaseExchange exchange in _exchangeList)
            {
                //Add to the running totals of the fiat and bitcoin balances
                totalFiat += exchange.AvailableFiat;
                totalBitcoin += exchange.AvailableBtc;

                //Add to the dictionary so the balances and be compared later
                returnInfo.exchangeBalanceDictionary.Add(exchange.Name, new []{exchange.AvailableFiat, exchange.AvailableBtc});
            }

            returnInfo.TotalBitcoinBalance = totalBitcoin;
            returnInfo.TotalFiatBalance = totalFiat;

            return returnInfo;
        }

        /// <summary>
        /// Loops through the given opportunityList and looks for matching arbitration opportunities in the 
        /// running opportunity dictionary. For opportunities that are matched, the roundsExisted property 
        /// is incremented by 1. If a matching opportunity is not found, it is added to the dictionary. In both
        /// cases, 'updated' is set to true.
        /// </summary>
        /// <param name="opportunityList">List of opportunities to locate within the running dictionary.</param>
        private void FindAndUpdateMatchingOpportunityInRunningList(List<ArbitrationOpportunity> opportunityList)
        {
            //For every opportunity in the given list, see if an opportunity with the same buy/sell exchange pair
            //exists in the running dictionary.
            foreach (ArbitrationOpportunity opportunity in opportunityList)
            {
                string dictionaryKey = opportunity.BuyExchange.Name + opportunity.SellExchange.Name;

                if (_opportunityDictionary.ContainsKey(dictionaryKey))
                {
                    OpportunityValidationInfo opportunityValidationInfo = _opportunityDictionary[dictionaryKey];

                    opportunityValidationInfo.Opportunity = opportunity;
                    opportunityValidationInfo.RoundsExisted += 1;
                    opportunityValidationInfo.Updated = true;
                    _opportunityDictionary[dictionaryKey] = opportunityValidationInfo;  
                }

                //If the current buy/sell exchange pair does not exist in the running opportunity dictionary, add it.
                else
                {
                    //This is the first round the opportunity occurs, so roundsExisted = 1
                    _opportunityDictionary.Add(dictionaryKey, new OpportunityValidationInfo() { Opportunity = opportunity, RoundsExisted = 1, Updated = true });
                }
            }
        }

        private List<ArbitrationOpportunity> CleanRunningDictionaryAndBuildValidateOpportunityList()
        {
            //List used to keep track of which entries in the dictionary need to be remvoed.
            List<string> removalList = new List<string>();

            //Can't iterate over the dictionary itself, so build a list of dictionary keys
            List<string> dictionaryKeys = new List<string>(_opportunityDictionary.Keys);
            List<ArbitrationOpportunity> returnList = new List<ArbitrationOpportunity>();

            foreach (string dictionaryKey in dictionaryKeys)
            {
                OpportunityValidationInfo opportunityValidationInfo = _opportunityDictionary[dictionaryKey];

                //Opportunity was not updated, which means it no longer exists. Flag that entry for deletion.
                if (opportunityValidationInfo.Updated == false)
                {
                    removalList.Add(dictionaryKey);
                }

                //Entry was updated, which means the opportunity still exists. If it has existed for the required number of rounds,
                //add it to the return list. Otherwise, just reset 'updated' to false to prepare for the next round.
                else
                {
                    opportunityValidationInfo.Updated = false;
                    _opportunityDictionary[dictionaryKey] = opportunityValidationInfo;

                    if (opportunityValidationInfo.RoundsExisted >= RoundsRequiredForValidaton)
                    {
                        returnList.Add(opportunityValidationInfo.Opportunity);
                    }
                }
            }

            //Remove all entries that were flagged for deletion
            foreach (string dictionaryKey in removalList)
            {
                _opportunityDictionary.Remove(dictionaryKey);
            }

            return returnList;
        }

        private struct OpportunityValidationInfo
        {
            public ArbitrationOpportunity Opportunity;
            public int RoundsExisted;
            public bool Updated;
        }


        /// <summary>
        /// Private helper class that describes the state of the balances for all exchanges at a given time period. Contains class members for 
        /// both the total fit and total bicoin. Also contains a dictionary that describes the balance of each exchange. THe key of this dictionary
        /// is the exchange name, and the value is an array of decimals, where the fiat balance is the first item in the array and the btc
        /// balance is the second item in the array.
        /// </summary>
        private class ExchangeBalanceInfo
        {
            public decimal TotalFiatBalance;
            public decimal TotalBitcoinBalance;

            public readonly Dictionary<string, decimal[]> exchangeBalanceDictionary = new Dictionary<string, decimal[]>();

            public ExchangeBalanceInfo(){}

            public ExchangeBalanceInfo(decimal totalFiatBalance, decimal totalBitcoinBalance)
            {
                TotalFiatBalance = totalFiatBalance;
                TotalBitcoinBalance = totalBitcoinBalance;
            }

            public string ToString()
            {
                string returnString = "";

                foreach (KeyValuePair<string, decimal[]> exchangeBalanceInfo in exchangeBalanceDictionary)
                {
                    if (returnString.Length > 0)
                    {
                        returnString += Environment.NewLine;
                    }

                    returnString += "\t\t" + exchangeBalanceInfo.Key + Environment.NewLine;
                    returnString += "\t\t\tFiat: " + exchangeBalanceInfo.Value[0] + Environment.NewLine;
                    returnString += "\t\t\tBtc: " + exchangeBalanceInfo.Value[1];
                }

                return returnString;
            }
        }
    }
}
