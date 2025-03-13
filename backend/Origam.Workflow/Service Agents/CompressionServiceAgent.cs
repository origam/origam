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
                    internalFileName: Parameters.Get<string>("InternalFileName"));
            }
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    "MethodName", MethodName,
                    ResourceUtils.GetString("InvalidMethodName"));
        } 
    }

 private byte[] CompressText(string compressionAlgorithm, string inputText, string internalFileName)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputText);
        using var outputStream = new MemoryStream();
        if (compressionAlgorithm.Equals("zip", StringComparison.OrdinalIgnoreCase))
        {
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
            throw new NotSupportedException("Unsupported compression algorithm: " + compressionAlgorithm);
        }
            
        return outputStream.ToArray();
    }
}