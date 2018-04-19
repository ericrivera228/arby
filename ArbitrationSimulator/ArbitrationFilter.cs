using System;
using System.Collections.Generic;
using System.Linq;
using BitcoinExchanges;

namespace ArbitrationSimulator
{
    /// <summary>
    /// Class that provides different methods for selecting the best arbitration trade.
    /// </summary>
    public static class ArbitrationFilter
    {
        /// <summary>
        /// Returns the ArbitrationOpportunity with the largest profit in the given list. If the given list is null or
        /// empty, this method returns null.
        /// </summary>
        /// <param name="opportunityList">List of arbitration opportunities to compare.</param>
        /// <returns>ArbitrationOpportunity with the greatest profit. Null if the given list is null or empty.</returns>
        public static ArbitrationOpportunity MostProfitableTrade(List<ArbitrationOpportunity> opportunityList)
        {
            ArbitrationOpportunity highestProfitOpportunity = null;

            if (opportunityList != null && opportunityList.Count > 0)
            {
                highestProfitOpportunity = opportunityList[0];

                //Since highestProfit was initialized to the profit of the first opportunity in the list, start looping 
                //at the second index.
                for (int counter = 1; counter < opportunityList.Count; counter++)
                {
                    if (opportunityList[counter].Profit > highestProfitOpportunity.Profit)
                    {
                        highestProfitOpportunity = opportunityList[counter];
                    }
                }
            }

            return highestProfitOpportunity;
        }

        /// <summary>
        /// Encourages Distribution!
        /// Returns the Arbitration trade whose sell exchange matches the exchange which has the least amount of BTC.
        /// If the exchange with the least amount of BTC has more than one arbitration opportunity in which it is the
        /// sell exchange, the opportunity with the most profit is returened. If no such opportunities exist, null is
        /// returned. This method does not require the lists to be in a certain order.
        /// 
        /// If either OpportunityList or ExchangeList is null; this method throws an error. If either of those lists
        /// are empty, this method returns null.
        /// <param name="opportunityList">List of arbitration opportunities to compare.</param>
        /// <param name="exchangeList">List of arbitration opportunities to compare.</param>
        /// </summary>
        public static ArbitrationOpportunity TradeForExchangeWithLowestBtc(List<ArbitrationOpportunity> opportunityList, List<BaseExchange> exchangeList)
        {
            if(opportunityList == null || exchangeList == null)
            {
                throw new ArgumentNullException("OpportunityList and ExchangeList cannot be null.");
            }
            
            if(opportunityList.Count <= 0 || exchangeList.Count < 0)
            {
                return null;
            }

            List<BaseExchange> orderedExchangeList = exchangeList.OrderBy(o => o.AvailableBtc).ToList();
            List<ArbitrationOpportunity> orderedOpportunityList = opportunityList.OrderByDescending(o => o.Profit).ToList();

            //Loop through all the exchange which are ordered by least amount of BTC. Then loop through all the 
            //opportunities which are ordered by most profit. Return the first match; ensure there is enough
            //btc in the exchange for the opportunity to take place.
            foreach (BaseExchange exchange in orderedExchangeList)
            {
                foreach (ArbitrationOpportunity opportunity in orderedOpportunityList)
                {
                    if (opportunity.SellExchange == exchange && exchange.AvailableBtc >= opportunity.BuyAmount)
                    {
                        return opportunity;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Limits the amount of base currency that can be in any on exchange. The given decimal is the maximum percentage of the total base 
        /// currency in the system than an exchange can have. If an opportunity is the most profitable, but would put the sell exchange over 
        /// the limit, then the next valid opportunity is used.
        /// 
        /// If opportunityList or exchangeList are null, an exception is thrown. If percentRestrictionAsDecimal is greater than 1 or less than
        /// 0, and exception is thrown. If the given opportunity list is empty or none of the opportunities are valid, null is returned.
        /// </summary>
        /// <param name="opportunityList">List of opportunities to examine</param>
        /// <param name="exchangeList">List of exchanges the given opportunities apply to. It is assumed the base currency is up to date.</param>
        /// <param name="percentRestrictionAsDecimal">Maximum percentage of the total base currency (as a decimal) that any one exchange can have.</param>
        /// <returns>The opportunity with the highest profit that doesn't break the percent restriction.</returns>
        public static ArbitrationOpportunity MostProfitableTradeWithPercentRestriction(
            List<ArbitrationOpportunity> opportunityList, List<BaseExchange> exchangeList,
            decimal percentRestrictionAsDecimal)
        {
            if (opportunityList == null || exchangeList == null)
            {
                throw new ArgumentNullException(@"OpportunityList and ExchangeList cannot be null.");
            }

            if (percentRestrictionAsDecimal > 1 || percentRestrictionAsDecimal < 0)
            {
                throw new ArgumentException(@"percentRestrictionAsDecimal must inclusively be between 0 and 1.");
            }

            if (opportunityList.Count <= 0 || exchangeList.Count <= 0)
            {
                return null;
            }

            decimal totalBaseCurrency = 0.0m;

            //First add up total base currency
            foreach (BaseExchange exchange in exchangeList)
            {
                totalBaseCurrency += exchange.AvailableFiat;
            }

            //Defensive code; shouldn't ever happen. But just in case, have a nice error message
            if (totalBaseCurrency <= 0)
            {
                throw new Exception("Called ArbitrationHunter.MostProfitableTradeWithPercentRestriction but didn't have any base currency in any of the exchages; caused a divide by zero error.");
            }

            List<ArbitrationOpportunity> orderedOpportunityList = opportunityList.OrderByDescending(o => o.Profit).ToList();

            foreach (ArbitrationOpportunity opportunity in orderedOpportunityList)
            {
                if (Decimal.Divide(Decimal.Add(opportunity.SellExchange.AvailableFiat, opportunity.TotalSellCost), totalBaseCurrency) <= percentRestrictionAsDecimal)
                {
                    return opportunity;
                }
            }

            return null;
        }
    }
}
