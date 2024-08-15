using DG.Common;
using FluentAssertions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DG.OneDrive.Tests
{
    public class ClientTests
    {
        private const string _helloWorldFileId = "667B8052A954FAAB!24786";
        private const string _largeFileId = "667B8052A954FAAB!28518";

        private Client SetupClient()
        {
            var client = Client.FromSerializedAccessToken(EnvironmentClientInfoProvider.Default, EnvironmentAccessTokenProvider.AccessToken);
            return client;
        }

        [Fact]
        public async Task GetDrive_ReturnsState()
        {
            var client = SetupClient();

            var drive = await client.GetDriveAsync();

            drive.Should().NotBeNull();
            drive.Quota.Total.Should().Match<ByteSize>(t => t > 0);
        }

        [Fact]
        public async Task GetChildren_ContainsTestFile()
        {
            var client = SetupClient();

            var children = await client.GetChildrenAsync("/Tests");

            children.Should().NotBeNull();
            if (!children.Any())
            {
                return;
            }

            children.Should().Contain(file => file.description == "A test file");
        }

        [Fact]
        public async Task GetUser_WithAccessToken_ReturnsUser()
        {
            var client = SetupClient();

            var user = await client.GetCurrentUserAsync();

            user.Should().NotBeNull();
            user.odataContext.Should().Be("https://graph.microsoft.com/v1.0/$metadata#users/$entity");
        }

        [Fact]
        public async Task CopyTo_ReturnsContent()
        {
            var client = SetupClient();

            using (var stream = new MemoryStream())
            {
                await client.CopyToStreamAsync(_helloWorldFileId, stream);

                using (var streamReader = new StreamReader(stream))
                {
                    string content = streamReader.ReadToEnd();
                    content.Should().Be("Hello world! This is a test.");
                }
            }
        }

        private static readonly Process _currentProcess = Process.GetCurrentProcess();

        [Fact(Skip = "Large memory footprint test, only run in local environments")]
        public async Task CopyTo_MemoryFootprint()
        {
            var client = SetupClient();

            using (var file = new DummyFile())
            {
                var fileStream = file.GetStream();
                _currentProcess.Refresh();
                var usedMemoryBefore = _currentProcess.PrivateMemorySize64;

                await client.CopyToStreamAsync(_largeFileId, fileStream);

                _currentProcess.Refresh();
                var usedMemoryAfter = _currentProcess.PrivateMemorySize64;
                var difference = ByteSize.FromBytes(usedMemoryAfter - usedMemoryBefore);

                Assert.True(difference > ByteSize.FromMB(50), $"Expected more than 50MB, actual {difference}");
            }
        }

        [Fact(Skip = "Large memory footprint test, only run in local environments")]
        public async Task GetContent_MemoryFootprint()
        {
            var client = SetupClient();

            using (var file = new DummyFile())
            using (var content = await client.GetContentAsync(_largeFileId))
            {
                var fileStream = file.GetStream();

                _currentProcess.Refresh();
                var usedMemoryBefore = _currentProcess.PrivateMemorySize64;

                byte[] buffer = new byte[ByteSize.FromMB(10)];

                int read;
                while ((read = await content.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, read);
                    await fileStream.FlushAsync();

                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }

                _currentProcess.Refresh();
                var usedMemoryAfter = _currentProcess.PrivateMemorySize64;
                var difference = ByteSize.FromBytes(usedMemoryAfter - usedMemoryBefore);

                Assert.True(difference < ByteSize.FromMB(50), $"Expected less than 50MB, actual {difference}");
            }
        }

        internal class DummyFile : IDisposable
        {
            private readonly string _name;
            private readonly FileStream fs;
            private bool disposedValue;

            public override string ToString()
            {
                return _name;
            }

            public DummyFile()
            {
                Directory.CreateDirectory(@"C:\tmp");
                _name = @"C:\tmp\" + Guid.NewGuid().ToString().ToLowerInvariant() + ".dummy";
                fs = File.Create(_name, (int)ByteSize.FromMB(10).TotalBytes, FileOptions.WriteThrough);
            }

            public Stream GetStream()
            {
                return fs;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    fs.Dispose();
                    File.Delete(_name);
                }
                disposedValue = true;
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
