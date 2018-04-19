using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BitcoinExchanges;
using ArbitrationSimulator.UiObjects;
using ArbitrationUtilities.EnumerationObjects;
using CommonFunctions;

namespace ArbitrationSimulator
{
    public partial class ArbitrationForm : Form
    {
        #region Class Variables
        
        private const int defaultSearchInterval = 5000;
        private const int defaultRoundsRequiredForValidation = 1;
        private const decimal defaultMaxBtcTradeAmount = 0.25m;
        private const decimal defaultMaxFiatTradeAmount = 35m;
        private const decimal defaultMinProfitForTrade = 0.03m;
        private const string INPUT_SETTINGS_PATH = "RunSettings";

        private ArbitrationManager _arbitrationManager;
        private List<ExchangeCheckBox> _exchangeCheckboxList;

        #endregion

        #region Constructors
        
        public ArbitrationForm()
        {
            InitializeComponent();

            //Event handler for when the red 'x' is clicked.
            FormClosing += Window_Closing;

            //Give each of the exchange checkboxes an exchange type
            chxAnx.ExchangeType = typeof(Anx);
            chxBitstamp.ExchangeType = typeof(Bitstamp);
            chxBitX.ExchangeType = typeof (BitX);
            chxBtce.ExchangeType = typeof(Btce);
            chxItBit.ExchangeType = typeof(ItBit);
            chxKraken.ExchangeType = typeof(Kraken);
            chxOkCoin.ExchangeType = typeof(OkCoin);
            chxCoinbase.ExchangeType = typeof(Coinbase);
            chxBitfinex.ExchangeType = typeof(Bitfinex);
            
            //Set enumeration for each of the filter rules radio buttons
            rbMostProfitable.SelectionType = OpportunitySelectionType.MostProfitableOpportunity;
            rbExchangeWithLeastBtc.SelectionType = OpportunitySelectionType.OpportunityForExchangeWithLeastBtc;
            rbMostProfitableWithPercentRestriction.SelectionType = OpportunitySelectionType.MostProfitableWithPercentRestriction;

            //Set enumeration for each of arbitration mode radio buttons
            rbSimulation.ArbitrationMode = ArbitrationMode.Simulation;
            rbLive.ArbitrationMode = ArbitrationMode.Live;

            //Set enumeration for each of the transer mode radio buttons
            rbTransferModeRollupByHours.TransferMode = TransferMode.RollupByHour;
            rbTransferModeRollupOnTrades.TransferMode = TransferMode.RollupOnTrades;
            rbTransferModeOnTime.TransferMode = TransferMode.OnTime;
            rbTransferModeNone.TransferMode = TransferMode.None;

            _exchangeCheckboxList = new List<ExchangeCheckBox>();
            _exchangeCheckboxList.Add(chxAnx);
            _exchangeCheckboxList.Add(chxBitfinex);
            _exchangeCheckboxList.Add(chxBitstamp);
            _exchangeCheckboxList.Add(chxBitX);
            _exchangeCheckboxList.Add(chxBtce);
            _exchangeCheckboxList.Add(chxCoinbase);
            _exchangeCheckboxList.Add(chxItBit);
            _exchangeCheckboxList.Add(chxKraken);
            _exchangeCheckboxList.Add(chxOkCoin);
            
            try
            {
                _arbitrationManager = new ArbitrationManager(this.txbTrades, this.dgvAccount, this.dgvTrades);
                _arbitrationManager.ErrorOccured += ArbitrationRunError;
            }

            catch (SqlException e)
            {
                MessageBox.Show("There was a problem opening the database, exiting:" + Environment.NewLine + e.Message, "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                throw e;
            }
        }

        #endregion

        #region Event Handling Methods

        private void startButton_Click(object sender, EventArgs e)
        {
            bool errorOccured = false;

            //Disable inputs while arbitration manager is running
            DisableInputs();

            ArbitrationRun currentRun = new ArbitrationRun();
           
            try
            {
                ValidateAndSetInputValues(currentRun);
                _arbitrationManager.Start(currentRun);
            }

            //There was a problem getting the inputs, make the inputs editable again so the user can fix them
            catch (ArgumentException exception)
            {
                errorOccured = true;
                MessageBox.Show(exception.Message, "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            catch (Exception exception)
            {
                errorOccured = true;
                MessageBox.Show(exception.Message, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (errorOccured)
            {
                ResetInputsAfterRunStop();
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            EndArbitrationRun();
            ResetInputsAfterRunStop();
        }

        //****NOTE****** Leaving in this in here in case I need need to re-implement the continue button
        //private void continueButton_Click(object sender, EventArgs e)
        //{
        //    StringBuilder errorMessage = new StringBuilder();
        //    bool errorOccured = false;

        //    DisableInputs();

        //    try
        //    {
        //        //Get the run id that is to be continued
        //        int runId = ParseAndValidateInputInteger(txbContinueRunId.Text, "continue run ID", ref errorMessage);

        //        //If there was an validation error, throw an error
        //        if (errorMessage.Length > 0)
        //        {
        //            throw new ArgumentException(errorMessage.ToString());
        //        }

        //        //Get the run from the db
        //        ArbitrationRun continuedRun = ArbitrationRun.GetArbitrationRunFromDbById(runId);

        //        //If a run with the given id could not be found, throw an error
        //        if (continuedRun == null)
        //        {
        //            throw new Exception("Could not find run with id " + runId + ". Sorry, have a nice day.");
        //        }

        //        SetInputsForContinuedRun(continuedRun);

        //        _arbitrationManager.Start(continuedRun);
        //    }

        //    catch (Exception exception)
        //    {
        //        errorOccured = true;
        //        MessageBox.Show(exception.Message, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }

        //    if (errorOccured)
        //    {
        //        ResetInputsAfterRunStop();
        //    }
        //}

        private void CompleteTransfersButton_Click(object sender, EventArgs e)
        {
            StringBuilder errorMessage = new StringBuilder("");
            int previousRunId = ParseAndValidateInputInteger(txbRunId.Text, lblRunId.Text, ref errorMessage);

            //If there was an error getting the run id input, errorMessage will have some value.
            if (errorMessage.Length <= 0)
            {
                //Confirm user really wants to do this.
                DialogResult answer = MessageBox.Show("Are you sure you want to complete transfers for all trades which do not an associated transfer in run " + previousRunId + "?", "Complete Transfers for Run", MessageBoxButtons.YesNo);

                if (answer == DialogResult.Yes)
                {
                    bool success = _arbitrationManager.CompleteTransfersFromPreviousRun(previousRunId);

                    if (success)
                    {
                        MessageBox.Show("Transfers completed for run " + previousRunId + ".", "Important Note", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    }
                }
            }

            //The run id input was did not have a valid value.
            else
            {
                MessageBox.Show(errorMessage.ToString(), "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private void ArbitrationRunError(object sender, ArbitrationManagerFailureEventArgs e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            //If there was a fatal error, the arbitration run stops itself. So just reset the inputs
            ResetInputsAfterRunStop();
        }

        #endregion

        #region Private Methods

        private void SetInputsForContinuedRun(ArbitrationRun continuedRun)
        {
            try
            {
                //Now set all the inputs
                switch (continuedRun.ArbitrationMode)
                {
                    case ArbitrationMode.Simulation:
                        rbSimulation.Checked = true;
                        break;

                    case ArbitrationMode.Live:
                        rbLive.Checked = true;
                        break;

                    default:
                        throw new Exception("Given run has unknown arbitration mode");
                }

                switch (continuedRun.TransferMode)
                {
                    case TransferMode.OnTime:
                    case TransferMode.RollupByHour:
                    case TransferMode.RollupOnTrades:
                        throw new Exception("Cannot continue the given arbitration run; it uses a transfer mode that is not supported");

                    case TransferMode.None:
                        rbTransferModeNone.Checked = true;
                        break;

                    default:
                        throw new Exception("Given run has unknown transfer mode");
                }

                switch (continuedRun.OpportunitySelectionMethod)
                {
                    case OpportunitySelectionType.MostProfitableOpportunity:
                        rbMostProfitable.Checked = true;
                        break;

                    case OpportunitySelectionType.OpportunityForExchangeWithLeastBtc:
                        rbExchangeWithLeastBtc.Checked = true;
                        break;

                    case OpportunitySelectionType.MostProfitableWithPercentRestriction:
                        rbMostProfitableWithPercentRestriction.Checked = true;
                        txbRestrictionPercent.Text = continuedRun.ExchangeBaseCurrencyPercentageRestriction.ToString();
                        break;
                }

                chxAnx.Checked = continuedRun.UseAnx;
                chxBitfinex.Checked = continuedRun.UseBitfinex;
                chxBitstamp.Checked = continuedRun.UseBitstamp;
                chxBitX.Checked = continuedRun.UseBitX;
                chxBtce.Checked = continuedRun.UseBtce;
                chxItBit.Checked = continuedRun.UseItBit;
                chxKraken.Checked = continuedRun.UseKraken;
                chxOkCoin.Checked = continuedRun.UseOkCoin;
                chxCoinbase.Checked = continuedRun.UseCoinbase;

                txbMinProfit.Text = continuedRun.MinimumProfit.ToString();
                txbRoundsRequiredForValidation.Text = continuedRun.RoundsRequiredForValidation.ToString();
                txbMaxBtcTradeAmount.Text = continuedRun.MaxBtcTradeAmount.ToString();
                txbMaxFiatTradeAmount.Text = continuedRun.MaxFiatTradeAmount.ToString();
                txbSearchInterval.Text = (continuedRun.SeachIntervalMilliseconds / 1000).ToString();
                txbLogFileName.Text = "";
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private void DisableInputs()
        {
            txbMinProfit.ReadOnly = true;
            txbSearchInterval.ReadOnly = true;
            txbMaxBtcTradeAmount.ReadOnly = true;
            startButton.Enabled = false;
            completeTransfersButton.Enabled = false;
            stopButton.Enabled = true;
            SetCheckboxListEnabled(false);
            txbRestrictionPercent.Enabled = false;
            lblPercentRestriction.Enabled = false;
            filterRulesGroupBox.Enabled = false;
            arbitrationModeGroupBox.Enabled = false;
            transferModeGroupBox.Enabled = false;
            txbRunId.Enabled = false;
            txbRollupNumber.Enabled = false;
            lblRollupNumber.Enabled = false;
            txbRollupHours.Enabled = false;
            lblRollupHours.Enabled = false;
            //txbContinueRunId.ReadOnly = true;
            //continueButton.Enabled = false;
        }

        private int ParseAndValidateInputInteger(String TextToParse, String InputName, ref StringBuilder ErrorMessage)
        {
            int returnInt;
            bool parseResult = int.TryParse(TextToParse, out returnInt);

            if (!parseResult)
            {
                AppendToErrorMessage(ErrorMessage, "The value for '" + InputName + "' is not valid.");
            }
            else if (returnInt <= 0)
            {
                //Given value must be greater than zero; it does make sense to have any zero or negative input values
                AppendToErrorMessage(ErrorMessage, "'" + InputName + "' must be an integer greater than 0.");
            }
            return returnInt;
        }

        /// <summary>
        /// Returns the opportunity radio button that is selected. Returns null if none of them are selected.
        /// </summary>
        /// <returns>The opportunity radio button that is selected.</returns>
        private OpportunitySelectionRadioButton GetCheckedOpportunitySelectionRadioButton()
        {
            foreach(var radioButton in filterRulesGroupBox.Controls.OfType<OpportunitySelectionRadioButton>())
            {
                if (radioButton.Checked)
                {
                    return radioButton;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the arbitration mode radio button that is selected. Returns null if none of them are selected.
        /// </summary>
        /// <returns>The arbitration mode radio button that is selected.</returns>
        private ArbitrationModeRadioButton GetCheckedArbitrationModeRadioButton()
        {
            foreach (var radioButton in arbitrationModeGroupBox.Controls.OfType<ArbitrationModeRadioButton>())
            {
                if (radioButton.Checked)
                {
                    return radioButton;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the transfer mode radio button that is selected. Returns null if none of them are selected.
        /// </summary>
        /// <returns>The transfer mode radio button that is selected.</returns>
        private TransferModeRadioButton GetCheckedTransferModeRadioButton()
        {
            foreach (var radioButton in transferModeGroupBox.Controls.OfType<TransferModeRadioButton>())
            {
                if (radioButton.Checked)
                {
                    return radioButton;
                }
            }

            return null;         
        }

        /// <summary>
        /// Loops through all of the exchange check boxes and set their 'enabled' property to the value specified.
        /// </summary>
        /// <param name="Enabled">The value to set to the enabled property of each of the exchange text boxes.</param>
        private void SetCheckboxListEnabled(bool Enabled)
        {
            foreach (ExchangeCheckBox exchangeCheckBox in _exchangeCheckboxList)
            {
                exchangeCheckBox.Enabled = Enabled;
            }
        }

        /// <summary>
        /// Sets the _exchangeList property of _arbitrationManager to all of the exchanges selected in the form.
        /// If none of the exchasnge check boxes are checked, an ArgumentException is thrown.
        /// </summary>
        private void SetExchangeList(ArbitrationRun run)
        {
            foreach (ExchangeCheckBox exchangeCheckBox in _exchangeCheckboxList)
            {
                if (exchangeCheckBox.Checked)
                {
                    run.AddExchange(exchangeCheckBox.ExchangeType);
                }
            }

            //If none of the exchange checkboxes are selected, thrown an error.
            if (run.ExchangeList.Count <= 0)
            {
                throw new ArgumentException();
            }
        }

        private void AppendToErrorMessage(StringBuilder ErrorMessage, string TextToAppend)
        {
            if (ErrorMessage.Length > 0)
            {
                ErrorMessage.Append(Environment.NewLine);
            }

            ErrorMessage.Append(TextToAppend);
        }

        private void FilterGroupBox_CheckChanged(object sender, EventArgs e)
        {
            if (sender == rbMostProfitableWithPercentRestriction)
            {
                lblPercentRestriction.Enabled = true;
                txbRestrictionPercent.Enabled = true;
            }
            else
            {
                lblPercentRestriction.Enabled = false;
                txbRestrictionPercent.Enabled = false;
            }
        }

        private void TransferModeGroupBox_CheckChanged(object sender, EventArgs e)
        {
            //Just set all second level parameters to false; if the appropriate rb is checked, it will be
            //changed further down.
            lblRollupNumber.Enabled = false;
            txbRollupNumber.Enabled = false;
            lblRollupHours.Enabled = false;
            txbRollupHours.Enabled = false;

            if (sender == rbTransferModeRollupOnTrades)
            {
                lblRollupNumber.Enabled = true;
                txbRollupNumber.Enabled = true;
            }

            if (sender == rbTransferModeRollupByHours)
            {
                lblRollupHours.Enabled = true;
                txbRollupHours.Enabled = true;
            }
        }

        private decimal ParseAndValidateInputDecimal(String TextToParse, String InputName, ref StringBuilder ErrorMessage)
        {
            decimal returnDecimal = 0m;

            try
            {
                returnDecimal = TypeConversion.ParseStringToDecimalStrict(TextToParse);
            }
            catch (Exception)
            {
                AppendToErrorMessage(ErrorMessage, "The value for '" + InputName + "' is not valid.");
            }
            
            if (returnDecimal <= 0)
            {
                //Given value must be greater than zero; it does make sense to have a negative maximum fiat trade amount.
                AppendToErrorMessage(ErrorMessage, "'" + InputName + "' must be a decimal greater than 0.");
            }

            return returnDecimal;
        }

        /// <summary>
        /// Validates all the inputs of the form. If the input is validated, it is passed on to the given arbitration run. If any of the
        /// fields weren't valid, an ArgumentException is thrown. If this method is called and the input field is empty, the default value
        /// of that input is used. The log file input is an exception to this; if that is null then the given run just doesn't have a log file.
        /// </summary>
        /// <param name="Run">The arbitration run the parameters are added to if everything is valid.</param>
        private void ValidateAndSetInputValues(ArbitrationRun Run)
        {
            int searchInterval;
            int roundsRequiredForValidation;
            decimal maxBtcTradeAmount;
            decimal minimumProfit;
            decimal maxFiatTradeAmount;
            decimal? percentageRestriction = null;
            int? rollupNumber = null;
            decimal? rollupHours = null;
            StringBuilder errorMessage = new StringBuilder("");

            //Get the config settings from app.config
            NameValueCollection inputSettings = (NameValueCollection)ConfigurationManager.GetSection(INPUT_SETTINGS_PATH);

            //Get Fiat Type
            try
            {
                Run.FiatType = (FiatType)Enum.Parse(typeof(FiatType), inputSettings["FiatType"]);
            }
            catch (Exception)
            {
                AppendToErrorMessage(errorMessage, "Fiat Type is not valid.");
            }

            //Validate Minimum profit
            if (!String.IsNullOrWhiteSpace(txbMinProfit.Text))
            {
                minimumProfit = ParseAndValidateInputDecimal(txbMinProfit.Text, lblMinProfit.Text, ref errorMessage);
            }
            else
            {
                txbMinProfit.Text = "" + defaultMinProfitForTrade;
                minimumProfit = defaultMinProfitForTrade;
            }

            //Validate search interval
            if (!String.IsNullOrWhiteSpace(txbSearchInterval.Text))
            {
                searchInterval = ParseAndValidateInputInteger(txbSearchInterval.Text, lblSearchInterval.Text, ref errorMessage);
                
                //Search interval was a valid number; convert it from seconds to milliseconds
                searchInterval = searchInterval * 1000;
            }
            else
            {
                //Convert the search interval to seconds and display it
                txbSearchInterval.Text = "" + defaultSearchInterval / 1000;
                searchInterval = defaultSearchInterval;
            }

            //Validate rounds required for validation
            if (!String.IsNullOrWhiteSpace(txbRoundsRequiredForValidation.Text))
            {
                roundsRequiredForValidation = ParseAndValidateInputInteger(txbRoundsRequiredForValidation.Text, lblRoundsRequiredForValidation.Text, ref errorMessage);
            }
            else
            {
                //Convert the search interval to seconds and display it
                txbRoundsRequiredForValidation.Text = "" + defaultRoundsRequiredForValidation;
                roundsRequiredForValidation = defaultRoundsRequiredForValidation;
            }

            //Validate max BTC amount
            if (!String.IsNullOrWhiteSpace(txbMaxBtcTradeAmount.Text))
            {
                maxBtcTradeAmount = ParseAndValidateInputDecimal(txbMaxBtcTradeAmount.Text, lblMaxBtcLabel.Text, ref errorMessage);
            }
            else
            {
                txbMaxBtcTradeAmount.Text = "" + defaultMaxBtcTradeAmount;
                maxBtcTradeAmount = defaultMaxBtcTradeAmount;
            }

            //Validate max Fiat amount
            if (!String.IsNullOrWhiteSpace(txbMaxFiatTradeAmount.Text))
            {
                maxFiatTradeAmount = ParseAndValidateInputDecimal(txbMaxFiatTradeAmount.Text, lblMaxFiatLabel.Text, ref errorMessage);
            }
            else
            {
                txbMaxFiatTradeAmount.Text = "" + defaultMaxFiatTradeAmount;
                maxFiatTradeAmount = defaultMaxFiatTradeAmount;
            }

            //Validate percentage restriction
            if (rbMostProfitableWithPercentRestriction.Checked)
            {
                percentageRestriction = ParseAndValidateInputInteger(txbRestrictionPercent.Text, lblPercentRestriction.Text, ref errorMessage);
            }

            //Validate rollup trade number
            if (rbTransferModeRollupOnTrades.Checked)
            {
                rollupNumber = ParseAndValidateInputInteger(txbRollupNumber.Text, lblRollupNumber.Text, ref errorMessage);
            }

            //Validate rollup hours
            if (rbTransferModeRollupByHours.Checked)
            {
                rollupHours = ParseAndValidateInputDecimal(txbRollupHours.Text, lblRollupHours.Text, ref errorMessage);
            }

            //Validate log file
            if (!String.IsNullOrWhiteSpace(txbLogFileName.Text))
            {
                try
                {
                    Run.LogFileName = txbLogFileName.Text;
                }

                catch (IOException)
                {
                    AppendToErrorMessage(errorMessage, "The specified log file cannot be used.");
                }
            }
            else
            {
                Run.LogFileName = null;
            }
            
            try
            {
                //Get exchange list from user and pass it to arbitration manager
                SetExchangeList(Run);
            }
            catch (ArgumentException)
            {
                AppendToErrorMessage(errorMessage, "Please select at least one exchange.");
            }

            //If there was an validation error, throw an error
            if (errorMessage.Length > 0)
            {
                throw new ArgumentException(errorMessage.ToString());
            }

            //If the code made it this far, all inputs are valid, so update the current run with the inputs
            Run.MinimumProfit = minimumProfit;
            Run.SeachIntervalMilliseconds = searchInterval;
            Run.RoundsRequiredForValidation = roundsRequiredForValidation;
            Run.MaxBtcTradeAmount = maxBtcTradeAmount;
            Run.MaxFiatTradeAmount = maxFiatTradeAmount;
            Run.OpportunitySelectionMethod = GetCheckedOpportunitySelectionRadioButton().SelectionType;
            Run.ExchangeBaseCurrencyPercentageRestriction = percentageRestriction / 100;
            Run.TransferMode = GetCheckedTransferModeRadioButton().TransferMode;
            Run.RollupNumber = rollupNumber;
            Run.RollupHours = rollupHours;
            Run.ArbitrationMode = GetCheckedArbitrationModeRadioButton().ArbitrationMode;
        }

        private void ResetInputsAfterRunStop()
        {
            //Re-enable the inputs
            stopButton.Enabled = false;
            txbMinProfit.ReadOnly = false;
            txbSearchInterval.ReadOnly = false;
            txbMaxBtcTradeAmount.ReadOnly = false;
            startButton.Enabled = true;
            completeTransfersButton.Enabled = true;
            SetCheckboxListEnabled(true);
            filterRulesGroupBox.Enabled = true;
            arbitrationModeGroupBox.Enabled = true;
            transferModeGroupBox.Enabled = true;
            txbRunId.ReadOnly = true;
            //txbContinueRunId.ReadOnly = false;
            //continueButton.Enabled = true;
            
            if (rbMostProfitableWithPercentRestriction.Checked)
            {
                lblPercentRestriction.Enabled = true;
                txbRestrictionPercent.Enabled = true;
            }

            if (rbTransferModeRollupOnTrades.Checked)
            {
                lblRollupNumber.Enabled = true;
                txbRollupNumber.Enabled = true;
            }

            if (rbTransferModeRollupByHours.Checked)
            {
                lblRollupHours.Enabled = true;
                txbRollupHours.Enabled = true;
            }
        }

        private void EndArbitrationRun()
        {
            try
            {
                _arbitrationManager.Stop();
            }

            catch (Exception exception)
            {
                MessageBox.Show("An error occured while stopping the run: " + Environment.NewLine + exception.Message, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Window_Closing(object sender, FormClosingEventArgs e)
        {
            EndArbitrationRun();
        }

        #endregion
    }

    #region Nested Classes

    public class CustomPanel : Panel
    {
        public CustomPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(BackColor))
                e.Graphics.FillRectangle(brush, ClientRectangle);
            e.Graphics.DrawRectangle(Pens.WhiteSmoke, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        }

    }

    #endregion
}
