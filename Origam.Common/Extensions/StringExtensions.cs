#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion
﻿using System;
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