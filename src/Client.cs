using DG.Common.Http.Fluent;
using DG.OneDrive.Serialized;
using DG.OneDrive.Serialized.DriveItems;
using DG.OneDrive.Serialized.Resources;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DG.OneDrive
{
    public class Client
    {
        private const string _apiBaseUri = "https://graph.microsoft.com/v1.0";
        private const int _maxUploadChunkSize = 41943040;
        private static HttpClient _client => HttpClientProvider.ClientForSettings(HttpClientSettings.WithoutBaseAddress());

        private readonly IClientInfoProvider _clientInfoProvider;

        private AccessTokenHeaderProvider _accessTokenHeaderProvider;

        public Client(IClientInfoProvider clientInfoProvider)
        {
            _clientInfoProvider = clientInfoProvider;
        }

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
            var request = FluentRequest.Get.To(_apiBaseUri + "/me")
                .WithHeader(FluentHeader.Authorization(_accessTokenHeaderProvider));

            return await _client.SendAndDeserializeAsync<Office365User>(request);
        }

        /// <summary>
        /// Creates a new upload session.
        /// </summary>
        /// <param name="information"></param>
        /// <returns></returns>
        public async Task<UploadSession> CreateUploadSessionAsync(UploadMetaData information)
        {
            var container = UploadRequest.WithMetaData(information);

            string uri = _apiBaseUri + $"/me/drive/special/approot:/{information.Path.TrimStart('/')}/{information.NameWithExtension}:/createUploadSession";
            var request = FluentRequest.Post.To(uri)
                .WithHeader(FluentHeader.Authorization(_accessTokenHeaderProvider))
                .WithSerializedJsonContent(container);

            return await _client.SendAndDeserializeAsync<UploadSession>(request);
        }

        public async Task<DriveItem> UploadStreamAsync(UploadMetaData fileData, Stream stream)
        {
            var session = await CreateUploadSessionAsync(fileData);

            if (session.UploadUri == null)
            {
                throw new System.Exception();
            }

            return await UploadStreamAsync(stream, session.UploadUri);
        }

        public async Task CopyToStreamAsync(string fileId, Stream outputStream)
        {
            var request = FluentRequest.Get.To(_apiBaseUri + $"/me/drive/items/{fileId}/content")
                .WithHeader(FluentHeader.Authorization(_accessTokenHeaderProvider));

            var response = await _client.SendAsync(request);

            await response.Content.CopyToAsync(outputStream);

            if (outputStream.CanSeek)
            {
                outputStream.Position = 0;
            }
        }

        public async Task<Stream> DownloadStreamAsync(string fileId)
        {
            MemoryStream stream = new MemoryStream();
            await CopyToStreamAsync(fileId, stream);
            return stream;
        }

        private async Task<DriveItem> UploadStreamAsync(Stream stream, Uri uri)
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            long totalLength = stream.Length;
            byte[] buffer = new byte[_maxUploadChunkSize];
            long offset = 0;

            HttpResponseMessage lastResult = null;
            while (true)
            {
                int byteCount = stream.Read(buffer, 0, buffer.Length);
                if (byteCount <= 0)
                {
                    break;
                }

                var request = FluentRequest.Put.To(uri)
                    .WithHeader(FluentHeader.ContentLength(byteCount))
                    .WithHeader(FluentHeader.ContentRange(offset, byteCount, totalLength))
                    .WithByteArrayContent(buffer, byteCount);

                lastResult = await _client.SendAsync(request);
                offset += byteCount;
            }

            var json = await lastResult.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DriveItem>(json);
        }
    }
}
