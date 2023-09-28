using System.IO;
using System.Security.Cryptography;

namespace DG.OneDrive.Cryptography
{
    public class StreamEncryptor
    {
        private readonly Stream _stream;
        private readonly byte[] _salt;
        private readonly byte[] _iv;

        public byte[] Salt => _salt.CreateCopy();
        public byte[] IV => _iv.CreateCopy();

        public Stream GetStream()
        {
            return _stream;
        }

        internal StreamEncryptor(CryptoStream stream, EncryptionKey key)
        {
            _stream = stream;
            _salt = key.Salt;
            _iv = key.IV;
        }
    }
}
