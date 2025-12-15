using Contal.Cgp.NCAS.Definitions;
using Contal.IwQuick;
using JetBrains.Annotations;

namespace Contal.Cgp.NCAS.DoorStateMachine
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class DsmStateTransitionContext : APoolable<DsmStateTransitionContext>, IDsmStateTransitionContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsm"></param>
        /// <param name="fromState"></param>
        /// <param name="toState"></param>
        /// <param name="fromPlanningTimer"></param>
        /// <param name="dsmStateDetailOverride"></param>
        /// <returns></returns>
        public static IDsmStateTransitionContext Get(
            [NotNull] DoorStateMachine dsm,
            DoorEnvironmentState fromState,
            DoorEnvironmentState toState,
            bool fromPlanningTimer,
            DoorEnvironmentStateDetail? dsmStateDetailOverride)
        {
            var filledContext = Get(context =>
            {
                context.Dsm = dsm;
                context.From = fromState;
                context.To = toState;
                context.FromPlanningTimer = fromPlanningTimer;
                context.DsmStateDetailOverride = dsmStateDetailOverride;
            });

            return filledContext;
        }

        [NotNull]
        public DoorStateMachine Dsm { get; private set; }

        public DoorEnvironmentState From { get; private set; }

        public DoorEnvironmentState To { get; private set; }

        public bool FromPlanningTimer { get; private set; }

        public DoorEnvironmentStateDetail? DsmStateDetailOverride { get; private set; }

        private DsmStateTransitionContext(AObjectPool<DsmStateTransitionContext> objectPool)
            : base(objectPool)
        {
        }

        protected override bool FinalizeBeforeReturn()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Dsm = null;
            From = DoorEnvironmentState.Unknown;
            To = DoorEnvironmentState.Unknown;
            FromPlanningTimer = false;
            DsmStateDetailOverride = null;
            return true;
        }

        public override string ToString()
        {
            return From + "->" + To;
        }
    }
}
