namespace DG.OneDrive.Serialized.Resources
{
    /// <summary>
    /// The state of the storage capacity of a drive.
    /// </summary>
    public enum QuotaState
    {
        /// <summary>
        /// The drive has plenty of remaining quota left.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Remaining quota is less than 10% of total quota space.
        /// </summary>
        Nearing = 1,

        /// <summary>
        /// Remaining quota is less than 1% of total quota space.
        /// </summary>
        Critical = 2,

        /// <summary>
        /// The used quota has exceeded the total quota. New files or folders cannot be added to the drive until it is under the total quota amount or more storage space is purchased.
        /// </summary>
        Exceeded = 3
    }
}
