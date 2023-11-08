using Newtonsoft.Json;

namespace DG.OneDrive.Serialized.Resources
{
    /// <summary>
    /// This class represents the information about an upload.
    /// </summary>
    public class UploadMetaData
    {

#pragma warning disable IDE0051 // (Remove unused private members) This property is used for serialization.

        [JsonProperty("@microsoft.graph.conflictBehavior")]
        private string _conflictBehavior => ConflictBehavior.ToString().ToLowerInvariant();

#pragma warning restore IDE0051

        /// <summary>
        /// Describes the action to take when another file with the same name and location already exists.
        /// </summary>
        [JsonIgnore]
        public UploadConflictBehavior ConflictBehavior { get; set; }

        /// <summary>
        /// A description of the file to upload.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// The name with extension of the file to upload.
        /// </summary>
        [JsonProperty("name")]
        public string NameWithExtension { get; set; }

        /// <summary>
        /// This property is not serialized, but instead will be used to form the url of the upload session.
        /// </summary>
        [JsonIgnore]
        public string Path { get; set; }
    }

    /// <summary>
    /// A container only used during the serialization process.
    /// </summary>
    internal class UploadRequest
    {
        [JsonProperty("item")]
        public UploadMetaData Information { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="UploadRequest"/> with the given meta data.
        /// </summary>
        /// <param name="information"></param>
        public UploadRequest(UploadMetaData information)
        {
            Information = information;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UploadRequest"/> with the given meta data.
        /// </summary>
        /// <param name="information"></param>
        public static UploadRequest WithMetaData(UploadMetaData information)
        {
            return new UploadRequest(information);
        }
    }
}
