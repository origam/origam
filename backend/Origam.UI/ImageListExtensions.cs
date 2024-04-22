

using System.Windows.Forms;

namespace Origam.UI;

public static class ImageListExtensions
{
    public  static int ImageIndex(this ImageList imglist , string icon)
    {
            int imageIndex;
            if (!int.TryParse(icon, out imageIndex))
            {
                imageIndex = imglist.Images.IndexOfKey(icon);
            }
            return imageIndex;
        }
}