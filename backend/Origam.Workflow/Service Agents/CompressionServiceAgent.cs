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
        switch (MethodName)
        {
            case "CompressText":
            {
                result = CompressText(
                    compressionAlgorithm: Parameters.Get<string>(key: "CompressionAlgorithm"),
                    inputText: Parameters.Get<string>(key: "InputText"),
                    internalFileName: Parameters.TryGet<string>(key: "InternalFileName")
                );
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(MethodName),
                    actualValue: MethodName,
                    message: ResourceUtils.GetString(key: "InvalidMethodName")
                );
            }
        }
    }

    private byte[] CompressText(
        string compressionAlgorithm,
        string inputText,
        string internalFileName
    )
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(s: inputText);
        using var outputStream = new MemoryStream();
        if (
            compressionAlgorithm.Equals(
                value: "zip",
                comparisonType: StringComparison.OrdinalIgnoreCase
            )
        )
        {
            if (string.IsNullOrWhiteSpace(value: internalFileName))
            {
                throw new ArgumentNullException(paramName: "InternalFileName");
            }

            using var zipStream = new ZipOutputStream(baseOutputStream: outputStream);
            zipStream.SetLevel(level: 9); // Maximum compression
            var entry = new ZipEntry(name: internalFileName) { DateTime = DateTime.Now };
            zipStream.PutNextEntry(entry: entry);
            zipStream.Write(buffer: inputBytes, offset: 0, count: inputBytes.Length);
            zipStream.CloseEntry();
        }
        else if (
            compressionAlgorithm.Equals(
                value: "bzip2",
                comparisonType: StringComparison.OrdinalIgnoreCase
            )
        )
        {
            using var bzip2Stream = new BZip2OutputStream(stream: outputStream);
            bzip2Stream.Write(buffer: inputBytes, offset: 0, count: inputBytes.Length);
        }
        else
        {
            throw new NotSupportedException(
                message: "Unsupported compression algorithm: " + compressionAlgorithm
            );
        }
        return outputStream.ToArray();
    }
}
