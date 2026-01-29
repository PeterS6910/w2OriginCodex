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
        private System.Windows.Forms.TabPage _tpAccessControlList;
        private System.Windows.Forms.TextBox _eFilterCards;
        private Contal.IwQuick.UI.ImageListBox _ilbCards;
        private System.Windows.Forms.Button _bDeleteCard;
        private System.Windows.Forms.Button _bAddCard;
        private System.Windows.Forms.Button _bCreateCard;
        private Contal.Cgp.Components.CgpDataGridView _cdgvAclCars;
        private Contal.IwQuick.UI.TextBoxMenu _tbmAccessControlList;
        private System.Windows.Forms.ToolStripMenuItem _tsiAclModify;
        private Contal.IwQuick.UI.TextBoxDatePicker _tbdpAclDateFrom;
        private Contal.IwQuick.UI.TextBoxDatePicker _tbdpAclDateTo;
        private System.Windows.Forms.Label _lAclAccessControlList;
        private System.Windows.Forms.Label _lAclDateFrom;
        private System.Windows.Forms.Label _lAclDateTo;
        private System.Windows.Forms.Button _bAclCreate;
        private System.Windows.Forms.Button _bAclUpdate;
        private System.Windows.Forms.Button _bAclCancel;
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
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
            this._bAddCard = new System.Windows.Forms.Button();
            this._bDeleteCard = new System.Windows.Forms.Button();
            this._bCreateCard = new System.Windows.Forms.Button();
            this._ilbCards = new Contal.IwQuick.UI.ImageListBox();
            this._eFilterCards = new System.Windows.Forms.TextBox();
            this._tpAccessControlList = new System.Windows.Forms.TabPage();
            this._bAclCancel = new System.Windows.Forms.Button();
            this._bAclUpdate = new System.Windows.Forms.Button();
            this._bAclCreate = new System.Windows.Forms.Button();
            this._lAclDateTo = new System.Windows.Forms.Label();
            this._lAclDateFrom = new System.Windows.Forms.Label();
            this._lAclAccessControlList = new System.Windows.Forms.Label();
            this._tbdpAclDateTo = new Contal.IwQuick.UI.TextBoxDatePicker();
            this._tbdpAclDateFrom = new Contal.IwQuick.UI.TextBoxDatePicker();
            this._tbmAccessControlList = new Contal.IwQuick.UI.TextBoxMenu();
            this._tsiAclModify = new System.Windows.Forms.ToolStripMenuItem();
            this._cdgvAclCars = new Contal.Cgp.Components.CgpDataGridView();
            this._tcCar.SuspendLayout();
            this._tpInformation.SuspendLayout();
            this._tpCards.SuspendLayout();
            this._tpAccessControlList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._cdgvAclCars.DataGrid)).BeginInit();
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
            this._tcCar.Controls.Add(this._tpAccessControlList);
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
            this._tpCards.Controls.Add(this._bAddCard);
            this._tpCards.Controls.Add(this._bCreateCard);
            this._tpCards.Controls.Add(this._bDeleteCard);
            this._tpCards.Controls.Add(this._ilbCards);
            this._tpCards.Controls.Add(this._eFilterCards);
            resources.ApplyResources(this._tpCards, "_tpCards");
            this._tpCards.Name = "_tpCards";
            this._tpCards.Enter += new System.EventHandler(this._tpCards_Enter);
            // 
            // _bAddCard
            // 
            this._bAddCard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            resources.ApplyResources(this._bAddCard, "_bAddCard");
            this._bAddCard.Name = "_bAddCard";
            this._bAddCard.UseVisualStyleBackColor = true;
            this._bAddCard.Click += new System.EventHandler(this._bAddCard_Click);
            // 
            // _bCreateCard
            // 
            this._bCreateCard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            resources.ApplyResources(this._bCreateCard, "_bCreateCard");
            this._bCreateCard.Name = "_bCreateCard";
            this._bCreateCard.UseVisualStyleBackColor = true;
            this._bCreateCard.Click += new System.EventHandler(this._bCreateCard_Click);
            // 
            // _bDeleteCard
            // 
            this._bDeleteCard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            resources.ApplyResources(this._bDeleteCard, "_bDeleteCard");
            this._bDeleteCard.Name = "_bDeleteCard";
            this._bDeleteCard.UseVisualStyleBackColor = true;
            this._bDeleteCard.Click += new System.EventHandler(this._bDeleteCard_Click);
            // 
            // _ilbCards
            // 
            this._ilbCards.AllowDrop = true;
            this._ilbCards.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._ilbCards.BackColor = System.Drawing.SystemColors.Info;
            resources.ApplyResources(this._ilbCards, "_ilbCards");
            this._ilbCards.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this._ilbCards.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this._ilbCards.FormattingEnabled = true;
            this._ilbCards.ImageList = null;
            this._ilbCards.ItemHeight = 18;
            this._ilbCards.Name = "_ilbCards";
            this._ilbCards.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this._ilbCards.TabStop = false;
            this._ilbCards.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this._ilbCards_MouseDoubleClick);
            // 
            // _eFilterCards
            // 
            this._eFilterCards.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            resources.ApplyResources(this._eFilterCards, "_eFilterCards");
            this._eFilterCards.Name = "_eFilterCards";
            this._eFilterCards.KeyUp += new System.Windows.Forms.KeyEventHandler(this._eFilterCards_KeyUp);
            // 
            // _tpAccessControlList
            //
            this._tpAccessControlList.Controls.Add(this._bAclCancel);
            this._tpAccessControlList.Controls.Add(this._bAclUpdate);
            this._tpAccessControlList.Controls.Add(this._bAclCreate);
            this._tpAccessControlList.Controls.Add(this._lAclDateTo);
            this._tpAccessControlList.Controls.Add(this._lAclDateFrom);
            this._tpAccessControlList.Controls.Add(this._lAclAccessControlList);
            this._tpAccessControlList.Controls.Add(this._tbdpAclDateTo);
            this._tpAccessControlList.Controls.Add(this._tbdpAclDateFrom);
            this._tpAccessControlList.Controls.Add(this._tbmAccessControlList);
            this._tpAccessControlList.Controls.Add(this._cdgvAclCars);
            resources.ApplyResources(this._tpAccessControlList, "_tpAccessControlList");
            this._tpAccessControlList.Name = "_tpAccessControlList";
            this._tpAccessControlList.Enter += new System.EventHandler(this._tpAccessControlList_Enter);
            // 
            // _bAclCancel
            // 
            resources.ApplyResources(this._bAclCancel, "_bAclCancel");
            this._bAclCancel.Name = "_bAclCancel";
            this._bAclCancel.UseVisualStyleBackColor = true;
            this._bAclCancel.Click += new System.EventHandler(this._bAclCancel_Click);
            // 
            // _bAclUpdate
            // 
            resources.ApplyResources(this._bAclUpdate, "_bAclUpdate");
            this._bAclUpdate.Name = "_bAclUpdate";
            this._bAclUpdate.UseVisualStyleBackColor = true;
            this._bAclUpdate.Click += new System.EventHandler(this._bAclUpdate_Click);
            // 
            // _bAclCreate
            // 
            resources.ApplyResources(this._bAclCreate, "_bAclCreate");
            this._bAclCreate.Name = "_bAclCreate";
            this._bAclCreate.UseVisualStyleBackColor = true;
            this._bAclCreate.Click += new System.EventHandler(this._bAclCreate_Click);
            // 
            // _lAclDateTo
            // 
            resources.ApplyResources(this._lAclDateTo, "_lAclDateTo");
            this._lAclDateTo.Name = "_lAclDateTo";
            // 
            // _lAclDateFrom
            // 
            resources.ApplyResources(this._lAclDateFrom, "_lAclDateFrom");
            this._lAclDateFrom.Name = "_lAclDateFrom";
            // 
            // _lAclAccessControlList
            // 
            resources.ApplyResources(this._lAclAccessControlList, "_lAclAccessControlList");
            this._lAclAccessControlList.Name = "_lAclAccessControlList";
            // 
            // _tbdpAclDateTo
            // 
            this._tbdpAclDateTo.addActualTime = false;
            resources.ApplyResources(this._tbdpAclDateTo, "_tbdpAclDateTo");
            this._tbdpAclDateTo.BackColor = System.Drawing.Color.Transparent;
            this._tbdpAclDateTo.ButtonClearDateImage = null;
            this._tbdpAclDateTo.ButtonClearDateText = "";
            this._tbdpAclDateTo.ButtonClearDateWidth = 23;
            this._tbdpAclDateTo.ButtonDateImage = null;
            this._tbdpAclDateTo.ButtonDateText = "";
            this._tbdpAclDateTo.ButtonDateWidth = 23;
            this._tbdpAclDateTo.CustomFormat = "d. M. yyyy HH:mm:ss";
            this._tbdpAclDateTo.DateFormName = "Calendar";
            this._tbdpAclDateTo.LocalizationHelper = null;
            this._tbdpAclDateTo.Name = "_tbdpAclDateTo";
            this._tbdpAclDateTo.ReadOnly = false;
            this._tbdpAclDateTo.SelectTime = Contal.IwQuick.UI.SelectedTimeOfDay.EndOfDay;
            this._tbdpAclDateTo.ValidateAfter = 2D;
            this._tbdpAclDateTo.ValidationEnabled = false;
            this._tbdpAclDateTo.ValidationError = "";
            this._tbdpAclDateTo.Value = null;
            // 
            // _tbdpAclDateFrom
            // 
            this._tbdpAclDateFrom.addActualTime = false;
            resources.ApplyResources(this._tbdpAclDateFrom, "_tbdpAclDateFrom");
            this._tbdpAclDateFrom.BackColor = System.Drawing.Color.Transparent;
            this._tbdpAclDateFrom.ButtonClearDateImage = null;
            this._tbdpAclDateFrom.ButtonClearDateText = "";
            this._tbdpAclDateFrom.ButtonClearDateWidth = 23;
            this._tbdpAclDateFrom.ButtonDateImage = null;
            this._tbdpAclDateFrom.ButtonDateText = "";
            this._tbdpAclDateFrom.ButtonDateWidth = 23;
            this._tbdpAclDateFrom.CustomFormat = "d. M. yyyy HH:mm:ss";
            this._tbdpAclDateFrom.DateFormName = "Calendar";
            this._tbdpAclDateFrom.LocalizationHelper = null;
            this._tbdpAclDateFrom.Name = "_tbdpAclDateFrom";
            this._tbdpAclDateFrom.ReadOnly = false;
            this._tbdpAclDateFrom.SelectTime = Contal.IwQuick.UI.SelectedTimeOfDay.StartOfDay;
            this._tbdpAclDateFrom.ValidateAfter = 2D;
            this._tbdpAclDateFrom.ValidationEnabled = false;
            this._tbdpAclDateFrom.ValidationError = "";
            this._tbdpAclDateFrom.Value = null;
            // 
            // _tbmAccessControlList
            // 
            this._tbmAccessControlList.AllowDrop = true;
            this._tbmAccessControlList.BackColor = System.Drawing.SystemColors.Control;
            // 
            // 
            // 
            this._tbmAccessControlList.Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._tbmAccessControlList.Button.BackColor = System.Drawing.SystemColors.Control;
            this._tbmAccessControlList.Button.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this._tbmAccessControlList.Button.Image = ((System.Drawing.Image)(resources.GetObject("_tbmAccessControlList.Button.Image")));
            this._tbmAccessControlList.Button.Location = new System.Drawing.Point(136, 0);
            this._tbmAccessControlList.Button.Name = "_bMenu";
            this._tbmAccessControlList.Button.Size = new System.Drawing.Size(20, 20);
            this._tbmAccessControlList.Button.TabIndex = 3;
            this._tbmAccessControlList.Button.UseVisualStyleBackColor = false;
            this._tbmAccessControlList.ButtonBaseColor = System.Drawing.SystemColors.Control;
            this._tbmAccessControlList.ButtonDefaultBehaviour = true;
            this._tbmAccessControlList.ButtonHoverColor = System.Drawing.SystemColors.GradientActiveCaption;
            this._tbmAccessControlList.ButtonImage = ((System.Drawing.Image)(resources.GetObject("_tbmAccessControlList.ButtonImage")));
            // 
            // 
            // 
            this._tbmAccessControlList.ButtonPopupMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._tsiAclModify});
            this._tbmAccessControlList.ButtonPopupMenu.Name = "";
            this._tbmAccessControlList.ButtonPopupMenu.Size = new System.Drawing.Size(113, 26);
            this._tbmAccessControlList.ButtonPosition = Contal.IwQuick.UI.MenuPosition.Right;
            this._tbmAccessControlList.ButtonShowImage = true;
            this._tbmAccessControlList.ButtonSizeHeight = 20;
            this._tbmAccessControlList.ButtonSizeWidth = 20;
            this._tbmAccessControlList.ButtonText = "";
            this._tbmAccessControlList.Cursor = System.Windows.Forms.Cursors.Hand;
            this._tbmAccessControlList.HoverTime = 600;
            // 
            // 
            // 
            this._tbmAccessControlList.ImageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tbmAccessControlList.ImageTextBox.BackColor = System.Drawing.SystemColors.Info;
            this._tbmAccessControlList.ImageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._tbmAccessControlList.ImageTextBox.ContextMenuStrip = this._tbmAccessControlList.ButtonPopupMenu;
            this._tbmAccessControlList.ImageTextBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this._tbmAccessControlList.ImageTextBox.Image = ((System.Drawing.Image)(resources.GetObject("_tbmAccessControlList.ImageTextBox.Image")));
            this._tbmAccessControlList.ImageTextBox.Location = new System.Drawing.Point(0, 0);
            this._tbmAccessControlList.ImageTextBox.Name = "_textBox";
            this._tbmAccessControlList.ImageTextBox.NoTextNoImage = true;
            this._tbmAccessControlList.ImageTextBox.ReadOnly = true;
            this._tbmAccessControlList.ImageTextBox.Size = new System.Drawing.Size(136, 20);
            this._tbmAccessControlList.ImageTextBox.TabIndex = 0;
            // 
            // 
            // 
            this._tbmAccessControlList.ImageTextBox.TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tbmAccessControlList.ImageTextBox.TextBox.BackColor = System.Drawing.SystemColors.Info;
            this._tbmAccessControlList.ImageTextBox.TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._tbmAccessControlList.ImageTextBox.TextBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this._tbmAccessControlList.ImageTextBox.TextBox.Location = new System.Drawing.Point(1, 2);
            this._tbmAccessControlList.ImageTextBox.TextBox.Name = "_tbTextBox";
            this._tbmAccessControlList.ImageTextBox.TextBox.ReadOnly = true;
            this._tbmAccessControlList.ImageTextBox.TextBox.Size = new System.Drawing.Size(134, 13);
            this._tbmAccessControlList.ImageTextBox.TextBox.TabIndex = 2;
            this._tbmAccessControlList.ImageTextBox.UseImage = true;
            this._tbmAccessControlList.ImageTextBox.DoubleClick += new System.EventHandler(this._tbmAccessControlList_DoubleClick);
            resources.ApplyResources(this._tbmAccessControlList, "_tbmAccessControlList");
            this._tbmAccessControlList.MaximumSize = new System.Drawing.Size(1200, 55);
            this._tbmAccessControlList.MinimumSize = new System.Drawing.Size(30, 22);
            this._tbmAccessControlList.Name = "_tbmAccessControlList";
            this._tbmAccessControlList.TextImage = ((System.Drawing.Image)(resources.GetObject("_tbmAccessControlList.TextImage")));
            this._tbmAccessControlList.ButtonPopupMenuItemClick += new Contal.IwQuick.UI.TextBoxMenu.DPopupMenuHandler(this._tbmAccessControlList_ButtonPopupMenuItemClick);
            // 
            // _tsiAclModify
            // 
            this._tsiAclModify.Name = "_tsiAclModify";
            resources.ApplyResources(this._tsiAclModify, "_tsiAclModify");
            // 
            // _cdgvAclCars
            // 
            this._cdgvAclCars.AllwaysRefreshOrder = false;
            this._cdgvAclCars.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this._cdgvAclCars.CgpDataGridEvents = null;
            this._cdgvAclCars.CopyOnRightClick = true;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.WhiteSmoke;
            this._cdgvAclCars.DataGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this._cdgvAclCars.DataGrid.ColumnHeadersHeight = 34;
            this._cdgvAclCars.DataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cdgvAclCars.DataGrid.Location = new System.Drawing.Point(0, 0);
            this._cdgvAclCars.DataGrid.Name = "_dgvAclCars";
            this._cdgvAclCars.DataGrid.RowHeadersWidth = 62;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            this._cdgvAclCars.DataGrid.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this._cdgvAclCars.DataGrid.TabIndex = 0;
            this._cdgvAclCars.DefaultSortColumnName = null;
            this._cdgvAclCars.DefaultSortDirection = System.ComponentModel.ListSortDirection.Ascending;
            this._cdgvAclCars.LocalizationHelper = null;
            resources.ApplyResources(this._cdgvAclCars, "_cdgvAclCars");
            this._cdgvAclCars.Name = "_cdgvAclCars";
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
            this._tpAccessControlList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._cdgvAclCars.DataGrid)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
