using DG.OneDrive.Serialized.DriveItems;
using DG.OneDrive.Serialized.Resources;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DG.OneDrive.Tests
{
    public class UploadClientTests
    {
        private UploadClient SetupClient()
        {
            var client = new Client(EnvironmentClientInfoProvider.Default);
            client.SetAccessToken(EnvironmentAccessTokenProvider.AccessToken);
            return client.Upload;
        }

        [Fact]
        public void Client_DefaultChunkSize_IsRecommended()
        {
            var client = SetupClient();

            var size = client.ChunkSize;

            Assert.True(size % 327680 == 0, "Upload chunk size should be a multiple of 320 KiB (327,680 bytes).");
            Assert.True(size >= 5242880, "Default upload chunk size should be at least 5 MiB (5,242,880 bytes).");
            Assert.True(size <= 10485760, "Default upload chunk size should be at most 10 MiB (10,485,760 bytes).");
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
    }
}
