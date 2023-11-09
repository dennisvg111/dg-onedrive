using DG.Common.Http.Fluent;
using DG.OneDrive.Serialized;
using DG.OneDrive.Serialized.DriveItems;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DG.OneDrive
{
    /// <summary>
    /// A client to interact with the OneDrive API.
    /// </summary>
    public class Client
    {
        private const string _apiBaseUri = "https://graph.microsoft.com/v1.0/";

        private static HttpClient _client => HttpClientProvider.ClientForSettings(HttpClientSettings.WithBaseAddress(_apiBaseUri));

        private readonly UploadClient _upload;
        private readonly IClientInfoProvider _clientInfoProvider;
        private AccessTokenHeaderProvider _accessTokenHeaderProvider;

        /// <summary>
        /// Provides functionality for creating upload sessions, and uploading files.
        /// </summary>
        public UploadClient Upload => _upload;

        /// <summary>
        /// Initializes a new instance of <see cref="Client"/> with the given implementation of <see cref="IClientInfoProvider"/>.
        /// </summary>
        /// <param name="clientInfoProvider"></param>
        public Client(IClientInfoProvider clientInfoProvider)
        {
            _clientInfoProvider = clientInfoProvider;
            _upload = new UploadClient(() => _accessTokenHeaderProvider);
        }

        /// <summary>
        /// Sets the access token for this client.
        /// </summary>
        /// <param name="accessToken"></param>
        public void SetAccessToken(string accessToken)
        {
            _accessTokenHeaderProvider = new AccessTokenHeaderProvider(new Authentication(_clientInfoProvider), accessToken);
        }

        /// <summary>
        /// Returns the currently authenticated user.
        /// </summary>
        /// <returns></returns>
        public async Task<Office365User> GetCurrentUserAsync()
        {
            var request = FluentRequest.Get.To("me")
                .WithHeader(FluentHeader.Authorization(_accessTokenHeaderProvider));

            return await _client.SendAndDeserializeAsync<Office365User>(request).ConfigureAwait(false);
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
                .WithHeader(FluentHeader.Authorization(_accessTokenHeaderProvider));

            var response = await _client.SendAsync(request);

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
                .WithHeader(FluentHeader.Authorization(_accessTokenHeaderProvider));

            return await _client.SendAndDeserializeAsync<Drive>(request).ConfigureAwait(false);
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
                    .WithHeader(FluentHeader.Authorization(_accessTokenHeaderProvider));

                var list = await _client.SendAndDeserializeAsync<ListResult<T>>(request).ConfigureAwait(false);

                result.AddRange(list.Values);

                if (list.NextLink == null)
                {
                    break;
                }

                url = list.NextLink.ToString();
            }

            return result;
        }
    }
}
