using Contal.Cgp.NCAS.Definitions;
using Contal.IwQuick;

namespace Contal.Cgp.NCAS.DoorStateMachine
{
    public interface IDsmStateTransitionContext:IPoolable
    {
        DoorStateMachine Dsm { get; }

        DoorEnvironmentState From { get; }

        DoorEnvironmentState To { get; }

        bool FromPlanningTimer { get; }

        DoorEnvironmentStateDetail? DsmStateDetailOverride { get; }
    }
}