using System;
using System.Collections.Generic;
using System.Linq;

using Contal.Cgp.Globals;
using Contal.Cgp.NCAS.CCU.Alarms;
using Contal.Cgp.NCAS.CCU.CardReaderMechanism.Scenes;
using Contal.Cgp.NCAS.CCU.EventParameters;
using Contal.Cgp.NCAS.Definitions;
using Contal.Drivers.CardReader;
using Contal.Drivers.LPC3250;
using Contal.IwQuick;
using Contal.IwQuick.CrossPlatform.Data;
using JetBrains.Annotations;
using Contal.IwQuick.Data;

namespace Contal.Cgp.NCAS.CCU
{
    public enum AccessGrantedSource
    {
        Card = 0,
        EmergencyCode = 1,
        Other = 2
    }

    internal sealed class DoorEnvironments : AStateAndSettingsObjectCollection<
        DoorEnvironments,
        DoorEnvironmentSettings,
        DB.DoorEnvironment>
    {
        public class DoorEnvironmentStateChangedArgs
        {
            public Guid IdDoorEnvironment { get; private set; }

            public Guid GuidCardReaderAccessed { get; private set; }
            public Guid GuidCardAccessed { get; private set; }

            public DoorEnvironmentState State { get; private set; }
            public DoorEnvironmentStateDetail StateDetail { get; private set; }

            public DoorEnvironmentStateChangedArgs(
                Guid idDoorEnvironment,
                Guid guidCardAccessed,
                Guid guidCardReaderAccessed,
                DoorEnvironmentState state,
                DoorEnvironmentStateDetail stateDetail)
            {
                IdDoorEnvironment = idDoorEnvironment;

                GuidCardAccessed = guidCardAccessed;
                GuidCardReaderAccessed = guidCardReaderAccessed;

                State = state;
                StateDetail = stateDetail;
            }
        }

        private const string OUTPUT_ACTIVATOR_USE_INTRUSION_BRIDGE_PREFIX = "IntrusionBridge";

        public const bool USE_INTRUSION_BRIDGE = true;
        public const int INTRUSION_BRIDGE_PROCESSING_TIME = 1000;

        private readonly object _lockAccessGrantedFromClient = new object();
        private readonly object _lockLprPendingAuthorizations = new object();
        private readonly Dictionary<Guid, LprPendingAuthorization> _lprPendingAuthorizations = new Dictionary<Guid, LprPendingAuthorization>();

        private sealed class LprPendingAuthorization
        {
            public Guid DoorEnvironmentId { get; set; }
            public Guid CorrelationId { get; set; }
            public Guid CarId { get; set; }
            public string PlateNormalized { get; set; }
            public HashSet<Guid> ValidCardIds { get; set; }
            public LprRequiredSecondFactor RequiredSecondFactor { get; set; }
            public LprPassDirection Direction { get; set; }
            public DateTime ValidToUtc { get; set; }
            public Guid SourceCameraId { get; set; }
        }

        public override ObjectType ObjectType
        {
            get { return ObjectType.DoorEnvironment; }
        }

        private DoorEnvironments()
            : base(null)
        {
        }

        private bool PerformSyncRequest(
            Guid guidDoorEnvironment,
            WaitableProcessingRequest<DoorEnvironmentSettings> request)
        {
            bool enqueued = false;

            _objects.TryGetValue(
                guidDoorEnvironment,
                (key, found, value) =>
                {
                    if (found)
                    {
                        enqueued = true;
                        value.EnqueueSyncRequest(request);
                    }
                });

            if (enqueued)
                request.WaitForCompletion();

            return enqueued;
        }

        private void PerformAsyncRequest(
            Guid guidDoorEnvironment,
            Action<DoorEnvironmentSettings> requestAction)
        {
            _objects.TryGetValue(
                guidDoorEnvironment,
                (key, found, value) =>
                {
                    if (found)
                        value.EnqueueAsyncRequest(requestAction);
                });
        }

        public int GetDoorEnvironmentsCount()
        {
            CcuCore.DebugLog.Info(Log.NORMAL_LEVEL, () => "void DoorEnvironments.GetDoorEnvironmentsCount()");
            switch ((MainBoardVariant)MainBoard.Variant)
            {
                case MainBoardVariant.CCU40:
                case MainBoardVariant.CCU12:
                case MainBoardVariant.CAT12CE:
                    // use this approach because of extension boards
                    return IOControl.GetOutputCount();

                case MainBoardVariant.CCU05:
                    return NCASConstants.CCU05_MAX_DSM_COUNT;

                default:
                    return 0;
            }
        }

        public string GetOutputActivatorIntrusionBridge(Guid idDoorEnvironment)
        {
            CcuCore.DebugLog.Info(
                Log.NORMAL_LEVEL,
                () => String.Format(
                    "string DoorEnvironments.GetOutputActivatorIntrusionBridge(Guid idDoorEnvironment): [{0}]",
                    Log.GetStringFromParameters(idDoorEnvironment)));

            if (idDoorEnvironment == Guid.Empty)
            {
                CcuCore.DebugLog.Warning(
                    Log.BELOW_NORMAL_LEVEL,
                    () => String.Format(
                        "string DoorEnvironments.GetOutputActivatorIntrusionBridge return {0}",
                        Log.GetStringFromParameters(String.Empty)));

                return String.Empty;
            }

            var result = OUTPUT_ACTIVATOR_USE_INTRUSION_BRIDGE_PREFIX + idDoorEnvironment;

            CcuCore.DebugLog.Info(
                Log.NORMAL_LEVEL,
                () => String.Format(
                    "string DoorEnvironments.GetOutputActivatorIntrusionBridge return {0}",
                    Log.GetStringFromParameters(result)));

            return result;
        }

        public void ClearAccessGrantedVariables(Guid guidDoorEnvironment)
        {
            CcuCore.DebugLog.Info(
                Log.NORMAL_LEVEL,
                () => String.Format(
                    "void DoorEnvironments.ClearAccessGrantedVariables(Guid guidDoorEnvironment): [{0}]",
                    Log.GetStringFromParameters(guidDoorEnvironment)));

            DoorEnvironmentSettings doorEnvironmentSettings;

            if (_objects.TryGetValue(
                    guidDoorEnvironment,
                    out doorEnvironmentSettings))
                doorEnvironmentSettings.ClearAccessGrantedVariables();
        }

        public void OnAccessGranted(
            Guid guidDoorEnvironment,
            Guid guidCardReader,
            AccessDataBase accessData,
            int crAddress)
        {
            PerformAsyncRequest(
                guidDoorEnvironment,
                doorEnvironmentSettings =>
                    doorEnvironmentSettings.OnAccessGranted(
                        guidCardReader,
                        accessData,
                        crAddress,
                        false));
        }

        private class AccessGrantedFromClientRequest : WaitableProcessingRequest<DoorEnvironmentSettings>
        {
            public Func<bool> WaitForUnlockedOpened { get; private set; }

            protected override void ExecuteInternal(DoorEnvironmentSettings doorEnvironmentSettings)
            {
                if (doorEnvironmentSettings.DoorEnviromentState != DoorEnvironmentState.Unlocked &&
                    doorEnvironmentSettings.DoorEnviromentState != DoorEnvironmentState.Opened)
                {
                    doorEnvironmentSettings.CreateWaitForUnlockedOpened();
                    doorEnvironmentSettings.AccessGrantedFromClient();

                    WaitForUnlockedOpened = doorEnvironmentSettings.WaitForUnlockedOpened;
                }
            }
        }

        public bool AccessGrantedFromClient(
            Guid guidDoorEnvironment)
        {
            CcuCore.DebugLog.Info(Log.NORMAL_LEVEL,
                () =>
                    String.Format(
                        "void DoorEnvironments.AccessGrantedFromClient(Guid guidDoorEnvironment): [{0}]",
                        Log.GetStringFromParameters(guidDoorEnvironment)));

            lock (_lockAccessGrantedFromClient)
            {
                var request = new AccessGrantedFromClientRequest();

                if (!PerformSyncRequest(
                    guidDoorEnvironment,
                    request))
                {
                    return false;
                }

                return request.WaitForUnlockedOpened == null || request.WaitForUnlockedOpened();
            }
        }

        public bool TryAuthorizeCardByLprContext(
    Guid doorEnvironmentId,
    Guid cardId)
        {
            if (doorEnvironmentId == Guid.Empty || cardId == Guid.Empty)
                return false;

            lock (_lockLprPendingAuthorizations)
            {
                LprPendingAuthorization pending;

                if (!_lprPendingAuthorizations.TryGetValue(doorEnvironmentId, out pending))
                    return true;

                if (pending.ValidToUtc <= DateTime.UtcNow)
                {
                    _lprPendingAuthorizations.Remove(doorEnvironmentId);

                    CcuCore.DebugLog.Info(
                        Log.NORMAL_LEVEL,
                        () =>
                            string.Format(
                                "LPR pending expired while authorizing card; doorEnvironmentId={0}; correlationId={1}",
                                doorEnvironmentId,
                                pending.CorrelationId));

                    return false;
                }

                if (pending.RequiredSecondFactor != LprRequiredSecondFactor.Card
                    && pending.RequiredSecondFactor != LprRequiredSecondFactor.CardOrPin)
                {
                    return true;
                }

                if (pending.ValidCardIds == null || !pending.ValidCardIds.Contains(cardId))
                    return false;

                _lprPendingAuthorizations.Remove(doorEnvironmentId);

                CcuCore.DebugLog.Info(
                    Log.NORMAL_LEVEL,
                    () =>
                        string.Format(
                            "LPR second factor card matched; doorEnvironmentId={0}; correlationId={1}; carId={2}; plate={3}; cardId={4}",
                            doorEnvironmentId,
                            pending.CorrelationId,
                            pending.CarId,
                            pending.PlateNormalized,
                            cardId));

                return true;
            }
        }

        public bool StartLprAssistedAuthorization(Guid doorEnvironmentId, LprAuthorizationContext context)
        {
            if (doorEnvironmentId == Guid.Empty || context == null)
                return false;

            lock (_lockLprPendingAuthorizations)
            {
                var utcNow = DateTime.UtcNow;

                var expiredKeys =
                    _lprPendingAuthorizations
                        .Where(item => item.Value.ValidToUtc <= utcNow)
                        .Select(item => item.Key)
                        .ToList();

                foreach (var expiredKey in expiredKeys)
                {
                    CcuCore.DebugLog.Info(
                        Log.NORMAL_LEVEL,
                        () =>
                            string.Format(
                                "LPR pending expired; doorEnvironmentId={0}; correlationId={1}",
                                expiredKey,
                                _lprPendingAuthorizations[expiredKey].CorrelationId));

                    _lprPendingAuthorizations.Remove(expiredKey);
                }

                var pending = new LprPendingAuthorization
                {
                    DoorEnvironmentId = doorEnvironmentId,
                    CorrelationId = context.CorrelationId,
                    CarId = context.CarId,
                    PlateNormalized = context.PlateNormalized,
                    ValidCardIds = context.ValidCardIds != null
                        ? new HashSet<Guid>(context.ValidCardIds.Where(validCardId => validCardId != Guid.Empty))
                        : new HashSet<Guid>(),
                    RequiredSecondFactor = context.RequiredSecondFactor,
                    Direction = context.Direction,
                    ValidToUtc = context.ValidToUtc,
                    SourceCameraId = context.SourceCameraId
                };

                _lprPendingAuthorizations[doorEnvironmentId] = pending;

                CcuCore.DebugLog.Info(
                    Log.NORMAL_LEVEL,
                    () =>
                        string.Format(
                            "LPR pending created; doorEnvironmentId={0}; correlationId={1}; securityLevel=LPR+Card; requiredSecondFactor={2}; plate={3}; carId={4}; validCardsCount={5}; validToUtc={6:o}; sourceCameraId={7}",
                            doorEnvironmentId,
                            pending.CorrelationId,
                            pending.RequiredSecondFactor,
                            pending.PlateNormalized,
                            pending.CarId,
                            pending.ValidCardIds.Count,
                            pending.ValidToUtc,
                            pending.SourceCameraId));
            }

            PerformAsyncRequest(doorEnvironmentId,
    doorEnvironmentSettings => doorEnvironmentSettings.SetLprAuthorizationContext(context));

            return true;
        }

        public void SendAllStates()
        {
            CcuCore.DebugLog.Info(
                Log.NORMAL_LEVEL,
                () => "void DoorEnvironments.SendAllStates()");

            foreach (var doorEnvironmentSettings in _objects.ValuesSnapshot)
                doorEnvironmentSettings.SendDoorEnvironmentState();
        }

        public static void SendDoorEnvironmentState(DoorEnvironmentState doorEnvironmentState, Guid doorEnvironmentId)
        {
            CcuCore.DebugLog.Info(
                Log.NORMAL_LEVEL,
                () => String.Format(
                    "void DoorEnvironments.SendDoorEnvironmentState(DoorEnvironmentState doorEnvironmentState, Guid doorEnvironmentId): [{0}]",
                    Log.GetStringFromParameters(doorEnvironmentState, doorEnvironmentId)));

            var state = State.Unknown;

            switch (doorEnvironmentState)
            {
                case DoorEnvironmentState.Locked:
                    state = State.locked;
                    break;

                case DoorEnvironmentState.Unlocking:
                    state = State.unlocking;
                    break;

                case DoorEnvironmentState.Unlocked:
                    state = State.unlocked;
                    break;

                case DoorEnvironmentState.Opened:
                    state = State.opened;
                    break;

                case DoorEnvironmentState.Locking:
                    break;

                case DoorEnvironmentState.Intrusion:
                    state = State.intrusion;
                    break;

                case DoorEnvironmentState.Sabotage:
                    state = State.sabotage;
                    break;

                case DoorEnvironmentState.AjarPrewarning:
                    state = State.ajarPrewarning;
                    break;

                case DoorEnvironmentState.Ajar:
                    state = State.ajar;
                    break;
            }

            UpdateDeDoorAjarAlarm(
                doorEnvironmentId,
                state == State.ajar);

            UpdateDeIntrusionAlarm(
                doorEnvironmentId,
                state == State.intrusion);

            UpdateDeSabotageAlarm(
                doorEnvironmentId,
                state == State.sabotage);

            Events.ProcessEvent(
                new EventDsmStateChanged(
                    state,
                    doorEnvironmentId));
        }

        public static void UpdateDeDoorAjarAlarm(
            Guid idDoorEnvironment,
            bool isAlarm)
        {
            if (!isAlarm)
                AlarmsManager.Singleton.StopAlarm(
                    DeDoorAjarAlarm.CreateAlarmKey(
                        idDoorEnvironment));
            else
                AlarmsManager.Singleton.AddAlarm(
                    new DeDoorAjarAlarm(
                        idDoorEnvironment));
        }

        public static void UpdateDeIntrusionAlarm(
            Guid idDoorEnvironment,
            bool isAlarm)
        {
            if (!isAlarm)
                AlarmsManager.Singleton.StopAlarm(
                    DeIntrusionAlarm.CreateAlarmKey(
                        idDoorEnvironment));
            else
                AlarmsManager.Singleton.AddAlarm(
                    new DeIntrusionAlarm(
                        idDoorEnvironment));
        }

        public static void UpdateDeSabotageAlarm(
            Guid idDoorEnvironment,
            bool isAlarm)
        {
            if (!isAlarm)
                AlarmsManager.Singleton.StopAlarm(
                    DeSabotageAlarm.CreateAlarmKey(
                        idDoorEnvironment));
            else
                AlarmsManager.Singleton.AddAlarm(
                    new DeSabotageAlarm(
                        idDoorEnvironment));
        }

        public interface IStateChangedHandler
        {
            void Execute(DoorEnvironmentStateChangedArgs doorEnvironmentStateChangedArgs);
        }

        public void AddStateChangedHandler(
            Guid idDoorEnvironment,
            IStateChangedHandler stateChangedHandler)
        {
            CcuCore.DebugLog.Info(
                Log.NORMAL_LEVEL,
                () => String.Format(
                    "void DoorEnvironments.AddStateChangedHandler(Guid idDoorEnvironment, IStateChangedHandler stateChangedHandler): [{0}]",
                    Log.GetStringFromParameters(idDoorEnvironment, stateChangedHandler)));

            _objects.TryGetValue(
                idDoorEnvironment,
                (key, found, value) =>
                {
                    if (found)
                        value.AddStateChangedHandler(stateChangedHandler);
                });
        }

        public void RemoveStateChangedHandler(
            Guid idDoorEnvironment,
            IStateChangedHandler stateChangedHandler)
        {
            CcuCore.DebugLog.Info(
                Log.NORMAL_LEVEL,
                () => String.Format(
                    "void DoorEnvironments.RemoveStateChangedHandler(Guid idDoorEnvironment, IStateChangedHandler stateChangedHandler): [{0}]",
                    Log.GetStringFromParameters(
                        idDoorEnvironment,
                        stateChangedHandler)));

            _objects.TryGetValue(
                idDoorEnvironment,
                (key, found, value) =>
                {
                    if (found)
                        value.RemoveStateChangedHandler(stateChangedHandler);
                });
        }

        public DoorEnvironmentState GetDoorEnviromentState(Guid guidDoorEnvironment)
        {
            CcuCore.DebugLog.Info(
                Log.NORMAL_LEVEL,
                () => String.Format(
                    "DoorEnvironmentState DoorEnvironmentSettings.GetDoorEnviromentState(Guid guidDoorEnvironment): [{0}]",
                    Log.GetStringFromParameters(guidDoorEnvironment)));

            var result = DoorEnvironmentState.Locked;

            DoorEnvironmentSettings doorEnvironmentSettings;
            if (_objects.TryGetValue(guidDoorEnvironment, out doorEnvironmentSettings))
            {
                result = doorEnvironmentSettings.DoorEnviromentState;
            }

            CcuCore.DebugLog.Info(
                Log.NORMAL_LEVEL,
                () =>
                    String.Format(
                        "DoorEnvironmentState DoorEnvironmentSettings.GetDoorEnviromentState return {0}",
                        Log.GetStringFromParameters(result)));

            return result;
        }

        public void SuppressCardReader(Guid idDoorEnvironment, byte cardReaderAddress)
        {
            DoorEnvironmentSettings doorEnvironmentSettings;
            if (_objects.TryGetValue(idDoorEnvironment, out doorEnvironmentSettings))
            {
                doorEnvironmentSettings.SuppressCardReader(cardReaderAddress);
            }
        }

        public void LooseCardReader(Guid idDoorEnvironment, byte cardReaderAddress)
        {
            DoorEnvironmentSettings doorEnvironmentSettings;
            if (_objects.TryGetValue(idDoorEnvironment, out doorEnvironmentSettings))
            {
                doorEnvironmentSettings.LooseCardReader(cardReaderAddress);
            }
        }

        public void SetImplicitCrCode(
            Guid idDoorEnvironment,
            int cardReaderAddress,
            CRMessage implicitCrMessage,
            IList<CRMessage> followingMessages,
            bool intrusionOnlyViaLed)
        {
            PerformAsyncRequest(
                idDoorEnvironment,
                doorEnvironmentSettings =>
                    doorEnvironmentSettings.SetImplicitCrCode(
                        cardReaderAddress,
                        implicitCrMessage,
                        followingMessages,
                        intrusionOnlyViaLed));
        }

        public void MarkForceDsmStart(
            [NotNull]
            DB.DCU dcu)
        {
            var idDoorEnvironments = dcu.GuidDoorEnvironments;

            if (idDoorEnvironments == null)
                return;

            foreach (var idDoorEnvironment in idDoorEnvironments)
            {
                DoorEnvironmentSettings doorEnvironmentSettings;

                if (_objects.TryGetValue(
                    idDoorEnvironment,
                    out doorEnvironmentSettings))
                {
                    doorEnvironmentSettings.MarkForceDsmStart();
                }
            }
        }

        public void OnDcuDisconnected(
            [NotNull]
            DB.DCU dcu)
        {
            var idDoorEnvironments = dcu.GuidDoorEnvironments;

            if (idDoorEnvironments == null)
                return;

            foreach (var idDoorEnvironment in idDoorEnvironments)
            {
                DoorEnvironmentSettings doorEnvironmentSettings;

                if (_objects.TryGetValue(
                    idDoorEnvironment,
                    out doorEnvironmentSettings))
                {
                    doorEnvironmentSettings.OnDcuDisconnected();
                }
            }
        }

        public void OnDcuDsmActivationStateChanged(
            Guid idDoorEnvironment,
            bool isRunning)
        {
            DoorEnvironmentSettings doorEnvironmentSettings;

            if (_objects.TryGetValue(
                idDoorEnvironment,
                out doorEnvironmentSettings))
            {
                doorEnvironmentSettings.OnDoorEnvironmentActivationChanged(isRunning);
            }
        }

        public void OnDcuDsmStateChanged(
            Guid idDoorEnvironment,
            DoorEnvironmentState state,
            DoorEnvironmentStateDetail stateDetail,
            DoorEnvironmentAccessTrigger agSource)
        {
            PerformAsyncRequest(
                idDoorEnvironment,
                doorEnvironmentSettings => doorEnvironmentSettings.OnDoorEnvironmentStateChanged(
                    state,
                    stateDetail,
                    agSource));
        }

        public bool IsDsmStartRequired(Guid idDoorEnvironment)
        {
            DoorEnvironmentSettings doorEnvironmentSettings;

            // in case doorEnvironmentSettings do not exist yet
            // we can safely assume the corresponding DSM would require start

            return
                !_objects.TryGetValue(
                    idDoorEnvironment,
                    out doorEnvironmentSettings)
                || doorEnvironmentSettings.IsDsmStartRequired;
        }

        protected override void PrepareConfigure(DB.DoorEnvironment doorEnvironment)
        {
            Guid idCardReader = doorEnvironment.GuidCardReaderInternal;

            if (idCardReader != Guid.Empty)
                CardReaders.Singleton.PrepareDoorEnvironmentAdapter(
                    idCardReader,
                    doorEnvironment.IdDoorEnvironment);

            idCardReader = doorEnvironment.GuidCardReaderExternal;

            if (idCardReader != Guid.Empty)
                CardReaders.Singleton.PrepareDoorEnvironmentAdapter(
                    idCardReader,
                    doorEnvironment.IdDoorEnvironment);
        }

        protected override DoorEnvironmentSettings CreateNewStateAndSettingsObject(DB.DoorEnvironment dbObject)
        {
            return new DoorEnvironmentSettings(dbObject);
        }
    }
}
