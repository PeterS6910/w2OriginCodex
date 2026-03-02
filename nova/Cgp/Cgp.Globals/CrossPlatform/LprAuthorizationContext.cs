using System;

namespace Contal.Cgp.Globals
{
    [Serializable]
    public enum LprRequiredSecondFactor : byte
    {
        None = 0,
        Card = 1,
        Pin = 2,
        CardOrPin = 3
    }

    [Serializable]
    public enum LprPassDirection : byte
    {
        Unknown = 0,
        Internal = 1,
        External = 2
    }

    [Serializable]
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
