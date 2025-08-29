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

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Origam.Gui.Win;
/// <summary>
/// Summary description for RoundedRectangularGroupBoxWithToolbar.
/// </summary>
public class RoundedRectangularGroupBoxWithToolbar : BaseContainer
{
	#region Private Data Members
	// This data member store value of width of the toolbar
	private int miToolBarWidth = 110;
	// The enum object to store the colorscheme value
	private EnmColorScheme meColorScheme = EnmColorScheme.Green;
	#endregion
	#region Public Data Members
	
	// This property is used to Get and set the toolbarwidth
	public int ToolbarWidth
	{
		get
		{
			return miToolBarWidth;
		}
		set
		{
			miToolBarWidth = value;
			this.Invalidate();
		}
	}
	
	// Overriding the base class's Mustoverride ColorScheme Property
	public override EnmColorScheme ColorScheme
	{
		get
		{
			return meColorScheme;
		}
		set
		{
			// Create object of ColorScheme Class
			ColorScheme oColorScheme = new ColorScheme(value);
			// Set the controls Diffrent color properties depending on the 
			// Color Scheme selected
			oColorScheme.SetColorScheme(this);
			meColorScheme = value;
			this.Invalidate();
		}
	}
	#endregion
	public RoundedRectangularGroupBoxWithToolbar():base(){}
    
	#region Private Methods
	// This Function is to get the Graphic path to draw the non rectangular interior
	private GraphicsPath GetInteriorRoundedRectanglarPath(Rectangle aoRectangle, int iBarWidth, Size sz)
	{
		GraphicsPath oInteriorPath = new GraphicsPath();
		
		// Add top horizontal line till the downward curve to graphics path
		oInteriorPath.AddLine(aoRectangle.Left, aoRectangle.Top,
			aoRectangle.Right - iBarWidth - (Single)(sz.Width / 2), 
			aoRectangle.Top);
		
		// Add arc to graphics path get the downward curve
		oInteriorPath.AddArc(aoRectangle.Right - iBarWidth - (Single)(sz.Width / 2), 
			aoRectangle.Top - (Single)(sz.Height / 2), 
			sz.Width, sz.Height, 180, -90);
		
		// Add Horizontal line from the curve to the right edge
		oInteriorPath.AddLine(aoRectangle.Right - iBarWidth, 
			aoRectangle.Top + (Single)(sz.Height / 2), 
			aoRectangle.Right, 
			aoRectangle.Top + (Single)(sz.Height / 2));
		
		// Add right vertical line to the graphics path
		oInteriorPath.AddLine(aoRectangle.Right, aoRectangle.Top + (Single)(sz.Height / 2), 
			aoRectangle.Right, aoRectangle.Bottom);
        
		// Add bottom horizontal line to the graphics path
		oInteriorPath.AddLine(aoRectangle.Right, aoRectangle.Bottom, 
			aoRectangle.Left, aoRectangle.Bottom);
		
		// Add left vertical line to the graphics path
		oInteriorPath.AddLine(aoRectangle.Left, aoRectangle.Bottom, 
			aoRectangle.Left, aoRectangle.Top);
		
		return oInteriorPath;
	}
	#endregion
	#region Overridden Methods
	// this method is called in the Onpaint method of the base class
	protected override void DrawInterior(System.Drawing.Graphics aoGraphics)
	{
		// Create rectangle to draw interior
		Rectangle oRcInterior = new Rectangle(this.BorderRectangle.X + this.BorderWidth + 1, 
			this.BorderRectangle.Y + this.BorderWidth + 12, 
			this.BorderRectangle.Width - (this.BorderWidth * 2), 
			this.BorderRectangle.Height - (12 + (this.BorderWidth * 2)));
		
		int iWdth = miToolBarWidth;
		SolidBrush oSolidBrush;
		
		for(int i = 1;i >= 0;i--)
		{
			// Define Shadow Brushes Dark to Light
			oSolidBrush = new SolidBrush(Color.FromArgb(127 * (2 - i), this.ShadowColor));
			Pen oPen = new Pen(oSolidBrush);
            
			// Draws vertical shadow lines on the left
			aoGraphics.DrawLine(oPen, oRcInterior.X, oRcInterior.Y, oRcInterior.X, oRcInterior.Bottom);
			
			// Draws horizontal shadow line till the Toolbar
			aoGraphics.DrawLine(oPen, oRcInterior.X, oRcInterior.Y, 
				oRcInterior.Right - iWdth - (Single)(mosizeBorderPixelIndent.Width / 2), 
				oRcInterior.Y);
			
			// Draws Shadow for the downward arc
			aoGraphics.DrawArc(oPen, oRcInterior.Right - iWdth - (Single)(mosizeBorderPixelIndent.Width / 2), 
				oRcInterior.Top - (Single)(mosizeBorderPixelIndent.Height / 2), 
				mosizeBorderPixelIndent.Width, mosizeBorderPixelIndent.Height, 180, -90);
			
			// Draws the horizontal shadow line after the curve
			aoGraphics.DrawLine(oPen, oRcInterior.Right - iWdth - 1, 
				oRcInterior.Y + (Single)(mosizeBorderPixelIndent.Height / 2), 
				oRcInterior.Right, 
				oRcInterior.Y + (Single)(mosizeBorderPixelIndent.Height / 2));
			
			// Increasing the X and Y postion of the rectangle
			oRcInterior.X += 1;
			oRcInterior.Y += 1;
			
			// Reducing the height and width of the rectangle
			oRcInterior.Width -= 2;
			oRcInterior.Height -= 2;
		}
		
		// Brush of LinearGradient type is created to draw gradient
		IGradientContainer oConatiner = this;
		LinearGradientBrush oGradientBrush = 
			new LinearGradientBrush(oRcInterior, 
			oConatiner.BackgroundTopColor, 
			oConatiner.BackgroundBottomColor, 
			LinearGradientMode.Vertical);
		
		// Blend is used to define the blend of the gradient
		Blend oBlend = new Blend();
		oBlend.Factors = this.IARR_RELATIVEINTENSITIES;
		oBlend.Positions = this.IARR_RELATIVEPOSITIONS;
		oGradientBrush.Blend = oBlend;
        
		// Fill the rectangle using Gradient Brush created above
		aoGraphics.FillPath(oGradientBrush, GetInteriorRoundedRectanglarPath(oRcInterior, miToolBarWidth, mosizeBorderPixelIndent));
	}
	#endregion
	
}
