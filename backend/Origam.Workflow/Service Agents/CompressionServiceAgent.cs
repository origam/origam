#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Zip;
using Origam.Service.Core;

namespace Origam.Workflow;

public class CompressionServiceAgent : AbstractServiceAgent
{
    private object result;
    public override object Result => result;
    public override void Run()
    {
        switch(MethodName)
        {
            case "CompressText":
            {
                result = CompressText(
                    compressionAlgorithm: Parameters.Get<string>("CompressionAlgorithm"),
                    inputText: Parameters.Get<string>("InputText"),
                    internalFileName: Parameters.TryGet<string>("InternalFileName"));
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(MethodName), MethodName,
                    ResourceUtils.GetString("InvalidMethodName"));
        } 
    }

    private byte[] CompressText(string compressionAlgorithm, string inputText, 
        string internalFileName)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputText);
        using var outputStream = new MemoryStream();
        if (compressionAlgorithm.Equals("zip", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(internalFileName))
            {
                throw new ArgumentNullException("InternalFileName");
            }

            using var zipStream = new ZipOutputStream(outputStream);
            zipStream.SetLevel(9); // Maximum compression
            var entry = new ZipEntry(internalFileName) { DateTime = DateTime.Now };
            zipStream.PutNextEntry(entry);
            zipStream.Write(inputBytes, 0, inputBytes.Length);
            zipStream.CloseEntry();
        }
        else if (compressionAlgorithm.Equals("bzip2", StringComparison.OrdinalIgnoreCase))
        {
            using var bzip2Stream = new BZip2OutputStream(outputStream);
            bzip2Stream.Write(inputBytes, 0, inputBytes.Length);
        }
        else
        {
            throw new NotSupportedException(
                "Unsupported compression algorithm: " + compressionAlgorithm);
        }
        return outputStream.ToArray();
    }
}