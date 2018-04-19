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
using ArbitrationUtilities.OrderObjects;
using CommonFunctions;
using RestSharp;
using BitcoinExchanges.ExchangeObjects;

namespace BitcoinExchanges
{
    public class Bitstamp : BaseExchange
    {
        #region Class Variables Properties
        
        private BitstampApiInfo _apiInfo = new BitstampApiInfo();
        private readonly string  _conversionRatePath = "eur_usd/";
        private readonly string _addBuyOrderPath = "buy/";
        private readonly string _addSellOrderPath = "sell/";
        private readonly string _transferPath = "bitcoin_withdrawal/";

        //Conversion rate of Euro to USD. Divide by this number to get Euro, multiply by this number to get USD.
        private decimal _conversionRate;

        protected override ApiInfo ApiInfo
        {
            get
            {
                if (_apiInfo == null)
                {
                    _apiInfo = new BitstampApiInfo();
                }

                return _apiInfo;
            }
        }

        protected decimal ConversionRate
        {
            get
            {
                //If the conversion rate has not been set yet, do so.
                if (_conversionRate == 0.0m)
                {
                    _conversionRate = GetUsdEurConversionRate();
                }

                return _conversionRate;
            }

            set { _conversionRate = value; }
        }

        #endregion

        #region Constructors

        public Bitstamp() : base("Bitstamp", CurrencyType.Fiat, 8)
        {
            InitializeBitstamp();
        }

        public Bitstamp(FiatType fiatTypeToUse) : base("Bitstamp", CurrencyType.Fiat, 8, true, fiatTypeToUse)
        {
            InitializeBitstamp();
        }

        #endregion

        #region Public Methods

        public override sealed void UpdateBalances()
        {
            //If using EUR, reset the converion rate everytime this method is called, to ensure it is always up to date.
            if (FiatTypeToUse == FiatType.Eur)
            {
                ConversionRate = GetUsdEurConversionRate();    
            }
            
            // Create the authenticated request
            RestRequest request = new BitstampRequest(AccountBalanceInfoPath, _apiInfo);
            Dictionary<string, dynamic> response = ApiPost(request);

            //Set the trade fee, fiat, and btc balances.
            //Note, Bitstamp only uses USD, so before setting the fiat balance you must convert them to Euros
            TradeFee = TypeConversion.ParseStringToDecimalStrict(response["fee"]);
            AvailableBtc = TypeConversion.ParseStringToDecimalStrict(response["btc_available"]);
            TotalBtc = (TypeConversion.ParseStringToDecimalStrict(response["btc_available"]) + TypeConversion.ParseStringToDecimalStrict(response["btc_reserved"]));
            
            switch (FiatTypeToUse)
            {
                case FiatType.Eur:
                    AvailableFiat = TypeConversion.ParseStringToDecimalStrict(response["usd_available"]) / ConversionRate;
                    TotalFiat = ((TypeConversion.ParseStringToDecimalStrict(response["usd_available"]) / ConversionRate) + (TypeConversion.ParseStringToDecimalStrict(response["usd_reserved"]) / ConversionRate));
                    break;

                case FiatType.Usd:
                    AvailableFiat = TypeConversion.ParseStringToDecimalStrict(response["usd_available"]);
                    TotalFiat = ((TypeConversion.ParseStringToDecimalStrict(response["usd_available"])) + TypeConversion.ParseStringToDecimalStrict(response["usd_reserved"]));
                    break;

                //Defensive code
                default:
                    throw new Exception("Unkown fiat type.");
            }
        }

        public override void SetTradeFee()
        {
            // Create the authenticated request
            RestRequest request = new BitstampRequest(AccountBalanceInfoPath, _apiInfo);
            Dictionary<string, dynamic> response = ApiPost(request);

            TradeFee = TypeConversion.ParseStringToDecimalStrict(response["fee"]);
        }

        public override void UpdateOrderBook(int? maxSize = null)
        {
            BitstampRequest orderBookRequest = new BitstampRequest(OrderBookPath);
            Dictionary<string, dynamic> response = ApiPost(orderBookRequest);

            if (!response.ContainsKey("bids") || !response.ContainsKey("asks"))
            {
                throw new Exception("Could not update order book for " + Name + ", 'asks' or 'bids' object was not in the response.");
            }

            BuildOrderBook(response, 1, 0, maxSize);

            if (FiatTypeToUse == FiatType.Eur)
            {
                ConvertOrderBookToEuro();
            }   
        }

        public override Dictionary<string, dynamic> GetOrderInformation(string orderId)
        {
            BitstampRequest orderStatusRequest = new BitstampRequest(OrderQueryPath, _apiInfo);
            orderStatusRequest.AddParameter("id", orderId);

            Dictionary<string, dynamic> orderStatusReponse = ApiPost(orderStatusRequest);

            return orderStatusReponse;
        }

        public override List<Dictionary<string, dynamic>> GetAllOpenOrders()
        {
            BitstampRequest openOrderRequest = new BitstampRequest(OpenOrderPath, _apiInfo);
            
            Dictionary<string, dynamic>[] orderStatusReponse = ApiPost_Array(openOrderRequest);

            if (orderStatusReponse.Length <= 0)
            {
                return null;
            }

            return orderStatusReponse.ToList();
        }

        public decimal GetUsdEurConversionRate()
        {
            //If the conversion re
            BitstampRequest conversionRateRequest = new BitstampRequest(_conversionRatePath);
            Dictionary<string, dynamic> response = ApiPost(conversionRateRequest);
            return TypeConversion.ParseStringToDecimalStrict((string)GetValueFromResponseResult(response, "sell"));
        }

        public override bool IsOrderFulfilled(string orderId)
        {
            Dictionary<string, dynamic> statusDictionary = GetOrderInformation(orderId);
            string status = (string)GetValueFromResponseResult(statusDictionary, "status");

            if (status.Equals("Finished", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public override void DeleteOrder(string orderId)
        {
            BitstampRequest deleteOrderRequest = new BitstampRequest(DeleteOrderPath, _apiInfo);
            deleteOrderRequest.AddParameter("id", orderId);

            //Can't call ApiPost, because this request does not return a dictionary unless there is an error.
            RestClient client = new RestClient { BaseUrl = new Uri(BaseUrl) };
            IRestResponse response = client.Execute(deleteOrderRequest);

            //Don't think this can happen, but just in case
            if (response == null)
            {
                throw new Exception("Could not delete order " + orderId + " for " + Name + "; did not get a response back from the api.");
            }

            //If the response is anything other than 'true,' something went wrong. Extract the error message and throw an exception.
            if (!response.Content.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                //Extract the error message
                Dictionary<string, dynamic> returnedContent = new JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(response.Content);
                string errorMessage = StringManipulation.AppendPeriodIfNecessary((string)returnedContent["error"]);

                throw new Exception("Problem deleting order " + orderId + " for " + Name + ": " + errorMessage);
            }

            //If the response just contains the word 'true', then the delete was successful.
        }

        /// <summary>
        /// Bitstamp rounds total costs to 2 decimal places.
        /// </summary>
        /// <param name="costToRound"></param>
        /// <returns></returns>
        public override decimal RoundTotalCost(decimal costToRound)
        {
            return Math.Round(costToRound, 2);
        }


        public override decimal ApplyFeeToBuyCost(decimal buyCost)
        {
            //Round up the buy cost
            buyCost = RoundTotalCost(buyCost);
          
            //Calculate the fee:
            decimal feeCost = Decimal.Multiply(buyCost, TradeFeeAsDecimal);

            //Ceiling round the fee:
            feeCost = MathHelpers.CeilingRound(feeCost, 2);

            //Now apply it to the total cost
            buyCost = Decimal.Add(buyCost, feeCost);
            
            return buyCost;
        }

        public override decimal ApplyFeeToSellCost(decimal sellCost)
        {
            //Round up the sellCost
            sellCost = RoundTotalCost(sellCost);

            //Calculate the fee:
            decimal feeCost = Decimal.Multiply(sellCost, TradeFeeAsDecimal);

            //Ceiling round the fee:
            feeCost = MathHelpers.CeilingRound(feeCost, 2);

            //Now apply it to the total cost
            sellCost = Decimal.Subtract(sellCost, feeCost);

            return sellCost;
        }

        #endregion

        #region Protected Methods

        protected override string BuyInternal(decimal amount, decimal price)
        {
            if (FiatTypeToUse == FiatType.Eur)
            {
                //Price is given in Euros, but bitstamp only deals in USD. Convert price to USD. Remember to use ceiling round as this is a
                //buy, and thus should be rounded up.
                price = MathHelpers.CeilingRound(price * ConversionRate, 2);    
            }
            
            BitstampRequest buyRequest = new BitstampRequest(_addBuyOrderPath, _apiInfo);
            buyRequest.AddParameter("amount", amount);
            buyRequest.AddParameter("price", price);

            Dictionary<string, dynamic> buyReponse = ApiPost(buyRequest);

            //Bitstamp returns an integer, so turn it into a string
            return "" + GetValueFromResponseResult(buyReponse, "id");
        }

        protected override string SellInternal(decimal amount, decimal price)
        {
            if (FiatTypeToUse == FiatType.Eur)
            {
                //Price is given in Euros, but bitstamp only deals in USD. Convert price to USD. Remember to use floor round as this is a
                //sell, and thus should be rounded down.
                price = MathHelpers.FloorRound(price * ConversionRate, 2);    
            }
            
            BitstampRequest sellRequest = new BitstampRequest(_addSellOrderPath, _apiInfo);
            sellRequest.AddParameter("amount", amount);
            sellRequest.AddParameter("price", price);

            Dictionary<string, dynamic> sellResponse = ApiPost(sellRequest);

            //Bitstamp returns an integer, so turn it into a string
            return "" + GetValueFromResponseResult(sellResponse, "id");
        }

        protected override string TransferInternal(decimal amount, string address)
        {
            BitstampRequest transferRequest = new BitstampRequest(_transferPath, _apiInfo);
            transferRequest.AddParameter("amount", amount);
            transferRequest.AddParameter("address", address);

            Dictionary<string, dynamic> response = ApiPost(transferRequest);

            //Bitstamp returns an integer, so turn it into a string
            return "" + GetValueFromResponseResult(response, "id");
        }

        protected override void CheckResponseForErrors(Dictionary<string, dynamic> responseContent)
        {
            if (responseContent.ContainsKey("error"))
            {
                string errorMessage = "There was a problem connecting to the " + Name + " api: ";

                //Bitstamp either returns a single error, or an arraylist of error. This statement looks for the existance of such a list.
                if (responseContent["error"].GetType() == typeof(Dictionary<string, dynamic>) && ((Dictionary<string, dynamic>)responseContent["error"]).ContainsKey("__all__"))
                {
                    errorMessage += StringManipulation.AppendPeriodIfNecessary(StringManipulation.CreateDelimitedStringFromArrayList(responseContent["error"]["__all__"], '|'));
                }

                else
                {
                    object errorContent = responseContent["error"];

                    if (errorContent is string)
                    {
                        errorMessage += StringManipulation.AppendPeriodIfNecessary((string)responseContent["error"]);
                    }
                    else if (errorContent is Dictionary<string, dynamic>)
                    {

                        //Try an capture all of the error message that may be in the response.
                        foreach (KeyValuePair<string, dynamic> errorEntry in (Dictionary<string, dynamic>)errorContent)
                        {
                            ArrayList errors = errorEntry.Value;
                            errorMessage += "[" + errorEntry.Key;

                            foreach (string arrayListError in errors)
                            {
                                errorMessage += ", ";
                                errorMessage += arrayListError;
                            }

                            errorMessage += "] ";
                        }
                    }
                }

                throw new WebException(errorMessage);
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
            _apiInfo.ClientId = configSettings["ClientId"];
        }

        #endregion

        #region Private Methods

        private void ConvertOrderBookToEuro()
        {
            try
            {
                decimal conversionRate = GetUsdEurConversionRate();

                foreach (Order ask in OrderBook.Asks)
                {
                    ask.ConvertPrice(Decimal.Divide(1.0m, conversionRate));
                }

                foreach (Order bid in OrderBook.Bids)
                {
                    bid.ConvertPrice(Decimal.Divide(1.0m, conversionRate));
                }
            }
            catch (Exception)
            {
                OrderBook.ClearOrderBook();
                throw;
            }

        }

        private void InitializeBitstamp()
        {
            BaseUrl = "https://www.bitstamp.net/api/";
            AccountBalanceInfoPath = "balance/";
            OrderBookPath = "order_book/";
            OrderQueryPath = "order_status/";
            DeleteOrderPath = "cancel_order/";
            OpenOrderPath = "open_orders/";
        }

        #endregion

        #region Nested Classes

        private class BitstampRequest : RestRequest, IExchangeRequest
        {
            private readonly BitstampApiInfo _apiInfo;
            private readonly long _nonce = DateTime.Now.Ticks;
            
            public BitstampRequest(string resource, BitstampApiInfo apiInfo)
                : base(resource, Method.POST)
            {
                _apiInfo = apiInfo;

                AddParameter("key", _apiInfo.Key);
                AddParameter("nonce", _nonce);
                AddSignatureHeader();
            }

            public BitstampRequest(string resource)
                : base(resource, Method.GET)
            {
            }

            public void AddSignatureHeader()
            {
                string msg = string.Format("{0}{1}{2}", _nonce, _apiInfo.ClientId, _apiInfo.Key);
                AddParameter("signature", ByteArrayToString(SignHmacSha256(_apiInfo.Secret, StringToByteArray(msg))).ToUpper());
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

        private class BitstampApiInfo : ApiInfo
        {
            public string ClientId;
        }
        #endregion
    }
}
