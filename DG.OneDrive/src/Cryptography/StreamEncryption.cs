using System.IO;
using System.Security.Cryptography;

namespace DG.OneDrive.Cryptography
{
    public static class StreamEncryption
    {

        public static StreamEncryptor CreateEncryptor(this Stream source, string password)
        {
            var encryptionKey = EncryptionKey.GenerateForPassword(password);

            var aes = encryptionKey.GetAes();

            ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);

            var cryptoStream = new CryptoStream(source, transform, CryptoStreamMode.Read);
            return new StreamEncryptor(cryptoStream, encryptionKey);
        }

        public static Stream Decode(this Stream source, EncryptionKey key)
        {
            var aes = key.GetAes();

            ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);

            var cryptoStream = new CryptoStream(source, transform, CryptoStreamMode.Read);
            return cryptoStream;
        }
    }
}
