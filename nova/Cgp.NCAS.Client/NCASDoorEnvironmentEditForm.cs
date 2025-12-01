using Contal.Cgp.Client;
using Contal.Cgp.Client.PluginSupport;
using Contal.Cgp.Components;
using Contal.Cgp.Globals;
using Contal.Cgp.NCAS.Definitions;
using Contal.Cgp.NCAS.RemotingCommon;
using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.RemotingCommon;
using Contal.Cgp.Server.Beans;
using Contal.IwQuick;
using Contal.IwQuick.Data;
using Contal.IwQuick.Localization;
using Contal.IwQuick.PlatformPC.UI.Accordion;
using Contal.IwQuick.Threads;
using Contal.IwQuick.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;

namespace Contal.Cgp.NCAS.Client
{
    public partial class NCASDoorEnvironmentEditForm :
#if DESIGNER
    Form
#else
        ACgpPluginEditFormWithAlarmInstructions<DoorEnvironment>
#endif
                , ICgpDataGridView
    {
        private const int DEFAULTIMPULSEDELAY = 1000;
        public const int BEFORE_UNLOCK_TIME = 50;

        private Action<Guid, byte> _eventStateChanged;
        private Action<ObjectType, object, bool> _cudObjectEvent;
        private readonly Color _baseColorResultAccessGranted;

        private AOrmObject _crInternal;
        private AOrmObject _crExternal;

        private readonly ControlAlarmTypeSettings _catsDsmIntrusion;
        private readonly ControlModifyAlarmArcs _cmaaDsmIntrusion;
        private readonly ControlAlarmTypeSettings _catsDsmDoorAjar;
        private readonly ControlModifyAlarmArcs _cmaaDsmDoorAjar;
        private readonly ControlAlarmTypeSettings _catsDsmSabotage;
        private readonly ControlModifyAlarmArcs _cmaaDsmSabotage;

        private readonly Dictionary<AlarmType, Action> _openAndScrollToControlByAlarmType;

        private readonly List<CarDoorEnvironment> _carDoorEnvironments = new List<CarDoorEnvironment>();

        public NCASDoorEnvironmentEditForm(
                DoorEnvironment doorEnvironment,
                ShowOptionsEditForm showOption,
                PluginMainForm<NCASClient> myTableForm)
            : base(
                doorEnvironment,
                showOption,
                CgpClientMainForm.Singleton,
                myTableForm,
                NCASClient.LocalizationHelper)
        {
            _openAndScrollToControlByAlarmType = new Dictionary<AlarmType, Action>
            {
                {
                    AlarmType.DoorEnvironment_DoorAjar,
                    () =>
                    {
                        _tcDoorsAutomat.SelectedTab = _tpAlarmSettings;
                        _accordionAlarmSettings.OpenAndScrollToControl(_catsDsmDoorAjar);
                    }
                },
                {
                    AlarmType.DoorEnvironment_Intrusion,
                    () =>
                    {
                        _tcDoorsAutomat.SelectedTab = _tpAlarmSettings;
                        _accordionAlarmSettings.OpenAndScrollToControl(_catsDsmIntrusion);
                    }
                },
                {
                    AlarmType.DoorEnvironment_Sabotage,
                    () =>
                    {
                        _tcDoorsAutomat.SelectedTab = _tpAlarmSettings;
                        _accordionAlarmSettings.OpenAndScrollToControl(_catsDsmSabotage);
                    }
                }
            };

            InitializeComponent();

            _chbIsVehicleAccess.Text = GetString("NCASDoorEnvironmentEditForm_chbIsVehicleAccess");
            _tpCar.Text = GetString("NCASDoorEnvironmentEditForm_tpCar");
            _bAddCarDoorEnvironment.Text = GetString("General_bAdd");
            _bEditCarDoorEnvironment.Text = GetString("General_bEdit");
            _bRemoveCarDoorEnvironment.Text = GetString("General_bRemove");
            _tcCarColumn.HeaderText = _tpCar.Text;
            _tcAccessTypeColumn.HeaderText = GetString("NCASDoorEnvironmentEditForm_AccessType");

            _dgCarDoorEnvironments.LocalizationHelper = NCASClient.LocalizationHelper;
            _dgCarDoorEnvironments.ImageList = ((ICgpVisualPlugin)Plugin).GetPluginObjectsImages();
            _dgCarDoorEnvironments.CgpDataGridEvents = this;
            _dgCarDoorEnvironments.EnabledInsertButton = false;
            _dgCarDoorEnvironments.EnabledDeleteButton = true;
            _dgCarDoorEnvironments.AutoOpenEditFormByDoubleClick = false;
            RefreshCarDoorEnvironmentsFromServer();

            _catsDsmDoorAjar = new ControlAlarmTypeSettings
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Name = "_catsDsmDoorAjar",
                TabIndex = 0,
                Plugin = Plugin,
                ThreeStateCheckBoxes = true,
                AlarmEnabledCheckState = null,
                BlockAlarmCheckState = null,
                EventlogDuringBlockedAlarmVisible = false
            };

            _catsDsmDoorAjar.EditTextChanger += () => EditTextChanger(null, null);
            _catsDsmDoorAjar.EditTextChangerOnlyInDatabase += () => EditTextChangerOnlyInDatabase(null, null);

            _cmaaDsmDoorAjar = new ControlModifyAlarmArcs
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Name = "_cmaaDsmDoorAjar",
                TabIndex = 1,
                Plugin = Plugin
            };

            _cmaaDsmDoorAjar.EditTextChanger += () => EditTextChanger(null, null);

            _catsDsmDoorAjar.AddControl(_cmaaDsmDoorAjar);

            _accordionAlarmSettings.Add(_catsDsmDoorAjar, "DSM door ajar").Name = "_chbDsmDoorAjar";

            _catsDsmIntrusion = new ControlAlarmTypeSettings
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Name = "_catsDsmIntrusion",
                TabIndex = 2,
                Plugin = Plugin,
                ThreeStateCheckBoxes = true,
                AlarmEnabledCheckState = null,
                BlockAlarmCheckState = null,
                EventlogDuringBlockedAlarmVisible = false
            };

            _catsDsmIntrusion.EditTextChanger += () => EditTextChanger(null, null);
            _catsDsmIntrusion.EditTextChangerOnlyInDatabase += () => EditTextChangerOnlyInDatabase(null, null);

            _cmaaDsmIntrusion = new ControlModifyAlarmArcs
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Name = "_cmaaDsmIntrusion",
                TabIndex = 3,
                Plugin = Plugin
            };

            _cmaaDsmIntrusion.EditTextChanger += () => EditTextChanger(null, null);

            _catsDsmIntrusion.AddControl(_cmaaDsmIntrusion);

            _accordionAlarmSettings.Add(_catsDsmIntrusion, "DSM intrusion").Name = "_chbDsmIntrusion";

            _catsDsmSabotage = new ControlAlarmTypeSettings
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Name = "_catsDsmSabotage",
                TabIndex = 4,
                Plugin = Plugin,
                ThreeStateCheckBoxes = true,
                AlarmEnabledCheckState = null,
                BlockAlarmCheckState = null,
                EventlogDuringBlockedAlarmVisible = false
            };

            _catsDsmSabotage.EditTextChanger += () => EditTextChanger(null, null);
            _catsDsmSabotage.EditTextChangerOnlyInDatabase += () => EditTextChangerOnlyInDatabase(null, null);

            _cmaaDsmSabotage = new ControlModifyAlarmArcs
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Name = "_cmaaDsmSabotage",
                TabIndex = 5,
                Plugin = Plugin
            };

            _cmaaDsmSabotage.EditTextChanger += () => EditTextChanger(null, null);

            _catsDsmSabotage.AddControl(_cmaaDsmSabotage);

            _accordionAlarmSettings.Add(_catsDsmSabotage, "DSM sabotage").Name = "_chbDsmSabotage";

            _accordionAlarmSettings.OpenedAllChnged +=
                isOpenAll => SetOpenAllCheckState(
                    _cbOpenAllAlarmSettings,
                    isOpenAll);

            _baseColorResultAccessGranted = _eResultAccessGranted.BackColor;
            WheelTabContorol = _tcDoorsAutomat;
            MinimumSize = new Size(Width, Height);
            SetReferenceEditColors();

            _chbNotInvokeIntrusionAlarm.Visible = false;
            SafeThread.StartThread(HideDisableTabPages);

            ObjectsPlacementHandler.AddImageList(_lbUserFolders);
        }

        private void SetOpenAllCheckState(CheckBox checkBox, bool? isOpenAll)
        {
            checkBox.CheckState = isOpenAll.HasValue
                ? (isOpenAll.Value
                    ? CheckState.Checked
                    : CheckState.Unchecked)
                : CheckState.Indeterminate;
        }

        private void HideDisableTabPages()
        {
            try
            {
                HideDisableTabPageDoorTiming(
                    CgpClient.Singleton.MainServerProvider.HasAccess(
                        NCASAccess.GetAccess(AccessNCAS.DoorEnvironmentsDoorTimingView)),
                    CgpClient.Singleton.MainServerProvider.HasAccess(
                        NCASAccess.GetAccess(AccessNCAS.DoorEnvironmentsDoorTimingAdmin)));

                HideDisableTabPageApas(
                    CgpClient.Singleton.MainServerProvider.HasAccess(
                        NCASAccess.GetAccess(AccessNCAS.DoorEnvironmentsApasView)),
                    CgpClient.Singleton.MainServerProvider.HasAccess(
                        NCASAccess.GetAccess(AccessNCAS.DoorEnvironmentsApasAdmin)));

                HideDisableTabPageControl(
                    Plugin.MainServerProvider.DoorEnvironments.HasAccessToAccessGranted(
                        _editingObject.IdDoorEnvironment));

                HideDisableTabPageAlarmSettings(
                    CgpClient.Singleton.MainServerProvider.HasAccess(
                        NCASAccess.GetAccess(AccessNCAS.DoorEnvironmentsAlarmSettingsView)),
                    CgpClient.Singleton.MainServerProvider.HasAccess(
                        NCASAccess.GetAccess(AccessNCAS.DoorEnvironmentsAlarmSettingsAdmin)));

                HideDisableTabPageSpecialOutputs(
                    CgpClient.Singleton.MainServerProvider.HasAccess(
                        NCASAccess.GetAccess(AccessNCAS.DoorEnvironmentsSpecialOutputsView)),
                    CgpClient.Singleton.MainServerProvider.HasAccess(
                        NCASAccess.GetAccess(AccessNCAS.DoorEnvironmentsSpecialOutputsAdmin)));

                HideDisableTabFoldersSructure(ObjectsPlacementHandler.HasAccessView());

                HideDisableTabReferencedBy(
                    CgpClient.Singleton.MainServerProvider.HasAccess(
                        NCASAccess.GetAccess(AccessNCAS.DoorEnvironmentsReferencedByView)));

                HideDisableTabPageDescription(
                    CgpClient.Singleton.MainServerProvider.HasAccess(
                        NCASAccess.GetAccess(AccessNCAS.DoorEnvironmentsDescriptionView)),
                    CgpClient.Singleton.MainServerProvider.HasAccess(
                        NCASAccess.GetAccess(AccessNCAS.DoorEnvironmentsDescriptionAdmin)));
            }
            catch
            {
            }
        }

        private void HideDisableTabPageDoorTiming(bool view, bool admin)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool, bool>(HideDisableTabPageDoorTiming),
                    view,
                    admin);
            }
            else
            {
                if (!admin)
                {
                    _eName.ReadOnly = true;
                }

                if (!view && !admin)
                {
                    _tcDoorsAutomat.TabPages.Remove(_tpDoorsTiming);
                    return;
                }

                _tpDoorsTiming.Enabled = admin;
            }
        }

        private void HideDisableTabPageApas(bool view, bool admin)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool, bool>(HideDisableTabPageApas),
                    view,
                    admin);
            }
            else
            {
                if (!view && !admin)
                {
                    _tcDoorsAutomat.TabPages.Remove(_tpApas);
                    return;
                }

                _tpApas.Enabled = admin;
            }
        }

        private void HideDisableTabPageControl(bool view)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(HideDisableTabPageControl),
                    view);
            }
            else
            {
                if (!view)
                {
                    //_tcDoorsAutomat.TabPages.Remove(_tpControl);
                }
            }
        }

        private void HideDisableTabPageAlarmSettings(bool view, bool admin)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool, bool>(HideDisableTabPageAlarmSettings),
                    view,
                    admin);
            }
            else
            {
                if (!view && !admin)
                {
                    _tcDoorsAutomat.TabPages.Remove(_tpAlarmSettings);
                    return;
                }

                _tpAlarmSettings.Enabled = admin;
            }
        }

        private void HideDisableTabPageSpecialOutputs(bool view, bool admin)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool, bool>(HideDisableTabPageSpecialOutputs),
                    view,
                    admin);
            }
            else
            {
                if (!view && !admin)
                {
                    _tcDoorsAutomat.TabPages.Remove(_tpSpecialOutputs);
                    return;
                }

                _tpSpecialOutputs.Enabled = admin;
            }
        }

        private void HideDisableTabFoldersSructure(bool view)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(HideDisableTabFoldersSructure), view);
            }
            else
            {
                if (!view)
                {
                    _tcDoorsAutomat.TabPages.Remove(_tpUserFolders);
                }
            }
        }

        private void HideDisableTabReferencedBy(bool view)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(HideDisableTabReferencedBy), view);
            }
            else
            {
                if (!view)
                {
                    _tcDoorsAutomat.TabPages.Remove(_tpReferencedBy);
                }
            }
        }

        private void HideDisableTabPageDescription(bool view, bool admin)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool, bool>(HideDisableTabPageDescription),
                    view,
                    admin);
            }
            else
            {
                if (!view && !admin)
                {
                    _tcDoorsAutomat.TabPages.Remove(_tpDescription);
                    return;
                }

                _tpDescription.Enabled = admin;
            }
        }

        protected override void RegisterEvents()
        {
            _eventStateChanged = ChangeState;
            DoorEnvironmentStateChangedHandler.Singleton.RegisterStateChanged(_eventStateChanged);
            _cudObjectEvent = CudObjectEvent;
            CUDObjectHandler.Singleton.Register(_cudObjectEvent, ObjectType.CardReader, ObjectType.Input,
                ObjectType.Output);
        }

        protected override void UnregisterEvents()
        {
            if (_eventStateChanged != null)
                DoorEnvironmentStateChangedHandler.Singleton.UnregisterStateChanged(_eventStateChanged);

            if (_cudObjectEvent != null)
                CUDObjectHandler.Singleton.Unregister(_cudObjectEvent, ObjectType.CardReader, ObjectType.Input,
                    ObjectType.Output);
        }

        protected override void BeforeInsert()
        {
            NCASDoorEnvironmentsForm.Singleton.BeforeInsert(this);
        }

        protected override void BeforeEdit()
        {
            NCASDoorEnvironmentsForm.Singleton.BeforeEdit(this, _editingObject);
        }

        protected override void AfterInsert()
        {
            NCASDoorEnvironmentsForm.Singleton.AfterInsert();
        }

        protected override void AfterEdit()
        {
            NCASDoorEnvironmentsForm.Singleton.AfterEdit(_editingObject);
        }

        public override void ReloadEditingObject(out bool allowEdit)
        {
            Exception error;
            var obj = Plugin.MainServerProvider.DoorEnvironments.GetObjectForEdit(_editingObject.IdDoorEnvironment,
                out error);
            if (error != null)
            {
                if (error is AccessDeniedException)
                {
                    allowEdit = false;
                    obj = Plugin.MainServerProvider.DoorEnvironments.GetObjectById(_editingObject.IdDoorEnvironment);
                }
                else
                {
                    throw error;
                }
                DisableForm();
            }
            else
            {
                allowEdit = true;
            }

            _editingObject = obj;
        }

        public override void ReloadEditingObjectWithEditedData()
        {
            Exception error;
            Plugin.MainServerProvider.DoorEnvironments.RenewObjectForEdit(_editingObject.IdDoorEnvironment, out error);
            if (error != null)
            {
                throw error;
            }
        }

        protected override void SetValuesInsert()
        {
            _chbIsVehicleAccess.Checked = false;
            _cbDoorEnvironment_SelectedIndexChanged(null, null);
            RefreshInternalCardReader();
            RefreshExternalCardReader();
            _bApply.Enabled = false;

            _carDoorEnvironments.Clear();
            LoadCarDoorEnvironments();
        }

        protected override void SetValuesEdit()
        {
            _eName.Text = _editingObject.Name;

            _eTimeOpen.Value = _editingObject.DoorTimeOpen;
            _eTimeSirenAjar.Value = _editingObject.DoorTimeSirenAjar;
            _eTimeUnlock.Value = _editingObject.DoorTimeUnlock;
            _eTimePreAlarm.Value = _editingObject.DoorTimePreAlarm;

            _eDelayBeforeUnlock.Value = _editingObject.DoorDelayBeforeUnlock;
            _eDelayBeforeLock.Value = _editingObject.DoorDelayBeforeLock;
            _eDelayBeforeClose.Value = _editingObject.DoorDelayBeforeClose;
            _eDelayBeforeBreakIn.Value = _editingObject.DoorDelayBeforeBreakIn;
            _chbNotInvokeIntrusionAlarm.Checked = _editingObject.NotInvokeIntrusionAlarm;
            _chbIsVehicleAccess.Checked = _editingObject.IsVehicleAccess;

            if (_editingObject.SensorsLockDoors != null)
            {
                _cbLockDoor.Items.Add(_editingObject.SensorsLockDoors);
                _cbLockDoor.SelectedItem = _editingObject.SensorsLockDoors;

                _chbLockDoorInverted.Enabled = true;
                _chbLockDoorBalanced.Enabled = true;

                _chbLockDoorInverted.Checked = _editingObject.SensorsLockDoorsInverted;
                _chbLockDoorBalanced.Checked = _editingObject.SensorsLockDoorsBalanced;
            }
            else
            {
                _cbLockDoor.SelectedItem = null;

                _chbLockDoorInverted.Enabled = false;
                _chbLockDoorBalanced.Enabled = false;
            }

            if (_editingObject.SensorsOpenDoors != null)
            {
                _cbOpenDoor.Items.Add(_editingObject.SensorsOpenDoors);
                _cbOpenDoor.SelectedItem = _editingObject.SensorsOpenDoors;

                _chbOpenDoorInverted.Enabled = true;
                _chbOpenDoorBalanced.Enabled = true;

                _chbOpenDoorInverted.Checked = _editingObject.SensorsOpenDoorsInverted;
                _chbOpenDoorBalanced.Checked = _editingObject.SensorsOpenDoorsBalanced;
            }
            else
            {
                _cbOpenDoor.SelectedItem = null;

                _chbOpenDoorInverted.Enabled = false;
                _chbOpenDoorBalanced.Enabled = false;
            }

            if (_editingObject.SensorsOpenMaxDoors != null)
            {
                _cbOpenMax.Items.Add(_editingObject.SensorsOpenMaxDoors);
                _cbOpenMax.SelectedItem = _editingObject.SensorsOpenMaxDoors;

                _chbOpenMaxDoorInverted.Enabled = true;
                _chbOpenMaxDoorBalanced.Enabled = true;

                _chbOpenMaxDoorInverted.Checked = _editingObject.SensorsOpenMaxDoorsInverted;
                _chbOpenMaxDoorBalanced.Checked = _editingObject.SensorsOpenMaxDoorsBalanced;
            }
            else
            {
                _cbOpenMax.SelectedItem = null;

                _chbOpenMaxDoorInverted.Enabled = false;
                _chbOpenMaxDoorBalanced.Enabled = false;
            }

            LoadDoorEnvironmentType();

            if (_editingObject.ActuatorsElectricStrike != null)
            {
                _cbElectricStrike.Items.Add(_editingObject.ActuatorsElectricStrike);
                _cbElectricStrike.SelectedItem = _editingObject.ActuatorsElectricStrike;

                _chbElectricStrikeImpulse.Enabled = true;
                _chbElectricStrikeImpulse.Checked = _editingObject.ActuatorsElectricStrikeImpulse;
                if (_chbElectricStrikeImpulse.Checked)
                {
                    _eElectricStrikeImpulseDelay.Enabled = true;
                    _eElectricStrikeImpulseDelay.Value = _editingObject.ActuatorsElectricStrikeImpulseDelay;
                }
                else
                    _eElectricStrikeImpulseDelay.Enabled = false;
            }
            else
            {
                _cbElectricStrike.SelectedItem = null;

                _chbElectricStrikeImpulse.Enabled = false;
                _eElectricStrikeImpulseDelay.Enabled = false;
            }

            if (_editingObject.ActuatorsElectricStrikeOpposite != null)
            {
                _cbElectricStrikeOpposite.Items.Add(_editingObject.ActuatorsElectricStrikeOpposite);
                _cbElectricStrikeOpposite.SelectedItem = _editingObject.ActuatorsElectricStrikeOpposite;

                var itemDoorEnvironmentType = ((ItemDoorEnvironmentType)_cbDoorEnvironment.SelectedItem);

                _chbElectricStrikeOppositeImpulse.Enabled = itemDoorEnvironmentType.DoorEnvironmentType ==
                                                            DoorEnviromentType.Rotating;
                _chbElectricStrikeOppositeImpulse.Checked = _editingObject.ActuatorsElectricStrikeOppositeImpulse;
                if (_chbElectricStrikeOppositeImpulse.Checked)
                {
                    _eElectricStrikeOppositeImpulseDelay.Enabled = itemDoorEnvironmentType.DoorEnvironmentType ==
                                                                   DoorEnviromentType.Rotating;
                    _eElectricStrikeOppositeImpulseDelay.Value =
                        _editingObject.ActuatorsElectricStrikeOppositeImpulseDelay;
                }
                else
                    _eElectricStrikeOppositeImpulseDelay.Enabled = false;
            }
            else
            {
                _cbElectricStrikeOpposite.SelectedItem = null;

                _chbElectricStrikeOppositeImpulse.Enabled = false;
                _eElectricStrikeOppositeImpulseDelay.Enabled = false;
            }

            if (_editingObject.ActuatorsExtraElectricStrike != null)
            {
                _cbExtraElectricStrike.Items.Add(_editingObject.ActuatorsExtraElectricStrike);
                _cbExtraElectricStrike.SelectedItem = _editingObject.ActuatorsExtraElectricStrike;

                _chbExtraElectricStrikeImpulse.Enabled = true;
                _chbExtraElectricStrikeImpulse.Checked = _editingObject.ActuatorsExtraElectricStrikeImpulse;
                if (_chbExtraElectricStrikeImpulse.Checked)
                {
                    _eExtraElectricStrikeImpulseDelay.Enabled = true;
                    _eExtraElectricStrikeImpulseDelay.Value = _editingObject.ActuatorsExtraElectricStrikeImpulseDelay;
                }
                else
                    _eExtraElectricStrikeImpulseDelay.Enabled = false;
            }
            else
            {
                _cbExtraElectricStrike.SelectedItem = null;

                _chbExtraElectricStrikeImpulse.Enabled = false;
                _eExtraElectricStrikeImpulseDelay.Enabled = false;
            }

            if (_editingObject.ActuatorsExtraElectricStrikeOpposite != null)
            {
                _cbExtraElectricStrikeOpposite.Items.Add(_editingObject.ActuatorsExtraElectricStrikeOpposite);
                _cbExtraElectricStrikeOpposite.SelectedItem = _editingObject.ActuatorsExtraElectricStrikeOpposite;

                _chbExtraElectricStrikeOppositeImpulse.Enabled =
                    ((ItemDoorEnvironmentType)_cbDoorEnvironment.SelectedItem).DoorEnvironmentType ==
                    DoorEnviromentType.Rotating;
                _chbExtraElectricStrikeOppositeImpulse.Checked =
                    _editingObject.ActuatorsExtraElectricStrikeOppositeImpulse;
                if (_chbExtraElectricStrikeOppositeImpulse.Checked)
                {
                    _eExtraElectricStrikeOppositeImpulseDelay.Enabled =
                        ((ItemDoorEnvironmentType)_cbDoorEnvironment.SelectedItem).DoorEnvironmentType ==
                        DoorEnviromentType.Rotating;
                    _eExtraElectricStrikeOppositeImpulseDelay.Value =
                        _editingObject.ActuatorsExtraElectricStrikeOppositeImpulseDelay;
                }
                else
                    _eExtraElectricStrikeOppositeImpulseDelay.Enabled = false;
            }
            else
            {
                _cbExtraElectricStrikeOpposite.SelectedItem = null;

                _chbExtraElectricStrikeOppositeImpulse.Enabled = false;
                _eExtraElectricStrikeOppositeImpulseDelay.Enabled = false;
            }

            if (_editingObject.ActuatorsBypassAlarm != null)
            {
                _cbBypassAlarm.Items.Add(_editingObject.ActuatorsBypassAlarm);
                _cbBypassAlarm.SelectedItem = _editingObject.ActuatorsBypassAlarm;
            }
            else
            {
                _cbBypassAlarm.SelectedItem = null;
            }

            if (_editingObject.DoorAjarOutput != null)
            {
                _cbOutputDoorAjar.Items.Add(_editingObject.DoorAjarOutput);
                _cbOutputDoorAjar.SelectedItem = _editingObject.DoorAjarOutput;
            }
            else
            {
                _cbOutputDoorAjar.SelectedItem = null;
            }

            if (_editingObject.IntrusionOutput != null)
            {
                _cbOutputIntrusion.Items.Add(_editingObject.IntrusionOutput);
                _cbOutputIntrusion.SelectedItem = _editingObject.IntrusionOutput;
            }
            else
            {
                _cbOutputIntrusion.SelectedItem = null;
            }

            if (_editingObject.SabotageOutput != null)
            {
                _cbOutputSabotage.Items.Add(_editingObject.SabotageOutput);
                _cbOutputSabotage.SelectedItem = _editingObject.SabotageOutput;
            }
            else
            {
                _cbOutputSabotage.SelectedItem = null;
            }

            ChangeState(_editingObject.IdDoorEnvironment,
                (byte)
                    Plugin.MainServerProvider.DoorEnvironments.GetDoorEnvironmentState(_editingObject.IdDoorEnvironment));

            _crInternal = _editingObject.CardReaderInternal;
            if (_crInternal == null)
            {
                _crInternal = _editingObject.PushButtonInternal;

                var input = _crInternal as Input;
                if (input != null)
                {
                    _chbPushButtonInternalInverted.Checked = input.Inverted;
                    _chbPushButtonInternalBalanced.Checked = input.InputType == (byte)InputType.BSI;
                }
            }

            _crExternal = _editingObject.CardReaderExternal;
            if (_crExternal == null)
            {
                _crExternal = _editingObject.PushButtonExternal;

                var input = _crExternal as Input;
                if (input != null)
                {
                    _chbPushButtonExternalInverted.Checked = input.Inverted;
                    _chbPushButtonExternalBalanced.Checked = input.InputType == (byte)InputType.BSI;
                }
            }

            if (_editingObject.Description != null)
                _eDescription.Text = _editingObject.Description;

            RefreshCarDoorEnvironmentsFromServer();

            var alarmArcsByAlarmType = new SyncDictionary<AlarmType, ICollection<AlarmArc>>();

            if (_editingObject.DoorEnvironmentAlarmArcs != null)
            {
                foreach (var doorEnvironmentAlarmArc in _editingObject.DoorEnvironmentAlarmArcs)
                {
                    var alarmArc = doorEnvironmentAlarmArc.AlarmArc;

                    alarmArcsByAlarmType.GetOrAddValue(
                        (AlarmType)doorEnvironmentAlarmArc.AlarmType,
                        key =>
                            new LinkedList<AlarmArc>(),
                        (key, value, newlyAdded) =>
                            value.Add(alarmArc));
                }
            }

            _catsDsmDoorAjar.AlarmEnabledCheckState = _editingObject.DoorAjarAlarmOn;
            _catsDsmDoorAjar.BlockAlarmCheckState = _editingObject.BlockAlarmDoorAjar;

            _catsDsmDoorAjar.ObjectForBlockingAlarm = GetBlockAlarmObject(
                _editingObject.ObjBlockAlarmDoorAjarObjectType,
                _editingObject.ObjBlockAlarmDoorAjarId);

            _catsDsmDoorAjar.PresentationGroup = _editingObject.DoorAjarPresentationGroup;

            _cmaaDsmDoorAjar.AlarmArcs = GetAlarmArcs(
                alarmArcsByAlarmType,
                AlarmType.DoorEnvironment_DoorAjar);

            _catsDsmIntrusion.AlarmEnabledCheckState = _editingObject.IntrusionAlarmOn;
            _catsDsmIntrusion.BlockAlarmCheckState = _editingObject.BlockAlarmIntrusion;

            _catsDsmIntrusion.ObjectForBlockingAlarm = GetBlockAlarmObject(
                _editingObject.ObjBlockAlarmIntrusionObjectType,
                _editingObject.ObjBlockAlarmIntrusionId);

            _catsDsmIntrusion.PresentationGroup = _editingObject.IntrusionPresentationGroup;

            _cmaaDsmIntrusion.AlarmArcs = GetAlarmArcs(
                alarmArcsByAlarmType,
                AlarmType.DoorEnvironment_Intrusion);

            _catsDsmSabotage.AlarmEnabledCheckState = _editingObject.SabotageAlarmOn;
            _catsDsmSabotage.BlockAlarmCheckState = _editingObject.BlockAlarmSabotage;

            _catsDsmSabotage.ObjectForBlockingAlarm = GetBlockAlarmObject(
                _editingObject.ObjBlockAlarmSabotageObjectType,
                _editingObject.ObjBlockAlarmSabotageId);

            _catsDsmSabotage.PresentationGroup = _editingObject.SabotagePresentationGroup;

            _cmaaDsmSabotage.AlarmArcs = GetAlarmArcs(
                alarmArcsByAlarmType,
                AlarmType.DoorEnvironment_Sabotage);

            _cbDoorEnvironment_SelectedIndexChanged(null, null);
            RefreshInternalCardReader();
            RefreshExternalCardReader();
            RefreshConfigured();
            RefreshCarDoorEnvironmentsFromServer();
            SetReferencedBy();
            _bApply.Enabled = false;
        }

        private ICollection<AlarmArc> GetAlarmArcs(
            IDictionary<AlarmType, ICollection<AlarmArc>> alarmArcsByAlarmType,
            AlarmType alarmType)
        {
            ICollection<AlarmArc> alarmArcsForAlarmType;

            if (!alarmArcsByAlarmType.TryGetValue(
                alarmType,
                out alarmArcsForAlarmType))
            {
                return null;
            }

            return alarmArcsForAlarmType;
        }

        private AOnOffObject GetBlockAlarmObject(byte? objectType, Guid? objectGuid)
        {
            if (!CgpClientMainForm.Singleton.MainIsConnectionLost(true) && objectType != null && objectGuid != null)
            {
                switch (objectType.Value)
                {
                    case (byte)ObjectType.DailyPlan:
                        return CgpClient.Singleton.MainServerProvider.DailyPlans.GetObjectById(objectGuid.Value);
                    case (byte)ObjectType.TimeZone:
                        return CgpClient.Singleton.MainServerProvider.TimeZones.GetObjectById(objectGuid.Value);
                    case (byte)ObjectType.Input:
                        return Plugin.MainServerProvider.Inputs.GetObjectById(objectGuid.Value);
                    case (byte)ObjectType.Output:
                        return Plugin.MainServerProvider.Outputs.GetObjectById(objectGuid.Value);
                }
            }

            return null;
        }

        private void LoadDoorEnvironmentType()
        {
            _cbDoorEnvironment.Items.Clear();

            byte? doorEnvironmentTypeValue = null;
            if (!Insert)
            {
                doorEnvironmentTypeValue = _editingObject.ActuatorsDoorEnvironment;
            }

            var doorEnvironmentTypes = ItemDoorEnvironmentType.GetList(LocalizationHelper);

            ItemDoorEnvironmentType actDoorEnvironmentType = null;
            foreach (var doorEnvironmentType in doorEnvironmentTypes)
            {
                if (doorEnvironmentType.DoorEnvironmentType == DoorEnviromentType.Standard
                    || doorEnvironmentType.DoorEnvironmentType == DoorEnviromentType.Minimal
                    || doorEnvironmentType.DoorEnvironmentType == DoorEnviromentType.Turnstile)
                {
                    _cbDoorEnvironment.Items.Add(doorEnvironmentType);
                    if (doorEnvironmentTypeValue != null &&
                        doorEnvironmentTypeValue == (byte)doorEnvironmentType.DoorEnvironmentType)
                        actDoorEnvironmentType = doorEnvironmentType;
                }
            }

            _cbDoorEnvironment.SelectedItem = actDoorEnvironmentType;
        }

        private void ChangeState(Guid doorEnvironmentGuid, byte state)
        {
            if (_editingObject.IdDoorEnvironment == doorEnvironmentGuid)
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action<Guid, byte>(ChangeState), doorEnvironmentGuid, state);
                }
                else
                {
                    var doorEnvironmentState = (DoorEnvironmentState)state;
                    switch (doorEnvironmentState)
                    {
                        case DoorEnvironmentState.Locked:
                            {
                                _eActualState.Text = GetString("DoorEnvironmentStateLocked");
                                break;
                            }
                        case DoorEnvironmentState.Locking:
                            {
                                _eActualState.Text = GetString("DoorEnvironmentStateLocking");
                                break;
                            }
                        case DoorEnvironmentState.Opened:
                            {
                                _eActualState.Text = GetString("DoorEnvironmentStateOpened");
                                break;
                            }
                        case DoorEnvironmentState.Unlocked:
                            {
                                _eActualState.Text = GetString("DoorEnvironmentStateUnlocked");
                                break;
                            }
                        case DoorEnvironmentState.Unlocking:
                            {
                                _eActualState.Text = GetString("DoorEnvironmentStateUnlocking");
                                break;
                            }
                        case DoorEnvironmentState.AjarPrewarning:
                            {
                                _eActualState.Text = GetString("DoorEnvironmentStateDoorAjarPrewarning");
                                break;
                            }
                        case DoorEnvironmentState.Ajar:
                            {
                                _eActualState.Text = GetString("DoorEnvironmentStateDoorAjar");
                                break;
                            }
                        case DoorEnvironmentState.Intrusion:
                            {
                                _eActualState.Text = GetString("DoorEnvironmentStateIntrusion");
                                break;
                            }
                        case DoorEnvironmentState.Sabotage:
                            {
                                _eActualState.Text = GetString("DoorEnvironmentStateSatotage");
                                break;
                            }
                        default:
                            {
                                _eActualState.Text = string.Empty;
                                break;
                            }
                    }
                }
            }
        }

        private void CudObjectEvent(ObjectType objectType, object id, bool isInsert)
        {
            try
            {
                if (!(id is Guid))
                    return;

                var idGuid = (Guid)id;

                switch (objectType)
                {
                    case ObjectType.CardReader:
                        if (_crInternal != null && _crInternal.GetObjectType() == ObjectType.CardReader)
                        {
                            var crObjectId = _crInternal.GetId();
                            if (crObjectId is Guid)
                            {
                                if ((Guid)crObjectId == idGuid)
                                {
                                    var cardReader = Plugin.MainServerProvider.CardReaders.GetObjectById(idGuid);

                                    _crInternal = cardReader;
                                    RefreshInternalCardReader();
                                }
                            }
                        }

                        if (_crExternal != null && _crExternal.GetObjectType() == ObjectType.CardReader)
                        {
                            var crObjectId = _crExternal.GetId();
                            if (crObjectId is Guid)
                            {
                                if ((Guid)crObjectId == idGuid)
                                {
                                    var cardReader = Plugin.MainServerProvider.CardReaders.GetObjectById(idGuid);

                                    _crExternal = cardReader;
                                    RefreshExternalCardReader();
                                }
                            }
                        }
                        break;
                    case ObjectType.Input:
                        if (_crInternal != null && _crInternal.GetObjectType() == ObjectType.Input)
                        {
                            var crObjectId = _crInternal.GetId();
                            if (crObjectId is Guid)
                            {
                                if ((Guid)crObjectId == idGuid)
                                {
                                    var input = Plugin.MainServerProvider.Inputs.GetObjectById(idGuid);

                                    _crInternal = input;
                                    RefreshInternalCardReader();
                                }
                            }
                        }

                        if (_crExternal != null && _crExternal.GetObjectType() == ObjectType.Input)
                        {
                            var crObjectId = _crExternal.GetId();
                            if (crObjectId is Guid)
                            {
                                if ((Guid)crObjectId == idGuid)
                                {
                                    var input = Plugin.MainServerProvider.Inputs.GetObjectById(idGuid);

                                    _crExternal = input;
                                    RefreshExternalCardReader();
                                }
                            }
                        }

                        RefreshSensors(idGuid);
                        break;
                    case ObjectType.Output:
                        RefreshSpecialOutputs(idGuid);
                        RefreshActivators(idGuid);
                        break;
                }
            }
            catch
            {
            }
        }

        private void RefreshSensors(Guid inputGuid)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<Guid>(RefreshSensors), inputGuid);
            }
            else
            {
                LockChanges();

                var input = _cbLockDoor.SelectedItem as Input;
                if (input != null && input.IdInput == inputGuid)
                {
                    var newInput = Plugin.MainServerProvider.Inputs.GetObjectById(inputGuid);

                    _cbLockDoor.Items.Add(newInput);
                    _cbLockDoor.SelectedItem = newInput;
                }

                input = _cbOpenDoor.SelectedItem as Input;
                if (input != null && input.IdInput == inputGuid)
                {
                    var newInput = Plugin.MainServerProvider.Inputs.GetObjectById(inputGuid);

                    _cbOpenDoor.Items.Add(newInput);
                    _cbOpenDoor.SelectedItem = newInput;
                }

                input = _cbOpenMax.SelectedItem as Input;
                if (input != null && input.IdInput == inputGuid)
                {
                    var newInput = Plugin.MainServerProvider.Inputs.GetObjectById(inputGuid);

                    _cbOpenMax.Items.Add(newInput);
                    _cbOpenMax.SelectedItem = newInput;
                }

                UnlockChanges();
            }
        }

        private void RefreshActivators(Guid outputGuid)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<Guid>(RefreshActivators), outputGuid);
            }
            else
            {
                LockChanges();

                var output = _cbElectricStrike.SelectedItem as Output;
                if (output != null && output.IdOutput == outputGuid)
                {
                    var newOutput = Plugin.MainServerProvider.Outputs.GetObjectById(outputGuid);

                    //_cbElectricStrike.Items.Add(newOutput);
                    _cbElectricStrike.SelectedItem = newOutput;
                }

                output = _cbElectricStrikeOpposite.SelectedItem as Output;
                if (output != null && output.IdOutput == outputGuid)
                {
                    var newOutput = Plugin.MainServerProvider.Outputs.GetObjectById(outputGuid);

                    _cbElectricStrikeOpposite.Items.Add(newOutput);
                    _cbElectricStrikeOpposite.SelectedItem = newOutput;
                }

                output = _cbExtraElectricStrike.SelectedItem as Output;
                if (output != null && output.IdOutput == outputGuid)
                {
                    var newOutput = Plugin.MainServerProvider.Outputs.GetObjectById(outputGuid);

                    _cbExtraElectricStrike.Items.Add(newOutput);
                    _cbExtraElectricStrike.SelectedItem = newOutput;
                }

                output = _cbExtraElectricStrikeOpposite.SelectedItem as Output;
                if (output != null && output.IdOutput == outputGuid)
                {
                    var newOutput = Plugin.MainServerProvider.Outputs.GetObjectById(outputGuid);

                    _cbExtraElectricStrikeOpposite.Items.Add(newOutput);
                    _cbExtraElectricStrikeOpposite.SelectedItem = newOutput;
                }

                output = _cbBypassAlarm.SelectedItem as Output;
                if (output != null && output.IdOutput == outputGuid)
                {
                    var newOutput = Plugin.MainServerProvider.Outputs.GetObjectById(outputGuid);

                    _cbBypassAlarm.Items.Add(newOutput);
                    _cbBypassAlarm.SelectedItem = newOutput;
                }

                UnlockChanges();
            }
        }

        private void RefreshSpecialOutputs(Guid outputGuid)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<Guid>(RefreshSpecialOutputs), outputGuid);
            }
            else
            {
                LockChanges();

                var output = _cbOutputDoorAjar.SelectedItem as Output;
                if (output != null && output.IdOutput == outputGuid)
                {
                    var newOutput = Plugin.MainServerProvider.Outputs.GetObjectById(outputGuid);

                    _cbOutputDoorAjar.Items.Add(newOutput);
                    _cbOutputDoorAjar.SelectedItem = newOutput;
                }

                output = _cbOutputIntrusion.SelectedItem as Output;
                if (output != null && output.IdOutput == outputGuid)
                {
                    var newOutput = Plugin.MainServerProvider.Outputs.GetObjectById(outputGuid);

                    _cbOutputIntrusion.Items.Add(newOutput);
                    _cbOutputIntrusion.SelectedItem = newOutput;
                }

                output = _cbOutputSabotage.SelectedItem as Output;
                if (output != null && output.IdOutput == outputGuid)
                {
                    var newOutput = Plugin.MainServerProvider.Outputs.GetObjectById(outputGuid);

                    _cbOutputSabotage.Items.Add(newOutput);
                    _cbOutputSabotage.SelectedItem = newOutput;
                }

                UnlockChanges();
            }
        }

        private void RefreshConfigured()
        {
            _bAccessGranted.Enabled = _editingObject.Configured;
            _bUnconfigureDoorEnvironment.Enabled = _editingObject.Configured;
        }

        protected override void DisableForm()
        {
            base.DisableForm();

            _bCancel.Enabled = true;
        }

        protected override bool GetValues()
        {
            try
            {
                _editingObject.Name = _eName.Text;
                _editingObject.Description = _eDescription.Text;
                _editingObject.DoorTimeOpen = (ushort)_eTimeOpen.Value;
                _editingObject.DoorTimeSirenAjar = (ushort)_eTimeSirenAjar.Value;
                _editingObject.DoorTimeUnlock = (byte)_eTimeUnlock.Value;
                _editingObject.DoorTimePreAlarm = (byte)_eTimePreAlarm.Value;

                _editingObject.DoorDelayBeforeUnlock = (ushort)_eDelayBeforeUnlock.Value;
                _editingObject.DoorDelayBeforeLock = (ushort)_eDelayBeforeLock.Value;
                _editingObject.DoorDelayBeforeClose = (ushort)_eDelayBeforeClose.Value;
                _editingObject.DoorDelayBeforeBreakIn = (ushort)_eDelayBeforeBreakIn.Value;
                _editingObject.NotInvokeIntrusionAlarm = _chbNotInvokeIntrusionAlarm.Checked;
                _editingObject.IsVehicleAccess = _chbIsVehicleAccess.Checked;

                var itemDoorEnvironmentType = _cbDoorEnvironment.SelectedItem as ItemDoorEnvironmentType;
                _editingObject.ActuatorsDoorEnvironment =
                    itemDoorEnvironmentType != null
                        ? (byte?)(byte)itemDoorEnvironmentType.DoorEnvironmentType
                        : null;

                _editingObject.ActuatorsElectricStrike = _cbElectricStrike.SelectedItem as Output;
                _editingObject.ActuatorsExtraElectricStrike = _cbExtraElectricStrike.SelectedItem as Output;
                _editingObject.ActuatorsBypassAlarm = _cbBypassAlarm.SelectedItem as Output;
                _editingObject.ActuatorsElectricStrikeOpposite = _cbElectricStrikeOpposite.SelectedItem as Output;
                _editingObject.ActuatorsExtraElectricStrikeOpposite =
                    _cbExtraElectricStrikeOpposite.SelectedItem as Output;

                _editingObject.SensorsLockDoors = _cbLockDoor.SelectedItem as Input;
                _editingObject.SensorsOpenDoors = _cbOpenDoor.SelectedItem as Input;
                _editingObject.SensorsOpenMaxDoors = _cbOpenMax.SelectedItem as Input;
                _editingObject.DoorAjarOutput = _cbOutputDoorAjar.SelectedItem as Output;
                _editingObject.IntrusionOutput = _cbOutputIntrusion.SelectedItem as Output;
                _editingObject.SabotageOutput = _cbOutputSabotage.SelectedItem as Output;

                _editingObject.ActuatorsElectricStrikeImpulse = _chbElectricStrikeImpulse.Checked;
                _editingObject.ActuatorsExtraElectricStrikeImpulse = _chbExtraElectricStrikeImpulse.Checked;
                _editingObject.ActuatorsElectricStrikeImpulseDelay = (int)_eElectricStrikeImpulseDelay.Value;
                _editingObject.ActuatorsExtraElectricStrikeImpulseDelay = (int)_eExtraElectricStrikeImpulseDelay.Value;

                _editingObject.ActuatorsElectricStrikeOppositeImpulse = _chbElectricStrikeOppositeImpulse.Checked;
                _editingObject.ActuatorsExtraElectricStrikeOppositeImpulse =
                    _chbExtraElectricStrikeOppositeImpulse.Checked;
                _editingObject.ActuatorsElectricStrikeOppositeImpulseDelay =
                    (int)_eElectricStrikeOppositeImpulseDelay.Value;
                _editingObject.ActuatorsExtraElectricStrikeOppositeImpulseDelay =
                    (int)_eExtraElectricStrikeOppositeImpulseDelay.Value;


                _editingObject.SensorsLockDoorsInverted = _chbLockDoorInverted.Checked;
                _editingObject.SensorsOpenDoorsInverted = _chbOpenDoorInverted.Checked;
                _editingObject.SensorsOpenMaxDoorsInverted = _chbOpenMaxDoorInverted.Checked;
                _editingObject.SensorsLockDoorsBalanced = _chbLockDoorBalanced.Checked;
                _editingObject.SensorsOpenDoorsBalanced = _chbOpenDoorBalanced.Checked;
                _editingObject.SensorsOpenMaxDoorsBalanced = _chbOpenMaxDoorBalanced.Checked;

                if (_crInternal != null)
                {
                    if (_crInternal is CardReader)
                    {
                        _editingObject.CardReaderInternal = _crInternal as CardReader;
                        _editingObject.PushButtonInternal = null;
                    }
                    else
                    {
                        _editingObject.CardReaderInternal = null;
                        _editingObject.PushButtonInternal = _crInternal as Input;
                    }
                }
                else
                {
                    _editingObject.CardReaderInternal = null;
                    _editingObject.PushButtonInternal = null;
                }


                if (_crExternal != null)
                {
                    if (_crExternal is CardReader)
                    {
                        _editingObject.CardReaderExternal = _crExternal as CardReader;
                        _editingObject.PushButtonExternal = null;
                    }
                    else
                    {
                        _editingObject.CardReaderExternal = null;
                        _editingObject.PushButtonExternal = _crExternal as Input;
                    }
                }
                else
                {
                    _editingObject.CardReaderExternal = null;
                    _editingObject.PushButtonExternal = null;
                }

                //Door ajar
                _editingObject.DoorAjarAlarmOn = _catsDsmDoorAjar.AlarmEnabledCheckState;
                _editingObject.BlockAlarmDoorAjar = _catsDsmDoorAjar.BlockAlarmCheckState;

                if (_catsDsmDoorAjar.ObjectForBlockingAlarm != null)
                {
                    _editingObject.ObjBlockAlarmDoorAjarObjectType =
                        (byte)_catsDsmDoorAjar.ObjectForBlockingAlarm.GetObjectType();

                    _editingObject.ObjBlockAlarmDoorAjarId =
                        (Guid)_catsDsmDoorAjar.ObjectForBlockingAlarm.GetId();
                }
                else
                {
                    _editingObject.ObjBlockAlarmDoorAjarObjectType = null;
                    _editingObject.ObjBlockAlarmDoorAjarId = null;
                }

                _editingObject.DoorAjarPresentationGroup = _catsDsmDoorAjar.PresentationGroup;

                if (_editingObject.DoorEnvironmentAlarmArcs != null)
                    _editingObject.DoorEnvironmentAlarmArcs.Clear();
                else
                    _editingObject.DoorEnvironmentAlarmArcs = new List<DoorEnvironmentAlarmArc>();

                SaveAlarmArcs(
                    _cmaaDsmDoorAjar.AlarmArcs,
                    AlarmType.DoorEnvironment_DoorAjar);

                //Intrusion
                _editingObject.IntrusionAlarmOn = _catsDsmIntrusion.AlarmEnabledCheckState;
                _editingObject.BlockAlarmIntrusion = _catsDsmIntrusion.BlockAlarmCheckState;

                if (_catsDsmIntrusion.ObjectForBlockingAlarm != null)
                {
                    _editingObject.ObjBlockAlarmIntrusionObjectType =
                        (byte)_catsDsmIntrusion.ObjectForBlockingAlarm.GetObjectType();

                    _editingObject.ObjBlockAlarmIntrusionId =
                        (Guid)_catsDsmIntrusion.ObjectForBlockingAlarm.GetId();
                }
                else
                {
                    _editingObject.ObjBlockAlarmIntrusionObjectType = null;
                    _editingObject.ObjBlockAlarmIntrusionId = null;
                }

                _editingObject.IntrusionPresentationGroup = _catsDsmIntrusion.PresentationGroup;

                SaveAlarmArcs(
                    _cmaaDsmIntrusion.AlarmArcs,
                    AlarmType.DoorEnvironment_Intrusion);

                //Sabotage
                _editingObject.SabotageAlarmOn = _catsDsmSabotage.AlarmEnabledCheckState;
                _editingObject.BlockAlarmSabotage = _catsDsmSabotage.BlockAlarmCheckState;

                if (_catsDsmSabotage.ObjectForBlockingAlarm != null)
                {
                    _editingObject.ObjBlockAlarmSabotageObjectType =
                        (byte)_catsDsmSabotage.ObjectForBlockingAlarm.GetObjectType();

                    _editingObject.ObjBlockAlarmSabotageId =
                        (Guid)_catsDsmSabotage.ObjectForBlockingAlarm.GetId();
                }
                else
                {
                    _editingObject.ObjBlockAlarmSabotageObjectType = null;
                    _editingObject.ObjBlockAlarmSabotageId = null;
                }

                _editingObject.SabotagePresentationGroup = _catsDsmSabotage.PresentationGroup;

                SaveAlarmArcs(
                    _cmaaDsmSabotage.AlarmArcs,
                    AlarmType.DoorEnvironment_Sabotage);

                _editingObject.LocalAlarmInstruction = GetNewLocalAlarmInstruction();

                return true;
            }
            catch
            {
                Dialog.Error(GetString("ErrorGetValuesFailed"));
                return false;
            }
        }

        private void SaveAlarmArcs(
            IEnumerable<AlarmArc> alarmArcs,
            AlarmType alarmType)
        {
            if (alarmArcs != null)
            {
                foreach (var alarmArc in alarmArcs)
                {
                    _editingObject.DoorEnvironmentAlarmArcs.Add(
                        new DoorEnvironmentAlarmArc(
                            _editingObject,
                            alarmArc,
                            alarmType));
                }
            }
        }

        protected override bool CheckValues()
        {
            if (string.IsNullOrEmpty(_eName.Text))
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne, _eName,
                    GetString("ErrorInsertName"), CgpClient.Singleton.ClientControlNotificationSettings);
                _eName.Focus();
                return false;
            }

            if (IsDoorEnvironemntConfigured())
            {
                var itemDoorEnvironmentType = _cbDoorEnvironment.SelectedItem as ItemDoorEnvironmentType;
                if (itemDoorEnvironmentType != null &&
                    itemDoorEnvironmentType.DoorEnvironmentType != DoorEnviromentType.Rotating)
                {
                    if (_cbOpenDoor.Enabled)
                    {
                        var openDoor = _cbOpenDoor.SelectedItem as Input;
                        if (openDoor == null)
                        {
                            if (_tcDoorsAutomat.SelectedTab != _tpApas)
                                _tcDoorsAutomat.SelectedTab = _tpApas;

                            ControlNotification.Singleton.Error(NotificationPriority.JustOne, _cbOpenDoor,
                                GetString("ErrorInsertSensorOpenDoor"), ControlNotificationSettings.Default);
                            _cbOpenDoor.Focus();
                            return false;
                        }
                    }

                    if (_cbLockDoor.Enabled)
                    {
                        var lockDoor = _cbLockDoor.SelectedItem as Input;
                        if (lockDoor == null)
                        {
                            if (_tcDoorsAutomat.SelectedTab != _tpApas)
                                _tcDoorsAutomat.SelectedTab = _tpApas;

                            ControlNotification.Singleton.Error(NotificationPriority.JustOne, _cbLockDoor,
                                GetString("ErrorInsertSensorLockDoor"), ControlNotificationSettings.Default);
                            _cbLockDoor.Focus();
                            return false;
                        }
                    }

                    if (_cbOpenMax.Enabled)
                    {
                        var openMaxDoor = _cbOpenMax.SelectedItem as Input;
                        if (openMaxDoor == null)
                        {
                            if (_tcDoorsAutomat.SelectedTab != _tpApas)
                                _tcDoorsAutomat.SelectedTab = _tpApas;

                            ControlNotification.Singleton.Error(NotificationPriority.JustOne, _cbOpenMax,
                                GetString("ErrorInsertSensorOpenMaxDoor"), ControlNotificationSettings.Default);
                            _cbOpenMax.Focus();
                            return false;
                        }
                    }

                    if (_cbElectricStrikeOpposite.Enabled)
                    {
                        var electricStrikeOpposite = _cbElectricStrikeOpposite.SelectedItem as Output;
                        if (electricStrikeOpposite == null)
                        {
                            if (_tcDoorsAutomat.SelectedTab != _tpApas)
                                _tcDoorsAutomat.SelectedTab = _tpApas;

                            ControlNotification.Singleton.Error(
                                NotificationPriority.JustOne,
                                _cbElectricStrikeOpposite,
                                GetString("ErrorInsertActuatorsElectricStrikeOpposite"),
                                ControlNotificationSettings.Default);

                            _cbElectricStrikeOpposite.Focus();
                            return false;
                        }

                    }

                    var electricStrike = _cbElectricStrike.SelectedItem as Output;
                    if (electricStrike == null)
                    {
                        if (_tcDoorsAutomat.SelectedTab != _tpApas)
                            _tcDoorsAutomat.SelectedTab = _tpApas;

                        ControlNotification.Singleton.Error(NotificationPriority.JustOne, _cbElectricStrike,
                            GetString("ErrorInsertActuatorsElectricStrike"), ControlNotificationSettings.Default);
                        _cbElectricStrike.Focus();
                        return false;
                    }

                    if (IsUsedInAnothertDoorEnvironments())
                        return false;
                }
            }

            if (!_catsDsmDoorAjar.ControlValues(
                control =>
                    _openAndScrollToControlByAlarmType[AlarmType.DoorEnvironment_DoorAjar]()))
            {
                return false;
            }

            if (!_catsDsmIntrusion.ControlValues(
                control =>
                    _openAndScrollToControlByAlarmType[AlarmType.DoorEnvironment_Intrusion]()))
            {
                return false;
            }

            if (!_catsDsmSabotage.ControlValues(
                control =>
                    _openAndScrollToControlByAlarmType[AlarmType.DoorEnvironment_Sabotage]()))
            {
                return false;
            }

            return true;
        }

        private bool IsUsedInAnothertDoorEnvironments()
        {
            var checkOutput = _cbElectricStrike.SelectedItem as Output;
            if (checkOutput != null &&
                Plugin.MainServerProvider.DoorEnvironments.UsedActuatorInAnotherDoorEnvironment(checkOutput.IdOutput,
                    _editingObject.IdDoorEnvironment))
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne, _cbElectricStrike,
                    GetString("ErrorUsedOutput"), ControlNotificationSettings.Default);
                _tcDoorsAutomat.SelectedTab = _tpApas;
                _cbElectricStrike.Focus();
                return true;
            }
            checkOutput = _cbElectricStrikeOpposite.SelectedItem as Output;
            if (checkOutput != null &&
                Plugin.MainServerProvider.DoorEnvironments.UsedActuatorInAnotherDoorEnvironment(checkOutput.IdOutput,
                    _editingObject.IdDoorEnvironment))
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne, _cbElectricStrikeOpposite,
                    GetString("ErrorUsedOutput"), ControlNotificationSettings.Default);
                _tcDoorsAutomat.SelectedTab = _tpApas;
                _cbElectricStrikeOpposite.Focus();
                return true;
            }
            checkOutput = _cbExtraElectricStrike.SelectedItem as Output;
            if (checkOutput != null &&
                Plugin.MainServerProvider.DoorEnvironments.UsedActuatorInAnotherDoorEnvironment(checkOutput.IdOutput,
                    _editingObject.IdDoorEnvironment))
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne, _cbExtraElectricStrike,
                    GetString("ErrorUsedOutput"), ControlNotificationSettings.Default);
                _tcDoorsAutomat.SelectedTab = _tpApas;
                _cbExtraElectricStrike.Focus();
                return true;
            }
            checkOutput = _cbExtraElectricStrikeOpposite.SelectedItem as Output;
            if (checkOutput != null &&
                Plugin.MainServerProvider.DoorEnvironments.UsedActuatorInAnotherDoorEnvironment(checkOutput.IdOutput,
                    _editingObject.IdDoorEnvironment))
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne, _cbExtraElectricStrikeOpposite,
                    GetString("ErrorUsedOutput"), ControlNotificationSettings.Default);
                _tcDoorsAutomat.SelectedTab = _tpApas;
                _cbExtraElectricStrikeOpposite.Focus();
                return true;
            }
            checkOutput = _cbBypassAlarm.SelectedItem as Output;
            if (checkOutput != null &&
                Plugin.MainServerProvider.DoorEnvironments.UsedActuatorInAnotherDoorEnvironment(checkOutput.IdOutput,
                    _editingObject.IdDoorEnvironment))
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne, _cbBypassAlarm,
                    GetString("ErrorUsedOutput"), ControlNotificationSettings.Default);
                _tcDoorsAutomat.SelectedTab = _tpApas;
                _cbBypassAlarm.Focus();
                return true;
            }

            var chceckInput = _cbOpenDoor.SelectedItem as Input;
            if (chceckInput != null &&
                Plugin.MainServerProvider.DoorEnvironments.UsedSensorInAnotherDoorEnvironment(chceckInput.IdInput,
                    _editingObject.IdDoorEnvironment))
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne, _cbOpenDoor,
                    GetString("ErrorUsedInput"), ControlNotificationSettings.Default);
                _tcDoorsAutomat.SelectedTab = _tpApas;
                _cbOpenDoor.Focus();
                return true;
            }
            chceckInput = _cbLockDoor.SelectedItem as Input;
            if (chceckInput != null &&
                Plugin.MainServerProvider.DoorEnvironments.UsedSensorInAnotherDoorEnvironment(chceckInput.IdInput,
                    _editingObject.IdDoorEnvironment))
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne, _cbLockDoor,
                    GetString("ErrorUsedInput"), ControlNotificationSettings.Default);
                _tcDoorsAutomat.SelectedTab = _tpApas;
                _cbLockDoor.Focus();
                return true;
            }
            chceckInput = _cbOpenMax.SelectedItem as Input;
            if (chceckInput != null &&
                Plugin.MainServerProvider.DoorEnvironments.UsedSensorInAnotherDoorEnvironment(chceckInput.IdInput,
                    _editingObject.IdDoorEnvironment))
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne, _cbOpenMax,
                    GetString("ErrorUsedInput"), ControlNotificationSettings.Default);
                _tcDoorsAutomat.SelectedTab = _tpApas;
                _cbOpenMax.Focus();
                return true;
            }

            if (_crInternal != null)
            {
                chceckInput = _crInternal as Input;
                if (chceckInput != null &&
                    Plugin.MainServerProvider.DoorEnvironments.UsedPushButtonInAnotherDoorEnvironment(
                        chceckInput.IdInput, _editingObject.IdDoorEnvironment))
                {
                    ControlNotification.Singleton.Error(NotificationPriority.JustOne, _tbmInternalCardReader,
                        GetString("ErrorUsedInput"), ControlNotificationSettings.Default);
                    _tcDoorsAutomat.SelectedTab = _tpApas;
                    _tbmInternalCardReader.Focus();
                    return true;
                }

                var checkCardReader = _crInternal as CardReader;
                if (checkCardReader != null &&
                    Plugin.MainServerProvider.DoorEnvironments.UsedCardReaderInAnotherDoorEnvironment(
                        checkCardReader.IdCardReader, _editingObject.IdDoorEnvironment))
                {
                    ControlNotification.Singleton.Error(NotificationPriority.JustOne, _tbmInternalCardReader,
                        GetString("ErrorUsedCardReader"), ControlNotificationSettings.Default);
                    _tcDoorsAutomat.SelectedTab = _tpApas;
                    _tbmInternalCardReader.Focus();
                    return true;
                }
            }

            if (_crExternal != null)
            {
                chceckInput = _crExternal as Input;
                if (chceckInput != null &&
                    Plugin.MainServerProvider.DoorEnvironments.UsedPushButtonInAnotherDoorEnvironment(
                        chceckInput.IdInput, _editingObject.IdDoorEnvironment))
                {
                    ControlNotification.Singleton.Error(NotificationPriority.JustOne, _tbmExternalCardReader,
                        GetString("ErrorUsedInput"), ControlNotificationSettings.Default);
                    _tcDoorsAutomat.SelectedTab = _tpApas;
                    _tbmExternalCardReader.Focus();
                    return true;
                }

                var checkCardReader = _crExternal as CardReader;
                if (checkCardReader != null &&
                    Plugin.MainServerProvider.DoorEnvironments.UsedCardReaderInAnotherDoorEnvironment(
                        checkCardReader.IdCardReader, _editingObject.IdDoorEnvironment))
                {
                    ControlNotification.Singleton.Error(NotificationPriority.JustOne, _tbmExternalCardReader,
                        GetString("ErrorUsedCardReader"), ControlNotificationSettings.Default);
                    _tcDoorsAutomat.SelectedTab = _tpApas;
                    _tbmExternalCardReader.Focus();
                    return true;
                }
            }



            checkOutput = _cbOutputDoorAjar.SelectedItem as Output;
            if (checkOutput != null &&
                Plugin.MainServerProvider.DoorEnvironments.UsedSensorsInAnotherDoorEnvironment(checkOutput.IdOutput,
                    _editingObject.IdDoorEnvironment))
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne, _cbOutputDoorAjar,
                    GetString("ErrorUsedOutput"), ControlNotificationSettings.Default);
                _tcDoorsAutomat.SelectedTab = _tpSpecialOutputs;
                _cbOutputDoorAjar.Focus();
                return true;
            }

            checkOutput = _cbOutputIntrusion.SelectedItem as Output;
            if (checkOutput != null &&
                Plugin.MainServerProvider.DoorEnvironments.UsedSensorsInAnotherDoorEnvironment(checkOutput.IdOutput,
                    _editingObject.IdDoorEnvironment))
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne, _cbOutputIntrusion,
                    GetString("ErrorUsedOutput"), ControlNotificationSettings.Default);
                _tcDoorsAutomat.SelectedTab = _tpSpecialOutputs;
                _cbOutputIntrusion.Focus();
                return true;
            }

            checkOutput = _cbOutputSabotage.SelectedItem as Output;
            if (checkOutput != null &&
                Plugin.MainServerProvider.DoorEnvironments.UsedSensorsInAnotherDoorEnvironment(checkOutput.IdOutput,
                    _editingObject.IdDoorEnvironment))
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne, _cbOutputSabotage,
                    GetString("ErrorUsedOutput"), ControlNotificationSettings.Default);
                _tcDoorsAutomat.SelectedTab = _tpSpecialOutputs;
                _cbOutputSabotage.Focus();
                return true;
            }

            return false;
        }

        private bool IsDoorEnvironemntConfigured()
        {
            var itemDoorEnvironmentType = _cbDoorEnvironment.SelectedItem as ItemDoorEnvironmentType;
            if (itemDoorEnvironmentType != null)
                return true;

            return false;
        }

        private void CancelClick(object sender, EventArgs e)
        {
            Cancel_Click();
        }

        private void OkClick(object sender, EventArgs e)
        {
            Ok_Click();
        }

        protected override bool SaveToDatabaseInsert()
        {
            throw new NotImplementedException();
        }

        protected override bool SaveToDatabaseEdit()
        {
            return SaveToDatabaseEditCore(false);
        }

        protected override bool SaveToDatabaseEditOnlyInDatabase()
        {
            return SaveToDatabaseEditCore(true);
        }

        private bool SaveToDatabaseEditCore(bool OnlyInDatabase)
        {
            try
            {
                SaveSensorsAndAuctuatorsAndPushButtons();
            }
            catch
            {
                return false;
            }

            Exception error;
            bool retValue;

            if (OnlyInDatabase)
                retValue = Plugin.MainServerProvider.DoorEnvironments.UpdateOnlyInDatabase(_editingObject, out error);
            else
                retValue = Plugin.MainServerProvider.DoorEnvironments.Update(_editingObject, out error);

            if (!retValue && error != null)
            {
                if (error is SqlUniqueException && error.Message == DoorEnvironment.COLUMNNAME)
                {
                    ControlNotification.Singleton.Error(NotificationPriority.JustOne, _eName,
                        GetString("ErrorUsedName"), CgpClient.Singleton.ClientControlNotificationSettings);
                    _eName.Focus();
                }
                else
                    throw error;
            }
            else
                UnsetPreviouslyUsedActuatorsAndPushButtons();

            return retValue;
        }

        /// <summary>
        /// Unsets outputs previously used as actuators or push buttons
        /// </summary>
        private void UnsetPreviouslyUsedActuatorsAndPushButtons()
        {
            if (CgpClient.Singleton.IsConnectionLost(true))
                return;

            try
            {
                ICollection<Output> outputs = null;
                if (_editingObject.DCU != null)
                    outputs = _editingObject.DCU.Outputs;
                else if (_editingObject.CCU != null)
                    outputs = _editingObject.CCU.Outputs;

                foreach (var output in outputs)
                {
                    if ((OutputControl)output.ControlType == OutputControl.controledByDoorEnvironment &&
                        !Plugin.MainServerProvider.DoorEnvironments.IsOutputInDoorEnvironmentsActuators(output))
                    {
                        Exception ex;
                        var outputForEdit = Plugin.MainServerProvider.Outputs.GetObjectForEdit(output.IdOutput, out ex);
                        if (ex == null && outputForEdit != null)
                        {
                            outputForEdit.ControlType = (byte)OutputControl.unblocked;
                            Plugin.MainServerProvider.Outputs.Update(outputForEdit, out ex);
                            Plugin.MainServerProvider.Outputs.EditEnd(outputForEdit);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void SetActuator(
            Output output,
            bool impulse,
            int impulseDelay,
            int delayToOn,
            int delayToOff)
        {
            if (output == null)
                return;

            Exception error;

            var outputForEdit = Plugin.MainServerProvider.Outputs.GetObjectForEdit(output.IdOutput, out error);

            if (outputForEdit == null)
                ThrowMyException(error);

            var wasChanged = false;
            if (outputForEdit.ControlType != (byte)OutputControl.controledByDoorEnvironment)
            {
                outputForEdit.ControlType = (byte)OutputControl.controledByDoorEnvironment;
                wasChanged = true;
            }

            if (impulse)
            {
                if (outputForEdit.OutputType != (byte)OutputCharacteristic.pulsed)
                {
                    outputForEdit.OutputType = (byte)OutputCharacteristic.pulsed;
                    wasChanged = true;
                }

                if (outputForEdit.SettingsPulseLength != impulseDelay)
                {
                    outputForEdit.SettingsPulseLength = impulseDelay;
                    wasChanged = true;
                }
            }
            else
            {
                if (outputForEdit.OutputType != (byte)OutputCharacteristic.level)
                {
                    outputForEdit.OutputType = (byte)OutputCharacteristic.level;
                    wasChanged = true;
                }
            }

            if (outputForEdit.SettingsDelayToOn != delayToOn)
            {
                outputForEdit.SettingsDelayToOn = delayToOn;
                wasChanged = true;
            }

            if (outputForEdit.SettingsDelayToOff != delayToOff)
            {
                outputForEdit.SettingsDelayToOff = delayToOff;
                wasChanged = true;
            }

            if (outputForEdit.Inverted)
            {
                outputForEdit.Inverted = false;
                wasChanged = true;
            }

            bool success = true;

            if (wasChanged)
                success = Plugin.MainServerProvider.Outputs.Update(outputForEdit, out error);

            Plugin.MainServerProvider.Outputs.EditEnd(outputForEdit);

            if (!success)
                ThrowMyException(error);
        }

        private void SetSensorAndPushButton(
            Input input,
            bool balanced,
            bool inverted,
            int delayToOn,
            int delayToOff)
        {
            if (input == null)
                return;

            Exception error;

            var inputForEdit = Plugin.MainServerProvider.Inputs.GetObjectForEdit(input.IdInput, out error);

            if (inputForEdit == null)
                ThrowMyException(error);

            var wasChanged = false;

            if (balanced)
            {
                if (inputForEdit.InputType != (byte)InputType.BSI)
                {
                    inputForEdit.InputType = (byte)InputType.BSI;
                    wasChanged = true;
                }
            }
            else
            {
                if (inputForEdit.InputType != (byte)InputType.DI)
                {
                    inputForEdit.InputType = (byte)InputType.DI;
                    wasChanged = true;
                }
            }

            if (inputForEdit.Inverted != inverted)
            {
                inputForEdit.Inverted = inverted;
                wasChanged = true;
            }

            if (inputForEdit.DelayToOn != delayToOn)
            {
                inputForEdit.DelayToOn = delayToOn;
                wasChanged = true;
            }

            if (inputForEdit.DelayToOff != delayToOff)
            {
                inputForEdit.DelayToOff = delayToOff;
                wasChanged = true;
            }

            if (inputForEdit.AlarmOn)
            {
                inputForEdit.AlarmOn = false;
                wasChanged = true;
            }

            if (inputForEdit.AlarmTamper)
            {
                inputForEdit.AlarmTamper = false;
                wasChanged = true;
            }

            bool success = true;

            if (wasChanged)
                success = Plugin.MainServerProvider.Inputs.Update(inputForEdit, out error);

            Plugin.MainServerProvider.Inputs.EditEnd(inputForEdit);

            if (!success)
                ThrowMyException(error);
        }

        public void SaveSensorsAndAuctuatorsAndPushButtons()
        {
            SetActuator(
                _editingObject.ActuatorsElectricStrike,
                _editingObject.ActuatorsElectricStrikeImpulse,
                _editingObject.ActuatorsElectricStrikeImpulseDelay,
                _editingObject.DoorDelayBeforeUnlock,
                 _editingObject.DoorDelayBeforeLock);

            SetActuator(
                _editingObject.ActuatorsExtraElectricStrike,
                _editingObject.ActuatorsExtraElectricStrikeImpulse,
                _editingObject.ActuatorsExtraElectricStrikeImpulseDelay,
                _editingObject.DoorDelayBeforeUnlock,
                _editingObject.DoorDelayBeforeLock);

            SetActuator(
                _editingObject.ActuatorsBypassAlarm,
                false,
                0,
                0,
                0);

            SetActuator(
                _editingObject.ActuatorsElectricStrikeOpposite,
                _editingObject.ActuatorsElectricStrikeOppositeImpulse,
                _editingObject.ActuatorsElectricStrikeOppositeImpulseDelay,
                0,
                0);

            SetActuator(
                _editingObject.ActuatorsExtraElectricStrikeOpposite,
                _editingObject.ActuatorsExtraElectricStrikeOppositeImpulse,
                _editingObject.ActuatorsExtraElectricStrikeOppositeImpulseDelay,
                0,
                0);

            SetSensorAndPushButton(
                _editingObject.SensorsOpenDoors,
                _editingObject.SensorsOpenDoorsBalanced,
                _editingObject.SensorsOpenDoorsInverted,
                _editingObject.DoorDelayBeforeBreakIn,
                _editingObject.DoorDelayBeforeClose
                );

            SetSensorAndPushButton(
               _editingObject.SensorsOpenMaxDoors,
               _editingObject.SensorsOpenMaxDoorsBalanced,
               _editingObject.SensorsOpenMaxDoorsInverted,
               0,
               0
               );

            SetSensorAndPushButton(
                _editingObject.SensorsLockDoors,
                _editingObject.SensorsLockDoorsBalanced,
                _editingObject.SensorsLockDoorsInverted,
                0,
                0);

            SetSensorAndPushButton(
                _editingObject.PushButtonInternal,
                _chbPushButtonInternalBalanced.Checked,
                _chbPushButtonInternalInverted.Checked,
                50,
                50);

            SetSensorAndPushButton(
                _editingObject.PushButtonExternal,
                _chbPushButtonExternalBalanced.Checked,
                _chbPushButtonExternalInverted.Checked,
                50,
                50);
        }

        private void ThrowMyException(Exception error)
        {
            if (error != null)
                throw (error);
            throw (new Exception());
        }

        protected override void EditTextChanger(object sender, EventArgs e)
        {
            base.EditTextChanger(sender, e);

            if (sender == _eTimeOpen)
            {
                _eTimePreAlarm.Maximum =
                    _eTimeOpen.Value > 0
                        ? _eTimeOpen.Value - 1
                        : 0;
            }

            if (!Insert && ValueChanged)
                _bApply.Enabled = true;
        }

        protected override void EditTextChangerOnlyInDatabase(object sender, EventArgs e)
        {
            base.EditTextChangerOnlyInDatabase(sender, e);

            if (!Insert && ValueChangedOnlyDatabase)
                _bApply.Enabled = true;
        }

        protected override void EditEnd()
        {
            if (CgpClient.Singleton.IsConnectionLost(false)) return;
            Plugin.MainServerProvider.DoorEnvironments.EditEnd(_editingObject);
        }

        private void _chbElectricStrikeImpulse_CheckedChanged(object sender, EventArgs e)
        {
            EditTextChanger(sender, e);
            _eElectricStrikeImpulseDelay.Enabled = _chbElectricStrikeImpulse.Checked;
        }

        private void _chbExtraElectricStrikeImpulse_CheckedChanged(object sender, EventArgs e)
        {
            EditTextChanger(sender, e);
            _eExtraElectricStrikeImpulseDelay.Enabled = _chbExtraElectricStrikeImpulse.Checked;
        }

        private void SensorsDropDown(object sender, EventArgs e)
        {
            if (CgpClient.Singleton.IsConnectionLost(false)) return;
            var cb = sender as ComboBox;

            if (cb == null) return;

            var selectedItem = cb.SelectedItem as Input;
            cb.Items.Clear();
            var inputs =
                Plugin.MainServerProvider.DoorEnvironments.GetInputsNotInDoorEnvironments(
                    _editingObject.IdDoorEnvironment);
            inputs = inputs.OrderBy(input => input.InputNumber).ToList();

            cb.Items.Add(string.Empty);
            foreach (var input in inputs)
            {
                AddSensorsToComboBox(input, cb, selectedItem);
            }
        }

        private void AddSensorsToComboBox(Input input, ComboBox cb, Input selectedItem)
        {
            if (input.Compare(_cbLockDoor.SelectedItem as Input))
            {
                return;
            }

            if (input.Compare(_cbOpenDoor.SelectedItem as Input))
            {
                return;
            }

            if (input.Compare(_cbOpenMax.SelectedItem as Input))
            {
                return;
            }

            if (input.Compare(_crInternal as Input))
            {
                return;
            }

            if (input.Compare(_crExternal as Input))
            {
                return;
            }

            cb.Items.Add(input);

            if (input.Compare(selectedItem))
            {
                cb.SelectedItem = input;
            }
        }

        private void ActuatorsDropDown(object sender, EventArgs e)
        {
            if (CgpClient.Singleton.IsConnectionLost(false)) return;
            var cb = sender as ComboBox;

            if (cb == null) return;

            var selectedItem = cb.SelectedItem as Output;
            cb.Items.Clear();
            var outputs = Plugin.MainServerProvider.DoorEnvironments.GetNotUsedOutputs(_editingObject.IdDoorEnvironment);
            outputs = outputs.OrderBy(output => output.OutputNumber).ToList();

            cb.Items.Add(string.Empty);
            foreach (var output in outputs)
            {
                if ((_editingObject.CCU == null || _editingObject.CCU.WatchdogOutput == null) ||
                    (_editingObject.CCU.WatchdogOutput.IdOutput != output.IdOutput))
                    AddActuatorsToComboBox(output, cb, selectedItem);
            }
        }

        private void AddActuatorsToComboBox(Output output, ComboBox cb, Output selectedItem)
        {
            if (output.Compare(_cbElectricStrike.SelectedItem as Output))
            {
                return;
            }

            if (output.Compare(_cbElectricStrikeOpposite.SelectedItem as Output))
            {
                return;
            }

            if (output.Compare(_cbExtraElectricStrike.SelectedItem as Output))
            {
                return;
            }

            if (output.Compare(_cbExtraElectricStrikeOpposite.SelectedItem as Output))
            {
                return;
            }

            if (output.Compare(_cbBypassAlarm.SelectedItem as Output))
            {
                return;
            }

            if (output.Compare(_cbOutputDoorAjar.SelectedItem as Output))
            {
                return;
            }

            if (output.Compare(_cbOutputIntrusion.SelectedItem as Output))
            {
                return;
            }

            if (output.Compare(_cbOutputSabotage.SelectedItem as Output))
            {
                return;
            }

            cb.Items.Add(output);

            if (output.Compare(selectedItem))
            {
                cb.SelectedItem = output;
            }
        }

        private void _cbLockDoor_SelectedIndexChanged(object sender, EventArgs e)
        {
            EditTextChanger(sender, e);

            if (_cbLockDoor.SelectedItem is Input)
            {
                _chbLockDoorInverted.Enabled = true;
                _chbLockDoorBalanced.Enabled = true;
            }
            else
            {
                _chbLockDoorInverted.Enabled = false;
                _chbLockDoorBalanced.Enabled = false;

                _chbLockDoorInverted.Checked = false;
                _chbLockDoorBalanced.Checked = false;
            }
        }

        private void _cbOpenDoor_SelectedIndexChanged(object sender, EventArgs e)
        {
            EditTextChanger(sender, e);

            if (_cbOpenDoor.SelectedItem is Input)
            {
                _chbOpenDoorInverted.Enabled = true;
                _chbOpenDoorBalanced.Enabled = true;
            }
            else
            {
                _chbOpenDoorInverted.Enabled = false;
                _chbOpenDoorBalanced.Enabled = false;

                _chbOpenDoorInverted.Checked = false;
                _chbOpenDoorBalanced.Checked = false;
            }
        }

        private void _cbOpenMax_SelectedIndexChanged(object sender, EventArgs e)
        {
            EditTextChanger(sender, e);

            if (_cbOpenMax.SelectedItem is Input)
            {
                _chbOpenMaxDoorBalanced.Enabled = true;
                _chbOpenMaxDoorInverted.Enabled = true;
            }
            else
            {
                _chbOpenMaxDoorBalanced.Enabled = false;
                _chbOpenMaxDoorInverted.Enabled = false;

                _chbOpenMaxDoorBalanced.Checked = false;
                _chbOpenMaxDoorInverted.Checked = false;
            }
        }

        private void _cbDoorEnvironment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(_cbDoorEnvironment.SelectedItem is ItemDoorEnvironmentType))
            {
                _cbElectricStrikeOpposite.SelectedItem = null;
                _cbElectricStrikeOpposite.Enabled = false;
                _cbExtraElectricStrikeOpposite.SelectedItem = null;
                _cbExtraElectricStrikeOpposite.Enabled = false;
                _cbOpenDoor.SelectedItem = null;
                _cbOpenDoor.Enabled = false;
                _cbLockDoor.SelectedItem = null;
                _cbLockDoor.Enabled = false;
                _cbOpenMax.SelectedItem = null;
                _cbOpenMax.Enabled = false;
                _cbBypassAlarm.SelectedItem = null;
                _cbBypassAlarm.Enabled = false;
                _cbElectricStrike.SelectedItem = null;
                _cbElectricStrike.Enabled = false;
                _cbExtraElectricStrike.SelectedItem = null;
                _cbExtraElectricStrike.Enabled = false;

                _tbmInternalCardReader.Enabled = false;
                _tbmExternalCardReader.Enabled = false;
                return;
            }

            _cbBypassAlarm.Enabled = true;
            _cbElectricStrike.Enabled = true;
            _cbExtraElectricStrike.Enabled = true;

            _tbmInternalCardReader.Enabled = true;
            _tbmExternalCardReader.Enabled = true;

            switch ((_cbDoorEnvironment.SelectedItem as ItemDoorEnvironmentType).DoorEnvironmentType)
            {
                case DoorEnviromentType.Standard:
                    _cbElectricStrikeOpposite.SelectedItem = null;
                    _cbElectricStrikeOpposite.Enabled = false;
                    _cbExtraElectricStrikeOpposite.SelectedItem = null;
                    _cbExtraElectricStrikeOpposite.Enabled = false;
                    _cbOpenDoor.Enabled = true;
                    _cbLockDoor.SelectedItem = null;
                    _cbLockDoor.Enabled = false;
                    _cbOpenMax.SelectedItem = null;
                    _cbOpenMax.Enabled = false;
                    SetDefaultApasValue(DoorEnviromentType.Standard);
                    break;
                case DoorEnviromentType.StandardWithMaxOpened:
                    _cbElectricStrikeOpposite.SelectedItem = null;
                    _cbElectricStrikeOpposite.Enabled = false;
                    _cbExtraElectricStrikeOpposite.SelectedItem = null;
                    _cbExtraElectricStrikeOpposite.Enabled = false;
                    _cbOpenDoor.Enabled = true;
                    _cbLockDoor.SelectedItem = null;
                    _cbLockDoor.Enabled = false;
                    _cbOpenMax.Enabled = true;
                    break;
                case DoorEnviromentType.StandardWithLocking:
                    _cbElectricStrikeOpposite.SelectedItem = null;
                    _cbElectricStrikeOpposite.Enabled = false;
                    _cbExtraElectricStrikeOpposite.SelectedItem = null;
                    _cbExtraElectricStrikeOpposite.Enabled = false;
                    _cbOpenDoor.Enabled = true;
                    _cbLockDoor.Enabled = true;
                    _cbOpenMax.SelectedItem = null;
                    _cbOpenMax.Enabled = false;
                    break;
                case DoorEnviromentType.Minimal:
                    _cbElectricStrikeOpposite.SelectedItem = null;
                    _cbElectricStrikeOpposite.Enabled = false;
                    _cbExtraElectricStrikeOpposite.SelectedItem = null;
                    _cbExtraElectricStrikeOpposite.Enabled = false;
                    _cbOpenDoor.SelectedItem = null;
                    _cbOpenDoor.Enabled = false;
                    _cbLockDoor.SelectedItem = null;
                    _cbLockDoor.Enabled = false;
                    _cbOpenMax.SelectedItem = null;
                    _cbOpenMax.Enabled = false;
                    SetDefaultApasValue(DoorEnviromentType.Minimal);
                    break;
                case DoorEnviromentType.Turnstile:
                    _cbElectricStrikeOpposite.Enabled = true;
                    _cbExtraElectricStrikeOpposite.Enabled = true;
                    _cbOpenDoor.Enabled = true;
                    _cbLockDoor.SelectedItem = null;
                    _cbLockDoor.Enabled = false;
                    _cbOpenMax.SelectedItem = null;
                    _cbOpenMax.Enabled = false;
                    SetDefaultApasValue(DoorEnviromentType.Standard);
                    break;
                default:
                    _cbElectricStrikeOpposite.Enabled = true;
                    _cbExtraElectricStrikeOpposite.Enabled = true;
                    _cbOpenDoor.Enabled = true;
                    _cbLockDoor.Enabled = true;
                    _cbOpenMax.Enabled = true;
                    break;
            }
            EditTextChanger(sender, e);
        }

        private void SetDefaultApasValue(DoorEnviromentType doorEnviromentType)
        {
            if (_editingObject.Configured)
                return;

            // defaults for door electric strike and door contact
            if (_editingObject.DCU != null)
            {
                ActuatorsDropDown(_cbElectricStrike, null);

                if (_cbElectricStrike.Items.Count > 1
                    && ((Output)_cbElectricStrike.Items[1]).OutputNumber == 0)
                {
                    _cbElectricStrike.SelectedIndex = 1;
                }

                SensorsDropDown(_cbOpenDoor, null);

                if (doorEnviromentType == DoorEnviromentType.Standard
                    && _cbOpenDoor.Items.Count > 1
                    && ((Input)_cbOpenDoor.Items[1]).InputNumber == 0)
                {
                    _cbOpenDoor.SelectedIndex = 1;
                }
            }

            // defaults for inside/outside access triggers
            if (_editingObject.DCU != null)
            {
                var dcu = Plugin.MainServerProvider.DCUs.GetObjectById(_editingObject.DCU.IdDCU);

                if (dcu == null)
                    return;

                CardReader cr1 = null;
                CardReader cr2 = null;

                foreach (var cr in dcu.CardReaders)
                {
                    switch (cr.Address)
                    {
                        case 1:
                            cr1 = cr;
                            break;
                        case 2:
                            cr2 = cr;
                            break;
                    }
                }

                if (cr1 != null && cr2 != null)
                {
                    _crInternal = cr2;
                    _crExternal = cr1;

                }
                else
                {
                    if (cr1 != null)
                    {
                        _crInternal = GetSecondInput();
                        _crExternal = cr1;
                    }
                    else if (cr2 != null)
                    {
                        _crExternal = cr2;
                    }
                    else
                    {
                        _crInternal = GetSecondInput();
                    }
                }
            }
            else if (_editingObject.CCU != null)
            {
                var ccu = Plugin.MainServerProvider.CCUs.GetObjectById(_editingObject.CCU.IdCCU);

                if (ccu == null)
                    return;

                _crInternal = ccu.CardReaders.FirstOrDefault(cr => cr.Address == (_editingObject.Number * 2) + 1);
                _crExternal = ccu.CardReaders.FirstOrDefault(cr => cr.Address == (_editingObject.Number * 2) + 2);
            }

            RefreshInternalCardReader();
            RefreshExternalCardReader();
        }

        private Input GetSecondInput()
        {
            if (_cbOpenDoor.Items.Count > 2)
            {
                var input2 = (Input)_cbOpenDoor.Items[2];

                if ((input2).InputNumber == 1)
                    return input2;
            }

            return null;
        }

        private void _cbElectricStrike_SelectedIndexChanged(object sender, EventArgs e)
        {
            EditTextChanger(sender, e);
            if (_cbElectricStrike.SelectedItem is Output)
            {
                _chbElectricStrikeImpulse.Enabled = true;
                if (_chbElectricStrikeImpulse.Checked)
                    _eElectricStrikeImpulseDelay.Enabled = true;
            }
            else
            {
                _chbElectricStrikeImpulse.Enabled = false;
                _chbElectricStrikeImpulse.Checked = false;
                _eElectricStrikeImpulseDelay.Enabled = false;
                _eElectricStrikeImpulseDelay.Value = DEFAULTIMPULSEDELAY;
            }
        }

        private void _cbElectricStrikeOpposite_SelectedIndexChanged(object sender, EventArgs e)
        {
            EditTextChanger(sender, e);
            if (_cbElectricStrikeOpposite.SelectedItem is Output)
            {
                _chbElectricStrikeOppositeImpulse.Enabled = true;
                if (_chbElectricStrikeOppositeImpulse.Checked)
                    _eElectricStrikeOppositeImpulseDelay.Enabled = true;
            }
            else
            {
                _chbElectricStrikeOppositeImpulse.Enabled = false;
                _chbElectricStrikeOppositeImpulse.Checked = false;
                _eElectricStrikeOppositeImpulseDelay.Enabled = false;
                _eElectricStrikeOppositeImpulseDelay.Value = DEFAULTIMPULSEDELAY;
            }
        }

        private void _cbExtraElectricStrike_SelectedIndexChanged(object sender, EventArgs e)
        {
            EditTextChanger(sender, e);
            if (_cbExtraElectricStrike.SelectedItem is Output)
            {
                _chbExtraElectricStrikeImpulse.Enabled = true;
                if (_chbExtraElectricStrikeImpulse.Checked)
                    _eExtraElectricStrikeImpulseDelay.Enabled = true;
            }
            else
            {
                _chbExtraElectricStrikeImpulse.Enabled = false;
                _chbExtraElectricStrikeImpulse.Checked = false;
                _eExtraElectricStrikeImpulseDelay.Enabled = false;
                _eExtraElectricStrikeImpulseDelay.Value = DEFAULTIMPULSEDELAY;
            }
        }

        private void _cbExtraElectricStrikeOpposite_SelectedIndexChanged(object sender, EventArgs e)
        {
            EditTextChanger(sender, e);
            if (_cbExtraElectricStrikeOpposite.SelectedItem is Output)
            {
                _chbExtraElectricStrikeOppositeImpulse.Enabled = true;
                if (_chbExtraElectricStrikeOppositeImpulse.Checked)
                    _eExtraElectricStrikeOppositeImpulseDelay.Enabled = true;
            }
            else
            {
                _chbExtraElectricStrikeOppositeImpulse.Enabled = false;
                _chbExtraElectricStrikeOppositeImpulse.Checked = false;
                _eExtraElectricStrikeOppositeImpulseDelay.Enabled = false;
                _eExtraElectricStrikeOppositeImpulseDelay.Value = DEFAULTIMPULSEDELAY;
            }
        }

        private void _cbElectricStrikeOpposite_EnabledChanged(object sender, EventArgs e)
        {
            if (_cbElectricStrikeOpposite.Enabled)
            {
                _lElectricStrikeOpposite.Enabled = true;
                if (_cbElectricStrikeOpposite.SelectedItem is Output)
                    _chbElectricStrikeOppositeImpulse.Enabled = true;

                if (_chbElectricStrikeOppositeImpulse.Checked)
                    _eElectricStrikeOppositeImpulseDelay.Enabled = true;
            }
            else
            {
                _lElectricStrikeOpposite.Enabled = false;
                _chbElectricStrikeOppositeImpulse.Enabled = false;
                _eElectricStrikeOppositeImpulseDelay.Enabled = false;
            }
        }

        private void _cbExtraElectricStrikeOpposite_EnabledChanged(object sender, EventArgs e)
        {
            if (_cbExtraElectricStrikeOpposite.Enabled)
            {
                _lExtraElectricStrikeOpposite.Enabled = true;
                if (_cbExtraElectricStrikeOpposite.SelectedItem is Output)
                    _chbExtraElectricStrikeOppositeImpulse.Enabled = true;

                if (_chbExtraElectricStrikeOppositeImpulse.Checked)
                    _eExtraElectricStrikeOppositeImpulseDelay.Enabled = true;
            }
            else
            {
                _lExtraElectricStrikeOpposite.Enabled = false;
                _chbExtraElectricStrikeOppositeImpulse.Enabled = false;
                _eExtraElectricStrikeOppositeImpulseDelay.Enabled = false;
            }
        }

        private void _cbLockDoor_EnabledChanged(object sender, EventArgs e)
        {
            if (_cbLockDoor.Enabled)
            {
                _lLockDoor.Enabled = true;
                if (_cbLockDoor.SelectedItem is Input)
                {
                    _chbLockDoorBalanced.Enabled = true;
                    _chbLockDoorInverted.Enabled = true;
                }
            }
            else
            {
                _lLockDoor.Enabled = false;
                _chbLockDoorBalanced.Enabled = false;
                _chbLockDoorInverted.Enabled = false;
            }
        }

        private void _cbOpenMax_EnabledChanged(object sender, EventArgs e)
        {
            if (_cbOpenMax.Enabled)
            {
                _lOpenDoorMax.Enabled = true;
                if (_cbOpenMax.SelectedItem is Input)
                {
                    _chbOpenMaxDoorBalanced.Enabled = true;
                    _chbOpenMaxDoorInverted.Enabled = true;
                }
            }
            else
            {
                _lOpenDoorMax.Enabled = false;
                _chbOpenMaxDoorBalanced.Enabled = false;
                _chbOpenMaxDoorInverted.Enabled = false;
            }
        }

        private void _cbBypassAlarm_SelectedIndexChanged(object sender, EventArgs e)
        {
            EditTextChanger(sender, e);

            if (_cbBypassAlarm.SelectedItem is Output
                && _eDelayBeforeUnlock.Minimum == 0)
            {
                if (_eDelayBeforeUnlock.Value < BEFORE_UNLOCK_TIME)
                {
                    _tcDoorsAutomat.SelectedTab = _tpDoorsTiming;

                    ControlNotification.Singleton.Info(NotificationPriority.JustOne, _eDelayBeforeUnlock,
                        GetString("InfoRecommendedBeforeUnlockTime"), ControlNotificationSettings.Default);
                    _eDelayBeforeUnlock.Focus();
                }

                _eDelayBeforeUnlock.Minimum = BEFORE_UNLOCK_TIME;
            }
            else
            {
                _eDelayBeforeUnlock.Minimum = 0;
            }
        }

        private void _chbElectricStrikeOppositeImpulse_CheckedChanged(object sender, EventArgs e)
        {
            EditTextChanger(sender, e);
            _eElectricStrikeOppositeImpulseDelay.Enabled = ((CheckBox)sender).Checked;
        }

        private void _chbExtraElectricStrikeOppositeImpulse_CheckedChanged(object sender, EventArgs e)
        {
            EditTextChanger(sender, e);
            _eExtraElectricStrikeOppositeImpulseDelay.Enabled = ((CheckBox)sender).Checked;
        }

        private void _bApply_Click(object sender, EventArgs e)
        {
            if (Apply_Click())
            {
                RefreshConfigured();
                _bApply.Enabled = false;
            }
        }

        private void _bAccessGranted_Click(object sender, EventArgs e)
        {
            _eResultAccessGranted.Text = string.Empty;
            _eResultAccessGranted.BackColor = _baseColorResultAccessGranted;

            if (!Plugin.MainServerProvider.DoorEnvironments.HasAccessToAccessGranted(_editingObject.IdDoorEnvironment))
            {
                _eResultAccessGranted.Text = GetString("NCASDoorEnvironmentEditForm_AccessGrantedFailedNoAccessRights");
                _eResultAccessGranted.BackColor = Color.Red;
                return;
            }

            SafeThread.StartThread(DoAccessGranted);
        }

        private void DoAccessGranted()
        {
            var result = Plugin.MainServerProvider.DoorEnvironments.DoorEnvironmentAccessGranted(_editingObject);
            SetAccessGrantedResult(result);
        }

        private void SetAccessGrantedResult(bool? result)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<bool?>(SetAccessGrantedResult), result);
            }
            else
            {
                if (result == null)
                {
                    _eResultAccessGranted.Text = GetString("NCASDoorEnvironmentEditForm_AccessGrantedFailed");
                    _eResultAccessGranted.BackColor = Color.Red;
                    return;
                }

                if (result.Value)
                {
                    _eResultAccessGranted.Text = GetString("NCASDoorEnvironmentEditForm_AccessGrantedSuccess");
                    _eResultAccessGranted.BackColor = Color.LightGreen;
                    return;
                }

                _eResultAccessGranted.Text = GetString("NCASDoorEnvironmentEditForm_AccessGrantedFailedTimeOut");
                _eResultAccessGranted.BackColor = Color.Red;
            }
        }

        private void _bUnconfigureDoorsAutomat_Click(object sender, EventArgs e)
        {
            if (Dialog.Question(GetString("QuestionUnconfigureDoorEnvironment")))
            {
                if (Plugin.MainServerProvider.DoorEnvironments.UnconfigureDoorEnvironments(_editingObject))
                    Close();
            }
        }

        private void OutputsDropDown(object sender, EventArgs e)
        {
            if (CgpClient.Singleton.IsConnectionLost(false)) return;
            var cb = sender as ComboBox;

            if (cb == null) return;

            var selectedItem = cb.SelectedItem as Output;
            cb.Items.Clear();
            var outputs = Plugin.MainServerProvider.DoorEnvironments.GetAllCCUOutputsNotInDoorEnvironmentsActuators(_editingObject.IdDoorEnvironment);
            outputs = outputs.OrderBy(output => output.OutputNumber).ToList();

            cb.Items.Add(string.Empty);
            foreach (var output in outputs)
            {
                AddOutputsToComboBox(output, cb, selectedItem);
            }
        }

        private void AddOutputsToComboBox(Output output, ComboBox cb, Output selectedItem)
        {
            if (output.Compare(_cbElectricStrike.SelectedItem as Output))
            {
                return;
            }

            if (output.Compare(_cbElectricStrikeOpposite.SelectedItem as Output))
            {
                return;
            }

            if (output.Compare(_cbExtraElectricStrike.SelectedItem as Output))
            {
                return;
            }

            if (output.Compare(_cbExtraElectricStrikeOpposite.SelectedItem as Output))
            {
                return;
            }

            if (output.Compare(_cbBypassAlarm.SelectedItem as Output))
            {
                return;
            }

            cb.Items.Add(output);

            if (output.Compare(selectedItem))
            {
                cb.SelectedItem = output;
            }
        }

        private void RefreshInternalCardReader()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DVoid2Void(RefreshInternalCardReader));
            }
            else
            {
                _chbPushButtonInternalInverted.Enabled = false;
                _chbPushButtonInternalBalanced.Enabled = false;

                if (_crInternal != null)
                {
                    _tbmInternalCardReader.Text = _crInternal.ToString();
                    _tbmInternalCardReader.TextImage = Plugin.GetImageForAOrmObject(_crInternal);

                    if (_crInternal is Input)
                    {
                        _chbPushButtonInternalInverted.Enabled = true;
                        _chbPushButtonInternalBalanced.Enabled = true;
                    }
                }
                else
                    _tbmInternalCardReader.Text = string.Empty;
            }
        }

        private void RefreshExternalCardReader()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DVoid2Void(RefreshExternalCardReader));
            }
            else
            {
                _chbPushButtonExternalInverted.Enabled = false;
                _chbPushButtonExternalBalanced.Enabled = false;

                if (_crExternal != null)
                {
                    _tbmExternalCardReader.Text = _crExternal.ToString();
                    _tbmExternalCardReader.TextImage = Plugin.GetImageForAOrmObject(_crExternal);

                    if (_crExternal is Input)
                    {
                        _chbPushButtonExternalInverted.Enabled = true;
                        _chbPushButtonExternalBalanced.Enabled = true;
                    }
                }
                else
                    _tbmExternalCardReader.Text = string.Empty;
            }
        }

        private void AddCardReaders(List<AOrmObject> listObjects, AOrmObject anotherCardReader)
        {
            var cardReaders = Plugin.MainServerProvider.DoorEnvironments.GetCardReadersNotInDoorEnvironments(_editingObject.IdDoorEnvironment).OrderBy(cardReader => cardReader.Name).ToList();

            if (cardReaders != null)
            {
                foreach (var cardReader in cardReaders)
                {
                    if (!cardReader.Compare(anotherCardReader))
                        listObjects.Add(cardReader);
                }
            }
        }

        private void AddPushButtons(List<AOrmObject> listObjects, AOrmObject anotherCardReader)
        {
            IList<Input> inputs = Plugin.MainServerProvider.DoorEnvironments.GetInputsNotInDoorEnvironments(_editingObject.IdDoorEnvironment).OrderBy(input => input.InputNumber).ToList();

            if (inputs != null)
            {
                foreach (var input in inputs)
                {
                    if (!input.Compare(_cbLockDoor.SelectedItem as Input) && !input.Compare(_cbOpenDoor.SelectedItem as Input)
                        && !input.Compare(_cbOpenMax.SelectedItem as Input) && !input.Compare(anotherCardReader))
                    {
                        listObjects.Add(input);
                    }
                }
            }
        }

        private void ModifyCardReaderInternal()
        {
            if (CgpClient.Singleton.IsConnectionLost(false)) return;
            var listObjects = new List<AOrmObject>();

            AddCardReaders(listObjects, _crExternal);
            AddPushButtons(listObjects, _crExternal);

            var formModify = new ListboxFormAdd(listObjects, GetString("NCASDoorEnvironmentEditForm_ModifyCardReaderPushButton"));
            object outObject;
            formModify.ShowDialog(out outObject);
            if (outObject != null)
            {
                if (ConfirmAddingUnlockedCardReader(outObject as CardReader))
                {
                    _crInternal = outObject as AOrmObject;
                    Plugin.AddToRecentList(_crInternal);
                    RefreshInternalCardReader();
                }
            }
        }

        private void ModifyCardReaderExternal()
        {
            if (CgpClient.Singleton.IsConnectionLost(false)) return;
            var listObjects = new List<AOrmObject>();

            AddCardReaders(listObjects, _crInternal);
            AddPushButtons(listObjects, _crInternal);
            var formModify = new ListboxFormAdd(listObjects, GetString("NCASDoorEnviromentEditForm_ModifyCardReaderPushButton"));
            object outObject;
            formModify.ShowDialog(out outObject);
            if (outObject != null)
            {
                if (ConfirmAddingUnlockedCardReader(outObject as CardReader))
                {
                    _crExternal = outObject as AOrmObject;
                    Plugin.AddToRecentList(_crExternal);
                    RefreshExternalCardReader();
                }
            }
        }

        private void _tbmInternalCardReader_DoubleClick(object sender, EventArgs e)
        {
            if (_crInternal != null)
            {
                if (_crInternal is CardReader)
                {
                    NCASCardReadersForm.Singleton.OpenEditForm(_crInternal as CardReader);
                }
                else
                {
                    NCASInputsForm.Singleton.OpenEditForm(_crInternal as Input);
                }
            }
        }

        private void _tbmExternalCardReader_DoubleClick(object sender, EventArgs e)
        {
            if (_crExternal != null)
            {
                if (_crExternal is CardReader)
                {
                    NCASCardReadersForm.Singleton.OpenEditForm(_crExternal as CardReader);
                }
                else
                {
                    NCASInputsForm.Singleton.OpenEditForm(_crExternal as Input);
                }
            }
        }

        private void _tbmInternalCardReader_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void _tbmInternalCardReader_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var output = e.Data.GetFormats();
                if (output == null) return;
                AddCardReaderInternal(e.Data.GetData(output[0]));
            }
            catch
            {
            }
        }

        private void AddCardReaderInternal(object newCardReaderPushButtonInternal)
        {
            try
            {
                if (newCardReaderPushButtonInternal is CardReader || newCardReaderPushButtonInternal is Input)
                {
                    var listObjects = new List<AOrmObject>();

                    if (newCardReaderPushButtonInternal is CardReader)
                    {
                        AddCardReaders(listObjects, _crExternal);
                    }
                    else
                    {
                        AddPushButtons(listObjects, _crExternal);
                    }

                    if (IsObjectInList(listObjects, newCardReaderPushButtonInternal as AOrmObject))
                    {
                        if (ConfirmAddingUnlockedCardReader(newCardReaderPushButtonInternal as CardReader))
                        {
                            _crInternal = newCardReaderPushButtonInternal as AOrmObject;
                            RefreshInternalCardReader();
                            Plugin.AddToRecentList(newCardReaderPushButtonInternal);
                        }
                    }
                    else
                    {
                        if (newCardReaderPushButtonInternal is CardReader)
                        {
                            ControlNotification.Singleton.Error(NotificationPriority.JustOne, _tbmInternalCardReader.ImageTextBox,
                                 GetString("ErrorCardReaderAlreadyUsed"), ControlNotificationSettings.Default);
                        }
                        else
                        {
                            ControlNotification.Singleton.Error(NotificationPriority.JustOne, _tbmInternalCardReader.ImageTextBox,
                                 GetString("ErrorInputAlreadyUsed"), ControlNotificationSettings.Default);
                        }
                    }
                }
                else
                {
                    ControlNotification.Singleton.Error(NotificationPriority.JustOne, _tbmInternalCardReader.ImageTextBox,
                       GetString("ErrorWrongObjectType"), ControlNotificationSettings.Default);
                }
            }
            catch
            { }
        }



        private bool IsObjectInList(List<AOrmObject> listObjects, AOrmObject ormObject)
        {
            if (ormObject != null && listObjects != null && listObjects.Count > 0)
            {
                foreach (object obj in listObjects)
                {
                    if (ormObject.Compare(obj))
                        return true;
                }
            }

            return false;
        }

        private void _tbmExternalCardReader_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void _tbmExternalCardReader_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var output = e.Data.GetFormats();
                if (output == null) return;
                AddCardReaderExternal(e.Data.GetData(output[0]));
            }
            catch
            {
            }
        }

        private void AddCardReaderExternal(object newCardReaderPushButtonExternal)
        {
            try
            {
                if (newCardReaderPushButtonExternal is CardReader || newCardReaderPushButtonExternal is Input)
                {
                    var listObjects = new List<AOrmObject>();

                    if (newCardReaderPushButtonExternal is CardReader)
                    {
                        AddCardReaders(listObjects, _crInternal);
                    }
                    else
                    {
                        AddPushButtons(listObjects, _crInternal);
                    }

                    if (IsObjectInList(listObjects, newCardReaderPushButtonExternal as AOrmObject))
                    {
                        if (ConfirmAddingUnlockedCardReader(newCardReaderPushButtonExternal as CardReader))
                        {
                            _crExternal = newCardReaderPushButtonExternal as AOrmObject;
                            RefreshExternalCardReader();
                            Plugin.AddToRecentList(newCardReaderPushButtonExternal);
                        }
                    }
                    else
                    {
                        if (newCardReaderPushButtonExternal is CardReader)
                        {
                            ControlNotification.Singleton.Error(NotificationPriority.JustOne, _tbmExternalCardReader.ImageTextBox,
                                 GetString("ErrorCardReaderAlreadyUsed"), ControlNotificationSettings.Default);
                        }
                        else
                        {
                            ControlNotification.Singleton.Error(NotificationPriority.JustOne, _tbmExternalCardReader.ImageTextBox,
                                 GetString("ErrorInputAlreadyUsed"), ControlNotificationSettings.Default);
                        }
                    }
                }
                else
                {
                    ControlNotification.Singleton.Error(NotificationPriority.JustOne, _tbmExternalCardReader.ImageTextBox,
                       GetString("ErrorWrongObjectType"), ControlNotificationSettings.Default);
                }
            }
            catch
            { }
        }

        private void SetReferencedBy()
        {
            _tpReferencedBy.Controls.Add(new ReferencedByForm(GetListReferencedObjects, NCASClient.LocalizationHelper,
                ObjectImageList.Singleton.GetAllObjectImages()));
        }

        IList<AOrmObject> GetListReferencedObjects()
        {
            return Plugin.MainServerProvider.DoorEnvironments.
                GetReferencedObjects(_editingObject.IdDoorEnvironment, CgpClient.Singleton.GetListLoadedPlugins());
        }

        private void _lbUserFolders_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ObjectsPlacementHandler.UserFolders_MouseDoubleClick(_lbUserFolders, _editingObject);
        }

        private void _tpUserFolders_Enter(object sender, EventArgs e)
        {
            ObjectsPlacementHandler.UserFolders_Enter(_lbUserFolders, _editingObject);
        }

        private void _bRefresh_Click(object sender, EventArgs e)
        {
            ObjectsPlacementHandler.UserFolders_Enter(_lbUserFolders, _editingObject);
        }

        private void _tbmInternalCardReader_ButtonPopupMenuItemClick(ToolStripItem item, int index)
        {
            if (item.Name == "_tsiModify1")
            {
                ModifyCardReaderInternal();
            }
            else if (item.Name == "_tsiRemove1")
            {
                _crInternal = null;
                RefreshInternalCardReader();
            }
        }

        private void _tbmExternalCardReader_ButtonPopupMenuItemClick(ToolStripItem item, int index)
        {
            if (item.Name == "_tsiModify2")
            {
                ModifyCardReaderExternal();
            }
            else if (item.Name == "_tsiRemove2")
            {
                _crExternal = null;
                RefreshExternalCardReader();
            }
        }

        private void _tpCar_Enter(object sender, EventArgs e)
        {
            RefreshCarDoorEnvironmentsFromServer();
        }

        private void _bAddCarDoorEnvironment_Click(object sender, EventArgs e)
        {
            AddCarDoorEnvironment();
        }

        private void _bEditCarDoorEnvironment_Click(object sender, EventArgs e)
        {
            EditSelectedCarDoorEnvironment();
        }

        private void _bRemoveCarDoorEnvironment_Click(object sender, EventArgs e)
        {
            RemoveSelectedCarDoorEnvironment();
        }

        private void _dgCarDoorEnvironments_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            EditSelectedCarDoorEnvironment();
        }

        private void RefreshCarDoorEnvironmentsFromServer()
        {
            var carDoorEnvironments = GetCarDoorEnvironmentsFromServer(out var error)
                            ?.Where(cde => cde.DoorEnvironment != null &&
                               cde.DoorEnvironment.IdDoorEnvironment == _editingObject.IdDoorEnvironment)
                .ToList();

            if (error != null || carDoorEnvironments == null)
                return;
            if (carDoorEnvironments.Count == 0 && _carDoorEnvironments.Count > 0)
            {
                // Do not clear already loaded items when the server returns an unexpected empty list
                LoadCarDoorEnvironments();
                return;
            }

            var carsTable = CgpClient.Singleton.MainServerProvider?.Cars;
            if (carsTable != null)
            {
                foreach (var carDoorEnvironment in carDoorEnvironments)
                {
                    var carId = carDoorEnvironment.Car?.IdCar;
                    if (carId != null && carId != Guid.Empty)
                        carDoorEnvironment.Car = carsTable.GetObjectById(carId.Value);
                }
            }

            _carDoorEnvironments.Clear();
            _carDoorEnvironments.AddRange(carDoorEnvironments);
            LoadCarDoorEnvironments();
        }

        private void LoadCarDoorEnvironments()
        {
            _dgCarDoorEnvironments.DataGrid.DataSource = null;

            var view = _carDoorEnvironments
                .OrderBy(cde => cde.Car?.Lp)
                .Select(cde => new CarDoorEnvironmentView
                {
                    Symbol = Plugin.GetImageForAOrmObject(cde.Car),
                    CarId = cde.Car?.IdCar ?? Guid.Empty,
                    CarName = cde.Car?.Lp ?? string.Empty,
                    AccessType = cde.AccessType
                })
                .ToList();

            _dgCarDoorEnvironments.DataGrid.DataSource = view;
        }

        private void AddCarDoorEnvironment()
        {
            Exception error;
            var availableCars = GetAvailableCars(out error)?.ToList();
            if (error != null)
            {
                MessageBox.Show(error.Message);
                return;
            }

            if (availableCars == null || availableCars.Count == 0)
            {
                MessageBox.Show(GetString("NCASDoorEnvironmentEditForm_NoAvailableCars"));
                return;
            }

            using (var addForm = new LookupedCarsForm(
           availableCars,
           _tcAccessTypeColumn.HeaderText,
           car => Plugin.GetImageForAOrmObject(car)))
            {
                if (addForm.ShowDialog(this) != DialogResult.OK)
                    return;

                var selectedCars = addForm.SelectedCars;
                if (selectedCars == null)
                    return;

                foreach (var selectedCar in selectedCars)
                {
                    if (selectedCar == null)
                        continue;

                    if (_carDoorEnvironments.Any(cde => cde.Car != null && cde.Car.IdCar == selectedCar.IdCar))
                        continue;
                    var carDoorEnvironment = new CarDoorEnvironment
                    {
                        Car = selectedCar,
                        DoorEnvironment = _editingObject,
                        AccessType = addForm.SelectedAccessType
                    };
                    _carDoorEnvironments.Add(carDoorEnvironment);
                    TryInsertCarDoorEnvironment(carDoorEnvironment);
                }

                LoadCarDoorEnvironments();
            }
        }

        private void EditSelectedCarDoorEnvironment()
        {
            var selected = GetSelectedCarDoorEnvironment();
            if (selected == null)
                return;

            using (var editForm = new NCASCarDoorEnvironmentEditForm(
                                        _editingObject?.Name,
                                        selected.Car?.Lp,
                                        selected.AccessType))
            {
                if (editForm.ShowDialog(this) != DialogResult.OK)
                    return;

                selected.AccessType = editForm.SelectedAccessType;
                if (selected.IdCarDoorEnvironment != Guid.Empty)
                    TryUpdateCarDoorEnvironment(selected);
                LoadCarDoorEnvironments();
            }
        }

        private void RemoveSelectedCarDoorEnvironment()
        {
            var selected = GetSelectedCarDoorEnvironment();
            if (selected == null)
                return;
            if (selected.IdCarDoorEnvironment != Guid.Empty)
                TryDeleteCarDoorEnvironment(selected);

            _carDoorEnvironments.Remove(selected);
            LoadCarDoorEnvironments();
        }

        void ICgpDataGridView.EditClick()
        {
            EditSelectedCarDoorEnvironment();
        }

        void ICgpDataGridView.EditClick(ICollection<int> indexes)
        {
            SelectCarDoorEnvironmentIndexes(indexes);
            EditSelectedCarDoorEnvironment();
        }

        void ICgpDataGridView.DeleteClick()
        {
            RemoveSelectedCarDoorEnvironment();
        }

        void ICgpDataGridView.DeleteClick(ICollection<int> indexes)
        {
            SelectCarDoorEnvironmentIndexes(indexes);
            RemoveSelectedCarDoorEnvironment();
        }

        void ICgpDataGridView.InsertClick()
        {
            AddCarDoorEnvironment();
        }

        private void SelectCarDoorEnvironmentIndexes(ICollection<int> indexes)
        {
            if (indexes == null || indexes.Count == 0)
                return;

            foreach (DataGridViewRow row in _dgCarDoorEnvironments.DataGrid.Rows)
            {
                row.Selected = false;
            }

            foreach (var index in indexes.Where(i => i >= 0 && i < _dgCarDoorEnvironments.DataGrid.Rows.Count))
            {
                _dgCarDoorEnvironments.DataGrid.Rows[index].Selected = true;
            }
        }

        private CarDoorEnvironment GetSelectedCarDoorEnvironment()
        {
            if (!(_dgCarDoorEnvironments.DataGrid.CurrentRow?.DataBoundItem is CarDoorEnvironmentView view))
                return null;

            if (view.CarId != Guid.Empty)
                return _carDoorEnvironments.FirstOrDefault(cde => cde.Car != null && cde.Car.IdCar == view.CarId);

            return _carDoorEnvironments.FirstOrDefault(cde => string.Equals(cde.Car?.Lp, view.CarName));
        }

        private void TryInsertCarDoorEnvironment(CarDoorEnvironment carDoorEnvironment)
        {
            try
            {
                var provider = Plugin.MainServerProvider as ICgpNCASRemotingProvider
                               ?? CgpClient.Singleton.MainServerProvider as ICgpNCASRemotingProvider;
                if (provider == null)
                {
                    MessageBox.Show("NCAS remoting provider is not available – cannot save car door environments.");
                    return;
                }

                var carDoorEnvironmentsTable = provider.CarDoorEnvironments;
                if (carDoorEnvironmentsTable == null || carDoorEnvironment == null)
                    return;

                var insertResult = carDoorEnvironmentsTable.Insert(ref carDoorEnvironment, out var insertError);

                if (insertResult == true)
                {
                    Plugin.AddToRecentList(carDoorEnvironment);
                }
                else if (insertError != null)
                {
                    MessageBox.Show(insertError.Message);
                }
            }
            catch (MissingMethodException)
            {
                MessageBox.Show(
                    "Current server version does not support car door environments. Please update the server.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TryUpdateCarDoorEnvironment(CarDoorEnvironment carDoorEnvironment)
        {
            try
            {
                var provider = Plugin.MainServerProvider as ICgpNCASRemotingProvider
                               ?? CgpClient.Singleton.MainServerProvider as ICgpNCASRemotingProvider;
                if (provider == null)
                {
                    MessageBox.Show("NCAS remoting provider is not available – cannot save car door environments.");
                    return;
                }

                var carDoorEnvironmentsTable = provider.CarDoorEnvironments;
                if (carDoorEnvironmentsTable == null || carDoorEnvironment == null)
                    return;

                var updateResult = carDoorEnvironmentsTable.Update(carDoorEnvironment, out var updateError);

                if (updateResult != true && updateError != null)
                {
                    MessageBox.Show(updateError.Message);
                }
            }
            catch (MissingMethodException)
            {
                MessageBox.Show(
                    "Current server version does not support car door environments. Please update the server.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TryDeleteCarDoorEnvironment(CarDoorEnvironment carDoorEnvironment)
        {
            try
            {
                var provider = Plugin.MainServerProvider as ICgpNCASRemotingProvider
                               ?? CgpClient.Singleton.MainServerProvider as ICgpNCASRemotingProvider;
                if (provider == null)
                {
                    MessageBox.Show("NCAS remoting provider is not available – cannot delete car door environments.");
                    return;
                }

                var carDoorEnvironmentsTable = provider.CarDoorEnvironments;
                if (carDoorEnvironmentsTable == null || carDoorEnvironment == null)
                    return;

                var deleteResult = carDoorEnvironmentsTable.Delete(carDoorEnvironment, out var deleteError);

                if (deleteResult != true && deleteError != null)
                {
                    MessageBox.Show(deleteError.Message);
                }
            }
            catch (MissingMethodException)
            {
                MessageBox.Show(
                    "Current server version does not support car door environments. Please update the server.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private ICollection<CarDoorEnvironment> GetCarDoorEnvironmentsFromServer(out Exception error)
        {
            error = null;

            var provider = Plugin.MainServerProvider as ICgpNCASRemotingProvider
               ?? CgpClient.Singleton.MainServerProvider as ICgpNCASRemotingProvider;
            if (provider == null)
            {
                error = new MissingFieldException(
                    typeof(ICgpNCASRemotingProvider).FullName,
                    nameof(ICgpNCASRemotingProvider));
                return null;
            }

            List<CarDoorEnvironment> carDoorEnvironments;

            try
            {
                var carDoorEnvironmentsTable = provider.CarDoorEnvironments;
                if (carDoorEnvironmentsTable == null)
                {
                    error = new MissingMemberException(
                        provider.GetType().FullName,
                        nameof(ICgpNCASRemotingProvider.CarDoorEnvironments));
                    return null;
                }

                carDoorEnvironments = carDoorEnvironmentsTable
                    .GetByDoorEnvironmentId(_editingObject.IdDoorEnvironment, out error)
                    ?.ToList();
            }
            catch (Exception ex)
            {
                error = ex;
                return null;
            }

            if (carDoorEnvironments == null && error == null)
            {
                error = new MissingMemberException(
                            provider.CarDoorEnvironments.GetType().FullName,
                        nameof(ICarDoorEnvironments.GetByDoorEnvironmentId));
            }

            return carDoorEnvironments;
        }


        private ICollection<Car> GetAvailableCars(out Exception error)
        {
            error = null;

            var carsTable = CgpClient.Singleton.MainServerProvider?.Cars;
            var allCars = carsTable?.List(out error);
            if (carsTable == null)
            {
                error = new MissingFieldException(
                    typeof(CgpClient).FullName,
                    nameof(ICgpServerRemotingProvider.Cars));
            }
            if (error != null || allCars == null)
                return allCars;

            var carDoorEnvironments = GetCarDoorEnvironmentsFromServer(out error);
            if (error != null)
                return null;

            if (carDoorEnvironments == null)
                return allCars.ToList();

            var assignedCarIds = new HashSet<Guid>(
                carDoorEnvironments
                    .Where(cde => cde.DoorEnvironment != null && cde.DoorEnvironment.IdDoorEnvironment == _editingObject.IdDoorEnvironment && cde.Car != null)
                    .Select(cde => cde.Car.IdCar));

            return allCars
                .Where(car => !assignedCarIds.Contains(car.IdCar))
                .ToList();
        }

        private static string GetCarDisplayName(Car car)
        {
            if (car == null)
                return string.Empty;

            if (!string.IsNullOrEmpty(car.Lp))
                return car.Lp;

            return car.ToString();
        }

        #region AlarmInstructions

        private class CarDoorEnvironmentView
        {
            public Image Symbol { get; set; }
            public Guid CarId { get; set; }
            public string CarName { get; set; }

            public CarDoorEnvironmentAccessType AccessType { get; set; }
        }

        private class LookupedCarsForm : Form
        {
            private readonly CgpDataGridView _carsGrid;
            private readonly BindingList<LookupedCarView> _carsBinding;
            private readonly ComboBox _cbAccessType;
            private readonly CheckBox _cbSelectUnselectAll;
            private bool _updatingSelectAll;

            public LookupedCarsForm(
                    IEnumerable<Car> cars,
                    string accessTypeTitle,
                    Func<Car, Image> getCarSymbol)
            {
                var localizationHelper = CgpClient.Singleton.LocalizationHelper;
                var selectAllTitle = CgpClient.Singleton.LocalizationHelper
                                        .GetString("LookupedCCUsForm_cbSelectUnselectAll");
                Text = GetLocalizedString(
                            localizationHelper,
                            "NCASDoorEnvironmentEditForm_lookupedCars",
                            "lookupedCars");
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MinimizeBox = false;
                MaximizeBox = false;
                ShowInTaskbar = false;
                Size = new Size(700, 450);

                _carsGrid = new CgpDataGridView
                {
                    Dock = DockStyle.Fill,
                    AllwaysRefreshOrder = false,
                    CopyOnRightClick = true
                };

                _carsGrid.DataGrid.AutoGenerateColumns = false;
                _carsGrid.DataGrid.AllowUserToAddRows = false;
                _carsGrid.DataGrid.AllowUserToDeleteRows = false;
                _carsGrid.DataGrid.AllowUserToResizeRows = false;
                _carsGrid.DataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                _carsGrid.DataGrid.MultiSelect = false;
                _carsGrid.DataGrid.ReadOnly = false;
                _carsGrid.DataGrid.RowHeadersVisible = false;
                _carsGrid.DataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                var selectedColumn = new DataGridViewCheckBoxColumn
                {
                    Name = nameof(LookupedCarView.Selected),
                    DataPropertyName = nameof(LookupedCarView.Selected),
                    HeaderText = GetLocalizedString(
                        localizationHelper,
                        "NCASDoorEnvironmentEditForm_SelectedCars",
                        "Selected"),
                    Width = 40,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
                };

                var carColumn = new DataGridViewTextBoxColumn
                {
                    Name = nameof(LookupedCarView.CarName),
                    DataPropertyName = nameof(LookupedCarView.CarName),
                    HeaderText = CgpClient.Singleton.LocalizationHelper
                        .GetString("NCASDoorEnvironmentEditForm_tpCar"),
                    ReadOnly = true
                };

                _carsGrid.DataGrid.Columns.Add(selectedColumn);
                _carsGrid.DataGrid.Columns.Add(carColumn);

                _carsBinding = new BindingList<LookupedCarView>(cars
                    ?.OrderBy(car => car?.Lp)
                    .Select(car => new LookupedCarView
                    {
                        Car = car,
                        CarName = GetCarDisplayName(car),
                        Selected = false,
                        Symbol = getCarSymbol?.Invoke(car)
                    })
                    .ToList() ?? new List<LookupedCarView>());

                _carsGrid.DataGrid.DataSource = _carsBinding;
                _carsGrid.DataGrid.CellClick += (sender, args) =>
                {
                    if (args.RowIndex < 0)
                        return;

                    var carView = _carsBinding.ElementAt(args.RowIndex);
                    if (args.ColumnIndex != selectedColumn.Index)
                        carView.Selected = !carView.Selected;

                    _carsGrid.DataGrid.Refresh();
                    UpdateSelectAllState();
                };

                _carsGrid.DataGrid.CurrentCellDirtyStateChanged += (sender, args) =>
                {
                    if (_carsGrid.DataGrid.IsCurrentCellDirty)
                    {
                        _carsGrid.DataGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                        UpdateSelectAllState();
                    }
                };

                _cbSelectUnselectAll = new CheckBox
                {
                    AutoSize = true,
                    Dock = DockStyle.Top,
                    Text = selectAllTitle,
                    ThreeState = true
                };

                _cbSelectUnselectAll.CheckedChanged += (sender, args) =>
                {
                    if (_updatingSelectAll)
                        return;

                    foreach (var carView in _carsBinding)
                        carView.Selected = _cbSelectUnselectAll.CheckState == CheckState.Checked;
                    _carsGrid.DataGrid.Refresh();
                };

                var accessPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 3,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Padding = new Padding(10)
                };

                var accessLabel = new Label
                {
                    AutoSize = true,
                    Text = GetLocalizedString(
                        localizationHelper,
                        "NCASDoorEnvironmentEditForm_AccessType",
                        accessTypeTitle)
                };

                _cbAccessType = new ComboBox
                {
                    Dock = DockStyle.Top,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Margin = new Padding(0, 5, 0, 0)
                };

                _cbAccessType.Items.AddRange(Enum.GetValues(typeof(CarDoorEnvironmentAccessType))
                    .Cast<object>()
                    .ToArray());
                _cbAccessType.SelectedItem = CarDoorEnvironmentAccessType.None;

                var buttonsPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    Padding = new Padding(10)
                };

                var addButton = new Button
                {
                    Text = GetLocalizedString(
                        localizationHelper,
                        "General_bAdd",
                        "Add"),
                    DialogResult = DialogResult.OK,
                    Size = new Size(90, 32)
                };

                var cancelButton = new Button
                {
                    Text = GetLocalizedString(
                        localizationHelper,
                        "General_bCancel",
                        "Cancel"),
                    DialogResult = DialogResult.Cancel,
                    Size = new Size(90, 32)
                };

                buttonsPanel.Controls.Add(addButton);
                buttonsPanel.Controls.Add(cancelButton);

                accessPanel.Controls.Add(accessLabel, 0, 0);
                accessPanel.Controls.Add(_cbAccessType, 0, 1);
                accessPanel.Controls.Add(_cbSelectUnselectAll, 0, 2);

                var mainLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 3
                };

                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                mainLayout.Controls.Add(_carsGrid, 0, 0);
                mainLayout.Controls.Add(accessPanel, 0, 1);
                mainLayout.Controls.Add(buttonsPanel, 0, 2);

                Controls.Add(mainLayout);

                UpdateSelectAllState();

                AcceptButton = addButton;
                CancelButton = cancelButton;
            }

            public IEnumerable<Car> SelectedCars => _carsBinding
                .Where(carView => carView.Selected)
                .Select(carView => carView.Car)
                .Where(car => car != null)
                .ToList();

            public CarDoorEnvironmentAccessType SelectedAccessType =>
                _cbAccessType.SelectedItem is CarDoorEnvironmentAccessType selected
                    ? selected
                    : CarDoorEnvironmentAccessType.None;

            private static string GetLocalizedString(
                                    LocalizationHelper localizationHelper,
                                    string resourceKey,
                                    string defaultValue)
            {
                var localizedText = localizationHelper?.GetString(resourceKey);
                return string.IsNullOrEmpty(localizedText) ? defaultValue : localizedText;
            }
            private void UpdateSelectAllState()
            {
                _updatingSelectAll = true;

                var selectedCount = _carsBinding.Count(carView => carView.Selected);
                if (selectedCount == 0)
                    _cbSelectUnselectAll.CheckState = CheckState.Unchecked;
                else if (selectedCount == _carsBinding.Count)
                    _cbSelectUnselectAll.CheckState = CheckState.Checked;
                else
                    _cbSelectUnselectAll.CheckState = CheckState.Indeterminate;

                _updatingSelectAll = false;
            }

            private class LookupedCarView
            {
                public bool Selected { get; set; }
                public Image Symbol { get; set; }
                public string CarName { get; set; }
                public Car Car { get; set; }
            }
        }

        protected override bool LocalAlarmInstructionsView()
        {
            return
                CgpClient.Singleton.MainServerProvider.HasAccess(
                    NCASAccess.GetAccess(AccessNCAS.DoorEnvironmentsLocalAlarmInstructionsView));
        }

        protected override bool LocalAlarmInstructionsAdmin()
        {
            return
                CgpClient.Singleton.MainServerProvider.HasAccess(
                    NCASAccess.GetAccess(AccessNCAS.DoorEnvironmentsLocalAlarmInstructionsAdmin));
        }

        protected override string GetLocalAlarmInstruction()
        {
            return _editingObject.LocalAlarmInstruction;
        }

        #endregion

        private bool ConfirmAddingUnlockedCardReader(CardReader cardReader)
        {
            if (cardReader == null || cardReader.SecurityLevel != (byte)SecurityLevel.Unlocked)
            {
                return true;
            }

            var internalCardReader = _crInternal as CardReader;
            var externalCardReader = _crExternal as CardReader;
            if ((internalCardReader != null && internalCardReader.SecurityLevel == (byte)SecurityLevel.Unlocked) ||
                (externalCardReader != null && externalCardReader.SecurityLevel == (byte)SecurityLevel.Unlocked))
            {
                return true;
            }

            if ((internalCardReader != null && internalCardReader.SecurityLevel == (byte)SecurityLevel.Locked) ||
                (externalCardReader != null && externalCardReader.SecurityLevel == (byte)SecurityLevel.Locked))
            {
                return Dialog.WarningQuestion(GetString("WarningQuestionAddUnlockedCardReaderToLockedDoorEnvironment"));
            }

            return Dialog.WarningQuestion(GetString("WarningQuestionAddUnlockedCardReaderToDoorEnvironment"));
        }

        private void _cbOpenAllAlarmSettings_CheckedChanged(object sender, EventArgs e)
        {
            OpenAllCheckStateChanged(
                _accordionAlarmSettings,
                _cbOpenAllAlarmSettings.CheckState);

            _accordionAlarmSettings.Focus();
        }

        private void OpenAllCheckStateChanged(Accordion accordion, CheckState checkState)
        {
            if (checkState == CheckState.Indeterminate)
                return;

            if (checkState == CheckState.Checked)
            {
                accordion.OpenAll();

                return;
            }

            accordion.CloseAll();
        }
    }

    public class ItemDoorEnvironmentType
    {
        private readonly DoorEnviromentType _doorEnvironmentType;
        private readonly LocalizationHelper _localizationHelper;

        public DoorEnviromentType DoorEnvironmentType { get { return _doorEnvironmentType; } }

        public ItemDoorEnvironmentType(DoorEnviromentType doorEnvironmentType, LocalizationHelper localizationHelper)
        {
            _doorEnvironmentType = doorEnvironmentType;
            _localizationHelper = localizationHelper;
        }

        public override string ToString()
        {
            return _localizationHelper.GetString("DoorEnvironmentTypes_" + _doorEnvironmentType);
        }

        public static IList<ItemDoorEnvironmentType> GetList(LocalizationHelper localizationHelper)
        {
            return EnumHelper.ListAllValuesWithProcessing<DoorEnviromentType, ItemDoorEnvironmentType>(
                value => new ItemDoorEnvironmentType(value, localizationHelper));
        }
    }
}
