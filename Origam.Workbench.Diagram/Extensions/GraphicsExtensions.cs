using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Origam.Workbench.Diagram.Extensions
{
    public static class GraphicsExtensions
    {
        public static void DrawUpSideDown(this Graphics graphics, Action<Graphics> drawAction, float yAxisCoordinate)
        {
            using (Matrix m = graphics.Transform)
            {
                using (Matrix saveM = m.Clone())
                {
                    using (var m2 = new Matrix(1, 0, 0, -1, 0, 2 * yAxisCoordinate))
                        m.Multiply(m2);

                    graphics.Transform = m;
                    drawAction(graphics);
                    graphics.Transform = saveM;
                    graphics.ResetClip();
                }
            }
        } 
    }
}