using System;
using System.Drawing;
using System.IO;

namespace Origam.Extensions
{
    public static class BitmapExtensions
    {
        public static byte[] ToByteArray(this Bitmap bitmap)
        {
            if (bitmap == null) return null;
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}