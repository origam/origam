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
		get
		{
				return cLeft;
			}
		set
		{
				cLeft = value;
			}
	}
	// property of end color in linear gradient
	public Color EndColor
	{
		get
		{
				return cRight;
			}
		set
		{
				cRight = value;
			}
	}
	public AsGradientLabel()
	{
			// Default get system color 		cLeft = SystemColors.ActiveCaption;
			cRight = SystemColors.Control;
		}
	protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
	{
			// declare linear gradient brush for fill background of label
			LinearGradientBrush GBrush = new LinearGradientBrush(
				new Point(0, 0),
				new Point(this.Width, 0), cLeft, cRight);
			Rectangle rect = new Rectangle(0,0,this.Width,this.Height);
			// Fill with gradient 		e.Graphics.FillRectangle(GBrush, rect);

			// draw text on label
			SolidBrush drawBrush = new SolidBrush(this.ForeColor);
			StringFormat sf = new StringFormat();
			// align with center
			sf.Alignment = StringAlignment.Near;
			// set rectangle bound text
			RectangleF rectF = new 
				RectangleF(0,this.Height/2-Font.Height/2,this.Width,this.Height);
			// output string
			e.Graphics.DrawString(this.Text, this.Font, drawBrush, rectF, sf);
		}
}