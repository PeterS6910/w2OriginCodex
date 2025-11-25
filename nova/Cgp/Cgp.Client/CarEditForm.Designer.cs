namespace Contal.Cgp.Client
{
    partial class CarEditForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox _eLp;
        private System.Windows.Forms.TextBox _eBrand;
        private System.Windows.Forms.TextBox _eDescription;
        private System.Windows.Forms.TextBox _eUtcDateStateLastChange;
        private System.Windows.Forms.Button _bOk;
        private System.Windows.Forms.Button _bCancel;
        private System.Windows.Forms.TabControl _tcCar;
        private System.Windows.Forms.TabPage _tpInformation;
        private System.Windows.Forms.TabPage _tpCards;
        private System.Windows.Forms.TabPage _tpDoorEnvironment;
        private System.Windows.Forms.ListView _lvAssignedCards;
        private System.Windows.Forms.ListView _lvAvailableCards;
        private System.Windows.Forms.Button _bAssignCard;
        private System.Windows.Forms.Button _bUnassignCard;
        private System.Windows.Forms.Label _lAssignedCards;
        private System.Windows.Forms.Label _lAvailableCards;
        private System.Windows.Forms.Button _bDeleteDoorEnvironment;
        private System.Windows.Forms.Button _bEditDoorEnvironment;
        private System.Windows.Forms.Button _bAddDoorEnvironment;
        private Contal.Cgp.Components.CgpDataGridView _dgDoorEnvironments;
        private System.Windows.Forms.DataGridViewImageColumn _tcSymbolColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _tcDoorEnvironmentColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _tcAccessTypeColumn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private IwQuick.UI.TextBoxDatePicker _dpValidityDateFrom;
        private IwQuick.UI.TextBoxDatePicker _dpValidityDateTo;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CarEditForm));
            this._eLp = new System.Windows.Forms.TextBox();
            this._eBrand = new System.Windows.Forms.TextBox();
            this._eDescription = new System.Windows.Forms.TextBox();
            this._eUtcDateStateLastChange = new System.Windows.Forms.TextBox();
            this._bOk = new System.Windows.Forms.Button();
            this._bCancel = new System.Windows.Forms.Button();
            this._tcCar = new System.Windows.Forms.TabControl();
            this._tpInformation = new System.Windows.Forms.TabPage();
            this._dpValidityDateTo = new Contal.IwQuick.UI.TextBoxDatePicker();
            this._dpValidityDateFrom = new Contal.IwQuick.UI.TextBoxDatePicker();
            this.label8 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._tpCards = new System.Windows.Forms.TabPage();
            this._lAvailableCards = new System.Windows.Forms.Label();
            this._lAssignedCards = new System.Windows.Forms.Label();
            this._bUnassignCard = new System.Windows.Forms.Button();
            this._bAssignCard = new System.Windows.Forms.Button();
            this._lvAvailableCards = new System.Windows.Forms.ListView();
            this._lvAssignedCards = new System.Windows.Forms.ListView();
            this._tpDoorEnvironment = new System.Windows.Forms.TabPage();
            this._bDeleteDoorEnvironment = new System.Windows.Forms.Button();
            this._bEditDoorEnvironment = new System.Windows.Forms.Button();
            this._bAddDoorEnvironment = new System.Windows.Forms.Button();
            this._dgDoorEnvironments = new Contal.Cgp.Components.CgpDataGridView();
            this._tcSymbolColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this._tcDoorEnvironmentColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._tcAccessTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._tcCar.SuspendLayout();
            this._tpInformation.SuspendLayout();
            this._tpCards.SuspendLayout();
            this._tpDoorEnvironment.SuspendLayout();
            this.SuspendLayout();
            // 
            // _eLp
            // 
            resources.ApplyResources(this._eLp, "_eLp");
            this._eLp.Name = "_eLp";
            this._eLp.Tag = "s";
            // 
            // _eBrand
            // 
            resources.ApplyResources(this._eBrand, "_eBrand");
            this._eBrand.Name = "_eBrand";
            // 
            // _eDescription
            // 
            resources.ApplyResources(this._eDescription, "_eDescription");
            this._eDescription.Name = "_eDescription";
            // 
            // _eUtcDateStateLastChange
            // 
            resources.ApplyResources(this._eUtcDateStateLastChange, "_eUtcDateStateLastChange");
            this._eUtcDateStateLastChange.Name = "_eUtcDateStateLastChange";
            // 
            // _bOk
            // 
            resources.ApplyResources(this._bOk, "_bOk");
            this._bOk.Name = "_bOk";
            this._bOk.Click += new System.EventHandler(this._bOk_Click);
            // 
            // _bCancel
            // 
            this._bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this._bCancel, "_bCancel");
            this._bCancel.Name = "_bCancel";
            this._bCancel.Click += new System.EventHandler(this._bCancel_Click);
            // 
            // _tcCar
            // 
            resources.ApplyResources(this._tcCar, "_tcCar");
            this._tcCar.Controls.Add(this._tpInformation);
            this._tcCar.Controls.Add(this._tpCards);
            this._tcCar.Controls.Add(this._tpDoorEnvironment);
            this._tcCar.Name = "_tcCar";
            this._tcCar.SelectedIndex = 0;
            this._tcCar.TabStop = false;
            this._tcCar.SelectedIndexChanged += new System.EventHandler(this._tcCar_SelectedIndexChanged);
            // 
            // _tpInformation
            // 
            this._tpInformation.Controls.Add(this._dpValidityDateTo);
            this._tpInformation.Controls.Add(this._dpValidityDateFrom);
            this._tpInformation.Controls.Add(this.label8);
            this._tpInformation.Controls.Add(this.label6);
            this._tpInformation.Controls.Add(this.label4);
            this._tpInformation.Controls.Add(this.label3);
            this._tpInformation.Controls.Add(this.label2);
            this._tpInformation.Controls.Add(this.label1);
            this._tpInformation.Controls.Add(this._eUtcDateStateLastChange);
            this._tpInformation.Controls.Add(this._eDescription);
            this._tpInformation.Controls.Add(this._eBrand);
            this._tpInformation.Controls.Add(this._eLp);
            resources.ApplyResources(this._tpInformation, "_tpInformation");
            this._tpInformation.Name = "_tpInformation";
            // 
            // _dpValidityDateTo
            // 
            this._dpValidityDateTo.addActualTime = false;
            resources.ApplyResources(this._dpValidityDateTo, "_dpValidityDateTo");
            this._dpValidityDateTo.BackColor = System.Drawing.Color.Transparent;
            this._dpValidityDateTo.ButtonClearDateImage = null;
            this._dpValidityDateTo.ButtonClearDateText = "";
            this._dpValidityDateTo.ButtonClearDateWidth = 23;
            this._dpValidityDateTo.ButtonDateImage = null;
            this._dpValidityDateTo.ButtonDateText = "";
            this._dpValidityDateTo.ButtonDateWidth = 23;
            this._dpValidityDateTo.CustomFormat = "d. M. yyyy HH:mm:ss";
            this._dpValidityDateTo.DateFormName = "Calendar";
            this._dpValidityDateTo.LocalizationHelper = null;
            this._dpValidityDateTo.Name = "_dpValidityDateTo";
            this._dpValidityDateTo.ReadOnly = false;
            this._dpValidityDateTo.SelectTime = Contal.IwQuick.UI.SelectedTimeOfDay.StartOfDay;
            this._dpValidityDateTo.ValidateAfter = 2D;
            this._dpValidityDateTo.ValidationEnabled = false;
            this._dpValidityDateTo.ValidationError = "";
            this._dpValidityDateTo.Value = null;
            // 
            // _dpValidityDateFrom
            // 
            this._dpValidityDateFrom.addActualTime = false;
            resources.ApplyResources(this._dpValidityDateFrom, "_dpValidityDateFrom");
            this._dpValidityDateFrom.BackColor = System.Drawing.Color.Transparent;
            this._dpValidityDateFrom.ButtonClearDateImage = null;
            this._dpValidityDateFrom.ButtonClearDateText = "";
            this._dpValidityDateFrom.ButtonClearDateWidth = 23;
            this._dpValidityDateFrom.ButtonDateImage = null;
            this._dpValidityDateFrom.ButtonDateText = "";
            this._dpValidityDateFrom.ButtonDateWidth = 23;
            this._dpValidityDateFrom.CustomFormat = "d. M. yyyy HH:mm:ss";
            this._dpValidityDateFrom.DateFormName = "Calendar";
            this._dpValidityDateFrom.LocalizationHelper = null;
            this._dpValidityDateFrom.Name = "_dpValidityDateFrom";
            this._dpValidityDateFrom.ReadOnly = false;
            this._dpValidityDateFrom.SelectTime = Contal.IwQuick.UI.SelectedTimeOfDay.StartOfDay;
            this._dpValidityDateFrom.ValidateAfter = 2D;
            this._dpValidityDateFrom.ValidationEnabled = false;
            this._dpValidityDateFrom.ValidationError = "";
            this._dpValidityDateFrom.Value = null;
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // _tpCards
            // 
            this._tpCards.Controls.Add(this._lAvailableCards);
            this._tpCards.Controls.Add(this._lAssignedCards);
            this._tpCards.Controls.Add(this._bUnassignCard);
            this._tpCards.Controls.Add(this._bAssignCard);
            this._tpCards.Controls.Add(this._lvAvailableCards);
            this._tpCards.Controls.Add(this._lvAssignedCards);
            resources.ApplyResources(this._tpCards, "_tpCards");
            this._tpCards.Name = "_tpCards";
            // 
            // _lAvailableCards
            // 
            resources.ApplyResources(this._lAvailableCards, "_lAvailableCards");
            this._lAvailableCards.Name = "_lAvailableCards";
            this._lAvailableCards.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _lAssignedCards
            // 
            resources.ApplyResources(this._lAssignedCards, "_lAssignedCards");
            this._lAssignedCards.Name = "_lAssignedCards";
            this._lAssignedCards.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _bUnassignCard
            // 
            resources.ApplyResources(this._bUnassignCard, "_bUnassignCard");
            this._bUnassignCard.Name = "_bUnassignCard";
            this._bUnassignCard.UseVisualStyleBackColor = true;
            this._bUnassignCard.Click += new System.EventHandler(this._bUnassignCard_Click);
            // 
            // _bAssignCard
            // 
            resources.ApplyResources(this._bAssignCard, "_bAssignCard");
            this._bAssignCard.Name = "_bAssignCard";
            this._bAssignCard.UseVisualStyleBackColor = true;
            this._bAssignCard.Click += new System.EventHandler(this._bAssignCard_Click);
            // 
            // _lvAvailableCards
            // 
            this._lvAvailableCards.HideSelection = false;
            resources.ApplyResources(this._lvAvailableCards, "_lvAvailableCards");
            this._lvAvailableCards.Name = "_lvAvailableCards";
            this._lvAvailableCards.UseCompatibleStateImageBehavior = false;
            this._lvAvailableCards.View = System.Windows.Forms.View.List;
            // 
            // _lvAssignedCards
            // 
            this._lvAssignedCards.HideSelection = false;
            resources.ApplyResources(this._lvAssignedCards, "_lvAssignedCards");
            this._lvAssignedCards.Name = "_lvAssignedCards";
            this._lvAssignedCards.UseCompatibleStateImageBehavior = false;
            this._lvAssignedCards.View = System.Windows.Forms.View.List;
            //
            // _tpDoorEnvironment
            //
            this._tpDoorEnvironment.Controls.Add(this._dgDoorEnvironments);
            this._tpDoorEnvironment.Controls.Add(this._bDeleteDoorEnvironment);
            this._tpDoorEnvironment.Controls.Add(this._bEditDoorEnvironment);
            this._tpDoorEnvironment.Controls.Add(this._bAddDoorEnvironment);
            this._tpDoorEnvironment.Location = new System.Drawing.Point(4, 29);
            this._tpDoorEnvironment.Name = "_tpDoorEnvironment";
            this._tpDoorEnvironment.Padding = new System.Windows.Forms.Padding(3);
            this._tpDoorEnvironment.Size = new System.Drawing.Size(588, 336);
            this._tpDoorEnvironment.TabIndex = 2;
            this._tpDoorEnvironment.Text = "DoorEnvironment";
            this._tpDoorEnvironment.UseVisualStyleBackColor = true;
            this._tpDoorEnvironment.Enter += new System.EventHandler(this._tpDoorEnvironment_Enter);
            //
            // _bDeleteDoorEnvironment
            //
            this._bDeleteDoorEnvironment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._bDeleteDoorEnvironment.Location = new System.Drawing.Point(486, 299);
            this._bDeleteDoorEnvironment.Name = "_bDeleteDoorEnvironment";
            this._bDeleteDoorEnvironment.Size = new System.Drawing.Size(96, 32);
            this._bDeleteDoorEnvironment.TabIndex = 3;
            this._bDeleteDoorEnvironment.Text = "Delete";
            this._bDeleteDoorEnvironment.UseVisualStyleBackColor = true;
            this._bDeleteDoorEnvironment.Click += new System.EventHandler(this._bDeleteDoorEnvironment_Click);
            //
            // _bEditDoorEnvironment
            //
            this._bEditDoorEnvironment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._bEditDoorEnvironment.Location = new System.Drawing.Point(108, 299);
            this._bEditDoorEnvironment.Name = "_bEditDoorEnvironment";
            this._bEditDoorEnvironment.Size = new System.Drawing.Size(96, 32);
            this._bEditDoorEnvironment.TabIndex = 2;
            this._bEditDoorEnvironment.Text = "Edit";
            this._bEditDoorEnvironment.UseVisualStyleBackColor = true;
            this._bEditDoorEnvironment.Click += new System.EventHandler(this._bEditDoorEnvironment_Click);
            //
            // _bAddDoorEnvironment
            //
            this._bAddDoorEnvironment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._bAddDoorEnvironment.Location = new System.Drawing.Point(6, 299);
            this._bAddDoorEnvironment.Name = "_bAddDoorEnvironment";
            this._bAddDoorEnvironment.Size = new System.Drawing.Size(96, 32);
            this._bAddDoorEnvironment.TabIndex = 1;
            this._bAddDoorEnvironment.Text = "Add";
            this._bAddDoorEnvironment.UseVisualStyleBackColor = true;
            this._bAddDoorEnvironment.Click += new System.EventHandler(this._bAddDoorEnvironment_Click);
            //
            // _dgDoorEnvironments
            //
            this._dgDoorEnvironments.AllwaysRefreshOrder = false;
            this._dgDoorEnvironments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._dgDoorEnvironments.CopyOnRightClick = true;
            this._dgDoorEnvironments.DataGrid.AllowUserToAddRows = false;
            this._dgDoorEnvironments.DataGrid.AllowUserToDeleteRows = false;
            this._dgDoorEnvironments.DataGrid.AllowUserToResizeRows = false;
            this._dgDoorEnvironments.DataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._dgDoorEnvironments.DataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dgDoorEnvironments.DataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._tcSymbolColumn,
            this._tcDoorEnvironmentColumn,
            this._tcAccessTypeColumn});
            this._dgDoorEnvironments.DataGrid.MultiSelect = false;
            this._dgDoorEnvironments.DataGrid.Name = "_dgvData";
            this._dgDoorEnvironments.DataGrid.ReadOnly = true;
            this._dgDoorEnvironments.DataGrid.RowHeadersVisible = false;
            this._dgDoorEnvironments.DataGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this._dgDoorEnvironments.DataGrid.RowTemplate.Height = 24;
            this._dgDoorEnvironments.DataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._dgDoorEnvironments.DataGrid.TabIndex = 4;
            this._dgDoorEnvironments.DataGrid.TabStop = false;
            this._dgDoorEnvironments.DataGrid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this._dgDoorEnvironments_CellDoubleClick);
            this._dgDoorEnvironments.LocalizationHelper = null;
            this._dgDoorEnvironments.Location = new System.Drawing.Point(6, 6);
            this._dgDoorEnvironments.Name = "_dgDoorEnvironments";
            this._dgDoorEnvironments.Size = new System.Drawing.Size(576, 287);
            this._dgDoorEnvironments.TabIndex = 0;
            this._dgDoorEnvironments.TabStop = false;
            //
            // _tcSymbolColumn
            //
            this._tcSymbolColumn.DataPropertyName = "Symbol";
            this._tcSymbolColumn.HeaderText = "Symbol";
            this._tcSymbolColumn.Name = "Symbol";
            this._tcSymbolColumn.ReadOnly = true;
            this._tcSymbolColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._tcSymbolColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            //
            // _tcDoorEnvironmentColumn
            //
            this._tcDoorEnvironmentColumn.DataPropertyName = "DoorEnvironmentName";
            this._tcDoorEnvironmentColumn.HeaderText = "DoorEnvironment";
            this._tcDoorEnvironmentColumn.Name = "DoorEnvironment";
            this._tcDoorEnvironmentColumn.ReadOnly = true;
            //
            // _tcAccessTypeColumn
            //
            this._tcAccessTypeColumn.DataPropertyName = "AccessType";
            this._tcAccessTypeColumn.HeaderText = "AccessType";
            this._tcAccessTypeColumn.Name = "AccessType";
            this._tcAccessTypeColumn.ReadOnly = true;
            // 
            // CarEditForm
            // 
            this.AcceptButton = this._bOk;
            this.CancelButton = this._bCancel;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this._tcCar);
            this.Controls.Add(this._bCancel);
            this.Controls.Add(this._bOk);
            this.Name = "CarEditForm";
            this._tcCar.ResumeLayout(false);
            this._tpInformation.ResumeLayout(false);
            this._tpInformation.PerformLayout();
            this._tpCards.ResumeLayout(false);
            this._tpCards.PerformLayout();
            this._tpDoorEnvironment.ResumeLayout(false);
            this.ResumeLayout(false);

        }


    }
}
