using DG.Common.Http;
using DG.Common.Http.Authorization.OAuth2.Data;
using DG.Common.Http.Authorization.OAuth2.Interfaces;
using DG.Common.Http.Fluent;
using DG.Common.Threading;
using DG.OneDrive.Serialized;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DG.OneDrive.Authorization
{
    /// <summary>
    /// Provides the logic to handle OAuth authorization for OneDrive.
    /// </summary>
    public class OneDriveOAuthLogic : IOAuthLogic
    {
        private const string _loginBaseUri = "https://login.microsoftonline.com/common/oauth2/v2.0";
        private static readonly HttpClientSettings _httpClientSettings = HttpClientSettings.WithoutBaseAddress();
        private static readonly string[] _scopes = new string[]
        {
            "openid",
            "offline_access",
            "User.Read",
            "Files.ReadWrite.AppFolder"
        };

        /// <summary>
        /// The scopes needed to use <see cref="Client"/>.
        /// </summary>
        public static IReadOnlyList<string> DefaultScopes => _scopes;

        private readonly IClientInfoProvider _clientInfoProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="OneDriveOAuthLogic"/> with the given <see cref="IClientInfoProvider"/>.
        /// </summary>
        /// <param name="clientInfoProvider"></param>
        public OneDriveOAuthLogic(IClientInfoProvider clientInfoProvider)
        {
            _clientInfoProvider = clientInfoProvider;
        }

        /// <inheritdoc/>
        public Uri BuildAuthorizationUri(OAuthRequest request)
        {
            return new UriBuilder(_loginBaseUri + $"/authorize")
                .WithQuery("client_id", _clientInfoProvider.ClientId)
                .WithQuery("response_type", "code")
                .WithQuery("redirect_uri", request.CallBackUri.OriginalString)
                .WithQuery("response_mode", "query")
                .WithQuery("prompt", "select_account")
                .WithQuery("scope", string.Join(" ", request.Scopes))
                .WithQuery("state", request.State)
                .Uri;
        }

        /// <inheritdoc/>
        public async Task<OAuthToken> GetAccessTokenAsync(OAuthRequest request, string callBackCode)
        {
            var tokenRequest = FluentRequest.Post.To(_loginBaseUri + "/token")
                .WithContent(FluentFormContentBuilder
                    .With("client_secret", _clientInfoProvider.ClientSecret)
                    .AndWith("scope", string.Join(" ", _scopes))
                    .AndWith("client_id", _clientInfoProvider.ClientId)
                    .AndWith("code", callBackCode)
                    .AndWith("redirect_uri", request.CallBackUri.ToString())
                    .AndWith("grant_type", "authorization_code")
                );

            var client = HttpClientProvider.ClientForSettings(_httpClientSettings);
            var token = await client.SendAndDeserializeAsync<AccessToken>(tokenRequest).ConfigureAwait(false);

            return new OAuthToken(token.access_token, token.ExpirationDate, token.refresh_token);
        }

        /// <inheritdoc/>
        public FluentAuthorization GetHeaderForToken(string accessToken)
        {
            return FluentAuthorization.ForBearer(accessToken);
        }

        /// <inheritdoc/>
        public async Task<TaskResult<OAuthToken>> TryRefreshTokenAsync(string refreshToken)
        {
            var tokenRequest = FluentRequest.Post.To(_loginBaseUri + "/token")
                .WithContent(FluentFormContentBuilder
                    .With("client_secret", _clientInfoProvider.ClientSecret)
                    .AndWith("scope", string.Join(" ", _scopes))
                    .AndWith("client_id", _clientInfoProvider.ClientId)
                    .AndWith("refresh_token", refreshToken)
                    .AndWith("grant_type", "refresh_token")
                );

            var client = HttpClientProvider.ClientForSettings(_httpClientSettings);
            var refreshedToken = await client.SendAndDeserializeAsync<AccessToken>(tokenRequest).ConfigureAwait(false);

            var token = new OAuthToken(refreshedToken.access_token, refreshedToken.ExpirationDate, refreshedToken.refresh_token);
            return TaskResult.Success(token);
        }
    }
}
