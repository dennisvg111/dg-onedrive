using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DG.OneDrive.Tests
{
    public class ClientTests
    {
        private Client SetupClient()
        {
            var client = new Client(EnvironmentClientInfoProvider.Default);
            client.SetAccessToken(EnvironmentAccessTokenProvider.AccessToken);
            return client;
        }

        [Fact]
        public async Task GetDrive_ReturnsState()
        {
            var client = SetupClient();

            var drive = await client.GetDriveAsync();

            Assert.NotNull(drive);
            Assert.True(drive.Quota.Total > 0);
        }

        [Fact]
        public async Task GetChildren_ContainsTestFile()
        {
            var client = SetupClient();

            var children = await client.GetChildrenAsync("/Tests");

            Assert.NotNull(children);
            if (!children.Any())
            {
                return;
            }

            Assert.Contains(children, file => file.description == "A test file");
        }

        [Fact]
        public async Task GetUser_WithAccessToken_ReturnsUser()
        {
            var client = SetupClient();

            var user = await client.GetCurrentUserAsync();

            Assert.NotNull(user);
            Assert.Equal("https://graph.microsoft.com/v1.0/$metadata#users/$entity", user.odataContext);
        }

        [Fact]
        public async Task CopyTo_ReturnsContent()
        {
            var client = SetupClient();
            var id = "667B8052A954FAAB!24786";
            string expectedContent = "Hello world! This is a test.";

            using (var stream = new MemoryStream())
            {
                await client.CopyToStreamAsync(id, stream);

                using (var streamReader = new StreamReader(stream))
                {
                    string content = streamReader.ReadToEnd();

                    Assert.Equal(expectedContent, content);
                }
            }
        }
    }
}
