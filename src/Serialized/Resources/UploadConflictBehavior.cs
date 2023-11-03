using System.Runtime.Serialization;

namespace DG.OneDrive.Serialized.Resources
{
    /// <summary>
    /// Describes the action to take when another file with the same name and location already exists as the currently uploading one.
    /// </summary>
    public enum UploadConflictBehavior
    {
        /// <summary>
        /// Rename the new file.
        /// </summary>
        [EnumMember(Value = "rename")]
        Rename,

        /// <summary>
        /// Fail the upload.
        /// </summary>
        [EnumMember(Value = "fail")]
        Fail,

        /// <summary>
        /// Replace the old file.
        /// </summary>
        [EnumMember(Value = "replace")]
        Replace
    }
}
