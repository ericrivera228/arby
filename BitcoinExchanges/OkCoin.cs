using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ArbitrationUtilities.EnumerationObjects;
using BitcoinExchanges.ExchangeObjects;
using CommonFunctions;
using RestSharp;

namespace BitcoinExchanges
{
    public class OkCoin : BaseExchange
    {
        private const string BTC_USD_PAIR_SYMBOL = "btc_usd";

        #region Class Properties and variables

        //The btc\fiat pair name symbol that is being used
        private readonly string _btcFiatPairSymbol;
        private string _transferPath;
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

        public OkCoin() : base("OkCoin", CurrencyType.Bitcoin, 3)
        {
            //No fiat type provided; default is USD
            _btcFiatPairSymbol = BTC_USD_PAIR_SYMBOL;

            InitializeOkCoin();
        }

        public OkCoin(FiatType fiatTypeToUse) : base("OkCoin", CurrencyType.Bitcoin, 3, true, fiatTypeToUse)
        {
            if (fiatTypeToUse != FiatType.Usd)
            {
                throw new Exception(Name + " does not support " + fiatTypeToUse);
            }

            //Only supported fiat type is usd
            _btcFiatPairSymbol = BTC_USD_PAIR_SYMBOL;

            InitializeOkCoin();
        }

        #endregion

        #region Public Methods

        public override void UpdateOrderBook(int? maxSize = null)
        {
            OkCoinRequest orderBookRequest = new OkCoinRequest(OrderBookPath);

            //If a max size was provided, add it as parameter
            if (maxSize != null)
            {
                orderBookRequest.AddParameter("size", maxSize);    
            }
            
            Dictionary<string, dynamic> response = ApiPost(orderBookRequest);

            if (!response.ContainsKey("bids") || !response.ContainsKey("asks"))
            {
                throw new Exception("Could not update order book for " + Name + ", 'asks' or 'bids' object was not in the response.");
            }

            //OkCoin returns the asks list in the wrong order (with best ask being at the end of the list), so reverse it
            ((ArrayList)response["asks"]).Reverse();

            BuildOrderBook(response, 1, 0, maxSize);
        }

        public override Dictionary<string, dynamic> GetOrderInformation(string orderId)
        {
            OkCoinRequest orderInfoRequest = new OkCoinRequest(OrderQueryPath, ApiInfo);

            orderInfoRequest.AddParameter("api_key", _apiInfo.Key);
            orderInfoRequest.AddParameter("order_id", orderId);
            orderInfoRequest.AddParameter("symbol", _btcFiatPairSymbol);
            orderInfoRequest.AddSignatureHeader();

            Dictionary<string, dynamic> response = ApiPost(orderInfoRequest);
            ArrayList orders = (ArrayList)GetValueFromResponseResult(response, "orders");

            if (orders.Count <= 0)
            {
                throw new Exception("Could not find order with id '" + orderId + ".");
            }

            //The array list should only have one order, which the given id. So just return the first one in the array.
            return (Dictionary<string, dynamic>)orders[0];
        }

        public override bool IsOrderFulfilled(string orderId)
        {
            Dictionary<string, dynamic> orderInfo = GetOrderInformation(orderId);

            int status = orderInfo["status"];

            if (status != 2)
            {
                return false;
            }

            return true;
        }

        public override List<Dictionary<string, dynamic>> GetAllOpenOrders()
        {
            OkCoinRequest orderInfoRequest = new OkCoinRequest(OpenOrderPath, ApiInfo);

            orderInfoRequest.AddParameter("api_key", _apiInfo.Key);
            orderInfoRequest.AddParameter("order_id", "-1");
            orderInfoRequest.AddParameter("symbol", _btcFiatPairSymbol);
            orderInfoRequest.AddSignatureHeader();

            Dictionary<string, dynamic> response = ApiPost(orderInfoRequest);
            ArrayList orders = (ArrayList)GetValueFromResponseResult(response, "orders");

            if (orders.Count <= 0)
            {
                return null;
            }

            return orders.Cast<Dictionary<string, dynamic>>().ToList();
        }

        public override void DeleteOrder(string orderId)
        {
            OkCoinRequest deleteOrderRequest = new OkCoinRequest(DeleteOrderPath, _apiInfo);
            deleteOrderRequest.AddParameter("api_key", _apiInfo.Key);
            deleteOrderRequest.AddParameter("order_id", orderId);
            deleteOrderRequest.AddParameter("symbol", _btcFiatPairSymbol);
            deleteOrderRequest.AddSignatureHeader();

            ApiPost(deleteOrderRequest);
        }

        public override decimal RoundTotalCost(decimal costToRound)
        {
            return MathHelpers.FloorRound(costToRound, 4);
        }

        public override decimal ApplyFeeToSellCost(decimal sellCost)
        {
            //Round the cost
            sellCost = RoundTotalCost(sellCost);

            //Take into account the fee
            sellCost = RoundTotalCost(Decimal.Multiply(Decimal.Subtract(1.0m, TradeFeeAsDecimal), sellCost));

            //Now round again
            return RoundTotalCost(sellCost);
        }

        public override void UpdateBalances()
        {
            OkCoinRequest request = new OkCoinRequest(AccountBalanceInfoPath, _apiInfo);
            request.AddParameter("api_key", _apiInfo.Key);
            request.AddSignatureHeader();

            Dictionary<string, dynamic> response = ApiPost(request);
            response = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "info");
            response = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "funds");

            Dictionary<string, dynamic> freeAssets = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "free");
            Dictionary<string, dynamic> frozenAssets = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "freezed");
            
            AvailableFiat = TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(freeAssets, FiatTypeToUse.ToString().ToLower()));
            AvailableBtc = TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(freeAssets, "btc"));

            //Calculate total balances by adding the 'free' and 'frozen' assets 
            TotalFiat = AvailableFiat + TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(frozenAssets, FiatTypeToUse.ToString().ToLower()));
            TotalBtc = AvailableBtc + TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(frozenAssets, "btc"));
        }

        public override void SetTradeFee()
        {
            //Trade fee for OkCoin has to be set manually; can't get it from the api.
            NameValueCollection configSettings = GetConfigSettings("ExchangeLoginInformation/" + Name);
            TradeFee = TypeConversion.ParseStringToDecimalStrict(configSettings["TradeFee"]);
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
            if (responseContent.ContainsKey("error_code"))
            {
                string message = "There was a problem connecting to the " + Name + " api: Error code - ";
                message += (int)(GetValueFromResponseResult(responseContent, "error_code"));

                throw new Exception(StringManipulation.AppendPeriodIfNecessary(message));
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
            //Have to set trade fee for OkCoin manually; can't get it from the api.
            TradeFee = TypeConversion.ParseStringToDecimalStrict(configSettings["TradeFee"]);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// With Okcoin, buying and selling is the same Api call. So both SellInternal and BuyInternal point to this.
        /// </summary>
        /// <param name="amount">Amount of btc to be bought/sold.</param>
        /// <param name="price">Price to set for the order.</param>
        /// <param name="orderType">Can be either be "buy" or "sell".</param>
        /// <returns>String representation of the executed order.</returns>
        private string ExecuteOrder(decimal amount, decimal price, OrderType orderType)
        {
            OkCoinRequest buyRequest = new OkCoinRequest(_addOrderPath, _apiInfo);
            buyRequest.AddParameter("amount", amount);
            buyRequest.AddParameter("api_key", ApiInfo.Key);
            buyRequest.AddParameter("price", price);
            buyRequest.AddParameter("symbol", _btcFiatPairSymbol);
            buyRequest.AddParameter("type", orderType.ToString().ToLower());
                        
            buyRequest.AddSignatureHeader();

            //Make the api call
            Dictionary<string, dynamic> response = ApiPost(buyRequest);

            //Get order id from the response
            return ((int)GetValueFromResponseResult(response, "order_id")).ToString();
        }

        private void InitializeOkCoin()
        {
            //Set the api path variables
            BaseUrl = "https://www.okcoin.com/api/v1/";
            AccountBalanceInfoPath = "userinfo.do";
            OrderBookPath = "depth.do?symbol=" + BTC_USD_PAIR_SYMBOL;
            DeleteOrderPath = "cancel_order.do";
            OrderQueryPath = "order_info.do";
            OpenOrderPath = "order_info.do";
            _transferPath = "withdraw.do";
            _addOrderPath = "trade.do";

            MinimumBitcoinOrderAmount = 0.01m;
        }

        #endregion

        #region Nested Classes

        private class OkCoinRequest : RestRequest, IExchangeRequest
        {
            private readonly ApiInfo _apiInfo;

            public OkCoinRequest(string resource, ApiInfo apiInfo)
                : base(resource, Method.POST)
            {
                _apiInfo = apiInfo;
            }

            public OkCoinRequest(string resource)
                : base(resource, Method.GET)
            {
            }

            /// <summary>
            /// Adds the signature parameter to the header of this web request. This MUST be called after (and only after)
            /// all parameters have been added to this request.
            /// </summary>
            public void AddSignatureHeader()
            {
                String mysign = "";
                String prestr = createLinkString();
                prestr = prestr + "&secret_key=" + _apiInfo.Secret; // 把拼接后的字符串再与安全校验码连接起来
                mysign = getMD5String(prestr);

                AddParameter("sign", mysign);
            }

            public String createLinkString()
            {
                StringBuilder parameterString = new StringBuilder("");

                //Add all the parameters to the signature string, with the exception of header parameters and the nonce (which
                //was already added).
                foreach (Parameter parameter in Parameters)
                {
                    if (parameter.Type != ParameterType.HttpHeader)
                    {
                        if (parameterString.Length > 0)
                        {
                            parameterString.Append("&");
                        }

                        parameterString.Append(parameter.Name + "=" + parameter.Value);
                    }
                }

                return parameterString.ToString();
            }

            private static char[] HEX_DIGITS = new char[]{'0', '1', '2', '3', '4', '5',
            '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

            public static String getMD5String(String str)
            {

                if (str == null || str.Trim().Length == 0)
                {
                    return "";
                }
                byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
                MD5CryptoServiceProvider md = new MD5CryptoServiceProvider();
                bytes = md.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(HEX_DIGITS[(bytes[i] & 0xf0) >> 4] + ""
                            + HEX_DIGITS[bytes[i] & 0xf]);
                }
                return sb.ToString();
            }
        }

        #endregion
    }
}
