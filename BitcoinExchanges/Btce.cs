using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using ArbitrationUtilities.EnumerationObjects;
using BitcoinExchanges.ExchangeObjects;
using CommonFunctions;
using RestSharp;

namespace BitcoinExchanges
{
    public class Btce : BaseExchange
    {
        private const string BTC_EUR_PAIR_SYMBOL = "btc_eur";
        private const string BTC_USD_PAIR_SYMBOL = "btc_usd";
        private const string EUR_SYMBOL = "eur";
        private const string USD_SYMBOL = "usd";

        //The btc\fiat pair name symbol that is being used
        private readonly string _btcFiatPairSymbol;
        private readonly string _tradeListPath = "TradeHistory";
        private readonly string _closedOrderQueryPath = "TradeHistory";
        
        private ApiInfo _apiInfo = new ApiInfo();

        public Btce() : base("Btce", CurrencyType.Bitcoin, 8, true)
        {
            //No fiat type provided; default is USD
            _btcFiatPairSymbol = BTC_USD_PAIR_SYMBOL;

            InitializeBtce();
        }

        public Btce(FiatType fiatTypeToUse) : base("Btce", CurrencyType.Bitcoin, 8, true, fiatTypeToUse)
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

            InitializeBtce();
        }

        #region Public Methods

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

        public override void UpdateBalances()
        {
            BtceRequest accountInfoRequest = new BtceRequest("", _apiInfo);
            accountInfoRequest.AddParameter("method", AccountBalanceInfoPath);
            accountInfoRequest.AddSignatureHeader();

            //Make the api call
            Dictionary<string, dynamic> response = ApiPost(accountInfoRequest);

            //Pull the fiat and bitcoin amounts from the response
            response = (Dictionary<string, dynamic>)GetValueFromResponse(response, "return");
            response = (Dictionary<string, dynamic>)GetValueFromResponse(response, "funds");

            //Get the BTC value from the response, convert it to a decimal and sign it to this exchange.
            AvailableBtc = GetNumberFromResponse(response, "btc");

            switch (FiatTypeToUse)
            {
                case FiatType.Eur:
                    AvailableFiat = GetNumberFromResponse(response, EUR_SYMBOL);
                    break;

                case FiatType.Usd:
                    AvailableFiat = GetNumberFromResponse(response, USD_SYMBOL);
                    break;
            }
            

            //TODO: Implement total balances for BTCe. For now, just make them the same as available
            TotalBtc = AvailableBtc;
            TotalFiat = AvailableFiat;
        }

        public override void SetTradeFee()
        {
            //Trade fee for Btce has to be set manually; can't get it from the api.
            NameValueCollection configSettings = GetConfigSettings("ExchangeLoginInformation/" + Name);
            TradeFee = TypeConversion.ParseStringToDecimalStrict(configSettings["TradeFee"]);
        }

        public override void UpdateOrderBook(int? maxSize = null)
        {
            BtceRequest orderBookRequest = new BtceRequest(OrderBookPath);

            Dictionary<string, dynamic> response = ApiPost(orderBookRequest, "https://wex.nz/api/3/depth/btc_usd");

            //TODO: Handle this more gracefully. Put this in here for now to get new BTCe API working
            if (response != null)
            {
                response = response["btc_usd"];

                BuildOrderBook(response, 1, 0, maxSize);
            }

            else
            {
                throw new Exception("Something went wrong. The world is probably ending. Do something about it I guess?");
            }

        }

        public override Dictionary<string, dynamic> GetOrderInformation(string orderId)
        {
            //Anx is really dumb and doesn't just tell you the status of an order. You have to make to different calls and see
            //which list the order ends up in to determine if it is open or closed.
            Dictionary<string, dynamic> order = GetClosedOrderInformation(orderId);

            if (order != null)
            {
                return order;
            }

            //See if the order is in the open orders:
            order = GetOpenOrderInformation(orderId);

            if (order != null)
            {
                return order;
            }

            //If the code made it this far, either was order was not valid or it has already been cancelled. Either, that is a problem.
            throw new Exception("Could not get information for order " + orderId + " at " + Name + "; order is either not valid.");
        }

        public override List<Dictionary<string, dynamic>> GetAllOpenOrders()
        {
            List<Dictionary<string, dynamic>> returnList = new List<Dictionary<string, dynamic>>();

            //Build the request
            BtceRequest orderInfoRequest = new BtceRequest("", _apiInfo);
            orderInfoRequest.AddParameter("method", OpenOrderPath);
            orderInfoRequest.AddSignatureHeader();

            try
            {
                Dictionary<string, dynamic> response = ApiPost(orderInfoRequest);
                response = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "return");

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
            catch (Exception e)
            {
                //The ApiPost will throw an error if there aren't any open orders. Check for that condition; for anything else, throw the error
                if (!e.Message.Contains("no orders"))
                {
                    throw;
                }

                return null;
            }
        }

        public Dictionary<string, dynamic> GetTradeList()
        {
            //Build the request
            BtceRequest tradeListRequest = new BtceRequest("", _apiInfo);
            tradeListRequest.AddParameter("method", _tradeListPath);
            tradeListRequest.AddSignatureHeader();

            //Post the request to ANX
            Dictionary<string, dynamic> response = ApiPost(tradeListRequest);
            response = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "return");
            return response;
        }

        #endregion

        #region Protected Methods

        protected override void SetUniqueExchangeSettings(NameValueCollection configSettings)
        {
            //Have to set trade fee for Btce manually; can't get it from the api.
            TradeFee = TypeConversion.ParseStringToDecimalStrict(configSettings["TradeFee"]);
        }

        public override bool IsOrderFulfilled(string orderId)
        {
            //When an order is filled immediately, Btce doesn't return an order id, it just returns zero.
            //Btce doesn't just tell you the status of an order. You have to make to different calls and see
            //which list the order ends up in to determine if it is open or closed.
            Dictionary<string, dynamic> order = GetClosedOrderInformation(orderId);

            if (order != null)
            {
                return true;
            }

            //See if the order is in the open orders:

            try
            {
                //If the order is a valid open order, this method should return it. Otherwise, this method shuold 
                //throw an exception. It shouldn't ever return null. 
                order = GetOpenOrderInformation(orderId);

                if (order != null)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                //If the exception is the btce standard 'no orders', replace it with a more helpful exception
                if (e.Message.Contains("no orders"))
                {
                    throw new Exception("Could not get information for order " + orderId + " at " + Name + "; order is not valid.");
                }

                //Else, something else went wrong with the call. Throw the original error.
                throw;
            }
            
            //Defensive code; this shouldn't ever be reached because the open order call will only return an order, or throw an exception
            //But just in case
            throw new Exception("Could not get information for order " + orderId + " at " + Name + "; order is not valid.");
        }

        public override void DeleteOrder(string orderId)
        {
            //Build the request
            BtceRequest deleteOrderRequest = new BtceRequest("", _apiInfo);
            deleteOrderRequest.AddParameter("method", DeleteOrderPath);
            deleteOrderRequest.AddParameter("order_id", orderId);
            deleteOrderRequest.AddSignatureHeader();

            //If the order is not valid, this will throw an error
            ApiPost(deleteOrderRequest);
        }

        public override decimal RoundTotalCost(decimal costToRound)
        {
            return Math.Round(costToRound, 8);
        }

        protected override string BuyInternal(decimal amount, decimal price)
        {
            return ExecuteOrder(amount, price, OrderType.Bid);
        }

        protected override string SellInternal(decimal amount, decimal price)
        {
            return ExecuteOrder(amount, price, OrderType.Ask);
        }

        protected override string TransferInternal(decimal amount, string address)
        {
            throw new NotImplementedException("Btce is annoying and can't do transfers from the api.");
        }

        protected override void CheckResponseForErrors(Dictionary<string, dynamic> responseContent)
        {
            if (responseContent.ContainsKey("error"))
            {
                throw new WebException(StringManipulation.AppendPeriodIfNecessary("There was a problem connecting to the " + Name + " api: " + (string)responseContent["error"]));
            }
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

        private Dictionary<string, dynamic> GetClosedOrderInformation(string orderId)
        {
            //Build the request
            BtceRequest orderInfoRequest = new BtceRequest("", _apiInfo);
            orderInfoRequest.AddParameter("method", _closedOrderQueryPath);
            orderInfoRequest.AddSignatureHeader();

            Dictionary<string, dynamic> response = ApiPost(orderInfoRequest);
            response = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "return");
            
            //Loop through all the orders, looking for one that matches the given id
            foreach (KeyValuePair<string, dynamic> orderInfo in response)
            {
                if (GetValueFromResponseResult(orderInfo.Value, "order_id").ToString() == orderId)
                {
                    return orderInfo.Value;
                }
            }
            
            //Order was not found, return null
            return null;
        }

        private Dictionary<string, dynamic> GetOpenOrderInformation(string orderId)
        {
            //Build the request
            BtceRequest orderInfoRequest = new BtceRequest("", _apiInfo);
            orderInfoRequest.AddParameter("method", OpenOrderPath);
            orderInfoRequest.AddSignatureHeader();

            Dictionary<string, dynamic> response = ApiPost(orderInfoRequest);
            response = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "return");

            //Post the request to ANX
            try
            {
                return (Dictionary<string, dynamic>)GetValueFromResponseResult(response, orderId);
            }
            catch (Exception)
            {
                //Order was not found; return null
                return null;
            }
        }

        private object GetValueFromResponse(Dictionary<string, dynamic> response, string key)
        {
            if (!response.ContainsKey(key))
            {
                throw new WebException("There was a problem with the " + Name + " api. '" + key + "' object was not part of the web response.");
            }

            return response[key];
        }

        /// <summary>
        /// With Btce, buying and selling is the same Api call. So both SellInternal and BuyInternal point to this.
        /// </summary>
        /// <param name="amount">Amount of btc to be bought/sold.</param>
        /// <param name="price">Price to set for the order.</param>
        /// <param name="orderType">Can be either be "buy" or "sell".</param>
        /// <returns>String representation of the executed order.</returns>
        private string ExecuteOrder(decimal amount, decimal price, OrderType orderType)
        {
            string orderTypeString = "";

            switch (orderType)
            {
                case OrderType.Bid:
                    orderTypeString = "buy";
                    break;

                case OrderType.Ask:
                    orderTypeString = "sell";
                    break;
            }

            BtceRequest buyRequest = new BtceRequest("", _apiInfo);
            buyRequest.AddParameter("method", "Trade");
            buyRequest.AddParameter("type", orderTypeString);
            buyRequest.AddParameter("pair", _btcFiatPairSymbol);
            buyRequest.AddParameter("rate", price);
            buyRequest.AddParameter("amount", amount);
            buyRequest.AddSignatureHeader();

            //Make the api call
            Dictionary<string, dynamic> response = ApiPost(buyRequest);

            //Get order id from the response
            response = (Dictionary<string, dynamic>)GetValueFromResponse(response, "return");

            return Convert.ToString(GetNumberFromResponse(response, "order_id"), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// For numbers, if it is '0' it is set as an int, but if it is anything else it is set as a decimal. This method detects this, and 
        /// always returns a decimal from the response.
        /// </summary>
        /// <returns>The value of the given key, as a decimal.</returns>
        private decimal GetNumberFromResponse(Dictionary<string, dynamic> response, string key)
        {
            object numberFromResponse = GetValueFromResponse(response, key);

            if (numberFromResponse is int)
            {
                return Convert.ToDecimal(numberFromResponse);
            }

            if (numberFromResponse is decimal)
            {
                return (decimal)numberFromResponse;
            }

            throw new Exception("The value for key '" + key + "' returned by " + Name + " was not an int or decimal.");
        }

        private void InitializeBtce()
        {
            BaseUrl = "https://wex.nz/tapi";
            AccountBalanceInfoPath = "getInfo";
            OrderBookPath = _btcFiatPairSymbol + "/depth";
            DeleteOrderPath = "CancelOrder";
            OpenOrderPath = "ActiveOrders";
        }

        #endregion

        #region Nested Classes

        private class BtceRequest : RestRequest, IExchangeRequest
        {
            private readonly ApiInfo _apiInfo;

            public BtceRequest(string resource, ApiInfo apiInfo)
                : base(resource, Method.POST)
            {
                _apiInfo = apiInfo;
                AddHeader("Key", _apiInfo.Key);
            }

            public BtceRequest(string resource)
                : base(resource, Method.GET)
            {
            }

            /// <summary>
            /// Adds the signature parameter to the header of this web request. This MUST be called after (and only after)
            /// all parameters have been added to this request.
            /// </summary>
            public void AddSignatureHeader()
            {
                //Nonce needs to be the last parameter that is added.
                AddParameter("nonce", GetNonce());

                StringBuilder signatureString = new StringBuilder();

                //Add all the parameters to the signature string, with the exception of header parameters and the nonce (which
                //was already added).
                foreach (Parameter parameter in Parameters)
                {
                    if (parameter.Type != ParameterType.HttpHeader)
                    {
                        if (signatureString.Length > 0)
                        {
                            signatureString.Append("&");
                        }

                        signatureString.Append(parameter.Name + "=" + parameter.Value);
                    }
                }

                AddHeader("Sign", GetHash(signatureString.ToString()).ToLower());
            }

            private double GetNonce()
            {
                DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return (UInt32)((DateTime.UtcNow - unixEpoch).TotalSeconds * 10);
            }

            private string GetHash(string message)
            {
                var data = Encoding.ASCII.GetBytes(message);
                HMACSHA512 hashMaker = new HMACSHA512(Encoding.ASCII.GetBytes(_apiInfo.Secret));
                return ByteArrayToString(hashMaker.ComputeHash(data)).ToLower();
            }

            private string ByteArrayToString(byte[] ba)
            {
                return BitConverter.ToString(ba).Replace("-", "");
            }
        }
        #endregion
    }
}
