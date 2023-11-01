using DG.Common;
using Newtonsoft.Json;
using System;
using System.Text;

namespace DG.OneDrive.Serialized
{
    internal class AccessToken
    {
        private static Encoding _defaultEncoding = Encoding.UTF8;
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore
        };


        public string token_type { get; set; }
        public string scope { get; set; }
        public int expires_in { get; set; }
        public int ext_expires_in { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string id_token { get; set; }
        public DateTime created { get; set; }

        public AccessToken()
        {
            created = DateTime.UtcNow;
        }

        [JsonIgnore]
        public bool HasRefreshToken => !string.IsNullOrEmpty(refresh_token);
        [JsonIgnore]
        public DateTime ExpirationDate => created.AddSeconds(expires_in);
        [JsonIgnore]
        public bool IsExpired => DateTime.UtcNow > ExpirationDate;

        public string Encrypt()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.None, _jsonSettings);
            var bytes = _defaultEncoding.GetBytes(json);
            return SafeBase64.Encode(bytes);
        }

        public static AccessToken Decrypt(string accessToken)
        {
            var bytes = SafeBase64.Decode(accessToken);
            var json = _defaultEncoding.GetString(bytes);
            return JsonConvert.DeserializeObject<AccessToken>(json, _jsonSettings);
        }
    }
}
