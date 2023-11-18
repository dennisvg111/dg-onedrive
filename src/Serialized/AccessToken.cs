using DG.Common;
using DG.Common.Http.Authorization.OAuth2.Data;
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
        public long expires_in { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public DateTimeOffset created { get; set; }

        public AccessToken()
        {
            created = DateTimeOffset.UtcNow;
        }

        [JsonIgnore]
        public bool HasRefreshToken => !string.IsNullOrEmpty(refresh_token);
        [JsonIgnore]
        public DateTimeOffset ExpirationDate => created.AddSeconds(expires_in);
        [JsonIgnore]
        public bool IsExpired => DateTimeOffset.UtcNow > ExpirationDate;

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

        public static AccessToken From(IReadOnlyOAuthData data)
        {
            var expiresIn = int.MaxValue;
            if (data.AccessTokenExpirationDate.HasValue)
            {
                expiresIn = (int)(data.AccessTokenExpirationDate.Value - data.Started).TotalSeconds;
            }

            return new AccessToken()
            {
                token_type = "bearer",
                scope = string.Join("+", data.Scopes),
                created = data.Started,
                expires_in = expiresIn,
                access_token = data.AccessToken,
                refresh_token = data.RefreshToken
            };
        }
    }
}
