using Newtonsoft.Json;

namespace DG.OneDrive.Serialized.DriveItems
{
    public class SpecialFolder
    {
        [JsonProperty("name")]
        private readonly string _name;

        public string Name => _name;

        public SpecialFolder(string name)
        {
            _name = name;
        }

        public static SpecialFolder AppRoot => new SpecialFolder("approot");

        public static SpecialFolder CameraRoll => new SpecialFolder("cameraroll");

        public static SpecialFolder Desktop => new SpecialFolder("desktop");

        public static SpecialFolder Documents => new SpecialFolder("documents");

        public static SpecialFolder Music => new SpecialFolder("music");

        public static SpecialFolder Photos => new SpecialFolder("photos");
    }
}
