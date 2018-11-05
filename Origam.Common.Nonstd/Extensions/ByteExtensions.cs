using System.Drawing;
using System.IO;

namespace Origam.Extensions
{
    public static class ByteExtensions
    {
        public static Bitmap ToBitMap(this byte[] array)
        {
            if (array == null) return null;
            using (var ms = new MemoryStream(array))
            {
                return new Bitmap(ms);
            }
        } 
    }
}