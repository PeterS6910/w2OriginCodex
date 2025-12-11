using System;
using System.Threading;
using Contal.IwQuick;
using JetBrains.Annotations;
using Contal.IwQuick.Data;
using Contal.Drivers.CardReader;
using Contal.Cgp.NCAS.Definitions;

// ReSharper disable once RedundantUsingDirective
using System.Diagnostics;

namespace Contal.Cgp.NCAS.DoorStateMachine
{
    internal class DsmCrSubmodule: ADisposable, ICardReaderEventHandler
    {
        private DoorStateMachine _dsm;

        private CRCommunicator _crComm;
        internal CRCommunicator CrCommunicator { get { return _crComm; }}

        internal DsmCrSubmodule(
            [NotNull] DoorStateMachine dsm, 
            [NotNull] CRCommunicator crCommunicator)
        {
            _crComm = crCommunicator;
            crCommunicator.EventHandler.Add(this);

            _dsm = dsm;
        }


        private volatile bool _atLeastOneCrLocked = false;
        /// <summary>
        /// 
        /// </summary>
        internal bool AtLeastOneCrLocked
        {
            get { return _atLeastOneCrLocked; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsmCrConfiguration"></param>
        /// <param name="icon4"></param>
        /// <param name="buttonIsOn4"></param>
        private void DisplayInfoTab(
            [NotNull] DsmCrConfiguration dsmCrConfiguration,
            CRInfoTabIcon icon4,
            bool buttonIsOn4)
        {
            _crComm.DisplayCommands.DisplayInfoTab(
                dsmCrConfiguration.CardReader,
                CRInfoTabIcon.NO_CHANGE, false,
                CRInfoTabIcon.NO_CHANGE, false,
                CRInfoTabIcon.NO_CHANGE, false,
                icon4, buttonIsOn4
                );
        }

        private void TransferFollowingCrMessage([NotNull] DsmCrConfiguration dsmCrConfiguration)
        {
            if (!ReferenceEquals(dsmCrConfiguration.FollowingCrMessage, null))
            {
                // double check, however, this is already overwritten in SetImplicitCrCode
                dsmCrConfiguration.FollowingCrMessage.Address = dsmCrConfiguration.CardReader.Address;

                _crComm.SendMessage(dsmCrConfiguration.FollowingCrMessage);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsmCrConfiguration"></param>
        /// <param name="revokeDisplayInfoTab"></param>
        private void TransferImplicitCrCommand(
            [NotNull] DsmCrConfiguration dsmCrConfiguration,
            bool revokeDisplayInfoTab)
        {
            if (dsmCrConfiguration.ImplicitCrMessage.MessageCode == CRMessageCode.DOOR_UNLOCKED
                && _atLeastOneCrLocked)
            {
                _crComm.AccessCommands.DoorLocked(dsmCrConfiguration.CardReader);
            }
            else
            {
                _crComm.SendMessage(dsmCrConfiguration.ImplicitCrMessage);
            }

            TransferFollowingCrMessage(dsmCrConfiguration);

            if (revokeDisplayInfoTab)
            {
                DisplayInfoTab(dsmCrConfiguration,
                    CRInfoTabIcon.EMPTY, false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsmCrConfiguration"></param>
        private void TransferImplicitCrCommand(
            [NotNull] DsmCrConfiguration dsmCrConfiguration)
        {
            TransferImplicitCrCommand(dsmCrConfiguration, false);
        }

        private delegate void DInformCr(
            DsmCrConfiguration dsmCrConfiguration,
            bool dsmForceUnlocked
            
            );

        private TinyCache<DoorEnvironmentState, DInformCr> _informCrMethodCache;
        private readonly object _syncInformCrMethodCache = new object();

        private TinyCache<DoorEnvironmentState, DInformCr> InformCrMethodCache
        {
            get
            {
                if (null == _informCrMethodCache)
                    lock (_syncInformCrMethodCache)
                    {
                        if (null == _informCrMethodCache)
                        {
                            var informCrMethodCache =
                                new TinyCache<DoorEnvironmentState, DInformCr>((int)DoorEnvironmentState.Unknown)
                                {
                                    {DoorEnvironmentState.Locked, CrInformWhenLocked},
                                    {DoorEnvironmentState.Unlocked, CrInformWhenUnlocked},
                                    {DoorEnvironmentState.Opened, CrInformWhenOpened},
                                    {DoorEnvironmentState.Intrusion, CrInformWhenIntrusion},
                                    {DoorEnvironmentState.AjarPrewarning, CrInformWhenAjarPrewarning},
                                    {DoorEnvironmentState.Ajar, CrInformWhenAjar}
                                };


                            Thread.MemoryBarrier();
                            _informCrMethodCache = informCrMethodCache;
                        }
                    }

                return _informCrMethodCache;
            }
        }

        private readonly SyncDictionary<int, DsmCrConfiguration> _assignedCardReaders = new SyncDictionary<int, DsmCrConfiguration>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crAddressFirst"></param>
        /// <param name="implicitCrMessageFirst"></param>
        /// <param name="followingCrMessageFirst"></param>
        /// <param name="intrusionOnlyViaLedsFirst"></param>
        /// <param name="crAddressSecond"></param>
        /// <param name="implicitCrMessageSecond"></param>
        /// <param name="followingCrMessageSecond"></param>
        /// <param name="intrusionOnlyViaLedsSecond"></param>
        internal void AssignCardReaders(
            int crAddressFirst,
            CRMessage implicitCrMessageFirst,
            CRMessage followingCrMessageFirst,
            bool intrusionOnlyViaLedsFirst,

            int crAddressSecond,
            CRMessage implicitCrMessageSecond,
            CRMessage followingCrMessageSecond,
            bool intrusionOnlyViaLedsSecond)
        {

            _assignedCardReaders.ForEach((assignedCrAddress, assignedDsmCrInfo) =>
            {
                if (assignedDsmCrInfo != null)
                    try
                    {
                        assignedDsmCrInfo.CardReader.ParametersByInt.Unset(DefaultDsmFactory.DsmCrInfoMark);
                    }
                    catch(DoesNotExistException)
                    {
                    }
            });
            _assignedCardReaders.Clear();

            for (int i = 1; i <= 2; i++)
            {
                int crAddress = i == 1 ? crAddressFirst : crAddressSecond;

                if (crAddress < 0)
                    continue;

                var crParamName = (i == 1 ? "crAddressFirst" : "crAddressSecond");
                var implicitCrMessage = (i == 1 ? implicitCrMessageFirst : implicitCrMessageSecond);
                var followingCrMessage = (i == 1 ? followingCrMessageFirst : followingCrMessageSecond);
                var intrusionOnlyViaLeds = (i == 1 ? intrusionOnlyViaLedsFirst : intrusionOnlyViaLedsSecond);


                CardReader cardReader = _crComm.GetCardReader((byte)crAddress);

                if (cardReader == null)
                    throw new ArgumentException(
                        "Card reader with address " + crAddress + " cannot be found",
                        crParamName);

                bool registerredNew = false;

                var dsmRegisterredInCr =
                    cardReader.ParametersByInt.Get<IDoorStateMachine>(
                    DefaultDsmFactory.DsmCrInfoMark,
                        key =>
                        {
                            registerredNew = true;
                            return _dsm;

                        });


                if (!registerredNew &&
                    !ReferenceEquals(dsmRegisterredInCr, null) &&
                    !ReferenceEquals(dsmRegisterredInCr, _dsm) &&
                    dsmRegisterredInCr.IsRunning)
                {
                    throw new ArgumentException(
                        "Card reader with address " + crAddress +
                        " already used by other running door environment",
                        crParamName);
                }


                // overwrite the parameter reference   
                var dsmCrInfo = new DsmCrConfiguration(cardReader, implicitCrMessage)
                {
                    DsmCrIndex = i,
                    FollowingCrMessage = followingCrMessage,
                    IntrusionOnlyViaLeds = intrusionOnlyViaLeds

                };

                _assignedCardReaders[cardReader.Address] = dsmCrInfo;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardReaderAddress"></param>
        /// <param name="throwExceptions">if true, return should always be NotNull</param>
        /// <exception cref="DSMException"></exception>
        /// <returns></returns>
        internal DsmCrConfiguration GetDsmCrInfo(int cardReaderAddress, bool throwExceptions)
        {
            DsmCrConfiguration dsmCrConfiguration;

            // this condition is enough, no need to check if exists in CrComm separately,
            // because if AssignCRs did not succeed, it cannot be in _assignedCardReaders
            if ((!_assignedCardReaders.TryGetValue(cardReaderAddress, out dsmCrConfiguration) ||
                dsmCrConfiguration == null)
                && throwExceptions)
                throw new DSMException(DSMError.CardReaderNotAssigned);

            return dsmCrConfiguration;
        }


        internal void LooseCardReader(
            int cardReaderAddress,
            bool revertSupress,
            bool raiseExceptions,
            bool isFromTamperReturn,
            DoorEnvironmentState dsmState,
            bool dsmForceUnlocked)
        {
            var dsmCrInfo = GetDsmCrInfo(cardReaderAddress, raiseExceptions);

            if (dsmCrInfo != null)
            //if (DsmCrConfiguration.Supressed) // LooseIsUsed also for refreshes, that's why condition commented
            {
                if (revertSupress)
                    dsmCrInfo.Supressed = false;

                if (isFromTamperReturn)
                    dsmCrInfo.MasterMessageTag = CRDirectCommunicator.TamperReturnTag;

                InformCr(dsmCrInfo, dsmState, dsmForceUnlocked);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsmCrConfiguration"></param>
        /// <param name="dsmState"></param>
        /// <param name="dsmForceUnlocked"></param>
        internal void InformCr(
            DsmCrConfiguration dsmCrConfiguration,
            DoorEnvironmentState dsmState,
            bool dsmForceUnlocked)
        {
            if (dsmCrConfiguration == null)
                return;

            if (dsmCrConfiguration.Supressed)
                return;

            DInformCr method;

            if (InformCrMethodCache.TryGetValue(dsmState, out method)
                && method != null)
            {
                method(dsmCrConfiguration,dsmForceUnlocked);

                Debug.WriteLine("INFORM CR " + dsmCrConfiguration.CardReader.Address + " " + dsmState);
            }

        }

        private void CrInformWhenLocked(DsmCrConfiguration dsmCrConfiguration, bool dsmForceUnlocked)
        {
            TransferImplicitCrCommand(dsmCrConfiguration, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsmCrConfiguration"></param>
        /// <param name="dsmForceUnlocked"></param>
        private void CrInformWhenAjar([NotNull] DsmCrConfiguration dsmCrConfiguration, bool dsmForceUnlocked)
        {
            if (dsmCrConfiguration.IntrusionOnlyViaLeds)
            {
                TransferImplicitCrCommand(dsmCrConfiguration);
                _crComm.ControlCommands.IndicatorAnnouncement(
                    dsmCrConfiguration.CardReader,
                    IndicatorMode.HighFrequency,
                    IndicatorMode.Off,
                    IndicatorMode.On,
                    IndicatorMode.On);
            }
            else
            {
                _crComm.AccessCommands.AlarmDoorAjar(dsmCrConfiguration.CardReader);
            }

            InfoTabCloseDoor(dsmCrConfiguration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsmCrConfiguration"></param>
        private void InfoTabCloseDoor(DsmCrConfiguration dsmCrConfiguration)
        {
            DisplayInfoTab(
                dsmCrConfiguration,
                CRInfoTabIcon.CLOSE_DOOR, true
                );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsmCrConfiguration"></param>
        /// <param name="dsmForceUnlocked"></param>
        private void CrInformWhenAjarPrewarning([NotNull] DsmCrConfiguration dsmCrConfiguration, bool dsmForceUnlocked)
        {
            if (dsmCrConfiguration.IntrusionOnlyViaLeds)
            {
                TransferImplicitCrCommand(dsmCrConfiguration);
                _crComm.ControlCommands.IndicatorAnnouncement(
                    dsmCrConfiguration.CardReader,
                    IndicatorMode.Off,
                    IndicatorMode.Off,
                    IndicatorMode.HighFrequency,
                    IndicatorMode.HighFrequency);
            }
            else
                _crComm.AccessCommands.WarningDoorAjar(dsmCrConfiguration.CardReader);

            InfoTabCloseDoor(dsmCrConfiguration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsmCrConfiguration"></param>
        /// <param name="dsmForceUnlocked"></param>
        private void CrInformWhenIntrusion([NotNull] DsmCrConfiguration dsmCrConfiguration, bool dsmForceUnlocked)
        {
            if (dsmCrConfiguration.IntrusionOnlyViaLeds)
            {
                TransferImplicitCrCommand(dsmCrConfiguration);
                _crComm.ControlCommands.IndicatorAnnouncement(
                    dsmCrConfiguration.CardReader,
                    IndicatorMode.HighFrequency,
                    IndicatorMode.Off,
                    IndicatorMode.On,
                    IndicatorMode.On);
            }
            else
                _crComm.AccessCommands.AlarmIntrusion(dsmCrConfiguration.CardReader, true, true);


            InfoTabCloseDoor(dsmCrConfiguration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsmCrConfiguration"></param>
        /// <param name="dsmForceUnlocked"></param>
        private void CrInformWhenOpened([NotNull] DsmCrConfiguration dsmCrConfiguration, bool dsmForceUnlocked)
        {
            _crComm.AccessCommands.DoorOpened(dsmCrConfiguration.CardReader);

            DisplayInfoTab(
                dsmCrConfiguration,
                CRInfoTabIcon.DOORS_OPENED_CLOSE, true
                );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsmCrConfiguration"></param>
        /// <param name="dsmForceUnlocked"></param>
        private void CrInformWhenUnlocked([NotNull] DsmCrConfiguration dsmCrConfiguration, bool dsmForceUnlocked)
        {
            if (dsmForceUnlocked)
            {
                if (dsmCrConfiguration.ImplicitCrMessage.MessageCode != CRMessageCode.ALARM_AREA_TMP_UNSET)
                {

                    _crComm.AccessCommands.DoorUnlocked(dsmCrConfiguration.CardReader);

                    DisplayInfoTab(
                        dsmCrConfiguration,
                        CRInfoTabIcon.DOORS_OPENED_CLOSE, false
                        );

                    TransferFollowingCrMessage(dsmCrConfiguration);
                }
                else
                {
                    if (dsmCrConfiguration.ImplicitCrMessage.OptionalDataLength > 0)
                        dsmCrConfiguration.ImplicitCrMessage.OptionalData[0] |=
                            (int) CRAATmpUnsetFlag.DontApplyMasterCommand;

                    _crComm.AccessCommands.DoorUnlocked(dsmCrConfiguration.CardReader);

                    // makes transcription of the virtual commands
                    TransferImplicitCrCommand(dsmCrConfiguration);
                }
            }
            else
            {
                _crComm.AccessCommands.DoorUnlocked(dsmCrConfiguration.CardReader);

                DisplayInfoTab(
                    dsmCrConfiguration,
                    CRInfoTabIcon.DOORS_OPENED_CLOSE, false
                    );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardReaderAddress"></param>
        /// <param name="implicitCrMessage"></param>
        /// <param name="followingCrMessage"></param>
        /// <param name="intrusionOnlyViaLed"></param>
        /// <exception cref="DSMException">if card reader is not assigned</exception>
        internal DsmCrConfiguration SetImplicitCRCode(
            int cardReaderAddress,
            [NotNull] CRMessage implicitCrMessage,
            [CanBeNull] CRMessage followingCrMessage,
            bool intrusionOnlyViaLed)
        {
            var dsmCrInfo = GetDsmCrInfo(cardReaderAddress, true);


            bool changed = dsmCrInfo.SetImplicitCode(implicitCrMessage);

            // hack to avoid unnecessary blinking on CR
            // as the DsmCrConfiguration keeps the augmented address
            // however the supplied followingCrMessage or its submessages might
            // contain zero address
            if (null != followingCrMessage)
                followingCrMessage.Address = (byte)cardReaderAddress;

            if (!ReferenceEquals(dsmCrInfo.FollowingCrMessage, followingCrMessage) &&
                (followingCrMessage == null || !followingCrMessage.Equals(dsmCrInfo.FollowingCrMessage))
                )
            {
                dsmCrInfo.FollowingCrMessage = followingCrMessage;
                changed = true;
            }

            if (dsmCrInfo.IntrusionOnlyViaLeds != intrusionOnlyViaLed)
            {
                dsmCrInfo.IntrusionOnlyViaLeds = intrusionOnlyViaLed;
                changed = true;
            }

            return changed ? dsmCrInfo : null;
        }

        internal void InformCrs(DoorEnvironmentState dsmState,bool dsmForceUnlocked)
        {
            var dsmCrInfos = _assignedCardReaders.ValuesSnapshot;
            foreach (var dsmCrInfo in dsmCrInfos)
            {
                try
                {
                    InformCr(dsmCrInfo, dsmState, dsmForceUnlocked);
                }
                catch
                {

                }
            }
        }

        internal bool CheckForceLockedOrForceUnlocked(out bool forceUnlocked)
        {
            forceUnlocked = false;
            bool atLeastOneCrLocked = false;

            foreach (var dsmCrInfo in _assignedCardReaders.Values)
            {
                if (dsmCrInfo != null)
                {
                    if (dsmCrInfo.ImplicitCrMessage.MessageCode == CRMessageCode.DOOR_UNLOCKED)
                    {
                        forceUnlocked = true;

                        // cannot stop here, there still can be other 
                        // CR reverting the forcedUnlock back due its locking state
                        //break;
                    }

                    if (dsmCrInfo.ForceLocked)
                    {
                        // denoting any unlocked possibility 
                        // by higher SL
                        atLeastOneCrLocked = true;
                        // ReSharper disable once RedundantAssignment
                        forceUnlocked = false;
                        break;
                    }

                }
            }

            // atomic assignment
            _atLeastOneCrLocked = atLeastOneCrLocked;

            return atLeastOneCrLocked;
        }

        internal bool CheckAccessGrantedSupression()
        {
            bool suppressAccessGranted = false;

            if (_atLeastOneCrLocked)
            {
                suppressAccessGranted = true;

                // occurence of carddata from CR should be
                // also filtered by the SL applied on CR 
                // 
                // e.g. AAset/ Locked should not produce card data on CR
                // thus crAddress identification is not always needed

                // no need for snapshot, when IsRunning, 
                // the _assignedCardReaders shouldn't change 
                // in key/value reference set

                foreach (var dsmCrInfo in _assignedCardReaders.Values)
                {
                    if (dsmCrInfo != null)
                    {
                        if (!dsmCrInfo.ForceLocked && dsmCrInfo.ImplicitCrMessage.MessageCode != CRMessageCode.DOOR_UNLOCKED)
                        {
                            suppressAccessGranted = false;
                            break;
                        }
                    }
                }
            }

            return suppressAccessGranted;

        }

        protected override void InternalDispose(bool isExplicitDispose)
        {
            _crComm.EventHandler.Remove(this);
            _crComm = null;
            _dsm = null;
        }

        void ICardReaderEventHandler.CardReaderIndirectMessageToSend(CardReader cr, CRMessage message, bool highPriority)
        {
        }

        void ICardReaderEventHandler.CardReaderFnBoxTimedOut(CardReader cr)
        {
        }

        void ICardReaderEventHandler.CardReaderCodeTimedOut(CardReader cr)
        {
        }

        void ICardReaderEventHandler.CardReaderCodeSpecified(CardReader cr, string codeData)
        {
        }

        void ICardReaderEventHandler.CardReaderMenuCancelled(CardReader cr, bool byOtherCommand)
        {
        }

        void ICardReaderEventHandler.CardReaderMenuTimedOut(CardReader cr)
        {
        }

        void ICardReaderEventHandler.CardReaderMenuItemSelected(CardReader cr, int itemIndex, string itemText)
        {
        }

        void ICardReaderEventHandler.CardReaderMenuItemSelected(CardReader cr, int itemReturnCode)
        {
            
        }

        void ICardReaderEventHandler.CardReaderNumericKeyPressed(CardReader cr, byte numeric)
        {
        }

        void ICardReaderEventHandler.CardReaderSabotageStateChanged(CardReader cr, bool tamperOn)
        {
            if (!tamperOn && _dsm.IsRunning)
            {
                Debug.WriteLine("CR Tamper return : Losing card reader");
                LooseCardReader(cr.Address, false, false, true, _dsm.InternalDsmState,_dsm.IsForceUnlocked);
            }
        }

        void ICardReaderEventHandler.CardReaderSpecialKeyPressed(CardReader cr, CRSpecialKey specialKey)
        {
        }

        void ICardReaderEventHandler.CardReaderOnlineStateChanged(CardReader cr, bool isOnline)
        {
            LooseCardReader(cr.Address, true, false, false,_dsm.InternalDsmState, _dsm.IsForceUnlocked);
        }

        void ICardReaderEventHandler.CardReaderCardSwiped(CardReader cr, string cardData, int cardSystemNumber)
        {
        }

        void ICardReaderEventHandler.CardReaderQueryDbStampResponse(CardReader cr, byte[] queryDbStamp)
        {
        }

        void ICardReaderEventHandler.CardReaderConfigurationResponse(CardReader cr, CRConfigurationResult result, byte dbStamp)
        {
        }

        void ICardReaderEventHandler.CardReaderConfigurationResponse(CardReader cr, CRConfigurationResult result, uint crUniqueId, ushort configId)
        {
            
        }

        void ICardReaderEventHandler.CardReaderResetOccured(CardReader cr)
        {
            
        }

        void ICardReaderEventHandler.CardReaderCommandFailed(CardReader cr, CRMessageCode messageCode)
        {
            
        }

        void ICardReaderEventHandler.CardReaderCommandTimedOut(CardReader cr, CRMessageCode messageCode, int retryToCome)
        {
            
        }

        void ICardReaderEventHandler.CardReaderServiceResultOccured(CardReader cr, CRServiceResult serviceResult)
        {
            
        }

        void ICardReaderEventHandler.CardReaderUpgradeResult(CardReader cr, CRUpgradeResult result, Exception error)
        {
            
        }

        void ICardReaderEventHandler.CardReaderUpgradeProgress(CardReader cr, int progress)
        {
           
        }

        void ICardReaderEventHandler.CardReaderCountryCodeConfirmed(CardReader cr)
        {
        }

        void ICardReaderEventHandler.CardReaderGraphicalMenuUpdated(CardReader cr)
        {
            
        }

        void ICardReaderEventHandler.CardReaderModeChanged(CardReader cr, CRMode readerMode)
        {
        }

        void ICardReaderEventHandler.CardReaderCardWriterResponse(CardReader cr, MifareCardWriteResult writeResult, MifareCardWriteError? writeError)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        internal sealed class DsmCrConfiguration
        {
// ReSharper disable once NotAccessedField.Local
            internal int DsmCrIndex;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="cardReader"></param>
            /// <param name="implicitCrMessage"></param>
            internal DsmCrConfiguration(
                [NotNull] CardReader cardReader, 
                CRMessage implicitCrMessage)
            {
                CardReader = cardReader;

                SetImplicitCode(implicitCrMessage);
            }

            private CRMessage _implicitCrMessage;

            /// <summary>
            /// 
            /// </summary>
            internal CRMessage ImplicitCrMessage
            {
                get { return _implicitCrMessage; }
                
            }

            private const int CrLockedWithException = 0x30;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="newImplicitCrMessage"></param>
            /// <returns></returns>
            internal bool SetImplicitCode(CRMessage newImplicitCrMessage)
            {
                if (!newImplicitCrMessage.Equals(
                    _implicitCrMessage,
                    false))
                {
                    var newImplicitCode = newImplicitCrMessage.MessageCode;
                    var implicitOptionalData = newImplicitCrMessage.OptionalDataLength > 0
                        ? newImplicitCrMessage.OptionalData[0]
                        : -1;

                    if ((newImplicitCode == CRMessageCode.DOOR_LOCKED ||
                         newImplicitCode == CRMessageCode.ALARM_AREA_IS_SET ||
                         newImplicitCode == CRMessageCode.ALARM_AREA_IN_ALARM) &&
                        (implicitOptionalData < 0 ||
                         (implicitOptionalData >= 0 &&
                          ((implicitOptionalData & CrLockedWithException) != CrLockedWithException))))
                    {
                        _forceLocked = true;
                    }
                    else
                    {
                        _forceLocked = false;
                    }

                    _implicitCrMessage = newImplicitCrMessage;

                    return true;
                }

                return false;
            }

            internal CRMessage FollowingCrMessage;

            internal bool IntrusionOnlyViaLeds;

            internal volatile bool Supressed = false;

            private bool _forceLocked = false;
            /// <summary>
            /// 
            /// </summary>
            internal bool ForceLocked
            {
                get { return _forceLocked; }
            }

            [NotNull] 
            internal readonly CardReader CardReader;

            internal object MasterMessageTag;
        }
    }
}
