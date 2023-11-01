using Newtonsoft.Json;

namespace DG.OneDrive.Serialized
{
    public class Office365User
    {
        [JsonProperty("@odata.context")]
        public string odataContext { get; set; }

        public string displayName { get; set; }
        public string surname { get; set; }
        public string givenName { get; set; }
        public string id { get; set; }
        public string userPrincipalName { get; set; }
        public object[] businessPhones { get; set; }
        public object jobTitle { get; set; }
        public string mail { get; set; }
        public object mobilePhone { get; set; }
        public object officeLocation { get; set; }
        public object preferredLanguage { get; set; }
    }
}
