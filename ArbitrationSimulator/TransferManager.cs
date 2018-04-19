using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ArbitrationSimulator.Exceptions;
using BitcoinExchanges;
using CommonFunctions;
using DatabaseLayer;
using log4net;

namespace ArbitrationSimulator
{
    public static class TransferManager
    {
        //Number of simulated minutes it takes for a transfer to complete.
        const int TRANSFER_TIME_MINUTES = 60;
        private const string LOG_MESSAGE = "Executed transfer {0}: Sent {1} btc from {2} to {3}.";

	    /// <summary>
	    /// Simulates a transfer of btc between the two exchanges. The coin amounts of the two exchanges are updated appropriately.
	    /// This method  throws an ArgumentException is Amount is greater than the AvailableBtc of the given origin exchange,
	    /// the given exchanges are null, or Amount is less than or equal to 0.
	    /// </summary>
	    /// <param name="originExchange">The exchange coins will be moved out of.</param>
	    /// <param name="destinationExchange">The exchange coins will be sent to.</param>
	    /// <param name="amount">The number of bitcoins to be exchanged between the two giving exchanges.</param>
	    /// <returns>A  transfer object representing the transfer that took place. This method saves the transfer
	    ///     object to the DB before it is returned.</returns>
        public static Transfer OnTimeTransfer_Simulate(BaseExchange originExchange, BaseExchange destinationExchange, decimal amount, ILog log = null)
        {
            Transfer returnTransfer = ValidateInputsAndBuildTransferObject(originExchange, destinationExchange, amount);

            //Simulate the transfer by updatjing the appropriate exchange amounts, making sure to take into account transfer fees
            originExchange.AvailableBtc = originExchange.AvailableBtc - amount;
            destinationExchange.BTCInTransfer = destinationExchange.BTCInTransfer + amount - originExchange.BtcTransferFee;

            //Save the transfer object to the DB before returning
            returnTransfer.PersistToDb();

            //If a log was given, create a log message
            if (log != null)
            {
                log.Info(String.Format(LOG_MESSAGE, returnTransfer.Id.Value, returnTransfer.Amount, returnTransfer.OriginExchange.Name, returnTransfer.DestinationExchange.Name));
            }
            
            return returnTransfer;
        }

        /// <summary>
        /// Transfers coins between the given exchanges. Coin amounts are not updated.
        /// This method  throws an ArgumentException is Amount is greater than the AvailableBtc of the given origin exchange,
        /// the given exchanges are null, or Amount is less than or equal to 0.
        /// </summary>
        /// <param name="originExchange">The exchange coins will be moved out of.</param>
        /// <param name="destinationExchange">The exchange coins will be sent to.</param>
        /// <param name="amount">The number of bitcoins to be exchanged between the two giving exchanges.</param>
        /// <returns>A  transfer object representing the transfer that took place. This method saves the transfer
        ///     object to the DB before it is returned.</returns>
        public static Transfer OnTimeTransfer_Live(BaseExchange originExchange, BaseExchange destinationExchange, decimal amount, ILog log = null)
        {
            Transfer returnTransfer = ValidateInputsAndBuildTransferObject(originExchange, destinationExchange, amount);

            originExchange.Transfer(amount, destinationExchange.BitcoinDepositAddress);
            
            //Save the transfer object to the DB before returning
            returnTransfer.PersistToDb();

            //If a log was given, create a log message
            if (log != null)
            {
                log.Info(String.Format(LOG_MESSAGE, returnTransfer.Id.Value, returnTransfer.Amount, returnTransfer.OriginExchange.Name, returnTransfer.DestinationExchange.Name));
            }

            return returnTransfer;
        }

        public static List<Transfer> DetectAndExecuteRollupTransfers_Simulate(int rollupNumber, int arbitrationRunId, List<BaseExchange> exchanges, ILog log = null)
        {
            DataTable transfersToExecute = CalculateRollupTransfers(rollupNumber, arbitrationRunId);
            List<Transfer> transfers = null;

            //If there were transfers found that need to be executed.
            if (transfersToExecute != null)
            {
                foreach (DataRow transferRow in transfersToExecute.Rows)
                {
                    BaseExchange originExchange = GetExchangeFromListByName((string)transferRow["BUY_EXCHANGE"], exchanges);
                    BaseExchange destinationExchange = GetExchangeFromListByName((string)transferRow["SELL_EXCHANGE"], exchanges);
                    decimal amount = TypeConversion.ParseStringToDecimalStrict(transferRow["AMOUNT"].ToString());

                    try
                    {
                        //This might throw a NotEnoughBtcException
                        Transfer transfer = ValidateInputsAndBuildTransferObject(originExchange, destinationExchange, amount);

                        //Setting transfer list here, because at least one transfer was found and properly built
                        if (transfers == null)
                        {
                            transfers = new List<Transfer>();
                        }

                        //Simulate the transfer by updatjing the appropriate exchange amounts, making sure to take into account transfer fees
                        originExchange.AvailableBtc = originExchange.AvailableBtc - amount;
                        destinationExchange.BTCInTransfer = destinationExchange.BTCInTransfer + amount - originExchange.BtcTransferFee;

                        //Save the transfer object to the DB before returning
                        transfer.PersistToDb();

                        //If a log was given, create a log message
                        if (log != null)
                        {
                            log.Info(String.Format(LOG_MESSAGE, transfer.Id.Value, transfer.Amount, transfer.OriginExchange.Name, transfer.DestinationExchange.Name));
                        }

                        //Update all the necessary arbitration trades with the transfer id
                        UpdateArbitrationTradesWithTransferId(arbitrationRunId, transfer.OriginExchange.Name, transfer.DestinationExchange.Name, transfer.Id.Value);

                        transfers.Add(transfer);
                    }
                    catch (NotEnoughBtcException e)
                    {
                        //This may or may not be a valid error. If there is not enough btc for the transfer, but there is btc in transfer, then it's ok, the transfers
                        //just need to get caught up. In this case, do nothing; just need to wait until the transfer complete and try again. But, if there is still not 
                        //enough btc while accounting for the amount in transfer, this is a legitmate error, so rethrow.
                        if (!(Decimal.Add(originExchange.AvailableBtc, originExchange.BTCInTransfer) >= amount))
                        {
                            throw;
                        }
                    }
                }
            }

            return transfers;
        }

        public static List<Transfer> DetectAndExecuteRollupTransfers_Live(int rollupNumber, int arbitrationRunId, List<BaseExchange> exchanges, ILog log = null)
        {
            DataTable transfersToExecute = CalculateRollupTransfers(rollupNumber, arbitrationRunId);
            List<Transfer> transfers = null;

            //If there were transfers found that need to be executed.
            if (transfersToExecute != null)
            {
                foreach (DataRow transferRow in transfersToExecute.Rows)
                {
                    BaseExchange originExchange = GetExchangeFromListByName((string)transferRow["BUY_EXCHANGE"], exchanges);
                    BaseExchange destinationExchange = GetExchangeFromListByName((string)transferRow["SELL_EXCHANGE"], exchanges);
                    decimal amount = (decimal)transferRow["AMOUNT"];

                    //Validate inputs Create the transfer object
                    try
                    {
                        //This might throw a NotEnoughBtcException
                        Transfer transfer = ValidateInputsAndBuildTransferObject(originExchange, destinationExchange, amount);

                        //Setting transfer list here, because at least one transfer was found and properly built
                        if (transfers == null)
                        {
                            transfers = new List<Transfer>();
                        }

                        //Execute the actual transfer
                        originExchange.Transfer(amount, destinationExchange.BitcoinDepositAddress);

                        //Save the transfer object to the DB before returning
                        transfer.PersistToDb();

                        //If a log was given, create a log message
                        if (log != null)
                        {
                            log.Info(String.Format(LOG_MESSAGE, transfer.Id.Value, transfer.Amount, transfer.OriginExchange.Name, transfer.DestinationExchange.Name));
                        }

                        //Update all the necessary arbitration trades with the transfer id
                        UpdateArbitrationTradesWithTransferId(arbitrationRunId, transfer.OriginExchange.Name, transfer.DestinationExchange.Name, transfer.Id.Value);

                        transfers.Add(transfer);
                    }
                    catch (NotEnoughBtcException)
                    {
                        //This may or may not be a valid error. If there is not enough btc for the transfer, but there is btc in transfer, then it's ok, the transfers
                        //just need to get caught up. In this case, do nothing; just need to wait until the transfer complete and try again. But, if there is still not 
                        //enough btc while accounting for the amount in transfer, this is a legitmate error, so rethrow.
                        if (!(Decimal.Add(originExchange.AvailableBtc, originExchange.BTCInTransfer) >= amount))
                        {
                            throw;
                        }
                    }
                }
            }

            return transfers;
        }

        private static DataTable CalculateRollupTransfers(int rollupNumber, int arbitrationRunId)
        {
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@ROLL_UP_NUMBER", rollupNumber);
            parameters[1] = new SqlParameter("@ARBITRATION_RUN_ID", arbitrationRunId);
            return DatabaseManager.ExecuteStoredProcedure("CALCULATE_ROLL_UP_TRANSFERS", parameters);
        }

        private static void UpdateArbitrationTradesWithTransferId(int arbitrationRunId, string originExchange, string destinationExchange, int transferId)
        {
            SqlParameter[] parameters = new SqlParameter[4];
            parameters[0] = new SqlParameter("@ORIGIN_EXCHANGE", originExchange);
            parameters[1] = new SqlParameter("@DESTINATION_EXCHANGE", destinationExchange);
            parameters[2] = new SqlParameter("@ARBITRATION_RUN_ID", arbitrationRunId);
            parameters[3] = new SqlParameter("@TRANSFER_ID", transferId);

            DatabaseManager.ExecuteStoredProcedure("UPDATE_ARBITRATION_TRADES_WITH_ROLL_UP_TRANSFER_ID", parameters);
        }

        /// <summary>
        /// Private helper method that retreives an exchange from a list of exchanges based upon the given name.
        /// </summary>
        /// <param name="exchangeName">Name of the exchange to return.</param>
        /// <param name="exchanges">List of exchanges to return.</param>
        /// <returns>The exchange object from the list with the matching name. If no such exchange is found,
        ///     an error is thrown.</returns>
        private static BaseExchange GetExchangeFromListByName(string exchangeName, List<BaseExchange> exchanges)
        {
            if (string.IsNullOrWhiteSpace(exchangeName) == false && exchanges.Count > 0)
            {
                foreach (BaseExchange exchange in exchanges)
                {
                    if (exchange.Name.Equals(exchangeName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return exchange;
                    }
                }
            }

            throw new Exception("Could not find exchange '" + exchangeName + "' in the given list.");
        }

        /// <summary>
        /// Private helper method that validates inputs given for a transfer and builds/intiates a new transfer object.
        /// </summary>
        /// <param name="originExchange"></param>
        /// <param name="destinationExchange"></param>
        /// <param name="amount"></param>
        /// <param name="arbitrationTradeId"></param>
        /// <returns></returns>
        private static Transfer ValidateInputsAndBuildTransferObject(BaseExchange originExchange, BaseExchange destinationExchange, decimal amount)
        {
            DateTime currentDateTime = DateTime.Now;

            if (originExchange == null)
            {
                throw new ArgumentException("OriginExchange cannot be null.");
            }

            if (destinationExchange == null)
            {
                throw new ArgumentException("DestinationExchange cannot be null.");
            }

            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0.");
            }

            //Make sure there is enough bitcoins in the OriginExchange to execute the transfer
            if (originExchange.AvailableBtc < amount)
            {
                throw new NotEnoughBtcException("The exchange " + originExchange.Name + " does not have enough bitcoins (" + originExchange.AvailableBtc + ") to execute a transfer of " + amount + ".");
            }

            Transfer returnTransfer = new Transfer(originExchange, destinationExchange, amount);

            //Set up times for this transfer
            returnTransfer.InitiateDateTime = currentDateTime;
            returnTransfer.CompleteDateTime = currentDateTime.AddMinutes(TRANSFER_TIME_MINUTES);   

            return returnTransfer;
        }

        /// <summary>
        /// Given a list of transfers, loops through the list and completes any transfer where the completed date time is before the 
        /// current date time. In such cases, the transfers are marked as completed, saved to the db, and the destination exchange
        /// BTC balance and BTC in transfer amounts are updated. That transfer is also removed from the given transfer list. If the 
        /// given transfer list is null or empty, this method does nothing.
        /// </summary>
        /// <param name="transferList">The list of transfers to be completed.</param>
        public static void CompleteTransfers(List<Transfer> transferList)
        {
            //List of indices of the transfers to be removed.
            List<Transfer> transfersToRemove = new List<Transfer>();
            DateTime currentDateTime = DateTime.Now;

            if (transferList != null && transferList.Count > 0)
            {
                for (int counter = 0; counter < transferList.Count; counter++)
                {
                    Transfer transfer = transferList[counter];

                    if (transfer.CompleteDateTime < currentDateTime)
                    {
                        //Calcuale the amount which actually needs to be moved, which is transfer.Amount - transfer.OriginExchange.BtcTransferFee
                        //Transferfee has already been taken into account when the transfer was simulated
                        decimal moveAmount = transfer.Amount - transfer.OriginExchange.BtcTransferFee;

                        //Move over the amount - transfer fee (which is the amount which has been placed into this field) 
                        transfer.DestinationExchange.BTCInTransfer = transfer.DestinationExchange.BTCInTransfer - moveAmount;
                        transfer.DestinationExchange.AvailableBtc = transfer.DestinationExchange.AvailableBtc + moveAmount;
                        transfer.Completed = true;
                        transfer.PersistToDb();

                        transfersToRemove.Add(transfer);
                    }
                }
            }

            RemoveCompletedTransfers(transfersToRemove, transferList);
        }

	    /// <summary>
	    /// Helper method that removes tranfers from a list of transfers. The given transfer list is itself modified, so no new list
	    /// is returned.
	    /// </summary>
	    /// <param name="transfersToRemove">Transfer to remove from the given transfer list.</param>
	    /// <param name="transferList">The list of transfers to be removed from.</param>
	    private static void RemoveCompletedTransfers(List<Transfer> transfersToRemove, List<Transfer> transferList)
        {
            foreach (Transfer transferToBeRemoved in transfersToRemove)
            {
                transferList.Remove(transferToBeRemoved);
            }
        }
    }
}
