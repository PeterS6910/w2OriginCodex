using Contal.IwQuick;
using Contal.Cgp.NCAS.Definitions;

namespace Contal.Cgp.NCAS.DoorStateMachine
{
    public sealed class DsmStateInfo : APoolable<DsmStateInfo>, IDsmStateInfo
    {

        private DsmStateInfo(AObjectPool<DsmStateInfo> objectPool) : base(objectPool)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public DoorEnvironmentState DsmState { get; internal set; }


        /// <summary>
        /// 
        /// </summary>
        public DoorEnvironmentStateDetail DsmStateDetail { get;internal set;}

        /// <summary>
        /// 
        /// </summary>
        public DoorEnvironmentState PreviousDsmState { get; internal set; }

        
        /// <summary>
        /// 
        /// </summary>
        public DoorEnvironmentAccessTrigger AccessTrigger { get; internal set; }

        //internal DsmStateInfo(DoorEnvironmentState dsmState, DoorEnvironmentStateDetail dsmStateDetail)
        //{
        //    _dsmStateDetail = dsmStateDetail;
        //    _dsmState = dsmState;
        //}

        /// <summary>
        /// 
        /// </summary>
        public int CardReaderId { get; internal set; }

        protected override bool FinalizeBeforeReturn()
        {
            // just to invalidate the state object for cases it was referenced to the outside
            DsmState = DoorEnvironmentState.Unknown;
            return true;
        }
    }
}
