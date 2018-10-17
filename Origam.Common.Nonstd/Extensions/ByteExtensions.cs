using System.Drawing;
using System.IO;

namespace Origam.Extensions
{
    public static class ByteExtensions
    {
        public static Bitmap ToBitmap(this byte[] array)
        {           
            using (var ms = new MemoryStream(array))
            {
                return new Bitmap(ms);
            }
        } 
    }
}