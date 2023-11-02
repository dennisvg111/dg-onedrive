using DG.Common.Http.Fluent;
using DG.OneDrive.Serialized;
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
        public async Task<Office365User> GetCurrentUser()
        {
            var request = FluentRequest.Get.To(_apiBaseUri + "/me")
                .WithAuthorizationHeaderProvider(_accessTokenHeaderProvider);

            return await _client.SendAndDeserializeAsync<Office365User>(request);
        }

        private async Task<UploadSession> CreateUploadSession(UploadInformation information)
        {
            var container = UploadInformationContainer.ForUpload(information);

            string uri = _apiBaseUri + $"/me/drive/special/approot:/{information.Path.TrimStart('/')}/{information.NameWithExtension}:/createUploadSession";
            var request = FluentRequest.Post.To(uri)
                .WithAuthorizationHeaderProvider(_accessTokenHeaderProvider)
                .WithSerializedJsonContent(container);

            return await _client.SendAndDeserializeAsync<UploadSession>(request);
        }

        public async Task UploadFile(UploadInformation fileData, Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            var session = await CreateUploadSession(fileData);

            long totalLength = stream.Length;
            byte[] buffer = new byte[_maxUploadChunkSize];
            long offset = 0;
            while (true)
            {
                int byteCount = stream.Read(buffer, 0, buffer.Length);
                if (byteCount <= 0)
                {
                    break;
                }

                FluentRequest.Put.To(session.uploadUrl)
                    .WithContent()

                HttpRequestMessage uploadMessage = new HttpRequestMessage(HttpMethod.Put, session.uploadUrl)
                {
                    Content = new ByteArrayContent(buffer, 0, byteCount)
                };
                uploadMessage.Content.Headers.Add("Content-Length", byteCount.ToString());
                string contentRange = $"bytes {offset}-{(offset + byteCount - 1)}/{totalLength}";
                uploadMessage.Content.Headers.Add("Content-Range", contentRange);
                var result = _client.SendAsync(uploadMessage).Result;
                var response = result.Content.ReadAsStringAsync();
                offset += byteCount;
            }
        }
    }
}
