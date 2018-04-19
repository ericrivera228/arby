using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using ArbitrationSimulator.Exchanges.ExchangeObjects;
using RestSharp;

namespace ArbitrationSimulator.Exchanges
{
    public class BtcXe : Exchange
    {
        #region Class variables and Properties

        private const string EUR_SYMBOL = "EUR";
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

        public BtcXe() : base("BtcXe")
        {
            //No trade fees for BtcXe
            TradeFee = 0.0m;
        }

        #endregion

        #region Public Methods

        public override void UpdateOrderBook(int? maxSize = null)
        {
            BtcXeRequest orderBookRequest = new BtcXeRequest(OrderBookPath);
            orderBookRequest.AddParameter("currency", EUR_SYMBOL);

            Dictionary<string, dynamic> response = ApiPost(orderBookRequest);

            //Pull the orderbook object from the response
            response = (Dictionary<string, dynamic>)GetValueFromResponseResult(response, "order-book");

            BuildOrderBook(response, "order_amount", "price", maxSize, "ask", "bid");
        }

        public override void UpdateAvailableBalances()
        {
            // Create the authenticated request
            RestRequest request = new BtcXeRequest(AccountBalanceInfoPath, _apiInfo);
            Dictionary<string, dynamic> response = ApiPost(request);
        }

        public override void UpdateTotalBalances()
        {
            throw new NotImplementedException();
        }

        public override void SetTradeFee()
        {
            //BtcXe doesn't not have trade fees, so just set it to 0. Not at all necessary, but hey, leave me alone about it already.
            TradeFee = 0.0m;
        }

        #endregion

        #region Protected Methods

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
            //ToDo: impelment!
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
            //No unique settings to be set; do nothing.
        }

        #endregion

        #region Nested Classes

        private class BtcXeRequest : RestRequest, IExchangeRequest
        {
            //private readonly Int32 _nonce = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            private readonly Int32 _nonce = 1;
            private readonly ApiInfo _apiInfo;

            /// <summary>
            /// Constructor for a private request to BtceXe. Builds a requst that is ready to be sent to BtceXe.
            /// </summary>
            /// <param name="resource">Which part of the api you are going to hit. i.e. 'private/Balance'</param>
            /// <param name="apiInfo">ApiInfoStruct containing the key and secret.</param>
            public BtcXeRequest(string resource, ApiInfo apiInfo)
                : base(resource, Method.POST)
            {
                _apiInfo = apiInfo;
                AddParameter("api_key", _apiInfo.Key);
                AddParameter("nonce", _nonce);
                AddSignatureHeader();
            }

            /// <summary>
            /// Constructor for a public request to BtceXe. Builds a requst that is ready to be sent to BtceXe.
            /// </summary>
            /// <param name="resource">Which part of the api you are going to hit. i.e. 'public/Depth'</param>
            public BtcXeRequest(string resource)
                : base(resource, Method.GET)
            {
            }

            public void AddSignatureHeader()
            {
                var params1 = new List<KeyValuePair<string, string>>();
                params1.Add(new KeyValuePair<string, string>("api_key", _apiInfo.Key));
                params1.Add(new KeyValuePair<string, string>("nonce", _nonce.ToString()));

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                //var message = serializer.Serialize(params1);
                var message = "{\"api_key\":\"" + _apiInfo.Key + "\"," + "\"nonce\":" + _nonce + "}";

                var encoding = new ASCIIEncoding();
                byte[] keyByte = encoding.GetBytes(_apiInfo.Secret);
                byte[] messageBytes = encoding.GetBytes(message);
                using (var hmacsha256 = new HMACSHA256(keyByte))
                {
                    byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                    var signature = BitConverter.ToString(hashmessage);
                    signature = signature.Replace("-", "").ToLower();

                    // add signature to request parameters
                    params1.Add(new KeyValuePair<string, string>("signature", signature));
                    AddParameter("signature", signature);
                }

                //Add stuff here! 
                //string msg = new JavaScriptSerializer().Serialize(Parameters);
                //AddParameter("signature", ByteArrayToString(SignHmacSha256(_apiInfo.Secret, StringToByteArray(message))).ToUpper());
            }

            private static byte[] SignHmacSha256(String key, byte[] data)
            {
                HMACSHA256 hashMaker = new HMACSHA256(Encoding.ASCII.GetBytes(key));
                return hashMaker.ComputeHash(data);
            }

            private static byte[] StringToByteArray(string str)
            {
                return Encoding.ASCII.GetBytes(str);
            }

            private static string ByteArrayToString(byte[] hash)
            {
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        #endregion
    }
}
