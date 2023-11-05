using DG.OneDrive.Serialized.DriveItems;
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
            var client = new Client(EnvironmentClientInfoProvider.Default);
            client.SetAccessToken(EnvironmentAccessTokenProvider.AccessToken);
            return client;
        }

        [Fact]
        public void Client_DefaultUploadChunkSize_IsRecommended()
        {
            var client = SetupClient();

            var size = client.UploadChunkSize;

            Assert.True(size % 327680 == 0, "Upload chunk size should be a multiple of 320 KiB (327,680 bytes).");
            Assert.True(size >= 5242880, "Default upload chunk size should be at least 5 MiB (5,242,880 bytes).");
            Assert.True(size <= 10485760, "Default upload chunk size should be at most 10 MiB (10,485,760 bytes).");
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
        public async Task UploadFile_ReturnsNewFileId()
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

            DriveItem newFile;

            using (var dummyFile = new MemoryStream(Encoding.UTF8.GetBytes(fileText)))
            {
                newFile = await client.UploadStreamAsync(uploadInformation, dummyFile);
            }

            Assert.NotNull(newFile);
            Assert.NotNull(newFile.id);
        }

        [Fact]
        public async Task DownloadFile_ReturnsContent()
        {
            var client = SetupClient();
            var id = "667B8052A954FAAB!24786";
            string expectedContent = "Hello world! This is a test.";

            using (var stream = await client.DownloadStreamAsync(id))
            {
                Assert.NotNull(stream);

                using (var streamReader = new StreamReader(stream))
                {
                    string content = streamReader.ReadToEnd();

                    Assert.Equal(expectedContent, content);
                }
            }
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
