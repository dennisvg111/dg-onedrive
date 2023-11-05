using DG.Common;
using Newtonsoft.Json;

namespace DG.OneDrive.Serialized.Resources
{
    /// <summary>
    /// Represents the storage capacity of a drive.
    /// </summary>
    public class Quota
    {
        [JsonProperty("deleted")]
        private readonly long _deleted;

        [JsonProperty("fileCount")]
        private readonly long _fileCount;

        [JsonProperty("remaining")]
        private readonly long _remaining;

        [JsonProperty("state")]
        private readonly QuotaState _state;

        [JsonProperty("total")]
        private readonly long _total;

        [JsonProperty("used")]
        private readonly long _used;

        /// <summary>
        /// Total allowed storage space.
        /// </summary>
        public ByteSize Total => ByteSize.FromBytes(_total);

        /// <summary>
        /// Total space used.
        /// </summary>
        public ByteSize Used => ByteSize.FromBytes(_used);

        /// <summary>
        /// Total space remaining before reaching the quota limit.
        /// </summary>
        public ByteSize Remaining => ByteSize.FromBytes(_remaining);

        /// <summary>
        /// Total space consumed by files in the recycle bin.
        /// </summary>
        public ByteSize RecycleBin => ByteSize.FromBytes(_deleted);

        /// <summary>
        /// Enumeration value that indicates the state of the storage space.
        /// </summary>
        public QuotaState State => _state;

        /// <summary>
        /// Total number of files. Not available on Personal OneDrive.
        /// </summary>
        public long FileCount => _fileCount;

        /// <summary>
        /// The percentage of used storage space, as a number from 0 through 1.
        /// </summary>
        public double PercentageUsed => _used / (double)_total;

        /// <summary>
        /// The percentage of remaining storage space, as a number from 0 through 1.
        /// </summary>
        public double PercentageRemaining => _remaining / (double)_total;
    }
}
