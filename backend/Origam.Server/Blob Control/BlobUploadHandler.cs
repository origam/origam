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
using ImageMagick;

namespace Origam.Server
{

    public class BlobUploadHandler
    {
        public static byte[] FixedSizeBytes(MagickImage img, int width, int height)
        {
            using MagickImage thumbnail = FixedSize(img, width, height);
            var memoryStream = new MemoryStream();
            try
            {
                thumbnail.Write(memoryStream);
                return memoryStream.GetBuffer();
            }
            finally
            {
                if (memoryStream != null) memoryStream.Close();
            }
        }

        public static MagickImage FixedSize(
            MagickImage imgPhoto, int width, int height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int destX = 0;
            int destY = 0;
            float nPercent;
            float nPercentW = (float)width / (float)sourceWidth;
            float nPercentH = (float)height / (float)sourceHeight;
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((width -
                    (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((height -
                    (sourceHeight * nPercent)) / 2);
            }
            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);
            var blackMarginPhoto = new MagickImage(
                MagickColors.Black, width, height);
            blackMarginPhoto.Format = MagickFormat.Png;
            imgPhoto.Resize(destWidth, destHeight);
            blackMarginPhoto.Composite(imgPhoto, destX, destY);
            return blackMarginPhoto;
        }
    }
}
