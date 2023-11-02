using System;

namespace DG.OneDrive.Serialized
{
    public class UploadSession
    {
        public Uri uploadUrl { get; set; }
        public DateTimeOffset expirationDateTime { get; set; }
        public string[] nextExpectedRanges { get; set; }
    }
}
