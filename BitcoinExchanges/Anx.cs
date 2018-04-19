using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using ArbitrationUtilities.EnumerationObjects;
using CommonFunctions;
using RestSharp;
using BitcoinExchanges.ExchangeObjects;

namespace BitcoinExchanges
{
    public class Anx : BaseExchange
    {
        #region Class variables and Properties

        private const int BITCOIN_AMOUNT_MULTIPLIER = 100000000;
        private const int BITCOIN_FIAT_MULTIPLIER = 100000;
        private const string API_VERSION = "2";
        private const string BTC_EUR_PAIR_SYMBOL = "BTCEUR";
        private const string BTC_USD_PAIR_SYMBOL = "BTCUSD";

        //The btc\fiat pair name symbol that is being used
        private readonly string _btcFiatPairSymbol;

        private string _transferPath;
        private string _addOrderPath;
        private string _tradeListPath;
        private string _closedOrderQueryPath;
        private string _openOrderQueryPath;

        private AnxApiInfo _apiInfo = new AnxApiInfo(){Version = API_VERSION};

        protected override ApiInfo ApiInfo
        {
            get
            {
                if (_apiInfo == null)
                {
                    _apiInfo = new AnxApiInfo();
                    _apiInfo.Version = API_VERSION;
                }

                return _apiInfo;
            }
        }

        #endregion

        #region Constructors

        public Anx()
            : base("Anx", CurrencyType.Bitcoin, 8)
        {
            //No fiat type provided; default is USD
            _btcFiatPairSymbol = BTC_USD_PAIR_SYMBOL;

            InitializeAnx();
        }

        public Anx(FiatType fiatTypeToUse) : base("Anx", CurrencyType.Bitcoin, 8, true, fiatTypeToUse)
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

            InitializeAnx();
        }

        #endregion

        #region Public Methods

        public override void UpdateBalances()
        {
            AnxRequest request = new AnxRequest(AccountBalanceInfoPath, _apiInfo);
            request.AddSignatureHeader();
            Dictionary<string, dynamic> response = ApiPost(request);

            //Pull relevant information from the response
            response = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "data");
            response = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "Wallets");

            //Get BTC wallet information
            Dictionary<string, dynamic> btcDictionary = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "BTC");
            Dictionary<string, dynamic> availableBtcDictionary = (Dictionary<string, dynamic>)GetValueFromResponseResult(btcDictionary, "Available_Balance");
            Dictionary<string, dynamic> totalBtcDictionary = (Dictionary<string, dynamic>)GetValueFromResponseResult(btcDictionary, "Balance");

            //Get fiat wallet information
            Dictionary<string, dynamic> fiatDictionary = null;
            switch (FiatTypeToUse)
            {
                case FiatType.Eur:
                    fiatDictionary = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "EUR");
                    break;

                case FiatType.Usd:
                    fiatDictionary = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "USD");
                    break;
            }
            
            Dictionary<string, dynamic> availableFiatDictionary = (Dictionary<string, dynamic>)GetValueFromResponseResult(fiatDictionary, "Available_Balance");
            Dictionary<string, dynamic> totalFiatDictionary = (Dictionary<string, dynamic>)GetValueFromResponseResult(fiatDictionary, "Balance");

            AvailableBtc = TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(availableBtcDictionary, "value"));
            TotalBtc = TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(totalBtcDictionary, "value"));
            AvailableFiat = TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(availableFiatDictionary, "value"));
            TotalFiat = TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(totalFiatDictionary, "value"));
        }

        public override List<Dictionary<string, dynamic>> GetAllOpenOrders()
        {
            AnxRequest request = new AnxRequest(OpenOrderPath, _apiInfo);
            request.AddSignatureHeader();
            Dictionary<string, dynamic> response = ApiPost(request);
            ArrayList openOrders = (ArrayList)GetValueFromResponseResult(response, "data");

            if (openOrders.Count <= 0)
            {
                return null;
            }

            return openOrders.Cast<Dictionary<string, dynamic>>().ToList();
        }

        public override void SetTradeFee()
        {
            //Trade fee for Anx has to be set manually; can't get it from the api.
            NameValueCollection configSettings = GetConfigSettings("ExchangeLoginInformation/" + Name);
            TradeFee = TypeConversion.ParseStringToDecimalStrict(configSettings["TradeFee"]);
        }

        public override void UpdateOrderBook(int? maxSize = null)
        {
            AnxRequest orderBookRequest = new AnxRequest(OrderBookPath);
            Dictionary<string, dynamic> response = ApiPost(orderBookRequest);
            response = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "data");
            BuildOrderBook(response, "amount", "price", maxSize);
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
            throw new Exception("Could not get information for order " + orderId + " at " + Name + "; order is either not valid or has already been cancelled.");
        }

        public ArrayList GetTradeList()
        {
            //Build the request
            AnxRequest tradeListRequest = new AnxRequest(_tradeListPath, _apiInfo);
            tradeListRequest.AddSignatureHeader();

            //Post the request to ANX
            Dictionary<string, dynamic> response = ApiPost(tradeListRequest);

            //Return the list of trades
            return (ArrayList)GetValueFromResponseResult(response, "data");
        }

        public override bool IsOrderFulfilled(string orderId)
        {
            //Anx is really dumb and doesn't just tell you the status of an order. You have to make to different calls and see
            //which list the order ends up in to determine if it is open or closed.
            Dictionary<string, dynamic> order = GetClosedOrderInformation(orderId);

            if (order != null)
            {
                return true;
            }

            //See if the order is in the open orders:
            order = GetOpenOrderInformation(orderId);

            if (order != null)
            {
                return false;
            }

            //If the code made it this far, either was order was not valid or it has already been cancelled. Either, that is a problem.
            throw new Exception("Could not get information for order " + orderId + " at " + Name + "; order is either not valid or has already been cancelled.");
        }

        public override void DeleteOrder(string orderId)
        {
            //Build the request
            AnxRequest deleteOrderRequest = new AnxRequest(DeleteOrderPath, _apiInfo);
            deleteOrderRequest.AddParameter("oid", orderId);
            deleteOrderRequest.AddSignatureHeader();

            //Post the request to ANX
            try
            {
                ApiPost(deleteOrderRequest);
            }
            catch (Exception)
            {
                throw new Exception("Could not get information for order " + orderId + " at " + Name + "; order is either not valid or has already been cancelled.");
            }

        }

        /// <summary>
        /// Anx does not do any rounding on it's total costs.
        /// </summary>
        /// <param name="costToRound"></param>
        /// <returns></returns>
        public override decimal RoundTotalCost(decimal costToRound)
        {
            return costToRound;
        }

        #endregion

        #region Protected Methods

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
            //With ANX, the amount of bitcoins you can transfer can only be specified to 8 decimal places. So just grab the first 8 decimal places. This can cause a small error if this
            //method is being called with an amount that has more decimal places, but that translates to 0.000000001 of a bitcoin, which is like 1 trillionth of a penny. So that's ok.
            amount = MathHelpers.FloorRound(amount, 8);

            //Anx needs the amount and price as integer, so convert them based on the multipliers they require.
            int amountInt = Decimal.ToInt32(amount * BITCOIN_AMOUNT_MULTIPLIER);

            //Build the request
            AnxRequest transferRequest = new AnxRequest(_transferPath, _apiInfo);
            transferRequest.AddParameter("address", address);
            transferRequest.AddParameter("amount_int", amountInt);
            transferRequest.AddSignatureHeader();

            //Post the request to ANX
            Dictionary<string, dynamic> response = ApiPost(transferRequest);

            //Pull the transaction id out of the response
            response = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "data");
            return (string)GetValueFromResponseResult(response, "transactionId");
        }

        protected override void CheckResponseForErrors(Dictionary<string, dynamic> responseContent)
        {
            if ((string)GetValueFromResponseResult(responseContent, "result") == "error")
            {
                string message = "There was a problem connecting to the " + Name + " api: ";

                //Get the error message from the response and throw an error. It may either be in 'error', or 'data.'
                if (responseContent.ContainsKey("error"))
                {
                    message += responseContent["error"];
                }
                else if (responseContent.ContainsKey("data"))
                {
                    responseContent = responseContent["data"];
                    message += (string) GetValueFromResponseResult(responseContent, "message");
                }
                else
                {
                    message += "unknown error.";
                }

                message = StringManipulation.AppendPeriodIfNecessary(message);
                throw new Exception(message);
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

        protected override void SetUniqueExchangeSettings(NameValueCollection configSettings)
        {
            //Have to set trade fee for Anx manually; can't get it from the api.
            TradeFee = TypeConversion.ParseStringToDecimalStrict(configSettings["TradeFee"]);
        }

        #endregion

        #region Private Methods

        private Dictionary<string, dynamic> GetOpenOrderInformation(string orderId)
        {
            //Build the request
            AnxRequest orderInformationRequest = new AnxRequest(_openOrderQueryPath, _apiInfo);
            orderInformationRequest.AddSignatureHeader();

            //Post the request to ANX

            Dictionary<string, dynamic> response = ApiPost(orderInformationRequest);
            ArrayList responseData = (ArrayList)GetValueFromResponseResult(response, "data");

            //Loop through all the orders and see if the given order was in the list
            foreach (Dictionary<string, object> order in responseData)
            {
                if (((string)order["oid"]).Equals(orderId, StringComparison.InvariantCultureIgnoreCase))
                {
                    return order;
                }
            }

            //Order wasn't found amongst the open orders, return null
            return null;
        }

        private Dictionary<string, dynamic> GetClosedOrderInformation(string orderId)
        {
            //Build the request
            AnxRequest orderInformationRequest = new AnxRequest(_closedOrderQueryPath, _apiInfo);
            orderInformationRequest.AddParameter("order", orderId);
            orderInformationRequest.AddSignatureHeader();

            //Post the request to ANX
            try
            {
                Dictionary<string, dynamic> response = ApiPost(orderInformationRequest);

                //Return the list of trades
                return (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "data");
            }
            catch (Exception)
            {
                //Order was not found; return null
                return null;
            }

        }

        /// <summary>
        /// With Anx, buying and selling is the same Api call. So both SellInternal and BuyInternal point to this.
        /// </summary>
        /// <param name="amount">Amount of btc to be bought/sold.</param>
        /// <param name="price">Price to set for the order.</param>
        /// <param name="orderType">Type of order to set.</param>
        /// <returns>String representation of the executed order.</returns>
        private string ExecuteOrder(decimal amount, decimal price, OrderType orderType)
        {
            //-> With ANX, the amount of bitcoins you can buy can only be specified to 8 decimal places. So just grab the first 8 decimal places. This can cause a small error if this
            //   method is being called with an amount that has more decimal places, but that translates to 0.000000001 of a bitcoin, which is like 1 trillionth of a penny. So that's ok.
            //   Same story with the price.

            //-> The amount is rounded down, so that the amount of available bitcoin is not exceeded (which could happen if you are rounding up), and it is more cautious to buy/sell less bitcoin
            amount = MathHelpers.FloorRound(amount, Convert.ToInt16(Math.Log10(Convert.ToDouble(BITCOIN_AMOUNT_MULTIPLIER))));

            if (orderType == OrderType.Bid)
            {
                //-> The price is rounded up, because in the case of buying that is more conservative.
                //Using log() because that tells the number of decimals that need to be rounded to. 
                price = MathHelpers.CeilingRound(price, Convert.ToInt16(Math.Log10(Convert.ToDouble(BITCOIN_FIAT_MULTIPLIER))));
            }
            else
            {
                //-> The price is rounded down, because in the case of selling that is more conservative.
                //Using log() because that tells the number of decimals that need to be rounded to. 
                price = MathHelpers.FloorRound(price, Convert.ToInt16(Math.Log10(Convert.ToDouble(BITCOIN_FIAT_MULTIPLIER))));
            }

            //Anx needs the amount and price as integers, so convert them based on the multipliers they require.
            int amountInt = Decimal.ToInt32(amount * BITCOIN_AMOUNT_MULTIPLIER);
            int priceInt = Decimal.ToInt32(price * BITCOIN_FIAT_MULTIPLIER);

            AnxRequest addOrderRequest = new AnxRequest(_addOrderPath, _apiInfo);
            addOrderRequest.AddParameter("type", orderType.ToString());
            addOrderRequest.AddParameter("amount_int", amountInt);
            addOrderRequest.AddParameter("price_int", priceInt);
            addOrderRequest.AddSignatureHeader();

            //Post the request to ANX
            Dictionary<string, dynamic> response = ApiPost(addOrderRequest);

            //Pull the transaction id out of the response
            return (string)GetValueFromResponseResult(response, "data");
        }

        private void InitializeAnx()
        {
            BaseUrl = "https://anxpro.com/api/2/";
            AccountBalanceInfoPath = "money/info";
            OrderBookPath = _btcFiatPairSymbol + "/money/depth/full";
            DeleteOrderPath = _btcFiatPairSymbol + "/money/order/cancel";
            OrderQueryPath = "money/wallet/history";
            OpenOrderPath = _btcFiatPairSymbol + "/money/orders";
            
            _transferPath = "money/BTC/send_simple";
            _addOrderPath = _btcFiatPairSymbol + "/money/order/add";
            _tradeListPath = "money/trade/list";
            _closedOrderQueryPath = _btcFiatPairSymbol + "/money/order/result";
            _openOrderQueryPath = _btcFiatPairSymbol + "/money/orders";
        }

        #endregion

        #region Nested Classes

        private class AnxRequest : RestRequest, IExchangeRequest
        {
            private readonly long _nonce = DateTime.Now.Ticks;
            private readonly AnxApiInfo _apiInfo;

            public AnxRequest(string resource, AnxApiInfo apiInfo)
                : base(resource, Method.POST)
            {
                _apiInfo = apiInfo;
                AddHeader("Rest-Key", _apiInfo.Key);
                AddParameter("nonce", _nonce);
            }

            public AnxRequest(string resource)
                : base(resource, Method.GET)
            {
            }

            /// <summary>
            /// Adds the signature parameter to the header of this web request. This MUST be called after (and only after)
            /// all parameters have been added to this request.
            /// </summary>
            public void AddSignatureHeader()
            {
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

                AddHeader("Rest-Sign", getHash(Convert.FromBase64String(_apiInfo.Secret), Resource + Convert.ToChar(0) + signatureString));
            }

            private string getHash(byte[] keyByte, string message)
            {
                var hmacsha512 = new HMACSHA512(keyByte);
                var messageBytes = Encoding.UTF8.GetBytes(message);
                return Convert.ToBase64String(hmacsha512.ComputeHash(messageBytes));
            }
        }

        private class AnxApiInfo : ApiInfo
        {
            public string Version;
        }

        #endregion
    }
}
