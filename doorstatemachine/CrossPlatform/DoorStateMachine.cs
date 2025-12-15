using System;
using System.Threading;
using Contal.Cgp.Globals;
using Contal.IwQuick;
using Contal.IwQuick.CrossPlatform.Data;
using Contal.IwQuick.CrossPlatform.Threads;
using JetBrains.Annotations;

using Contal.IwQuick.Data;
using Contal.IwQuick.Sys;
using Contal.Drivers.CardReader;
using Contal.Cgp.NCAS.Definitions;
// ReSharper disable once RedundantUsingDirective

namespace Contal.Cgp.NCAS.DoorStateMachine
{
    /// <summary>
    /// DSM#
    /// </summary>
    public class DoorStateMachine : ADsmTransitionMethodBridge,IDoorStateMachine
    {
        public const uint DefaultUnlockTime = 5000;
        public const uint DefaultOpenedTime = 15000;
        public const uint DefaultAjarPrewarningTimeout = 5000;

        private volatile uint _doorUnlockTime = DefaultUnlockTime; 
        private volatile uint _doorOpenedTime = DefaultOpenedTime;
        private volatile uint _ajarPrewarningTimeout = DefaultAjarPrewarningTimeout;
        private volatile uint _beforeIntrusionTime = 0;


        private volatile DoorEnvironmentState _dsmState = 
            // base state
            DoorEnvironmentState.Locked;

        /// <summary>
        /// previous Dsm state only for Stop-Start sequences
        /// </summary>
        private volatile DoorEnvironmentState _lastDsmState = DoorEnvironmentState.Unknown;

        private volatile DoorEnvironmentStateDetail _dsmStateDetail = DoorEnvironmentStateDetail.None;

        private readonly object _dsmStateLock = new object();

        private volatile bool _doorLockedState = true;
        private readonly object _syncDoorLockedState = new object();

        /// <summary>
        /// 
        /// </summary>
        public bool DoorLockedState
        {
            get
            {
                lock(_syncDoorLockedState)
                    return _doorLockedState;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isLocked"></param>
        /// <param name="forceRaiseEvent"></param>
        /// <param name="dsmAccessTrigger"></param>
        private void SetDoorLockedState(
            bool isLocked,
            bool forceRaiseEvent,
            DoorEnvironmentAccessTrigger dsmAccessTrigger)
        {
            lock (_syncDoorLockedState)
            {
                bool changed = false;

                if (isLocked != _doorLockedState)
                {
                    _doorLockedState = isLocked;
                    changed = true;

                }

                if (changed || forceRaiseEvent)
                {
                    try
                    {
                        _eventHandler.UnlockStateChanged(!isLocked, dsmAccessTrigger);
                    }
                    catch
                    {

                    }
                }
            }
        }

        private volatile bool _ajarState = false;
        private readonly object _syncAjarState = new object();

        /// <summary>
        /// 
        /// </summary>
        public bool AjarState
        {
            get
            {
                lock (_syncAjarState)
                    return _ajarState;
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isInAjar"></param>
        /// <param name="forceEventRaise"></param>
        private void SetAjarState(bool isInAjar,bool forceEventRaise)
        {
            lock (_syncAjarState)
            {
                bool changed = false;
                if (isInAjar != _ajarState)
                {
                    changed = true;
                    _ajarState = isInAjar;
                }

                if (changed || forceEventRaise)
                    try
                    {
                        _eventHandler.AjarStateChanged(isInAjar);
                    }
                    catch
                    {

                    }

            }
        }

        private volatile bool _intrusionState = false;
        private readonly object _syncIntrusionState = new object();

        /// <summary>
        /// 
        /// </summary>
        public bool IntrusionState
        {
            get
            {
                lock (_syncIntrusionState)
                    return _intrusionState;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isInIntrusion"></param>
        /// <param name="forceRaiseEvent"></param>
        private void SetIntrusionState(bool isInIntrusion, bool forceRaiseEvent)
        {
            lock (_syncIntrusionState)
            {
                bool changed = false;
                if (isInIntrusion != _intrusionState)
                {
                    _intrusionState = isInIntrusion;
                    changed = true;
                }

                if (changed || forceRaiseEvent)
                    try
                    {
                        _eventHandler.IntrusionStateChanged(isInIntrusion);
                    }
                    catch
                    {

                    }
            }
        }

        private volatile bool _bypassAlarmState = false;
        private readonly object _syncBypassAlarmState = new object();

        /// <summary>
        /// 
        /// </summary>
        public bool BypassAlarmState
        {
            get
            {
                lock (_syncIntrusionState)
                    return _intrusionState;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isBypassActivated"></param>
        /// <param name="forceRaiseEvent"></param>
        private void SetBypassAlarmState(bool isBypassActivated, bool forceRaiseEvent)
        {
            lock (_syncBypassAlarmState)
            {
                bool changed = false;
                if (isBypassActivated != _bypassAlarmState)
                {
                    _bypassAlarmState = isBypassActivated;
                    changed = true;
                }

                if (changed || forceRaiseEvent)
                    try
                    {
                        _eventHandler.BypassAlarmStateChanged(isBypassActivated);
                    }
                    catch
                    {

                    }
            }
        }

        private volatile bool _sabotageState = false;
        private readonly object _syncSabotageState = new object();

        /// <summary>
        /// 
        /// </summary>
        public bool SabotageState
        {
            get
            {
                lock (_syncSabotageState)
                    return _sabotageState;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isInSabotage"></param>
        /// <param name="forceRaiseEvent"></param>
        private void SetSabotageState(bool isInSabotage, bool forceRaiseEvent)
        {
            lock (_syncSabotageState)
            {
                bool changed = false;

                if (isInSabotage != _sabotageState)
                {
                    _sabotageState = isInSabotage;
                    changed = true;

                }

                if (changed || forceRaiseEvent)
                {
                    try
                    {

                        _eventHandler.SabotageStateChanged(isInSabotage);
                    }
                    catch
                    {

                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DoorEnvironmentState DsmState
        {
            get
            {
                if (!_sabotageState)
                {
                    lock (_dsmStateLock)
                        return _dsmState;
                }
                
                return DoorEnvironmentState.Sabotage;
            }
        }

        internal DoorEnvironmentState InternalDsmState
        {
            get
            {
                lock (_dsmStateLock)
                    return _dsmState;
            }
        }

        private enum DsmEventType
        {
            DsmStateChanged,
        }

        private sealed class DsmEvent 
            : APoolable<DsmEvent>
            , IProcessingQueueRequest<DoorStateMachine>
        {
            internal DsmEventType EventType;
            internal DsmStateInfo DsmStateInfo;

            private DsmEvent(AObjectPool<DsmEvent> objectPool)
                : base(objectPool)
            {
            }

  
            protected override bool FinalizeBeforeReturn()
            {
                try
                {
                    if (DsmStateInfo != null)
                        DsmStateInfo.Return(ref DsmStateInfo);
                }
                catch (Exception error)
                {
                    HandledExceptionAdapter.Examine(error);
                }

                return true;
            }

            public void Execute(DoorStateMachine doorStateMachine)
            {
                try
                {
                    switch (EventType)
                    {
                        case DsmEventType.DsmStateChanged:

                            doorStateMachine._eventHandler.DSMStateChanged(DsmStateInfo);

                            break;
                    }
                }
                finally
                {
                    this.TryReturn(); // returns also encapsulated DsmStateInfo
                }
            }

            public void OnError(
                DoorStateMachine param, 
                Exception error)
            {
                HandledExceptionAdapter.Examine(error);
            }
        }

        private readonly ThreadPoolQueue<DsmEvent, DoorStateMachine> _pqEvents;

        private void InvokeEvent(
            DsmEventType eventType,
            DoorEnvironmentState state,
            DoorEnvironmentStateDetail stateDetail,
            DoorEnvironmentAccessTrigger accessTrigger,
            DoorEnvironmentState previousState)
        {
            var dsmEvent = DsmEvent.Get(dsmEventPassed =>
            {
                dsmEventPassed.EventType = eventType;
                dsmEventPassed.DsmStateInfo = DsmStateInfo.Get(dsmStateInfoPassed =>
                {
                    dsmStateInfoPassed.DsmState = state;
                    dsmStateInfoPassed.DsmStateDetail = stateDetail;
                    dsmStateInfoPassed.PreviousDsmState = previousState;
                    dsmStateInfoPassed.AccessTrigger = accessTrigger;
                });
            });

            _pqEvents.Enqueue(dsmEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        private void DoorOpenedRefresh()
        {
            if (_dsmForceUnlocked)
				// disable any future state by non-force-locked loops
                PlanNextState(-1, DoorEnvironmentState.Unknown);
            else
            {
                if (_ajarPrewarningTimeout > 0)
                    PlanNextState((int) (_doorOpenedTime - _ajarPrewarningTimeout), DoorEnvironmentState.AjarPrewarning);
                else
                    PlanNextState((int) _doorOpenedTime, DoorEnvironmentState.Ajar);
            }
            
            SetBypassAlarmState(true, false);

            if (_dsmForceUnlocked)
                SetDoorLockedState(
                    false,
                    false,
                    DoorEnvironmentAccessTrigger.CardReader);
        }

        private void CorrectApasRestored(IDsmStateTransitionContext transitionContext)
        {
            // disable planned timer
            PlanNextState(-1, DoorEnvironmentState.Unknown);

            SetDsmStateDetail(DoorEnvironmentStateDetail.APASRestored, transitionContext);
            
            SetBypassAlarmState(false, false);
            SetAjarState(false, false);
        }

        private void IntroducedAjarHandling()
        {
            SetBypassAlarmState(false, false);
            SetAjarState(true, false);
        }

        

        /// <summary>
        /// should be called from synchronous actions only
        /// </summary>
        /// <param name="newDsmState"></param>
        /// <param name="informCRs"></param>
        private void SetDsmState(DoorEnvironmentState newDsmState, bool informCRs)
        {
            SetDsmState(false, newDsmState, null, informCRs, true);
        }

        /// <summary>
        /// should be called from planning timer only
        /// </summary>
        /// <param name="newDsmState"></param>
        private void SetDsmStateFromPlaningTimer(DoorEnvironmentState newDsmState)
        {
            SetDsmState(true, newDsmState, null, true, true);
        }


        /// <summary>
        /// makes the DoorEnvironmentStateDetail.None
        /// </summary>
        /// <param name="newDsmState"></param>
        /// <param name="informCRs"></param>
        private void SetDsmStateWithDetailNone(
            DoorEnvironmentState newDsmState,
            bool informCRs)
        {
            SetDsmState(false, newDsmState, DoorEnvironmentStateDetail.None, informCRs, true);
        }


        internal override void TransitionIrrelevantToLocked(IDsmStateTransitionContext transitionContext)
        {
#if DEBUG
            DebugHelper.TryBreak("DSM# Irrelevant transition to Locked. Transition " + transitionContext);
#endif 
        }

        internal override void TransitionIrrelevantToOpened(IDsmStateTransitionContext transitionContext)
        {
#if DEBUG
            DebugHelper.TryBreak("DSM# Irrelevant transition to Opened. Transition " + transitionContext);
#endif 
        }

        /// <summary>
        /// main state workflow
        /// </summary>
        /// <param name="fromPlanningTimer"></param>
        /// <param name="newDsmState"></param>
        /// <param name="dsmStateDetailOverride"></param>
        /// <param name="informCrs"></param>
        /// <param name="raiseDsmStateChangeEvent"></param>
        private void SetDsmState(
            bool fromPlanningTimer, 
            DoorEnvironmentState newDsmState,
            DoorEnvironmentStateDetail? dsmStateDetailOverride, 
            bool informCrs, 
            bool raiseDsmStateChangeEvent)
        {
            lock (_dsmStateLock)
            {
                if (_dsmState != newDsmState)
                {
                    _lastDsmState = _dsmState;
                    _dsmState = newDsmState;

                    var transitionContext = DsmStateTransitionContext.Get(
                            this, 
                            _lastDsmState,
                            newDsmState,
                            fromPlanningTimer,
                            dsmStateDetailOverride
                        );
                    

                    try
                    {
                        DsmTransitionBridge.Singleton.Proceed(transitionContext);
                    }
                    finally
                    {
                        transitionContext.TryReturn();
                    }


                    /*switch (newDsmState)
                    {
                        
                        case DoorEnvironmentState.Ajar:
                            TransitionToAjar();
                            break;*/
                        // replaced by virtual state
                        /*
                        case DoorEnvironmentState.Sabotage:
                            _dsmStateDetail = DoorEnvironmentStateDetail.AccessViolated;
                            PlanNextState(-1, DoorEnvironmentState.Unknown);
                            SetBypassAlarmState(false, false);
                            SetSabotageState(true, false);

                            SetDoorLockedState(true, false);

                            SetAjarState(false, false);
                            SetIntrusionState(false, false);
                            
                            break;*/
                    //}

                    // executed before InvokeDsmStateChanged due latency of the state projection on CRs
                    if (informCrs && _crSubmodule != null)
                        _crSubmodule.InformCrs(InternalDsmState,_dsmForceUnlocked);

                    if (raiseDsmStateChangeEvent)
                        InvokeDsmStateChanged();

                    // the AGsource gets into activation only on successful! AccessGranted 
					// and successful! push button trigger
                    // all other states should have it None
                    _lastDsmAccessTrigger = DoorEnvironmentAccessTrigger.None;
                }
            }
        }

        

        

        internal override void TransitionOpenedToAjar(IDsmStateTransitionContext transitionContext)
        {
            IntroducedAjarHandling();
        }

        internal override void TransitionIrrelevantToAjar(IDsmStateTransitionContext transitionContext)
        {
#if DEBUG
            DebugHelper.TryBreak("DSM# Ajar should not follow any other state than Opened and AjarPrewarning. Transition "+transitionContext);
#endif
        }

        internal override void TransitionOpenedToAjarPrewarning(IDsmStateTransitionContext transitionContext)
        {
            SetDsmStateDetail(DoorEnvironmentStateDetail.None, transitionContext);
            PlanNextState((int) _ajarPrewarningTimeout, DoorEnvironmentState.Ajar);
        }


        internal override void TransitionIrrelevantToAjarPrewarning(IDsmStateTransitionContext transitionContext)
        {
#if DEBUG
            DebugHelper.TryBreak("DSM# AjarPrewarning should not follow any other state than Opened. Transition "+transitionContext);
#endif
        }

        internal override void TransitionLockedToIntrusion(IDsmStateTransitionContext transitionContext)
        {
            SetDsmStateDetail(DoorEnvironmentStateDetail.AccessViolated, transitionContext);

            SetIntrusionState(true, false);
        }

        internal override void TransitionIrrelevantToIntrusion(IDsmStateTransitionContext transitionContext)
        {
#if DEBUG
            DebugHelper.TryBreak("DSM# Intrusion should be enterred only from Locked state. Transition "+transitionContext);
#endif
        }


        internal override void TransitionIrrelevantToUnlocked(IDsmStateTransitionContext transitionContext)
        {
#if DEBUG
            DebugHelper.TryBreak("DSM# Unlocked should follow only Locked state. Transition " + transitionContext);
#endif
        }

        internal override void TransitionOpenedToUnlocked(IDsmStateTransitionContext transitionContext)
        {
// relevant in forced unlock only
            SetDsmStateDetail(DoorEnvironmentStateDetail.None, transitionContext);
        }

        internal override void TransitionLockedToUnlocked(IDsmStateTransitionContext transitionContext)
        {
            SetBypassAlarmState(true, false);

            SetDsmStateDetail(DoorEnvironmentStateDetail.None, transitionContext);

            if (!_dsmForceUnlocked)
                PlanNextState((int) _doorUnlockTime, DoorEnvironmentState.Locked);


            SetDoorLockedState(false, false, _lastDsmAccessTrigger);
        }


        internal override void TransitionOpenedToLocked(IDsmStateTransitionContext transitionContext)
        {
            SetDoorLockedState(true, false, DoorEnvironmentAccessTrigger.None);

            // SetDsmDetail called inside
            CorrectApasRestored(transitionContext);
        }

        internal override void TransitionIntrusionToLocked(IDsmStateTransitionContext transitionContext)
        {
            SetDoorLockedState(true, false, DoorEnvironmentAccessTrigger.None);

            SetDsmStateDetail(DoorEnvironmentStateDetail.ViolationReturn, transitionContext);
            SetIntrusionState(false, false);
        }

        internal override void TransitionUnlockedToLocked(IDsmStateTransitionContext transitionContext)
        {
            SetDoorLockedState(true, false, DoorEnvironmentAccessTrigger.None);

            if (transitionContext.FromPlanningTimer)
            {
                SetDsmStateDetail(DoorEnvironmentStateDetail.InterruptedAccess, transitionContext);
            }
            else
            {
                // if it's a force lock
                // or return from forced unlock
                PlanNextState(-1, DoorEnvironmentState.Unknown);
                SetDsmStateDetail(DoorEnvironmentStateDetail.APASRestored, transitionContext);
            }

            // bypass should be reverted after it's known the lock is locked    
            SetBypassAlarmState(false, false);
        }

        [UsedImplicitly]
        internal override void TransitionAjarToOpened(IDsmStateTransitionContext transitionContext)
        {
            SetAjarState(false, false);
            DoorOpenedRefresh();
        }

        [UsedImplicitly]
        internal override void TransitionAjarPrewarningToOpened(IDsmStateTransitionContext transitionContext)
        {
            DoorOpenedRefresh();
        }

        internal override void TransitionIntrusionToOpened(IDsmStateTransitionContext transitionContext)
        {
            SetIntrusionState(false, false);

            // Done in doorOpenedRefresh
            //SetBypassAlarmState(true, false);

            SetDsmStateDetail(DoorEnvironmentStateDetail.ViolationReturn, transitionContext);

            // no need to unlock the lock, as the door are already opened
            //DoorLockedState = false;
            DoorOpenedRefresh();
        }

        internal override void TransitionLockedToOpened(IDsmStateTransitionContext transitionContext)
        {
// this situation when door are being opened (tolerably)
            // during StartDSM
            DoorOpenedRefresh();
        }

        internal override void TransitionUnlockedToOpened(IDsmStateTransitionContext transitionContext)
        {
            SetDsmStateDetail(DoorEnvironmentStateDetail.NormalAccess, transitionContext);

            if (!_dsmForceUnlocked)
                // locking the door immediately
                SetDoorLockedState(true, false, DoorEnvironmentAccessTrigger.None);

            DoorOpenedRefresh();
        }

        private void SetDsmStateDetail(
            DoorEnvironmentStateDetail detail, 
            [NotNull] IDsmStateTransitionContext transitionContext)
        {
            _dsmStateDetail = transitionContext.DsmStateDetailOverride != null
                ? transitionContext.DsmStateDetailOverride.Value
                : detail;
        }

        private void InvokeDsmStateChanged()
        {
            InvokeEvent(
                DsmEventType.DsmStateChanged,
                DsmState, // to evaluate also sabotage state
                _dsmStateDetail,
                _lastDsmAccessTrigger,
                _lastDsmState);
        }

        /// <summary>
        /// 
        /// </summary>
        public DoorEnvironmentStateDetail DsmStateDetail
        {
            get { lock (_dsmStateLock) return _dsmStateDetail; }
        }

        

        
        private volatile Timer _timerForPlannedStates  = null;
        private readonly object _syncTimerAndPlannedState = new object();

        /// <summary>
        /// MUST be accessed from _syncTimerAndPlannedDsmState lock section only
        /// </summary>
        private DoorEnvironmentState _plannedDsmState = DoorEnvironmentState.Locked;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="nextState"></param>
        private void PlanNextState(int timeout, DoorEnvironmentState nextState)
        {
            lock (_syncTimerAndPlannedState)
            {
                _plannedDsmState = nextState;

                if (_timerForPlannedStates == null)
                {
                    if (timeout > 0) // don't start until not necessary
                    {
                        _timerForPlannedStates = new Timer(OnTimerTick, null, timeout, Timeout.Infinite);                        
                    }
                }
                else
                {
                    _timerForPlannedStates.Change(timeout, Timeout.Infinite);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void OnTimerTick(object state)
        {
            // validates currently proceeding timer change,
            // as well as the exclusive access to the _plannedDsmState 
            if (!Monitor.TryEnter(_syncTimerAndPlannedState))
                return;

            var plannedDsmStateSnapshot = _plannedDsmState;

            // the lock section cannot span via SetDsmStateFromPlanningTimer, 
            // as it can turn into deadlock while performing SetDsmState internally
            Monitor.Exit(_syncTimerAndPlannedState);

            try
            {
                SetDsmStateFromPlaningTimer(plannedDsmStateSnapshot);
            }
            catch(Exception e)
            {
                HandledExceptionAdapter.Examine(e);
            }

        }

        private readonly IDoorStateMachineEventHandler _eventHandler;
        //private readonly CRCommunicator _crComm;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventHandler"></param>
        internal DoorStateMachine(
            [NotNull] IDoorStateMachineEventHandler eventHandler)
        {
            if (ReferenceEquals(null, eventHandler))
                throw new ArgumentNullException("eventHandler");

            _eventHandler = eventHandler;

            // intentional, so method transition cache preallocates itself
            DebugHelper.Keep(DsmTransitionBridge.Singleton);


            _pqEvents = new ThreadPoolQueue<DsmEvent, DoorStateMachine>(
                ThreadPoolGetter.Get(),
                this);
           
        }

        #region IDoorStateMachine Members

        /// <summary>
        /// 
        /// </summary>
        public IDoorStateMachineEventHandler EventHandler
        {
            get { return _eventHandler; }
        }

        private DoorEnviromentType _dsmType;
        private volatile bool _isRunning = false;
        private readonly object _isRunningLock = new object();

        /// <summary>
        /// 
        /// </summary>
        public bool IsRunning
        {
            get
            {
                lock (_isRunningLock)
                    return _isRunning;
            }
        }

// ReSharper disable once UnusedMember.Local
        private void ValidateDsmRunnning()
        {
            if (!IsRunning)
                throw new DSMException(DSMError.DSMNotStarted);
        }

        private void ValidateDsmNotRunning()
        {
            if (IsRunning)
                throw new DSMException(DSMError.DSMAlreadyStarted);
        }

        private const uint DsmStopStartAntiIntrusionTolerance = 3000;

        
        private static uint TickCountDifference(int currentTickCount, int previousTickCount)
        {
            return (uint) (currentTickCount - previousTickCount);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsmType"></param>
        public void StartDSM(DoorEnviromentType dsmType)
        {
            lock (_isRunningLock)
            {
                if (_isRunning)
                    throw new DSMException(DSMError.DSMAlreadyStarted);

                if (dsmType != DoorEnviromentType.Minimal
                    && dsmType != DoorEnviromentType.Standard
                    && dsmType != DoorEnviromentType.Turnstile)
                {
                    DebugHelper.TryBreak("DSM# Unknown DSM type " + dsmType);
                    throw new ArgumentException("Door environment of type " + dsmType + " is not supported", "dsmType");
                }


                //if (dsmType == DoorEnviromentType.Minimal &&
                //    _doorSensorState != DSMSensorState.Normal)
                //{
                //    // state of old door sensor is not relevant anymore
                //    _doorSensorState = DSMSensorState.Normal;
                //}

                _dsmType = dsmType;

                bool isNoSensorInSabotage = IsNoSensorInSabotage();

                if (_dsmForceUnlocked)
                {
                    //SetDoorLockedState(true, true);
                    //SetBypassAlarmState(false, true);
                    SetDsmStateWithDetailNone(
                        DoorSensorState == DSMSensorState.Alarm
                            ? DoorEnvironmentState.Opened
                            : DoorEnvironmentState.Unlocked,
                        isNoSensorInSabotage);
                }
                else
                {
                    switch (DoorSensorState)
                    {
                        case DSMSensorState.Alarm:
                            if ((_lastDsmState == DoorEnvironmentState.Opened ||
                                 _lastDsmState == DoorEnvironmentState.Ajar ||
                                 _lastDsmState == DoorEnvironmentState.AjarPrewarning)
                                &&
                                TickCountDifference(Environment.TickCount, _lastStopDsmTickCount) <=
                                DsmStopStartAntiIntrusionTolerance
                                )
                            {
                                SetDsmState(
                                    DoorEnvironmentState.Opened,
                                    isNoSensorInSabotage);
                            }
                            else
                            {
                                SetDsmState(
                                    DoorEnvironmentState.Intrusion,
                                    isNoSensorInSabotage);
                            }
                            break;
                        case DSMSensorState.Normal:
                            // USUAL BASIC LOCKED STATE
                            SetBypassAlarmState(false, true);
                            SetDoorLockedState(true, true, DoorEnvironmentAccessTrigger.None);
                            SetAjarState(false, true);
                            SetIntrusionState(false, true);
                            SetSabotageState(false, true);

                            if (_crSubmodule != null)
                                _crSubmodule.InformCrs(InternalDsmState,_dsmForceUnlocked);
                            break;
                        case DSMSensorState.Tamper:
                            SetBypassAlarmState(false, true);
                            SetDoorLockedState(true, true, DoorEnvironmentAccessTrigger.None);
                            SetAjarState(false, true);
                            SetIntrusionState(false, true);

                            // rest handled later
                            if (isNoSensorInSabotage)
                                isNoSensorInSabotage = false;

                            if (_crSubmodule != null)
                                _crSubmodule.InformCrs(InternalDsmState, _dsmForceUnlocked);
                            break;
                    }
                }

                // replaced by informCrs==true parameter in SetDsmState
                // InformCrs();

                if (isNoSensorInSabotage)
                {
                    InvokeDsmStateChanged();
                }
                else
                {
                    HandleSabotageState(true);
                }

                _isRunning = true;
            }
        }

        private int _lastStopDsmTickCount = 0;

        /// <summary>
        /// 
        /// </summary>
        private void StopDSM(bool useLocking, bool raiseDsmNotStartedException)
        {
            if (useLocking)
                Monitor.Enter(_isRunningLock);

            try
            {
                if (!_isRunning)
                {
                    if (raiseDsmNotStartedException)
                        throw new DSMException(DSMError.DSMNotStarted);
                    return;
                }


                _lastStopDsmTickCount = Environment.TickCount;

                PlanDelayedIntrusion(-1);
                SetSabotageState(false, false);
                SetDsmState(
                    false,
                    DoorEnvironmentState.Locked,
                    DoorEnvironmentStateDetail.None,
                    false,
                    false);

                // dont rely on change/no-change in the SetDsmState
                InvokeDsmStateChanged();

                _isRunning = false;
            }
            finally
            {
                if (useLocking)
                    Monitor.Exit(_isRunningLock);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopDSM()
        {
            StopDSM(true,true);
        }

        private volatile DoorEnvironmentAccessTrigger _lastDsmAccessTrigger = DoorEnvironmentAccessTrigger.None;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsmAccessGrantedSource"></param>
        private void CorrectDoorUnlockingOpening(DoorEnvironmentAccessTrigger dsmAccessGrantedSource)
        {
            switch (InternalDsmState)
            {
                case DoorEnvironmentState.Locked:
                    // AccessGrantedSource should be changed only when the trigger is valid
                    _lastDsmAccessTrigger = dsmAccessGrantedSource;
                    SetDsmState(DoorEnvironmentState.Unlocked,true);
                    break;
                case DoorEnvironmentState.Intrusion:
                case DoorEnvironmentState.AjarPrewarning:
                case DoorEnvironmentState.Ajar:
                    _lastDsmAccessTrigger = dsmAccessGrantedSource;
                    SetDsmState(DoorEnvironmentState.Opened,true);
                    break;

                case DoorEnvironmentState.Opened:
                    // special case, especially when ending with ForcedUnlocked
                    DoorOpenedRefresh();

                    // would filter change from opened->opened
                    //SetDsmState(DoorEnvironmentState.Opened, false);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessGrantedSource"></param>
        /// <param name="dsmAccessGrantedSource"></param>
        public void SignalAccessGranted(
            DsmAccessGrantedSeverity accessGrantedSource,
            DoorEnvironmentAccessTrigger dsmAccessGrantedSource)
        {
            if (IsRunning)
            {
                if (_crSubmodule != null)
                {
                    bool suppressAccessGranted = _crSubmodule.CheckAccessGrantedSupression();

                    if (suppressAccessGranted
                        && accessGrantedSource != DsmAccessGrantedSeverity.EmergencyCode
                        && accessGrantedSource != DsmAccessGrantedSeverity.ForceUnlockedToForceLocked)
                        return;
                }

                CorrectDoorUnlockingOpening(dsmAccessGrantedSource);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessGrantedSource"></param>
        public void SignalAccessGranted(DsmAccessGrantedSeverity accessGrantedSource)
        {
            SignalAccessGranted(accessGrantedSource, DoorEnvironmentAccessTrigger.CardReader);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pushButtonType"></param>
        public void SetPushButton(PushButtonType pushButtonType)
        {
            DsmPushButtonInfo pbi;
            _pushButtons.GetOrAddValue(
                pushButtonType,
                out pbi,
                key => new DsmPushButtonInfo(pushButtonType),
                null
                );


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pushButtonType"></param>
        public void UnsetPushButton(PushButtonType pushButtonType)
        {
            _pushButtons.Remove(pushButtonType);
        }

        /// <summary>
        /// limit direct access to this member in SetDoorSensorState only
        /// otherwise use DoorSensorState property
        /// </summary>
        private volatile DSMSensorState _doorSensorState = DSMSensorState.Normal;

        /// <summary>
        /// 
        /// </summary>
        public DSMSensorState DoorSensorState
        {
            get
            {
                lock (_syncDoorSensorState)
                {
                    if (_dsmType == DoorEnviromentType.Minimal)
                        return DSMSensorState.Normal;

                    return _doorSensorState;
                }
            }
        }

        private readonly object _syncDoorSensorState = new object();

        private volatile Timer _delayedIntrusionTimer ;
        private readonly object _syncDelayedIntrusionTimer = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        private void PlanDelayedIntrusion(int timeout)
        {
            lock (_syncDelayedIntrusionTimer)
            {
                if (null == _delayedIntrusionTimer)
                {
                    if (timeout > 0)
                    {
                        _delayedIntrusionTimer = new Timer(OnDelayedIntrusionTimer, null, timeout, Timeout.Infinite);
                    }
                    // else
                        // no need to stop something that is not started
                }
                else
                {
                    _delayedIntrusionTimer.Change(timeout, Timeout.Infinite);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void OnDelayedIntrusionTimer(object state)
        {
            if (Monitor.TryEnter(_syncDelayedIntrusionTimer))
            {
                try
                {
                    SetDsmState(DoorEnvironmentState.Intrusion,true);
                }
                catch
                {

                }
                finally
                {
                    Monitor.Exit(_syncDelayedIntrusionTimer);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newDoorSensorState"></param>
        public void SetDoorSensorState(DSMSensorState newDoorSensorState)
        {
            lock(_syncDoorSensorState)
            {
                DSMSensorState previousState;
                // cache the value even for sequences StopDSM(Minimal), SetDoorState(Balaned,DSM still minimal), StartDSM(Standard)
                if (_doorSensorState != newDoorSensorState)
                {
                    previousState = _doorSensorState;
                    _doorSensorState = newDoorSensorState;
                }
                else
                    // means no change
                    return;

                if (_dsmType == DoorEnviromentType.Minimal)
                    // no sensor in minimal door environment
                    return;

                if (IsRunning)
                {
                    switch (newDoorSensorState)
                    {
                        case DSMSensorState.Alarm:
                            if (previousState == DSMSensorState.Tamper)
                            {
                                HandleSabotageState(false);
                                // no return here, Tamper->Alarm is still relevant 
                                // action unlike the different behavior in PushButton Tamper->Alarm
                            }

                            switch (InternalDsmState)
                            {
                                case DoorEnvironmentState.Opened:
                                case DoorEnvironmentState.Ajar:
                                case DoorEnvironmentState.AjarPrewarning:
                                case DoorEnvironmentState.Intrusion:
                                    // no change
                                    break;

                                case DoorEnvironmentState.Unlocked:
                                    SetDsmState(DoorEnvironmentState.Opened, true);

                                    break;
                                case DoorEnvironmentState.Locked:

                                    if (_beforeIntrusionTime == 0)
                                        SetDsmState(DoorEnvironmentState.Intrusion, true);
                                    else
                                        PlanDelayedIntrusion((int)_beforeIntrusionTime);
                                    break;

                            }
                            break;
                        case DSMSensorState.Normal:
                            if (previousState == DSMSensorState.Tamper)
                            {
                                HandleSabotageState(false);
                            }

                            PlanDelayedIntrusion(-1);

                            switch (InternalDsmState)
                            {
                                case DoorEnvironmentState.Opened:
                                case DoorEnvironmentState.Intrusion:
                                case DoorEnvironmentState.Ajar:
                                case DoorEnvironmentState.AjarPrewarning:
// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                                    if (_dsmForceUnlocked)
                                        SetDsmState(DoorEnvironmentState.Unlocked, true);
                                    else
                                        SetDsmState(DoorEnvironmentState.Locked, true);
                                    break;
                                case DoorEnvironmentState.Locked:
                                case DoorEnvironmentState.Unlocked:
                                    // redundant
                                    break;

                            }
                            break;
                        case DSMSensorState.Tamper:
                            HandleSabotageState(true);

                            PlanDelayedIntrusion(-1);

                            switch (InternalDsmState)
                            {
                                // supressing the state, as the state is not known
                                case DoorEnvironmentState.Opened:
                                case DoorEnvironmentState.Intrusion:
                                case DoorEnvironmentState.Ajar:
                                case DoorEnvironmentState.AjarPrewarning:
                                    if (_dsmForceUnlocked)
                                        SetDsmStateWithDetailNone(
                                            DoorEnvironmentState.Unlocked, 
                                            true);
                                    else
                                        SetDsmState(DoorEnvironmentState.Locked, true);
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        private sealed class DsmPushButtonInfo
        {
            internal DSMSensorState PreviousState = DSMSensorState.Normal;

            [PublicAPI]
            internal readonly PushButtonType PushButtonType;

            internal DsmPushButtonInfo(PushButtonType pushButtonType)
            {
                PushButtonType = pushButtonType;
            }
        }

        private readonly SyncDictionary<PushButtonType, DsmPushButtonInfo> _pushButtons =
            new SyncDictionary<PushButtonType, DsmPushButtonInfo>(2);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pushButtonType"></param>
        /// <param name="pushButtonState"></param>
        public void SetPushButtonState(PushButtonType pushButtonType, DSMSensorState pushButtonState)
        {
            bool pushButtonIsUsed = false;
           
            // default value would be overwritten by the following lambda anyway
            // but not let compiler complain
            var previousState = DSMSensorState.Normal;

            _pushButtons.TryGetValue(
                pushButtonType, 
                (key, found, pushButtonInfo) =>
                {
                    if (found && pushButtonInfo != null)
                    {
                        pushButtonIsUsed = true;

                        previousState = pushButtonInfo.PreviousState;
                        pushButtonInfo.PreviousState = pushButtonState;
                    }
                }
                );

            if (!pushButtonIsUsed)
                return;

            if (previousState == pushButtonState)
                // no change
                return;

            if (!IsRunning)
                return;

            
            switch (pushButtonState)
            {
                case DSMSensorState.Alarm:
                {
                    if (previousState == DSMSensorState.Tamper)
                    {
                        HandleSabotageState(false);
                        // returning from Sabotage is not considered a relevant trigger for unlocking door
                        return;
                    }

                    if (IsRunning)
                    {
                        if (AtLeastOneCrLocked)
                            return;

                        CorrectDoorUnlockingOpening(
                            pushButtonType == PushButtonType.Internal
                                ? DoorEnvironmentAccessTrigger.InternalPushButton
                                : DoorEnvironmentAccessTrigger.ExternalPushButton);
                    }


                }
                    break;
                case DSMSensorState.Normal:
                    if (previousState == DSMSensorState.Tamper)
                    {
                        HandleSabotageState(false);
                    }
                    break;
                case DSMSensorState.Tamper:
                    HandleSabotageState(true);

                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="singleInSabotage"></param>
        private void HandleSabotageState(bool singleInSabotage)
        {
            if (singleInSabotage)
            {
                SetSabotageState(true, false);
                //SetDsmState(DoorEnvironmentState.Sabotage, false, true);
                InvokeDsmStateChanged();
            }
            else
            {
                if (SabotageState)
                {
                    var isNormal = IsNoSensorInSabotage();

                    if (isNormal)
                    {
                        SetSabotageState(false, false);
                        InvokeDsmStateChanged();
                        /*SetDsmState(
                            _doorSensorState == DSMSensorState.Alarm
                                ? DoorEnvironmentState.Intrusion
                                : DoorEnvironmentState.Locked,
                            false,
                            true);*/
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool IsNoSensorInSabotage()
        {
            bool isNormal = 
                _dsmType == DoorEnviromentType.Minimal ||
                _doorSensorState != DSMSensorState.Tamper;

            if (isNormal)
            {
                var pushButtonsSnapshot = _pushButtons.ValuesSnapshot;
                foreach (var pbi in pushButtonsSnapshot)
                {
                    if (pbi.PreviousState == DSMSensorState.Tamper)
                    {
                        isNormal = false;
                        break;
                    }
                }
            }
            return isNormal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unlockTime"></param>
        /// <param name="openTime"></param>
        /// <param name="preAlarmTime"></param>
        /// <param name="sireneAjarDelay"></param>
        /// <param name="beforeIntrusionDelay"></param>
        public void SetTimmings(
            uint unlockTime,
            uint openTime,
            uint preAlarmTime,
            uint sireneAjarDelay,
            uint beforeIntrusionDelay)
        {
            _doorUnlockTime = unlockTime;
            _doorOpenedTime = openTime;
            _ajarPrewarningTimeout = preAlarmTime;
            _beforeIntrusionTime = beforeIntrusionDelay;
        }

        //private readonly SyncDictionary<int,DsmCrConfiguration>

        // moved to defaultdsmfactory
        //private const string DSM_CR_INFO_MARK = "DSM#";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crCommunicator"></param>
        /// <param name="crAddressFirst"></param>
        /// <param name="implicitCrMessageFirst"></param>
        /// <param name="followingCrMessageFirst"></param>
        /// <param name="intrusionOnlyViaLedsFirst"></param>
        /// <param name="crAddressSecond"></param>
        /// <param name="implicitCrMessageSecond"></param>
        /// <param name="followingCrMessageSecond"></param>
        /// <param name="intrusionOnlyViaLedsSecond"></param>
        public void AssignCardReaders(
            CRCommunicator crCommunicator,

            int crAddressFirst, 
            CRMessage implicitCrMessageFirst, 
            CRMessage followingCrMessageFirst, 
            bool intrusionOnlyViaLedsFirst, 

            int crAddressSecond, 
            CRMessage implicitCrMessageSecond, 
            CRMessage followingCrMessageSecond, 
            bool intrusionOnlyViaLedsSecond)
        {
            ValidateDsmNotRunning();

            if (_crSubmodule == null)
                lock (_syncCrSubmodule)
                {
                    if (_crSubmodule == null)
                        _crSubmodule = new DsmCrSubmodule(this, crCommunicator);
                }

            _crSubmodule.AssignCardReaders(
                crAddressFirst,
                implicitCrMessageFirst,
                followingCrMessageFirst,
                intrusionOnlyViaLedsFirst,
                crAddressSecond,
                implicitCrMessageSecond,
                followingCrMessageSecond,
                intrusionOnlyViaLedsSecond);
                

            HandleForcedUnlockedOrForceLocked(true);
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardReaderAddress"></param>
        /// <exception cref="DSMException">if card reader is not assigned</exception>
        public void SuppressCardReader(int cardReaderAddress)
        {
            if (_crSubmodule == null)
                throw new DSMException(DSMError.CardReaderNotAssigned);
            
            var dsmCrInfo = _crSubmodule.GetDsmCrInfo(cardReaderAddress,true);

            dsmCrInfo.Supressed = true;
        }

        
        [CanBeNull]
        private volatile DsmCrSubmodule _crSubmodule;
        private readonly object _syncCrSubmodule = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardReaderAddress"></param>
        /// <exception cref="DSMException"></exception>
        public void LooseCardReader(int cardReaderAddress)
        {
            if (_crSubmodule == null)
                throw new DSMException(DSMError.CardReaderNotAssigned);

            _crSubmodule.LooseCardReader(
                cardReaderAddress, 
                true, 
                true, 
                false,
                InternalDsmState,
                _dsmForceUnlocked);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardReaderAddress"></param>
        /// <param name="crMessage"></param>
        /// <param name="intrusionOnlyViaLed"></param>
        /// <exception cref="DSMException">if card reader is not assigned</exception>
        public void SetImplicitCRCode(
            int cardReaderAddress,
            [NotNull] CRMessage implicitCrMessage,
            [CanBeNull] CRMessage followingMessages,
            bool intrusionOnlyViaLed)
        {
            if (_crSubmodule == null)
                throw new DSMException(DSMError.CardReaderNotAssigned);

            var dsmCrInfo = _crSubmodule.SetImplicitCRCode(
                cardReaderAddress,
                implicitCrMessage,
                followingMessages,
                intrusionOnlyViaLed);

            if (dsmCrInfo != null)
            {
                int unlockedResult = HandleForcedUnlockedOrForceLocked(false);


                if (IsRunning)
                {
                    if (unlockedResult >= 0)
                        _crSubmodule.InformCrs(InternalDsmState, _dsmForceUnlocked);
                    else if (InternalDsmState == DoorEnvironmentState.Unlocked
                             || InternalDsmState == DoorEnvironmentState.Locked)
                        _crSubmodule.InformCr(dsmCrInfo, InternalDsmState, _dsmForceUnlocked);
                }
            }
        }


        private bool AtLeastOneCrLocked
        {
            get
            {
                if (_crSubmodule == null)
                    return false;

                return _crSubmodule.AtLeastOneCrLocked;
            }
        }

        private int HandleForcedUnlockedOrForceLocked(bool refreshCrs)
        {
            bool forceUnlockedByCr;
            // no need to verify the null, as the Handle.. is called within sections already checking this condition
            // ReSharper disable once PossibleNullReferenceException
            _crSubmodule.CheckForceLockedOrForceUnlocked(out forceUnlockedByCr);

            return SetForceUnlocked(forceUnlockedByCr,refreshCrs);

        }

        private volatile bool _dsmForceUnlocked = false;

        /// <summary>
        /// 
        /// </summary>
        public bool IsForceUnlocked
        {
            get { return _dsmForceUnlocked; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isForceUnlocked"></param>
        public void SetForceUnlocked(bool isForceUnlocked)
        {
            SetForceUnlocked(isForceUnlocked, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isForceUnlocked"></param>
        /// <param name="refreshCrs"></param>
        /// <returns></returns>
        private int SetForceUnlocked(bool isForceUnlocked, bool refreshCrs)
        {
            if (IsRunning)
            {
                if (_dsmForceUnlocked != isForceUnlocked)
                {

                    if (isForceUnlocked)
                    {
                        _dsmForceUnlocked = true;

                        if (InternalDsmState == DoorEnvironmentState.Locked)
                        {
                            SetDsmStateWithDetailNone(
                                DoorEnvironmentState.Unlocked,
                                refreshCrs);
                        }
                        else if (InternalDsmState == DoorEnvironmentState.Opened ||
                                 InternalDsmState == DoorEnvironmentState.Intrusion ||
                                 InternalDsmState == DoorEnvironmentState.AjarPrewarning ||
                                 InternalDsmState == DoorEnvironmentState.Ajar)
                        {
                            SignalAccessGranted(DsmAccessGrantedSeverity.NormalCard);
                        }
                        else 
                            if (InternalDsmState == DoorEnvironmentState.Unlocked)
                            {
                                // to prevent DSM# from reverting back to locked
                                PlanNextState(-1, DoorEnvironmentState.Unknown);
                            }

                        //Debug.WriteLine("HandleUnlocked : 1");
                        
                        return 1;
                    }
                    
                    _dsmForceUnlocked = false;

                    if (InternalDsmState == DoorEnvironmentState.Opened)
                    {
                        // must be forced to revert from always unlocked to locked
                        // as the door are already opened
                        SetDoorLockedState(
                            true,
                            false,
                            DoorEnvironmentAccessTrigger.CardReader);

                        // must be emergency card, otherwise the 
                        SignalAccessGranted(DsmAccessGrantedSeverity.ForceUnlockedToForceLocked);
                    }
                    else if (InternalDsmState == DoorEnvironmentState.Unlocked)
                    {
                        SetDsmState(
                            DoorEnvironmentState.Locked, 
                            refreshCrs);

                        // replaced by informCrs parameter in SetDsmState
                        //if (refreshCrs)
                        //    InformCrs();
                    }

                    //Debug.WriteLine("HandleUnlocked : 0");
                    return 0;
                }
            }

            _dsmForceUnlocked = isForceUnlocked;

            //Debug.WriteLine("HandleUnlocked : nochange/"+_dsmForceUnlocked);
            return -1;
        }

        #endregion

        protected override void InternalDispose(bool isExplicitDispose)
        {
            StopDSM(isExplicitDispose,false);
            _pqEvents.Dispose(isExplicitDispose);
        }
    }
}
