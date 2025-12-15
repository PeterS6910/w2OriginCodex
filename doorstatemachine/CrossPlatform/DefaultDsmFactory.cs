namespace Contal.Cgp.NCAS.DoorStateMachine
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultDsmFactory : IDoorStateMachineFactory
    {
        #region IDoorStateMachineFactory Members

        /// <summary>
        /// identification of the parameter containing possible 
        /// instance of IDoorStateMachine within CardReader.ParametersById structure
        /// </summary>
        public const int DsmCrInfoMark = 0x44534D23; // equivalent to former "DSM#"

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <returns></returns>
        public IDoorStateMachine Create(IDoorStateMachineEventHandler eventHandler)
        {
            IDoorStateMachine dsmInstance = new DoorStateMachine(eventHandler);

            return dsmInstance;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        private DefaultDsmFactory()
        {
            
        }

        public static readonly DefaultDsmFactory Singleton = new DefaultDsmFactory();
        
    }
}
