using System.Drawing;

namespace Origam.Workbench.Diagram.Extensions
{
    public static class RectangleExtensions
    {
        public static Point GetCenter(this Rectangle rectangle)
        {
            return new Point(
                rectangle.X + rectangle.Width/2 , 
                rectangle.Y + rectangle.Height/2);
        }
    }
}