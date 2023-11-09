using DG.Common.Http.Fluent;
using DG.OneDrive.Serialized;
using System.Net.Http;
using System.Threading.Tasks;

namespace DG.OneDrive
{
    public class Authentication
    {
        private const string _loginBaseUri = "https://login.microsoftonline.com/common/oauth2/v2.0";
        private static readonly string[] scopes = new string[]
        {
            "openid",
            "offline_access",
            "User.Read",
            "Files.ReadWrite.AppFolder"
        };
        private static HttpClient _client => HttpClientProvider.ClientForSettings(HttpClientSettings.WithoutBaseAddress());

        private readonly IClientInfoProvider _clientInfoProvider;

        public Authentication(IClientInfoProvider clientInfoProvider)
        {
            _clientInfoProvider = clientInfoProvider;
        }

        public string GetAuthenticationUrl(string redirectUrl, string state)
        {
            string url = _loginBaseUri + $"/authorize" +
                $"?client_id={_clientInfoProvider.ClientId}" +
                $"&response_type=code" +
                $"&redirect_uri={redirectUrl}" +
                $"&response_mode=query" +
                $"&prompt=select_account" +
                $"&scope={string.Join("+", scopes)}" +
                $"&state={state}";
            return url;
        }

        public async Task<string> GetToken(string authorizationCode, string redirectUrl)
        {
            var request = FluentRequest.Post.To(_loginBaseUri + "/token")
                .WithContent(FluentFormContentBuilder
                    .With("client_secret", _clientInfoProvider.ClientSecret)
                    .AndWith("scope", string.Join(" ", scopes))
                    .AndWith("client_id", _clientInfoProvider.ClientId)
                    .AndWith("code", authorizationCode)
                    .AndWith("redirect_uri", redirectUrl)
                    .AndWith("grant_type", "authorization_code")
                );

            var token = await _client.SendAndDeserializeAsync<AccessToken>(request).ConfigureAwait(false);

            return token.Encrypt();
        }

        public async Task<string> Refresh(string token)
        {
            var decryptedToken = AccessToken.Decrypt(token);

            var request = FluentRequest.Post.To(_loginBaseUri + "/token")
                .WithContent(FluentFormContentBuilder
                    .With("client_secret", _clientInfoProvider.ClientSecret)
                    .AndWith("scope", string.Join(" ", scopes))
                    .AndWith("client_id", _clientInfoProvider.ClientId)
                    .AndWith("refresh_token", decryptedToken.refresh_token)
                    .AndWith("grant_type", "refresh_token")
                );

            var refreshedToken = await _client.SendAndDeserializeAsync<AccessToken>(request).ConfigureAwait(false);

            return refreshedToken.Encrypt();
        }
    }
}
