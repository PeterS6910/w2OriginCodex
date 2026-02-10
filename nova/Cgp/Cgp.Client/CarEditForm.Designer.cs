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
        private System.Windows.Forms.Button _bApply;
        private System.Windows.Forms.TabControl _tcCar;
        private System.Windows.Forms.TabPage _tpInformation;
        private System.Windows.Forms.TabPage _tpCards;
        private System.Windows.Forms.TabPage _tpAccessControlList;
        private System.Windows.Forms.TabPage _tpAccessZone;
        private System.Windows.Forms.Panel _pAccessZone;
        private Contal.Cgp.Components.CgpDataGridView _cdgvAccessZone;
        private Contal.IwQuick.UI.TextBoxMenu _tbmAccessZoneCardReader;
        private Contal.IwQuick.UI.TextBoxMenu _tbmAccessZoneTimeZone;
        private System.Windows.Forms.Button _bAccessZoneUpdate;
        private System.Windows.Forms.Label _lAccessZoneDescription;
        private System.Windows.Forms.Button _bAccessZoneCancel;
        private System.Windows.Forms.Button _bAccessZoneCreate;
        private System.Windows.Forms.TextBox _eAccessZoneDescription;
        private System.Windows.Forms.Label _lAccessZoneCardReader;
        private System.Windows.Forms.Label _lAccessZoneTimeZone;
        private System.Windows.Forms.TextBox _eFilterCards;
        private Contal.IwQuick.UI.ImageListBox _ilbCards;
        private System.Windows.Forms.Button _bDeleteCard;
        private System.Windows.Forms.Button _bAddCard;
        private System.Windows.Forms.Button _bCreateCard;
        private Contal.Cgp.Components.CgpDataGridView _cdgvAclCars;
        private Contal.IwQuick.UI.TextBoxMenu _tbmAccessControlList;
        private System.Windows.Forms.ToolStripMenuItem _tsiAclModify;
        private System.Windows.Forms.ToolStripMenuItem _tsiAclCreate;
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
        private System.Windows.Forms.TextBox _eSecurityLevel;
        private Contal.IwQuick.UI.TextBoxMenu _tbmDepartment;
        private System.Windows.Forms.ToolStripMenuItem _tsiDepartmentModify;
        private System.Windows.Forms.ToolStripMenuItem _tsiDepartmentRemove;
        private System.Windows.Forms.Label _lSecurityLevel;
        private System.Windows.Forms.Label _lDepartment;
        private System.Windows.Forms.Label _lDepartmentFilter;
        private Contal.IwQuick.UI.TextBoxMenu _tbDepartmentFilter;
        private System.Windows.Forms.Button _bDepartmentFilter;
        private System.Windows.Forms.Button _bDepartmentFilterClear;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CarEditForm));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this._eLp = new System.Windows.Forms.TextBox();
            this._eBrand = new System.Windows.Forms.TextBox();
            this._eDescription = new System.Windows.Forms.TextBox();
            this._eUtcDateStateLastChange = new System.Windows.Forms.TextBox();
            this._bOk = new System.Windows.Forms.Button();
            this._bCancel = new System.Windows.Forms.Button();
            this._bApply = new System.Windows.Forms.Button();
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
            this._eSecurityLevel = new System.Windows.Forms.TextBox();
            this._tbmDepartment = new Contal.IwQuick.UI.TextBoxMenu();
            this._tsiDepartmentModify = new System.Windows.Forms.ToolStripMenuItem();
            this._tsiDepartmentRemove = new System.Windows.Forms.ToolStripMenuItem();
            this._lSecurityLevel = new System.Windows.Forms.Label();
            this._lDepartment = new System.Windows.Forms.Label();
            this._tpCards = new System.Windows.Forms.TabPage();
            this._bAddCard = new System.Windows.Forms.Button();
            this._bCreateCard = new System.Windows.Forms.Button();
            this._bDeleteCard = new System.Windows.Forms.Button();
            this._ilbCards = new Contal.IwQuick.UI.ImageListBox();
            this._eFilterCards = new System.Windows.Forms.TextBox();
            this._lDepartmentFilter = new System.Windows.Forms.Label();
            this._tbDepartmentFilter = new Contal.IwQuick.UI.TextBoxMenu();
            this._bDepartmentFilter = new System.Windows.Forms.Button();
            this._bDepartmentFilterClear = new System.Windows.Forms.Button();
            this._tpAccessControlList = new System.Windows.Forms.TabPage();
            this._tpAccessZone = new System.Windows.Forms.TabPage();
            this._pAccessZone = new System.Windows.Forms.Panel();
            this._cdgvAccessZone = new Contal.Cgp.Components.CgpDataGridView();
            this._tbmAccessZoneCardReader = new Contal.IwQuick.UI.TextBoxMenu();
            this._tbmAccessZoneTimeZone = new Contal.IwQuick.UI.TextBoxMenu();
            this._bAccessZoneUpdate = new System.Windows.Forms.Button();
            this._lAccessZoneDescription = new System.Windows.Forms.Label();
            this._bAccessZoneCancel = new System.Windows.Forms.Button();
            this._bAccessZoneCreate = new System.Windows.Forms.Button();
            this._eAccessZoneDescription = new System.Windows.Forms.TextBox();
            this._lAccessZoneCardReader = new System.Windows.Forms.Label();
            this._lAccessZoneTimeZone = new System.Windows.Forms.Label();
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
            this._tsiAclCreate = new System.Windows.Forms.ToolStripMenuItem();
            this._cdgvAclCars = new Contal.Cgp.Components.CgpDataGridView();
            this._tcCar.SuspendLayout();
            this._tpInformation.SuspendLayout();
            this._tpCards.SuspendLayout();
            this._tpAccessControlList.SuspendLayout();
            this._tpAccessZone.SuspendLayout();
            this._pAccessZone.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._cdgvAccessZone.DataGrid)).BeginInit();
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
            this._eUtcDateStateLastChange.Visible = false;
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
            this._bCancel.Location = new System.Drawing.Point(280, 386);
            this._bCancel.Name = "_bCancel";
            this._bCancel.Click += new System.EventHandler(this._bCancel_Click);
            // 
            // _tcCar
            // 
            resources.ApplyResources(this._tcCar, "_tcCar");
            this._tcCar.Controls.Add(this._tpInformation);
            this._tcCar.Controls.Add(this._tpCards);
            this._tcCar.Controls.Add(this._tpAccessControlList);
            this._tcCar.Controls.Add(this._tpAccessZone);
            this._tcCar.Name = "_tcCar";
            this._tcCar.SelectedIndex = 0;
            this._tcCar.TabStop = false;
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
            this._tpInformation.Controls.Add(this._lSecurityLevel);
            this._tpInformation.Controls.Add(this._lDepartment);
            this._tpInformation.Controls.Add(this._eSecurityLevel);
            this._tpInformation.Controls.Add(this._tbmDepartment);
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
            this.label6.Visible = false;
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
            // _bApply
            // 
            this._bApply.Location = new System.Drawing.Point(362, 386);
            this._bApply.Name = "_bApply";
            this._bApply.Size = new System.Drawing.Size(75, 32);
            this._bApply.TabIndex = 2;
            this._bApply.Text = "Apply";
            this._bApply.UseVisualStyleBackColor = true;
            this._bApply.Click += new System.EventHandler(this._bApply_Click);
            // 
            // _eSecurityLevel
            // 
            this._eSecurityLevel.Location = new System.Drawing.Point(259, 194);
            this._eSecurityLevel.Name = "_eSecurityLevel";
            this._eSecurityLevel.Size = new System.Drawing.Size(260, 22);
            this._eSecurityLevel.TabIndex = 10;
            // 
            // _tbmDepartment
            // 
            this._tbmDepartment.BackColor = System.Drawing.Color.White;
            // 
            // 
            // 
            this._tbmDepartment.ButtonPopupMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._tsiDepartmentModify,
            this._tsiDepartmentRemove});
            this._tbmDepartment.ButtonPopupMenu.Name = "";
            this._tbmDepartment.ButtonPopupMenu.Size = new System.Drawing.Size(118, 48);
            this._tbmDepartment.ButtonPosition = Contal.IwQuick.UI.MenuPosition.Right;
            this._tbmDepartment.ButtonDefaultBehaviour = true;
            this._tbmDepartment.ButtonShowImage = true;
            this._tbmDepartment.ButtonSizeHeight = 20;
            this._tbmDepartment.ButtonSizeWidth = 20;
            this._tbmDepartment.Location = new System.Drawing.Point(259, 243);
            this._tbmDepartment.Name = "_tbmDepartment";
            this._tbmDepartment.Size = new System.Drawing.Size(260, 20);
            this._tbmDepartment.TabIndex = 11;
            this._tbmDepartment.ImageTextBox.ContextMenuStrip = this._tbmDepartment.ButtonPopupMenu;
            this._tbmDepartment.ButtonPopupMenuItemClick += new Contal.IwQuick.UI.TextBoxMenu.DPopupMenuHandler(this.DepartmentButtonPopupMenuItemClick);
            this._tbmDepartment.ImageTextBox.DoubleClick += new System.EventHandler(this.DepartmentTextBoxDoubleClick);
            // 
            // _tsiDepartmentModify
            // 
            this._tsiDepartmentModify.Name = "_tsiDepartmentModify";
            this._tsiDepartmentModify.Size = new System.Drawing.Size(117, 22);
            this._tsiDepartmentModify.Text = "Modify";
            // 
            // _tsiDepartmentRemove
            // 
            this._tsiDepartmentRemove.Name = "_tsiDepartmentRemove";
            this._tsiDepartmentRemove.Size = new System.Drawing.Size(117, 22);
            this._tsiDepartmentRemove.Text = "Remove";
            // 
            // _lSecurityLevel
            // 
            this._lSecurityLevel.AutoSize = true;
            this._lSecurityLevel.Location = new System.Drawing.Point(43, 200);
            this._lSecurityLevel.Name = "_lSecurityLevel";
            this._lSecurityLevel.Size = new System.Drawing.Size(83, 16);
            this._lSecurityLevel.Text = "SecurityLevel";
            // 
            // _lDepartment
            // 
            this._lDepartment.AutoSize = true;
            this._lDepartment.Location = new System.Drawing.Point(43, 248);
            this._lDepartment.Name = "_lDepartment";
            this._lDepartment.Size = new System.Drawing.Size(79, 16);
            this._lDepartment.Text = "Department";
            // 
            // _tpCards
            // 
            this._tpCards.Controls.Add(this._bAddCard);
            this._tpCards.Controls.Add(this._bCreateCard);
            this._tpCards.Controls.Add(this._bDeleteCard);
            this._tpCards.Controls.Add(this._ilbCards);
            this._tpCards.Controls.Add(this._bDepartmentFilterClear);
            this._tpCards.Controls.Add(this._bDepartmentFilter);
            this._tpCards.Controls.Add(this._tbDepartmentFilter);
            this._tpCards.Controls.Add(this._lDepartmentFilter);
            this._tpCards.Controls.Add(this._eFilterCards);
            resources.ApplyResources(this._tpCards, "_tpCards");
            this._tpCards.Name = "_tpCards";
            this._tpCards.Enter += new System.EventHandler(this._tpCards_Enter);
            // 
            // _bAddCard
            // 
            resources.ApplyResources(this._bAddCard, "_bAddCard");
            this._bAddCard.Name = "_bAddCard";
            this._bAddCard.UseVisualStyleBackColor = true;
            this._bAddCard.Click += new System.EventHandler(this._bAddCard_Click);
            // 
            // _bCreateCard
            // 
            resources.ApplyResources(this._bCreateCard, "_bCreateCard");
            this._bCreateCard.Name = "_bCreateCard";
            this._bCreateCard.UseVisualStyleBackColor = true;
            this._bCreateCard.Click += new System.EventHandler(this._bCreateCard_Click);
            // 
            // _bDeleteCard
            // 
            resources.ApplyResources(this._bDeleteCard, "_bDeleteCard");
            this._bDeleteCard.Name = "_bDeleteCard";
            this._bDeleteCard.UseVisualStyleBackColor = true;
            this._bDeleteCard.Click += new System.EventHandler(this._bDeleteCard_Click);
            // 
            // _ilbCards
            // 
            this._ilbCards.AllowDrop = true;
            resources.ApplyResources(this._ilbCards, "_ilbCards");
            this._ilbCards.BackColor = System.Drawing.SystemColors.Info;
            this._ilbCards.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this._ilbCards.FormattingEnabled = true;
            this._ilbCards.ImageList = null;
            this._ilbCards.Name = "_ilbCards";
            this._ilbCards.SelectedItemObject = null;
            this._ilbCards.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this._ilbCards.TabStop = false;
            this._ilbCards.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this._ilbCards_MouseDoubleClick);
            // 
            // _eFilterCards
            // 
            resources.ApplyResources(this._eFilterCards, "_eFilterCards");
            this._eFilterCards.Name = "_eFilterCards";
            this._eFilterCards.KeyUp += new System.Windows.Forms.KeyEventHandler(this._eFilterCards_KeyUp);
            // 
            // _lDepartmentFilter
            // 
            this._lDepartmentFilter.AutoSize = true;
            this._lDepartmentFilter.Location = new System.Drawing.Point(6, 9);
            this._lDepartmentFilter.Name = "_lDepartmentFilter";
            this._lDepartmentFilter.Size = new System.Drawing.Size(79, 16);
            this._lDepartmentFilter.Text = "Department";
            // 
            // _tbDepartmentFilter
            // 
            this._tbDepartmentFilter.Location = new System.Drawing.Point(91, 6);
            this._tbDepartmentFilter.Name = "_tbDepartmentFilter";
            this._tbDepartmentFilter.Size = new System.Drawing.Size(160, 20);
            this._tbDepartmentFilter.Button.Click += new System.EventHandler(this._bDepartmentFilter_Click);
            // 
            // _bDepartmentFilter
            // 
            this._bDepartmentFilter.Location = new System.Drawing.Point(257, 4);
            this._bDepartmentFilter.Name = "_bDepartmentFilter";
            this._bDepartmentFilter.Size = new System.Drawing.Size(55, 23);
            this._bDepartmentFilter.Text = "...";
            this._bDepartmentFilter.UseVisualStyleBackColor = true;
            this._bDepartmentFilter.Click += new System.EventHandler(this._bDepartmentFilter_Click);
            // 
            // _bDepartmentFilterClear
            // 
            this._bDepartmentFilterClear.Location = new System.Drawing.Point(318, 4);
            this._bDepartmentFilterClear.Name = "_bDepartmentFilterClear";
            this._bDepartmentFilterClear.Size = new System.Drawing.Size(55, 23);
            this._bDepartmentFilterClear.Text = "X";
            this._bDepartmentFilterClear.UseVisualStyleBackColor = true;
            this._bDepartmentFilterClear.Click += new System.EventHandler(this._bDepartmentFilterClear_Click);
            // 
            // _tpAccessControlList
            // 
            resources.ApplyResources(this._tpAccessControlList, "_tpAccessControlList");
            this._tpAccessControlList.Name = "_tpAccessControlList";
            // 
            // _tpAccessZone
            // 
            resources.ApplyResources(this._tpAccessZone, "_tpAccessZone");
            this._tpAccessZone.Controls.Add(this._pAccessZone);
            this._tpAccessZone.Name = "_tpAccessZone";
            // 
            // _pAccessZone
            // 
            this._pAccessZone.Controls.Add(this._cdgvAccessZone);
            this._pAccessZone.Controls.Add(this._tbmAccessZoneCardReader);
            this._pAccessZone.Controls.Add(this._tbmAccessZoneTimeZone);
            this._pAccessZone.Controls.Add(this._bAccessZoneUpdate);
            this._pAccessZone.Controls.Add(this._lAccessZoneDescription);
            this._pAccessZone.Controls.Add(this._bAccessZoneCancel);
            this._pAccessZone.Controls.Add(this._bAccessZoneCreate);
            this._pAccessZone.Controls.Add(this._eAccessZoneDescription);
            this._pAccessZone.Controls.Add(this._lAccessZoneCardReader);
            this._pAccessZone.Controls.Add(this._lAccessZoneTimeZone);
            this._pAccessZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pAccessZone.Location = new System.Drawing.Point(3, 3);
            this._pAccessZone.Name = "_pAccessZone";
            this._pAccessZone.Size = new System.Drawing.Size(582, 334);
            this._pAccessZone.TabIndex = 0;
            // 
            // _cdgvAccessZone
            // 
            this._cdgvAccessZone.AllwaysRefreshOrder = false;
            this._cdgvAccessZone.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._cdgvAccessZone.CgpDataGridEvents = null;
            this._cdgvAccessZone.CopyOnRightClick = true;
            // 
            // 
            // 
            this._cdgvAccessZone.DataGrid.AllowUserToAddRows = false;
            this._cdgvAccessZone.DataGrid.AllowUserToDeleteRows = false;
            this._cdgvAccessZone.DataGrid.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.WhiteSmoke;
            this._cdgvAccessZone.DataGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this._cdgvAccessZone.DataGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this._cdgvAccessZone.DataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this._cdgvAccessZone.DataGrid.ColumnHeadersHeight = 34;
            this._cdgvAccessZone.DataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cdgvAccessZone.DataGrid.Location = new System.Drawing.Point(0, 0);
            this._cdgvAccessZone.DataGrid.Name = "_dgvAccessZone";
            this._cdgvAccessZone.DataGrid.ReadOnly = true;
            this._cdgvAccessZone.DataGrid.RowHeadersVisible = false;
            this._cdgvAccessZone.DataGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            this._cdgvAccessZone.DataGrid.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this._cdgvAccessZone.DataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._cdgvAccessZone.DataGrid.Size = new System.Drawing.Size(570, 220);
            this._cdgvAccessZone.DataGrid.TabIndex = 0;
            this._cdgvAccessZone.LocalizationHelper = null;
            this._cdgvAccessZone.Location = new System.Drawing.Point(6, 108);
            this._cdgvAccessZone.Name = "_cdgvAccessZone";
            this._cdgvAccessZone.Size = new System.Drawing.Size(570, 220);
            this._cdgvAccessZone.TabIndex = 10;
            // 
            // _tbmAccessZoneCardReader
            // 
            this._tbmAccessZoneCardReader.AllowDrop = true;
            this._tbmAccessZoneCardReader.BackColor = System.Drawing.SystemColors.Control;
            // 
            // 
            // 
            this._tbmAccessZoneCardReader.Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._tbmAccessZoneCardReader.Button.BackColor = System.Drawing.SystemColors.Control;
            this._tbmAccessZoneCardReader.Button.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this._tbmAccessZoneCardReader.Button.Location = new System.Drawing.Point(190, 0);
            this._tbmAccessZoneCardReader.Button.Name = "_bMenu";
            this._tbmAccessZoneCardReader.Button.Size = new System.Drawing.Size(20, 20);
            this._tbmAccessZoneCardReader.Button.TabIndex = 3;
            this._tbmAccessZoneCardReader.Button.UseVisualStyleBackColor = false;
            this._tbmAccessZoneCardReader.ButtonBaseColor = System.Drawing.SystemColors.Control;
            this._tbmAccessZoneCardReader.ButtonDefaultBehaviour = true;
            this._tbmAccessZoneCardReader.ButtonHoverColor = System.Drawing.SystemColors.GradientActiveCaption;
            this._tbmAccessZoneCardReader.ButtonImage = null;
            this._tbmAccessZoneCardReader.ButtonPopupMenu.Name = "";
            this._tbmAccessZoneCardReader.ButtonPosition = Contal.IwQuick.UI.MenuPosition.Right;
            this._tbmAccessZoneCardReader.ButtonShowImage = true;
            this._tbmAccessZoneCardReader.ButtonSizeHeight = 20;
            this._tbmAccessZoneCardReader.ButtonSizeWidth = 20;
            this._tbmAccessZoneCardReader.ButtonText = "";
            this._tbmAccessZoneCardReader.Cursor = System.Windows.Forms.Cursors.Hand;
            this._tbmAccessZoneCardReader.HoverTime = 600;
            // 
            // 
            // 
            this._tbmAccessZoneCardReader.ImageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._tbmAccessZoneCardReader.ImageTextBox.BackColor = System.Drawing.SystemColors.Info;
            this._tbmAccessZoneCardReader.ImageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._tbmAccessZoneCardReader.ImageTextBox.ContextMenuStrip = this._tbmAccessZoneCardReader.ButtonPopupMenu;
            this._tbmAccessZoneCardReader.ImageTextBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this._tbmAccessZoneCardReader.ImageTextBox.Image = null;
            this._tbmAccessZoneCardReader.ImageTextBox.Location = new System.Drawing.Point(0, 0);
            this._tbmAccessZoneCardReader.ImageTextBox.Name = "_textBox";
            this._tbmAccessZoneCardReader.ImageTextBox.NoTextNoImage = true;
            this._tbmAccessZoneCardReader.ImageTextBox.ReadOnly = true;
            this._tbmAccessZoneCardReader.ImageTextBox.Size = new System.Drawing.Size(190, 20);
            this._tbmAccessZoneCardReader.ImageTextBox.TabIndex = 0;
            // 
            // 
            // 
            this._tbmAccessZoneCardReader.ImageTextBox.TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._tbmAccessZoneCardReader.ImageTextBox.TextBox.BackColor = System.Drawing.SystemColors.Info;
            this._tbmAccessZoneCardReader.ImageTextBox.TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._tbmAccessZoneCardReader.ImageTextBox.TextBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this._tbmAccessZoneCardReader.ImageTextBox.TextBox.Location = new System.Drawing.Point(1, 2);
            this._tbmAccessZoneCardReader.ImageTextBox.TextBox.Name = "_tbTextBox";
            this._tbmAccessZoneCardReader.ImageTextBox.TextBox.ReadOnly = true;
            this._tbmAccessZoneCardReader.ImageTextBox.TextBox.Size = new System.Drawing.Size(188, 13);
            this._tbmAccessZoneCardReader.ImageTextBox.TextBox.TabIndex = 2;
            this._tbmAccessZoneCardReader.ImageTextBox.UseImage = true;
            this._tbmAccessZoneCardReader.Location = new System.Drawing.Point(6, 22);
            this._tbmAccessZoneCardReader.MaximumSize = new System.Drawing.Size(1200, 55);
            this._tbmAccessZoneCardReader.MinimumSize = new System.Drawing.Size(30, 22);
            this._tbmAccessZoneCardReader.Name = "_tbmAccessZoneCardReader";
            this._tbmAccessZoneCardReader.Size = new System.Drawing.Size(210, 22);
            this._tbmAccessZoneCardReader.TabIndex = 1;
            this._tbmAccessZoneCardReader.TextImage = null;
            // 
            // _tbmAccessZoneTimeZone
            // 
            this._tbmAccessZoneTimeZone.AllowDrop = true;
            this._tbmAccessZoneTimeZone.BackColor = System.Drawing.SystemColors.Control;
            // 
            // 
            // 
            this._tbmAccessZoneTimeZone.Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._tbmAccessZoneTimeZone.Button.BackColor = System.Drawing.SystemColors.Control;
            this._tbmAccessZoneTimeZone.Button.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this._tbmAccessZoneTimeZone.Button.Location = new System.Drawing.Point(190, 0);
            this._tbmAccessZoneTimeZone.Button.Name = "_bMenu";
            this._tbmAccessZoneTimeZone.Button.Size = new System.Drawing.Size(20, 20);
            this._tbmAccessZoneTimeZone.Button.TabIndex = 3;
            this._tbmAccessZoneTimeZone.Button.UseVisualStyleBackColor = false;
            this._tbmAccessZoneTimeZone.ButtonBaseColor = System.Drawing.SystemColors.Control;
            this._tbmAccessZoneTimeZone.ButtonDefaultBehaviour = true;
            this._tbmAccessZoneTimeZone.ButtonHoverColor = System.Drawing.SystemColors.GradientActiveCaption;
            this._tbmAccessZoneTimeZone.ButtonImage = null;
            this._tbmAccessZoneTimeZone.ButtonPopupMenu.Name = "";
            this._tbmAccessZoneTimeZone.ButtonPosition = Contal.IwQuick.UI.MenuPosition.Right;
            this._tbmAccessZoneTimeZone.ButtonShowImage = true;
            this._tbmAccessZoneTimeZone.ButtonSizeHeight = 20;
            this._tbmAccessZoneTimeZone.ButtonSizeWidth = 20;
            this._tbmAccessZoneTimeZone.ButtonText = "";
            this._tbmAccessZoneTimeZone.Cursor = System.Windows.Forms.Cursors.Hand;
            this._tbmAccessZoneTimeZone.HoverTime = 600;
            // 
            // 
            // 
            this._tbmAccessZoneTimeZone.ImageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._tbmAccessZoneTimeZone.ImageTextBox.BackColor = System.Drawing.SystemColors.Info;
            this._tbmAccessZoneTimeZone.ImageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._tbmAccessZoneTimeZone.ImageTextBox.ContextMenuStrip = this._tbmAccessZoneTimeZone.ButtonPopupMenu;
            this._tbmAccessZoneTimeZone.ImageTextBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this._tbmAccessZoneTimeZone.ImageTextBox.Image = null;
            this._tbmAccessZoneTimeZone.ImageTextBox.Location = new System.Drawing.Point(0, 0);
            this._tbmAccessZoneTimeZone.ImageTextBox.Name = "_textBox";
            this._tbmAccessZoneTimeZone.ImageTextBox.NoTextNoImage = true;
            this._tbmAccessZoneTimeZone.ImageTextBox.ReadOnly = true;
            this._tbmAccessZoneTimeZone.ImageTextBox.Size = new System.Drawing.Size(190, 20);
            this._tbmAccessZoneTimeZone.ImageTextBox.TabIndex = 0;
            // 
            // 
            // 
            this._tbmAccessZoneTimeZone.ImageTextBox.TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._tbmAccessZoneTimeZone.ImageTextBox.TextBox.BackColor = System.Drawing.SystemColors.Info;
            this._tbmAccessZoneTimeZone.ImageTextBox.TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._tbmAccessZoneTimeZone.ImageTextBox.TextBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this._tbmAccessZoneTimeZone.ImageTextBox.TextBox.Location = new System.Drawing.Point(1, 2);
            this._tbmAccessZoneTimeZone.ImageTextBox.TextBox.Name = "_tbTextBox";
            this._tbmAccessZoneTimeZone.ImageTextBox.TextBox.ReadOnly = true;
            this._tbmAccessZoneTimeZone.ImageTextBox.TextBox.Size = new System.Drawing.Size(188, 13);
            this._tbmAccessZoneTimeZone.ImageTextBox.TextBox.TabIndex = 2;
            this._tbmAccessZoneTimeZone.ImageTextBox.UseImage = true;
            this._tbmAccessZoneTimeZone.Location = new System.Drawing.Point(222, 22);
            this._tbmAccessZoneTimeZone.MaximumSize = new System.Drawing.Size(1200, 55);
            this._tbmAccessZoneTimeZone.MinimumSize = new System.Drawing.Size(30, 22);
            this._tbmAccessZoneTimeZone.Name = "_tbmAccessZoneTimeZone";
            this._tbmAccessZoneTimeZone.Size = new System.Drawing.Size(210, 22);
            this._tbmAccessZoneTimeZone.TabIndex = 2;
            this._tbmAccessZoneTimeZone.TextImage = null;
            // 
            // _bAccessZoneUpdate
            // 
            this._bAccessZoneUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._bAccessZoneUpdate.Location = new System.Drawing.Point(414, 63);
            this._bAccessZoneUpdate.Name = "_bAccessZoneUpdate";
            this._bAccessZoneUpdate.Size = new System.Drawing.Size(75, 23);
            this._bAccessZoneUpdate.TabIndex = 6;
            this._bAccessZoneUpdate.Text = "Update";
            this._bAccessZoneUpdate.UseVisualStyleBackColor = true;
            // 
            // _lAccessZoneDescription
            // 
            this._lAccessZoneDescription.AutoSize = true;
            this._lAccessZoneDescription.Location = new System.Drawing.Point(6, 48);
            this._lAccessZoneDescription.Name = "_lAccessZoneDescription";
            this._lAccessZoneDescription.Size = new System.Drawing.Size(60, 13);
            this._lAccessZoneDescription.TabIndex = 3;
            this._lAccessZoneDescription.Text = "Description";
            // 
            // _bAccessZoneCancel
            // 
            this._bAccessZoneCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._bAccessZoneCancel.Location = new System.Drawing.Point(495, 63);
            this._bAccessZoneCancel.Name = "_bAccessZoneCancel";
            this._bAccessZoneCancel.Size = new System.Drawing.Size(75, 23);
            this._bAccessZoneCancel.TabIndex = 7;
            this._bAccessZoneCancel.Text = "Cancel";
            this._bAccessZoneCancel.UseVisualStyleBackColor = true;
            // 
            // _bAccessZoneCreate
            // 
            this._bAccessZoneCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._bAccessZoneCreate.Location = new System.Drawing.Point(333, 63);
            this._bAccessZoneCreate.Name = "_bAccessZoneCreate";
            this._bAccessZoneCreate.Size = new System.Drawing.Size(75, 23);
            this._bAccessZoneCreate.TabIndex = 5;
            this._bAccessZoneCreate.Text = "Create";
            this._bAccessZoneCreate.UseVisualStyleBackColor = true;
            // 
            // _eAccessZoneDescription
            // 
            this._eAccessZoneDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._eAccessZoneDescription.Location = new System.Drawing.Point(6, 64);
            this._eAccessZoneDescription.Multiline = true;
            this._eAccessZoneDescription.Name = "_eAccessZoneDescription";
            this._eAccessZoneDescription.Size = new System.Drawing.Size(321, 36);
            this._eAccessZoneDescription.TabIndex = 4;
            // 
            // _lAccessZoneCardReader
            // 
            this._lAccessZoneCardReader.AutoSize = true;
            this._lAccessZoneCardReader.Location = new System.Drawing.Point(6, 6);
            this._lAccessZoneCardReader.Name = "_lAccessZoneCardReader";
            this._lAccessZoneCardReader.Size = new System.Drawing.Size(98, 13);
            this._lAccessZoneCardReader.TabIndex = 8;
            this._lAccessZoneCardReader.Text = "Card reader objects";
            // 
            // _lAccessZoneTimeZone
            // 
            this._lAccessZoneTimeZone.AutoSize = true;
            this._lAccessZoneTimeZone.Location = new System.Drawing.Point(222, 6);
            this._lAccessZoneTimeZone.Name = "_lAccessZoneTimeZone";
            this._lAccessZoneTimeZone.Size = new System.Drawing.Size(57, 13);
            this._lAccessZoneTimeZone.TabIndex = 9;
            this._lAccessZoneTimeZone.Text = "Time zone";
            // 
            // CarEditForm
            // 
            this.AcceptButton = this._bOk;
            this.CancelButton = this._bCancel;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this._tcCar);
            this.Controls.Add(this._bCancel);
            this.Controls.Add(this._bApply);
            this.Controls.Add(this._bOk);
            this.Name = "CarEditForm";
            this._tcCar.ResumeLayout(false);
            this._tpInformation.ResumeLayout(false);
            this._tpInformation.PerformLayout();
            this._tpCards.ResumeLayout(false);
            this._tpCards.PerformLayout();
            this._tpAccessControlList.ResumeLayout(false);
            this._tpAccessZone.ResumeLayout(false);
            this._pAccessZone.ResumeLayout(false);
            this._pAccessZone.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._cdgvAccessZone.DataGrid)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
