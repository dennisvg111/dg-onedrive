using DG.Common;
using DG.OneDrive.Cryptography;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;

namespace DG.OneDrive.Tests.Cryptography
{
    public class EncryptionTests
    {
        private static readonly Process _currentProcess = Process.GetCurrentProcess();

        [Fact]
        public void CreateEncryptor_CanDecode()
        {
            var text = "Hello world!";
            var password = "password";

            using (var original = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                var encryptor = StreamEncryption.CreateEncryptor(original, password);
                Assert.NotNull(encryptor);

                var encryptedStream = encryptor.GetStream();

                using (var decrypted = StreamEncryption.Decode(encryptedStream, EncryptionKey.For(password, encryptor.Salt, encryptor.IV)))
                using (var reader = new StreamReader(decrypted))
                {
                    var decryptedText = reader.ReadToEnd();
                    Assert.Equal(text, decryptedText);
                }
            }
        }

        [Fact(Skip = "Large memory footprint test, not for use in production")]
        public void CreateEncryptor_MemoryUsage()
        {
            using (var file = new DummyFile())
            using (var stream = file.GetStream())
            {
                var encryptor = StreamEncryption.CreateEncryptor(stream, "password");
                Assert.NotNull(encryptor);

                var internalStream = encryptor.GetStream();
                var firstByte = internalStream.ReadByte();
                Assert.NotEqual(-1, firstByte);


                _currentProcess.Refresh();
                var usedMemory = ByteSize.FromBytes(_currentProcess.PrivateMemorySize64);
                Assert.True(usedMemory < ByteSize.FromMB(500), $"Expected less than 500MB, actual {usedMemory}");

                using (var ms = new MemoryStream())
                {
                    internalStream.CopyTo(ms);
                    _currentProcess.Refresh();
                    usedMemory = ByteSize.FromBytes(_currentProcess.PrivateMemorySize64);
                    Assert.True(usedMemory > ByteSize.FromMB(500), $"Expected more than 500MB, actual {usedMemory}");
                }
            }
        }

        public class DummyFile : IDisposable
        {
            private static long bytes = ByteSize.FromMB(500);
            private readonly string _name;
            private bool disposedValue;

            public override string ToString()
            {
                return _name + ": " + ByteSize.FromBytes(bytes);
            }

            public DummyFile()
            {
                Directory.CreateDirectory(@"C:\tmp");
                _name = @"C:\tmp\" + Guid.NewGuid().ToString().ToLowerInvariant() + ".dummy";
                FileStream fs = new FileStream(_name, FileMode.CreateNew);
                fs.Seek(bytes - 1, SeekOrigin.Begin);
                fs.WriteByte(0);
                fs.Close();
            }

            public Stream GetStream()
            {
                return File.OpenRead(_name);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    File.Delete(_name);
                }
                disposedValue = true;
            }

            // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
            // ~DummyFile()
            // {
            //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            //     Dispose(disposing: false);
            // }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
