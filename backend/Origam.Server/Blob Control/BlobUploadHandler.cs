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

using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

public class BlobUploadHandler
{
    public static byte[] FixedSizeBytes(Image image, int width, int height)
    {
        using Image thumbnail = FixedSize(sourceImage: image, width: width, height: height);
        using var memoryStream = new MemoryStream();
        thumbnail.SaveAsPng(stream: memoryStream, encoder: new PngEncoder());
        return memoryStream.ToArray();
    }

    private static Image FixedSize(Image sourceImage, int width, int height)
    {
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
            destX = System.Convert.ToInt16(value: (width - (sourceWidth * nPercent)) / 2);
        }
        else
        {
            nPercent = nPercentW;
            destY = System.Convert.ToInt16(value: (height - (sourceHeight * nPercent)) / 2);
        }
        int destWidth = (int)(sourceWidth * nPercent);
        int destHeight = (int)(sourceHeight * nPercent);
        Image backgroundImage = new Image<Rgba32>(width: width, height: height);
        backgroundImage.Mutate(operation: x => x.Fill(color: Color.Black));
        using Image resizedImage = sourceImage.Clone(operation: ctx =>
            ctx.Resize(width: destWidth, height: destHeight)
        );
        backgroundImage.Mutate(operation: x =>
            x.DrawImage(
                foreground: resizedImage,
                backgroundLocation: new Point(x: destX, y: destY),
                opacity: 1f
            )
        );
        return backgroundImage;
    }
}
