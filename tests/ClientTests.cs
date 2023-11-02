using System.Threading.Tasks;
using Xunit;

namespace DG.OneDrive.Tests
{
    public class ClientTests
    {
        [Fact]
        public async Task GetUser_WithAccessToken_ReturnsUser()
        {
            var client = new Client(EnvironmentClientInfoProvider.Instance);

            client.SetAccessToken(EnvironmentClientInfoProvider.AccessToken);

            var user = await client.GetCurrentUser();

            Assert.NotNull(user);
            Assert.Equal("https://graph.microsoft.com/v1.0/$metadata#users/$entity", user.odataContext);
        }
    }
}
