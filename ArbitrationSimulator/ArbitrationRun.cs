using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ArbitrationUtilities.EnumerationObjects;
using BitcoinExchanges;
using DatabaseLayer;

namespace ArbitrationSimulator
{
    public class ArbitrationRun : IDomainObject
    {
        private bool _useAnx = false;
        private bool _useBitfinex = false;
        private bool _useBitstamp = false;
        private bool _useBitX = false;
        private bool _useBtce = false;
        private bool _useItBit = false;
        private bool _useKraken = false;
        private bool _useOkCoin = false;
        private bool _useCoinbase = false;
        private decimal _minimumProfit;
        private decimal _maxBtcTradeAmount;
        private decimal _maxFiatTradeAmount;
        private decimal? _exchangeBaseCurrencyPercentageRestriction;
        private int? _rollupNumber;
        private decimal? _rollupHours;
        private int _searchIntervalMilliseconds;
        private int _roundsRequiredForValidation;
        private string _logFileName;
        private DateTime? _startDateTime;
        private DateTime? _endDateTime;
        private DateTime? _createDateTime;
        private DateTime? _lastModifiedDateTime;
        private int? _id;
        private List<BaseExchange> _exchangeList;
        private ArbitrationMode _arbitrationMode;
        private OpportunitySelectionType _opportunitySelectionMethod;
        private TransferMode _transferMode;
        private FiatType _fiatType;

        #region Property Getters and Setters
        public List<BaseExchange> ExchangeList
        {
            get
            {
                return _exchangeList;
            }

            set
            {
                _exchangeList = value;
            }
        }

        public string LogFileName
        {
            get
            {
                return _logFileName;
            }

            set
            {
                _logFileName = value;

                try
                {
                    InitializeLogFile();
                }
                catch (IOException)
                {
                    //Cannot use the file given; clear _logFileName becaue it is not valid
                    _logFileName = null;

                    throw;
                }
            }
        }

        public bool UseAnx
        {
            get
            {
                return _useAnx;
            }

            set
            {
                _useAnx = value;
            }
        }

        public bool UseBitfinex
        {
            get
            {
                return _useBitfinex;
            }

            set
            {
                _useBitfinex = value;
            }
        }

        public bool UseBitstamp
        {
            get
            {
                return _useBitstamp;
            }

            set
            {
                _useBitstamp = value;
            }
        }

        public bool UseBitX
        {
            get
            {
                return _useBitX;
            }

            set
            {
                _useBitX = value;
            }
        }

        public bool UseBtce
        {
            get
            {
                return _useBtce;
            }

            set
            {
                _useBtce = value;
            }
        }

        public bool UseCoinbase
        {
            get
            {
                return _useCoinbase;
            }

            set { _useCoinbase = value; }
        }

        public bool UseItBit
        {
            get
            {
                return _useItBit;
            }

            set
            {
                _useItBit = value;
            }
        }

        public bool UseKraken
        {
            get
            {
                return _useKraken;
            }

            set
            {
                _useKraken = value;
            }
        }

        public bool UseOkCoin
        {
            get
            {
                return _useOkCoin;
            }

            set
            {
                _useOkCoin = value;
            }
        }

        /// <summary>
        /// Amount of time between arbitration hunts; in milliseconds.
        /// </summary>
        public int SeachIntervalMilliseconds
        {
            get
            {
                return _searchIntervalMilliseconds;
            }

            set
            {
                if (value > 0)
                {
                    _searchIntervalMilliseconds = value;
                }

                else
                {
                    throw new ArgumentException("Search interval cannot be less than 0");
                }
            }
        }

        /// <summary>
        /// Number of rounds that an arbitration opportunity must exist before it will be executed.
        /// Value must be greater than zero.
        /// </summary>
        public int RoundsRequiredForValidation
        {
            get
            {
                return _roundsRequiredForValidation;
            }
            set
            {
                if (value > 0)
                {
                    _roundsRequiredForValidation = value;
                }

                else
                {
                    throw new ArgumentException("Rounds required for validation cannot be less than 0.");
                }
            }
        }

        public decimal MinimumProfit
        {
            get
            {
                return _minimumProfit;
            }

            set
            {
                _minimumProfit = value;
            }
        }

        public decimal MaxBtcTradeAmount
        {
            get
            {
                return _maxBtcTradeAmount;
            }

            set
            {
                if (value > 0)
                {
                    _maxBtcTradeAmount = value;
                }

                else
                {
                    throw new ArgumentException("Max btc amount cannot be less than 0.");
                }
            }
        }

        public decimal MaxFiatTradeAmount
        {
            get
            {
                return _maxFiatTradeAmount;
            }

            set
            {
                if (value > 0)
                {
                    _maxFiatTradeAmount = value;
                }

                else
                {
                    throw new ArgumentException("Max fiat amount cannot be less than 0");
                }
            }
        }

        public decimal? ExchangeBaseCurrencyPercentageRestriction
        {
            get
            {
                return _exchangeBaseCurrencyPercentageRestriction;
            }

            set
            {
                //Value can be null, or it can be between 0 and 1. Anything else is an error.
                if (value == null || (value >= 0 && value <= 1.0m))
                {
                    _exchangeBaseCurrencyPercentageRestriction = value;
                }

                else
                {
                    throw new ArgumentException("Exchange base currency percentage restriction amount must be between 0 and 1.");
                }
            }
        }

        public int? RollupNumber
        {
            get { return _rollupNumber; }
            set
            {
                //Value can be null, or it can be 1 or greater. Anything else is an error.
                if (value == null || value > 0)
                {
                    _rollupNumber = value;
                }

                else
                {
                    throw new Exception("Rollup number must be at least 1.");
                }
            }
        }

        public decimal? RollupHours
        {
            get { return _rollupHours; }
            set
            {
                //Value can be null, or it must be greater than 0. Anything else is an error.
                if (value == null || value > 0)
                {
                    _rollupHours = value;
                }

                else
                {
                    throw new Exception("Rollup hours must be at least 0.");
                }
            }
        }

        /// <summary>
        /// Time at which this arbitration run began.
        /// </summary>
        public DateTime? StartDateTime
        {
            get
            {
                return _startDateTime;
            }

            set
            {
                _startDateTime = value;
            }
        }

        /// <summary>
        /// Time at which this arbitration run ended.
        /// </summary>
        public DateTime? EndDateTime
        {
            get
            {
                return _endDateTime;
            }

            set
            {
                _endDateTime = value;
            }
        }

        /// <summary>
        /// Time this arbitration run was put into the db.
        /// </summary>
        public DateTime? CreateDateTime
        {
            get
            {
                return _createDateTime;
            }

            set
            {
                _createDateTime = value;
            }
        }

        /// <summary>
        /// Time this arbitration run instance was last updated.
        /// </summary>
        public DateTime? LastModifiedDateTime
        {
            get
            {
                return _lastModifiedDateTime;
            }

            set
            {
                _lastModifiedDateTime = value;
            }
        }

        /// <summary>
        /// Id of this arbitration run in the ARBITRATION_RUN table in the db. 
        /// If this arbitration run object has not yet been saved to the database, then Id is null.
        /// </summary>
        public int? Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public OpportunitySelectionType OpportunitySelectionMethod
        {
            get
            {
                return _opportunitySelectionMethod;
            }
            set
            {
                _opportunitySelectionMethod = value;
            }
        }

        public ArbitrationMode ArbitrationMode
        {
            get
            {
                return _arbitrationMode;
            }
            set
            {
                if (value != ArbitrationMode.Live && value != ArbitrationMode.Simulation)
                {
                    throw new Exception("Unknown arbitration mode type: " + value + ".");
                }

                _arbitrationMode = value;
            }
        }

        public TransferMode TransferMode
        {
            get
            {
                return _transferMode;;
            }
            set
            {
                if (value != TransferMode.OnTime && value != TransferMode.RollupOnTrades && value != TransferMode.RollupByHour && value != TransferMode.None)
                {
                    throw new Exception("Unkown transfer mode type: " + value + ".");
                }

                _transferMode = value;
            }
        }

        public FiatType FiatType
        {
            get
            {
                return _fiatType;
            }
            set { _fiatType = value; }
        }
        #endregion

        public void AddExchange(Type exchangeType)
        {
            //If _exchangeList has not already been initialized, do so.
            if (_exchangeList == null)
            {
                _exchangeList = new List<BaseExchange>();
            }

            if (exchangeType == typeof(Anx))
            {
                _useAnx = true;
                ExchangeList.Add(new Anx(FiatType));
            }

            else if (exchangeType == typeof(Bitfinex))
            {
                _useBitstamp = true;
                ExchangeList.Add(new Bitfinex(FiatType));
            }

            else if (exchangeType == typeof(Bitstamp))
            {
                _useBitstamp = true;
                ExchangeList.Add(new Bitstamp(FiatType));
            }

            else if (exchangeType == typeof(BitX))
            {
                _useBitstamp = true;
                ExchangeList.Add(new BitX(FiatType));
            }

            else if (exchangeType == typeof(Btce))
            {
                _useBtce = true;
                ExchangeList.Add(new Btce(FiatType));
            }

            else if (exchangeType == typeof(ItBit))
            {
                _useItBit = true;
                ExchangeList.Add(new ItBit(FiatType));
            }

            else if (exchangeType == typeof(Kraken))
            {
                _useKraken = true;
                ExchangeList.Add(new Kraken(FiatType));
            }

            else if (exchangeType == typeof (OkCoin))
            {
                _useOkCoin = true;
                ExchangeList.Add(new OkCoin(FiatType));
            }

            else if (exchangeType == typeof (Coinbase))
            {
                _useCoinbase = true;
                ExchangeList.Add(new Coinbase(FiatType));
            }

            else
            {
                throw new Exception("Unspported type.");
            }
        }

        /// <summary>
        /// Sets EndDateTime to the current datetime and updates the 'END_DATETIME' column in the 
        /// db for this arbitration run. If Id is null (meaning this arbitration run hasn't been 
        /// persisited to the db yet), then this method sets EndDateTime and that's it.
        /// </summary>
        public void EndRun()
        {
            _endDateTime = DateTime.Now;

            if (_id != null)
            {
                string endDateTimeSqlString = DatabaseManager.FormatDateTimeForDb(_endDateTime);

                var sql = string.Format("" +
                    "UPDATE " +
                    "    ARBITRATION_RUN " +
                    "SET " +
                    "    END_DATETIME = {0} " +
                    "WHERE " +
                    "    ID = {1} ", endDateTimeSqlString, _id);

                DatabaseManager.ExecuteNonQuery(sql);
            }
        }

        /// <summary>
        /// Saves this arbitration run to the database. If this arbitration run does not have an id, then 
        /// an INSERT statement is used. Otherwise, the record in the ARBITRATION_RUN table with the corresponding id 
        /// is updated. This method throws any error that could occur when saving to the db.
        /// </summary>
        /// <returns>The id of this arbitration run in the ARBITRATION_RUN table. (If this is an update, id is unchanged).</returns>
        public int? PersistToDb()
        {
            //Whether this is an update or an insert, the last modified date time will need to be updated.
            _lastModifiedDateTime = DateTime.Now;

            string logFileNameSqlString = DatabaseManager.FormatStringForDb(_logFileName);
            string startDateTimeSqlString = DatabaseManager.FormatDateTimeForDb(_startDateTime);
            string endDateTimeSqlString = DatabaseManager.FormatDateTimeForDb(_endDateTime);
            string lastModifiedDateTimeSqlString = DatabaseManager.FormatDateTimeForDb(_lastModifiedDateTime);
            string opportunitySelectionMethodString = DatabaseManager.FormatStringForDb(_opportunitySelectionMethod.ToString());
            string arbitrationModeString = DatabaseManager.FormatStringForDb(_arbitrationMode.ToString());
            string transferModeString = DatabaseManager.FormatStringForDb(_transferMode.ToString());
            string rollupNumberString = DatabaseManager.FormatNullableIntegerForDb(_rollupNumber);
            string rollupHoursString = DatabaseManager.FormatNullableDecimalForDb(_rollupHours);
            string fiatTypeString = DatabaseManager.FormatStringForDb(_fiatType.ToString());
            string exchangeBaseCurrencyPercentageRestrictionString = DatabaseManager.FormatNullableDecimalForDb(_exchangeBaseCurrencyPercentageRestriction);

            if (_id == null)
            {
                //If we are inserting a new record to the db, set CREATE_DATETIME
                _createDateTime = DateTime.Now;
                string createDateTimeString = DatabaseManager.FormatDateTimeForDb(_createDateTime);

                string insertSql = String.Format("" +
                    "insert into ARBITRATION_RUN( " +
                    "    USE_ANX, " +
                    "    USE_BITFINEX, " +
                    "    USE_BITSTAMP, " +
                    "    USE_BITX, " +
                    "    USE_BTCE, " +
                    "    USE_COINBASE, " +
                    "    USE_ITBIT, " +
                    "    USE_KRAKEN, " +
                    "    USE_OKCOIN, " +
                    "    MINIMUM_PROFIT_FOR_TRADE, " +
                    "    SEARCH_INTERVAL_MILLISECONDS, " +
                    "    ROUNDS_REQUIRED_FOR_VALIDATION, " +
                    "    MAX_BTC_FOR_TRADE, " +
                    "    MAX_FIAT_FOR_TRADE, " +
                    "    FIAT_TYPE, " + 
                    "    OPPORTUNITY_SELECTION_METHOD, " +
                    "    MODE, " + 
                    "    TRANSFER_MODE, " + 
                    "    ROLLUP_TRADE_NUMBER, " +
                    "    ROLLUP_HOURS, " +
                    "    EXCHANGE_BASE_CURRENCY_PERCENTAGE_RESTRICTION, " +
                    "    LOG_FILE, " +
                    "    START_DATETIME, " +
                    "    END_DATETIME, " +
                    "    CREATE_DATETIME, " +
                    "    LAST_MODIFIED_DATETIME)" +
                    "OUTPUT INSERTED.ID VALUES( " +
                    "    {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}" +
                    ")", _useAnx ? 1 : 0, _useBitfinex ? 1 : 0, _useBitstamp ? 1 : 0, _useBitX ? 1 : 0, _useBtce ? 1 : 0, _useCoinbase ? 1 : 0, _useItBit ? 1 : 0, _useKraken ? 1 : 0, _useOkCoin ? 1 : 0, _minimumProfit, _searchIntervalMilliseconds, _roundsRequiredForValidation, _maxBtcTradeAmount, _maxFiatTradeAmount, fiatTypeString, opportunitySelectionMethodString, arbitrationModeString, transferModeString, rollupNumberString, rollupHoursString, exchangeBaseCurrencyPercentageRestrictionString, logFileNameSqlString, startDateTimeSqlString, endDateTimeSqlString, createDateTimeString, lastModifiedDateTimeSqlString);

                _id = DatabaseManager.ExecuteInsert(insertSql);
            }

            else
            {
                string updateSql = String.Format("" +
                    "UPDATE " +
                    "    ARBITRATION_RUN " +
                    "SET " +
                    "    USE_ANX = {0}, " +
                    "    USE_BITFINEX = {1}, " +
                    "    USE_BITSTAMP = {2}, " +
                    "    USE_BITX = {3}, " +
                    "    USE_BTCE = {4}, " +
                    "    USE_COINBASE = {5}, " +
                    "    USE_ITBIT = {6}, " +
                    "    USE_KRAKEN = {7}, " +
                    "    USE_OKCOIN = {8}, " +
                    "    MINIMUM_PROFIT_FOR_TRADE = {9}, " +
                    "    SEARCH_INTERVAL_MILLISECONDS = {10}, " +
                    "    MAX_BTC_FOR_TRADE = {11}, " +
                    "    LOG_FILE = {12}, " +
                    "    start_datetime = {13}, " +
                    "    end_datetime = {14}, " +
                    "    LAST_MODIFIED_DATETIME = {15}, " +
                    "    MAX_FIAT_FOR_TRADE = {16}, " +
                    "    OPPORTUNITY_SELECTION_METHOD = {17}, " +
                    "    MODE = {18}, " +
                    "    EXCHANGE_BASE_CURRENCY_PERCENTAGE_RESTRICTION = {19}, " + 
                    "    TRANSFER_MODE = {20}, " + 
                    "    ROLLUP_TRADE_NUMBER = {21}, " +
                    "    ROLLUP_HOURS = {22}, " +
                    "    ROUNDS_REQUIRED_FOR_VALIDATION = {23}, " +
                    "    FIAT_TYPE = {24} " + 
                    "WHERE " +
                    "    ID = {25} ", _useAnx ? 1 : 0, _useBitfinex ? 1 : 0, _useBitstamp ? 1 : 0, _useBitX ? 1 : 0, _useBtce ? 1 : 0, _useCoinbase ? 1 : 0, _useItBit ? 1 : 0, _useKraken ? 1 : 0, _useOkCoin ? 1 : 0, _minimumProfit, _searchIntervalMilliseconds, _maxBtcTradeAmount, logFileNameSqlString, startDateTimeSqlString, endDateTimeSqlString, lastModifiedDateTimeSqlString, _maxFiatTradeAmount, opportunitySelectionMethodString, arbitrationModeString, exchangeBaseCurrencyPercentageRestrictionString, transferModeString, rollupNumberString, rollupHoursString, _roundsRequiredForValidation, fiatTypeString, _id.Value);

                DatabaseManager.ExecuteNonQuery(updateSql);
            }

            return _id;
        }

        public static ArbitrationRun GetArbitrationRunFromDbById(int arbitrationRunId)
        {
            DataTable results = DatabaseManager.ExecuteQuery("SELECT * from ARBITRATION_RUN where ID = " + arbitrationRunId);
            ArbitrationRun returnRun = null;

            if (results != null)
            {
                returnRun = new ArbitrationRun();
                
                returnRun.Id = (int)results.Rows[0]["ID"];
                returnRun.UseAnx = (bool)results.Rows[0]["USE_ANX"];
                returnRun.UseBitfinex = (bool)results.Rows[0]["USE_BITFINEX"];
                returnRun.UseBitstamp = (bool)results.Rows[0]["USE_BITSTAMP"];
                returnRun.UseBitX = (bool)results.Rows[0]["USE_BITX"];
                returnRun.UseBtce = (bool)results.Rows[0]["USE_BTCE"];
                returnRun.UseCoinbase = (bool)results.Rows[0]["USE_COINBASE"];
                returnRun.UseItBit = (bool)results.Rows[0]["USE_ITBIT"];
                returnRun.UseKraken = (bool)results.Rows[0]["USE_KRAKEN"];
                returnRun.UseOkCoin = (bool)results.Rows[0]["USE_OKCOIN"];
                returnRun.MinimumProfit = Convert.ToDecimal(results.Rows[0]["MINIMUM_PROFIT_FOR_TRADE"]);
                returnRun.SeachIntervalMilliseconds = Convert.ToInt16(results.Rows[0]["SEARCH_INTERVAL_MILLISECONDS"]);
                returnRun.RoundsRequiredForValidation = Convert.ToInt16(results.Rows[0]["ROUNDS_REQUIRED_FOR_VALIDATION"]);
                returnRun.MaxBtcTradeAmount = Convert.ToDecimal(results.Rows[0]["MAX_BTC_FOR_TRADE"]);
                returnRun.MaxFiatTradeAmount = Convert.ToDecimal(results.Rows[0]["MAX_FIAT_FOR_TRADE"]);
                returnRun.FiatType = (FiatType)Enum.Parse(typeof(FiatType), (string)results.Rows[0]["FIAT_TYPE"]);
                returnRun.OpportunitySelectionMethod = (OpportunitySelectionType)Enum.Parse(typeof(OpportunitySelectionType), (string)results.Rows[0]["OPPORTUNITY_SELECTION_METHOD"]);
                if (results.Rows[0]["EXCHANGE_BASE_CURRENCY_PERCENTAGE_RESTRICTION"] == DBNull.Value) { returnRun.ExchangeBaseCurrencyPercentageRestriction = null; } else { returnRun.ExchangeBaseCurrencyPercentageRestriction = Convert.ToDecimal(results.Rows[0]["EXCHANGE_BASE_CURRENCY_PERCENTAGE_RESTRICTION"]); }
                returnRun.LogFileName = (string)results.Rows[0]["LOG_FILE"];
                returnRun.ArbitrationMode = (ArbitrationMode)Enum.Parse(typeof(ArbitrationMode), (string)results.Rows[0]["MODE"]);
                returnRun.TransferMode = (TransferMode)Enum.Parse(typeof(TransferMode), (string)results.Rows[0]["TRANSFER_MODE"]);
                if (results.Rows[0]["ROLLUP_TRADE_NUMBER"] == DBNull.Value) { returnRun.RollupNumber = null; } else { returnRun.RollupNumber = Convert.ToInt16(results.Rows[0]["ROLLUP_TRADE_NUMBER"]); }
                if (results.Rows[0]["ROLLUP_HOURS"] == DBNull.Value) { returnRun.RollupHours = null; } else { returnRun.RollupHours = Convert.ToInt16(results.Rows[0]["ROLLUP_HOURS"]); }
                returnRun.StartDateTime = (DateTime?)results.Rows[0]["START_DATETIME"];
                returnRun.EndDateTime = (DateTime?)results.Rows[0]["END_DATETIME"];
                returnRun.CreateDateTime = (DateTime?)results.Rows[0]["CREATE_DATETIME"];
                returnRun.LastModifiedDateTime = (DateTime?)results.Rows[0]["LAST_MODIFIED_DATETIME"];
            }

            return returnRun;
        }

        /// <summary>
        /// If this ArbitrationRun has a log file name in _logFileName, creates the log file and puts a header in.
        /// If the file already exists, it is overwritten. If the file is being used by another process, an IOException is thrown.
        /// </summary>
        private void InitializeLogFile()
        {
            if (_logFileName != null)
            {
                FileStream fileWriter = new FileStream(_logFileName, FileMode.Create, FileAccess.Write, FileShare.Read);

                using (StreamWriter steamWriter = new StreamWriter(fileWriter))
                {
                    steamWriter.WriteLine("Time, Buy Exchange, Sell Exchange, Buy Price, Sell Price, Amount, Profit, Buy Exchange Ask List, Sell Exchange Bid List");
                }

                fileWriter.Dispose();
            }
        }
    }
}
