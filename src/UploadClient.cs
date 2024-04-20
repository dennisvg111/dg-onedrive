using DG.Common.Http;
using DG.Common.Http.Authorization.OAuth2;
using DG.Common.Http.Fluent;
using DG.OneDrive.Authorization;
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
        private readonly OAuthFlow<OneDriveOAuthLogic> _authorization;

        private static HttpClient _client => HttpClientProvider.ClientForSettings(HttpClientSettings.WithBaseAddress(_apiBaseUri));

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

        internal UploadClient(OAuthFlow<OneDriveOAuthLogic> authorization)
        {
            _authorization = authorization;
        }

        /// <summary>
        /// Creates a new upload session.
        /// </summary>
        /// <param name="information"></param>
        /// <returns></returns>
        public async Task<UploadSession> CreateSessionAsync(UploadMetaData information)
        {
            var container = UploadRequest.WithMetaData(information);

            string uri = $"me/drive/special/approot:/{information.Path.TrimStart('/')}/{information.NameWithExtension}:/createUploadSession";
            var request = FluentRequest.Post.To(uri)
                .WithAuthorization(_authorization)
                .WithSerializedJsonContent(container);

            return await _client.SendAndDeserializeAsync<UploadSession>(request).ConfigureAwait(false);
        }

        /// <summary>
        /// Uploads a <see cref="Stream"/> to the given <paramref name="session"/>.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task<DriveItem> ToSessionAsync(UploadSession session, Stream stream)
        {
            return await UploadStreamAsync(stream, session.UploadUri).ConfigureAwait(false);
        }

        /// <summary>
        /// Uploads a stream to OneDrive, by creating a session using <see cref="CreateSessionAsync(UploadMetaData)"/> and then uploading chunks of the stream to that session.
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task<DriveItem> ToNewSessionAsync(UploadMetaData fileData, Stream stream)
        {
            var session = await CreateSessionAsync(fileData).ConfigureAwait(false);

            if (session?.UploadUri == null)
            {
                throw new OneDriveUploadSessionException(fileData);
            }

            return await UploadStreamAsync(stream, session.UploadUri).ConfigureAwait(false);
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

                lastResult = await _client.SendAsync(request).ConfigureAwait(false);
                offset += byteCount;
            }

            if (lastResult == null)
            {
                throw new EndOfStreamException("End of stream reached before uploading could start.");
            }

            var json = await lastResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<DriveItem>(json);
        }
    }
}
