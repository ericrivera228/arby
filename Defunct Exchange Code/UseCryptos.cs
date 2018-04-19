using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using ArbitrationSimulator.Exchanges.ExchangeObjects;
using ArbitrationSimulator.OrderObjects;
using RestSharp;

namespace ArbitrationSimulator.Exchanges
{
    public class UseCryptos : Exchange
    {
        #region Class variables and properties

        private const string BTC_EUR_SYMBOL = "btc-eur";
        private ApiInfo _apiInfo = new ApiInfo();
        private string _closedOrdersQueryPath;
        private string _openOrdersQueryPath;

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

        public UseCryptos() : base("UseCryptos")
        {
        }

        #endregion

        #region Public methods

        public override void UpdateOrderBook(int? maxSize = null)
        {
            UseCryptosRequest orderBookRequest = new UseCryptosRequest(OrderBookPath);

            //The order book response for UseCryptos is very different than other exchanges, so it does not follow the normal pattern
            ArrayList response = UseCryptosOrderBookGet(orderBookRequest);

            BuildOrderBook(response, maxSize);
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

        public override void UpdateAvailableBalances()
        {
            UseCryptosRequest request = new UseCryptosRequest(AccountBalanceInfoPath, _apiInfo);
            request.AddSignatureHeader();
            Dictionary<string, dynamic> response = ApiPost(request);
        }

        public override void UpdateTotalBalances()
        {
            throw new NotImplementedException();
        }

        public override void SetTradeFee()
        {
            //TODO Fix this!
            TradeFee = 0.35m; 
        }

        #endregion

        #region Protected methods

        protected void BuildOrderBook(ArrayList ordersJsonStruct, int? MaxSize = null)
        {
            if (OrderBook != null)
            {
                OrderBook.ClearOrderBook();
            }

            if (ordersJsonStruct != null && ordersJsonStruct.Count > 0)
            {
                foreach (Dictionary<string, dynamic> order in ordersJsonStruct)
                {
                    string orderType = (string) GetValueFromResponseResult(order, "type");
                    decimal amount = (decimal)GetValueFromResponseResult(order, "amount");
                    decimal price = (decimal)GetValueFromResponseResult(order, "price");
                    Order newOrder = new Order(amount, price);
                    
                    if (orderType == "buy")
                    {
                        OrderBook.Bids.Add(newOrder);
                    }
                    else if (orderType == "sell")
                    {
                        OrderBook.Asks.Add(newOrder);
                    }
                    else
                    {
                        throw new ArgumentException("Unknown order type found in order book response from " + Name + ": " + orderType);
                    }
                }

                //UseCryptos isn't nice and doesn't supply ordered bids/asks, so reorder them with an in-place sort.
                //Bids are sorted by descending price, asks are orded by ascending price.
                OrderBook.Bids.Sort((order1, order2) => -1* order1.Price.CompareTo(order2.Price));
                OrderBook.Asks.Sort((order1, order2) => order1.Price.CompareTo(order2.Price));

                //If necessary, trim down the lists to the given size.
                if (MaxSize != null)
                {
                    OrderBook.Asks.RemoveRange(MaxSize.Value, OrderBook.Asks.Count - MaxSize.Value);
                    OrderBook.Bids.RemoveRange(MaxSize.Value, OrderBook.Bids.Count - MaxSize.Value);
                }
            }
        }

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
            if(responseContent.ContainsKey("msg"))
            {
                throw new WebException((string)GetValueFromResponseResult(responseContent, "msg"));
            }
        }

        protected override object GetValueFromResponseResult(Dictionary<string, dynamic> resultContent, string key, bool keyAbsenceAllowed = false)
        {
            if (!resultContent.ContainsKey(key))
            {
                if (keyAbsenceAllowed == false)
                {
                    throw new WebException("There was a problem with the " + Name + " api. '" + key + "' object was not part of the web response.");
                }

                return "";
            }

            return resultContent[key];
        }

        protected override void SetUniqueExchangeSettings(NameValueCollection configSettings)
        {
            //These were already set in Exchange.SetUniversalConfigSettings(), but because UseCryptos api paths have parameters in them, need to do the substition here
            OrderBookPath = OrderBookPath.Replace("{CurrencyPair}", BTC_EUR_SYMBOL);

            _openOrdersQueryPath = configSettings["OpenOrdersQueryPath"];
            _closedOrdersQueryPath = configSettings["ClosedOrdersQueryPath"];
        }

        #endregion

        #region Private methods

        /// <summary>
        /// ApiPost unique for UseCryptos, which formats their responses differently from anyone else. Otherwise, operates in the same way as the base ApiPost method.
        /// The given request is posted, checked for errors, and returned.
        /// </summary>
        /// <param name="request">Request to be sent to the exchange.</param>
        /// <param name="baseUrl">Optional, address to send the request to. If not given, uses the BaseUrl property.</param>
        /// <returns></returns>
        private ArrayList UseCryptosOrderBookGet(RestRequest request, string baseUrl = null)
        {
            ArrayList returnedContent = null;

            if (baseUrl == null)
            {
                baseUrl = BaseUrl;
            }

            RestClient client = new RestClient { BaseUrl = new Uri(baseUrl) };
            IRestResponse response = client.Execute(request);

            //See if there was an error making the call
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }

            //First, check to see if the response had an error, which would be returned as a dictionary.
            try
            {
                Dictionary<string, dynamic> errorContent = new JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(response.Content);

                //If the resposne was able to be deserialized to a dictionary, that means there was an error.
                if (errorContent == null)
                {
                    throw new Exception("Response from " + Name + " was null. Here is the raw response string: " + Environment.NewLine + response.Content);
                }

                CheckResponseForErrors(errorContent);
            }

            //Rethrow the exceptions that come out of CheckResponseForErrors (the below catch clause actually also catches WebExceptions, so you need to explicitly rethrow
            catch (WebException)
            {
                throw;
            }

            //If there was an exception, that means the response came back correctly as an arraylist; try deserializing again
            catch (InvalidOperationException)
            {
                returnedContent = new JavaScriptSerializer().Deserialize<ArrayList>(response.Content);
            }

            return returnedContent;
        }

        #endregion

        #region Nested Classes
        
        private class UseCryptosRequest : RestRequest, IExchangeRequest
        {
            private readonly long _nonce = DateTime.Now.Ticks;
            private const string _privateApiEndpoint = "jsonapi/privateapi";
            private readonly ApiInfo _apiInfo;

            public UseCryptosRequest(string method, ApiInfo apiInfo) : base(_privateApiEndpoint, Method.POST)
            {
                _apiInfo = apiInfo;
                AddParameter("method", method);
                AddParameter("nonce", _nonce);
                AddHeader("Key", _apiInfo.Key);
            }

            /// <summary>
            /// Constructor for a public request to UseCryptos. Builds a requst that is ready to be sent to UseCryptos.
            /// </summary>
            /// <param name="resource">Which part of the api you are going to hit. i.e. 'public/Depth'</param>
            public UseCryptosRequest(string resource)
                : base(resource, Method.GET)
            {
            }

            public void AddSignatureHeader()
            {
                string paramString = BuildParamString();

                var hmAcSha = new HMACSHA512(Encoding.ASCII.GetBytes(_apiInfo.Secret));
                var messagebyte = Encoding.ASCII.GetBytes(paramString);
                var hashmessage = hmAcSha.ComputeHash(messagebyte);
                var sign = BitConverter.ToString(hashmessage);
                sign = sign.Replace("-", "");
                AddHeader("Sign", sign.ToLower());
            }

            private string BuildParamString()
            {
                StringBuilder parameterString = new StringBuilder();

                //Add all the parameters to the signature string, with the exception of header parameters and the nonce (which
                //was already added).
                foreach (Parameter parameter in Parameters)
                {
                    if (parameter.Type != ParameterType.HttpHeader && parameter.Name != "method" && parameter.Name != "nonce")
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
        }

        #endregion
    }
}
