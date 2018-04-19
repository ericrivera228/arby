using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using ArbitrationUtilities.EnumerationObjects;
using BitcoinExchanges.ExchangeObjects;
using CommonFunctions;
using RestSharp;

namespace BitcoinExchanges
{
    public class Coinbase : BaseExchange
    {
        #region Class variables and Properties

        private const string BTC_USD_PAIR_SYMBOL = "BTC-USD";

        private string _addOrderPath;
        private CoinbaseApiInfo _apiInfo = new CoinbaseApiInfo();

        //The btc\fiat pair name symbol that is being used
        private readonly string _btcFiatPairSymbol;

        protected override ApiInfo ApiInfo
        {
            get
            {
                if (_apiInfo == null)
                {
                    _apiInfo = new CoinbaseApiInfo();
                }

                return _apiInfo;
            }
        }

        #endregion

        #region Constructors

        public Coinbase() : base("Coinbase", CurrencyType.Fiat, 8, true)
        {
            //No fiat type provided; default is USD
            _btcFiatPairSymbol = BTC_USD_PAIR_SYMBOL;

            InitializeCoinbase();
        }

        public Coinbase(FiatType fiatTypeToUse = FiatType.Usd) : base("Coinbase", CurrencyType.Fiat, 8, true, fiatTypeToUse)
        {
            if (fiatTypeToUse != FiatType.Usd)
            {
                throw new Exception(Name + " does not support " + fiatTypeToUse);
            }

            //Only supported fiat type is usd
            _btcFiatPairSymbol = BTC_USD_PAIR_SYMBOL;

            InitializeCoinbase();
        }

        #endregion

        #region Public Methods

        public override void UpdateOrderBook(int? maxSize = null)
        {
            CoinbaseRequest orderBookRequest = new CoinbaseRequest(OrderBookPath);
            orderBookRequest.AddQueryParameter("level", "2");

            Dictionary<string, dynamic> response = ApiPost(orderBookRequest);

            if (!response.ContainsKey("bids") || !response.ContainsKey("asks"))
            {
                throw new Exception("Could not update order book for " + Name + ", 'asks' or 'bids' object was not in the response.");
            }

            BuildOrderBook(response, 1, 0, maxSize);
        }

        public override List<Dictionary<string, dynamic>> GetAllOpenOrders()
        {
            CoinbaseRequest openOrderRequest = new CoinbaseRequest(OpenOrderPath, _apiInfo);
            openOrderRequest.AddSignatureHeader();

            Dictionary<string, dynamic>[] response = ApiPost_Array(openOrderRequest);

            return response.ToList();
        }

        public override Dictionary<string, dynamic> GetOrderInformation(string orderId)
        {
            string orderPathWithId = OpenOrderPath + "/" + orderId;

            CoinbaseRequest orderRequest = new CoinbaseRequest(orderPathWithId, _apiInfo);
            orderRequest.AddSignatureHeader();

            Dictionary<string, dynamic> response = ApiPost(orderRequest);

            return response;
        }

        public override bool IsOrderFulfilled(string orderId)
        {
            Dictionary<string, dynamic> orderInfo = GetOrderInformation(orderId);
            string status = (string)GetValueFromResponseResult(orderInfo, "status");

            //Order that are completed at bitcoin are either 'settled' or 'done'.
            if (String.Equals("settled", status, StringComparison.InvariantCultureIgnoreCase) || String.Equals("done", status, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public override void DeleteOrder(string orderId)
        {
            string deleteOrderPathWithId = DeleteOrderPath + "/" + orderId;
            CoinbaseRequest deleteOrderRequest = new CoinbaseRequest(deleteOrderPathWithId, _apiInfo, Method.DELETE);
            deleteOrderRequest.AddSignatureHeader();

            string response = SimplePost(deleteOrderRequest);

            //If the response was not 'ok', try to derserialize it and get the error message
            if (!String.Equals(response, "OK", StringComparison.InvariantCultureIgnoreCase))
            {
                Dictionary<string, dynamic> returnedContent = null;

                try
                {
                    returnedContent = new JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(response);
                }
                catch
                {
                    //No need to do anything here; just swallow the derserialzing exception. The null check below gives the proper error message.
                }

                if (returnedContent == null)
                {
                    throw new Exception("Response from " + Name + " was null. Here is the raw response string: " + Environment.NewLine + response);
                }

                CheckResponseForErrors(returnedContent);
            }
        }

        public override void UpdateBalances()
        {
            CoinbaseRequest balanceInfoRequest = new CoinbaseRequest(AccountBalanceInfoPath, _apiInfo);
            balanceInfoRequest.AddSignatureHeader();

            Dictionary<string, dynamic>[] response = ApiPost_Array(balanceInfoRequest);

            foreach (Dictionary<string, dynamic> currencyInfoDictionary in response)
            {
                if (String.Equals((string)GetValueFromResponseResult(currencyInfoDictionary, "currency"), "USD", StringComparison.InvariantCultureIgnoreCase))
                {
                    TotalFiat = TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(currencyInfoDictionary, "balance"));
                    AvailableFiat = TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(currencyInfoDictionary, "available"));
                }

                else if (String.Equals((string)GetValueFromResponseResult(currencyInfoDictionary, "currency"), "BTC", StringComparison.InvariantCultureIgnoreCase))
                {
                    TotalBtc = TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(currencyInfoDictionary, "balance"));
                    AvailableBtc = TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(currencyInfoDictionary, "available"));
                }
            }

            //Round everything to 8 decimal places.
            TotalFiat = Math.Round(TotalFiat, 8);
            AvailableFiat = Math.Round(AvailableFiat, 8);
            TotalBtc = Math.Round(TotalBtc, 8);
            AvailableBtc = Math.Round(AvailableBtc, 8);
        }

        /// <summary>
        /// Coinbase does not do any rounding to their total cost; just returns the number.
        /// </summary>
        /// <param name="costToRound"></param>
        /// <returns></returns>
        public override decimal RoundTotalCost(decimal costToRound)
        {
            return costToRound;
        }

        public override void SetTradeFee()
        {
            //Trade fee for Coinbase has to be set manually; can't get it from the api.
            NameValueCollection configSettings = GetConfigSettings("ExchangeLoginInformation/" + Name);
            TradeFee = TypeConversion.ParseStringToDecimalStrict(configSettings["TradeFee"]);
        }

        #endregion

        #region Protected methods

        protected override string BuyInternal(decimal amount, decimal price)
        {
            return ExecuteOrder(amount, price, OrderType.Buy);
        }

        protected override string SellInternal(decimal amount, decimal price)
        {
            return ExecuteOrder(amount, price, OrderType.Sell);
        }

        protected override string TransferInternal(decimal amount, string address)
        {
            throw new NotImplementedException();
        }

        protected override void CheckResponseForErrors(Dictionary<string, dynamic> responseContent)
        {
            if (responseContent.ContainsKey("message"))
            {
                string errorMessage = "There was a problem connecting to the " + Name + " api: ";
                errorMessage += GetValueFromResponseResult(responseContent, "message");

                throw new WebException(errorMessage);
            }
        }

        protected override object GetValueFromResponseResult(Dictionary<string, dynamic> resultContent, string key, bool keyAbsenceAllowed = false)
        {
            if (!resultContent.ContainsKey(key))
            {
                throw new WebException("There was a problem with the " + Name + " api: '" + key + "' object was not part of the web response.");
            }

            return resultContent[key];  
        }

        protected override void SetUniqueExchangeSettings(NameValueCollection configSettings)
        {
            //Have to set trade fee for Coinbase manually; can't get it from the api.
            TradeFee = TypeConversion.ParseStringToDecimalStrict(configSettings["TradeFee"]);

            _apiInfo.Passphrase = configSettings["Passphrase"];
        }
        #endregion

        #region Private Methods

        private string ExecuteOrder(decimal amount, decimal price, OrderType orderType)
        {
            AddOrderJsonBody requestBody = new AddOrderJsonBody()
            {
                product_id = _btcFiatPairSymbol,
                side = orderType.ToString().ToLower(),
                price = price.ToString(),
                size = amount.ToString(),
                type = "limit"
            };

            CoinbaseRequest orderRequest = new CoinbaseRequest(_addOrderPath, _apiInfo, requestBody, Method.POST);
                     
            orderRequest.AddSignatureHeader();

            Dictionary<string, dynamic> orderResponse = ApiPost(orderRequest);

            return (string)GetValueFromResponseResult(orderResponse, "id");
        }

        private void InitializeCoinbase()
        {
            //Set the api path variables
            BaseUrl = "https://api.gdax.com";
            AccountBalanceInfoPath = "accounts";
            OrderBookPath = "products/" + _btcFiatPairSymbol + "/book";
            DeleteOrderPath = "orders";
            OrderQueryPath = "orders";
            OpenOrderPath = "orders";
            _addOrderPath = "orders";
        }

        #endregion

        #region Nested Classes

        private class CoinbaseRequest : RestRequest, IExchangeRequest
        {
            private readonly CoinbaseApiInfo _apiInfo;
            private DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            public CoinbaseRequest(string resource)
                : base(resource, Method.GET)
            {
            }

            public CoinbaseRequest(string resource, CoinbaseApiInfo apiInfo)
                : base(resource, Method.GET)
            {
                _apiInfo = apiInfo;
            }

            public CoinbaseRequest(string resource, CoinbaseApiInfo apiInfo, Method method)
                : base(resource, method)
            {
                _apiInfo = apiInfo;
            }

            public CoinbaseRequest(string resource, CoinbaseApiInfo apiInfo, object requestBody, Method method)
                : base(resource, method)
            {
                _apiInfo = apiInfo;
                RequestFormat = DataFormat.Json;
                AddBody(requestBody);
            }

            /// <summary>
            /// Adds the signature parameter to the header of this web request. This MUST be called after (and only after)
            /// all parameters have been added to this request.
            /// </summary>
            public void AddSignatureHeader()
            {
                string timestamp = GetCurrentUnixTimestampSeconds().ToString(CultureInfo.InvariantCulture);
                //timestamp = "1454703850";

                var body = GetRequestFormattedBodyString();
                
                string method = this.Method.ToString().ToUpper(CultureInfo.InvariantCulture);
                string message = timestamp + method + "/" + Resource + body;
                var hmacSig = getHash(_apiInfo.Secret, message);

                AddHeader("CB-ACCESS-KEY", _apiInfo.Key);
                AddHeader("CB-ACCESS-SIGN", hmacSig);
                AddHeader("CB-ACCESS-TIMESTAMP", timestamp);
                AddHeader("CB-ACCESS-PASSPHRASE", _apiInfo.Passphrase);
            }

            /// <summary>
            /// Returns the value of the body parameter of this request as a string. If this request does
            /// not have a body, an empty string is returned.
            /// </summary>
            /// <returns></returns>
            private string GetRequestFormattedBodyString()
            {
                string requestBodyString = "";

                foreach (Parameter parameter in Parameters)
                {
                    if (parameter.Type == ParameterType.RequestBody)
                    {
                        requestBodyString = parameter.Value.ToString();
                    }
                }

                return requestBodyString;
            }

            private string getHash(string key, string message)
            {
                var hmacsha256 = new HMACSHA256(Convert.FromBase64String(key));
                var messageBytes = Encoding.UTF8.GetBytes(message);
                return Convert.ToBase64String(hmacsha256.ComputeHash(messageBytes));
            }

            private decimal GetCurrentUnixTimestampSeconds()
            {
                return (int)(DateTime.UtcNow - _unixEpoch).TotalSeconds;
            }
        }

        private class CoinbaseApiInfo : ApiInfo
        {
            public string Passphrase;
        }

        private struct AddOrderJsonBody
        {
            public string price;
            public string size;
            public string type;
            public string side;
            public string product_id;
        }

        #endregion
    }
}
