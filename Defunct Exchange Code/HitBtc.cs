using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing.Design;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using ArbitrationSimulator.Exchanges.ExchangeObjects;
using CommonFunctions;
using RestSharp;
using ArbitrationSimulator.EnumerationObjects;

namespace ArbitrationSimulator.Exchanges
{
    public class HitBtc : Exchange
    {
        #region Class variables and properties

        private const string BTC_SYMBOL = "BTC";
        private const string EUR_SYMBOL = "EUR";
        private const string BTC_EUR_SYMBOL = "BTCEUR";

        private string _addOrderPath;
        private ApiInfo _apiInfo = new ApiInfo();

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

        public HitBtc()
            : base("HitBtc")
        {
            //TODO: Remove hardcoded trade fee
            TradeFee = 0.1m;
        }

        #endregion

        #region Public methods

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

        public override void UpdateAvailableBalances()
        {
            HitBtcRequest request2 = new HitBtcRequest(AccountBalanceInfoPath, _apiInfo, Method.GET);
            Dictionary<string, dynamic> response = ApiPost(request2);
            ArrayList balances = (ArrayList)GetValueFromResponseResult(response, "balance");

            BTCBalance = GetCurrencyAmountFromBalancesArrayList(balances, BTC_SYMBOL);
            FiatBalance = GetCurrencyAmountFromBalancesArrayList(balances, EUR_SYMBOL);
        }

        public override void UpdateTotalBalances()
        {
            throw new NotImplementedException();
        }

        public override void SetTradeFee()
        {
            //TODO: Fix this!
        }

        public override void UpdateOrderBook(int? maxSize = null)
        {
            HitBtcRequest request = new HitBtcRequest(OrderBookPath);
            Dictionary<string, dynamic> response = ApiPost(request);

            BuildOrderBook(response, 1, 0, maxSize);
        }

        #endregion

        #region Protected methods

        protected override string BuyInternal(decimal amount, decimal price)
        {
            return ExecuteOrder(amount, price, OrderType.Sell);
        }

        protected override string SellInternal(decimal amount, decimal price)
        {
            return ExecuteOrder(amount, price, OrderType.Buy);
        }

        protected override string TransferInternal(decimal amount, string address)
        {
            return null;
        }

        protected override void CheckResponseForErrors(Dictionary<string, dynamic> responseContent)
        {
            if (responseContent.ContainsKey("code"))
            {
                string errorMessage = "There was a problem connecting to the " + Name + " api: ";
                errorMessage += Environment.NewLine + "\t\tCode: " + (string)GetValueFromResponseResult(responseContent, "code");

                if (responseContent.ContainsKey("message"))
                {
                    errorMessage += Environment.NewLine + "\t\tMessage: " + (string)GetValueFromResponseResult(responseContent, "message");
                }

                else
                {
                    errorMessage += Environment.NewLine + "\t\tMessage: <none given>";
                }

                throw new WebException(errorMessage);
            }
        }

        protected override object GetValueFromResponseResult(Dictionary<string, dynamic> resultContent, string key, bool keyAbsenceAllowed = false)
        {
            //If there isn't anything in the given content, return an empty string as whatever key is being accessed is 0 or doens't have a value            
            if (!resultContent.ContainsKey(key))
            {
                if (keyAbsenceAllowed == false)
                {
                    throw new WebException("There was a problem with the " + Name + " api. '" + key + "' object was not part of the web response.");
                }

                //Adsence of key is allowed, so a nonexistant entry implies an empty string
                return "";
            }

            return resultContent[key];
        }

        protected override void SetUniqueExchangeSettings(NameValueCollection configSettings)
        {
            //This was already set in Exchange.SetUniversalConfigSettings(), but because HitBit api paths have currency pairs, need to do the substition here
            OrderBookPath = OrderBookPath.Replace("{CurrencyPair}", BTC_EUR_SYMBOL);
            _addOrderPath = configSettings["AddOrderPath"];
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// With HitBtc, buying and selling is the same Api call. So both SellInternal and BuyInternal point to this.
        /// </summary>
        /// <param name="amount">Amount of btc to be bought/sold.</param>
        /// <param name="price">Price to set for the order.</param>
        /// <param name="orderType">Can be either be "buy" or "sell".</param>
        /// <returns>String representation of the executed order.</returns>
        private string ExecuteOrder(decimal amount, decimal price, OrderType orderType)
        {
            amount = 100m;

            HitBtcRequest orderRequest = new HitBtcRequest(_addOrderPath, _apiInfo, Method.POST);
            //orderRequest.AddParameter("clientOrderId", DateTime.Now.Ticks);
            //orderRequest.AddParameter("symbol", BTC_EUR_SYMBOL);
            //orderRequest.AddParameter("side", orderType.ToString().ToLower());     //Important note: HitBtc api requires that the order type be lower case (else there will be an error), thus the ToLower().
            //orderRequest.AddParameter("price", price);
            //orderRequest.AddParameter("quantity", amount);
            //orderRequest.AddParameter("type", "limit");

            //TODO: PICK UP HERE!!!!!

            Dictionary<string, dynamic> response = ApiPost(orderRequest);

            return null;
        }

        private decimal GetCurrencyAmountFromBalancesArrayList(ArrayList arrayList, string currencyCode)
        {
            //Loop through each of the key values pairs in the array list
            foreach (Dictionary<string, object> dictionaryEntry in arrayList)
            {
                if (dictionaryEntry.ContainsKey("currency_code") && (string)dictionaryEntry["currency_code"] == currencyCode)
                {
                    //Note, the dictionary entry for cash can either be an integer or a decimal; so ToString it and let the Parse method 
                    //convert it appropriately.
                    return TypeConversion.ParseStringToDecimalStrict(dictionaryEntry["cash"].ToString());
                }
            }

            throw new Exception("Currency code '" + currencyCode + "' was not found in the given arraylist.");
        }

        #endregion

        #region Nested classes

        private class HitBtcRequest : RestRequest, IExchangeRequest
        {
            private readonly ApiInfo _apiInfo;
            private readonly long _nonce = DateTime.Now.Ticks * 10 / TimeSpan.TicksPerMillisecond;

            /// <summary>
            /// Constructor for a private request to HitBtc. Builds a requst that is ready to be posted to HitBtc.
            /// </summary>
            /// <param name="resource">Which part of the api you are going to hit. i.e. 'private/Balance'</param>
            /// <param name="apiInfo">ApiInfoStruct containing the api key and secret</param>
            public HitBtcRequest(string resource, ApiInfo apiInfo, Method methodType)
                : base(resource, methodType)
            {
                _apiInfo = apiInfo;
                AddParameter("apikey", apiInfo.Key);
                AddParameter("nonce", _nonce);
                AddSignatureHeader();
            }

            /// <summary>
            /// Constructor for a public request to HitBtc. Builds a requst that is ready to be sent to HitBtc.
            /// </summary>
            /// <param name="resource">Which part of the api you are going to hit. i.e. 'public/Depth'</param>
            public HitBtcRequest(string resource)
                : base(resource, Method.GET) {}

            public void AddSignatureHeader()
            {
                string signature;
                StringBuilder signatureString = new StringBuilder("/" + Resource + "?");

                //Add all the parameters to the signature string, with the exception of header parameters
                foreach (Parameter parameter in Parameters)
                {
                    if (parameter.Type != ParameterType.HttpHeader)
                    {
                        signatureString.Append(parameter.Name + "=" + parameter.Value + "&");
                    }
                }

                //Remove the last '&'
                signatureString.Remove(signatureString.Length - 1, 1);

                using (var hmacsha512 = new HMACSHA512(Encoding.UTF8.GetBytes(_apiInfo.Secret)))
                {
                    hmacsha512.ComputeHash(Encoding.UTF8.GetBytes(signatureString.ToString()));
                    signature = string.Concat(hmacsha512.Hash.Select(b => b.ToString("x2")).ToArray());
                }

                AddHeader("X-Signature", signature);
            }
        }


        #endregion
    }
}
