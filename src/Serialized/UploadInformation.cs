using Newtonsoft.Json;

namespace DG.OneDrive.Serialized
{
    public class UploadInformation
    {
        /// <summary>
        /// Describes the action to take when another file with the same name and location already exists.
        /// </summary>
        [JsonProperty("@microsoft.graph.conflictBehavior")]
        public UploadConflictBehaviour ConflictBehaviour { get; set; }

        /// <summary>
        /// A description of the file to upload.
        /// </summary>
        [JsonProperty("description")]
        public string description { get; set; }

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
    internal class UploadInformationContainer
    {
        [JsonProperty("item")]
        public UploadInformation Information { get; set; }

        public UploadInformationContainer(UploadInformation item)
        {
            this.Information = item;
        }

        public static UploadInformationContainer ForUpload(UploadInformation information)
        {
            return new UploadInformationContainer(information);
        }
    }
}
