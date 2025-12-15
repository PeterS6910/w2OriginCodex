using System;
using System.Diagnostics;
using Contal.Cgp.NCAS.Definitions;
using Contal.IwQuick;
using Contal.IwQuick.Sys;
using JetBrains.Annotations;

namespace Contal.Cgp.NCAS.DoorStateMachine
{
    /// <summary>
    /// class to provide static decision for particular Transition* method based on from-DSMstate and to-DSMstate parameters,
    /// bridged to  DsmStateTransitionContext DSM's Transition methods
    /// 
    /// motivation : _dsmStateTransitionMethodCache instanced only once, however transition methods being called on context's DSM
    /// </summary>
    internal sealed class DsmTransitionBridge : ADsmTransitionMethodBridge
    {

        public static readonly DsmTransitionBridge Singleton = new DsmTransitionBridge();


        [Conditional("DEBUG")]
        private void ValidateCacheDimension(int dimensionToValidate)
        {
            int count = 0;

            EnumHelper.IterateAllValues<DoorEnvironmentState>(enumValue =>
            {
                count++;
            });

            if (dimensionToValidate < count)
            {
                var e = new SystemException("Dimension for dsmStateTransitionCache is too low, raise to " + count);
                DebugHelper.TryBreak(e, dimensionToValidate, count);
                throw e;
            }
        }


        private delegate void DTransitionToState(IDsmStateTransitionContext transitionContext);

        [NotNull]
        private volatile DTransitionToState[,] _dsmStateTransitionMethodCache;

        private DsmTransitionBridge()
        {
            InitMethodCache();
        }

        private DTransitionToState GetTransitionMethodCache(
            DoorEnvironmentState fromState,
            DoorEnvironmentState toState)
        {

            return _dsmStateTransitionMethodCache[(int)fromState, (int)toState];

        }

        private void InitMethodCache()
        {
            // dimensions strictly bound to number of DoorEnvironmentState values
            const int cacheDimension = 10;

#if DEBUG
            ValidateCacheDimension(cacheDimension);
#endif

            var tmc = new DTransitionToState[cacheDimension, cacheDimension];

            // to Opened
            int dstState = (int)DoorEnvironmentState.Opened;

            tmc[(int)DoorEnvironmentState.Locked, dstState] = TransitionLockedToOpened;
            tmc[(int)DoorEnvironmentState.Unlocked, dstState] = TransitionUnlockedToOpened;
            tmc[(int)DoorEnvironmentState.Intrusion, dstState] = TransitionIntrusionToOpened;
            tmc[(int)DoorEnvironmentState.AjarPrewarning, dstState] = TransitionAjarPrewarningToOpened;
            tmc[(int)DoorEnvironmentState.Ajar, dstState] = TransitionAjarToOpened;


            tmc[(int)DoorEnvironmentState.Unlocking, dstState] =
                tmc[(int)DoorEnvironmentState.Locking, dstState] =
                    tmc[(int)DoorEnvironmentState.Sabotage, dstState] =
                        TransitionIrrelevantToOpened;


            // to Locked
            dstState = (int)DoorEnvironmentState.Locked;
            tmc[(int)DoorEnvironmentState.Unlocked, dstState] = TransitionUnlockedToLocked;
            tmc[(int)DoorEnvironmentState.Opened, dstState] = TransitionOpenedToLocked;
            tmc[(int)DoorEnvironmentState.Intrusion, dstState] = TransitionIntrusionToLocked;
            tmc[(int)DoorEnvironmentState.AjarPrewarning, dstState] = TransitionOpenedToLocked;
            tmc[(int)DoorEnvironmentState.Ajar, dstState] = TransitionOpenedToLocked;

            tmc[(int)DoorEnvironmentState.Unlocking, dstState] =
                tmc[(int)DoorEnvironmentState.Locking, dstState] =
                    tmc[(int)DoorEnvironmentState.Sabotage, dstState] =
                        TransitionIrrelevantToLocked;


            // to Unlocked
            dstState = (int)DoorEnvironmentState.Unlocked;
            tmc[(int)DoorEnvironmentState.Locked, dstState] = TransitionLockedToUnlocked;
            tmc[(int)DoorEnvironmentState.Opened, dstState] = TransitionOpenedToUnlocked;

            tmc[(int)DoorEnvironmentState.Unlocking, dstState] =
                tmc[(int)DoorEnvironmentState.Locking, dstState] =
                    tmc[(int)DoorEnvironmentState.Intrusion, dstState] =
                        tmc[(int)DoorEnvironmentState.Sabotage, dstState] =
                            tmc[(int)DoorEnvironmentState.Ajar, dstState] =
                                tmc[(int)DoorEnvironmentState.AjarPrewarning, dstState]
                                    = TransitionIrrelevantToUnlocked;


            // to Intrusion
            dstState = (int)DoorEnvironmentState.Intrusion;
            tmc[(int)DoorEnvironmentState.Locked, dstState] = TransitionLockedToIntrusion;

            tmc[(int)DoorEnvironmentState.Unlocking, dstState] =
                tmc[(int)DoorEnvironmentState.Unlocked, dstState] =
                    tmc[(int)DoorEnvironmentState.Opened, dstState] =
                        tmc[(int)DoorEnvironmentState.Locking, dstState] =
                            tmc[(int)DoorEnvironmentState.Sabotage, dstState] =
                                tmc[(int)DoorEnvironmentState.Ajar, dstState] =
                                    tmc[(int)DoorEnvironmentState.AjarPrewarning, dstState] =
                                        TransitionIrrelevantToIntrusion;

            // to AjarPrewarning
            dstState = (int)DoorEnvironmentState.AjarPrewarning;
            tmc[(int)DoorEnvironmentState.Opened, dstState] = TransitionOpenedToAjarPrewarning;

            tmc[(int)DoorEnvironmentState.Locked, dstState] =
                tmc[(int)DoorEnvironmentState.Unlocking, dstState] =
                    tmc[(int)DoorEnvironmentState.Unlocked, dstState] =
                        tmc[(int)DoorEnvironmentState.Locking, dstState] =
                            tmc[(int)DoorEnvironmentState.Intrusion, dstState] =
                                tmc[(int)DoorEnvironmentState.Sabotage, dstState] =
                                    tmc[(int)DoorEnvironmentState.Ajar, dstState] =
                                        TransitionIrrelevantToAjarPrewarning;

            // to Ajar                       
            dstState = (int)DoorEnvironmentState.Ajar;
            tmc[(int)DoorEnvironmentState.Opened, dstState] =
                tmc[(int)DoorEnvironmentState.AjarPrewarning, dstState] =
                    TransitionOpenedToAjar;

            tmc[(int)DoorEnvironmentState.Locked, dstState] =
                tmc[(int)DoorEnvironmentState.Unlocking, dstState] =
                    tmc[(int)DoorEnvironmentState.Unlocked, dstState] =
                        tmc[(int)DoorEnvironmentState.Locking, dstState] =
                            tmc[(int)DoorEnvironmentState.Intrusion, dstState] =
                                tmc[(int)DoorEnvironmentState.Sabotage, dstState] =
                                    TransitionIrrelevantToAjar;


            _dsmStateTransitionMethodCache = tmc;
        }

        internal void Proceed([NotNull] IDsmStateTransitionContext transitionContext)
        {
            DTransitionToState transitionMethod = null;

            try
            {
                transitionMethod = GetTransitionMethodCache(transitionContext.From, transitionContext.To);
            }
            catch (Exception e)
            {
                HandledExceptionAdapter.Examine(e);
            }

            if (transitionMethod != null)
            {
                Debug.WriteLine(Environment.TickCount + " DSM# Before transition " + transitionContext);
                transitionMethod(transitionContext);
                Debug.WriteLine(Environment.TickCount + " DSM# After transition " + transitionContext);
            }
#if DEBUG
            else
            {
                DebugHelper.TryBreak("DSM#.SetDsmState(Static dispatch) no transition method for "
                                    + transitionContext.From + "->"
                                    + transitionContext.To);
            }
#endif
        }

        internal override void TransitionLockedToOpened(IDsmStateTransitionContext transitioncontext)
        {
            transitioncontext.Dsm.TransitionLockedToOpened(transitioncontext);
        }

        internal override void TransitionIntrusionToLocked(IDsmStateTransitionContext transitioncontext)
        {
            transitioncontext.Dsm.TransitionIntrusionToLocked(transitioncontext);
        }

        internal override void TransitionOpenedToAjarPrewarning(IDsmStateTransitionContext transitioncontext)
        {
            transitioncontext.Dsm.TransitionOpenedToAjarPrewarning(transitioncontext);
        }

        internal override void TransitionIrrelevantToAjar(IDsmStateTransitionContext transitioncontext)
        {
            transitioncontext.Dsm.TransitionIrrelevantToAjar(transitioncontext);
        }

        internal override void TransitionOpenedToAjar(IDsmStateTransitionContext transitioncontext)
        {
            transitioncontext.Dsm.TransitionOpenedToAjar(transitioncontext);
        }

        internal override void TransitionAjarPrewarningToOpened(IDsmStateTransitionContext transitioncontext)
        {
            transitioncontext.Dsm.TransitionAjarPrewarningToOpened(transitioncontext);
        }

        internal override void TransitionAjarToOpened(IDsmStateTransitionContext transitioncontext)
        {
            transitioncontext.Dsm.TransitionAjarToOpened(transitioncontext);
        }

        internal override void TransitionOpenedToLocked(IDsmStateTransitionContext transitioncontext)
        {
            transitioncontext.Dsm.TransitionOpenedToLocked(transitioncontext);
        }

        internal override void TransitionIntrusionToOpened(IDsmStateTransitionContext transitioncontext)
        {
            transitioncontext.Dsm.TransitionIntrusionToOpened(transitioncontext);
        }

        internal override void TransitionUnlockedToLocked(IDsmStateTransitionContext transitioncontext)
        {
            transitioncontext.Dsm.TransitionUnlockedToLocked(transitioncontext);
        }

        internal override void TransitionLockedToUnlocked(IDsmStateTransitionContext transitioncontext)
        {
            transitioncontext.Dsm.TransitionLockedToUnlocked(transitioncontext);
        }

        internal override void TransitionOpenedToUnlocked(IDsmStateTransitionContext transitioncontext)
        {
            transitioncontext.Dsm.TransitionOpenedToUnlocked(transitioncontext);
        }

        internal override void TransitionIrrelevantToUnlocked(IDsmStateTransitionContext transitioncontext)
        {
            transitioncontext.Dsm.TransitionIrrelevantToUnlocked(transitioncontext);
        }

        internal override void TransitionLockedToIntrusion(IDsmStateTransitionContext transitioncontext)
        {
            transitioncontext.Dsm.TransitionLockedToIntrusion(transitioncontext);
        }

        internal override void TransitionIrrelevantToIntrusion(IDsmStateTransitionContext transitioncontext)
        {
            transitioncontext.Dsm.TransitionIrrelevantToIntrusion(transitioncontext);
        }

        internal override void TransitionIrrelevantToAjarPrewarning(IDsmStateTransitionContext transitioncontext)
        {
            transitioncontext.Dsm.TransitionIrrelevantToAjarPrewarning(transitioncontext);
        }

        internal override void TransitionUnlockedToOpened(IDsmStateTransitionContext transitioncontext)
        {
            transitioncontext.Dsm.TransitionUnlockedToOpened(transitioncontext);
        }


        internal override void TransitionIrrelevantToLocked(IDsmStateTransitionContext transitionContext)
        {
            transitionContext.Dsm.TransitionIrrelevantToLocked(transitionContext);
        }

        internal override void TransitionIrrelevantToOpened(IDsmStateTransitionContext transitionContext)
        {
            transitionContext.Dsm.TransitionIrrelevantToOpened(transitionContext);
        }

        protected override void InternalDispose(bool isExplicitDispose)
        {
         
        }
    }

}
