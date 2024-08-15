using DG.Common.Http;
using DG.Common.Http.Fluent;
using DG.Common.Threading;
using DG.OneDrive.Serialized.DriveItems;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DG.OneDrive
{
    /// <summary>
    /// The byte contents of a <see cref="DriveItem"/>.
    /// </summary>
    public class DriveItemStream : Stream
    {
        private readonly long _contentLength;
        private readonly string _downloadUrl;
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of <see cref="DriveItemStream"/> with the given metadata.
        /// </summary>
        /// <param name="item"></param>
        public DriveItemStream(DownloadableDriveItem item)
        {
            Position = 0;

            _contentLength = item.size;
            _downloadUrl = item.downloadUrl;

            _client = HttpClientProvider.ClientForSettings(Client._clientSettings);
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

        private bool TryGetRequestForCount(int count, out FluentRequest request)
        {
            if (Position >= Length)
            {
                request = null;
                return false;
            }

            long lastInclusive = Position + count - 1;
            string rangeHeader = $"bytes={Position}-{lastInclusive}";
            if (lastInclusive >= Length)
            {
                rangeHeader = $"bytes={Position}-";
            }
            request = FluentRequest.Get.To(_downloadUrl)
                .WithHeader(new FluentHeader("Range", rangeHeader));

            return true;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!TryGetRequestForCount(count, out FluentRequest request))
            {
                return 0;
            }

            using (var response = _client.SendAsync(request).GetUnwrappedResult())
            using (var content = response.Content)
            {
                var contentBytes = content.ReadAsByteArrayAsync().GetUnwrappedResult();
                contentBytes.CopyTo(buffer, offset);

                Position += contentBytes.LongLength;
                return contentBytes.Length;
            }
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (!TryGetRequestForCount(count, out FluentRequest request))
            {
                return 0;
            }
            request = request.WithCancellationToken(cancellationToken);

            using (var response = await _client.SendAsync(request).ConfigureAwait(false))
            using (var content = response.Content)
            {
                var contentBytes = await content.ReadAsByteArrayAsync().ConfigureAwait(false);
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
