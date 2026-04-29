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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Origam;

public static class ImageResizer
{
    private static byte[] FixedSizeBytes(Image img, int width, int height)
    {
        using (Image thumbnail = FixedSize(imgPhoto: img, Width: width, Height: height))
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                thumbnail.Save(stream: ms, format: System.Drawing.Imaging.ImageFormat.Png);
                return ms.GetBuffer();
            }
            finally
            {
                if (ms != null)
                {
                    ms.Close();
                }
            }
        }
    }

    public static Image Resize(Image image, int width)
    {
        if (image.Width == width)
        {
            return image;
        }

        return Resize(imgPhoto: image, Width: width, Height: width, KeepAspectRatio: true);
    }

    private static byte[] ResizeBytes(
        Image img,
        int width,
        int height,
        bool keepAspectRatio,
        System.Drawing.Imaging.ImageFormat outFormat
    )
    {
        using (
            Image resized = Resize(
                imgPhoto: img,
                Width: width,
                Height: height,
                KeepAspectRatio: keepAspectRatio
            )
        )
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                resized.Save(stream: ms, format: outFormat);
                return ms.GetBuffer();
            }
            finally
            {
                if (ms != null)
                {
                    ms.Close();
                }
            }
        }
    }

    public static byte[] ResizeBytesInBytesOut(
        byte[] imgBytes,
        int width,
        int height,
        bool keepAspectRatio,
        string outFormat
    )
    {
        System.Drawing.Imaging.ImageFormat format;
        switch (outFormat.ToLower())
        {
            case "jpeg":
            case "jpg":
            {
                format = System.Drawing.Imaging.ImageFormat.Jpeg;
                break;
            }

            case "png":
            {
                format = System.Drawing.Imaging.ImageFormat.Png;
                break;
            }

            case "gif":
            {
                format = System.Drawing.Imaging.ImageFormat.Gif;
                break;
            }

            default:
            {
                throw new OrigamException(
                    message: String.Format(
                        format: "Invalid program exception. Unexpected "
                            + "output image format: {0}",
                        arg0: outFormat
                    )
                );
            }
        }
        MemoryStream ms = new MemoryStream(buffer: imgBytes);
        Image img = Image.FromStream(stream: ms);
        return ResizeBytes(
            img: img,
            width: width,
            height: height,
            keepAspectRatio: keepAspectRatio,
            outFormat: format
        );
    }

    public static byte[] FixedSizeBytesInBytesOut(byte[] imgBytes, int width, int height)
    {
        MemoryStream ms = new MemoryStream(buffer: imgBytes);
        Image img = Image.FromStream(stream: ms);
        return FixedSizeBytes(img: img, width: width, height: height);
    }

    public static int[] GetImageDimensions(byte[] imgBytes)
    {
        MemoryStream ms = new MemoryStream(buffer: imgBytes);
        Image img = Image.FromStream(stream: ms);
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
            destX = System.Convert.ToInt16(value: (Width - (sourceWidth * nPercent)) / 2);
        }
        else
        {
            nPercent = nPercentW;
            destY = System.Convert.ToInt16(value: (Height - (sourceHeight * nPercent)) / 2);
        }
        int destWidth = (int)(sourceWidth * nPercent);
        int destHeight = (int)(sourceHeight * nPercent);
        Bitmap bmPhoto = new Bitmap(
            width: Width,
            height: Height,
            format: PixelFormat.Format24bppRgb
        );
        bmPhoto.SetResolution(
            xDpi: imgPhoto.HorizontalResolution,
            yDpi: imgPhoto.VerticalResolution
        );
        bmPhoto.MakeTransparent(transparentColor: Color.Transparent);
        System.Drawing.Graphics grPhoto = System.Drawing.Graphics.FromImage(image: bmPhoto);
        grPhoto.Clear(color: Color.Transparent);
        grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
        grPhoto.DrawImage(
            image: imgPhoto,
            destRect: new Rectangle(x: destX, y: destY, width: destWidth, height: destHeight),
            srcRect: new Rectangle(
                x: sourceX,
                y: sourceY,
                width: sourceWidth,
                height: sourceHeight
            ),
            srcUnit: GraphicsUnit.Pixel
        );
        grPhoto.Dispose();
        return bmPhoto;
    }

    private static Image Resize(Image imgPhoto, int Width, int Height, bool KeepAspectRatio)
    {
        // fix iphone rotation
        if (imgPhoto.PropertyIdList.Contains(value: 0x0112))
        {
            int rotationValue = imgPhoto.GetPropertyItem(propid: 0x0112).Value[0];
            switch (rotationValue)
            {
                case 1: // landscape, do nothing
                {
                    break;
                }
                case 8: // rotated 90 right
                {
                    // de-rotate:
                    imgPhoto.RotateFlip(rotateFlipType: RotateFlipType.Rotate270FlipNone);
                    break;
                }

                case 3: // bottoms up
                {
                    imgPhoto.RotateFlip(rotateFlipType: RotateFlipType.Rotate180FlipNone);
                    break;
                }

                case 6: // rotated 90 left
                {
                    imgPhoto.RotateFlip(rotateFlipType: RotateFlipType.Rotate90FlipNone);
                    break;
                }
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
        Bitmap bmPhoto = new Bitmap(
            width: destWidth,
            height: destHeight,
            format: PixelFormat.Format24bppRgb
        );
        bmPhoto.SetResolution(
            xDpi: imgPhoto.HorizontalResolution,
            yDpi: imgPhoto.VerticalResolution
        );
        //bmPhoto.MakeTransparent(Color.Transparent);
        System.Drawing.Graphics grPhoto = System.Drawing.Graphics.FromImage(image: bmPhoto);
        //grPhoto.Clear(Color.Transparent);
        grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
        grPhoto.DrawImage(
            image: imgPhoto,
            destRect: new Rectangle(x: 0, y: 0, width: destWidth, height: destHeight),
            srcRect: new Rectangle(x: 0, y: 0, width: sourceWidth, height: sourceHeight),
            srcUnit: GraphicsUnit.Pixel
        );
        grPhoto.Dispose();
        return bmPhoto;
    }
}
