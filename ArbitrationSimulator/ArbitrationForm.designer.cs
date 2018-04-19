using System.ComponentModel;
using System.Windows.Forms;
using ArbitrationSimulator.UiObjects;

namespace ArbitrationSimulator
{
    partial class ArbitrationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArbitrationForm));
            this.txbTrades = new System.Windows.Forms.TextBox();
            this.dgvAccount = new System.Windows.Forms.DataGridView();
            this.ExchangeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BtcAmount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FiatAmount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OpenOrders = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvTrades = new System.Windows.Forms.DataGridView();
            this.Id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BuyExchange = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SellExchange = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Profit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Amount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BuyPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TotalBuyCost = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SellPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TotalSellCost = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1 = new ArbitrationSimulator.CustomPanel();
            this.lblRoundsRequiredForValidation = new System.Windows.Forms.Label();
            this.txbRoundsRequiredForValidation = new System.Windows.Forms.TextBox();
            this.completeTransfersButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.transferCompleteGroupbox = new System.Windows.Forms.GroupBox();
            this.txbRunId = new System.Windows.Forms.TextBox();
            this.lblRunId = new System.Windows.Forms.Label();
            this.transferModeGroupBox = new System.Windows.Forms.GroupBox();
            this.rbTransferModeNone = new ArbitrationSimulator.UiObjects.TransferModeRadioButton();
            this.lblRollupHours = new System.Windows.Forms.Label();
            this.txbRollupHours = new System.Windows.Forms.TextBox();
            this.rbTransferModeRollupByHours = new ArbitrationSimulator.UiObjects.TransferModeRadioButton();
            this.lblRollupNumber = new System.Windows.Forms.Label();
            this.txbRollupNumber = new System.Windows.Forms.TextBox();
            this.rbTransferModeRollupOnTrades = new ArbitrationSimulator.UiObjects.TransferModeRadioButton();
            this.rbTransferModeOnTime = new ArbitrationSimulator.UiObjects.TransferModeRadioButton();
            this.arbitrationModeGroupBox = new System.Windows.Forms.GroupBox();
            this.rbSimulation = new ArbitrationSimulator.UiObjects.ArbitrationModeRadioButton();
            this.rbLive = new ArbitrationSimulator.UiObjects.ArbitrationModeRadioButton();
            this.lblMaxFiatLabel = new System.Windows.Forms.Label();
            this.txbMaxFiatTradeAmount = new System.Windows.Forms.TextBox();
            this.filterRulesGroupBox = new System.Windows.Forms.GroupBox();
            this.lblPercentRestriction = new System.Windows.Forms.Label();
            this.txbRestrictionPercent = new System.Windows.Forms.TextBox();
            this.rbMostProfitableWithPercentRestriction = new ArbitrationSimulator.UiObjects.OpportunitySelectionRadioButton();
            this.rbExchangeWithLeastBtc = new ArbitrationSimulator.UiObjects.OpportunitySelectionRadioButton();
            this.rbMostProfitable = new ArbitrationSimulator.UiObjects.OpportunitySelectionRadioButton();
            this.lblMaxBtcLabel = new System.Windows.Forms.Label();
            this.txbMaxBtcTradeAmount = new System.Windows.Forms.TextBox();
            this.lblArbitrationHunterOutput = new System.Windows.Forms.Label();
            this.txbLogFileName = new System.Windows.Forms.TextBox();
            this.lblSearchInterval = new System.Windows.Forms.Label();
            this.txbSearchInterval = new System.Windows.Forms.TextBox();
            this.txbMinProfit = new System.Windows.Forms.TextBox();
            this.lblMinProfit = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chxBitX = new ArbitrationSimulator.UiObjects.ExchangeCheckBox();
            this.chxBitfinex = new ArbitrationSimulator.UiObjects.ExchangeCheckBox();
            this.chxCoinbase = new ArbitrationSimulator.UiObjects.ExchangeCheckBox();
            this.chxOkCoin = new ArbitrationSimulator.UiObjects.ExchangeCheckBox();
            this.chxBitstamp = new ArbitrationSimulator.UiObjects.ExchangeCheckBox();
            this.chxKraken = new ArbitrationSimulator.UiObjects.ExchangeCheckBox();
            this.chxAnx = new ArbitrationSimulator.UiObjects.ExchangeCheckBox();
            this.chxBtce = new ArbitrationSimulator.UiObjects.ExchangeCheckBox();
            this.chxItBit = new ArbitrationSimulator.UiObjects.ExchangeCheckBox();
            this.startButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAccount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTrades)).BeginInit();
            this.panel1.SuspendLayout();
            this.transferCompleteGroupbox.SuspendLayout();
            this.transferModeGroupBox.SuspendLayout();
            this.arbitrationModeGroupBox.SuspendLayout();
            this.filterRulesGroupBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txbTrades
            // 
            this.txbTrades.BackColor = System.Drawing.SystemColors.InfoText;
            this.txbTrades.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txbTrades.ForeColor = System.Drawing.SystemColors.Window;
            this.txbTrades.Location = new System.Drawing.Point(1095, 12);
            this.txbTrades.Multiline = true;
            this.txbTrades.Name = "txbTrades";
            this.txbTrades.ReadOnly = true;
            this.txbTrades.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txbTrades.Size = new System.Drawing.Size(765, 784);
            this.txbTrades.TabIndex = 2;
            // 
            // dgvAccount
            // 
            this.dgvAccount.AllowUserToAddRows = false;
            this.dgvAccount.AllowUserToDeleteRows = false;
            this.dgvAccount.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvAccount.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAccount.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ExchangeName,
            this.BtcAmount,
            this.FiatAmount,
            this.OpenOrders});
            this.dgvAccount.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvAccount.Location = new System.Drawing.Point(13, 182);
            this.dgvAccount.Name = "dgvAccount";
            this.dgvAccount.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvAccount.Size = new System.Drawing.Size(809, 224);
            this.dgvAccount.TabIndex = 6;
            // 
            // ExchangeName
            // 
            this.ExchangeName.HeaderText = "Exchange";
            this.ExchangeName.Name = "ExchangeName";
            this.ExchangeName.ReadOnly = true;
            this.ExchangeName.Width = 150;
            // 
            // BtcAmount
            // 
            this.BtcAmount.HeaderText = "BTC Amount";
            this.BtcAmount.Name = "BtcAmount";
            this.BtcAmount.ReadOnly = true;
            this.BtcAmount.Width = 200;
            // 
            // FiatAmount
            // 
            this.FiatAmount.HeaderText = "Fiat Amount";
            this.FiatAmount.Name = "FiatAmount";
            this.FiatAmount.ReadOnly = true;
            this.FiatAmount.Width = 200;
            // 
            // OpenOrders
            // 
            this.OpenOrders.HeaderText = "Open Orders";
            this.OpenOrders.Name = "OpenOrders";
            this.OpenOrders.ReadOnly = true;
            this.OpenOrders.Width = 200;
            // 
            // dgvTrades
            // 
            this.dgvTrades.AllowUserToAddRows = false;
            this.dgvTrades.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleVertical;
            this.dgvTrades.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvTrades.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTrades.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Id,
            this.Time,
            this.BuyExchange,
            this.SellExchange,
            this.Profit,
            this.Amount,
            this.BuyPrice,
            this.TotalBuyCost,
            this.SellPrice,
            this.TotalSellCost});
            this.dgvTrades.Location = new System.Drawing.Point(13, 412);
            this.dgvTrades.Name = "dgvTrades";
            this.dgvTrades.ReadOnly = true;
            this.dgvTrades.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvTrades.Size = new System.Drawing.Size(1076, 384);
            this.dgvTrades.TabIndex = 7;
            // 
            // Id
            // 
            this.Id.HeaderText = "ID";
            this.Id.Name = "Id";
            this.Id.ReadOnly = true;
            // 
            // Time
            // 
            this.Time.HeaderText = "Time";
            this.Time.Name = "Time";
            this.Time.ReadOnly = true;
            this.Time.Width = 120;
            // 
            // BuyExchange
            // 
            this.BuyExchange.HeaderText = "Buy Exchange";
            this.BuyExchange.Name = "BuyExchange";
            this.BuyExchange.ReadOnly = true;
            // 
            // SellExchange
            // 
            this.SellExchange.HeaderText = "Sell Exchange";
            this.SellExchange.Name = "SellExchange";
            this.SellExchange.ReadOnly = true;
            // 
            // Profit
            // 
            this.Profit.HeaderText = "Profit";
            this.Profit.Name = "Profit";
            this.Profit.ReadOnly = true;
            // 
            // Amount
            // 
            this.Amount.HeaderText = "Amount";
            this.Amount.Name = "Amount";
            this.Amount.ReadOnly = true;
            // 
            // BuyPrice
            // 
            this.BuyPrice.HeaderText = "Buy Price";
            this.BuyPrice.Name = "BuyPrice";
            this.BuyPrice.ReadOnly = true;
            // 
            // TotalBuyCost
            // 
            this.TotalBuyCost.HeaderText = "TotalBuyCost";
            this.TotalBuyCost.Name = "TotalBuyCost";
            this.TotalBuyCost.ReadOnly = true;
            // 
            // SellPrice
            // 
            this.SellPrice.HeaderText = "Sell Price";
            this.SellPrice.Name = "SellPrice";
            this.SellPrice.ReadOnly = true;
            // 
            // TotalSellCost
            // 
            this.TotalSellCost.HeaderText = "TotalSellCost";
            this.TotalSellCost.Name = "TotalSellCost";
            this.TotalSellCost.ReadOnly = true;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.lblRoundsRequiredForValidation);
            this.panel1.Controls.Add(this.txbRoundsRequiredForValidation);
            this.panel1.Controls.Add(this.completeTransfersButton);
            this.panel1.Controls.Add(this.stopButton);
            this.panel1.Controls.Add(this.transferCompleteGroupbox);
            this.panel1.Controls.Add(this.transferModeGroupBox);
            this.panel1.Controls.Add(this.arbitrationModeGroupBox);
            this.panel1.Controls.Add(this.lblMaxFiatLabel);
            this.panel1.Controls.Add(this.txbMaxFiatTradeAmount);
            this.panel1.Controls.Add(this.filterRulesGroupBox);
            this.panel1.Controls.Add(this.lblMaxBtcLabel);
            this.panel1.Controls.Add(this.txbMaxBtcTradeAmount);
            this.panel1.Controls.Add(this.lblArbitrationHunterOutput);
            this.panel1.Controls.Add(this.txbLogFileName);
            this.panel1.Controls.Add(this.lblSearchInterval);
            this.panel1.Controls.Add(this.txbSearchInterval);
            this.panel1.Controls.Add(this.txbMinProfit);
            this.panel1.Controls.Add(this.lblMinProfit);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.startButton);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1075, 164);
            this.panel1.TabIndex = 4;
            // 
            // lblRoundsRequiredForValidation
            // 
            this.lblRoundsRequiredForValidation.AutoSize = true;
            this.lblRoundsRequiredForValidation.Location = new System.Drawing.Point(795, 40);
            this.lblRoundsRequiredForValidation.Name = "lblRoundsRequiredForValidation";
            this.lblRoundsRequiredForValidation.Size = new System.Drawing.Size(154, 13);
            this.lblRoundsRequiredForValidation.TabIndex = 8;
            this.lblRoundsRequiredForValidation.Text = "Rounds Required for Validation";
            this.lblRoundsRequiredForValidation.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txbRoundsRequiredForValidation
            // 
            this.txbRoundsRequiredForValidation.Location = new System.Drawing.Point(955, 33);
            this.txbRoundsRequiredForValidation.Name = "txbRoundsRequiredForValidation";
            this.txbRoundsRequiredForValidation.Size = new System.Drawing.Size(100, 20);
            this.txbRoundsRequiredForValidation.TabIndex = 8;
            // 
            // completeTransfersButton
            // 
            this.completeTransfersButton.BackColor = System.Drawing.Color.Gray;
            this.completeTransfersButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.completeTransfersButton.ForeColor = System.Drawing.Color.White;
            this.completeTransfersButton.Location = new System.Drawing.Point(688, 122);
            this.completeTransfersButton.Name = "completeTransfersButton";
            this.completeTransfersButton.Size = new System.Drawing.Size(67, 23);
            this.completeTransfersButton.TabIndex = 16;
            this.completeTransfersButton.Text = "Complete";
            this.completeTransfersButton.UseVisualStyleBackColor = false;
            this.completeTransfersButton.Click += new System.EventHandler(this.CompleteTransfersButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.BackColor = System.Drawing.Color.Maroon;
            this.stopButton.Enabled = false;
            this.stopButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.stopButton.ForeColor = System.Drawing.Color.White;
            this.stopButton.Location = new System.Drawing.Point(186, 133);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(169, 23);
            this.stopButton.TabIndex = 8;
            this.stopButton.Text = "St&op";
            this.stopButton.UseVisualStyleBackColor = false;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // transferCompleteGroupbox
            // 
            this.transferCompleteGroupbox.Controls.Add(this.txbRunId);
            this.transferCompleteGroupbox.Controls.Add(this.lblRunId);
            this.transferCompleteGroupbox.Location = new System.Drawing.Point(675, 3);
            this.transferCompleteGroupbox.Name = "transferCompleteGroupbox";
            this.transferCompleteGroupbox.Size = new System.Drawing.Size(97, 152);
            this.transferCompleteGroupbox.TabIndex = 15;
            this.transferCompleteGroupbox.TabStop = false;
            this.transferCompleteGroupbox.Text = "Transfer Complete";
            // 
            // txbRunId
            // 
            this.txbRunId.Location = new System.Drawing.Point(6, 62);
            this.txbRunId.Name = "txbRunId";
            this.txbRunId.Size = new System.Drawing.Size(85, 20);
            this.txbRunId.TabIndex = 10;
            // 
            // lblRunId
            // 
            this.lblRunId.AutoSize = true;
            this.lblRunId.Location = new System.Drawing.Point(6, 46);
            this.lblRunId.Name = "lblRunId";
            this.lblRunId.Size = new System.Drawing.Size(41, 13);
            this.lblRunId.TabIndex = 10;
            this.lblRunId.Text = "Run ID";
            this.lblRunId.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // transferModeGroupBox
            // 
            this.transferModeGroupBox.Controls.Add(this.rbTransferModeNone);
            this.transferModeGroupBox.Controls.Add(this.lblRollupHours);
            this.transferModeGroupBox.Controls.Add(this.txbRollupHours);
            this.transferModeGroupBox.Controls.Add(this.rbTransferModeRollupByHours);
            this.transferModeGroupBox.Controls.Add(this.lblRollupNumber);
            this.transferModeGroupBox.Controls.Add(this.txbRollupNumber);
            this.transferModeGroupBox.Controls.Add(this.rbTransferModeRollupOnTrades);
            this.transferModeGroupBox.Controls.Add(this.rbTransferModeOnTime);
            this.transferModeGroupBox.Location = new System.Drawing.Point(413, 3);
            this.transferModeGroupBox.Name = "transferModeGroupBox";
            this.transferModeGroupBox.Size = new System.Drawing.Size(148, 152);
            this.transferModeGroupBox.TabIndex = 14;
            this.transferModeGroupBox.TabStop = false;
            this.transferModeGroupBox.Text = "Transfer Mode";
            // 
            // rbTransferModeNone
            // 
            this.rbTransferModeNone.AutoSize = true;
            this.rbTransferModeNone.Checked = true;
            this.rbTransferModeNone.Location = new System.Drawing.Point(7, 132);
            this.rbTransferModeNone.Name = "rbTransferModeNone";
            this.rbTransferModeNone.Size = new System.Drawing.Size(51, 17);
            this.rbTransferModeNone.TabIndex = 8;
            this.rbTransferModeNone.TabStop = true;
            this.rbTransferModeNone.Text = "None";
            this.rbTransferModeNone.UseVisualStyleBackColor = true;
            // 
            // lblRollupHours
            // 
            this.lblRollupHours.AutoSize = true;
            this.lblRollupHours.Enabled = false;
            this.lblRollupHours.Location = new System.Drawing.Point(67, 109);
            this.lblRollupHours.Name = "lblRollupHours";
            this.lblRollupHours.Size = new System.Drawing.Size(35, 13);
            this.lblRollupHours.TabIndex = 16;
            this.lblRollupHours.Text = "Hours";
            this.lblRollupHours.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txbRollupHours
            // 
            this.txbRollupHours.Enabled = false;
            this.txbRollupHours.Location = new System.Drawing.Point(23, 106);
            this.txbRollupHours.Name = "txbRollupHours";
            this.txbRollupHours.Size = new System.Drawing.Size(38, 20);
            this.txbRollupHours.TabIndex = 16;
            // 
            // rbTransferModeRollupByHours
            // 
            this.rbTransferModeRollupByHours.AutoSize = true;
            this.rbTransferModeRollupByHours.Enabled = false;
            this.rbTransferModeRollupByHours.Location = new System.Drawing.Point(7, 83);
            this.rbTransferModeRollupByHours.Name = "rbTransferModeRollupByHours";
            this.rbTransferModeRollupByHours.Size = new System.Drawing.Size(101, 17);
            this.rbTransferModeRollupByHours.TabIndex = 15;
            this.rbTransferModeRollupByHours.Text = "Rollup By Hours";
            this.rbTransferModeRollupByHours.UseVisualStyleBackColor = true;
            this.rbTransferModeRollupByHours.CheckedChanged += new System.EventHandler(this.TransferModeGroupBox_CheckChanged);
            // 
            // lblRollupNumber
            // 
            this.lblRollupNumber.AutoSize = true;
            this.lblRollupNumber.Enabled = false;
            this.lblRollupNumber.Location = new System.Drawing.Point(67, 60);
            this.lblRollupNumber.Name = "lblRollupNumber";
            this.lblRollupNumber.Size = new System.Drawing.Size(72, 13);
            this.lblRollupNumber.TabIndex = 15;
            this.lblRollupNumber.Text = "No. of Trades";
            this.lblRollupNumber.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txbRollupNumber
            // 
            this.txbRollupNumber.Enabled = false;
            this.txbRollupNumber.Location = new System.Drawing.Point(23, 57);
            this.txbRollupNumber.Name = "txbRollupNumber";
            this.txbRollupNumber.Size = new System.Drawing.Size(38, 20);
            this.txbRollupNumber.TabIndex = 15;
            // 
            // rbTransferModeRollupOnTrades
            // 
            this.rbTransferModeRollupOnTrades.AutoSize = true;
            this.rbTransferModeRollupOnTrades.Location = new System.Drawing.Point(6, 41);
            this.rbTransferModeRollupOnTrades.Name = "rbTransferModeRollupOnTrades";
            this.rbTransferModeRollupOnTrades.Size = new System.Drawing.Size(108, 17);
            this.rbTransferModeRollupOnTrades.TabIndex = 1;
            this.rbTransferModeRollupOnTrades.Text = "Rollup On Trades";
            this.rbTransferModeRollupOnTrades.UseVisualStyleBackColor = true;
            this.rbTransferModeRollupOnTrades.CheckedChanged += new System.EventHandler(this.TransferModeGroupBox_CheckChanged);
            // 
            // rbTransferModeOnTime
            // 
            this.rbTransferModeOnTime.AutoSize = true;
            this.rbTransferModeOnTime.Enabled = false;
            this.rbTransferModeOnTime.Location = new System.Drawing.Point(7, 20);
            this.rbTransferModeOnTime.Name = "rbTransferModeOnTime";
            this.rbTransferModeOnTime.Size = new System.Drawing.Size(65, 17);
            this.rbTransferModeOnTime.TabIndex = 0;
            this.rbTransferModeOnTime.Text = "On Time";
            this.rbTransferModeOnTime.UseVisualStyleBackColor = true;
            this.rbTransferModeOnTime.CheckedChanged += new System.EventHandler(this.TransferModeGroupBox_CheckChanged);
            // 
            // arbitrationModeGroupBox
            // 
            this.arbitrationModeGroupBox.Controls.Add(this.rbSimulation);
            this.arbitrationModeGroupBox.Controls.Add(this.rbLive);
            this.arbitrationModeGroupBox.Location = new System.Drawing.Point(567, 3);
            this.arbitrationModeGroupBox.Name = "arbitrationModeGroupBox";
            this.arbitrationModeGroupBox.Size = new System.Drawing.Size(102, 152);
            this.arbitrationModeGroupBox.TabIndex = 13;
            this.arbitrationModeGroupBox.TabStop = false;
            this.arbitrationModeGroupBox.Text = "Arbitration Mode";
            // 
            // rbSimulation
            // 
            this.rbSimulation.AutoSize = true;
            this.rbSimulation.Location = new System.Drawing.Point(6, 41);
            this.rbSimulation.Name = "rbSimulation";
            this.rbSimulation.Size = new System.Drawing.Size(73, 17);
            this.rbSimulation.TabIndex = 1;
            this.rbSimulation.Text = "Simulation";
            this.rbSimulation.UseVisualStyleBackColor = true;
            // 
            // rbLive
            // 
            this.rbLive.AutoSize = true;
            this.rbLive.Checked = true;
            this.rbLive.Location = new System.Drawing.Point(7, 18);
            this.rbLive.Name = "rbLive";
            this.rbLive.Size = new System.Drawing.Size(45, 17);
            this.rbLive.TabIndex = 0;
            this.rbLive.TabStop = true;
            this.rbLive.Text = "Live";
            this.rbLive.UseVisualStyleBackColor = true;
            // 
            // lblMaxFiatLabel
            // 
            this.lblMaxFiatLabel.AutoSize = true;
            this.lblMaxFiatLabel.Location = new System.Drawing.Point(858, 112);
            this.lblMaxFiatLabel.Name = "lblMaxFiatLabel";
            this.lblMaxFiatLabel.Size = new System.Drawing.Size(93, 13);
            this.lblMaxFiatLabel.TabIndex = 12;
            this.lblMaxFiatLabel.Text = "Max Fiat For Tade";
            this.lblMaxFiatLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txbMaxFiatTradeAmount
            // 
            this.txbMaxFiatTradeAmount.Location = new System.Drawing.Point(955, 109);
            this.txbMaxFiatTradeAmount.Name = "txbMaxFiatTradeAmount";
            this.txbMaxFiatTradeAmount.Size = new System.Drawing.Size(100, 20);
            this.txbMaxFiatTradeAmount.TabIndex = 11;
            // 
            // filterRulesGroupBox
            // 
            this.filterRulesGroupBox.Controls.Add(this.lblPercentRestriction);
            this.filterRulesGroupBox.Controls.Add(this.txbRestrictionPercent);
            this.filterRulesGroupBox.Controls.Add(this.rbMostProfitableWithPercentRestriction);
            this.filterRulesGroupBox.Controls.Add(this.rbExchangeWithLeastBtc);
            this.filterRulesGroupBox.Controls.Add(this.rbMostProfitable);
            this.filterRulesGroupBox.Location = new System.Drawing.Point(186, 3);
            this.filterRulesGroupBox.Name = "filterRulesGroupBox";
            this.filterRulesGroupBox.Size = new System.Drawing.Size(221, 122);
            this.filterRulesGroupBox.TabIndex = 10;
            this.filterRulesGroupBox.TabStop = false;
            this.filterRulesGroupBox.Text = "Filter Rules";
            // 
            // lblPercentRestriction
            // 
            this.lblPercentRestriction.AutoSize = true;
            this.lblPercentRestriction.Enabled = false;
            this.lblPercentRestriction.Location = new System.Drawing.Point(67, 60);
            this.lblPercentRestriction.Name = "lblPercentRestriction";
            this.lblPercentRestriction.Size = new System.Drawing.Size(136, 13);
            this.lblPercentRestriction.TabIndex = 13;
            this.lblPercentRestriction.Text = "Percent Restriction Amount";
            this.lblPercentRestriction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txbRestrictionPercent
            // 
            this.txbRestrictionPercent.Enabled = false;
            this.txbRestrictionPercent.Location = new System.Drawing.Point(24, 57);
            this.txbRestrictionPercent.Name = "txbRestrictionPercent";
            this.txbRestrictionPercent.Size = new System.Drawing.Size(38, 20);
            this.txbRestrictionPercent.TabIndex = 13;
            this.txbRestrictionPercent.Text = "25";
            // 
            // rbMostProfitableWithPercentRestriction
            // 
            this.rbMostProfitableWithPercentRestriction.AutoSize = true;
            this.rbMostProfitableWithPercentRestriction.Checked = true;
            this.rbMostProfitableWithPercentRestriction.Location = new System.Drawing.Point(6, 41);
            this.rbMostProfitableWithPercentRestriction.Name = "rbMostProfitableWithPercentRestriction";
            this.rbMostProfitableWithPercentRestriction.Size = new System.Drawing.Size(213, 17);
            this.rbMostProfitableWithPercentRestriction.TabIndex = 13;
            this.rbMostProfitableWithPercentRestriction.TabStop = true;
            this.rbMostProfitableWithPercentRestriction.Text = "Most Profitable With Percent Restriction";
            this.rbMostProfitableWithPercentRestriction.UseVisualStyleBackColor = true;
            this.rbMostProfitableWithPercentRestriction.CheckedChanged += new System.EventHandler(this.FilterGroupBox_CheckChanged);
            // 
            // rbExchangeWithLeastBtc
            // 
            this.rbExchangeWithLeastBtc.AutoSize = true;
            this.rbExchangeWithLeastBtc.Location = new System.Drawing.Point(6, 87);
            this.rbExchangeWithLeastBtc.Name = "rbExchangeWithLeastBtc";
            this.rbExchangeWithLeastBtc.Size = new System.Drawing.Size(202, 17);
            this.rbExchangeWithLeastBtc.TabIndex = 11;
            this.rbExchangeWithLeastBtc.Text = "Exchange With Least Amount of BTC";
            this.rbExchangeWithLeastBtc.UseVisualStyleBackColor = true;
            this.rbExchangeWithLeastBtc.CheckedChanged += new System.EventHandler(this.FilterGroupBox_CheckChanged);
            // 
            // rbMostProfitable
            // 
            this.rbMostProfitable.AutoSize = true;
            this.rbMostProfitable.Location = new System.Drawing.Point(6, 19);
            this.rbMostProfitable.Name = "rbMostProfitable";
            this.rbMostProfitable.Size = new System.Drawing.Size(152, 17);
            this.rbMostProfitable.TabIndex = 0;
            this.rbMostProfitable.Text = "Most Profitable Opportunity";
            this.rbMostProfitable.UseVisualStyleBackColor = true;
            this.rbMostProfitable.CheckedChanged += new System.EventHandler(this.FilterGroupBox_CheckChanged);
            // 
            // lblMaxBtcLabel
            // 
            this.lblMaxBtcLabel.AutoSize = true;
            this.lblMaxBtcLabel.Location = new System.Drawing.Point(858, 83);
            this.lblMaxBtcLabel.Name = "lblMaxBtcLabel";
            this.lblMaxBtcLabel.Size = new System.Drawing.Size(91, 13);
            this.lblMaxBtcLabel.TabIndex = 9;
            this.lblMaxBtcLabel.Text = "Max BTC to Tade";
            this.lblMaxBtcLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txbMaxBtcTradeAmount
            // 
            this.txbMaxBtcTradeAmount.Location = new System.Drawing.Point(955, 83);
            this.txbMaxBtcTradeAmount.Name = "txbMaxBtcTradeAmount";
            this.txbMaxBtcTradeAmount.Size = new System.Drawing.Size(100, 20);
            this.txbMaxBtcTradeAmount.TabIndex = 9;
            // 
            // lblArbitrationHunterOutput
            // 
            this.lblArbitrationHunterOutput.AutoSize = true;
            this.lblArbitrationHunterOutput.Location = new System.Drawing.Point(825, 133);
            this.lblArbitrationHunterOutput.Name = "lblArbitrationHunterOutput";
            this.lblArbitrationHunterOutput.Size = new System.Drawing.Size(124, 13);
            this.lblArbitrationHunterOutput.TabIndex = 8;
            this.lblArbitrationHunterOutput.Text = "Arbitration Hunter Output";
            this.lblArbitrationHunterOutput.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txbLogFileName
            // 
            this.txbLogFileName.Location = new System.Drawing.Point(955, 130);
            this.txbLogFileName.Name = "txbLogFileName";
            this.txbLogFileName.Size = new System.Drawing.Size(100, 20);
            this.txbLogFileName.TabIndex = 7;
            this.txbLogFileName.Text = "HunterOutput.csv";
            // 
            // lblSearchInterval
            // 
            this.lblSearchInterval.AutoSize = true;
            this.lblSearchInterval.Location = new System.Drawing.Point(821, 64);
            this.lblSearchInterval.Name = "lblSearchInterval";
            this.lblSearchInterval.Size = new System.Drawing.Size(128, 13);
            this.lblSearchInterval.TabIndex = 6;
            this.lblSearchInterval.Text = "Search Interval (seconds)";
            this.lblSearchInterval.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txbSearchInterval
            // 
            this.txbSearchInterval.Location = new System.Drawing.Point(955, 57);
            this.txbSearchInterval.Name = "txbSearchInterval";
            this.txbSearchInterval.Size = new System.Drawing.Size(100, 20);
            this.txbSearchInterval.TabIndex = 5;
            // 
            // txbMinProfit
            // 
            this.txbMinProfit.Location = new System.Drawing.Point(955, 7);
            this.txbMinProfit.Name = "txbMinProfit";
            this.txbMinProfit.Size = new System.Drawing.Size(100, 20);
            this.txbMinProfit.TabIndex = 4;
            // 
            // lblMinProfit
            // 
            this.lblMinProfit.AutoSize = true;
            this.lblMinProfit.Location = new System.Drawing.Point(778, 10);
            this.lblMinProfit.Name = "lblMinProfit";
            this.lblMinProfit.Size = new System.Drawing.Size(171, 13);
            this.lblMinProfit.TabIndex = 3;
            this.lblMinProfit.Text = "Minimum Profit for Abritration Trade";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chxBitX);
            this.groupBox1.Controls.Add(this.chxBitfinex);
            this.groupBox1.Controls.Add(this.chxCoinbase);
            this.groupBox1.Controls.Add(this.chxOkCoin);
            this.groupBox1.Controls.Add(this.chxBitstamp);
            this.groupBox1.Controls.Add(this.chxKraken);
            this.groupBox1.Controls.Add(this.chxAnx);
            this.groupBox1.Controls.Add(this.chxBtce);
            this.groupBox1.Controls.Add(this.chxItBit);
            this.groupBox1.Location = new System.Drawing.Point(13, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(167, 122);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Exchanges";
            // 
            // chxBitX
            // 
            this.chxBitX.AutoSize = true;
            this.chxBitX.Location = new System.Drawing.Point(6, 99);
            this.chxBitX.Name = "chxBitX";
            this.chxBitX.Size = new System.Drawing.Size(48, 17);
            this.chxBitX.TabIndex = 12;
            this.chxBitX.Text = "Bit-X";
            this.chxBitX.UseVisualStyleBackColor = true;
            this.chxBitX.Visible = false;
            // 
            // chxBitfinex
            // 
            this.chxBitfinex.AutoSize = true;
            this.chxBitfinex.Checked = true;
            this.chxBitfinex.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chxBitfinex.Location = new System.Drawing.Point(6, 36);
            this.chxBitfinex.Name = "chxBitfinex";
            this.chxBitfinex.Size = new System.Drawing.Size(60, 17);
            this.chxBitfinex.TabIndex = 11;
            this.chxBitfinex.Text = "Bitfinex";
            this.chxBitfinex.UseVisualStyleBackColor = true;
            // 
            // chxCoinbase
            // 
            this.chxCoinbase.AutoSize = true;
            this.chxCoinbase.Checked = true;
            this.chxCoinbase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chxCoinbase.Location = new System.Drawing.Point(91, 18);
            this.chxCoinbase.Name = "chxCoinbase";
            this.chxCoinbase.Size = new System.Drawing.Size(70, 17);
            this.chxCoinbase.TabIndex = 10;
            this.chxCoinbase.Text = "Coinbase";
            this.chxCoinbase.UseVisualStyleBackColor = true;
            // 
            // chxOkCoin
            // 
            this.chxOkCoin.AutoSize = true;
            this.chxOkCoin.Checked = true;
            this.chxOkCoin.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chxOkCoin.Location = new System.Drawing.Point(90, 76);
            this.chxOkCoin.Name = "chxOkCoin";
            this.chxOkCoin.Size = new System.Drawing.Size(61, 17);
            this.chxOkCoin.TabIndex = 1;
            this.chxOkCoin.Text = "OkCoin";
            this.chxOkCoin.UseVisualStyleBackColor = true;
            // 
            // chxBitstamp
            // 
            this.chxBitstamp.AutoSize = true;
            this.chxBitstamp.Checked = true;
            this.chxBitstamp.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chxBitstamp.Location = new System.Drawing.Point(6, 56);
            this.chxBitstamp.Name = "chxBitstamp";
            this.chxBitstamp.Size = new System.Drawing.Size(66, 17);
            this.chxBitstamp.TabIndex = 5;
            this.chxBitstamp.Text = "Bitstamp";
            this.chxBitstamp.UseVisualStyleBackColor = true;
            // 
            // chxKraken
            // 
            this.chxKraken.AutoSize = true;
            this.chxKraken.Checked = true;
            this.chxKraken.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chxKraken.Location = new System.Drawing.Point(91, 56);
            this.chxKraken.Name = "chxKraken";
            this.chxKraken.Size = new System.Drawing.Size(60, 17);
            this.chxKraken.TabIndex = 9;
            this.chxKraken.Text = "Kraken";
            this.chxKraken.UseVisualStyleBackColor = true;
            // 
            // chxAnx
            // 
            this.chxAnx.AutoSize = true;
            this.chxAnx.Enabled = false;
            this.chxAnx.Location = new System.Drawing.Point(6, 19);
            this.chxAnx.Name = "chxAnx";
            this.chxAnx.Size = new System.Drawing.Size(48, 17);
            this.chxAnx.TabIndex = 2;
            this.chxAnx.Text = "ANX";
            this.chxAnx.UseVisualStyleBackColor = true;
            // 
            // chxBtce
            // 
            this.chxBtce.AutoSize = true;
            this.chxBtce.Checked = true;
            this.chxBtce.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chxBtce.Location = new System.Drawing.Point(6, 76);
            this.chxBtce.Name = "chxBtce";
            this.chxBtce.Size = new System.Drawing.Size(57, 17);
            this.chxBtce.TabIndex = 0;
            this.chxBtce.Text = "BTC-E";
            this.chxBtce.UseVisualStyleBackColor = true;
            // 
            // chxItBit
            // 
            this.chxItBit.AutoSize = true;
            this.chxItBit.Checked = true;
            this.chxItBit.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chxItBit.Location = new System.Drawing.Point(91, 36);
            this.chxItBit.Name = "chxItBit";
            this.chxItBit.Size = new System.Drawing.Size(44, 17);
            this.chxItBit.TabIndex = 7;
            this.chxItBit.Text = "ItBit";
            this.chxItBit.UseVisualStyleBackColor = true;
            // 
            // startButton
            // 
            this.startButton.AutoEllipsis = true;
            this.startButton.BackColor = System.Drawing.Color.DarkGreen;
            this.startButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.startButton.ForeColor = System.Drawing.Color.White;
            this.startButton.Location = new System.Drawing.Point(11, 132);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(169, 23);
            this.startButton.TabIndex = 0;
            this.startButton.Text = "St&art";
            this.startButton.UseVisualStyleBackColor = false;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // ArbitrationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(1860, 808);
            this.Controls.Add(this.dgvTrades);
            this.Controls.Add(this.dgvAccount);
            this.Controls.Add(this.txbTrades);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ArbitrationForm";
            this.Text = "Bitcoin Arbitrator";
            ((System.ComponentModel.ISupportInitialize)(this.dgvAccount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTrades)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.transferCompleteGroupbox.ResumeLayout(false);
            this.transferCompleteGroupbox.PerformLayout();
            this.transferModeGroupBox.ResumeLayout(false);
            this.transferModeGroupBox.PerformLayout();
            this.arbitrationModeGroupBox.ResumeLayout(false);
            this.arbitrationModeGroupBox.PerformLayout();
            this.filterRulesGroupBox.ResumeLayout(false);
            this.filterRulesGroupBox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox txbTrades;
        private Button startButton;
        private CustomPanel panel1;
        private TextBox txbMinProfit;
        private Label lblMinProfit;
        private GroupBox groupBox1;
        private ExchangeCheckBox chxBtce;
        private Label lblSearchInterval;
        private DataGridView dgvAccount;
        private DataGridView dgvTrades;
        private Button stopButton;
        private Label lblArbitrationHunterOutput;
        private TextBox txbLogFileName;
        private TextBox txbSearchInterval;
        private Label lblMaxBtcLabel;
        private TextBox txbMaxBtcTradeAmount;
        private ExchangeCheckBox chxBitstamp;
        private ExchangeCheckBox chxAnx;
        private ExchangeCheckBox chxItBit;
        private ExchangeCheckBox chxKraken;
        private GroupBox filterRulesGroupBox;
        private OpportunitySelectionRadioButton rbExchangeWithLeastBtc;
        private OpportunitySelectionRadioButton rbMostProfitable;
        private TextBox txbMaxFiatTradeAmount;
        private Label lblMaxFiatLabel;
        private DataGridViewTextBoxColumn Id;
        private DataGridViewTextBoxColumn Time;
        private DataGridViewTextBoxColumn BuyExchange;
        private DataGridViewTextBoxColumn SellExchange;
        private DataGridViewTextBoxColumn Profit;
        private DataGridViewTextBoxColumn Amount;
        private DataGridViewTextBoxColumn BuyPrice;
        private DataGridViewTextBoxColumn TotalBuyCost;
        private DataGridViewTextBoxColumn SellPrice;
        private DataGridViewTextBoxColumn TotalSellCost;
        private Label lblPercentRestriction;
        private TextBox txbRestrictionPercent;
        private OpportunitySelectionRadioButton rbMostProfitableWithPercentRestriction;
        private GroupBox arbitrationModeGroupBox;
        private ArbitrationModeRadioButton rbSimulation;
        private ArbitrationModeRadioButton rbLive;
        private GroupBox transferModeGroupBox;
        private TransferModeRadioButton rbTransferModeRollupOnTrades;
        private TransferModeRadioButton rbTransferModeOnTime;
        private Label lblRollupNumber;
        private TextBox txbRollupNumber;
        private Label lblRollupHours;
        private TextBox txbRollupHours;
        private TransferModeRadioButton rbTransferModeRollupByHours;
        private Button completeTransfersButton;
        private GroupBox transferCompleteGroupbox;
        private TextBox txbRunId;
        private Label lblRunId;
        private Label lblRoundsRequiredForValidation;
        private TextBox txbRoundsRequiredForValidation;
        private TransferModeRadioButton rbTransferModeNone;
        private ExchangeCheckBox chxOkCoin;
        private DataGridViewTextBoxColumn ExchangeName;
        private DataGridViewTextBoxColumn BtcAmount;
        private DataGridViewTextBoxColumn FiatAmount;
        private DataGridViewTextBoxColumn OpenOrders;
        private ExchangeCheckBox chxCoinbase;
        private ExchangeCheckBox chxBitfinex;
        private ExchangeCheckBox chxBitX;
    }
}

