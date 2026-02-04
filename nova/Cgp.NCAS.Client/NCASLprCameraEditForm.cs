using Contal.Cgp.BaseLib;
using Contal.Cgp.Client;
using Contal.Cgp.Client.PluginSupport;
using Contal.Cgp.Globals;
using Contal.Cgp.NCAS.RemotingCommon;
using Contal.Cgp.NCAS.Server.Beans;
using Contal.IwQuick;
using Contal.IwQuick.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Contal.Cgp.NCAS.Client
{
    public partial class NCASLprCameraEditForm :
#if DESIGNER
        Form
#else
        ACgpPluginEditFormWithAlarmInstructions<LprCamera>
#endif
    {
        private sealed class CcuComboItem
        {
            public CcuComboItem(Guid id, string display)
            {
                Id = id;
                Display = display;
            }

            public Guid Id { get; }

            public string Display { get; }

            public override string ToString()
            {
                return Display;
            }
        }

#if !DESIGNER
        private bool _isLoadingValues;
        private IList<CcuComboItem> _ccuItems;
        private bool _controlsInitialized;
        private bool _eventsRegistered;
#endif
#if DESIGNER
        public NCASLprCameraEditForm()
        {
            InitializeComponent();
        }
#else
        public NCASLprCameraEditForm(
            LprCamera camera,
            ShowOptionsEditForm showOption,
            PluginMainForm<NCASClient> myTableForm)
            : base(
                camera ?? new LprCamera(),
                showOption,
                CgpClientMainForm.Singleton,
                myTableForm,
                NCASClient.LocalizationHelper)
        {
            InitializeComponent();
            _controlsInitialized = true;
            RegisterEvents();
            _bApply.Enabled = false;
        }
#endif

#if !DESIGNER
        protected override void RegisterEvents()
        {
            if (!_controlsInitialized || _eventsRegistered)
                return;

            _eName.TextChanged += EditTextChanger;
            _eIpAddress.TextChanged += EditTextChanger;
            _ePort.TextChanged += EditTextChanger;
            _ePortSsl.TextChanged += EditTextChanger;
            _eMacAddress.TextChanged += EditTextChanger;
            _eDescription.TextChanged += EditTextChanger;
            _eLocalAlarmInstruction.TextChanged += EditTextChanger;
            _cbCommunicationScope.SelectedIndexChanged += ControlChanged;
            _cbCcu.SelectedIndexChanged += ControlChanged;
            _chkEnableParentInFullName.CheckedChanged += ControlChanged;
            _chkLocked.CheckedChanged += ControlChanged;
            _chkIsOnline.CheckedChanged += ControlChanged;

            _eventsRegistered = true;
        }

        protected override void BeforeInsert()
        {
            NCASLprCamerasForm.Singleton.BeforeInsert(this);
        }

        protected override void BeforeEdit()
        {
            NCASLprCamerasForm.Singleton.BeforeEdit(this, _editingObject);
        }

        public override void ReloadEditingObject(out bool allowEdit)
        {
            allowEdit = true;

            var table = GetLprCameraTable();
            if (table == null)
                return;

            var result = GetObjectForEditById(table, _editingObject.IdLprCamera, out allowEdit);

            if (!allowEdit)
            {
                var readOnly = table.GetObjectById(_editingObject.IdLprCamera);
                if (readOnly != null)
                    _editingObject = readOnly;
                DisableForm();
            }
            else if (result != null)
            {
                _editingObject = result;
            }
        }

        public override void ReloadEditingObjectWithEditedData()
        {
            var table = GetLprCameraTable();
            if (table == null)
                return;

            try
            {
                Exception error;
                table.RenewObjectForEdit(_editingObject.IdLprCamera, out error);

                if (error != null)
                    throw error;
            }
            catch (MissingMethodException)
            {

            }
        }

        protected override void SetValuesInsert()
        {
            SetFormValues();
        }

        protected override void SetValuesEdit()
        {
            SetFormValues();
        }

        private void SetFormValues()
        {
            LockChanges();
            _isLoadingValues = true;
            try
            {
                EnsureCommunicationScopeItems();
                EnsureHealthStateItems();
                EnsureCcuItems();
                _eName.Text = _editingObject.Name ?? string.Empty;
                _eIpAddress.Text = _editingObject.IpAddress ?? string.Empty;
                _ePort.Text = _editingObject.Port ?? string.Empty;
                _ePortSsl.Text = _editingObject.PortSsl ?? string.Empty;
                _eMacAddress.Text = _editingObject.MacAddress ?? string.Empty;
                _eDescription.Text = _editingObject.Description ?? string.Empty;
                _eLocalAlarmInstruction.Text = _editingObject.LocalAlarmInstruction ?? string.Empty;
                _cbCommunicationScope.SelectedItem = _editingObject.CommunicationScope;
                SelectCcu(_editingObject.CCU?.IdCCU ?? Guid.Empty);
                _chkLocked.Checked = _editingObject.Locked;
                _eLockingClientIp.Text = _editingObject.LockingClientIp ?? string.Empty;
                _chkIsOnline.Checked = _editingObject.IsOnline;
                _eLastHeartbeatAt.Text = _editingObject.LastHeartbeatAt?.ToString("g", CultureInfo.CurrentCulture) ?? string.Empty;
                _eLastLicensePlate.Text = _editingObject.LastLicensePlate ?? string.Empty;
                _cbHealthState.SelectedItem = _editingObject.HealthState;
                _chkEnableParentInFullName.Checked = _editingObject.EnableParentInFullName;
                _eCkUnique.Text = _editingObject.CkUnique != Guid.Empty
                    ? _editingObject.CkUnique.ToString()
                    : string.Empty;
                _eObjectType.Text = _editingObject.ObjectType.ToString(CultureInfo.InvariantCulture);
                _eVersion.Text = _editingObject.Version.ToString(CultureInfo.InvariantCulture);
                UpdateCameraStreamPreview();
            }
            finally
            {
                _isLoadingValues = false;
                UnlockChanges();
            }
        }

        private void UpdateCameraStreamPreview()
        {
            if (_cameraStreamBrowser == null)
                return;

            var ipAddress = _eIpAddress.Text?.Trim();
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                _cameraStreamBrowser.DocumentText = BuildCameraStreamPlaceholder("IP address is not set.");
                return;
            }

            var url = BuildCameraStreamUrl();
            if (string.IsNullOrWhiteSpace(url))
            {
                _cameraStreamBrowser.DocumentText = BuildCameraStreamPlaceholder("Unable to open the provided IP address.");
                return;
            }

            if (IsNanopackCamera())
                url = AppendNanopackHomePath(url);

            try
            {
                _cameraStreamBrowser.Navigate(url);
            }
            catch (UriFormatException)
            {
                _cameraStreamBrowser.DocumentText = BuildCameraStreamPlaceholder("Unable to open the provided IP address.");
            }
        }

        private string BuildCameraStreamUrl()
        {
            var ipAddress = _eIpAddress.Text?.Trim();
            if (string.IsNullOrWhiteSpace(ipAddress))
                return null;

            var hasScheme = ipAddress.StartsWith("http", StringComparison.OrdinalIgnoreCase);
            var baseUrl = hasScheme
                ? ipAddress
                : $"http://{ipAddress}";

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
                return null;

            var portText = GetPortTextForScheme(uri.Scheme);
            var port = TryParsePort(portText);
            if (port.HasValue && uri.IsDefaultPort)
            {
                var builder = new UriBuilder(uri) { Port = port.Value };
                uri = builder.Uri;
            }

            return uri.ToString();
        }

        private string GetPortTextForScheme(string scheme)
        {
            if (scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                return _ePortSsl.Text;

            return _ePort.Text;
        }

        private static int? TryParsePort(string portText)
        {
            if (string.IsNullOrWhiteSpace(portText))
                return null;

            if (!int.TryParse(portText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var port))
                return null;

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                return null;

            return port;
        }

        private static string BuildCameraStreamPlaceholder(string message)
        {
            var safeMessage = WebUtility.HtmlEncode(message ?? string.Empty);
            return "<html><body style='font-family:Segoe UI, sans-serif;font-size:14px;background-color:#1e1e1e;color:#f3f3f3;"
                   + "display:flex;align-items:center;justify-content:center;height:100%;margin:0;'>" + safeMessage
                   + "</body></html>";
        }

        private bool IsNanopackCamera()
        {
            return ContainsNanopack(_editingObject?.Name) || ContainsNanopack(_editingObject?.Description);
        }

        private static string AppendNanopackHomePath(string url)
        {
            const string nanopackHomePath = "/#/home";
            if (string.IsNullOrWhiteSpace(url))
                return url;

            if (url.IndexOf(nanopackHomePath, StringComparison.OrdinalIgnoreCase) >= 0)
                return url;

            return $"{url.TrimEnd('/')}{nanopackHomePath}";
        }

        private static readonly char[] NanopackNameSeparators = { ' ', '-', '_' };

        private static bool ContainsNanopack(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            if (text.IndexOf("nanopack", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            var tokens = text.Split(NanopackNameSeparators, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                if (token.Equals("nanopack", StringComparison.OrdinalIgnoreCase)
                    || token.Equals("nanopack5", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        protected override bool CheckValues()
        {
            if (string.IsNullOrWhiteSpace(_eName.Text))
            {
                Dialog.Error("Name can not be empty.");
                _eName.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_eIpAddress.Text)
                && !IPAddress.TryParse(_eIpAddress.Text.Trim(), out _))
            {
                Dialog.Error(GetString("ErrorNotValidIpAddress"));
                _eIpAddress.Focus();
                return false;
            }

            return true;
        }

        protected override bool GetValues()
        {
            _editingObject.Name = _eName.Text.Trim();
            _editingObject.IpAddress = _eIpAddress.Text.Trim();
            _editingObject.Port = _ePort.Text.Trim();
            _editingObject.PortSsl = _ePortSsl.Text.Trim();
            _editingObject.MacAddress = _eMacAddress.Text.Trim();
            _editingObject.Description = _eDescription.Text.Trim();
            _editingObject.LocalAlarmInstruction = _eLocalAlarmInstruction.Text;
            _editingObject.CommunicationScope = _cbCommunicationScope.SelectedItem is CommunicationScope scope
    ? scope
    : CommunicationScope.CcuOnly;
            _editingObject.EnableParentInFullName = _chkEnableParentInFullName.Checked;
            _editingObject.Locked = _chkLocked.Checked;
            _editingObject.IsOnline = _chkIsOnline.Checked;

            var selectedCcuId = GetSelectedCcuId();
            if (selectedCcuId == Guid.Empty)
            {
                _editingObject.CCU = null;
            }
            else if (_editingObject.CCU == null || _editingObject.CCU.IdCCU != selectedCcuId)
            {
                var provider = Plugin?.MainServerProvider?.CCUs;
                if (provider != null)
                {
                    try
                    {
                        _editingObject.CCU = provider.GetObjectById(selectedCcuId);
                    }
                    catch
                    {
                        _editingObject.CCU = new CCU { IdCCU = selectedCcuId };
                    }
                }
                else
                {
                    _editingObject.CCU = new CCU { IdCCU = selectedCcuId };
                }
            }

            return true;
        }

        protected override bool SaveToDatabaseInsert()
        {
            var table = GetLprCameraTableOrThrow();
            var result = table.Insert(ref _editingObject, out var error);

            if (error != null)
                throw error;

            return result;
        }

        protected override bool SaveToDatabaseEdit()
        {
            var table = GetLprCameraTableOrThrow();

            var result = table.Update(_editingObject, out var error);


            if (error != null)
                throw error;

            return result;
        }

        protected override void UnregisterEvents()
        {
            if (!_eventsRegistered)
                return;

            _eName.TextChanged -= EditTextChanger;
            _eIpAddress.TextChanged -= EditTextChanger;
            _ePort.TextChanged -= EditTextChanger;
            _ePortSsl.TextChanged -= EditTextChanger;
            _eMacAddress.TextChanged -= EditTextChanger;
            _eDescription.TextChanged -= EditTextChanger;
            _eLocalAlarmInstruction.TextChanged -= EditTextChanger;
            _cbCommunicationScope.SelectedIndexChanged -= ControlChanged;
            _cbCcu.SelectedIndexChanged -= ControlChanged;
            _chkEnableParentInFullName.CheckedChanged -= ControlChanged;
            _chkLocked.CheckedChanged -= ControlChanged;
            _chkIsOnline.CheckedChanged -= ControlChanged;

            _eventsRegistered = false;
        }

        protected override void EditEnd()
        {
            var table = GetLprCameraTable();
            if (table == null)
                return;

            table.EditEnd(_editingObject);
        }

        protected override void AfterInsert()
        {
            NCASLprCamerasForm.Singleton.AfterInsert();
        }

        protected override void AfterEdit()
        {
            NCASLprCamerasForm.Singleton.AfterEdit(_editingObject);
        }

        protected override string GetLocalAlarmInstruction()
        {
            return _editingObject.LocalAlarmInstruction;
        }

        protected override void EditTextChanger(object sender, EventArgs e)
        {
            if (_isLoadingValues)
                return;

            base.EditTextChanger(sender, e);

            if (ReferenceEquals(sender, _eIpAddress))
                UpdateCameraStreamPreview();

            if (!Insert)
                _bApply.Enabled = true;
        }

        private void ControlChanged(object sender, EventArgs e)
        {
            if (_isLoadingValues)
                return;

            EditTextChanger(sender, e);
        }

        private void EnsureCommunicationScopeItems()
        {
            if (_cbCommunicationScope.Items.Count > 0)
                return;

            _cbCommunicationScope.Items.AddRange(Enum.GetValues(typeof(CommunicationScope))
                .Cast<object>()
                .ToArray());
        }

        private void EnsureHealthStateItems()
        {
            if (_cbHealthState.Items.Count > 0)
                return;

            _cbHealthState.Items.AddRange(Enum.GetValues(typeof(HealthState))
                .Cast<object>()
                .ToArray());
        }

        private string GetStringOrDefault(string key, string fallback)
        {
            var value = GetString(key);
            return string.IsNullOrEmpty(value) || string.Equals(value, key, StringComparison.Ordinal)
                ? fallback
                : value;
        }

        private void EnsureCcuItems()
        {
            if (_ccuItems != null)
                return;

            var provider = Plugin?.MainServerProvider?.CCUs;
            var noneText = GetStringOrDefault("None", "<None>");
            if (provider == null)
            {
                _ccuItems = new List<CcuComboItem> { new CcuComboItem(Guid.Empty, noneText) };
                _cbCcu.DataSource = _ccuItems;
                _cbCcu.Enabled = false;
                return;
            }

            try
            {
                Exception error;
                var ccus = provider.ShortSelectByCriteria(null, out error) ?? new List<CCUShort>();

                if (error != null)
                    throw error;

                _ccuItems = new List<CcuComboItem>
                {
                    new CcuComboItem(Guid.Empty, noneText)
                };

                _ccuItems = _ccuItems
                    .Concat(ccus
                        .OrderBy(ccu => ccu.IndexCCU)
                        .ThenBy(ccu => ccu.Name)
                        .Select(ccu => new CcuComboItem(ccu.IdCCU, string.Format(CultureInfo.CurrentCulture, "{0} - {1}", ccu.IndexCCU, ccu.Name ?? string.Empty))))
                    .ToList();

                _cbCcu.DataSource = _ccuItems;
                _cbCcu.Enabled = true;
            }
            catch
            {
                _ccuItems = new List<CcuComboItem> { new CcuComboItem(Guid.Empty, noneText) };
                _cbCcu.DataSource = _ccuItems;
                _cbCcu.Enabled = false;
            }
        }

        private void SelectCcu(Guid ccuId)
        {
            if (_ccuItems == null)
            {
                _cbCcu.SelectedIndex = -1;
                return;
            }

            if (ccuId != Guid.Empty && _ccuItems.All(item => item.Id != ccuId))
            {
                var wasEnabled = _cbCcu.Enabled;
                var missingText = GetStringOrDefault("NotAvailable", "Not available");
                _ccuItems.Add(new CcuComboItem(ccuId, string.Format(CultureInfo.CurrentCulture, "{0} ({1})", ccuId, missingText)));
                _cbCcu.DataSource = null;
                _cbCcu.DataSource = _ccuItems;
                _cbCcu.Enabled = wasEnabled;
            }

            var index = _ccuItems
                .Select((item, position) => new { item, position })
                .FirstOrDefault(pair => pair.item.Id == ccuId)?.position ?? 0;

            if (index >= 0 && index < _cbCcu.Items.Count)
                _cbCcu.SelectedIndex = index;
            else
                _cbCcu.SelectedIndex = 0;
        }

        private Guid GetSelectedCcuId()
        {
            return _cbCcu.SelectedItem is CcuComboItem item
                ? item.Id
                : Guid.Empty;
        }

        private static LprCamera GetObjectForEditById(ILprCameras table, Guid id, out bool allowEdit)
        {
            try
            {
                return table.GetObjectForEditById(id, out allowEdit);
            }
            catch (Exception ex) when (ex is MissingMethodException
                                       || ex is NotImplementedException
                                       || ex.InnerException is MissingMethodException)
            {
                var result = table.GetObjectForEdit(id, out var error);

                if (error is AccessDeniedException)
                {
                    allowEdit = false;
                    return null;
                }

                if (error != null)
                    throw error;

                allowEdit = true;
                return result;
            }
        }

        private ILprCameras GetLprCameraTable()
        {
            var provider = Plugin?.MainServerProvider;
            if (provider == null)
                return null;

            return provider.LprCameras;
        }

        private ILprCameras GetLprCameraTableOrThrow()
        {
            var table = GetLprCameraTable();
            if (table == null)
                throw new InvalidOperationException("Lpr cameras provider is not available.");

            return table;
        }
#endif

        private void _bApply_Click(object sender, EventArgs e)
        {
#if !DESIGNER
            if (Apply_Click())
                _bApply.Enabled = false;
#endif
        }

        private void _bOk_Click(object sender, EventArgs e)
        {
#if !DESIGNER
            Ok_Click();
#endif
        }

        private void _bCancel_Click(object sender, EventArgs e)
        {
#if !DESIGNER
            Cancel_Click();
#else
            Close();
#endif
        }
    }
}
