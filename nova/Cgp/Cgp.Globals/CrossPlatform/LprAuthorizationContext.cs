using System;
using Contal.IwQuick.Data;

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
        [LwSerialize]
        public Guid CorrelationId { get; set; }
        [LwSerialize]
        public Guid CarId { get; set; }
        [LwSerialize]
        public string PlateNormalized { get; set; }
        [LwSerialize]
        public Guid[] ValidCardIds { get; set; }
        [LwSerialize]
        public LprRequiredSecondFactor RequiredSecondFactor { get; set; }
        [LwSerialize]
        public LprPassDirection Direction { get; set; }
        [LwSerialize]
        public DateTime ValidToUtc { get; set; }
        [LwSerialize]
        public Guid SourceCameraId { get; set; }
    }
}
