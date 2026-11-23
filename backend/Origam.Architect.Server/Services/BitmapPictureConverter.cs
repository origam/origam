#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using System.Buffers.Binary;
using System.IO.Compression;

namespace Origam.Architect.Server.Services;

public static class BitmapPictureConverter
{
    private const int PictureSize = 16;
    private const int TransparentColor = 0xFF00FF;
    private const int ScanlineLength = 1 + (PictureSize * 4);

    public static byte[] ToPng(byte[] bitmap)
    {
        if (bitmap.Length < 54 || bitmap[0] != 'B' || bitmap[1] != 'M')
        {
            return null;
        }
        int pixelOffset = BitConverter.ToInt32(bitmap, startIndex: 10);
        int paletteOffset = 14 + BitConverter.ToInt32(bitmap, startIndex: 14);
        int width = BitConverter.ToInt32(bitmap, startIndex: 18);
        int signedHeight = BitConverter.ToInt32(bitmap, startIndex: 22);
        int height = Math.Abs(signedHeight);
        int bitsPerPixel = BitConverter.ToUInt16(bitmap, startIndex: 28);
        int compression = BitConverter.ToInt32(bitmap, startIndex: 30);
        int colorsUsed = BitConverter.ToInt32(bitmap, startIndex: 46);
        int paletteCount =
            bitsPerPixel > 8 ? 0
            : colorsUsed > 0 ? colorsUsed
            : 1 << bitsPerPixel;
        long rowSize = (((long)width * bitsPerPixel) + 31) / 32 * 4;
        if (
            compression != 0
            || bitsPerPixel is not (1 or 4 or 8 or 24 or 32)
            || width <= 0
            || height <= 0
            || pixelOffset + (rowSize * height) > bitmap.Length
            || paletteOffset + (4L * paletteCount) > pixelOffset
        )
        {
            return null;
        }
        byte[] scanlines = new byte[PictureSize * ScanlineLength];
        for (int targetRow = 0; targetRow < PictureSize; targetRow++)
        {
            long sourceRow = (long)targetRow * height / PictureSize;
            long rowStart =
                pixelOffset + ((signedHeight > 0 ? height - 1 - sourceRow : sourceRow) * rowSize);
            for (int targetColumn = 0; targetColumn < PictureSize; targetColumn++)
            {
                long sourceColumn = (long)targetColumn * width / PictureSize;
                long colorOffset;
                if (bitsPerPixel >= 24)
                {
                    colorOffset = rowStart + (sourceColumn * (bitsPerPixel / 8));
                }
                else
                {
                    long bitOffset = sourceColumn * bitsPerPixel;
                    int paletteIndex =
                        (
                            bitmap[rowStart + (bitOffset / 8)]
                            >> (int)(8 - bitsPerPixel - (bitOffset % 8))
                        ) & ((1 << bitsPerPixel) - 1);
                    if (paletteIndex >= paletteCount)
                    {
                        return null;
                    }
                    colorOffset = paletteOffset + (paletteIndex * 4L);
                }
                byte blue = bitmap[colorOffset];
                byte green = bitmap[colorOffset + 1];
                byte red = bitmap[colorOffset + 2];
                int target = (targetRow * ScanlineLength) + 1 + (targetColumn * 4);
                scanlines[target] = red;
                scanlines[target + 1] = green;
                scanlines[target + 2] = blue;
                scanlines[target + 3] =
                    ((red << 16) | (green << 8) | blue) == TransparentColor ? (byte)0 : (byte)255;
            }
        }
        return EncodePng(scanlines);
    }

    private static byte[] EncodePng(byte[] scanlines)
    {
        byte[] header = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, PictureSize);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), PictureSize);
        header[8] = 8;
        header[9] = 6;
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            zlib.Write(scanlines);
        }
        using var png = new MemoryStream();
        png.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        WriteChunk(png, "IHDR"u8, header);
        WriteChunk(png, "IDAT"u8, compressed.ToArray());
        WriteChunk(png, "IEND"u8, []);
        return png.ToArray();
    }

    private static void WriteChunk(Stream png, ReadOnlySpan<byte> type, byte[] data)
    {
        byte[] typeAndData = [.. type, .. data];
        byte[] number = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(number, data.Length);
        png.Write(number);
        png.Write(typeAndData);
        BinaryPrimitives.WriteUInt32BigEndian(number, Crc32(typeAndData));
        png.Write(number);
    }

    private static uint Crc32(byte[] bytes)
    {
        uint crc = 0xFFFFFFFF;
        foreach (byte value in bytes)
        {
            crc ^= value;
            for (int bit = 0; bit < 8; bit++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
            }
        }
        return ~crc;
    }
}
