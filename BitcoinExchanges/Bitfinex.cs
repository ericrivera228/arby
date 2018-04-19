using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using ArbitrationUtilities.EnumerationObjects;
using BitcoinExchanges.ExchangeObjects;
using CommonFunctions;
using RestSharp;

namespace BitcoinExchanges
{
    public class Bitfinex : BaseExchange
    {
        #region Class variables and Properties
        private const string BTC_USD_PAIR_SYMBOL = "BTCUSD";
        private string _tradeFeePath;
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

        public Bitfinex() : base("Bitfinex", CurrencyType.Fiat, 8)
        {
            //No fiat type provided; default is USD
            _btcFiatPairSymbol = BTC_USD_PAIR_SYMBOL;

            InitializeBitfinex();
        }

        public Bitfinex(FiatType fiatTypeToUse = FiatType.Usd) : base("Bitfinex", CurrencyType.Fiat, 8, true, fiatTypeToUse)
        {
            if (fiatTypeToUse != FiatType.Usd)
            {
                throw new Exception(Name + " does not support " + fiatTypeToUse);
            }

            //Only supported fiat type is usd
            _btcFiatPairSymbol = BTC_USD_PAIR_SYMBOL;

            InitializeBitfinex();
        }

        #endregion

        #region Public Methods

        public override void UpdateBalances()
        {
            bool btcFound = false;
            bool fiatFound = false;

            BitfinexRequest balanceInfoRequest = new BitfinexRequest(AccountBalanceInfoPath, _apiInfo);
            balanceInfoRequest.AddSignatureHeader();

            Dictionary<string, dynamic>[] response = ApiPost_Array(balanceInfoRequest);

            //Loop through each of the dictionaries in the response; looking for the btc and usd exchange wallets
            foreach (Dictionary<string, dynamic> walletDictionary in response)
            {
                if (String.Equals((string)GetValueFromResponseResult(walletDictionary, "type"), "exchange", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (String.Equals((string) GetValueFromResponseResult(walletDictionary, "currency"), "usd", StringComparison.InvariantCultureIgnoreCase))
                    {
                        fiatFound = true;

                        TotalFiat = TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(walletDictionary, "amount"));
                        AvailableFiat = TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(walletDictionary, "available"));
                    }

                    else if (String.Equals((string)GetValueFromResponseResult(walletDictionary, "currency"), "btc", StringComparison.InvariantCultureIgnoreCase))
                    {
                        btcFound = true;

                        TotalBtc = TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(walletDictionary, "amount"));
                        AvailableBtc = TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(walletDictionary, "available"));
                    }
                }
            }

            //If there wasn't any btc or usd, Bitfinex does not return a dictionary of that currency. So if the currency dictionaries were not found, set values to 0.
            if (!btcFound)
            {
                AvailableBtc = 0.0m;
                TotalBtc = 0.0m;
            }

            if (!fiatFound)
            {
                AvailableFiat = 0.0m;
                TotalFiat = 0.0m; 
            }
        }

        public override void SetTradeFee()
        {
            BitfinexRequest tradeFeeRequest = new BitfinexRequest(_tradeFeePath, ApiInfo);
            tradeFeeRequest.AddSignatureHeader();

            Dictionary<string, dynamic> response = ApiPost(tradeFeeRequest);
            TradeFee = TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(response, "taker_fees"));
        }

        public override void UpdateOrderBook(int? maxSize = null)
        {
            BitfinexRequest orderBookRequest = new BitfinexRequest(OrderBookPath);

            //If max size was given, add limit parameters to the request
            if (maxSize != null)
            {
                orderBookRequest.AddParameter("limit_bids", maxSize);
                orderBookRequest.AddParameter("limit_asks", maxSize);
            }


            Dictionary<string, dynamic> response = ApiPost(orderBookRequest);

            if (!response.ContainsKey("bids") || !response.ContainsKey("asks"))
            {
                throw new Exception("Could not update order book for " + Name + ", 'asks' or 'bids' object was not in the response.");
            }

            BuildOrderBook(response, "amount", "price", maxSize);
        }

        public override Dictionary<string, dynamic> GetOrderInformation(string orderId)
        {
            BitfinexRequest orderStatusRequest = new BitfinexRequest(OrderQueryPath, _apiInfo);
            orderStatusRequest.AddPayloadParameter("order_id", orderId);
            orderStatusRequest.AddSignatureHeader();

            return ApiPost(orderStatusRequest);
        }

        public override bool IsOrderFulfilled(string orderId)
        {
            try
            {
                //If the order is still open, this query will successfully run. Otherwise, if the order is fulfilled, it will throw an error
                Dictionary<string, dynamic> order = GetOrderInformation(orderId);

                return false;
            }
            catch (Exception e)
            {
                //If the order was not found, then it was fulfilled.
                if (e.Message.Contains("No such order found"))
                {
                    return true;
                }

                //Some other unknown error.
                throw;
            }
        }

        public override void DeleteOrder(string orderId)
        {
            BitfinexRequest deleteOrderRequest = new BitfinexRequest(DeleteOrderPath, _apiInfo);
            deleteOrderRequest.AddPayloadParameter("order_id", orderId);
            deleteOrderRequest.AddSignatureHeader();

            Dictionary<string, dynamic> response = ApiPost(deleteOrderRequest);
        }

        public override List<Dictionary<string, dynamic>> GetAllOpenOrders()
        {
            BitfinexRequest openOrderRequest = new BitfinexRequest(OpenOrderPath, _apiInfo);
            openOrderRequest.AddSignatureHeader();

            Dictionary<string, dynamic>[] response = ApiPost_Array(openOrderRequest);

            return response.ToList();
        }

        public override decimal RoundTotalCost(decimal costToRound)
        {
            return Math.Round(costToRound, 8);
        }

        #endregion

        #region Protected Methods

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
            //Bitfinex does not have any unique settings that need to be set
        }

        #endregion

        #region Private Methods

        private string ExecuteOrder(decimal amount, decimal price, OrderType orderType)
        {
            BitfinexRequest addOrderRequest = new BitfinexRequest(_addOrderPath, _apiInfo);
            addOrderRequest.AddPayloadParameter("symbol", _btcFiatPairSymbol);
            addOrderRequest.AddPayloadParameter("amount", amount.ToString());
            addOrderRequest.AddPayloadParameter("price", price.ToString("F"));
            addOrderRequest.AddPayloadParameter("exchange", "bitfinex");
            addOrderRequest.AddPayloadParameter("side", orderType.ToString().ToLower());
            addOrderRequest.AddPayloadParameter("type", "exchange limit");

            addOrderRequest.AddSignatureHeader();

            Dictionary<string, dynamic> orderResponse = ApiPost(addOrderRequest);

            return ((int)GetValueFromResponseResult(orderResponse, "id")).ToString();
        }

        private void InitializeBitfinex()
        {
            //Set the api path variables
            BaseUrl = "https://api.bitfinex.com/v1";
            AccountBalanceInfoPath = "balances";
            OrderBookPath = "book/" + _btcFiatPairSymbol;
            DeleteOrderPath = "order/cancel";
            OrderQueryPath = "order/status";
            OpenOrderPath = "orders";
            
            //Api path specific to Bitfinex
            _tradeFeePath = "account_infos";
            _addOrderPath = "order/new";

            SetTradeFee();
        }

        #endregion

        #region Inner Classes

        private class BitfinexRequest : RestRequest, IExchangeRequest
        {
            private readonly ApiInfo _apiInfo;
            private decimal _nonce;
            private Dictionary<string, string> payload = new Dictionary<string, string>();
            private DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            public BitfinexRequest(string resource)
                : base(resource, Method.GET)
            {
                _nonce = GetNonce();
            }

            public BitfinexRequest(string resource, ApiInfo apiInfo)
                : base(resource, Method.POST)
            {
                _apiInfo = apiInfo;
                _nonce = GetNonce();
            }

            /// <summary>
            /// Adds the signature parameter to the header of this web request. This MUST be called after (and only after)
            /// all parameters have been added to this request.
            /// </summary>
            public void AddSignatureHeader()
            {
                string payload = "{" + BuildPayloadString() + "\"request\":\"/v1/" + Resource + "\",\"nonce\":\"" + _nonce + "\"}";
                var payloadEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));

                AddHeader("X-BFX-APIKEY", _apiInfo.Key);
                AddHeader("X-BFX-PAYLOAD", payloadEncoded);
                AddHeader("X-BFX-SIGNATURE", GetHexHashSignature(payloadEncoded));
            }

            public void AddPayloadParameter(string name, string value)
            {
                payload.Add(name, value);
            }

            private string BuildPayloadString()
            {
                StringBuilder payloadMessage = new StringBuilder();

                foreach (KeyValuePair<string, string> payloadItem in payload)
                {
                    //Need to include or not include '"' depending on if the value is an integer
                    try
                    {
                        TypeConversion.ParseStringToIntegerStrict(payloadItem.Value);

                        //parse succeeded; value must be an integer
                        payloadMessage.Append("\"" + payloadItem.Key + "\":" + payloadItem.Value + ",");
                    }
                    catch
                    {
                        //parse faile; value must be a string
                        payloadMessage.Append("\"" + payloadItem.Key + "\":\"" + payloadItem.Value + "\",");
                    }
                }

                return payloadMessage.ToString();
            }

            private decimal GetNonce()
            {
                return (decimal)(DateTime.UtcNow - _unixEpoch).TotalSeconds;
            }

            private string GetHexHashSignature(string payload)
            {
                HMACSHA384 hmac = new HMACSHA384(Encoding.UTF8.GetBytes(_apiInfo.Secret));
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        #endregion
    }
}
