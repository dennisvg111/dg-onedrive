using DG.OneDrive.Serialized.Resources;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DG.OneDrive.Tests
{
    public class ClientTests
    {
        private Client SetupClient()
        {
            var client = new Client(EnvironmentClientInfoProvider.Instance);
            client.SetAccessToken(EnvironmentClientInfoProvider.AccessToken);
            return client;
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
        public async Task UploadFile()
        {
            var client = SetupClient();
            var uploadInformation = new UploadMetaData()
            {
                NameWithExtension = $"test-file {DateTime.Now:yyyy-MM-dd HHmmss}.txt",
                Path = "/Tests",
                ConflictBehavior = UploadConflictBehavior.Rename,
                Description = "A test file"
            };
            string fileText = "Hello world! This is a test.";

            using (var dummyFile = new MemoryStream(Encoding.UTF8.GetBytes(fileText)))
            {
                await client.UploadStreamAsync(uploadInformation, dummyFile);
            }
        }
    }
}
