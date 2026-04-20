using System;
using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Origam.Security.Common;

namespace Origam.Server
{
    public class CorePasswordHasherService : IPasswordHasher<IOrigamUser>
    {
        private const int SALT_SIZE = 64; // 64 bytes
        private const int SUBKEY_LENGTH = 32; // 256 bit
        private const int ITERATION_COUNT = 600000;
        private const string KEY_PREFIX = "pbkdf2-sha256";
        private const int KEY_PARTS_LENGTH = 4;

        private IPasswordHasher<IOrigamUser> legacyPasswordHasher;

        public CorePasswordHasherService()
        {
            legacyPasswordHasher = new CorePasswordHasher();
        }

        public PasswordVerificationResult VerifyHashedPassword(
            IOrigamUser user,
            string hashedPassword,
            string providedPassword
        )
        {
            if (string.IsNullOrEmpty(hashedPassword))
            {
                return PasswordVerificationResult.Failed;
            }

            var parts = hashedPassword.Split(".");
            var prefix = parts[0];

            if (parts.Length != KEY_PARTS_LENGTH || !isKnownFunction(prefix))
            {
                // Old password format, using legacy hasher
                var result = legacyPasswordHasher.VerifyHashedPassword(
                    user,
                    hashedPassword,
                    providedPassword
                );
                return (result == PasswordVerificationResult.Success)
                    ? PasswordVerificationResult.SuccessRehashNeeded
                    : result;
            }

            int count;
            // Tenhle parse se předtím vůbec neoveřoval
            if (!Int32.TryParse(parts[1], NumberStyles.HexNumber, null, out count))
            {
                return PasswordVerificationResult.Failed;
            }

            string saltString = parts[2];
            hashedPassword = parts[3];

            byte[] saltBytes;
            byte[] hashedPasswordBytes;
            try
            {
                saltBytes = Convert.FromBase64String(saltString);
                hashedPasswordBytes = Convert.FromBase64String(hashedPassword);
            }
            catch
            {
                return PasswordVerificationResult.Failed;
            }

            byte[] generatedSubkey = Rfc2898DeriveBytes.Pbkdf2(
                providedPassword,
                saltBytes,
                count,
                HashAlgorithmName.SHA256,
                SUBKEY_LENGTH
            );

            bool success = CryptographicOperations.FixedTimeEquals(
                hashedPasswordBytes,
                generatedSubkey
            );

            if (success && count != ITERATION_COUNT)
            {
                return PasswordVerificationResult.SuccessRehashNeeded;
            }
            if (success && saltBytes.Length != SALT_SIZE)
            {
                return PasswordVerificationResult.SuccessRehashNeeded;
            }
            return success ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
        }

        public string HashPassword(IOrigamUser user, string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            byte[] salt = new byte[SALT_SIZE];
            RandomNumberGenerator.Fill(salt);
            byte[] subkey = KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                ITERATION_COUNT,
                SUBKEY_LENGTH
            );
            // [prefix].[iteration_count in hexdecimal].[salt in base64].[subkey in base64]
            return $"{KEY_PREFIX}.{ITERATION_COUNT.ToString("X")}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(subkey)}";
        }

        public bool isKnownFunction(string prefix)
        {
            return prefix == KEY_PREFIX;
        }
    }
}
