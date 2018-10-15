using System;
using System.Security.Cryptography;
using System.Text;

namespace Origam.Rule
{
    public class XslCryptoFunctions
    {
        public const string Crypto = "http://xsl.origam.com/crypto";

        public string Nonce()
        {
            Guid guid = Guid.NewGuid();
            return Convert.ToBase64String(guid.ToByteArray());
        }

        public string PasswordDigest(
            string password, string timestamp, string nonce)
        {
            SHA1 sha1 = SHA1Managed.Create();
            byte[] passwordHashedBytes = sha1.ComputeHash(
                Encoding.UTF8.GetBytes(password));
            byte[] timestampBytes 
                = string.IsNullOrEmpty(timestamp) 
                ? new byte[0] 
                : Encoding.UTF8.GetBytes(timestamp);
            byte[] nonceBytes 
                = string.IsNullOrEmpty(nonce) 
                ? new byte[0] 
                : Convert.FromBase64String(nonce);
            byte[] input = new byte[
                passwordHashedBytes.Length + timestampBytes.Length 
                + nonceBytes.Length];
            int offset = 0;
            nonceBytes.CopyTo(input, offset);
            offset += nonceBytes.Length;
            timestampBytes.CopyTo(input, offset);
            offset += timestampBytes.Length;
            passwordHashedBytes.CopyTo(input, offset);
            return Convert.ToBase64String(sha1.ComputeHash(input));
        }
    }
}
