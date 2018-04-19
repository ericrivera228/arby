using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ArbitrationUtilities.EnumerationObjects;
using BitcoinExchanges.ExchangeObjects;
using CommonFunctions;
using RestSharp;

namespace BitcoinExchanges
{
    public class BitX : BaseExchange
    {
        #region Class variables and Properties
        private const string BTC_USD_PAIR_SYMBOL = "BTCUSD";

        private string _addOrderPath;
        private ApiInfo _apiInfo = new ApiInfo();

        //The btc\fiat pair name symbol that is being used
        private readonly string _btcFiatPairSymbol;

        protected override ApiInfo ApiInfo
        {
            get
            {
                if (_apiInfo == null)
                {
                    _apiInfo = new ApiInfo();
                }

                return _apiInfo;
            }
        }

        #endregion

        #region Constructors

        public BitX() : base("BitX", CurrencyType.Fiat, 8)
        {
            //No fiat type provided; default is USD
            _btcFiatPairSymbol = BTC_USD_PAIR_SYMBOL;

            InitializeBitX();
        }

        public BitX(FiatType fiatTypeToUse = FiatType.Usd) : base("BitX", CurrencyType.Fiat, 8, true, fiatTypeToUse)
        {
            if (fiatTypeToUse != FiatType.Usd)
            {
                throw new Exception(Name + " does not support " + fiatTypeToUse);
            }

            //Only supported fiat type is usd
            _btcFiatPairSymbol = BTC_USD_PAIR_SYMBOL;

            InitializeBitX();
        }

        #endregion

        #region Public methods

        public override void UpdateOrderBook(int? maxSize = null)
        {
            BitXRequest orderBookRequest = new BitXRequest(OrderBookPath);
            
            Dictionary<string, dynamic> response = ApiPost(orderBookRequest);

            if (!response.ContainsKey("bids") || !response.ContainsKey("asks"))
            {
                throw new Exception("Could not update order book for " + Name + ", 'asks' or 'bids' object was not in the response.");
            }

            BuildOrderBook(response, 1, 0, maxSize);
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

        public override List<Dictionary<string, dynamic>> GetAllOpenOrders()
        {
            throw new NotImplementedException();
        }

        public override decimal RoundTotalCost(decimal costToRound)
        {
            throw new NotImplementedException();
        }

        public override void UpdateBalances()
        {
            throw new NotImplementedException();
        }

        public override void SetTradeFee()
        {
            //Trade fee for Coinbase has to be set manually; can't get it from the api.
            NameValueCollection configSettings = GetConfigSettings("ExchangeLoginInformation/" + Name);
            TradeFee = TypeConversion.ParseStringToDecimalStrict(configSettings["TradeFee"]);
        }

        #endregion

        #region Protected Methods

        protected override string BuyInternal(decimal amount, decimal price)
        {
            throw new NotImplementedException();
        }

        protected override string SellInternal(decimal amount, decimal price)
        {
            throw new NotImplementedException();
        }

        protected override string TransferInternal(decimal amount, string address)
        {
            throw new NotImplementedException();
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
            //No unique settings for Bix
        }

        #endregion

        #region Private methods

        private void InitializeBitX()
        {
            //Set the api path variables
            //For BitX, these are method names
            BaseUrl = "https://bit-x.com/api/";
            AccountBalanceInfoPath = "getInfo";
            OrderBookPath = "orderBook";
            DeleteOrderPath = "cancelOrder";
            OpenOrderPath = "activeOrders";

            //Api path specific to Bitfinex
            _addOrderPath = "trade";
        }

        #endregion

        #region Inner Classes

        private class BitXRequest : RestRequest, IExchangeRequest
        {
            private readonly ApiInfo _apiInfo;

            public BitXRequest(string resource)
                : base(resource, Method.GET)
            {
            }

            public BitXRequest(string resource, ApiInfo apiInfo)
                : base(resource, Method.POST)
            {
                _apiInfo = apiInfo;
            }

            /// <summary>
            /// Adds the signature parameter to the header of this web request. This MUST be called after (and only after)
            /// all parameters have been added to this request.
            /// </summary>
            public void AddSignatureHeader()
            {
            }
        }

        #endregion
    }
}
