using DG.Common.Http.Fluent;
using DG.OneDrive.Exceptions;
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
    /// Provides uploading functionality for OneDrive.
    /// </summary>
    public class UploadClient
    {
        private const string _apiBaseUri = "https://graph.microsoft.com/v1.0/";
        private const int _defaultUploadChunkSize = 7864320;

        private static HttpClient _client => HttpClientProvider.ClientForSettings(HttpClientSettings.WithBaseAddress(_apiBaseUri));

        private readonly Func<AccessTokenHeaderProvider> _authorizationProvider;
        private int _uploadChunkSize = _defaultUploadChunkSize;

        /// <summary>
        /// The size in bytes of the parts of a file that are upload at a time.
        /// </summary>
        public int ChunkSize
        {
            get
            {
                return _uploadChunkSize;
            }
            set
            {
                if (value < 0 || value % 327680 != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(ChunkSize), $"{nameof(ChunkSize)} should be a multiple of 320 KiB (327,680 bytes).");
                }
                if (value > 62914560)
                {
                    throw new ArgumentOutOfRangeException(nameof(ChunkSize), $"{nameof(ChunkSize)} should be less than 60 MiB (62914560 bytes).");
                }
                _uploadChunkSize = value;
            }
        }

        internal UploadClient(Func<AccessTokenHeaderProvider> accessTokenHeaderProvider)
        {
            _authorizationProvider = accessTokenHeaderProvider;
        }

        /// <summary>
        /// Creates a new upload session.
        /// </summary>
        /// <param name="information"></param>
        /// <returns></returns>
        public async Task<UploadSession> CreateUploadSessionAsync(UploadMetaData information)
        {
            var container = UploadRequest.WithMetaData(information);

            var authorization = _authorizationProvider();

            string uri = $"me/drive/special/approot:/{information.Path.TrimStart('/')}/{information.NameWithExtension}:/createUploadSession";
            var request = FluentRequest.Post.To(uri)
                .WithHeader(FluentHeader.Authorization(authorization))
                .WithSerializedJsonContent(container);

            return await _client.SendAndDeserializeAsync<UploadSession>(request);
        }

        /// <summary>
        /// Uploads a <see cref="Stream"/> to the given <paramref name="session"/>.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task<DriveItem> UploadToSessionAsync(UploadSession session, Stream stream)
        {
            return await UploadStreamAsync(stream, session.UploadUri);
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

            if (session?.UploadUri == null)
            {
                throw new OneDriveUploadSessionException(fileData);
            }

            return await UploadStreamAsync(stream, session.UploadUri);
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

            if (lastResult == null)
            {
                throw new EndOfStreamException("End of stream reached before uploading could start.");
            }

            var json = await lastResult.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DriveItem>(json);
        }
    }
}
