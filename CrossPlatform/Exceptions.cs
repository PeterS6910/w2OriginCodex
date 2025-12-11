using System;


#if !COMPACT_FRAMEWORK

// serialization only for PC version
using System.Runtime.Serialization;
using System.Security.Permissions;
#endif

namespace Contal.Cgp.NCAS.DoorStateMachine
{
    public enum DSMError
    {
        CROutOfRange,
        CardReaderNotAssigned,
        NoSuchCardreader,
        DSMNotStarted,
        DSMAlreadyStarted,
    }

    [Serializable]
    public class DSMException : Exception
#if ! COMPACT_FRAMEWORK
        , ISerializable
#endif
    {
        private readonly DSMError _dsmError;
        /// <summary>
        /// value that is out of interval
        /// </summary>
        public DSMError DsmError
        {
            get { return _dsmError; }
        }

        private const string ERROR_PREFIX = "Door state machine generated error ";


        public DSMException(DSMError dsmError)
            : base(ERROR_PREFIX + dsmError)

        {
            _dsmError = dsmError;
        }

        public DSMException(DSMError dsmError,Exception innerException)
            : base(ERROR_PREFIX + dsmError,innerException)
        {
            _dsmError = dsmError;
        }

        public DSMException(DSMError dsmError, string message, Exception innerException)
            : base(message ?? (ERROR_PREFIX + dsmError), innerException)
        {
            _dsmError = dsmError;
        }


        #region ISerializable Members

#if ! COMPACT_FRAMEWORK
         private const string SERIAL_DSM_ERROR = "dsmError";

        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public DSMException(SerializationInfo serialInfo, StreamingContext context) :
            base(serialInfo, context)
        {
            _dsmError = (DSMError)serialInfo.GetInt32(SERIAL_DSM_ERROR);

        }

        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo serialInfo, StreamingContext context)
        {
            base.GetObjectData(serialInfo, context);

            serialInfo.AddValue(SERIAL_DSM_ERROR, (Int32)_dsmError);

        }
#endif

        #endregion
    }
}
