using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using ArbitrationUtilities.EnumerationObjects;
using CommonFunctions;
using RestSharp;
using BitcoinExchanges.ExchangeObjects;

namespace BitcoinExchanges
{
    public class ItBit : BaseExchange
    {
        #region Class variables

        private const string BTC_EUR_PAIR_SYMBOL = "XBTEUR";
        private const string BTC_USD_PAIR_SYMBOL = "XBTUSD";

        //The btc\fiat pair name symbol that is being used
        private readonly string _btcFiatPairSymbol;
        private string _addOrderPath = "wallets/{WalletId}/orders/";
        private string _transferPath = "wallets/{WalletId}/cryptocurrency_withdrawals/";
        private string _walletId;
        private ItBitApiInfo _apiInfo = new ItBitApiInfo();

        protected override ApiInfo ApiInfo
        {
            get
            {
                if (_apiInfo == null)
                {
                    _apiInfo = new ItBitApiInfo();
                }

                return _apiInfo;
            }
        }

        #endregion

        #region Constructors

        public ItBit()
            : base("ItBit", CurrencyType.Fiat, 4, true)
        {
            //No fiat type provided; default is USD
            _btcFiatPairSymbol = BTC_USD_PAIR_SYMBOL;

            InitializeItBit();
        }

        public ItBit(FiatType fiatTypeToUse)
            : base("ItBit", CurrencyType.Fiat, 4, true, fiatTypeToUse)
        {
            switch (fiatTypeToUse)
            {
                case FiatType.Eur:
                    _btcFiatPairSymbol = BTC_EUR_PAIR_SYMBOL;
                    break;

                case FiatType.Usd:
                    _btcFiatPairSymbol = BTC_USD_PAIR_SYMBOL;
                    break;
            }

            InitializeItBit();
        }
        
        #endregion

        #region Public Methods

        public override void UpdateBalances()
        {
            ItBitRequest request = new ItBitRequest(BaseUrl, AccountBalanceInfoPath, _apiInfo.Key, _apiInfo.Secret);
            request.AddParameter("userId", _apiInfo.UserId);
            request.AddSignatureHeader();

            Dictionary<string, dynamic> response = ApiPost(request);
            ArrayList balancesArrayList = (ArrayList)GetValueFromResponseResult(response, "balances");

            //Get EUR from response
            Dictionary<string, object> fiatWallet = null;

            switch (FiatTypeToUse)
            {
                case FiatType.Eur:
                    fiatWallet = GetDictionaryFromArrayList(balancesArrayList, "currency", "EUR");
                    break;

                case FiatType.Usd:
                    fiatWallet = GetDictionaryFromArrayList(balancesArrayList, "currency", "USD");
                    break;
            }
            AvailableFiat = TypeConversion.ParseStringToDecimalLoose((string)GetValueFromResponseResult(fiatWallet, "availableBalance"));
            TotalFiat = TypeConversion.ParseStringToDecimalLoose((string)GetValueFromResponseResult(fiatWallet, "totalBalance"));

            //Get BTC from response
            Dictionary<string, object> btcWallet = GetDictionaryFromArrayList(balancesArrayList, "currency", "XBT");
            AvailableBtc = TypeConversion.ParseStringToDecimalLoose((string)GetValueFromResponseResult(btcWallet, "availableBalance"));
            TotalBtc = TypeConversion.ParseStringToDecimalLoose((string)GetValueFromResponseResult(btcWallet, "totalBalance"));
        }

        public override void SetTradeFee()
        {
            //Trade fee for Anx has to be set manually; can't get it from the api.
            NameValueCollection configSettings = GetConfigSettings("ExchangeLoginInformation/" + Name);
            TradeFee = TypeConversion.ParseStringToDecimalStrict(configSettings["TradeFee"]);
        }

        public override void UpdateOrderBook(int? maxSize = null)
        {
            ItBitRequest orderBookRequest = new ItBitRequest(OrderBookPath);
            Dictionary<string, dynamic> response = ApiPost(orderBookRequest);

            BuildOrderBook(response, 1, 0, maxSize);
        }

        public override Dictionary<string, dynamic> GetOrderInformation(string orderId)
        {
            //Need to build the url from the order id
            string fullOrderQueryPath = OrderQueryPath + orderId;

            ItBitRequest orderQueryRequest = new ItBitRequest(BaseUrl, fullOrderQueryPath, _apiInfo.Key, _apiInfo.Secret);
            orderQueryRequest.AddParameter("walletId", _walletId);
            orderQueryRequest.AddParameter("orderId", orderId);
            orderQueryRequest.AddSignatureHeader();

            //ItBit doesn't error properly when given a bad order id, so wrap the api call in a try/catch to get a more useful error message.
            try
            {
                Dictionary<string, dynamic> orderQueryResponse = ApiPost(orderQueryRequest);
                return orderQueryResponse;
            }
            catch (Exception e)
            {
                throw new Exception("Could not get information for order " + orderId + " from " + Name + ", most likely the order Id was wrong: " + Environment.NewLine + e.Message);
            }
        }

        public override List<Dictionary<string, dynamic>> GetAllOpenOrders()
        {
            //Can't get a list of open orders from ItBit (lame!), so just return null;
            return null;
        }

        public override void DeleteOrder(string orderId)
        {
            //Need to build the url from the order id
            string fullDeleteOrderPath = DeleteOrderPath + orderId;

            ItBitRequest deleteOrderRequest = new ItBitRequest(BaseUrl, fullDeleteOrderPath, _apiInfo.Key, _apiInfo.Secret, Method.DELETE);
            deleteOrderRequest.AddParameter("walletId", _walletId);
            deleteOrderRequest.AddParameter("orderId", orderId);
            deleteOrderRequest.AddSignatureHeader();

            //ItBit doesn't error properly when given a bad order id, so wrap the api call in a try/catch to get a more useful error message.
            try
            {
                ApiPost(deleteOrderRequest);
            }
            catch (Exception e)
            {
                throw new Exception("Could not delete order " + orderId + " from " + Name + ", most likely the order Id was wrong: " + Environment.NewLine + e.Message);
            }
        }


        public override decimal RoundTotalCost(decimal costToRound)
        {
            return Math.Round(costToRound, 4);
        }

        public override bool IsOrderFulfilled(string orderId)
        {
            Dictionary<string, dynamic> orderQueryResponse = GetOrderInformation(orderId);
            string status = (string)GetValueFromResponseResult(orderQueryResponse, "status");

            if (!status.Equals("filled", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Protected Methods

        protected override void CheckResponseForErrors(Dictionary<string, dynamic> responseContent)
        {
            if (responseContent.ContainsKey("code"))
            {
                string errorMessage = "There was a problem connecting to the " + Name + " api: code " + responseContent["code"] + " - ";
                errorMessage += StringManipulation.AppendPeriodIfNecessary((string)responseContent["description"]);

                throw new WebException(errorMessage);
            }

            //ItBit apparently has two different ways that errors from the api can manifest, so check for the second way
            if (responseContent.ContainsKey("error"))
            {
                string errorMessage = "There was a problem connecting to the " + Name + " api: ";
                errorMessage += StringManipulation.AppendPeriodIfNecessary((string)responseContent["error"]);

                throw new WebException(errorMessage);
            }
        }

        protected override void SetUniqueExchangeSettings(NameValueCollection configSettings)
        {
            _apiInfo.UserId = configSettings["UserId"];
            _walletId = configSettings["WalletId"];

            _addOrderPath = _addOrderPath.Replace("{WalletId}", _walletId);
            _transferPath = _transferPath.Replace("{WalletId}", _walletId);

            //Have to set trade fee for Itbit manually; can't get it from the api.
            TradeFee = TypeConversion.ParseStringToDecimalStrict(configSettings["TradeFee"]);
        }

        protected override string TransferInternal(decimal amount, string address)
        {
            TransferJsonBody requestBody = new TransferJsonBody
            {
                currency = "XBT",
                amount = amount.ToString(CultureInfo.InvariantCulture),
                address = address
            };

            ItBitRequest transferRequest = new ItBitRequest(BaseUrl, _transferPath, _apiInfo.Key, _apiInfo.Secret, requestBody);
            transferRequest.AddSignatureHeader();

            Dictionary<string, dynamic> response = ApiPost(transferRequest);

            //Pull the transfer Id from the response and return it
            //Note, this is an int in the object, thus the toString
            return GetValueFromResponseResult(response, "withdrawalId").ToString();
        }

        protected override string SellInternal(decimal amount, decimal price)
        {
            return ExecuteOrder(amount, price, OrderType.Sell);
        }

        protected override string BuyInternal(decimal amount, decimal price)
        {
            return ExecuteOrder(amount, price, OrderType.Buy);
        }

        /// <summary>
        /// With ItBit, buying and selling is the same Api call. So both SellInternal and BuyInternal point to this.
        /// </summary>
        /// <param name="amount">Amount of btc to be bought/sold.</param>
        /// <param name="price">Price to set for the order.</param>
        /// <param name="orderType">Can be either be "buy" or "sell".</param>
        /// <returns>String representation of the executed order.</returns>
        private string ExecuteOrder(decimal amount, decimal price, OrderType orderType)
        {
            //Need to round amount to 4 decimal places, as that is all ItBit accepts
            amount = Math.Round(amount, 4);

            AddOrderJsonBody requestBody = BuildAddOrderJsonBody(orderType, price, amount);

            ItBitRequest request = new ItBitRequest(BaseUrl, _addOrderPath, _apiInfo.Key, _apiInfo.Secret, requestBody);
            request.AddSignatureHeader();

            Dictionary<string, dynamic> response = ApiPost(request);

            //Pull the order Id from the response and return it
            return (string)GetValueFromResponseResult(response, "id");
        }

        protected override object GetValueFromResponseResult(Dictionary<string, dynamic> resultContent, string key, bool keyAbsenceAllowed = false)
        {
            if (!resultContent.ContainsKey(key))
            {
                throw new WebException("There was a problem with the " + Name + " api. '" + key + "' object was not part of the web response.");
            }

            return resultContent[key];
        }

        #endregion

        #region Private Methods

        private Dictionary<string, object> GetDictionaryFromArrayList(ArrayList arrayList, string keyName, string value)
        {
            if (String.IsNullOrWhiteSpace(keyName))
            {
                throw new Exception("GetDictionaryFromArrayList called with empty key string.");
            }

            if (String.IsNullOrWhiteSpace(value))
            {
                throw new Exception("GetDictionaryFromArrayList called with empty value string.");
            }

            if (arrayList != null && arrayList.Count > 0)
            {
                foreach (Dictionary<string, object> dictionary in arrayList)
                {
                    if (dictionary.ContainsKey(keyName) && (string)dictionary[keyName] == value)
                    {
                        return dictionary;
                    }
                }
            }

            return null;
        }

        private AddOrderJsonBody BuildAddOrderJsonBody(OrderType orderType, decimal price, decimal amount)
        {
            AddOrderJsonBody returnBody = new AddOrderJsonBody();

            returnBody.amount = amount.ToString(CultureInfo.InvariantCulture);
            returnBody.price = price.ToString(CultureInfo.InvariantCulture);
            returnBody.currency = "XBT";
            returnBody.instrument = _btcFiatPairSymbol;
            returnBody.side = orderType.ToString();
            returnBody.type = "limit";

            return returnBody;
        }

        private void InitializeItBit()
        {
            BaseUrl = "https://api.itbit.com/v1/";
            OrderBookPath = "markets/" + _btcFiatPairSymbol + "/order_book";
            AccountBalanceInfoPath = "wallets";
            OrderQueryPath = "wallets/" + _walletId + "/orders/";
            DeleteOrderPath = "wallets/" + _walletId + "/orders/";

            MinimumBitcoinOrderAmount = 0.0001m;
        }

        #endregion

        #region Nested Classes

        private class ItBitRequest : RestRequest
        {
            private string _fullUrl;
            private string _apiKey;
            private string _apiSecret;
            private readonly long _timestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            private readonly Method _requestType;
            //Note, I'm not really sure what the deal with the nonce is; just set it to 1, that works.
            private readonly long _nonce = 1;

            //Authenticated Post
            public ItBitRequest(string baseUrl, string resource, string apiKey, string apiSecret, object requestBody)
                : base(resource, Method.POST)
            {
                InitializeAuthenticatedRequest(baseUrl, resource, apiKey, apiSecret);
                _requestType = Method.POST;
                RequestFormat = DataFormat.Json;

                AddBody(requestBody);
            }

            //Authenticated get
            public ItBitRequest(string baseUrl, string resource, string apiKey, string apiSecret, Method method = Method.GET)
                : base(resource, method)
            {
                InitializeAuthenticatedRequest(baseUrl, resource, apiKey, apiSecret);
                _requestType = method;
            }

            //Non-Authenticated get
            public ItBitRequest(string resource)
                : base(resource, Method.GET)
            {
            }

            /// <summary>
            /// Adds the signature parameter to the header of this web request. This MUST be called after (and only after)
            /// all parameters have been added to this request.
            /// </summary>
            public void AddSignatureHeader()
            {
                string message;
                bool firstParameter = true;

                //Add all the parameters to the full url to build the query string, with the exception of header parameters.
                foreach (Parameter parameter in Parameters)
                {
                    if (parameter.Type != ParameterType.HttpHeader && parameter.Type != ParameterType.RequestBody)
                    {
                        if (firstParameter)
                        {
                            _fullUrl += "?";
                        }
                        else
                        {
                            _fullUrl += "&";
                        }

                        _fullUrl += (parameter.Name + "=" + parameter.Value);
                        firstParameter = false;
                    }
                }

                //Build a json formatted string.
                message = _nonce + "[\"" + _requestType + "\"," + "\"" + _fullUrl + "\"," + "\"" + GetRequestFormattedBodyString() + "\"," + "\"" + _nonce + "\"," + "\"" + _timestamp + "\"]";

                AddHeader("Authorization", _apiKey + ":" + CreateSignature(message));
            }

            private void InitializeAuthenticatedRequest(string baseUrl, string resource, string apiKey, string apiSecret)
            {
                _apiKey = apiKey;
                _apiSecret = apiSecret;

                _fullUrl = baseUrl + resource;

                AddHeader("X-Auth-Timestamp", _timestamp.ToString());
                AddHeader("X-Auth-Nonce", _nonce.ToString());
            }

            private string CreateSignature(string message)
            {
                var hash256Bytes = sha256_hash(message);
                var urlBytes = StringToByteArray(_fullUrl);
                var messageAndUrlByteArray = ByteArrayHelper.CombineByteArrays(urlBytes, hash256Bytes);
                var signature = getHash(StringToByteArray(_apiSecret), messageAndUrlByteArray);
                var base64EncodedSignature = Convert.ToBase64String(signature);
                return base64EncodedSignature;
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

                //Need to escape the '\' as well as the '"'; this replacement statement does such a thing
                requestBodyString = requestBodyString.Replace("\"", "\\\"");

                return requestBodyString;
            }

            private byte[] sha256_hash(String value)
            {
                using (SHA256 hash = SHA256.Create())
                {
                    Encoding enc = Encoding.UTF8;

                    Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                    return result;
                }
            }

            private byte[] getHash(byte[] keyByte, byte[] messageBytes)
            {
                using (var hmacsha512 = new HMACSHA512(keyByte))
                {
                    Byte[] result = hmacsha512.ComputeHash(messageBytes);
                    return result;
                }
            }

            private static byte[] StringToByteArray(string str)
            {
                return Encoding.UTF8.GetBytes(str);
            }
        }

        private class ItBitApiInfo : ApiInfo
        {
            public string UserId;
        }

        private struct AddOrderJsonBody
        {
            public string type;
            public string amount;
            public string price;
            public string side;
            public string instrument;
            public string currency;
        }

        private struct TransferJsonBody
        {
            public string currency;
            public string amount;
            public string address;
        }

        #endregion
    }
}
