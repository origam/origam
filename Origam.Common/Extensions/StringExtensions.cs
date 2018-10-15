using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Origam.Extensions
{
    public static class StringExtensions
    {
        public static string FirstToUpper(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.Substring(0,1).ToUpper() +
                                input.Substring(1).ToLower();
            }
        }
        public static string[] Split(this string str, string splitWith) => 
            str.Split(new [] {splitWith}, StringSplitOptions.None);

        public static string ReplaceInvalidFileCharacters(
            this string fileNameCandidate, string replaceWith)
        {
            string regex = String.Format("[{0}]",
                Regex.Escape(new string(Path.GetInvalidFileNameChars())));
            Regex removeInvalidChars = new Regex(regex,
                RegexOptions.Singleline | RegexOptions.Compiled |
                RegexOptions.CultureInvariant);
            return removeInvalidChars.Replace(fileNameCandidate, replaceWith);
        }

        public static string GetBase64Hash(this string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            using (MD5 md5 = MD5.Create())
            {
                return Convert.ToBase64String(md5.ComputeHash(new MemoryStream(bytes)));
            }
        }
    }
}