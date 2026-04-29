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
    private static readonly Graphics graphics = Graphics.FromImage(
        image: new Bitmap(width: 1, height: 1)
    );

    public static int Width(this string text, Font font)
    {
        return (int)graphics.MeasureString(text: text, font: font).Width;
    }

    public static int Height(this string text, Font font)
    {
        return (int)graphics.MeasureString(text: text, font: font).Height;
    }

    public static string Wrap(this String text, int widthInPixels, Font font)
    {
        var unwrapedTextWidth = text.Width(font: font);
        if (unwrapedTextWidth > widthInPixels)
        {
            var indexAtButtonEdge = WidthToIndex(
                text: text,
                targetPosition: widthInPixels,
                font: font
            );
            var indexOfNearestSpace = GetIndexOfNearestSpace(
                text: text,
                pivotIndex: indexAtButtonEdge
            );
            var wrappedText = text.Insert(
                startIndex: indexOfNearestSpace,
                value: Environment.NewLine
            );
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
            var position = (int)
                graphics
                    .MeasureString(text: text.Substring(startIndex: 0, length: i), font: font)
                    .Width;
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
        throw new Exception(message: "Could not convert pixels to index in suplied string.");
    }

    private static int GetIndexOfNearestSpace(string text, int pivotIndex)
    {
        var canBeSplit = text.Contains(value: ' ');
        if (!canBeSplit)
        {
            return text.Length;
        }

        var spaceIndices = FindAllSpaceIndices(text: text);
        return spaceIndices
            .OrderBy(keySelector: spaceIndex => Math.Abs(value: pivotIndex - spaceIndex))
            .FirstOrDefault();
    }

    private static List<int> FindAllSpaceIndices(string text)
    {
        var indicesOfSpaces = new List<int>();
        for (int i = 0; i < text.Length; i++)
        {
            if (text[index: i] == ' ')
            {
                indicesOfSpaces.Add(item: i);
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
                throw new ArgumentNullException(paramName: nameof(input));
            }
            case "":
            {
                throw new ArgumentException(
                    message: $"{nameof(input)} cannot be empty",
                    paramName: nameof(input)
                );
            }
            default:
            {
                return input.Substring(startIndex: 0, length: 1).ToUpper()
                    + input.Substring(startIndex: 1).ToLower();
            }
        }
    }

    public static string[] Split(this string str, string splitWith) =>
        str.Split(separator: new[] { splitWith }, options: StringSplitOptions.None);

    public static string ReplaceInvalidFileCharacters(
        this string fileNameCandidate,
        string replaceWith
    )
    {
        string regex = String.Format(
            format: "[{0}]",
            arg0: Regex.Escape(str: new string(value: Path.GetInvalidFileNameChars()))
        );
        Regex removeInvalidChars = new Regex(
            pattern: regex,
            options: RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant
        );
        return removeInvalidChars.Replace(input: fileNameCandidate, replacement: replaceWith);
    }

    public static string GetBase64Hash(this string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(s: str);
        using (MD5 md5 = MD5.Create())
        {
            return Convert.ToBase64String(
                inArray: md5.ComputeHash(inputStream: new MemoryStream(buffer: bytes))
            );
        }
    }

    public static string GetAssemblyVersion(this string tag)
    {
        var currentVersion = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyFileVersionAttribute>()
            .Version;
        Console.WriteLine(
            value: string.Format(format: "Current version is {0}", arg0: currentVersion)
        );
        if (
            !currentVersion.StartsWith(value: "1.0.0.0")
            && !currentVersion.StartsWith(value: "0.0.0.0")
        )
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
            return input.Truncate(maxLength: 0);
        }
        if (maxLength == 0)
        {
            return string.Empty;
        }
        if (maxLength > input.Length)
        {
            return input;
        }
        return input.Substring(startIndex: 0, length: maxLength);
    }
}
