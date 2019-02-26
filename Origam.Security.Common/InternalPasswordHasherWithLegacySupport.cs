using System;
using System.Security.Cryptography;
using System.Text;
using BrockAllen.IdentityReboot;

namespace Origam.Security.Common
{
      public class InternalPasswordHasherWithLegacySupport : AdaptivePasswordHasher
    {
        public VerificationResult VerifyHashedPassword(
            string hashedPassword, string providedPassword)
        {
            if (String.IsNullOrWhiteSpace(hashedPassword))
            {
                return VerificationResult.Failed;
            }
            string[] passwordProperties = hashedPassword.Split('|');
            if (passwordProperties.Length != 2)
            {
                // use AdaptiveHasher
                return base.VerifyHashedPassword(
                    hashedPassword, providedPassword);
            }
            else
            {
                // migrated account from NetMembership
                // format hashedFormat|salt
                string passwordHash = passwordProperties[0];
                string salt = passwordProperties[1];
                if (String.Equals(EncryptPassword(providedPassword, salt), 
                    passwordHash, StringComparison.CurrentCultureIgnoreCase))
                {
                    return VerificationResult.SuccessRehashNeeded;
                }
                else
                {
                    return VerificationResult.Failed;
                }
            }
        }

        private string EncryptPassword(string pass, string salt)
        {
            byte[] bIn = Encoding.Unicode.GetBytes(pass);
            byte[] bSalt = Convert.FromBase64String(salt);
            byte[] bRet = null;
            HashAlgorithm hm = HashAlgorithm.Create("SHA1");
            if (hm is KeyedHashAlgorithm)
            {
                KeyedHashAlgorithm kha = (KeyedHashAlgorithm)hm;
                if (kha.Key.Length == bSalt.Length)
                {
                    kha.Key = bSalt;
                }
                else if (kha.Key.Length < bSalt.Length)
                {
                    byte[] bKey = new byte[kha.Key.Length];
                    Buffer.BlockCopy(bSalt, 0, bKey, 0, bKey.Length);
                    kha.Key = bKey;
                }
                else
                {
                    byte[] bKey = new byte[kha.Key.Length];
                    for (int iter = 0; iter < bKey.Length; )
                    {
                        int len = Math.Min(bSalt.Length, bKey.Length - iter);
                        Buffer.BlockCopy(bSalt, 0, bKey, iter, len);
                        iter += len;
                    }
                    kha.Key = bKey;
                }
                bRet = kha.ComputeHash(bIn);
            }
            else
            {
                byte[] bAll = new byte[bSalt.Length + bIn.Length];
                Buffer.BlockCopy(bSalt, 0, bAll, 0, bSalt.Length);
                Buffer.BlockCopy(bIn, 0, bAll, bSalt.Length, bIn.Length);
                bRet = hm.ComputeHash(bAll);
            }
            return Convert.ToBase64String(bRet);
        }
    }
}