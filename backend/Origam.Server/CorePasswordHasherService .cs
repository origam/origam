using System;
using System.Globalization;
using System.Security.Cryptography;
using BrockAllen.IdentityReboot;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Origam.Security.Common;

namespace Origam.Server
{
    public class CorePasswordHasherService : IPasswordHasher<IOrigamUser>
    {
        private const int SALT_SIZE = 16; // 128 bit
        private const int SUBKEY_LENGTH = 32; // 256 bit
        private const int ITERATION_COUNT = 250000;
        private const string KEY_PREFIX = "$pbkdf2-sha256$";

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
                return legacyPasswordHasher.VerifyHashedPassword(
                    user,
                    hashedPassword,
                    providedPassword
                );
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

        public PasswordVerificationResult VerifyHashedPasswordA(
           IOrigamUser user,
           string hashedPassword,
           string providedPassword
       )
        {
            if (string.IsNullOrEmpty(hashedPassword))
            {
                return PasswordVerificationResult.Failed;
            }
            if (!isKnownFunction(hashedPassword))
            {
                // Old password format, using legacy hasher
                return legacyPasswordHasher.VerifyHashedPassword(
                    user,
                    hashedPassword,
                    providedPassword
                );
            }

            var parts = hashedPassword.Split(".");
            int count;
            // Tenhle parse se předtím vůbec neoveřoval
            if (Int32.TryParse(parts[0], NumberStyles.HexNumber, null, out count))
            {
                return PasswordVerificationResult.Failed;
            }
            hashedPassword = parts[1];

            byte[] hashedPasswordBytes;
            try
            {
                hashedPasswordBytes = Convert.FromBase64String(hashedPassword);
            }
            catch //Chyram vyjmku?
            {
                return PasswordVerificationResult.Failed;
            }

            //TODO Ma smysl delat check na delku klice?
            //if (hashedPasswordBytes.Length <= (KEY_PREFIX.Length + SALT_SIZE + SUBKEY_LENGTH))
            //{
            //    return PasswordVerificationResult.Failed;
            //}

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

            bool success = CryptographicOperations.FixedTimeEquals( //Misto ByteArraysEqual
                storedSubkey,
                generatedSubkey
            );
            return success ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;

            //VS stara verze
            //var verificationResult =
            //  (ByteArraysEqual(storedSubkey, generatedSubkey))
            //      ? VerificationResult.Success
            //      : VerificationResult.Failed;
            //return ToAspNetCoreResult(verificationResult);
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
            // [header].[salt][subkey]
            byte[] outputBytes = new byte[1 + salt.Length + subkey.Length];
            Buffer.BlockCopy(salt, 0, outputBytes, 1, salt.Length);
            Buffer.BlockCopy(subkey, 0, outputBytes, 1 + salt.Length, subkey.Length);
            return $"{KEY_PREFIX}.{Convert.ToBase64String(outputBytes)}";
        }

        public bool isKnownFunction(string password)
        {
            return password.StartsWith(KEY_PREFIX);
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
