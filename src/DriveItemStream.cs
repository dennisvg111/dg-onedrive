using DG.Common.Http;
using DG.Common.Http.Authorization.OAuth2;
using DG.Common.Http.Fluent;
using DG.Common.Threading;
using DG.OneDrive.Authorization;
using DG.OneDrive.Serialized.DriveItems;
using System.IO;

namespace DG.OneDrive
{
    /// <summary>
    /// The byte contents of a <see cref="DriveItem"/>.
    /// </summary>
    public class DriveItemStream : Stream
    {
        private readonly string _itemId;
        private readonly OAuthFlow<OneDriveOAuthLogic> _authorization;

        private readonly long _contentLength;
        private readonly string _downloadUrl;

        public DriveItemStream(string itemId, OAuthFlow<OneDriveOAuthLogic> authorization)
        {
            _itemId = itemId;
            _authorization = authorization;
            Position = 0;
            _downloadUrl = GetDownloadUrl(out _contentLength);
        }

        /// <inheritdoc/>
        public override long Position { get; set; }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => _contentLength;

        private string GetDownloadUrl(out long contentLength)
        {
            var request = FluentRequest.Get.To($"drive/items/{_itemId}?select=id,@microsoft.graph.downloadUrl,size")
                .WithAuthorization(_authorization);

            var client = HttpClientProvider.ClientForSettings(Client._clientSettings);
            var itemStub = client.SendAndDeserializeAsync<DownloadableDriveItem>(request).GetUnwrappedResult();
            contentLength = itemStub.size;
            return itemStub.downloadUrl;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Position >= Length)
            {
                return 0;
            }
            long lastInclusive = Position + count - 1;
            string rangeHeader = $"bytes={Position}-{lastInclusive}";
            if (lastInclusive >= Length)
            {
                rangeHeader = $"bytes={Position}-";
            }
            var request = FluentRequest.Get.To(_downloadUrl)
                .WithHeader(new FluentHeader("Range", rangeHeader));
            var client = HttpClientProvider.ClientForSettings(Client._clientSettings);
            using (var response = client.SendAsync(request).GetUnwrappedResult())
            using (var content = response.Content)
            {
                var contentBytes = content.ReadAsByteArrayAsync().GetUnwrappedResult();
                contentBytes.CopyTo(buffer, offset);
                Position += contentBytes.LongLength;
                return contentBytes.Length;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
            }
            return Position;
        }

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }
    }
}
