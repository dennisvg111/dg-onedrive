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
    /// <summary>
    /// A client to interact with the OneDrive API.
    /// </summary>
    public class Client
    {
        private const string _apiBaseUri = "https://graph.microsoft.com/v1.0";
        private const int _defaultUploadChunkSize = 7864320;
        private static HttpClient _client => HttpClientProvider.ClientForSettings(HttpClientSettings.WithoutBaseAddress());

        private readonly IClientInfoProvider _clientInfoProvider;

        private AccessTokenHeaderProvider _accessTokenHeaderProvider;
        private int _uploadChunkSize = _defaultUploadChunkSize;

        /// <summary>
        /// The size in bytes of the parts of a file that are upload at a time.
        /// </summary>
        public int UploadChunkSize
        {
            get
            {
                return _uploadChunkSize;
            }
            set
            {
                if (value < 0 || value % 327680 != 0)
                {
                    throw new ArgumentOutOfRangeException($"{nameof(UploadChunkSize)} should be a multiple of 320 KiB (327,680 bytes).");
                }
                if (value > 62914560)
                {
                    throw new ArgumentOutOfRangeException($"{nameof(UploadChunkSize)} should be less than 60 MiB (62914560 bytes).");
                }
                _uploadChunkSize = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Client"/> with the given implementation of <see cref="IClientInfoProvider"/>.
        /// </summary>
        /// <param name="clientInfoProvider"></param>
        public Client(IClientInfoProvider clientInfoProvider)
        {
            _clientInfoProvider = clientInfoProvider;
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
        /// Sets the upload chunk size, in bytes.
        /// </summary>
        /// <param name="uploadChunkSize"></param>
        public void SetUploadChunkSize(int uploadChunkSize)
        {
            _uploadChunkSize = uploadChunkSize;
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

        /// <summary>
        /// Uploads a stream to OneDrive, by creating a session using <see cref="CreateUploadSessionAsync(UploadMetaData)"/> and then uploading chunks of the stream to that session.
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task<DriveItem> UploadStreamAsync(UploadMetaData fileData, Stream stream)
        {
            var session = await CreateUploadSessionAsync(fileData);

            if (session.UploadUri == null)
            {
                throw new System.Exception();
            }

            return await UploadStreamAsync(stream, session.UploadUri);
        }

        /// <summary>
        /// Copies the content of the file with the given <paramref name="fileId"/> to <paramref name="outputStream"/>.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="outputStream"></param>
        /// <returns></returns>
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

        private async Task<DriveItem> UploadStreamAsync(Stream stream, Uri uri)
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            long totalLength = stream.Length;
            byte[] buffer = new byte[_defaultUploadChunkSize];
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
