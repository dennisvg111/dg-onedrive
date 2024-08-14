using Newtonsoft.Json;

namespace DG.OneDrive.Serialized.DriveItems
{
    public class DownloadableDriveItem
    {
        public string id { get; set; }
        public long size { get; set; }

        [JsonProperty("@microsoft.graph.downloadUrl")]
        public string downloadUrl { get; set; }
    }
}
