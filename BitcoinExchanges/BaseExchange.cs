using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Web.Script.Serialization;
using ArbitrationUtilities.EnumerationObjects;
using ArbitrationUtilities.OrderObjects;
using CommonFunctions;
using RestSharp;

namespace BitcoinExchanges
{
    abstract public class BaseExchange
    {
        private string _name;
        private int _amountDecimalPlaces;
        private int _openOrders;
        private decimal _availableBtc;
        private decimal _btcInTransfer;
        private decimal _totalBtc;
        private decimal _availableFiat;
        private decimal _totalFiat;
        private decimal _tradeFee;
        private decimal _tradeFeeAsDecimal;
        private decimal _minimumBitcoinWithdrawalAmount;
        private decimal _btcTransferFee;
        private string _bitcoinDepositAddress;
        private OrderBook _orderBook;
        private readonly CurrencyType _currencyTypeBuyFeeIsAppliedTo;

        private readonly decimal _minimumOrderAmount = 5.0m;

        //Variables for api path informatoin
        public string AccountBalanceInfoPath;
        public string BaseUrl;
        public string OrderBookPath;
        public string OrderQueryPath;
        public string DeleteOrderPath;
        public string OpenOrderPath;

        //TODO REVISIT! Used to be 0.01, but with new implementation this has gone up
        public decimal MinimumBitcoinOrderAmount = 0.00001m;

        public abstract void UpdateOrderBook(int? maxSize = null);
        public abstract Dictionary<string, dynamic> GetOrderInformation(string orderId);
        public abstract bool IsOrderFulfilled(string orderId);
        public abstract void DeleteOrder(string orderId);
        public abstract List<Dictionary<string, dynamic>> GetAllOpenOrders();
        public abstract decimal RoundTotalCost(decimal costToRound);
        protected abstract string BuyInternal(decimal amount, decimal price);
        protected abstract string SellInternal(decimal amount, decimal price);
        protected abstract string TransferInternal(decimal amount, string address);
        protected abstract void CheckResponseForErrors(Dictionary<string, dynamic> responseContent);
        protected abstract object GetValueFromResponseResult(Dictionary<string, dynamic> resultContent, string key, bool keyAbsenceAllowed = false);
        protected abstract void SetUniqueExchangeSettings(NameValueCollection configSettings);
        protected abstract ExchangeObjects.ApiInfo ApiInfo { get; }

        /// <summary>
        /// Updates the available BTC, Fiat, and trade fee for this exchange.
        /// </summary>
        /// <param name="fiatType"></param>
        public abstract void UpdateBalances();

        /// <summary>
        /// Gets the trade fee of the user account from the exchange.
        /// </summary>
        public abstract void SetTradeFee();

        #region Property Getters and Setters

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public decimal AvailableBtc
        {
            get
            {
                return _availableBtc;
            }
            set
            {
                _availableBtc = value;
            }
        }

        public int OpenOrders
        {
            get
            {
                return _openOrders;
            }

            set { _openOrders = value; }
        }

        //Includes the total amount of btc, even that which is tied up in orders.
        public decimal TotalBtc
        {
            get
            {
                return _totalBtc;
            }
            set
            {
                _totalBtc = value;
            }
        }

        //The number of bitcoins that are in transfer to this exchange.
        public decimal BTCInTransfer
        {
            get
            {
                return _btcInTransfer;
            }

            set
            {
                _btcInTransfer = value;
            }
        }

        public decimal AvailableFiat
        {
            get
            {
                return _availableFiat;
            }
            set
            {
                _availableFiat = value;
            }
        }

        //Includes the total amount of btc, even that which is tied up in orders.
        public decimal TotalFiat
        {
            get
            {
                return _totalFiat;
            }
            set
            {
                _totalFiat = value;
            }
        }

        public decimal TradeFee
        {
            get
            {
                return _tradeFee;
            }
            set
            {
                _tradeFee = value;
                _tradeFeeAsDecimal = Decimal.Divide(_tradeFee, 100);
            }
        }

        public decimal TradeFeeAsDecimal
        {
            get
            {
                return _tradeFeeAsDecimal;
            }
        }

        public OrderBook OrderBook
        {
            get
            {
                if (_orderBook == null)
                {
                    _orderBook = new OrderBook();
                }

                return _orderBook;
            }
            set
            {
                _orderBook = value;
            }
        }

        public decimal MinimumBitcoinWithdrawalAmount
        {
            get
            {
                return _minimumBitcoinWithdrawalAmount;
            }
        }

        public string BitcoinDepositAddress
        {
            get
            {
                return _bitcoinDepositAddress;
            }
        }

        public Decimal BtcTransferFee
        {
            get
            {
                return _btcTransferFee;
            }
        }

        public CurrencyType CurrencyTypeBuyFeeIsAppliedTo
        {
            get { return _currencyTypeBuyFeeIsAppliedTo; }
        }

        /// <summary>
        /// The number of decimals places that the amount parameter must be rounded to when buying or selling from this exchange.
        /// </summary>
        public int AmountDecimalPlace
        {
            get {return _amountDecimalPlaces;}
        }

        public FiatType FiatTypeToUse { get; private set; }

        #endregion

        public BaseExchange(string name, bool getRequiredSettings = true, FiatType fiatTypeToUse = FiatType.Usd)
        {
            _name = name;
            FiatTypeToUse = fiatTypeToUse;

            if (getRequiredSettings)
            {
                SetRequiredConfigSettings();
            }

        }

        public BaseExchange(string name, CurrencyType currencyTypeBuyFeeIsApplieTo, int amountDecimalPlaces, bool getRequiredSettings = true, FiatType fiatTypeToUse = FiatType.Usd)
        {
            _name = name;
            _currencyTypeBuyFeeIsAppliedTo = currencyTypeBuyFeeIsApplieTo;
            _amountDecimalPlaces = amountDecimalPlaces;
            FiatTypeToUse = fiatTypeToUse;

            if (getRequiredSettings)
            {
                SetRequiredConfigSettings();
            }
        }

        public void UpdateEverything()
        {
            UpdateBalances();
            UpdateOrderBook();
        }

        public int UpdateOpenOrderCount()
        {
            List<Dictionary<string, dynamic>> openOrderList = GetAllOpenOrders();

            if (openOrderList == null)
            {
                _openOrders = 0;
            }

            else
            {
                _openOrders = openOrderList.Count;
            }
            
            return _openOrders;
        }

        /// <summary>
        /// Transfer the specified amount of btc to the given address from this exchange. If the given amount is less than 
        /// MinimumBitcoinWithdrawalAmount, this method will throw an error.
        /// </summary>
        /// <param name="amount">Amonut of btc to be transfered.</param>
        /// <param name="address">Btc address to send bitcoins to.</param>
        /// <returns>A string representing the confirmation of the transfer.</returns>
        public string Transfer(decimal amount, string address)
        {
            //If a MinimumBitcoinWithdrawalAmount was given for this exchange, the less than that amount if trying to be withdrawn,
            //throw an error.
            if (MinimumBitcoinWithdrawalAmount > 0 && amount < MinimumBitcoinWithdrawalAmount)
            {
                throw new ArgumentException("Amount ('" + amount + " btc') is too small to be withdrawn from " + Name + ".");
            }

            return TransferInternal(amount, address);
        }

        /// <summary>
        /// Sells the specified amount of btc at the given price. If the given amount is less than MinimumBitcoinOrderAmount, or 
        /// if the given price is less than zero, an error is thrown. 
        /// </summary>
        /// <param name="amount">Amount of bitcoins to sell.</param>
        /// <param name="price">Price to sell the bitcoins at.</param>
        /// <returns>A string representing the order.</returns>
        public string Sell(decimal amount, decimal price)
        {
            if (amount < MinimumBitcoinOrderAmount)
            {
                throw new ArgumentException("Cannot create ask order at " + Name + " because the given amount (" + amount + ") is too small.");
            }

            if (price <= 0.00m)
            {
                throw new ArgumentException("Cannot create bid order at " + Name + " because the given price must be greater than 0.");
            }

            return SellInternal(amount, price);
        }

        /// <summary>
        /// Buys the specified amount of btc at the given price. If the given amount is less than MinimumBitcoinOrderAmount, or 
        /// if the given price is less than zero, an error is thrown. 
        /// </summary>
        /// <param name="amount">Amount of bitcoins to buy.</param>
        /// <param name="price">Price to buy the bitcoins at.</param>
        /// <returns>A string representing the order.</returns>
        public string Buy(decimal amount, decimal price)
        {
            if (amount < MinimumBitcoinOrderAmount)
            {
                throw new ArgumentException("Cannot create ask order at " + Name + " because the given amount (" + amount + ") is too small.");
            }

            if (price <= 0.00m)
            {
                throw new ArgumentException("Cannot create bid order at " + Name + " because the given price must be greater than 0.");
            }

            return BuyInternal(amount, price);
        }

        public void SimulatedBuy(decimal amount, decimal totalBuyPrice)
        {
            if (totalBuyPrice > this.AvailableFiat)
            {
                throw new ArgumentException(String.Format("Cannot simulate a buy that costs ${0}; this exchange only has ${1}", totalBuyPrice, this.AvailableFiat));
            }

            AvailableBtc = Decimal.Add(AvailableBtc, amount);
            AvailableFiat = Decimal.Subtract(AvailableFiat, totalBuyPrice);
        }

        public void SimulatedSell(decimal amount, decimal totalSellPrice)
        {
            if (amount > this.AvailableBtc)
            {
                throw new ArgumentException(String.Format("Cannot simulate a sell with {0} bitcoins; this exchange only has {1} bitcoins", amount, this.AvailableBtc));
            }

            AvailableBtc = Decimal.Subtract(AvailableBtc, amount);
            AvailableFiat = Decimal.Add(AvailableFiat, totalSellPrice);
        }

        /// <summary>
        /// Sets the BTC and fiat balances for this exchange to the values specified in the app.config file. If a balance is not specified, then it defaults to 0.
        /// </summary>
        public void SetIntialSimulationBalances()
        {
            NameValueCollection simulationSettings = GetConfigSettings("ExchangeLoginInformation/" + Name);

            AvailableFiat = TypeConversion.ParseStringToDecimalLoose(simulationSettings["AvailableFiat"]);
            AvailableBtc = TypeConversion.ParseStringToDecimalLoose(simulationSettings["AvailableBtc"]);
        }

        public virtual bool IsOrderCostValidForExchange(decimal orderPrice, decimal orderAmount)
        {
            //For most exchanges, orders need to be at least $5
            if (orderPrice < _minimumOrderAmount)
            {
                return false;
            }

            //Also make sure the order amount meetse the minimum requirement.
            if (orderAmount < MinimumBitcoinOrderAmount)
            {
                return false;
            }

            return true;
        }

        public virtual decimal ApplyFeeToBuyCost(decimal buyCost)
        {
            //Fee is applied to btc, so no need to take into account when calculating total cost
            if (CurrencyTypeBuyFeeIsAppliedTo == CurrencyType.Bitcoin)
            {
                return RoundTotalCost(buyCost);
            }

            //Fee is applied to fiat; so include it in the buy calculation
            return RoundTotalCost(Decimal.Multiply(Decimal.Add(1.0m, TradeFeeAsDecimal), buyCost));
        }

        public virtual decimal ApplyFeeToSellCost(decimal sellCost)
        {
            return RoundTotalCost(Decimal.Multiply(Decimal.Subtract(1.0m, TradeFeeAsDecimal), sellCost));
        }

        /// <summary>
        /// Simply posts the request the baseUrl, and returns the response as a string.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        protected string SimplePost(RestRequest request, string baseUrl = null)
        {
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

            return response.Content;
        }

        /// <summary>
        /// Posts the given request, checkes for errors, and a derserialized JSON object as a dictionary.
        /// </summary>
        /// <param name="request">Request to be sent to the exchange.</param>
        /// <param name="baseUrl">Optional, address to send the request to. If not given, uses the BaseUrl property.</param>
        /// <returns></returns>
        protected virtual Dictionary<string, dynamic> ApiPost(RestRequest request, string baseUrl = null)
        {
            string response = SimplePost(request, baseUrl);

            //Some exchanges (like ItBit) do not return a properly formatted json object. This method will remove square brackets from the beginning and end of the string.
            response = RemoveBeginAndEndingSquareBrackets(response);

            Dictionary<string, dynamic> returnedContent = new JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(response);

            if (returnedContent == null)
            {
                throw new Exception("Response from " + Name + " was null. Here is the raw response string: " + Environment.NewLine + response);
            }

            //See if there was an error using the api
            CheckResponseForErrors(returnedContent);

            return returnedContent;
        }

        /// <summary>
        /// Posts the given request, checkes for errors, and a derserialized JSON object as a array of dictionaries.
        /// </summary>
        /// <param name="request">Request to be sent to the exchange.</param>
        /// <param name="baseUrl">Optional, address to send the request to. If not given, uses the BaseUrl property.</param>
        /// <returns></returns>
        protected Dictionary<string, dynamic>[] ApiPost_Array(RestRequest request, string baseUrl = null)
        {
            string response = SimplePost(request, baseUrl);

            //First try derserialing as an array
            try
            {
                Dictionary<string, dynamic>[] returnedContent = new JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>[]>(response);

                if (returnedContent == null)
                {
                    throw new Exception("Response from " + Name + " was null. Here is the raw response string: " + Environment.NewLine + response);
                }

                return returnedContent;
            }

            catch (Exception)
            {
                //There was some kind of problem; try deserialzing as a dictionary, and try to get the error out.
                Dictionary<string, dynamic> returnedContent = new JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(response);

                if (returnedContent == null)
                {
                    throw new Exception("Response from " + Name + " was null. Here is the raw response string: " + Environment.NewLine + response);
                }

                //See if there was an error using the api
                CheckResponseForErrors(returnedContent);

                //If the check method didn't come up with anything, who knows what happened
                throw new Exception("Unknown error derserializing response.");
            }

        }
     
        protected void BuildOrderBook(Dictionary<string, dynamic> jsonDict, object amountPosition, object pricePosition, int? maxSize, string askKey = "asks", string bidKey = "bids")
        {
            if (OrderBook != null)
            {
                OrderBook.ClearOrderBook();
            }

            if (jsonDict != null)
            {
                OrderBook.Asks = ConvertOrderArrayListToOrderList((ArrayList)jsonDict[askKey], amountPosition, pricePosition, maxSize);
                OrderBook.Bids = ConvertOrderArrayListToOrderList((ArrayList)jsonDict[bidKey], amountPosition, pricePosition, maxSize);
            }
        }

        protected NameValueCollection GetConfigSettings(string settingsPath)
        {
            NameValueCollection returnSettingsCollection;
            returnSettingsCollection = (NameValueCollection)ConfigurationManager.GetSection(settingsPath);

            if (returnSettingsCollection == null)
            {
                throw new Exception("Could not find config settings for " + Name + ".");
            }

            return returnSettingsCollection;
        }

        /// <summary>
        /// Sets all the required settings for this exchange object, including both the universal settings (settings that apply),
        /// and the exchange specific settings.
        /// </summary>
        private void SetRequiredConfigSettings()
        {
            NameValueCollection configSettings = GetConfigSettings("ExchangeLoginInformation/" + Name);

            SetUniversalConfigSettings(configSettings);
            SetUniqueExchangeSettings(configSettings);
        }

        /// <summary>
        /// Sets all the universal exchange settings.
        /// </summary>
        /// <param name="configSettings">Collection of config settings to pull the values from.</param>
        private void SetUniversalConfigSettings(NameValueCollection configSettings)
        {
            ApiInfo.Key = configSettings["ApiKey"];
            ApiInfo.Secret = configSettings["ApiSecret"];
            _bitcoinDepositAddress = configSettings["BitcoinDepositAddress"];
            _minimumBitcoinWithdrawalAmount = TypeConversion.ParseStringToDecimalLoose(configSettings["MinimumBitcoinWithdrawalAmount"]);
            _btcTransferFee = TypeConversion.ParseStringToDecimalLoose(configSettings["BitcoinTransferFee"]);
        }

        /// <summary>
        /// If the given string starts with a '[' or ends with a ']', those characters are removed from the string. These characters are
        /// only removed if they are the first (for '[') or last (for ']'); brackets anywhere else in the string are not removed.
        /// </summary>
        /// <param name="content">The string to be modified.</param>
        /// <returns>The given string, minus any preceeding '[' and ending ']'.</returns>
        private string RemoveBeginAndEndingSquareBrackets(string content)
        {
            if (string.IsNullOrEmpty(content) == false)
            {
                if (content.StartsWith("["))
                {
                    content = content.Substring(1);
                }

                if (content.EndsWith("]"))
                {
                    content = content.Substring(0, content.Length - 1);
                }
            }
            return content;
        }

        private OrderList ConvertOrderArrayListToOrderList(ArrayList orderList, object amountPosition, object pricePosition, int? maxSize = null)
        {
            OrderList returnList = new OrderList();

            //If a maximum size was given, only put that number of orders in the asks and bids lists. Otherwise, get them all.
            if (maxSize != null && maxSize < orderList.Count)
            {
                orderList = orderList.GetRange(0, maxSize.Value);
            }

            if (orderList != null && orderList.Count > 0)
            {
                foreach (object order in orderList)
                {
                    decimal amount = 0.0m;
                    decimal price = 0.0m;

                    if (order != null && order.GetType() == typeof(Dictionary<string, object>))
                    {
                        Dictionary<string, object> orderDict = (Dictionary<string, object>)order;

                        if (orderDict.ContainsKey((string)pricePosition) && orderDict.ContainsKey((string)amountPosition) && orderDict[(string)amountPosition] != null && orderDict[(string)pricePosition] != null)
                        {
                            amount = TypeConversion.ParseStringToDecimalStrict(orderDict[(string)amountPosition].ToString());
                            price = TypeConversion.ParseStringToDecimalStrict(orderDict[(string)pricePosition].ToString());
                        }
                    }

                    else if (order != null && order.GetType() == typeof(ArrayList))
                    {
                        ArrayList orderArrayList = (ArrayList)order;

                        //Make sure each order has a price and amount
                        if (orderArrayList.Count >= 2)
                        {
                            amount = TypeConversion.ParseStringToDecimalStrict(orderArrayList[(int)amountPosition].ToString());
                            price = TypeConversion.ParseStringToDecimalStrict(orderArrayList[(int)pricePosition].ToString());
                        }
                    }

                    returnList.Add(new Order(amount, price));
                }
            }

            return returnList.Count <= 0 ? null : returnList;
        }
    }
}
