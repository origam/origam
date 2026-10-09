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

public class BlobUploadHandler
{
    public static byte[] FixedSizeBytes(SKBitmap image, int width, int height)
    {
        using SKBitmap thumbnail = FixedSize(image, width, height, transparentPadding: false);
        return Encode(thumbnail, SKEncodedImageFormat.Png);
    }

    public static byte[] ResizeImage(byte[] bytes, int width, int height)
    {
        using var stream = new SKMemoryStream(bytes);
        using var codec = SKCodec.Create(stream);
        if (codec == null)
        {
            throw new ArgumentException(message: null, nameof(bytes));
        }
        using SKBitmap image = SKBitmap.Decode(codec);
        using SKBitmap thumbnail = FixedSize(image, width, height, transparentPadding: true);
        return Encode(thumbnail, codec.EncodedFormat);
    }

    private static byte[] Encode(SKBitmap bitmap, SKEncodedImageFormat format)
    {
        using SKImage image = SKImage.FromBitmap(bitmap);
        // Skia can decode formats such as GIF and BMP but cannot encode them.
        using SKData data =
            image.Encode(format, quality: 75)
            ?? image.Encode(SKEncodedImageFormat.Png, quality: 100);
        return data.ToArray();
    }

    private static SKBitmap FixedSize(
        SKBitmap sourceImage,
        int width,
        int height,
        bool transparentPadding
    )
    {
        ArgumentNullException.ThrowIfNull(sourceImage);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        int sourceWidth = sourceImage.Width;
        int sourceHeight = sourceImage.Height;
        int destX = 0;
        int destY = 0;
        float nPercent;
        float nPercentW = (float)width / (float)sourceWidth;
        float nPercentH = (float)height / (float)sourceHeight;
        if (nPercentH < nPercentW)
        {
            nPercent = nPercentH;
            destX = Convert.ToInt32((width - (sourceWidth * nPercent)) / 2);
        }
        else
        {
            nPercent = nPercentW;
            destY = Convert.ToInt32((height - (sourceHeight * nPercent)) / 2);
        }
        int destWidth = Math.Max(val1: 1, (int)(sourceWidth * nPercent));
        int destHeight = Math.Max(val1: 1, (int)(sourceHeight * nPercent));
        var backgroundImage = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(backgroundImage);
        canvas.Clear(transparentPadding ? SKColors.Transparent : SKColors.Black);
        var destination = new SKRect(destX, destY, destX + destWidth, destY + destHeight);
        using var paint = new SKPaint { Color = SKColors.Black };
        canvas.DrawRect(destination, paint);
        using SKImage image = SKImage.FromBitmap(sourceImage);
        var sampling =
            destWidth == sourceWidth && destHeight == sourceHeight
                ? new SKSamplingOptions(SKFilterMode.Nearest)
                : new SKSamplingOptions(SKCubicResampler.Mitchell);
        canvas.DrawImage(image, destination, sampling);
        return backgroundImage;
    }
}
