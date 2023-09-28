using DG.Cryptography.Random;
using System.Security.Cryptography;

namespace DG.OneDrive.Cryptography
{
    public class EncryptionKey
    {
        private const int _iterations = 1042; // Recommendation is >= 1000.
        private static readonly SecureRandomNumberProvider _randomBytes = new SecureRandomNumberProvider();

        private readonly AesManaged _aes;
        private readonly byte[] _salt;
        private readonly byte[] _iv;

        public byte[] Salt => _salt.CreateCopy();
        public byte[] IV => _iv.CreateCopy();

        public EncryptionKey(AesManaged aes, byte[] salt, byte[] iv)
        {
            _aes = aes;
            _salt = salt;
            _iv = iv;
        }

        internal AesManaged GetAes()
        {
            return _aes;
        }

        public static EncryptionKey For(string password, byte[] salt, byte[] iv)
        {
            AesManaged aes = new AesManaged();
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            aes.IV = iv;

            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, _iterations);
            aes.Key = key.GetBytes(aes.KeySize / 8);

            aes.Mode = CipherMode.CBC;

            return new EncryptionKey(aes, salt, iv);
        }

        internal static EncryptionKey GenerateForPassword(string password)
        {
            byte[] salt = _randomBytes.NextBytes(16);

            AesManaged aes = new AesManaged();
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;

            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, _iterations);
            aes.Key = key.GetBytes(aes.KeySize / 8);

            byte[] iv = _randomBytes.NextBytes(aes.BlockSize / 8);
            aes.IV = iv;

            aes.Mode = CipherMode.CBC;

            return new EncryptionKey(aes, salt, iv);
        }
    }
}
