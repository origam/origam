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

using System.Drawing;
using System.Drawing.Drawing2D;

namespace Origam.Gui.Win;

public class AsGradientLabel : System.Windows.Forms.Label
{
    // declare two color for linear gradient
    private Color cLeft;
    private Color cRight;

    // property of begin color in linear gradient
    public Color BeginColor
    {
        get { return cLeft; }
        set { cLeft = value; }
    }

    // property of end color in linear gradient
    public Color EndColor
    {
        get { return cRight; }
        set { cRight = value; }
    }

    public AsGradientLabel()
    {
        // Default get system color
        cLeft = SystemColors.ActiveCaption;
        cRight = SystemColors.Control;
    }

    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
    {
        // declare linear gradient brush for fill background of label
        LinearGradientBrush GBrush = new LinearGradientBrush(
            point1: new Point(x: 0, y: 0),
            point2: new Point(x: this.Width, y: 0),
            color1: cLeft,
            color2: cRight
        );
        Rectangle rect = new Rectangle(x: 0, y: 0, width: this.Width, height: this.Height);
        // Fill with gradient
        e.Graphics.FillRectangle(brush: GBrush, rect: rect);
        // draw text on label
        SolidBrush drawBrush = new SolidBrush(color: this.ForeColor);
        StringFormat sf = new StringFormat();
        // align with center
        sf.Alignment = StringAlignment.Near;
        // set rectangle bound text
        RectangleF rectF = new RectangleF(
            x: 0,
            y: (this.Height / 2) - (Font.Height / 2),
            width: this.Width,
            height: this.Height
        );
        // output string
        e.Graphics.DrawString(
            s: this.Text,
            font: this.Font,
            brush: drawBrush,
            layoutRectangle: rectF,
            format: sf
        );
    }
}
