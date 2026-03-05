using Contal.IwQuick.Data;
using System;

namespace Contal.Cgp.Globals
{
    [Serializable]
    [LwSerialize(846)]
    public enum LprRequiredSecondFactor : byte
    {
        None = 0,
        Card = 1,
        Pin = 2,
        CardOrPin = 3
    }

    [Serializable]
    [LwSerialize(847)]
    public enum LprPassDirection : byte
    {
        Unknown = 0,
        Internal = 1,
        External = 2
    }

    [Serializable]
    [LwSerialize(848)]
    public class LprAuthorizationContext
    {
        public Guid CorrelationId { get; set; }
        public Guid CarId { get; set; }
        public string PlateNormalized { get; set; }
        public LprRequiredSecondFactor RequiredSecondFactor { get; set; }
        public LprPassDirection Direction { get; set; }
        public DateTime ValidToUtc { get; set; }
        public Guid SourceCameraId { get; set; }
    }
}
