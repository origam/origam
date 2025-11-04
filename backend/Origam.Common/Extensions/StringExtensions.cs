#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Origam.Extensions;

public static class StringExtensions
{
    private static readonly Graphics graphics = Graphics.FromImage(new Bitmap(1, 1));

    public static int Width(this string text, Font font)
    {
        return (int)graphics.MeasureString(text, font).Width;
    }

    public static int Height(this string text, Font font)
    {
        return (int)graphics.MeasureString(text, font).Height;
    }

    public static string Wrap(this String text, int widthInPixels, Font font)
    {
        var unwrapedTextWidth = text.Width(font);
        if (unwrapedTextWidth > widthInPixels)
        {
            var indexAtButtonEdge = WidthToIndex(
                text: text,
                targetPosition: widthInPixels,
                font: font
            );
            var indexOfNearestSpace = GetIndexOfNearestSpace(text, indexAtButtonEdge);
            var wrappedText = text.Insert(indexOfNearestSpace, Environment.NewLine);
            return wrappedText;
        }
        return text;
    }

    private static int WidthToIndex(string text, int targetPosition, Font font)
    {
        int indexEstimation = text.Length / 2;
        int i = indexEstimation;
        for (int n = 0; n < text.Length; n++)
        {
            var position = (int)graphics.MeasureString(text.Substring(0, i), font).Width;
            var error = targetPosition - position;
            if (error < 5)
            {
                return i;
            }
            if (error > 0)
            {
                i++;
            }
            else if (error < 0)
            {
                i--;
            }

            if (i >= text.Length)
            {
                return i - 1;
            }

            if (i < 0)
            {
                return 0;
            }
        }
        throw new Exception("Could not convert pixels to index in suplied string.");
    }

    private static int GetIndexOfNearestSpace(string text, int pivotIndex)
    {
        var canBeSplit = text.Contains(' ');
        if (!canBeSplit)
        {
            return text.Length;
        }

        var spaceIndices = FindAllSpaceIndices(text);
        return spaceIndices
            .OrderBy(spaceIndex => Math.Abs(pivotIndex - spaceIndex))
            .FirstOrDefault();
    }

    private static List<int> FindAllSpaceIndices(string text)
    {
        var indicesOfSpaces = new List<int>();
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == ' ')
            {
                indicesOfSpaces.Add(i);
            }
        }
        return indicesOfSpaces;
    }

    public static string FirstToUpper(this string input)
    {
        switch (input)
        {
            case null:
            {
                throw new ArgumentNullException(nameof(input));
            }
            case "":
            {
                throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
            }
            default:
            {
                return input.Substring(0, 1).ToUpper() + input.Substring(1).ToLower();
            }
        }
    }

    public static string[] Split(this string str, string splitWith) =>
        str.Split(new[] { splitWith }, StringSplitOptions.None);

    public static string ReplaceInvalidFileCharacters(
        this string fileNameCandidate,
        string replaceWith
    )
    {
        string regex = String.Format(
            "[{0}]",
            Regex.Escape(new string(Path.GetInvalidFileNameChars()))
        );
        Regex removeInvalidChars = new Regex(
            regex,
            RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant
        );
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

    public static string GetAssemblyVersion(this string tag)
    {
        var currentVersion = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyFileVersionAttribute>()
            .Version;
        Console.WriteLine(string.Format("Current version is {0}", currentVersion));
        if (!currentVersion.StartsWith("1.0.0.0") && !currentVersion.StartsWith("0.0.0.0"))
        {
            tag = currentVersion;
        }
        return tag;
    }

    public static string Truncate(this string input, int maxLength)
    {
        if (input is null)
        {
            return null;
        }
        if (maxLength < 0)
        {
            return input.Truncate(0);
        }
        if (maxLength == 0)
        {
            return string.Empty;
        }
        if (maxLength > input.Length)
        {
            return input;
        }
        return input.Substring(0, maxLength);
    }
}
