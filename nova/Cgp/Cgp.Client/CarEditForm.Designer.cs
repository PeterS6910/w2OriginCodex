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
        private Contal.Cgp.Components.CgpDataGridView _cdgvAclCars;
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
            this._ilbCards = new Contal.IwQuick.UI.ImageListBox();
            this._eFilterCards = new System.Windows.Forms.TextBox();
            this._tpAccessControlList = new System.Windows.Forms.TabPage();
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
            this._tpAccessControlList.Controls.Add(this._cdgvAclCars);
            resources.ApplyResources(this._tpAccessControlList, "_tpAccessControlList");
            this._tpAccessControlList.Name = "_tpAccessControlList";
            this._tpAccessControlList.Enter += new System.EventHandler(this._tpAccessControlList_Enter);
            // 
            // _cdgvAclCars
            // 
            this._cdgvAclCars.AllwaysRefreshOrder = false;
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
            this._cdgvAclCars.Dock = System.Windows.Forms.DockStyle.Fill;
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
