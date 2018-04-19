using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using ArbitrationUtilities.EnumerationObjects;
using BitcoinExchanges.ExchangeObjects;
using CommonFunctions;
using RestSharp;

namespace BitcoinExchanges
{
    public class Kraken : BaseExchange
    {
        #region Class Variables and Properties

        private const string XBT_EUR_PAIR_SYMBOL = "XXBTZEUR";
        private const string XBT_USD_PAIR_SYMBOL = "XXBTZUSD";
        private const string BTC_SYMBOL = "XXBT";
        private const string API_VERSION = "0";

        //The btc\fiat pair name symbol that is being used
        private readonly string _btcFiatPairSymbol;

        private readonly string _addOrderPath = "private/AddOrder";
        private readonly string _tradeFeeInfoPath = "private/TradeVolume";
        private readonly string _transferPath = "private/Withdraw";
        
        private KrakenApiInfo _apiInfo = new KrakenApiInfo(){Version = API_VERSION};

        protected override ApiInfo ApiInfo
        {
            get
            {
                if (_apiInfo == null)
                {
                    _apiInfo = new KrakenApiInfo();
                    _apiInfo.Version = API_VERSION;
                }

                return _apiInfo;
            }
        }
        #endregion

        #region Constructors

        public Kraken() : base("Kraken", CurrencyType.Fiat, 8)
        {
            //No fiat type provided; default is USD
            _btcFiatPairSymbol = XBT_USD_PAIR_SYMBOL;

            InitializeKraken();
        }

        public Kraken(FiatType fiatTypeToUse) : base("Kraken", CurrencyType.Fiat, 8, true, fiatTypeToUse)
        {
            switch (fiatTypeToUse)
            {
                case FiatType.Eur:
                    _btcFiatPairSymbol = XBT_EUR_PAIR_SYMBOL;
                    break;

                case FiatType.Usd:
                    _btcFiatPairSymbol = XBT_USD_PAIR_SYMBOL;
                    break;
            }

            InitializeKraken();
        }

        #endregion

        #region Public Methods

        public override void UpdateBalances()
        {
            KrakenRequest request = new KrakenRequest(AccountBalanceInfoPath, _apiInfo);
            request.AddSignatureHeader();
            Dictionary<string, dynamic> response = ApiPost(request);

            //Get the BTC value from the response, convert it to a decimal and sign it to this exchange.
            TotalBtc = TypeConversion.ParseStringToDecimalLoose((string)GetValueFromResponseResult(response, "XXBT", true));

            switch (FiatTypeToUse)
            {
                case FiatType.Eur:
                    TotalFiat = TypeConversion.ParseStringToDecimalLoose((string)GetValueFromResponseResult(response, "ZEUR", true));
                    break;

                case FiatType.Usd:
                    TotalFiat = TypeConversion.ParseStringToDecimalLoose((string)GetValueFromResponseResult(response, "ZUSD", true));
                    break;
            }
           
            //The above returned the total amounts for fiat and btc. Need to get the open orders, and calculate the amount of currencies tied up
            CalculateAvailableCurrenciesFromOpenOrders();
        }

        public override Dictionary<string, dynamic> GetOrderInformation(string orderId)
        {
            KrakenRequest request = new KrakenRequest(OrderQueryPath, _apiInfo);
            request.AddParameter("txid", orderId);
            request.AddSignatureHeader();

            Dictionary<string, dynamic> response = ApiPost(request);
            response = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, orderId);

            return response;
        }

        public override bool IsOrderFulfilled(string orderId)
        {
            string orderStatus = (string)GetValueFromResponseResult(GetOrderInformation(orderId), "status");

            if (orderStatus.Equals("Closed", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public override void DeleteOrder(string orderId)
        {
            //Build the request
            KrakenRequest deleteOrderRequest = new KrakenRequest(DeleteOrderPath, _apiInfo);
            deleteOrderRequest.AddParameter("txid", orderId);
            deleteOrderRequest.AddSignatureHeader();

            //Post the request
            Dictionary<string, dynamic> response = ApiPost(deleteOrderRequest);

            //Kraken should return a response indicating that exactly 1 order was deleted. If more than 1 was deleted, something went wrong.
            if ((int)GetValueFromResponseResult(response, "count") > 1)
            {
                throw new Exception("Delete for order " + orderId + " at " + Name + " resulted in more than 1 order being deleted.");
            }
        }

        public override List<Dictionary<string, dynamic>> GetAllOpenOrders()
        {
            List<Dictionary<string, dynamic>> returnList = new List<Dictionary<string, dynamic>>();

            KrakenRequest request = new KrakenRequest(OpenOrderPath, _apiInfo);
            request.AddSignatureHeader();
            Dictionary<string, dynamic> response = ApiPost(request);
            response = (Dictionary<string, dynamic>) GetValueFromResponseResult(response, "open");
            
            //Loop through all the orders, add the id as one of the properties and build the return list
            foreach (var order in response)
            {
                order.Value["order-id"] = order.Key;
                returnList.Add(order.Value);
            }

            if (returnList.Count <= 0)
            {
                return null;
            }

            return returnList;
        }

        /// <summary>
        /// Kraken rounds their total costs to 4 decimal places.
        /// </summary>
        /// <param name="costToRound"></param>
        /// <returns></returns>
        public override decimal RoundTotalCost(decimal costToRound)
        {
            return Math.Round(costToRound, 5);
        }

        public override void SetTradeFee()
        {
            KrakenRequest request = new KrakenRequest(_tradeFeeInfoPath, _apiInfo);
            request.AddParameter("pair", _btcFiatPairSymbol);
            request.AddSignatureHeader();

            Dictionary<string, dynamic> response = ApiPost(request);

            //The trade fee value is buried deep in the response; par down response until we get to the bit we want
            response = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "fees");
            response = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, _btcFiatPairSymbol);

            TradeFee = TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(response, "fee"));
        }

        public override void UpdateOrderBook(int? maxSize = null)
        {
            KrakenRequest request = new KrakenRequest(OrderBookPath);
            request.AddParameter("pair", _btcFiatPairSymbol);

            if (maxSize != null)
            {
                request.AddParameter("count", maxSize.Value);
            }

            Dictionary<string, dynamic> response = ApiPost(request);
            response = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, _btcFiatPairSymbol);
            BuildOrderBook(response, 1, 0, maxSize);
        }

        #endregion

        #region Protected Methods

        protected override string SellInternal(decimal amount, decimal price)
        {
            return ExecuteOrder(amount, price, OrderType.Sell);
        }

        protected override string BuyInternal(decimal amount, decimal price)
        {
            return ExecuteOrder(amount, price, OrderType.Buy);
        }

        protected override string TransferInternal(decimal amount, string address)
        {
            KrakenRequest transferRequest = new KrakenRequest(_transferPath, _apiInfo);
            transferRequest.AddParameter("asset", BTC_SYMBOL);
            transferRequest.AddParameter("key", address);
            transferRequest.AddParameter("amount", amount);
            transferRequest.AddSignatureHeader();

            Dictionary<string, dynamic> response = ApiPost(transferRequest);

            return (string)GetValueFromResponseResult(response, "refid");
        }

        /// <summary>
        /// Same as base ApiPost; just unwraps the 'result' portion of the response. If there are any errors along the way, a WebException
        /// is thrown. 
        /// </summary>
        /// <param name="request">Request to post to the api.</param>
        /// <param name="baseUrl"></param>
        /// <returns>The 'result' portion of the response from Kraken (aka, the important part with the needed information).</returns>
        protected override Dictionary<string, dynamic> ApiPost(RestRequest request, string baseUrl = null)
        {
            Dictionary<string, dynamic> response = base.ApiPost(request);

            //Make sure the 'result' portion of the response exists, and returned just part. (The 'error' has was already checked by ApiPost).
            if (!response.ContainsKey("result"))
            {
                throw new WebException("There was a problem with the " + Name + " api. 'Result' object was not part of the post response.");
            }

            return response["result"];
        }

        protected override void CheckResponseForErrors(Dictionary<string, dynamic> responseContent)
        {
            if (responseContent.ContainsKey("error"))
            {
                ArrayList errorList = (ArrayList)responseContent["error"];

                if (errorList.Count > 0)
                {
                    string errorMessage = "There was a problem connecting to the " + Name + " api: ";

                    for (int counter = 0; counter < errorList.Count; counter++)
                    {
                        errorMessage += errorList[counter];

                        //Add a delimiter on all but the last error.
                        if (counter < errorList.Count - 1)
                        {
                            errorMessage += " | ";
                        }
                    }

                    throw new WebException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Wrapper for getting a value from a Kraken response. It throws an error if the desired key does not exist in the given 'result' 
        /// content.
        /// </summary>
        /// <param name="resultContent">The 'result' portion of the response from a post to the kraken API.</param>
        /// <param name="key">Key of the value to look up in the given content.</param>
        /// <param name="keyAbsenceAllowed">Optional parameter (default is false). If true, returns null if the given key does not exist 
        ///     instead of throwing an error.</param>
        /// <returns>Value of given key as an object</returns>
        protected override object GetValueFromResponseResult(Dictionary<string, dynamic> resultContent, string key, bool keyAbsenceAllowed = false)
        {
            //If there isn't anything in the given content, return an empty string as whatever key is being accessed is 0 or doens't have a value
            if (resultContent.Count <= 0)
            {
                return "";
            }

            if (!resultContent.ContainsKey(key))
            {
                if (keyAbsenceAllowed == false)
                {
                    throw new WebException("There was a problem with the " + Name + " api. '" + key + "' object was not part of the web response.");
                }
                else
                {
                    return "";
                }
            }

            return resultContent[key];
        }

        protected override void SetUniqueExchangeSettings(NameValueCollection configSettings)
        {
            //Nothing to do here
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// With Kraken, buying and selling is the same Api call. So both SellInternal and BuyInternal point to this.
        /// </summary>
        /// <param name="amount">Amount of btc to be bought/sold.</param>
        /// <param name="price">Price to set for the order.</param>
        /// <param name="orderType">Can be either be "buy" or "sell".</param>
        /// <returns>String representation of the executed order.</returns>
        private string ExecuteOrder(decimal amount, decimal price, OrderType orderType)
        {
            KrakenRequest sellRequest = new KrakenRequest(_addOrderPath, _apiInfo);
            sellRequest.AddParameter("pair", _btcFiatPairSymbol);
            sellRequest.AddParameter("type", orderType.ToString().ToLower());     //Important note: Kraken api requires that the order type be lower case (else there will be an error), thus the ToLower().
            sellRequest.AddParameter("ordertype", "limit");
            sellRequest.AddParameter("price", price);
            sellRequest.AddParameter("volume", amount);
            sellRequest.AddSignatureHeader();

            Dictionary<string, dynamic> sellResponse = ApiPost(sellRequest);
            ArrayList transactionIdList = (ArrayList)GetValueFromResponseResult(sellResponse, "txid");

            return StringManipulation.CreateDelimitedStringFromArrayList(transactionIdList, '|');
        }

        /// <summary>
        /// The Kraken api does not provide a way to get only available currencies; it only returns total currencies, including those tied up in open orders. This method gets the currently open orders,
        /// and calculates the available btc and fiat from those open orders.
        /// 
        /// This method assumes that the 'TotalBtc' and 'TotalFiat' class variables have already been updated. If there aren't any open orders, 'AvailableBtc' and 'AvailableFiat' are set equal to
        /// 'TotalBtc' and 'TotalFiat', respectively.
        /// </summary>
        private void CalculateAvailableCurrenciesFromOpenOrders()
        {
            List<Dictionary<string, dynamic>> openOrders = GetAllOpenOrders();

            //Initialize the available amounts to the be total amounts
            AvailableBtc = TotalBtc;
            AvailableFiat = TotalFiat;

            if (openOrders != null)
            {
                foreach (var order in openOrders)
                {
                    decimal orderAmount = TypeConversion.ParseStringToDecimalLoose((string)GetValueFromResponseResult(order, "vol"));

                    //Order might be partially filled; calculate how much actual volume is left
                    orderAmount = orderAmount - TypeConversion.ParseStringToDecimalLoose((string)GetValueFromResponseResult(order, "vol_exec"));

                    Dictionary<string, dynamic> orderDescription = (Dictionary<string, dynamic>)GetValueFromResponseResult(order, "descr");

                    //Determine if this is a buy or sell
                    string orderType = (string)GetValueFromResponseResult(orderDescription, "type");

                    if (String.Equals(orderType, OrderType.Buy.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        //This is a buy; need to calculate the buy cost and subtract it from the available fiat
                        decimal orderPrice = TypeConversion.ParseStringToDecimalLoose((string)GetValueFromResponseResult(orderDescription, "price"));

                        //Calculate the buy cost
                        decimal totalBuyCost = Decimal.Multiply(orderPrice, orderAmount);

                        //Now apply the fee; this also does the necessary rounding
                        totalBuyCost = ApplyFeeToBuyCost(totalBuyCost);

                        //Subtract total cost from available 
                        AvailableFiat = AvailableFiat - totalBuyCost;
                    }
                    else
                    {
                        //This is a sell, subtract the amount of btc from the total btc
                        AvailableBtc = AvailableBtc - orderAmount;
                    }
                }
            }
        }

        public void InitializeKraken()
        {
            BaseUrl = "https://api.kraken.com/" + API_VERSION + "/";
            AccountBalanceInfoPath = "private/Balance";
            OrderBookPath = "public/Depth";
            OrderQueryPath = "private/QueryOrders";
            DeleteOrderPath = "private/CancelOrder";
            OpenOrderPath = "private/OpenOrders";

            //Set the trade fee; Kraken is special in that it has to calculate available currencies from open orders, and that requires knowing the  trade fee.
            //Thus, set it here to ensure it always has a value.
            SetTradeFee();
        }

        #endregion

        #region Nested Classes

        private class KrakenRequest : RestRequest, IExchangeRequest
        {
            private readonly long _nonce = DateTime.Now.Ticks;
            private readonly KrakenApiInfo _apiInfo;

            /// <summary>
            /// Constructor for a private request to Kraken. Builds a requst that is ready to be sent to Kraken.
            /// </summary>
            /// <param name="resource">Which part of the api you are going to hit. i.e. 'private/Balance'</param>
            /// <param name="apiInfo">ApiInfoStruct containing the key, secret, and version of Kraken api.</param>
            public KrakenRequest(string resource, KrakenApiInfo apiInfo)
                : base(resource, Method.POST)
            {
                _apiInfo = apiInfo;

                AddHeader("API-Key", _apiInfo.Key);
                AddHeader("Content-Type", "application/x-www-form-urlencoded");
                AddParameter("nonce", _nonce);
            }

            /// <summary>
            /// Constructor for a public request to Kraken. Builds a requst that is ready to be sent to Kraken.
            /// </summary>
            /// <param name="resource">Which part of the api you are going to hit. i.e. 'public/Depth'</param>
            public KrakenRequest(string resource)
                : base(resource, Method.GET)
            {
            }

            /// <summary>
            /// Adds the signature parameter to the header of this web request. This MUST be called after (and only after)
            /// all parameters have been added to this request.
            /// </summary>
            public void AddSignatureHeader()
            {
                StringBuilder signatureString = new StringBuilder(_nonce + Convert.ToChar(0) + "nonce=" + _nonce);

                //Add all the parameters to the signature string, with the exception of header parameters and the nonce (which
                //was already added).
                foreach (Parameter parameter in Parameters)
                {
                    if (parameter.Type != ParameterType.HttpHeader && parameter.Name != "nonce")
                    {
                        signatureString.Append("&" + parameter.Name + "=" + parameter.Value);
                    }
                }

                AddHeader("API-Sign", CreateSignature(signatureString.ToString()));
            }

            private string CreateSignature(string signatureString)
            {
                byte[] base64DecodedSecred = Convert.FromBase64String(_apiInfo.Secret);
                string path = "/" + _apiInfo.Version + "/" + Resource;

                var pathBytes = Encoding.UTF8.GetBytes(path);
                var hash256Bytes = sha256_hash(signatureString);
                var z = new byte[pathBytes.Count() + hash256Bytes.Count()];
                pathBytes.CopyTo(z, 0);
                hash256Bytes.CopyTo(z, pathBytes.Count());

                var signature = getHash(base64DecodedSecred, z);
                return Convert.ToBase64String(signature);
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
        }

        private class KrakenApiInfo : ApiInfo
        {
            public string Version;
        }

        #endregion
    }
}
