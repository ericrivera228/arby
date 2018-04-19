using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using ArbitrationSimulator.Exchanges.ExchangeObjects;

namespace ArbitrationSimulator.Exchanges
{
    public class Paymium : Exchange
    {
	    private const string ApiIVersion = "v1";

        public Paymium()
            : base("Paymium", false)
        {
            OrderBookPath = string.Format("https://paymium.com/api/{0}/data/eur/depth", ApiIVersion);

            //TODO: Remove hardcoded trade fee
            TradeFee = 0.59m;
        }

        public override Dictionary<string, dynamic> GetOrderInformation(string orderId)
        {
            throw new NotImplementedException();
        }

        public override bool IsOrderFulfilled(string orderId)
        {
            throw new NotImplementedException();
        }

        public override void DeleteOrder(string orderId)
        {
            throw new NotImplementedException();
        }

        protected override string BuyInternal(decimal amount, decimal price)
        {
            return null;
        }

        protected override string SellInternal(decimal amount, decimal price)
        {
            return null;
        }

        protected override string TransferInternal(decimal amount, string address)
        {
            return null;
        }

        protected override void CheckResponseForErrors(Dictionary<string, dynamic> responseContent)
        {
            throw new NotImplementedException();
        }

        protected override object GetValueFromResponseResult(Dictionary<string, dynamic> resultContent, string key, bool keyAbsenceAllowed = false)
        {
            throw new NotImplementedException();
        }

        protected override void SetUniqueExchangeSettings(NameValueCollection configSettings)
        {
          
        }

        protected override ApiInfo ApiInfo
        {
            get { throw new NotImplementedException(); }
        }

        public override void UpdateAvailableBalances()
        {
            throw new NotImplementedException();
        }

        public override void UpdateTotalBalances()
        {
            throw new NotImplementedException();
        }

        public override void SetTradeFee()
        {
            throw new NotImplementedException();
        }

        public override void UpdateOrderBook(int? maxSize = null)
        {
            throw new NotImplementedException();
        }
    }
}
