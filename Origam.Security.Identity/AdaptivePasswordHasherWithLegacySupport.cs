using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BrockAllen.IdentityReboot;
using Microsoft.AspNet.Identity;

// legacy password verification based on: 
// http://www.asp.net/identity/overview/migrations/migrating-an-existing-website-from-sql-membership-to-aspnet-identity
namespace Origam.Security.Identity
{
    public class AdaptivePasswordHasherWithLegacySupport : AdaptivePasswordHasher
    {
        override public PasswordVerificationResult VerifyHashedPassword(
            string hashedPassword, string providedPassword)
        {
            if (String.IsNullOrWhiteSpace(hashedPassword))
            {
                return PasswordVerificationResult.Failed;
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
                    return PasswordVerificationResult.SuccessRehashNeeded;
                }
                else
                {
                    return PasswordVerificationResult.Failed;
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
