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
using SkiaSharp;

namespace Origam;

public static class ImageResizer
{
    public static byte[] ResizeBytesInBytesOut(
        byte[] imgBytes,
        int width,
        int height,
        bool keepAspectRatio,
        string outFormat
    )
    {
        SKEncodedImageFormat format = outFormat.ToLowerInvariant() switch
        {
            "jpeg" or "jpg" => SKEncodedImageFormat.Jpeg,
            "png" => SKEncodedImageFormat.Png,
            _ => throw new OrigamException(
                string.Format(Strings.UnsupportedOutputImageFormat, outFormat)
            ),
        };
        using var stream = new SKMemoryStream(imgBytes);
        using var codec = SKCodec.Create(stream);
        if (codec == null)
        {
            throw new ArgumentException(message: null, nameof(imgBytes));
        }
        using SKBitmap decoded =
            SKBitmap.Decode(codec) ?? throw new ArgumentException(message: null, nameof(imgBytes));
        using SKBitmap oriented = Orient(decoded, codec.EncodedOrigin);
        using SKBitmap resized = Resize(oriented, width, height, keepAspectRatio);
        return Encode(resized, format);
    }

    public static byte[] FixedSizeBytesInBytesOut(byte[] imgBytes, int width, int height)
    {
        using SKBitmap image = Decode(imgBytes);
        using SKBitmap thumbnail = FixedSize(image, width, height);
        return Encode(thumbnail, SKEncodedImageFormat.Png);
    }

    public static int[] GetImageDimensions(byte[] imgBytes)
    {
        using SKBitmap image = Decode(imgBytes);
        return new[] { image.Width, image.Height };
    }

    private static SKBitmap Decode(byte[] imgBytes)
    {
        using var stream = new SKMemoryStream(imgBytes);
        using var codec = SKCodec.Create(stream);
        if (codec == null)
        {
            throw new ArgumentException(message: null, nameof(imgBytes));
        }
        return SKBitmap.Decode(codec)
            ?? throw new ArgumentException(message: null, nameof(imgBytes));
    }

    private static byte[] Encode(SKBitmap bitmap, SKEncodedImageFormat format)
    {
        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(format, quality: 75);
        return data.ToArray();
    }

    public static SKBitmap FixedSize(SKBitmap image, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        float scale = Math.Min((float)width / image.Width, (float)height / image.Height);
        int destWidth = Math.Max(1, (int)(image.Width * scale));
        int destHeight = Math.Max(1, (int)(image.Height * scale));
        int destX = Convert.ToInt32((width - (image.Width * scale)) / 2);
        int destY = Convert.ToInt32((height - (image.Height * scale)) / 2);
        return Draw(
            image,
            width,
            height,
            new SKRect(destX, destY, destX + destWidth, destY + destHeight),
            SKColors.Transparent
        );
    }

    private static SKBitmap Resize(SKBitmap image, int width, int height, bool keepAspectRatio)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        if (keepAspectRatio)
        {
            float scale = Math.Min((float)width / image.Width, (float)height / image.Height);
            width = Math.Max(1, (int)(image.Width * scale));
            height = Math.Max(1, (int)(image.Height * scale));
        }
        return Draw(image, width, height, new SKRect(0, 0, width, height), SKColors.Black);
    }

    private static SKBitmap Draw(
        SKBitmap source,
        int width,
        int height,
        SKRect destination,
        SKColor background
    )
    {
        var result = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(result);
        canvas.Clear(background);
        using SKImage image = SKImage.FromBitmap(source);
        canvas.DrawImage(image, destination, new SKSamplingOptions(SKCubicResampler.Mitchell));
        return result;
    }

    private static SKBitmap Orient(SKBitmap source, SKEncodedOrigin origin)
    {
        bool swapDimensions = origin >= SKEncodedOrigin.LeftTop;
        int width = swapDimensions ? source.Height : source.Width;
        int height = swapDimensions ? source.Width : source.Height;
        var result = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(result);
        switch (origin)
        {
            case SKEncodedOrigin.TopRight:
            {
                canvas.Translate(width, 0);
                canvas.Scale(-1, 1);
                break;
            }
            case SKEncodedOrigin.BottomRight:
            {
                canvas.Translate(width, height);
                canvas.RotateDegrees(180);
                break;
            }
            case SKEncodedOrigin.BottomLeft:
            {
                canvas.Translate(0, height);
                canvas.Scale(1, -1);
                break;
            }
            case SKEncodedOrigin.LeftTop:
            {
                canvas.RotateDegrees(90);
                canvas.Scale(1, -1);
                break;
            }
            case SKEncodedOrigin.RightTop:
            {
                canvas.Translate(width, 0);
                canvas.RotateDegrees(90);
                break;
            }
            case SKEncodedOrigin.RightBottom:
            {
                canvas.Translate(width, height);
                canvas.RotateDegrees(90);
                canvas.Scale(-1, 1);
                break;
            }
            case SKEncodedOrigin.LeftBottom:
            {
                canvas.Translate(0, height);
                canvas.RotateDegrees(270);
                break;
            }
        }
        using SKImage image = SKImage.FromBitmap(source);
        canvas.DrawImage(image, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest));
        return result;
    }
}
