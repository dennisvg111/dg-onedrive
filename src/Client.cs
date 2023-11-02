using DG.Common.Http.Fluent;
using DG.OneDrive.Serialized;
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

        public async Task<Office365User> GetUser()
        {
            var request = FluentRequest.Get.To(_apiBaseUri + "/me")
                .WithAuthorizationHeaderProvider(_accessTokenHeaderProvider);

            return await _client.SendAndDeserializeAsync<Office365User>(request);
        }

        public void SetAccessToken(string accessToken)
        {
            _accessTokenHeaderProvider = new AccessTokenHeaderProvider(new Authentication(_clientInfoProvider), accessToken);
        }
    }
}
