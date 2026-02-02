using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Cgp.Components;
using Contal.Cgp.Client.PluginSupport;
using Contal.Cgp.Components;
using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.BaseLib;
using Contal.Cgp.Client;
using Contal.Cgp.Server.Beans;
using Contal.IwQuick.UI;
using Contal.Cgp.Globals;
using Contal.IwQuick.Threads;
using Contal.IwQuick;

using SqlDeleteReferenceConstraintException = Contal.IwQuick.SqlDeleteReferenceConstraintException;
using SqlUniqueException = Contal.IwQuick.SqlUniqueException;

namespace Contal.Cgp.NCAS.Client
{
    public partial class NCASACLCarEditForm :
#if DESIGNER    
        Form
#else
 PluginMainForm<NCASClient>, ICgpDataGridView
#endif
    {
        private ListOfObjects _actAccessControlLists;
        private readonly Car _actCar;
        private BindingSource _bindingSource;
        private ACLCar _editingAclCar;
        DVoid2Void _dAfterTranslateForm;
        private readonly bool _allowEdit;
        private readonly Control _parentControl;

        public override NCASClient Plugin
        {
            get { return NCASClient.Singleton; }
        }

        public NCASACLCarEditForm(Car car, Control control, bool allowEdit)
            : base(NCASClient.LocalizationHelper, CgpClientMainForm.Singleton)
        {
            InitializeComponent();
            _tbdpDateFrom.LocalizationHelper = LocalizationHelper;
            _tbdpDateTo.LocalizationHelper = LocalizationHelper;
            LocalizationHelper.TranslateForm(this);
            _pBack.Parent = control;
            _parentControl = control;
            _actCar = car;
            control.Enter += RunOnEnter;
            control.Disposed += RunOnDisposed;
            ResetValues();
            MinimumSize = new Size(405, 334);
            _allowEdit = allowEdit;
            InitCGPDataGridView();
            _cdgvData.DataGrid.MultiSelect = true;
        }

        private void InitCGPDataGridView()
        {
            _cdgvData.LocalizationHelper = NCASClient.LocalizationHelper;
            _cdgvData.ImageList = ((ICgpVisualPlugin)NCASClient.Singleton).GetPluginObjectsImages();
            _cdgvData.CgpDataGridEvents = this;
            _cdgvData.DataGrid.DragDrop += _dgValues_DragDrop;
            _cdgvData.DataGrid.DragOver += _dgValues_DragOver;
            _cdgvData.EnabledInsertButton = false;
        }

        protected override void AfterTranslateForm()
        {
            LocalizationHelper.TranslateControl(_pBack);
        }

        void RunOnEnter(object sender, EventArgs e)
        {
            if (CgpClient.Singleton.IsConnectionLost(false)) return;
            if (_dAfterTranslateForm == null)
            {
                _dAfterTranslateForm = AfterTranslateForm;
                LocalizationHelper.LanguageChanged += _dAfterTranslateForm;
            }

            if (CgpClient.Singleton.IsConnectionLost(true)) return;

            if (_allowEdit)
                EnabledForm();
            else
                DisabledForm();

            SafeThread.StartThread(ShowACLCars);
        }

        private void DisabledForm()
        {
            _tbmAccessControlList.Enabled = false;
            _tbdpDateFrom.Enabled = false;
            _tbdpDateTo.Enabled = false;

            _bCreate1.Enabled = false;
            //_bEdit.Enabled = false;
            _cdgvData.EnabledDeleteButton = false;
        }

        private void EnabledForm()
        {
            _tbmAccessControlList.Enabled = true;
            _tbdpDateFrom.Enabled = true;
            _tbdpDateTo.Enabled = true;

            _bCreate1.Enabled = true;
            //_bEdit.Enabled = true;
            _cdgvData.EnabledDeleteButton = true;
        }

        void RunOnDisposed(object sender, EventArgs e)
        {
            LocalizationHelper.LanguageChanged -= _dAfterTranslateForm;
        }

        private void ShowACLCars()
        {
            if (CgpClient.Singleton.IsConnectionLost(true)) return;

            if (_parentControl == null)
                return;

            Exception error;
            var aclCars = Plugin.MainServerProvider.ACLCars.GetAclCarsByCar(_actCar.IdCar, out error);

            if (error != null)
            {
                if (error is AccessDeniedException)
                {
                    Dialog.Error(CgpClient.Singleton.LocalizationHelper.GetString("ErrorLoadTableAccessDenied"));
                }
                else
                {
                    Dialog.Error(CgpClient.Singleton.LocalizationHelper.GetString("ErrorLoadTable"));
                }

                _parentControl.Invoke(new DVoid2Void(
                    delegate
                    {
                        _bindingSource = null;
                        _cdgvData.DataGrid.DataSource = null;
                    }));

                return;
            }

            _parentControl.Invoke(new DVoid2Void(
                delegate
                {
                    _bindingSource =
                        new BindingSource
                        {
                            DataSource = aclCars
                        };

                    _cdgvData.ModifyGridView(
                        _bindingSource,
                        ACLCar.COLUMN_ACCESS_CONTROL_LIST,
                        ACLCar.COLUMN_DATE_FROM,
                        ACLCar.COLUMN_DATE_TO);
                    _cdgvData.DataGrid.Columns[ACLCar.COLUMN_DATE_FROM].DefaultCellStyle.Format = "MM-dd-yyyy HH:mm:ss";
                    _cdgvData.DataGrid.Columns[ACLCar.COLUMN_DATE_FROM].AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.DisplayedCells;
                    _cdgvData.DataGrid.Columns[ACLCar.COLUMN_DATE_TO].DefaultCellStyle.Format =
                        "MM-dd-yyyy HH:mm:ss";
                }));
        }

        protected void HideColumnDgw(DataGridView gridView, string columnName)
        {
            if (gridView == null) return;

            if (gridView.Columns.Contains(columnName))
                gridView.Columns[columnName].Visible = false;
        }

        private void RefreshAccessControlList()
        {
            if (_actAccessControlLists != null)
            {
                _tbmAccessControlList.Text = _actAccessControlLists.ToString();
                _tbmAccessControlList.TextImage = Plugin.GetImageForObjectType(ObjectType.AccessControlList);
            }
            else
            {
                _tbmAccessControlList.Text = string.Empty;
            }
        }

        private void ModifyAccesControlList()
        {
            if (CgpClient.Singleton.IsConnectionLost(true)) return;

            try
            {
                var listAccessControlList = new List<AOrmObject>();

                Exception error;
                var listAccessControlListFromDatabase = Plugin.MainServerProvider.AccessControlLists.List(out error);
                foreach (var accessControlList in listAccessControlListFromDatabase)
                {
                    listAccessControlList.Add(accessControlList);
                }

                var formAdd = new ListboxFormAdd(listAccessControlList, GetString("NCASAccessControlListsFormNCASAccessControlListsForm"));
                ListOfObjects outAccessControlLists;
                if (_bCreate1.Visible)
                {
                    formAdd.ShowDialogMultiSelect(out outAccessControlLists);
                    if (outAccessControlLists != null)
                    {
                        _actAccessControlLists = outAccessControlLists;
                        RefreshAccessControlList();
                    }
                }
                else
                {
                    object outAccessControlList;
                    formAdd.ShowDialog(out outAccessControlList);
                    if (outAccessControlList is AccessControlList)
                    {
                        outAccessControlLists = new ListOfObjects();
                        outAccessControlLists.Objects.Add(outAccessControlList);
                        _actAccessControlLists = outAccessControlLists;
                        RefreshAccessControlList();
                    }
                }
            }
            catch
            {
            }
        }

        private void _tbmAccessControlList_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void _tbmAccessControlList_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var output = e.Data.GetFormats();
                if (output == null) return;
                AddAccessControlList(e.Data.GetData(output[0]));
            }
            catch
            {
            }
        }

        private void AddAccessControlList(object newAccessControlList)
        {
            try
            {
                if (newAccessControlList.GetType() == typeof(AccessControlList))
                {
                    var accessControlList = newAccessControlList as AccessControlList;
                    _actAccessControlLists = new ListOfObjects();
                    _actAccessControlLists.Objects.Add(accessControlList);
                    RefreshAccessControlList();
                    Plugin.AddToRecentList(newAccessControlList);
                }
                else
                {
                    ControlNotification.Singleton.Error(NotificationPriority.JustOne, _tbmAccessControlList.ImageTextBox,
                       CgpClient.Singleton.LocalizationHelper.GetString("ErrorWrongObjectType"), ControlNotificationSettings.Default);
                }
            }
            catch
            { }
        }

        private void _tbmAccessControlList_DoubleClick(object sender, EventArgs e)
        {
            if (_actAccessControlLists != null && _actAccessControlLists.Count == 1)
            {
                NCASAccessControlListsForm.Singleton.OpenEditForm(_actAccessControlLists[0] as AccessControlList);
            }
        }

        private bool ControlValues()
        {
            if (_actAccessControlLists == null)
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne, _tbmAccessControlList.ImageTextBox,
                GetString("ErrorEntryAccessControlList"), ControlNotificationSettings.Default);
                return false;
            }

            if ((_tbdpDateTo.Value != null && _tbdpDateFrom.Value != null) && _tbdpDateFrom.Value > _tbdpDateTo.Value)
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne,
                   _tbdpDateTo.TextBox, GetString("ErrorACLDateRange"), ControlNotificationSettings.Default);
                return false;
            }

            return true;
        }

        private void GetValues(ACLCar aclCar, AccessControlList accessControlList)
        {
            aclCar.AccessControlList = accessControlList;
            aclCar.Car = _actCar;

            if (_tbdpDateFrom.Text != string.Empty)
            {
                DateTime dateTime;
                if (DateTime.TryParse(_tbdpDateFrom.Text, out dateTime))
                {
                    aclCar.DateFrom = dateTime;
                }
            }
            else
                aclCar.DateFrom = null;

            if (_tbdpDateTo.Text != string.Empty)
            {
                DateTime dateTime;
                if (DateTime.TryParse(_tbdpDateTo.Text, out dateTime))
                {
                    aclCar.DateTo = dateTime;
                }
            }
            else
                aclCar.DateTo = null;
        }

        private bool GetValues(ACLCar aclCar)
        {
            if (!ControlValues())
            {
                return false;
            }

            if (_actAccessControlLists.Objects.Count == 0)
            {
                return false;
            }

            try
            {
                GetValues(aclCar, _actAccessControlLists.Objects[0] as AccessControlList);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool GetValues(out IList<ACLCar> aclCars)
        {
            aclCars = null;

            if (!ControlValues())
            {
                return false;
            }

            try
            {
                aclCars = new List<ACLCar>();

                foreach (var obj in _actAccessControlLists.Objects)
                {
                    var aclCar = new ACLCar();
                    GetValues(aclCar, obj as AccessControlList);
                    aclCars.Add(aclCar);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void _bCreate1_Click(object sender, EventArgs e)
        {
            if (CgpClient.Singleton.IsConnectionLost(false)) return;

            IList<ACLCar> aclCars;

            if (!ControlValues())
            {
                return;
            }

            if (!GetValues(out aclCars))
                return;

            if (!Dialog.Question(GetString("QuestionInsertACLCar")))
                return;

            DateTime? dateTimeTo = null;
            if (_tbdpDateTo.Text != string.Empty)
            {
                DateTime dateTime;
                if (DateTime.TryParse(_tbdpDateTo.Text, out dateTime))
                    dateTimeTo = dateTime;
            }

            if ((dateTimeTo != null && aclCars != null && aclCars.EmploymentEndDate != null &&
                dateTimeTo.Value.CompareTo(aclCars.EmploymentEndDate) > 0) ||
                (dateTimeTo == null && aclCars != null && aclCars.EmploymentEndDate != null))
            {
                if (!Dialog.WarningQuestion(GetString("QuestionUpdateACLDateTo")))
                    return;
            }

            CreateAclCar(aclCars);
        }

        private void CreateAclCar(IList<ACLCar> aclCars)
        {
            SafeThread<IList<ACLCar>>.StartThread(DoCreateAclCar, aclCars);
        }

        private void DoCreateAclCar(IList<ACLCar> aclCars)
        {
            if (aclCars != null && aclCars.Count > 0)
            {
                foreach (var actAclCar in aclCars)
                {
                    Exception error;
                    var aclCar = actAclCar;
                    Plugin.MainServerProvider.ACLCars.Insert(ref aclCar, out error);
                    if (error != null)
                    {
                        if (error is AccessDeniedException)
                        {
                            Dialog.Error(CgpClient.Singleton.LocalizationHelper.GetString("ErrorInsertAccessDenied"));
                        }
                        else if (error is SqlUniqueException)
                        {
                            Dialog.Error(GetString("ErrorUsedAccessControlListCar"));
                        }
                        else
                        {
                            Dialog.Error(CgpClient.Singleton.LocalizationHelper.GetString("ErrorInsertFailed"));
                        }

                        ShowACLCars();
                        return;
                    }
                }

                ResetValues();
                ShowACLCars();
            }
        }

        private void ResetValues()
        {
            _actAccessControlLists = null;

            if (_parentControl == null)
                return;

            _parentControl.Invoke(new DVoid2Void(
                delegate
                {
                    RefreshAccessControlList();
                    _tbdpDateFrom.Value = DateTime.Now.Date;
                    _tbdpDateTo.Value = null;
                }));
        }

        private void DeleteAclCar(ICollection<ACLCar> aclCars)
        {
            SafeThread<ACLCar>.StartThread(DoDeleteAclCar, aclCars);
        }

        private void DoDeleteAclCar(ICollection<ACLCar> aclCars)
        {
            if (aclCars != null)
            {
                foreach (var aclCar in aclCars)
                {
                    if (!aclCar.Compare(_editingAclCar))
                    {
                        Exception error;
                        Plugin.MainServerProvider.ACLCars.Delete(aclCar, out error);

                        if (error != null)
                        {
                            if (error is SqlDeleteReferenceConstraintException)
                                Dialog.Error(
                                    CgpClient.Singleton.LocalizationHelper.GetString("ErrorDeleteRowInRelationship"));
                            else
                                Dialog.Error(CgpClient.Singleton.LocalizationHelper.GetString("ErrorDeleteFailed"));
                        }
                        else
                        {
                            ShowACLCars();
                        }
                    }
                    else
                    {
                        Dialog.Error(CgpClient.Singleton.LocalizationHelper.GetString("ErrorDeleteEditing"));
                    }
                }
            }
        }

        private void SetValues()
        {
            if (_editingAclCar != null)
            {
                _actAccessControlLists = new ListOfObjects();
                _actAccessControlLists.Objects.Add(_editingAclCar.AccessControlList);

                if (_parentControl == null)
                    return;

                _parentControl.Invoke(new DVoid2Void(
                    delegate
                    {
                        RefreshAccessControlList();

                        if (_editingAclCar.DateFrom != null)
                            _tbdpDateFrom.Value = _editingAclCar.DateFrom.Value;
                        else
                            _tbdpDateFrom.Value = null;

                        if (_editingAclCar.DateTo != null)
                            _tbdpDateTo.Value = _editingAclCar.DateTo.Value;
                        else
                            _tbdpDateTo.Value = null;
                    }));
            }
        }

        private void _bCancelEdit_Click(object sender, EventArgs e)
        {
            if (CgpClient.Singleton.IsConnectionLost(false)) return;
            _editingAclCar = null;

            if (_parentControl != null)
            {
                _parentControl.Invoke(new DVoid2Void(
                    delegate
                    {
                        _bUpdate.Visible = false;
                        _bCreate1.Visible = true;
                        _bCancelEdit.Visible = false;
                    }));
            }

            ResetValues();
        }

        private void _bUpdate_Click(object sender, EventArgs e)
        {
            if (CgpClient.Singleton.IsConnectionLost(false)) return;
            if (!GetValues(_editingAclCar))
                return;

            if (!Dialog.Question(GetString("QuestionUpdateACLCar")))
                return;

            UpdateAclCar();
        }

        private void UpdateAclCar()
        {
            SafeThread.StartThread(DoUpdateAclCar);
        }

        private void DoUpdateAclCar()
        {
            Exception error;
            Plugin.MainServerProvider.ACLCars.Update(_editingAclCar, out error);
            if (error != null)
            {
                if (error is AccessDeniedException)
                {
                    Dialog.Error(CgpClient.Singleton.LocalizationHelper.GetString("ErrorEditAccessDenied"));
                }
                else if (error is SqlUniqueException)
                {
                    Dialog.Error(GetString("ErrorUsedAccessControlListCar"));
                }
                else if (error is IncoherentDataException)
                {
                    if (Dialog.WarningQuestion(CgpClient.Singleton.LocalizationHelper.GetString("QuestionLoadActualData")))
                    {
                        _editingAclCar = Plugin.MainServerProvider.ACLCars.GetObjectForEdit(_editingAclCar.IdACLCar, out error);
                        SetValues();
                    }
                    else
                    {
                        _editingAclCar = Plugin.MainServerProvider.ACLCars.GetObjectForEdit(_editingAclCar.IdACLCar, out error);
                    }
                }
                else
                {
                    Dialog.Error(
                        CgpClient.Singleton.LocalizationHelper.GetString("ErrorEditFailed") +
                        ": " + error.Message);
                }

                return;
            }

            Plugin.MainServerProvider.ACLCars.EditEnd(_editingAclCar);
            _bCancelEdit_Click(null, null);
            ResetValues();
            ShowACLCars();
        }

        private void DoAfterACCreated(object newAccessControlList)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object>(DoAfterACCreated), newAccessControlList);
            }
            else
            {
                if (newAccessControlList is AccessControlList)
                {
                    _actAccessControlLists = new ListOfObjects();
                    _actAccessControlLists.Objects.Add(newAccessControlList as AccessControlList);
                    RefreshAccessControlList();
                }
            }
        }

        private static void _dgValues_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void _dgValues_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var output = e.Data.GetFormats();
                if (output == null) return;
                SafeThread<object>.StartThread(AddACLCar, e.Data.GetData(output[0]));
            }
            catch
            {
            }
        }

        private void AddACLCar(object newAccessControlList)
        {
            if (CgpClient.Singleton.IsConnectionLost(false)) return;
            try
            {
                if (newAccessControlList.GetType() == typeof(AccessControlList))
                {
                    var accessControlList = newAccessControlList as AccessControlList;
                    var aclCar =
                        new ACLCar
                        {
                            AccessControlList = accessControlList,
                            DateFrom = DateTime.Now.Date,
                            DateTo = null,
                            Car = _actCar
                        };

                    Exception error;
                    Plugin.MainServerProvider.ACLCars.Insert(ref aclCar, out error);
                    if (error != null)
                    {
                        if (error is AccessDeniedException)
                        {
                            Dialog.Error(CgpClient.Singleton.LocalizationHelper.GetString("ErrorInsertAccessDenied"));
                        }
                        else if (error is SqlUniqueException)
                        {
                            Dialog.Error(GetString("ErrorUsedAccessControlListCar"));
                        }
                        else
                        {
                            Dialog.Error(CgpClient.Singleton.LocalizationHelper.GetString("ErrorInsertFailed"));
                        }

                        return;
                    }

                    ShowACLCars();
                    Plugin.AddToRecentList(newAccessControlList);
                }
                else
                {
                    ControlNotification.Singleton.Error(NotificationPriority.JustOne, _cdgvData.DataGrid,
                       CgpClient.Singleton.LocalizationHelper.GetString("ErrorWrongObjectType"), ControlNotificationSettings.Default);
                }
            }
            catch
            { }
        }

        private void _tbmAccessControlList_ButtonPopupMenuItemClick(ToolStripItem item, int index)
        {
            if (item.Name == "_tsiModify")
            {
                ModifyAccesControlList();
            }
            else if (item.Name == "_tsiCreate")
            {
                var accessControlList = new AccessControlList();
                NCASAccessControlListsForm.Singleton.OpenInsertFromEdit(ref accessControlList, DoAfterACCreated);
            }
        }

        public static void SaveAfterInsertWithData(NCASClient ncasClient, Car car, Car clonedCar)
        {
            if (ncasClient == null || car == null || CgpClient.Singleton.IsConnectionLost(false)) return;

            IList<FilterSettings> filterSettings = new List<FilterSettings>();
            var filterSetting = new FilterSettings(ACLCar.COLUMN_CAR, clonedCar, ComparerModes.EQUALL);
            filterSettings.Add(filterSetting);

            Exception error;
            var aclCars = ncasClient.MainServerProvider.ACLCars.SelectByCriteria(filterSettings, out error);

            if (aclCars != null && aclCars.Count > 0)
            {
                foreach (var aclCar in aclCars)
                {
                    if (aclCar != null)
                    {
                        var newACLCar =
                            new ACLCar
                            {
                                Car = car,
                                AccessControlList = aclCar.AccessControlList,
                                DateFrom = aclCar.DateFrom,
                                DateTo = aclCar.DateTo
                            };

                        ncasClient.MainServerProvider.ACLCars.Insert(ref newACLCar, out error);
                    }
                }
            }
        }

        #region ICgpDataGridView Members

        public void EditClick()
        {
            if (CgpClient.Singleton.IsConnectionLost(false))
                return;

            if (_bindingSource != null && _bindingSource.Count > 0)
            {
                _editingAclCar = _bindingSource[_bindingSource.Position] as ACLCar

                if (_editingAclCar != null)
                {
                    Exception error;
                    _editingAclCar = Plugin.MainServerProvider.ACLCars.GetObjectForEdit(_editingAclCar.IdACLCar, out error);

                    if (_editingAclCar != null)
                    {
                        SetValues();
                        _bCreate1.Visible = false;
                        _bUpdate.Visible = true;
                        _bCancelEdit.Visible = true;
                    }
                }
            }
        }

        public virtual void EditClick(ICollection<int> indexes)
        {
        }

        public void DeleteClick()
        {
            if (CgpClient.Singleton.IsConnectionLost(false))
                return;

            if (_bindingSource != null && _bindingSource.Count > 0)
            {
                var aclCars = new LinkedList<ACLCar>();

                foreach (var selectedRow in _cdgvData.DataGrid.SelectedRows)
                {
                    aclCars.AddLast(_bindingSource[((DataGridViewRow)selectedRow).Index] as ACLCar);
                }

                DeleteAclCar(aclCars);
            }
        }

        public virtual void DeleteClick(ICollection<int> indexes)
        {
            if (CgpClient.Singleton.IsConnectionLost(false))
                return;

            if (_bindingSource == null
                || _bindingSource.Count == 0)
                return;

            var items =
                 indexes.Select(index => (IShortObject)(new AccessControlListShort(((ACLCar)_bindingSource.List[index]).AccessControlList)))
                    .ToList();

            var dialog = new DeleteDataGridItemsDialog(
                Plugin.GetPluginObjectsImages(),
                items,
                CgpClient.Singleton.LocalizationHelper)
            {
                SelectItem = items.FirstOrDefault(item => item.Id.Equals((((ACLCar)_bindingSource.Current).AccessControlList.IdAccessControlList)))
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var aclCars = new LinkedList<ACLCar>();

                if (dialog.DeleteAll)
                {
                    foreach (int index in indexes)
                    {
                        if (_bindingSource.Count > index)
                            aclCars.AddLast(_bindingSource[index] as ACLCar);
                    }
                }
                else
                {
                    foreach (var item in dialog.SelectedItems)
                    {
                        var aclCar =
                            _bindingSource.List.Cast<ACLCar>()
                                .FirstOrDefault(car => car.AccessControlList.IdAccessControlList.Equals(item.Id));

                        aclCars.AddLast(aclCar);
                    }
                }

                DeleteAclCar(aclCars);
            }
        }

        public void InsertClick()
        {
        }

        #endregion
    }
}
