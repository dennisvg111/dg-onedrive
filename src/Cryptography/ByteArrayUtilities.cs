namespace DG.OneDrive.Cryptography
{
    internal static class ByteArrayUtilities
    {
        public static byte[] CreateCopy(this byte[] source)
        {
            var array = new byte[source.Length];
            source.CopyTo(array, 0);
            return array;
        }
    }
}
