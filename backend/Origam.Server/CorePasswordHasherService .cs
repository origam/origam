using System;
using System.Globalization;
using System.Security.Cryptography;
using BrockAllen.IdentityReboot;
using Microsoft.AspNetCore.Identity;
using Origam.Security.Common;

namespace Origam.Server
{
    public class CorePasswordHasherService : IPasswordHasher<IOrigamUser>
    {
        private const int SALT_SIZE     = 16; // 128 bit
        private const int SUBKEY_LENGTH = 32; // 256 bit

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
            if (!isKnownFunction(hashedPassword))
            {
                // Old password format, using legacy hasher
                return legacyPasswordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
            }

            var parts = hashedPassword.Split(".");
            int count;
            Int32.TryParse(parts[0], NumberStyles.HexNumber, null, out count);
            hashedPassword = parts[1];
            byte[] hashedPasswordBytes = Convert.FromBase64String(hashedPassword);
            byte[] salt = new byte[SALT_SIZE];
            Buffer.BlockCopy(hashedPasswordBytes, 1, salt, 0, SALT_SIZE);
            byte[] storedSubkey = new byte[SUBKEY_LENGTH];
            Buffer.BlockCopy(hashedPasswordBytes, 1 + SALT_SIZE, storedSubkey, 0, SUBKEY_LENGTH);
            byte[] generatedSubkey;

            generatedSubkey = Rfc2898DeriveBytes.Pbkdf2(
                providedPassword,
                salt,
                count,
                HashAlgorithmName.SHA256,
                SUBKEY_LENGTH
            );
            var verificationResult =
                (ByteArraysEqual(storedSubkey, generatedSubkey))
                    ? VerificationResult.Success
                    : VerificationResult.Failed;
            return ToAspNetCoreResult(verificationResult);
        }

        public string HashPassword(IOrigamUser user, string password)
        {
            int iterationCount = 250000;

            if (password == null)
            {
                throw new ArgumentNullException("password");
            }

            byte[] salt = RandomNumberGenerator.GetBytes(SALT_SIZE);
            byte[] subkey = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterationCount,
                HashAlgorithmName.SHA256,
                SUBKEY_LENGTH
            );

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var derivedBytes = Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivation.Pbkdf2(
                password,
                salt,
                Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivationPrf.HMACSHA512,
                iterationCount,
                SUBKEY_LENGTH
            );

            // [ver][salt][subkey]
            byte[] outputBytes = new byte[1 + SALT_SIZE + SUBKEY_LENGTH];
            outputBytes[0] = 0x01; // version marker JE TO POTREBA?????

            Buffer.BlockCopy(salt, 0, outputBytes, 1, SALT_SIZE);
            Buffer.BlockCopy(subkey, 0, outputBytes, 1 + SALT_SIZE, SUBKEY_LENGTH);

            return Convert.ToBase64String(outputBytes);
        }

        public bool isKnownFunction(string password)
        {
            return password.StartsWith("$pbkdf2-sha5");
        }

        // CpyPaste from CorePasswordHasher.cs
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            if ((a == null) || (b == null) || (a.Length != b.Length))
            {
                return false;
            }
            bool areSame = true;
            for (int i = 0; i < a.Length; i++)
            {
                areSame &= (a[i] == b[i]);
            }
            return areSame;
        }

        // CpyPaste from CorePasswordHasher.cs
        private static PasswordVerificationResult ToAspNetCoreResult(VerificationResult result)
        {
            switch (result)
            {
                case VerificationResult.Failed:
                    return PasswordVerificationResult.Failed;
                case VerificationResult.Success:
                    return PasswordVerificationResult.Success;
                case VerificationResult.SuccessRehashNeeded:
                    return PasswordVerificationResult.SuccessRehashNeeded;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
