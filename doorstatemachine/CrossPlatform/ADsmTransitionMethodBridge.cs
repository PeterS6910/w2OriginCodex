using Contal.IwQuick;

namespace Contal.Cgp.NCAS.DoorStateMachine
{
    /// <summary>
    /// motivation : pattern for DoorStateMachine and DsmTransitionBridge to ensure coherrent Transition* method layour,
    /// however still hidden to the outside, not as implemented interface
    /// </summary>
    public abstract class ADsmTransitionMethodBridge:ADisposable
    {
        internal abstract void TransitionLockedToOpened(IDsmStateTransitionContext transitioncontext);

        internal abstract void TransitionIntrusionToLocked(IDsmStateTransitionContext transitioncontext);

        internal abstract void TransitionOpenedToAjarPrewarning(IDsmStateTransitionContext transitioncontext);

        internal abstract void TransitionIrrelevantToAjar(IDsmStateTransitionContext transitioncontext);

        internal abstract void TransitionOpenedToAjar(IDsmStateTransitionContext transitioncontext);

        internal abstract void TransitionAjarPrewarningToOpened(IDsmStateTransitionContext transitioncontext);

        internal abstract void TransitionAjarToOpened(IDsmStateTransitionContext transitioncontext);

        internal abstract void TransitionOpenedToLocked(IDsmStateTransitionContext transitioncontext);

        internal abstract void TransitionIntrusionToOpened(IDsmStateTransitionContext transitioncontext);

        internal abstract void TransitionUnlockedToLocked(IDsmStateTransitionContext transitioncontext);

        internal abstract void TransitionLockedToUnlocked(IDsmStateTransitionContext transitioncontext);

        internal abstract void TransitionOpenedToUnlocked(IDsmStateTransitionContext transitioncontext);

        internal abstract void TransitionIrrelevantToUnlocked(IDsmStateTransitionContext transitioncontext);

        internal abstract void TransitionLockedToIntrusion(IDsmStateTransitionContext transitioncontext);

        internal abstract void TransitionIrrelevantToIntrusion(IDsmStateTransitionContext transitioncontext);

        internal abstract void TransitionIrrelevantToAjarPrewarning(IDsmStateTransitionContext transitioncontext);

        internal abstract void TransitionUnlockedToOpened(IDsmStateTransitionContext transitioncontext);

        internal abstract void TransitionIrrelevantToLocked(IDsmStateTransitionContext transitionContext);

        internal abstract void TransitionIrrelevantToOpened(IDsmStateTransitionContext transitionContext);
    }
}
