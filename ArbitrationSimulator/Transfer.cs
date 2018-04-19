using System;
using BitcoinExchanges;
using DatabaseLayer;

namespace ArbitrationSimulator
{
    /// <summary>
    /// Class to represent the transfer of coins from one exchange to another.
    /// </summary>
    public class Transfer : IDomainObject
    {
        private int? _id;
        private bool _completed;
        private decimal _amount;
        private BaseExchange _originExchange;
        private BaseExchange _destinationExchange;
        private DateTime? _initiateDateTime;
        private DateTime? _completeDateTime;
        private DateTime? _createDateTime;
        private DateTime? _lastModifiedDateTime;

        #region Property Getters and Setters

        /// <summary>
        /// Id of this transfer object in the TRANSFER table in the db. If this transfer object has
        /// not yet been saved to the database, then Id is null.
        /// </summary>
        public int? Id
        {
            get { 
                return _id; 
            }
            set{ 
                _id = value; 
            }
        }

        public bool Completed
        {
            get
            {
                return _completed;
            }

            set
            {
                _completed = value;
            }
        }

        /// <summary>
        /// Amount of coins transfered from the 'from exchange' to the 'to exchange.'
        /// </summary>
        public decimal Amount
        {
            get
            {
                return _amount;
            }
            set
            {
                _amount = value;
            }
        }

        /// <summary>
        /// The time ths transfer was initiated.
        /// </summary>
        public DateTime? InitiateDateTime
        {
            get
            {
                return _initiateDateTime;
            }

            set
            {
                _initiateDateTime = value;
            }
        }

        /// <summary>
        /// The time this transfer will completed.
        /// </summary>
        public DateTime? CompleteDateTime
        {
            get
            {
                return _completeDateTime;
            }

            set
            {
                _completeDateTime = value;
            }
        }
        
        /// <summary>
        /// The exchange that coins are being removed from.
        /// </summary>
        public BaseExchange OriginExchange
        {
            get
            {
                return _originExchange;
            }

            set
            {
                _originExchange = value;
            }
        }

        /// <summary>
        /// The exchange that coins are being sent to.
        /// </summary>
        public BaseExchange DestinationExchange
        {
            get
            {
                return _destinationExchange;
            }

            set
            {
                _destinationExchange = value;
            }
        }

        /// <summary>
        /// Time this arbitration trade was put into the db.
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
        /// Time this arbitration trade was last updated.
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
        #endregion

        #region Constructors

        public Transfer() {}

        public Transfer(BaseExchange originExchange, BaseExchange destinationExchange, decimal amount)
        {
            _originExchange = originExchange;
            _destinationExchange = destinationExchange;
            _amount = amount;

            //Initialize completed to false
            _completed = false;
        }

        #endregion

        /// <summary>
        /// Saves this Transfer to the database. If this transfer does not have an id, then an INSERT statement is used. Otherwise, 
        /// the record in the TRANSFER with the corresponding id is updated. This method throws any that could occur when 
        /// saving to the db.
        /// </summary>
        /// <returns>The id of this transfer in the TRANSFER table. (If this is an update, id is unchanged).</returns>
        public int? PersistToDb()
        {
            string insertSql;
            string updateSql;

            //Whether this is an update or an insert, the last modified date time will need to be updated.
            _lastModifiedDateTime = DateTime.Now;

            string originExchangeNameSqlString = DatabaseManager.FormatStringForDb(_originExchange.Name);
            string destinationExchangeNameSqlString = DatabaseManager.FormatStringForDb(_destinationExchange.Name);
            string initiateDateTimeString = DatabaseManager.FormatDateTimeForDb(_initiateDateTime);
            string completeDateTimeString = DatabaseManager.FormatDateTimeForDb(_completeDateTime);
            string lastModifiedDateTimeString = DatabaseManager.FormatDateTimeForDb(_lastModifiedDateTime);
            string completedString = "'" + _completed + "'";

            if (_id == null)
            {
                //If we are inserting a new record to the db, set CREATE_DATETIME
                _createDateTime = DateTime.Now;
                string createDateTimeString = DatabaseManager.FormatDateTimeForDb(_createDateTime);

                insertSql = String.Format("" +
                    "INSERT INTO TRANSFER( " +
                    "    ORIGIN_EXCHANGE, " +
                    "    DESTINATION_EXCHANGE, " +
                    "    AMOUNT, " +
                    "    INITIATE_DATETIME, " +
                    "    COMPLETE_DATETIME, " +
                    "    COMPLETED," +
                    "    CREATE_DATETIME, " +
                    "    LAST_MODIFIED_DATETIME " +
                    ") " +
                    "OUTPUT INSERTED.ID " +
                    "values( " +
                    "    {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}" +
                    ") ", originExchangeNameSqlString, destinationExchangeNameSqlString, _amount, initiateDateTimeString, completeDateTimeString, completedString, createDateTimeString, lastModifiedDateTimeString);

                _id = DatabaseManager.ExecuteInsert(insertSql);
            }

            else
            {
                updateSql = String.Format("" +
                    "UPDATE " +
                    "   TRANSFER " +
                    "SET " +
                    "    ORIGIN_EXCHANGE = {0}, " +
                    "    DESTINATION_EXCHANGE = {1}, " +
                    "    AMOUNT = {2}, " +
                    "    INITIATE_DATETIME = {3}, " +
                    "    COMPLETED = {4}, " +
                    "    COMPLETE_DATETIME = {5}, " +
                    "    LAST_MODIFIED_DATETIME = {6} " +
                    "WHERE " +
                    "    ID = {7} ", originExchangeNameSqlString, destinationExchangeNameSqlString, _amount, initiateDateTimeString, completedString, completeDateTimeString, lastModifiedDateTimeString, _id.Value);

                DatabaseManager.ExecuteNonQuery(updateSql);
            }

            return _id;
        }
    }
}
