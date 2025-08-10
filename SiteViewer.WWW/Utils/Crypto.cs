using System.Text;

namespace SiteViewer.WWW.Utils
{
    public static class Crypto
    {
        public static string MD5Hash(string input, Encoding? encoding = null)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            
            encoding ??= Encoding.UTF8;

            var inputBytes = encoding.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        public static string SHA256Hash(string input, Encoding? encoding = null)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();

            encoding ??= Encoding.UTF8;
            var inputBytes = encoding.GetBytes(input);
            var hashBytes = sha256.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
