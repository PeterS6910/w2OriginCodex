using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Cgp.Components;
using Contal.Cgp.Client.PluginSupport;
using Contal.Cgp.Components;
using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.BaseLib;
using Contal.Cgp.Client;
using Contal.Cgp.Server.Beans;
using Contal.Cgp.Globals;
using Contal.IwQuick;
using Contal.IwQuick.UI;

using SqlDeleteReferenceConstraintException = Contal.IwQuick.SqlDeleteReferenceConstraintException;
using SqlUniqueException = Contal.IwQuick.SqlUniqueException;
using TimeZone = Contal.Cgp.Server.Beans.TimeZone;

namespace Contal.Cgp.NCAS.Client
{
    public partial class NCASAccessZoneCarEditForm :
#if DESIGNER    
        Form
#else
 PluginMainForm<NCASClient>, ICgpDataGridView
#endif
    {
        private Car _actCar;
        private ICollection<AccessZoneCar> _accessZoneCars;
        private BindingSource _bindingSource;
        DVoid2Void _dAfterTranslateForm;

        private ListOfObjects _actLprCameras;
        private TimeZone _actTimeZone;
        private AccessZoneCar _editAccessZone;
        private bool _allowEdit;

        public override NCASClient Plugin
        {
            get { return NCASClient.Singleton; }
        }

        public NCASAccessZoneCarEditForm(Car car, Control control, bool allowEdit)
            : base(NCASClient.LocalizationHelper, CgpClientMainForm.Singleton)
        {
            InitializeComponent();

            LocalizationHelper.TranslateForm(this);
            _dAfterTranslateForm = AfterTranslateForm;
            _pBack.Parent = control;
            _actCar = car;
            control.Enter += RunOnEnter;
            control.Disposed += RunOnDisposed;
            ButtonsCreateAccessZone();

            _allowEdit = allowEdit;
            InitCGPDataGridView();
        }

        private void InitCGPDataGridView()
        {
            _cdgvData.LocalizationHelper = NCASClient.LocalizationHelper;
            _cdgvData.ImageList = ((ICgpVisualPlugin)NCASClient.Singleton).GetPluginObjectsImages();
            _cdgvData.CgpDataGridEvents = this;
            _cdgvData.EnabledInsertButton = false;
            _cdgvData.DataGrid.DragDrop += _dgValues_DragDrop;
            _cdgvData.DataGrid.DragOver += _dgValues_DragOver;
            _cdgvData.DataGrid.MultiSelect = true;
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

            ShowCarAccessZone();
        }

        void RunOnDisposed(object sender, EventArgs e)
        {
            LocalizationHelper.LanguageChanged -= _dAfterTranslateForm;
        }

        private void DisabledForm()
        {
            _cdgvData.EnabledDeleteButton = false;
            //_bEdit.Enabled = false;

            _tbmCardReader.Enabled = false;
            _tbmTimeZone.Enabled = false;
            _eDescription.Enabled = false;
            _bUpdate.Enabled = false;
            _bCancel.Enabled = false;
            _bCreate0.Enabled = false;
        }

        private void EnabledForm()
        {
            _cdgvData.EnabledDeleteButton = true;
            //_bEdit.Enabled = true;

            _tbmCardReader.Enabled = true;
            _tbmTimeZone.Enabled = true;
            _eDescription.Enabled = true;
            _bUpdate.Enabled = true;
            _bCancel.Enabled = true;
            _bCreate0.Enabled = true;
        }

        private void ShowCarAccessZone()
        {
            if (CgpClient.Singleton.IsConnectionLost(true)) return;

            if (InvokeRequired)
            {
                BeginInvoke(new DVoid2Void(ShowCarAccessZone));
            }
            else
            {
                Exception error;
                ICollection<AccessZoneCar> accessZoneCars = Plugin.MainServerProvider.AccessZoneCars.GetAccessZonesByCar(_actCar.IdCar, out error);

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

                    _bindingSource = null;
                    _cdgvData.DataGrid.DataSource = null;
                    return;
                }

                if (_accessZoneCars == null)
                {
                    _accessZoneCars = new List<AccessZoneCar>();
                }
                else
                {
                    _accessZoneCars.Clear();
                }

                _accessZoneCars = accessZoneCars;
                _bindingSource = new BindingSource();
                _bindingSource.DataSource = _accessZoneCars;

                _cdgvData.ModifyGridView(
                    _bindingSource,
                    AccessZoneCar.COLUMNLPRCAMERA,
                    AccessZoneCar.COLUMNTIMEZONE,
                    AccessZoneCar.COLUMNDESCRIPTION);
            }
        }

        protected void HideColumnDgw(DataGridView gridView, string columnName)
        {
            if (gridView == null) return;

            if (gridView.Columns.Contains(columnName))
                gridView.Columns[columnName].Visible = false;
        }

        #region CardReader

        private void ModifyCardReader()
        {
            if (CgpClient.Singleton.IsConnectionLost(true)) return;

            Exception error = null;
            try
            {
                List<AOrmObject> listLprCameras = new List<AOrmObject>();

                ICollection<LprCamera> listLprCamerasFromDatabase = Plugin.MainServerProvider.LprCameras.List(out error);

                if (error != null) throw error;

                if (listLprCamerasFromDatabase != null)
                    listLprCameras.AddRange(listLprCamerasFromDatabase);

                ListboxFormAdd formAdd = new ListboxFormAdd(listLprCameras, GetString("NCASLprCamerasFormNCASLprCamerasForm"));

                ListOfObjects outLprCameras = null;
                //enables to create more object at once by enabling multiselection (only in create mode!!!)
                if (_bCreate0.Visible)
                {
                    formAdd.ShowDialogMultiSelect(out outLprCameras);
                    if (outLprCameras != null)
                    {
                        _actLprCameras = outLprCameras;

                    }
                }
                //editing the object
                else
                {
                    object outLprCamera;
                    formAdd.ShowDialog(out outLprCamera);
                    if (outLprCamera != null)
                    {
                        outLprCameras = new ListOfObjects();
                        outLprCameras.Objects.Add(outLprCamera);

                    }
                }

                if (outLprCameras != null)
                {
                    _actLprCameras = new ListOfObjects();
                    foreach (object selectedObject in outLprCameras)
                    {
                        LprCamera lprCamera = selectedObject as LprCamera;
                        if (lprCamera != null)
                        {
                            _actLprCameras.Objects.Add(lprCamera);
                        }
                    }
                    RefreshCardReaderObject();
                }
            }
            catch
            {
            }
        }

        private void DoAfterCardReaderCreated(object newLprCamera)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object>(DoAfterCardReaderCreated), newLprCamera);
            }
            else
            {
                if (newLprCamera is LprCamera)
                {
                    _actLprCameras = new ListOfObjects();
                    _actLprCameras.Objects.Add(newLprCamera as LprCamera);
                    RefreshCardReaderObject();
                }
            }
        }

        private void RefreshCardReaderObject()
        {
            if (_actLprCameras != null)
            {
                _tbmCardReader.Text = _actLprCameras.ToString();
                _tbmCardReader.TextImage = Plugin.GetImageForListOfObject(_actLprCameras);
            }
            else
            {
                _tbmCardReader.Text = string.Empty;
            }
        }

        private void AddCardReader(object newCardReaderObject)
        {
            try
            {
                if (newCardReaderObject is LprCamera)
                {
                    var lprCamera = newCardReaderObject as LprCamera;
                    _actLprCameras = new ListOfObjects();
                    _actLprCameras.Objects.Add(lprCamera);
                    RefreshCardReaderObject();
                    Plugin.AddToRecentList(newCardReaderObject);
                    return;
                }

                ControlNotification.Singleton.Error(NotificationPriority.JustOne, _tbmCardReader.ImageTextBox,
   CgpClient.Singleton.LocalizationHelper.GetString("ErrorWrongObjectType"), ControlNotificationSettings.Default);
            }
            catch
            { }
        }

        private void _tbmCardReader_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void _tbmCardReader_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] output = e.Data.GetFormats();
                if (output == null) return;
                AddCardReader((object)e.Data.GetData(output[0]));
            }
            catch
            {
            }
        }

        private void _tbmCardReader_DoubleClick(object sender, EventArgs e)
        {
            if (_actLprCameras != null && _actLprCameras.Count == 1)
            {
                if (_actLprCameras[0] is LprCamera)
                {
                    NCASLprCamerasForm.Singleton.OpenEditForm(_actLprCameras[0] as LprCamera);
                }
            }
        }
        #endregion

        #region TimeZone


        private void CreateTimeZoneClick(object sender, EventArgs e)
        {
            TimeZone timeZone = new TimeZone();
            TimeZonesForm.Singleton.OpenInsertFromEdit(ref timeZone, DoAfterTimeZoneCreated);
        }

        private void DoAfterTimeZoneCreated(object newTimeZone)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object>(DoAfterTimeZoneCreated), newTimeZone);
            }
            else
            {
                if (newTimeZone is TimeZone)
                {
                    _actTimeZone = newTimeZone as TimeZone;
                    RefreshTimeZone();
                }
            }
        }
        private void ModifyTimeZone()
        {

            if (CgpClient.Singleton.IsConnectionLost(true)) return;

            try
            {
                List<AOrmObject> listTimeZones = new List<AOrmObject>();

                Exception error;
                ICollection<TimeZone> listTimeZonesFromDatabase = CgpClient.Singleton.MainServerProvider.TimeZones.List(out error);
                foreach (TimeZone timeZone in listTimeZonesFromDatabase)
                {
                    listTimeZones.Add(timeZone);
                }

                ListboxFormAdd formAdd = new ListboxFormAdd(listTimeZones, CgpClient.Singleton.LocalizationHelper.GetString("TimeZonesFormTimeZonesForm"));
                object outTimeZone;
                formAdd.ShowDialog(out outTimeZone);
                if (outTimeZone != null)
                {
                    _actTimeZone = outTimeZone as TimeZone;
                    RefreshTimeZone();
                    Plugin.AddToRecentList(_actTimeZone);
                }
            }
            catch
            {
            }
        }

        private void ModifyTimeZoneClick(object sender, EventArgs e)
        {
            ModifyTimeZone();
        }


        private void RemoveTimeZoneClick(object sender, EventArgs e)
        {
            if (Dialog.Question(GetString("QuestionRemoveTimeZone")))
            {
                _actTimeZone = null;
                RefreshTimeZone();
            }
        }

        private void RefreshTimeZone()
        {
            if (_actTimeZone != null)
            {
                _tbmTimeZone.Text = _actTimeZone.ToString();
                _tbmTimeZone.TextImage = Plugin.GetImageForAOrmObject(_actTimeZone);
            }
            else
                _tbmTimeZone.Text = string.Empty;
        }

        private void TimeZoneDoubleClick(object sender, EventArgs e)
        {
            if (_actTimeZone != null)
                TimeZonesForm.Singleton.OpenEditForm(_actTimeZone);
        }

        private void TimeZoneDragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void TimeZoneDragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] output = e.Data.GetFormats();
                if (output == null) return;
                AddTimeZone((object)e.Data.GetData(output[0]));
            }
            catch
            {
            }
        }

        private void AddTimeZone(object newTimeZone)
        {
            try
            {
                if (newTimeZone.GetType() == typeof(TimeZone))
                {
                    if (Dialog.Question(GetString("QuestionAddTimeZone")))
                    {
                        TimeZone timeZone = newTimeZone as TimeZone;
                        _actTimeZone = timeZone;
                        RefreshTimeZone();
                        Plugin.AddToRecentList(newTimeZone);
                    }
                }
                else
                {
                    ControlNotification.Singleton.Error(NotificationPriority.JustOne, _tbmTimeZone.ImageTextBox,
                       CgpClient.Singleton.LocalizationHelper.GetString("ErrorWrongObjectType"), ControlNotificationSettings.Default);
                }
            }
            catch
            { }
        }

        #endregion

        private void _bCreate0_Click(object sender, EventArgs e)
        {
            CreateNewAccessZone();
        }

        private void _bUpdate_Click(object sender, EventArgs e)
        {
            UpdateAccessZone();
        }

        private void _bCancel_Click(object sender, EventArgs e)
        {
            ClearEdits();
        }

        private void CreateNewAccessZone()
        {
            if (_actLprCameras != null)
            {
                foreach (var selectedLprCamera in _actLprCameras)
                {
                    if (!ControlValues()) return;
                    _editAccessZone = new AccessZoneCar
                    {
                        Car = _actCar,
                        GuidCar = _actCar.IdCar,
                        TimeZone = _actTimeZone,
                        Description = _eDescription.Text,
                        GuidTimeZone = _actTimeZone?.IdTimeZone ?? Guid.Empty
                    };

                    var lprCamera = selectedLprCamera as LprCamera;
                    _editAccessZone.LprCamera = lprCamera;
                    if (lprCamera != null)
                    {
                        _editAccessZone.GuidLprCamera = lprCamera.IdLprCamera;
                    }

                    SaveAccessZoneInsert();
                }
                ShowCarAccessZone();
                ClearEdits();
            }
            else
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne, _tbmCardReader.ImageTextBox,
                GetString("ErrorEntryCardReader"), ControlNotificationSettings.Default);
                _tbmCardReader.ImageTextBox.Focus();
            }
        }

        private void UpdateAccessZone()
        {
            if (!ControlValues() || _actLprCameras.Objects.Count < 1) return;
            _editAccessZone.Car = _actCar;
            _editAccessZone.GuidCar = _actCar.IdCar;

            object lprCameraObject = _actLprCameras.Objects[0];
            if (lprCameraObject is LprCamera)
            {
                _editAccessZone.LprCamera = lprCameraObject as LprCamera;
                _editAccessZone.GuidLprCamera = (lprCameraObject as LprCamera).IdLprCamera;
            }

            _editAccessZone.TimeZone = _actTimeZone;
            _editAccessZone.GuidTimeZone = _actTimeZone?.IdTimeZone ?? Guid.Empty;
            _editAccessZone.Description = _eDescription.Text;

            if (!SaveAccessZoneEdit())
                Dialog.Error("Failed");

            ShowCarAccessZone();
            ClearEdits();
        }

        private void EditAccessZone()
        {
            if (CgpClient.Singleton.IsConnectionLost(false)) return;
            AccessZoneCar accessZone = _bindingSource[_bindingSource.Position] as AccessZoneCar;
            Exception error;
            _editAccessZone = Plugin.MainServerProvider.AccessZoneCars.GetObjectForEdit(accessZone.IdAccessZoneCar, out error);
            if (_editAccessZone != null)
            {
                ButtonsEditAccessZone();
                if (_editAccessZone.TimeZone != null)
                {
                    _actTimeZone = CgpClient.Singleton.MainServerProvider.TimeZones.GetObjectById(_editAccessZone.TimeZone.IdTimeZone);
                }
                else
                {
                    _actTimeZone = null;
                }
                _actLprCameras = new ListOfObjects();
                if (_editAccessZone.LprCamera != null)
                {
                    _actLprCameras.Objects.Add(Plugin.MainServerProvider.LprCameras.GetObjectById(_editAccessZone.GuidLprCamera));
                }
                _eDescription.Text = _editAccessZone.Description;
                RefreshCardReaderObject();
                RefreshTimeZone();
                ButtonsEditAccessZone();
            }
        }

        private void ClearEdits()
        {
            _actTimeZone = null;
            _actLprCameras = null;
            _tbmCardReader.Text = string.Empty;
            _tbmTimeZone.Text = string.Empty;
            _eDescription.Text = string.Empty;
            ButtonsCreateAccessZone();
        }

        private bool SaveAccessZoneInsert()
        {
            Exception error;
            bool retValue = Plugin.MainServerProvider.AccessZoneCars.Insert(ref _editAccessZone, out error);
            if (!retValue && error != null)
            {
                if (error is SqlUniqueException)
                {
                    Dialog.Error(GetString("ErrorUsedAccessZonePair"));
                    _tbmCardReader.ImageTextBox.Focus();
                }
            }
            return retValue;
        }

        private bool SaveAccessZoneEdit()
        {
            Exception error;
            bool retValue = Plugin.MainServerProvider.AccessZoneCars.Update(_editAccessZone, out error);
            if (!retValue && error != null)
            {
                if (error is SqlUniqueException)
                {
                    Dialog.Error(GetString("ErrorUsedAccessZonePair"));
                    _tbmCardReader.ImageTextBox.Focus();
                }
            }
            return retValue;
        }

        private bool ControlValues()
        {
            if (_actLprCameras == null)
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne, _tbmCardReader.ImageTextBox,
                GetString("ErrorEntryCardReader"), ControlNotificationSettings.Default);
                _tbmCardReader.ImageTextBox.Focus();
                return false;
            }
            return true;
        }

        private void ButtonsCreateAccessZone()
        {
            _bCancel.Visible = false;
            _bUpdate.Visible = false;
            _bCreate0.Visible = true;
            _cdgvData.EnabledDeleteButton = true;
        }

        private void ButtonsEditAccessZone()
        {
            _bCancel.Visible = true;
            _bUpdate.Visible = true;
            _bCreate0.Visible = false;
            _cdgvData.EnabledDeleteButton = false;
        }

        private void _dgValues_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void _dgValues_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] output = e.Data.GetFormats();
                if (output == null) return;
                AddDDAccessZone((object)e.Data.GetData(output[0]));
            }
            catch
            {
            }
        }

        private void AddDDAccessZone(object newCardReaderObject)
        {
            try
            {
                if (newCardReaderObject is LprCamera)
                {
                    var lprCamera = newCardReaderObject as LprCamera;
                    var accessZone = new AccessZoneCar
                    {
                        Car = _actCar,
                        GuidCar = _actCar.IdCar,
                        TimeZone = null,
                        LprCamera = lprCamera,
                        GuidLprCamera = lprCamera?.IdLprCamera ?? Guid.Empty
                    };

                    Exception error;
                    bool retValue = Plugin.MainServerProvider.AccessZoneCars.Insert(ref accessZone, out error);
                    if (!retValue && error != null)
                    {
                        if (error is SqlUniqueException)
                        {
                            Dialog.Error(GetString("ErrorUsedAccessZonePair"));
                        }
                        else
                        {
                            Dialog.Error(GetString("ErrorInsertCardReaderObject"));
                        }
                        return;
                    }
                    //EditTextChanger(null, null);
                    ShowCarAccessZone();
                    Plugin.AddToRecentList(newCardReaderObject);
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

        private void _tbmTimeZone_ButtonPopupMenuItemClick(ToolStripItem item, int index)
        {
            if (item.Name == "_tsiModify2")
            {
                ModifyTimeZone();
            }
            else if (item.Name == "_tsiRemove")
            {
                if (Dialog.Question(GetString("QuestionRemoveTimeZone")))
                {
                    _actTimeZone = null;
                    RefreshTimeZone();
                }
            }
            else if (item.Name == "_tsiCreate2")
            {
                TimeZone timeZone = new TimeZone();
                TimeZonesForm.Singleton.OpenInsertFromEdit(ref timeZone, DoAfterTimeZoneCreated);
            }
        }

        private void _tbmCardReader_ButtonPopupMenuItemClick(ToolStripItem item, int index)
        {
            if (item.Name == "_tsiModify")
            {
                ModifyCardReader();
            }
        }

        public static void SaveAfterInsertWithData(NCASClient ncasClient, Car car, Car clonedCar)
        {
            if (ncasClient == null || car == null || CgpClient.Singleton.IsConnectionLost(false)) return;

            IList<FilterSettings> filterSettings = new List<FilterSettings>();
            FilterSettings filterSetting = new FilterSettings(AccessZoneCar.COLUMNCAR, clonedCar, ComparerModes.EQUALL);
            filterSettings.Add(filterSetting);

            Exception error;
            ICollection<AccessZoneCar> accessZoneCars = ncasClient.MainServerProvider.AccessZoneCars.SelectByCriteria(filterSettings, out error);

            if (accessZoneCars != null && accessZoneCars.Count > 0)
            {
                foreach (AccessZoneCar accessZone in accessZoneCars)
                {
                    if (accessZone != null)
                    {
                        AccessZoneCar newAccessZone = new AccessZoneCar();
                        newAccessZone.Car = car;
                        newAccessZone.GuidCar = car.IdCar;
                        newAccessZone.LprCamera = accessZone.LprCamera;
                        newAccessZone.GuidLprCamera = accessZone.GuidLprCamera;
                        newAccessZone.TimeZone = accessZone.TimeZone;
                        newAccessZone.GuidTimeZone = accessZone.GuidTimeZone;
                        newAccessZone.Description = accessZone.Description;

                        ncasClient.MainServerProvider.AccessZoneCars.Insert(ref newAccessZone, out error);
                    }
                }
            }
        }

        public void EditClick()
        {
            EditAccessZone();
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
                var accessZone = _bindingSource[_bindingSource.Position] as AccessZoneCar;

                if (accessZone != null)
                {
                    if (Dialog.Question(CgpClient.Singleton.LocalizationHelper.GetString("QuestionDeleteConfirm")))
                    {
                        Exception error;
                        Plugin.MainServerProvider.AccessZoneCars.Delete(accessZone, out error);

                        if (error != null)
                        {
                            if (error is SqlDeleteReferenceConstraintException)
                                Dialog.Error(CgpClient.Singleton.LocalizationHelper.GetString("ErrorDeleteRowInRelationship"));
                            else
                                Dialog.Error(CgpClient.Singleton.LocalizationHelper.GetString("ErrorDeleteFailed"));
                        }
                        else
                        {
                            ShowCarAccessZone();
                        }
                    }
                }
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
                indexes.Select(index => (IShortObject)(new AccessZoneCarShort((AccessZoneCar)_bindingSource.List[index])))
                    .ToList();

            var dialog = new DeleteDataGridItemsDialog(
                Plugin.GetPluginObjectsImages(),
                items,
                CgpClient.Singleton.LocalizationHelper)
            {
                SelectItem = items.FirstOrDefault(item => item.Id.Equals((((AccessZoneCar)_bindingSource.Current).IdAccessZoneCar)))
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var accessZones = new LinkedList<AccessZoneCar>();

                if (dialog.DeleteAll)
                {
                    foreach (int index in indexes)
                    {
                        if (_bindingSource.Count > index)
                            accessZones.AddLast(_bindingSource[index] as AccessZoneCar);
                    }
                }
                else
                {
                    foreach (var item in dialog.SelectedItems)
                    {
                        var accessZone =
                            _bindingSource.List.Cast<AccessZoneCar>()
                                .FirstOrDefault(az => az.IdAccessZoneCar.Equals(item.Id));

                        accessZones.AddLast(accessZone);
                    }
                }

                DeleteAccessZones(accessZones);
            }
        }

        private void DeleteAccessZones(IEnumerable<AccessZoneCar> accessZones)
        {
            if (accessZones == null)
                return;

            Exception error = null;

            foreach (var accessZone in accessZones)
            {
                Plugin.MainServerProvider.AccessZoneCars.Delete(accessZone, out error);
            }

            if (error != null)
            {
                if (error is SqlDeleteReferenceConstraintException)
                    Dialog.Error(CgpClient.Singleton.LocalizationHelper.GetString("ErrorDeleteRowInRelationship"));
                else
                    Dialog.Error(CgpClient.Singleton.LocalizationHelper.GetString("ErrorDeleteFailed"));
            }

            ShowCarAccessZone();
        }

        public void InsertClick()
        {
        }
    }
}
