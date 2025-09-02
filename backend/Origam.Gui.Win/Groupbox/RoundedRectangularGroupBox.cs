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

#region license
// Copyright 2004 Shouvik - https://www.codeproject.com/Articles/8103/Creating-some-cool-buttons-and-groupboxes
#endregion

using System.Drawing;
using System.Drawing.Drawing2D;

namespace Origam.Gui.Win;
/// <summary>
/// Summary description for RoundedRectangularGroupBox.
/// </summary>
public class RoundedRectangularGroupBox : BaseContainer 
{
	#region Private data members
	// The enum object to store the colorscheme value
	private EnmColorScheme meColorScheme = EnmColorScheme.Green;
	#endregion
	#region Public Properties
	// Overriding the base class's Mustoverride ColorScheme Property
	public override EnmColorScheme ColorScheme 
	{
		get
		{
			return meColorScheme;
		}
		set
		{
			ColorScheme oColorScheme = new ColorScheme(value);
			oColorScheme.SetColorScheme(this);
			meColorScheme = value;
		}
	}
	#endregion
	public RoundedRectangularGroupBox():base()
	{}
	#region Overridden Methods
	// This method is called in the base class' OnPaint method
	protected override void DrawBorder(System.Drawing.Graphics aoGraphics, System.Drawing.Rectangle aoRectangle)
	{
		Rectangle oRcInterior;
		// Check if text property is Empty
		if(this.Text.Trim() != "")
		{
			// Creating rectangle to draw interior with more top width than other side of border
			oRcInterior = new Rectangle(this.BorderRectangle.X + this.BorderWidth + 1, 
				this.BorderRectangle.Y + 12 + this.BorderWidth, 
				this.BorderRectangle.Width - (this.BorderWidth * 2), 
				this.BorderRectangle.Height - (12 + (this.BorderWidth * 2)));
		}
		else
		{
			// Creating rectangle to draw interior with all sides equall
			oRcInterior = new Rectangle(this.BorderRectangle.X + this.BorderWidth + 1, 
				this.BorderRectangle.Y + this.BorderWidth + 1, 
				this.BorderRectangle.Width - (this.BorderWidth * 2), 
				this.BorderRectangle.Height - (this.BorderWidth * 2));
		}
		SolidBrush oSoildBrush;
		
		// Draw shadows 
		for(int i = 1;i>=0; i--)
		{
			// Define Shadow Brushes Dark to Light
			oSoildBrush = new SolidBrush(Color.FromArgb(127 * (2 - i), this.ShadowColor));
			Pen p = new Pen(oSoildBrush);
			
			// Draws vertical line on Left side
			aoGraphics.DrawLine(p, oRcInterior.X, oRcInterior.Y, oRcInterior.X, oRcInterior.Bottom);
            
			// Draws horizontal lines on the top
			aoGraphics.DrawLine(p, oRcInterior.X, oRcInterior.Y, oRcInterior.Right, oRcInterior.Y);
			
			// Increasing the X and Y postion of the rectangle
			oRcInterior.X += 1;
			oRcInterior.Y += 1;
			
			// Reducing the height and width of the rectangle
			oRcInterior.Width -= 2;
			oRcInterior.Height -= 2;
		}
		
		// Brush of LinearGradient type is created to draw gradient
		IGradientContainer oContainer=this;
		LinearGradientBrush oGradientBrush = 
			new LinearGradientBrush(oRcInterior,oContainer.BackgroundTopColor, 
			oContainer.BackgroundBottomColor, LinearGradientMode.Vertical);
		// Blend is used to define the blend of the gradient
		Blend oBlend = new Blend();
		oBlend.Factors = this.IARR_RELATIVEINTENSITIES;
		oBlend.Positions = this.IARR_RELATIVEPOSITIONS;
		oGradientBrush.Blend = oBlend;
		
		// Fill the rectangle using Gradient Brush Created above
		aoGraphics.FillRectangle(oGradientBrush, oRcInterior);
	}
	#endregion
}
