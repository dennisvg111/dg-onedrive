using DG.Common;
using DG.Common.Http.Authorization.OAuth2;
using DG.Common.Http.Authorization.OAuth2.Data;
using DG.Common.Http.Fluent;
using DG.OneDrive.Authorization;
using DG.OneDrive.Serialized;
using DG.OneDrive.Serialized.DriveItems;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DG.OneDrive
{
    /// <summary>
    /// A client to interact with the OneDrive API.
    /// </summary>
    public class Client
    {
        private const string _apiBaseUri = "https://graph.microsoft.com/v1.0/";
        private static readonly HttpClientSettings _clientSettings = HttpClientSettings.WithBaseAddress(_apiBaseUri);

        private readonly UploadClient _upload;
        private readonly OAuthFlow<OneDriveOAuthLogic> _authorization;

        /// <summary>
        /// Provides functionality for creating upload sessions, and uploading files.
        /// </summary>
        public UploadClient Upload => _upload;

        /// <summary>
        /// Initializes a new instance of <see cref="Client"/> with the given implementation of <see cref="IClientInfoProvider"/>.
        /// </summary>
        /// <param name="authorization"></param>
        public Client(OAuthFlow<OneDriveOAuthLogic> authorization)
        {
            _authorization = authorization;
            _upload = new UploadClient(authorization);
        }

        /// <summary>
        /// Returns the currently authenticated user.
        /// </summary>
        /// <returns></returns>
        public async Task<Office365User> GetCurrentUserAsync()
        {
            var request = FluentRequest.Get.To("me")
                .WithAuthorization(_authorization);

            var client = HttpClientProvider.ClientForSettings(_clientSettings);
            return await client.SendAndDeserializeAsync<Office365User>(request).ConfigureAwait(false);
        }

        /// <summary>
        /// Copies the content of the file with the given <paramref name="fileId"/> to <paramref name="outputStream"/>.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="outputStream"></param>
        /// <returns></returns>
        public async Task CopyToStreamAsync(string fileId, Stream outputStream)
        {
            var request = FluentRequest.Get.To($"me/drive/items/{fileId}/content")
                .WithAuthorization(_authorization);

            var client = HttpClientProvider.ClientForSettings(_clientSettings);
            var response = await client.SendAsync(request);

            await response.Content.CopyToAsync(outputStream).ConfigureAwait(false);

            if (outputStream.CanSeek)
            {
                outputStream.Position = 0;
            }
        }

        /// <summary>
        /// Returns information, such as storage capacity, about the current drive.
        /// </summary>
        /// <returns></returns>
        public async Task<Drive> GetDriveAsync()
        {
            var request = FluentRequest.Get.To("me/drive")
                .WithAuthorization(_authorization);

            var client = HttpClientProvider.ClientForSettings(_clientSettings);
            return await client.SendAndDeserializeAsync<Drive>(request).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a list of children (folders and files) at the given <paramref name="path"/>.
        /// </summary>
        /// <param name="path"></param>
        public async Task<List<DriveItem>> GetChildrenAsync(string path)
        {
            var url = $"me/drive/special/approot:/{path.TrimStart('/')}:/children?top=2";

            return await GetFeed<DriveItem>(url).ConfigureAwait(false);
        }

        private async Task<List<T>> GetFeed<T>(string url)
        {
            List<T> result = new List<T>();
            while (true)
            {
                var request = FluentRequest.Get.To(url)
                .WithAuthorization(_authorization);

                var client = HttpClientProvider.ClientForSettings(_clientSettings);
                var list = await client.SendAndDeserializeAsync<ListResult<T>>(request).ConfigureAwait(false);

                result.AddRange(list.Values);

                if (list.NextLink == null)
                {
                    break;
                }

                url = list.NextLink.ToString();
            }

            return result;
        }

        /// <summary>
        /// Serializes the current access token for this client to a string.
        /// </summary>
        /// <returns></returns>
        public string SerializeAccessToken()
        {
            var data = _authorization.Export();
            var token = AccessToken.From(data);
            return token.Encrypt();
        }

        /// <summary>
        /// Initialize a new client from the given serialized access token created using <see cref="SerializeAccessToken()"/>.
        /// </summary>
        /// <param name="clientInfo"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public static Client FromSerializedAccessToken(IClientInfoProvider clientInfo, string accessToken)
        {
            var token = AccessToken.Decrypt(accessToken);
            var oauthData = new OAuthData()
            {
                Started = token.created,
                Scopes = token.scope?.Split('+', ' ') ?? new string[0],
                CallBackUri = null,
                AccessToken = token.access_token,
                AccessTokenExpirationDate = token.ExpirationDate,
                RefreshToken = token.refresh_token,

                State = Uulsid.NewUulsid().ToString()
            };

            var logic = new OneDriveOAuthLogic(clientInfo);
            var oauthFlow = OAuthFlow.ContinueFor(logic, oauthData);
            return new Client(oauthFlow);
        }
    }
}
