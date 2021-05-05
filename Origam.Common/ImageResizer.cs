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
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

namespace Origam
{
    public static class ImageResizer
    {
        private static byte[] FixedSizeBytes(Image img, int width, int height)
        {
            using (Image thumbnail = FixedSize(img, width, height))
            {
                MemoryStream ms = new MemoryStream();

                try
                {
                    thumbnail.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return ms.GetBuffer();
                }
                finally
                {
                    if (ms != null) ms.Close();
                }
            }
        }

        public static Image Resize(Image image, int width)
        {
            if (image.Width == width) return image;
            return Resize(image, width, width, true);
        }

        private static byte[] ResizeBytes(Image img, int width, int height
            , bool keepAspectRatio
            , System.Drawing.Imaging.ImageFormat outFormat)
        {
            using (Image resized = Resize(img, width, height, keepAspectRatio))
            {
                MemoryStream ms = new MemoryStream();
                try
                {
                    resized.Save(ms, outFormat);
                    return ms.GetBuffer();
                }
                finally
                {
                    if (ms != null) ms.Close();
                }
            }
        }

        public static byte[] ResizeBytesInBytesOut(byte[] imgBytes, int width, int height,
            bool keepAspectRatio, string outFormat)
        {
            System.Drawing.Imaging.ImageFormat format;
            switch (outFormat.ToLower())
            {
                case "jpeg":
                case "jpg":
                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                    break;
                case "png":
                    format = System.Drawing.Imaging.ImageFormat.Png;
                    break;
                case "gif":
                    format = System.Drawing.Imaging.ImageFormat.Gif;
                    break;
                default:
                    throw new OrigamException(String.Format(
                        "Invalid program exception. Unexpected "
                        + "output image format: {0}",
                        outFormat));
            }

            MemoryStream ms = new MemoryStream(imgBytes);
            Image img = Image.FromStream(ms);
            return ResizeBytes(img, width, height,
                keepAspectRatio, format);
        }

        public static byte[] FixedSizeBytesInBytesOut(byte[] imgBytes, int width, int height)
        {
            MemoryStream ms = new MemoryStream(imgBytes);
            Image img = Image.FromStream(ms);
            return FixedSizeBytes(img, width, height);
        }

        public static int[] GetImageDimensions(byte[] imgBytes)
        {
            MemoryStream ms = new MemoryStream(imgBytes);
            Image img = Image.FromStream(ms);
            return new int[2] { img.Width, img.Height };
        }

        public static Image FixedSize(Image imgPhoto, int Width, int Height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width -
                    (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height -
                    (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height,
                PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                imgPhoto.VerticalResolution);
            bmPhoto.MakeTransparent(Color.Transparent);

            System.Drawing.Graphics grPhoto = System.Drawing.Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Transparent);
            grPhoto.InterpolationMode =
                InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        private static Image Resize(Image imgPhoto, int Width, int Height, bool KeepAspectRatio)
        {
            // fix iphone rotation
            if (imgPhoto.PropertyIdList.Contains(0x0112))
            {
                int rotationValue = imgPhoto.GetPropertyItem(0x0112).Value[0];
                switch (rotationValue)
                {
                    case 1: // landscape, do nothing
                        break;

                    case 8: // rotated 90 right
                            // de-rotate:
                        imgPhoto.RotateFlip(rotateFlipType: RotateFlipType.Rotate270FlipNone);
                        break;

                    case 3: // bottoms up
                        imgPhoto.RotateFlip(rotateFlipType: RotateFlipType.Rotate180FlipNone);
                        break;

                    case 6: // rotated 90 left
                        imgPhoto.RotateFlip(rotateFlipType: RotateFlipType.Rotate90FlipNone);
                        break;
                }
            }

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
            }
            else
            {
                nPercent = nPercentW;
            }
            int destWidth;
            int destHeight;
            if (KeepAspectRatio)
            {
                destWidth = (int)(sourceWidth * nPercent);
                destHeight = (int)(sourceHeight * nPercent);
            }
            else
            {
                destWidth = Width;
                destHeight = Height;
            }

            Bitmap bmPhoto = new Bitmap(destWidth, destHeight,
                PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                imgPhoto.VerticalResolution);
            //bmPhoto.MakeTransparent(Color.Transparent);

            System.Drawing.Graphics grPhoto = System.Drawing.Graphics.FromImage(bmPhoto);
            //grPhoto.Clear(Color.Transparent);
            grPhoto.InterpolationMode =
                InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(0, 0, destWidth, destHeight),
                new Rectangle(0, 0, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }
    }
}
