using System;
using JetBrains.Annotations;

using Contal.Drivers.CardReader;
using Contal.Cgp.NCAS.Definitions;

namespace Contal.Cgp.NCAS.DoorStateMachine
{
    public enum DSMSensorState
    {
        Normal = 0,
        Alarm = 1,
        Tamper = 3,
       
    }

    public interface IDsmStateInfo
    {
        /// <summary>
        /// 
        /// </summary>
        DoorEnvironmentStateDetail DsmStateDetail { get; }

        /// <summary>
        /// 
        /// </summary>
        DoorEnvironmentState PreviousDsmState { get; }

        /// <summary>
        /// 
        /// </summary>
        DoorEnvironmentAccessTrigger AccessTrigger { get; }

        /// <summary>
        /// 
        /// </summary>
        int CardReaderId { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IDoorStateMachineEventHandler
    {
        /// <summary>
        /// HIGH PRIORITY EVENT ; 
        /// handling of the event has to be fast as possible
        /// </summary>
        void UnlockStateChanged(bool isUnlocked, DoorEnvironmentAccessTrigger dsmAccessTrigger);

        /// <summary>
        /// HIGH PRIORITY EVENT ; 
        /// handling of the event has to be fast as possible
        /// </summary>
        void IntrusionStateChanged(bool isInIntrusion);

        /// <summary>
        /// HIGH PRIORITY EVENT ; 
        /// handling of the event has to be fast as possible
        /// </summary>
        /// <param name="isInSabotage"></param>
        void SabotageStateChanged(bool isInSabotage);

        /// <summary>
        /// HIGH PRIORITY EVENT ; 
        /// handling of the event has to be fast as possible
        /// </summary>
        void AjarStateChanged(bool isInAjar);

        /// <summary>
        /// HIGH PRIORITY EVENT ; 
        /// handling of the event has to be fast as possible
        /// </summary>
        void BypassAlarmStateChanged(bool isBypassActivated);

        /// <summary>
        /// NORMAL PRIORITY EVENT ; event of this type is stacked and it's
        /// delayed by previous change handling
        /// </summary>
        void DSMStateChanged([NotNull] DsmStateInfo dsmStateInfo);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IDoorStateMachine:IDisposable
    {

        /// <summary>
        /// 
        /// </summary>
        [NotNull] 
        IDoorStateMachineEventHandler EventHandler { get; }

        /// <summary>
        /// 
        /// </summary>
        DoorEnvironmentState DsmState { get; }

        /// <summary>
        /// 
        /// </summary>
        DoorEnvironmentStateDetail DsmStateDetail { get; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="enviromentType"></param>
        void StartDSM(DoorEnviromentType enviromentType);

        /// <summary>
        /// 
        /// </summary>
        void StopDSM();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessGrantedSeverity"></param>
        /// <param name="dsmAccessGrantedSource"></param>
        void SignalAccessGranted(
            DsmAccessGrantedSeverity accessGrantedSeverity,
            DoorEnvironmentAccessTrigger dsmAccessGrantedSource);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessGrantedSeverity"></param>
        void SignalAccessGranted(DsmAccessGrantedSeverity accessGrantedSeverity);

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="invokeIntrusionAlarm"></param>
        void InvokeIntrusionAlarm(bool invokeIntrusionAlarm);*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pushButtonType"></param>
        void SetPushButton(PushButtonType pushButtonType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pushButtonType"></param>
        void UnsetPushButton(PushButtonType pushButtonType);

        // not used , replaced by IDoorStateMachineEventHandlerSetDoorOpenedState
        //void SetSensor(DSMSensorType sensorType);
        // door locked not yet supported
        void SetDoorSensorState(DSMSensorState newDoorSensorState);

        // not used , replaced by IDoorStateMachineEventHandlerSetDoorOpenedState
        //void UnsetSensor(DSMSensorType sensorType);

        // replaced by binding to IDoorStateMachineEventHandlerLockStateChanged event
        // opposite variants not yet supported
        //void SetActuator(DSMActuatorType actuatorType);

        // replaced by unbinding from IDoorStateMachineEventHandlerLockStateChanged event
        // opposite variants not yet supported
        //void UnsetActuator(DSMActuatorType actuatorType);

        // replaced by binding to IDoorStateMachineEventHandlerBypassAlarmStateChanged event
        //void SetBypassAlarm();

        // replaced by unbinding from IDoorStateMachineEventHandlerBypassAlarmStateChanged event
        //void UnsetByPassAlarm();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unlockTime"></param>
        /// <param name="openTime"></param>
        /// <param name="preAlarmTime"></param>
        /// <param name="sireneAjarDelay"></param>
        /// <param name="beforeIntrusionDelay"></param>
        void SetTimmings(
            UInt32 unlockTime, 
            UInt32 openTime, 
            UInt32 preAlarmTime, 
            UInt32 sireneAjarDelay,
            UInt32 beforeIntrusionDelay);

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
        void AssignCardReaders(
            [NotNull] CRCommunicator crCommunicator,

            int crAddressFirst, 
            CRMessage implicitCrMessageFirst, 
            CRMessage followingCrMessageFirst, 
            bool intrusionOnlyViaLedsFirst,

            int crAddressSecond, 
            CRMessage implicitCrMessageSecond,  
            CRMessage followingCrMessageSecond, 
            bool intrusionOnlyViaLedsSecond);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardReaderAddress"></param>
        void SuppressCardReader(int cardReaderAddress);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardReaderAddress"></param>
        void LooseCardReader(int cardReaderAddress);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardReaderAddress"></param>
        /// <param name="implicitCrMessage"></param>
        /// <param name="followingCrMessage"></param>
        /// <param name="intrusionOnlyViaLed"></param>
        void SetImplicitCRCode(
            int cardReaderAddress, 
            CRMessage implicitCrMessage,
            CRMessage followingCrMessage,
            bool intrusionOnlyViaLed);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pushButtonType"></param>
        /// <param name="pushButtonState"></param>
        void SetPushButtonState(PushButtonType pushButtonType, DSMSensorState pushButtonState);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isForceUnlocked"></param>
        void SetForceUnlocked(bool isForceUnlocked);

        /// <summary>
        /// 
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 
        /// </summary>
        bool DoorLockedState { get; }

        /// <summary>
        /// 
        /// </summary>
        bool AjarState { get; }

        /// <summary>
        /// 
        /// </summary>
        bool IntrusionState { get; }

        /// <summary>
        /// 
        /// </summary>
        bool IsForceUnlocked { get; }

        /// <summary>
        /// 
        /// </summary>
        DSMSensorState DoorSensorState { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IDoorStateMachineFactory
    {
        IDoorStateMachine Create([NotNull] IDoorStateMachineEventHandler eventHandler);
    }
}
