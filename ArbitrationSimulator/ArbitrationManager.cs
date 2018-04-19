using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using ArbitrationUtilities.EnumerationObjects;
using ArbitrationSimulator.Exceptions;
using CommonFunctions;
using DatabaseLayer;
using BitcoinExchanges;
using log4net;
using Timer = System.Timers.Timer;

namespace ArbitrationSimulator
{
    class ArbitrationManager
    {
        #region Class Variables

        public const string LOG_MESSAGE = "Executed transfer {0}: Sent {1} btc from {2} to {3}.";
        public event ArbitrationManagerFailureEventHandler ErrorOccured;
        public delegate void ArbitrationManagerFailureEventHandler(object sender, ArbitrationManagerFailureEventArgs args);

		private ArbitrationHunter _hunter;
        private OpportunityValidator _opportunityValidator;
        private TextBox _outputTextBox;
        private DataGridView _tradeDataGridView;
        private DataGridView _exchangeDataGridView;
        private Timer _timer;
        private bool _stopped;
        private ArbitrationRun _currentRun;
        private List<Transfer> _transfersInProcess;
        private DateTime _lastTransferRollup = DateTime.Now;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ArbitrationOpportunity _previousOpportunity = null;

        #endregion

        #region Property Getters and Setters

        public ArbitrationRun CurrentRun
        {
            get
            {
                return _currentRun;
            }

            set
            {
                _currentRun = value;

                //Since this aribtration manager has a new list of exchanges, clear out the information in the exchange dgv and
                //update the balances for the new accounts
                _exchangeDataGridView.Invoke(new Action(delegate() { _exchangeDataGridView.Rows.Clear(); }));

                //Update arbitration hunter with the new exchange list
                _hunter.ExchangeList = _currentRun.ExchangeList;
            }
        }

        #endregion

        #region Constructors
        
        public ArbitrationManager(TextBox tradeTextBox, DataGridView exchangeDataGridView, DataGridView tradeDataGridView)
		{
            _outputTextBox = tradeTextBox;
            _exchangeDataGridView = exchangeDataGridView;
            _tradeDataGridView = tradeDataGridView;
                    
            //Set up the timer
            _timer = new Timer();
            _timer.Elapsed += IntervalElapsed;

            //Test the db connection; if the connection string is invalid this will thrown an error.
            DatabaseManager.TestDbConnection();

            _hunter = new ArbitrationHunter(null, log);
            _transfersInProcess = new List<Transfer>();

            //No idea why this line is needed; it just is to properly connect to Kraken, and perhaps other exchanges. So put this here.
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
		}

        #endregion

        #region Public Methods
        
        public void Start(ArbitrationRun givenRun)
        {
            //Set property because some special logic needs to happen on the set
            CurrentRun = givenRun;

            //Now that the run is officially starting, set the start time if it has not already been set
            //(continued runs keep old start time
            if (_currentRun.StartDateTime == null)
            {
                _currentRun.StartDateTime = DateTime.Now;    
            }
            
            //Set the validator
            _opportunityValidator = new OpportunityValidator(givenRun.RoundsRequiredForValidation, CurrentRun.ExchangeList);

            //Need to reset previousOpportunity since this is a new run; don't validate opportunities from previous runs
            _previousOpportunity = null;

            try
            {
                _currentRun.PersistToDb();

                log.Info("Arbitration hunting started. Arbitration run ID = " + givenRun.Id + ".");

                SetExchangeTradeFee();
                SetInitialAccountBalances();                
                DisplayExchangeBalances();

                _timer.Interval = _currentRun.SeachIntervalMilliseconds;
                _timer.Enabled = true;
                _stopped = false;
            }

            //Catch any kind of exception here that could occur from updating the exchange balances. A problem can manifest itself several ways depending on the exchange, so this needs to be a catch all.
            catch (Exception exception)
            {
                OnError("There was a problem updating one of the exchange balances.", exception, "There was a problem getting the account balance for one of the exchanges: ");
            }

		}

        public void Stop()
        {
            _timer.Enabled = false;
            _stopped = true;

            if (_currentRun != null)
            {
                try
                {
                    _currentRun.EndRun();
                    log.Info("Arbitration hunting stopped. Arbitration run ID = " + _currentRun.Id + ".");
                }

                catch (Exception exception)
                {
                    OnError("There was a problem ending the arbitration run: ", exception);
                }
            }
        }

        public bool CompleteTransfersFromPreviousRun(int arbitrationRunId)
        {
            //Get arbitration run record from the db:
            ArbitrationRun previousRun = ArbitrationRun.GetArbitrationRunFromDbById(arbitrationRunId);

            //Ensure the given was in live mode, as this is to poin to completing transfers from a simulated run
            if (previousRun.ArbitrationMode == ArbitrationMode.Simulation)
            {
                ArbitrationManagerFailureEventHandler handler = ErrorOccured;

                //Only  throw the event is there is a listener 
                if (handler != null)
                {
                    handler(this, new ArbitrationManagerFailureEventArgs("Arbitration run " + arbitrationRunId + " was ran in simulation mode; no point in completing transfers for it."));
                    return false;
                }
            }   

            //First need to build up the exchange list, as it is needed by the transfer manager.
            List<BaseExchange> previousRunExchangeList = BuildExchangeListFromPreviousRun(previousRun);

            TransferManager.DetectAndExecuteRollupTransfers_Live(1, previousRun.Id.Value, previousRunExchangeList);

            return true;
        }
        #endregion

        #region Private Methods

        private List<BaseExchange> BuildExchangeListFromPreviousRun(ArbitrationRun 
            arbitrationRun)
        {
            List<BaseExchange> exchangeList = new List<BaseExchange>();

            if (arbitrationRun.UseAnx) { exchangeList.Add(new Anx()); }
            if (arbitrationRun.UseBitfinex) { exchangeList.Add(new Bitfinex()); }
            if (arbitrationRun.UseBitstamp) { exchangeList.Add(new Bitstamp()); }
            if (arbitrationRun.UseBitX) {exchangeList.Add(new BitX());}
            if (arbitrationRun.UseBtce) { exchangeList.Add(new Btce()); }
            if (arbitrationRun.UseCoinbase) {exchangeList.Add(new Coinbase());}
            if (arbitrationRun.UseItBit) { exchangeList.Add(new ItBit()); }
            if (arbitrationRun.UseKraken) { exchangeList.Add(new Kraken()); }
            if (arbitrationRun.UseOkCoin) { exchangeList.Add(new OkCoin()); }
            
            return exchangeList;
        }

        private void IntervalElapsed(Object source, ElapsedEventArgs e)
        {
            ArbitrationOpportunity opportunityToExecute = null;
            List<ArbitrationOpportunity> opportunityList = null;

            //Timer needs to chill while arbitration is looked for.
            _timer.Enabled = false;

            //Update the balances first, to give time for orders that may have been initiated on the previous round to execute.
            //Only need to update account balances in live mode.
            if (_currentRun.ArbitrationMode == ArbitrationMode.Live)
            {
                UpdateExchangeAccountInfo();
            }

            DisplayExchangeBalances();
            
            try
            {
                //Before looking for another arbitration opportunity, see if the previous one was executed correctly.
                if (_previousOpportunity != null)
                {
                    //Verify the arbitration trade executed as expected.
                    try
                    {
                        //See if the orders filled (only necesary in live mode)
                        if (CurrentRun.ArbitrationMode == ArbitrationMode.Live)
                        {
                            _opportunityValidator.ValidateArbitrationTradeOrderExecution(_previousOpportunity);   
                        }
                        
                        //Compare the balances - this will detect if realized profit is different than computed profit, which would indicate there is a serious problem somewhere
                        log.Info(_opportunityValidator.ValidateExchangeBalancesAfterTrade(_previousOpportunity));
                    }
                    catch (ArbitrationTradeValidationException error)
                    {
                        LogAndDisplayString(error.Message, LogLevel.Warning);
                        log.Info(error.ExchangeBalanceDetailMessage);
                    }
                }

                //Reset _previousOpportunity, so it is not verified again
                _previousOpportunity = null;
                
                //Prepare the opportunity validator so it can compare balances before and after a trade
                _opportunityValidator.SetFiatAndBitcoinBalanceBeforeArbitrationTrade();

                try
                {
                    opportunityList = FindArbitrationOpportunities();

                    if (opportunityList != null && opportunityList.Count > 0)
                    {
                        opportunityToExecute = DetermineOpportunityToExecute(opportunityList);   
                    }
                }
                catch (Exception exception)
                {
                    log.Warn("There was a problem looking for arbitration: " + Environment.NewLine + exception.Message);
                    log.Debug(exception.StackTrace);
                }

                //If an opportunity was found, execute it
                if (opportunityToExecute != null)
                {
                    ExecuteArbitrationTrade(opportunityToExecute);

                    //Save the arbitration opportunity so it can be verified on the next round.
                    _previousOpportunity = opportunityToExecute;
                }

                //Do the logging and display after the fact, to reduce lage between order book get and order execution
                DisplayAndLogArbitrationOpportunityList(opportunityList);

                //Only need to keep track of transfers in simulation mode.
                if (_currentRun.ArbitrationMode == ArbitrationMode.Simulation)
                {
                    //Complete any transfers that are done processing.
                    TransferManager.CompleteTransfers(_transfersInProcess);
                }

                //Based on the transfer mode of the current run, process any transfers that are due.
                try
                {
                    if (_currentRun.TransferMode == TransferMode.RollupOnTrades)
                    {
                        TransferRollupOnTrades();
                    }

                    else if (_currentRun.TransferMode == TransferMode.RollupByHour)
                    {
                        TransferRollupByHours();
                    }

                    //Note, for transfer mode 'None,' don't need to do anything.
                }
                catch (Exception exception)
                {
                    OnError("There was a problem processing the transfers.", exception, "There was a problem processing the transfers: ");
                }

                //If this manager hasn't been stopped since the timer fired (which would be the case if there were a fatal error or the stop button was clicked), turn the timer back on.
                if (!_stopped)
                {
                    _timer.Enabled = true;
                }
                
            }
            catch (Exception exception)
            {   
                OnError("There was an error in this arbitration run, stopping the run.", exception, "Unexpected error with the arbitration run: ");
            }
        }

        private List<ArbitrationOpportunity> FindArbitrationOpportunities()
        {
            List<ArbitrationOpportunity> opportunityList;

            //If the transfer mode is 'OnTime,' need to take into account transfer cost when doing arbitration trade.
            if (_currentRun.TransferMode == TransferMode.OnTime)
            {
                opportunityList = _hunter.FindArbitration(_currentRun.MaxBtcTradeAmount, _currentRun.MaxFiatTradeAmount, _currentRun.MinimumProfit, true);
            }
            else
            {
                opportunityList = _hunter.FindArbitration(_currentRun.MaxBtcTradeAmount, _currentRun.MaxFiatTradeAmount, _currentRun.MinimumProfit, false);
            }

            if (opportunityList != null && opportunityList.Count > 0)
            {  
                return opportunityList;
            }

            return null;
        }

        private void DisplayAndLogArbitrationOpportunityList(List<ArbitrationOpportunity> opportunityList)
        {
            //List<ArbitrationOpportunity> shortenedList = new List<ArbitrationOpportunity>();
            //List<string> buyExchangeList = new List<string>();
            //List<string> sellExchangeList = new List<string>();

            if (opportunityList != null && opportunityList.Count > 0)
            {
                //Create list of arbitration opportunities to be displayed. This shortened list is not acutally used for calculation; just for displaying
                //This pulls the best arbitration trade from the arbitration list
                for (int counter = 0; counter < opportunityList.Count; counter++)
                {
                    LogArbitrationOpportunity(opportunityList[counter]);

                    //if (!buyExchangeList.Contains(opportunityList[counter].BuyExchange.Name) || !sellExchangeList.Contains(opportunityList[counter].SellExchange.Name))
                    //{
                    //    LogArbitrationOpportunity(opportunityList[counter]);
                    //    shortenedList.Add(opportunityList[counter]);

                    //    buyExchangeList.Add(opportunityList[counter].BuyExchange.Name);
                    //    sellExchangeList.Add(opportunityList[counter].SellExchange.Name);
                    //}
                }
            }

            //DisplayArbitrationOpportunityList(shortenedList);
            DisplayArbitrationOpportunityList(opportunityList);
        }

        //private ArbitrationOpportunity FindArbitration(List<ArbitrationOpportunity> opportunityList)
        //{
        //    ArbitrationOpportunity returnOpportunity = null;

        //    if (opportunityList != null && opportunityList.Count > 0)
        //    {
        //        //Validate the list of opportunities
        //        opportunityList = _opportunityValidator.ValidateOpportunities(opportunityList);

        //        //Decide which opportunity to execute:
        //        returnOpportunity = DetermineOpportunityToExecute(opportunityList);
        //    }

        //    return returnOpportunity;
        //}

        private void ExecuteArbitrationTrade(ArbitrationOpportunity opportunityToExecute)
        {
            if (opportunityToExecute != null)
            {
                opportunityToExecute.ExecutionDateTime = DateTime.Now;
                opportunityToExecute.ArbitrationRunId = _currentRun.Id;

                try
                {
                    Parallel.Invoke(() => { Sell(opportunityToExecute); }, () => { Buy(opportunityToExecute); });
                }
                catch (AggregateException ae)
                {
                    //Save the opportunity to the db to generate an id; will be useful for the error message
                    opportunityToExecute.PersistToDb();    

                    StringBuilder errorMesasge = new StringBuilder("There was a problem executing arbitration trade " + opportunityToExecute.Id + ": ");
                   
                    foreach (var e in ae.InnerExceptions)
                    {
                        errorMesasge.Append(Environment.NewLine + "\t" + e.Message);
                    }

                    log.Error(errorMesasge);
                }

                //If there is not already an id (which would be the case if there was an error), save the opportunity to the db.
                //Do this after executing the trade, to save a couple precious milliseconds between the orderbook update and order execution
                if (opportunityToExecute.Id == null)
                {
                    opportunityToExecute.PersistToDb();    
                }
                
                LogAndDisplayString("Just executed arbitration opportunity " + opportunityToExecute.Id + ".", LogLevel.Info);

                //Print the balance info before the trade is excecuted.
                log.Info("Balances before trade:" + Environment.NewLine + _opportunityValidator.BalanceInfoBeforeTradeString());
                
                //If the transfer mode is 'OnTime,' move transfer bicoins from the buy exchange to the sell exchange.
                if (_currentRun.TransferMode == TransferMode.OnTime)
                {
                    TransferOnTime(opportunityToExecute);                    
                }
                
                //If in live mode, save the arbitration trade again since it now contains ids for the buy and sell orders
                if (CurrentRun.ArbitrationMode == ArbitrationMode.Live)
                {
                    opportunityToExecute.PersistToDb();
                }

                DisplayArbitrationTrade(opportunityToExecute);
            }
        }

        private void TransferOnTime(ArbitrationOpportunity opportunityToExecute)
        {
            Transfer executedTransfer = null;

            switch (_currentRun.ArbitrationMode)
            {
                case ArbitrationMode.Live:

                    //No need to keep track of transfers in live mode since they are completed automatically.
                    executedTransfer = TransferManager.OnTimeTransfer_Live(opportunityToExecute.BuyExchange, opportunityToExecute.SellExchange, opportunityToExecute.BuyAmount, log);
                    break;

                case ArbitrationMode.Simulation:
                    executedTransfer = TransferManager.OnTimeTransfer_Simulate(opportunityToExecute.BuyExchange, opportunityToExecute.SellExchange, opportunityToExecute.BuyAmount, log);
                    //In simulation mode, keep track of transfers so that they can be completed manually later.
                    _transfersInProcess.Add(executedTransfer);
                    break;
            }

            //if (executedTransfer != null)
            {
                PrintToOutputTextBox((String.Format(LOG_MESSAGE, executedTransfer.Id.Value, executedTransfer.Amount, executedTransfer.OriginExchange.Name, executedTransfer.DestinationExchange.Name)));
            }
        }

        private void TransferRollupOnTrades()
        {
            //No null check needed for RollupNumber because if the transfer mode is 'Rollup on  trades,' then the current run must have a rollup number.
            DetectAndExecuteRollupTransfer(_currentRun.RollupNumber.Value);
        }

        private void TransferRollupByHours()
        {
            //If the specified number of hours has passed since the last time a transfer rollup was done, rollup transfers again.
            if (DateTime.Now > _lastTransferRollup.AddHours(Convert.ToDouble(_currentRun.RollupHours.Value)))
            {
                DetectAndExecuteRollupTransfer(1);

                //Don't forget to update last rollup time!
                _lastTransferRollup = DateTime.Now;
            }
        }

        /// <summary>
        /// Just a private helper method that calls the appropriate DetectAndExecuteRollupTransfers of the TransferManager depending on the arbitration
        /// mode of the current run. If the current run is in simulation mode, the created transfers are added to the _transfersInProcess list.
        /// </summary>
        /// <param name="rollupNumber">Number of trades to roll up on.</param>
        /// <returns></returns>
        private void DetectAndExecuteRollupTransfer(int rollupNumber)
        {
            List<Transfer> transferList = null;

            switch (_currentRun.ArbitrationMode)
            {
                case ArbitrationMode.Live:

                    transferList = TransferManager.DetectAndExecuteRollupTransfers_Live(rollupNumber, _currentRun.Id.Value, _currentRun.ExchangeList, log);
                    break;

                case ArbitrationMode.Simulation:
                    
                    transferList = TransferManager.DetectAndExecuteRollupTransfers_Simulate(rollupNumber, _currentRun.Id.Value, _currentRun.ExchangeList, log);

                    if (transferList != null)
                    {
                        _transfersInProcess.AddRange(transferList);
                    }
                    
                    break;
            }

            //If some transfers occured, display them to the output window
            if (transferList != null && transferList.Count > 0)
            {
                DisplayTransferList(transferList);
            }   
        }

        private void Sell(ArbitrationOpportunity opportunityToExecute)
        {
            switch (_currentRun.ArbitrationMode)
            {
                case ArbitrationMode.Live:

                    //In live move, save the order id string
                    opportunityToExecute.SellOrderId = opportunityToExecute.SellExchange.Sell(opportunityToExecute.SellAmount, opportunityToExecute.SellPrice);
                    break;

                case ArbitrationMode.Simulation:
                    opportunityToExecute.SellExchange.SimulatedSell(opportunityToExecute.SellAmount, opportunityToExecute.TotalSellCost);
                    break;
            }
        }

        private void Buy(ArbitrationOpportunity opportunityToExecute)
        {
            //*NOTE* doing a floor round here to avoid a round up situation (where Math.Round rounds up so the total buy cost is slightly more
            //than the amount of base currency at the buy exchange). This actually will leave some base currency in the exchange which is wrong, 
            //but since it is rounded to 9 decimal places that's ok (we're are talking trillionths of a penny here)
            switch (_currentRun.ArbitrationMode)
            {
                case ArbitrationMode.Live:

                    //When in live mode, save the order id string
                    opportunityToExecute.BuyOrderId = opportunityToExecute.BuyExchange.Buy(opportunityToExecute.BuyAmount, opportunityToExecute.BuyPrice);
                    break;

                case ArbitrationMode.Simulation:
                    opportunityToExecute.BuyExchange.SimulatedBuy(opportunityToExecute.BuyAmount, MathHelpers.FloorRound(opportunityToExecute.TotalBuyCost, 9));
                    break;
            }
        }
        
        /// <summary>
        /// Uses the arbitration filter to determine which opportunity should be executed based upon the selection method
        /// that is specified in the arbitration run. 
        /// </summary>
        /// <param name="opportunityList"></param>
        /// <returns></returns>
        private ArbitrationOpportunity DetermineOpportunityToExecute(List<ArbitrationOpportunity> opportunityList)
        {
            ArbitrationOpportunity opportunityToExecute;

            switch (_currentRun.OpportunitySelectionMethod)
            {
                case OpportunitySelectionType.MostProfitableOpportunity:
                    opportunityToExecute = ArbitrationFilter.MostProfitableTrade(opportunityList);
                    break;

                case OpportunitySelectionType.OpportunityForExchangeWithLeastBtc:
                    opportunityToExecute = ArbitrationFilter.TradeForExchangeWithLowestBtc(opportunityList, _currentRun.ExchangeList);
                    break;

                case OpportunitySelectionType.MostProfitableWithPercentRestriction:
                    if (_currentRun.ExchangeBaseCurrencyPercentageRestriction != null)
                    {
                        opportunityToExecute = ArbitrationFilter.MostProfitableTradeWithPercentRestriction(opportunityList, _currentRun.ExchangeList, _currentRun.ExchangeBaseCurrencyPercentageRestriction.Value);
                    }
                    else
                    {
                        throw new ArgumentException("Cannot use MostProfitableWithPercentRestriction without a percentage restriction.");
                    }
                        
                    break;

                default:
                    throw new ArgumentException("Unsupported opportunity selection method");
            }

            return opportunityToExecute;
        }

        private void SetTotalRowOnExchangeGrid()
        {
            decimal totalBtc = 0.0m;
            decimal totalFiat = 0.0m;
            decimal totalLeverageCurrencyInTransfer = 0.0m;
            const string totalName = "Total";

            foreach (BaseExchange exchange in _currentRun.ExchangeList)
            {
                totalFiat += exchange.AvailableFiat;
                totalBtc += exchange.AvailableBtc;
                totalLeverageCurrencyInTransfer += exchange.BTCInTransfer;
            }

            //Loop through each exchange and calculat the total btc and total fiat
            foreach (DataGridViewRow row in _exchangeDataGridView.Rows)
            {
                if (row.Cells["ExchangeName"].Value.ToString().Equals(totalName))
                {
                    int rowIndex = row.Index;

                    _exchangeDataGridView.Invoke(new Action(delegate() { _exchangeDataGridView.Rows[rowIndex].Cells[1].Value = totalBtc; }));
                    _exchangeDataGridView.Invoke(new Action(delegate() { _exchangeDataGridView.Rows[rowIndex].Cells[2].Value = totalFiat; }));
                    _exchangeDataGridView.Invoke(new Action(delegate() { _exchangeDataGridView.Rows[rowIndex].Cells[3].Value = totalLeverageCurrencyInTransfer; }));
                    return;
                }
            }

            //A total row wasn't found; add one
            _exchangeDataGridView.Invoke(new Action(delegate() { _exchangeDataGridView.Rows.Add(totalName, totalBtc, totalFiat); }));
        }

        /// <summary>
        /// Updates the balance and open order count for each exchange. Each exchange is updated in parallel.
        /// </summary>
        private void UpdateExchangeAccountInfo()
        {
            if (_currentRun.ExchangeList != null && _currentRun.ExchangeList.Count > 0)
            {
                Parallel.ForEach(_currentRun.ExchangeList, exchange =>
                {
                    //If there is an error updating the exchange balances, that's ok. Just update set the balances to 0 and let
                    //them be updated next.
                    try
                    {
                        exchange.UpdateBalances();
                        exchange.UpdateOpenOrderCount();
                    }

                    catch (Exception e)
                    {
                        //Log the error, and set the balances to 0
                        log.Warn("There was an error updating account info for " + exchange.Name + ": " + e.Message);
                        log.Debug("Stack trace: " + Environment.NewLine + e.StackTrace);

                        //Intialize all the exchange balances to zero
                        exchange.AvailableBtc = 0.0m;
                        exchange.AvailableFiat = 0.0m;
                        exchange.TotalBtc = 0.0m;
                        exchange.TotalFiat = 0.0m;
                    }
                });
            }
        }

        /// <summary>
        /// Sets the trade fee of each of the exchanges.
        /// </summary>
        private void SetExchangeTradeFee()
        {
            if (_currentRun.ExchangeList != null && _currentRun.ExchangeList.Count > 0)
            {
                Parallel.ForEach(_currentRun.ExchangeList, exchange =>
                {
                    exchange.SetTradeFee();
                });
            }
        }

        /// <summary>
        /// Updates the btc and fiat balances of all the exchanges based upon the mode of the current run. For live mode, the balance is pulled from the user's acccount. In simulation mode, the balance
        /// is pulled from app.config.
        /// </summary>
        private void SetInitialAccountBalances()
        {
            switch (CurrentRun.ArbitrationMode)
            {
                case ArbitrationMode.Live:
                    UpdateExchangeAccountInfo();
                    break;

                case ArbitrationMode.Simulation:
                    SetInitialSimulationBalances();
                    break;
            }
        }

        private void SetInitialSimulationBalances()
        {
            if (_currentRun.ExchangeList != null && _currentRun.ExchangeList.Count > 0)
            {
                foreach (BaseExchange exchange in _currentRun.ExchangeList)
                {
                    exchange.SetIntialSimulationBalances();
                }
            }
        }

        private void OnError(string messageBoxMessage, Exception exception, string logExceptionPrefix = "")
        {
            ArbitrationManagerFailureEventHandler handler = ErrorOccured;

            //Stop this run.
            Stop();

            log.Error(logExceptionPrefix + Environment.NewLine + exception);
            
            //Only  throw the event is there is a listener 
            if (handler != null)
            {
                handler(this, new ArbitrationManagerFailureEventArgs(messageBoxMessage));
            }
        }

        #region Log and Display Methods

        /// <summary>
        /// Displays a list of arbitration opportunities to the output textbox. If the given list is null or empty, then 'nothing' is printed
        /// to the display textbox. For each opportunity in the list, if the opporunity is null, 'nothing' is displayed to textbox. Otherwise,
        /// the buy exchange name, sell exchange name, profit, amount, buy price, and sell price of the opportunity are displayed.
        /// </summary>
        /// <param name="opportunityList">List of arbitration opportunities to be displayed to the output textbox.</param>
        private void DisplayArbitrationOpportunityList(List<ArbitrationOpportunity> opportunityList)
        {
            StringBuilder displayString = new StringBuilder("");

            //If the list is null or empty, print 'nothing' to the display textbox.
            if (opportunityList == null || opportunityList.Count <= 0)
            {
                displayString.Append(DateTime.Now.ToString() + " --> nothing.");
            }

            else
            {
                foreach (ArbitrationOpportunity opportunity in opportunityList)
                {
                    //Put each opportunity is on its own line.
                    if (displayString.Length > 0)
                    {
                        displayString.Append(Environment.NewLine);
                    }

                    if (opportunity == null)
                    {
                        displayString.Append(DateTime.Now.ToString() + " --> nothing.");
                    }
                    else
                    {
                        displayString.Append(DateTime.Now.ToString() + " --> " + opportunity.BuyExchange.Name.PadRight(9, ' ') + "\t" + opportunity.SellExchange.Name.PadRight(9, ' ') + "\t" + decimal.Round(opportunity.Profit, 2) + "\t" + decimal.Round(opportunity.BuyAmount, 4) + "\t" + decimal.Round(opportunity.BuyPrice, 2) + "\t" + decimal.Round(opportunity.SellPrice, 2));
                    }
                }
            }

            PrintToOutputTextBox(displayString.ToString());
        }

        private void DisplayArbitrationTrade(ArbitrationOpportunity tradeToDisplay)
        {
            if (tradeToDisplay == null)
            {
                return;
            }

            if (tradeToDisplay.Id == null)
            {
                throw new Exception("Cannot display given trade; it does not have an Id.");
            }
            
            _tradeDataGridView.Invoke(new Action(delegate() { _tradeDataGridView.Rows.Add(tradeToDisplay.Id, tradeToDisplay.ExecutionDateTime, tradeToDisplay.BuyExchange.Name, tradeToDisplay.SellExchange.Name, Math.Round(tradeToDisplay.Profit, 10), Math.Round(tradeToDisplay.BuyAmount, 10), Math.Round(tradeToDisplay.BuyPrice, 4), Math.Round(tradeToDisplay.TotalBuyCost, 10), Math.Round(tradeToDisplay.SellPrice, 4), Math.Round(tradeToDisplay.TotalSellCost, 10)); }));
        }

        private void DisplayExchangeBalances()
        {
            if (_currentRun.ExchangeList != null && _currentRun.ExchangeList.Count > 0)
            {
                //There aren't any rows in the data grid view; add a row for each exchange
                if (_exchangeDataGridView.Rows.Count <= 0)
                {
                    foreach (BaseExchange exchange in _currentRun.ExchangeList)
                    {
                        _exchangeDataGridView.Invoke(new Action(delegate() { _exchangeDataGridView.Rows.Add(exchange.Name, exchange.AvailableBtc, exchange.AvailableFiat, exchange.OpenOrders); }));
                    }
                }

                else
                {
                    foreach (BaseExchange exchange in _currentRun.ExchangeList)
                    {
                        foreach (DataGridViewRow row in _exchangeDataGridView.Rows)
                        {
                            if (row.Cells["ExchangeName"].Value.ToString().Equals(exchange.Name))
                            {
                                int rowIndex = row.Index;

                                _exchangeDataGridView.Invoke(new Action(delegate() { _exchangeDataGridView.Rows[rowIndex].Cells[1].Value = exchange.AvailableBtc; }));
                                _exchangeDataGridView.Invoke(new Action(delegate() { _exchangeDataGridView.Rows[rowIndex].Cells[2].Value = exchange.AvailableFiat; }));
                                _exchangeDataGridView.Invoke(new Action(delegate() { _exchangeDataGridView.Rows[rowIndex].Cells[3].Value = exchange.OpenOrders; }));
                            }
                        }
                    }
                }

                SetTotalRowOnExchangeGrid();
            }
        }

        private void LogAndDisplayString(string Message, LogLevel loglevel)
        {
            //Log the message at the appropriate level
            switch (loglevel)
            {
                case LogLevel.Debug:
                    log.Debug(Message);
                    break;
                    
                case LogLevel.Info:
                    log.Info(Message);
                    break;

                case LogLevel.Warning:
                    log.Warn(Message);
                    break;

                case LogLevel.Error:
                    log.Error(Message);
                    break;

                case LogLevel.Fatal:
                    log.Fatal(Message);
                    break;
            }

            //Display the message to the textbox
            PrintToOutputTextBox(DateTime.Now.ToString() + " --> " + Message);
        }

        private void LogArbitrationOpportunity(ArbitrationOpportunity opportunity)
        {
            if (opportunity != null)
            {
                StringBuilder logStringBuilder = new StringBuilder();
                logStringBuilder.Append("\"" + DateTime.Now.ToString() + "\",\"" + opportunity.BuyExchange.Name + "\",\"" + opportunity.SellExchange.Name + "\",\"" + opportunity.BuyPrice + "\",\"" + opportunity.SellPrice + "\",\"" + opportunity.BuyAmount + "\",\"" + opportunity.Profit + "\",\"" + opportunity.BuyExchangeOrderBook.AsksToString() + "\",\"" + opportunity.SellExchangeOrderBook.BidsToString() + "\"");

                try
                {
                    FileWriting.WriteToFile(_currentRun.LogFileName, logStringBuilder.ToString(), FileMode.Append);
                }
                catch (IOException)
                {
                    //Do nothing; just need to catch the exception. If the file is not available its no worries,
                    //just don't log.
                }
            }
        }

        private void DisplayTransferList(List<Transfer> transferList)
        {
            if (transferList != null && transferList.Count > 0)
            {
                foreach (Transfer transfer in transferList)
                {
                   PrintToOutputTextBox((String.Format(LOG_MESSAGE, transfer.Id.Value, transfer.Amount, transfer.OriginExchange.Name, transfer.DestinationExchange.Name)));
                }
            }
        }

        /// <summary>
        /// Displays the given text to the output textbox on its own line. If the _outputTextBox of this class is null, or the given text
        /// is null or empty, this method does nothing.
        /// </summary>
        /// <param name="text">Text to be appended to the display textbox.</param>
        private void PrintToOutputTextBox(string text)
        {
            //Called softLimit becuse this isn't the actual hard limit of how many lines the display textbox can have; the maximum is actually
            //softLimit + the number of lines in the Text parameter. 
            int softLimit = 1000;

            //Make sure class has a display textbox and the given text is not empty.
            if (_outputTextBox != null && !String.IsNullOrWhiteSpace(text))
            {

                //If the number of lines in the display textbox is over the soft limit, chop off the lines in front to hit the soft limit.
                if (_outputTextBox.Lines.Length > softLimit)
                {
                    string[] adjustedText = new string[softLimit];
                    Array.Copy(_outputTextBox.Lines, (_outputTextBox.Lines.Length - softLimit), adjustedText, 0, softLimit);
                    _outputTextBox.Invoke(new Action(delegate() { _outputTextBox.Lines = adjustedText; }));
                }

                _outputTextBox.Invoke(new Action(delegate() { _outputTextBox.SelectionStart = _outputTextBox.Text.Length; }));
                _outputTextBox.Invoke(new Action(delegate() { _outputTextBox.ScrollToCaret(); }));

                //Now that the display textbox has been limited, go ahead and append new text (agian, note that adding text here puts the 
                //textbox over the soft limit).
                if (!String.IsNullOrWhiteSpace(_outputTextBox.Text))
                {
                    //If the textbox is not empty, need to insert a new line so that stuff that is about to be appended appears on its own line.
                    _outputTextBox.Invoke(new Action(delegate() { _outputTextBox.AppendText(Environment.NewLine); }));
                }

                _outputTextBox.Invoke(new Action(delegate() { _outputTextBox.AppendText(text); }));
            }
        }

        #endregion

        #endregion
    }
}
