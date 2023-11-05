using DG.OneDrive.Serialized.Resources;
using Newtonsoft.Json;

namespace DG.OneDrive.Serialized
{
    public class Drive
    {
        [JsonProperty("quota")]
        private readonly Quota _quota;

        public Quota Quota => _quota;
    }
}
