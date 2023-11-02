using System.Runtime.Serialization;

namespace DG.OneDrive.Serialized
{
    public enum UploadConflictBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        [EnumMember(Value = "rename")]
        Rename,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember(Value = "fail")]
        Fail,
        [EnumMember(Value = "replace")]
        Replace
    }
}
