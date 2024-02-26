

using System.Security.Cryptography;

namespace AuthService.Common.Helpers
{
    public static class HashHelper
    {
        public static string GetHashFromString(string value)
        {
            using var rngCsp = RandomNumberGenerator.Create();
            byte[] salt;

            rngCsp.GetBytes(salt = new byte[16]);
            var pbkdf2 = new Rfc2898DeriveBytes(value, salt, 1000000, HashAlgorithmName.SHA256);

            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];

            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }

        public static bool IsEqual(string hash, string value)
        {
            byte[] hashBytes = Convert.FromBase64String(hash);

            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(value, salt, 1000000, HashAlgorithmName.SHA256);
            byte[] resultHash = pbkdf2.GetBytes(20);

            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != resultHash[i])
                    return false;

            return true;
        }

        public static string GenerateVerificationCode()
        {
            return new Random().Next(0, 1000000).ToString("### ###");
        }

        public static string GenerateGUID()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
