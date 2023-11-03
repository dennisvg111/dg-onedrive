using System;

namespace DG.OneDrive.Serialized.DriveItems
{
    public class FileSystemInfo
    {
        public DateTimeOffset createdDateTime { get; set; }
        public DateTimeOffset? lastAccessedDateTime { get; set; }
        public DateTimeOffset lastModifiedDateTime { get; set; }
    }
}
