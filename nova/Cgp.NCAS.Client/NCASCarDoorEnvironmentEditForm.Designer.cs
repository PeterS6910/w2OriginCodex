namespace Contal.Cgp.NCAS.Client
{
    partial class NCASCarDoorEnvironmentEditForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NCASCarDoorEnvironmentEditForm));
            this._mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this._fieldsLayout = new System.Windows.Forms.TableLayoutPanel();
            this._lDoorEnvironment = new System.Windows.Forms.Label();
            this._tbDoorEnvironment = new System.Windows.Forms.TextBox();
            this._lCar = new System.Windows.Forms.Label();
            this._tbCar = new System.Windows.Forms.TextBox();
            this._lAccessType = new System.Windows.Forms.Label();
            this._cbAccessType = new System.Windows.Forms.ComboBox();
            this._buttonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._bCancel = new System.Windows.Forms.Button();
            this._bOk = new System.Windows.Forms.Button();
            this._mainLayout.SuspendLayout();
            this._fieldsLayout.SuspendLayout();
            this._buttonsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _mainLayout
            // 
            resources.ApplyResources(this._mainLayout, "_mainLayout");
            this._mainLayout.Controls.Add(this._fieldsLayout, 0, 0);
            this._mainLayout.Controls.Add(this._buttonsPanel, 0, 1);
            this._mainLayout.Name = "_mainLayout";
            // 
            // _fieldsLayout
            // 
            resources.ApplyResources(this._fieldsLayout, "_fieldsLayout");
            this._fieldsLayout.Controls.Add(this._lDoorEnvironment, 0, 0);
            this._fieldsLayout.Controls.Add(this._tbDoorEnvironment, 1, 0);
            this._fieldsLayout.Controls.Add(this._lCar, 0, 1);
            this._fieldsLayout.Controls.Add(this._tbCar, 1, 1);
            this._fieldsLayout.Controls.Add(this._lAccessType, 0, 2);
            this._fieldsLayout.Controls.Add(this._cbAccessType, 1, 2);
            this._fieldsLayout.Name = "_fieldsLayout";
            // 
            // _lDoorEnvironment
            // 
            resources.ApplyResources(this._lDoorEnvironment, "_lDoorEnvironment");
            this._lDoorEnvironment.Name = "_lDoorEnvironment";
            // 
            // _tbDoorEnvironment
            // 
            resources.ApplyResources(this._tbDoorEnvironment, "_tbDoorEnvironment");
            this._tbDoorEnvironment.Name = "_tbDoorEnvironment";
            this._tbDoorEnvironment.ReadOnly = true;
            // 
            // _lCar
            // 
            resources.ApplyResources(this._lCar, "_lCar");
            this._lCar.Name = "_lCar";
            // 
            // _tbCar
            // 
            resources.ApplyResources(this._tbCar, "_tbCar");
            this._tbCar.Name = "_tbCar";
            this._tbCar.ReadOnly = true;
            // 
            // _lAccessType
            // 
            resources.ApplyResources(this._lAccessType, "_lAccessType");
            this._lAccessType.Name = "_lAccessType";
            // 
            // _cbAccessType
            // 
            resources.ApplyResources(this._cbAccessType, "_cbAccessType");
            this._cbAccessType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cbAccessType.FormattingEnabled = true;
            this._cbAccessType.Name = "_cbAccessType";
            // 
            // _buttonsPanel
            // 
            resources.ApplyResources(this._buttonsPanel, "_buttonsPanel");
            this._buttonsPanel.Controls.Add(this._bCancel);
            this._buttonsPanel.Controls.Add(this._bOk);
            this._buttonsPanel.Name = "_buttonsPanel";
            // 
            // _bCancel
            // 
            this._bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this._bCancel, "_bCancel");
            this._bCancel.Name = "_bCancel";
            this._bCancel.UseVisualStyleBackColor = true;
            // 
            // _bOk
            // 
            resources.ApplyResources(this._bOk, "_bOk");
            this._bOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._bOk.Name = "_bOk";
            this._bOk.UseVisualStyleBackColor = true;
            // 
            // NCASCarDoorEnvironmentEditForm
            // 
            this.AcceptButton = this._bOk;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this._bCancel;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this._mainLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NCASCarDoorEnvironmentEditForm";
            this.ShowIcon = false;
            this._mainLayout.ResumeLayout(false);
            this._mainLayout.PerformLayout();
            this._fieldsLayout.ResumeLayout(false);
            this._fieldsLayout.PerformLayout();
            this._buttonsPanel.ResumeLayout(false);
            this._buttonsPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _mainLayout;
        private System.Windows.Forms.TableLayoutPanel _fieldsLayout;
        private System.Windows.Forms.Label _lDoorEnvironment;
        private System.Windows.Forms.TextBox _tbDoorEnvironment;
        private System.Windows.Forms.Label _lCar;
        private System.Windows.Forms.TextBox _tbCar;
        private System.Windows.Forms.Label _lAccessType;
        private System.Windows.Forms.ComboBox _cbAccessType;
        private System.Windows.Forms.FlowLayoutPanel _buttonsPanel;
        private System.Windows.Forms.Button _bOk;
        private System.Windows.Forms.Button _bCancel;
    }
}
