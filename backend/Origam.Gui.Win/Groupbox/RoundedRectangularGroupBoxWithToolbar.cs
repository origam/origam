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
        get { return miToolBarWidth; }
        set
        {
            miToolBarWidth = value;
            this.Invalidate();
        }
    }

    // Overriding the base class's Mustoverride ColorScheme Property
    public override EnmColorScheme ColorScheme
    {
        get { return meColorScheme; }
        set
        {
            // Create object of ColorScheme Class
            ColorScheme oColorScheme = new ColorScheme(aoColorScheme: value);
            // Set the controls Diffrent color properties depending on the
            // Color Scheme selected
            oColorScheme.SetColorScheme(aCtrl: this);
            meColorScheme = value;
            this.Invalidate();
        }
    }
    #endregion
    public RoundedRectangularGroupBoxWithToolbar()
        : base() { }

    #region Private Methods
    // This Function is to get the Graphic path to draw the non rectangular interior
    private GraphicsPath GetInteriorRoundedRectanglarPath(
        Rectangle aoRectangle,
        int iBarWidth,
        Size sz
    )
    {
        GraphicsPath oInteriorPath = new GraphicsPath();

        // Add top horizontal line till the downward curve to graphics path
        oInteriorPath.AddLine(
            x1: aoRectangle.Left,
            y1: aoRectangle.Top,
            x2: aoRectangle.Right - iBarWidth - (Single)(sz.Width / 2),
            y2: aoRectangle.Top
        );

        // Add arc to graphics path get the downward curve
        oInteriorPath.AddArc(
            x: aoRectangle.Right - iBarWidth - (Single)(sz.Width / 2),
            y: aoRectangle.Top - (Single)(sz.Height / 2),
            width: sz.Width,
            height: sz.Height,
            startAngle: 180,
            sweepAngle: -90
        );

        // Add Horizontal line from the curve to the right edge
        oInteriorPath.AddLine(
            x1: aoRectangle.Right - iBarWidth,
            y1: aoRectangle.Top + (Single)(sz.Height / 2),
            x2: aoRectangle.Right,
            y2: aoRectangle.Top + (Single)(sz.Height / 2)
        );

        // Add right vertical line to the graphics path
        oInteriorPath.AddLine(
            x1: aoRectangle.Right,
            y1: aoRectangle.Top + (Single)(sz.Height / 2),
            x2: aoRectangle.Right,
            y2: aoRectangle.Bottom
        );

        // Add bottom horizontal line to the graphics path
        oInteriorPath.AddLine(
            x1: aoRectangle.Right,
            y1: aoRectangle.Bottom,
            x2: aoRectangle.Left,
            y2: aoRectangle.Bottom
        );

        // Add left vertical line to the graphics path
        oInteriorPath.AddLine(
            x1: aoRectangle.Left,
            y1: aoRectangle.Bottom,
            x2: aoRectangle.Left,
            y2: aoRectangle.Top
        );

        return oInteriorPath;
    }
    #endregion
    #region Overridden Methods
    // this method is called in the Onpaint method of the base class
    protected override void DrawInterior(System.Drawing.Graphics aoGraphics)
    {
        // Create rectangle to draw interior
        Rectangle oRcInterior = new Rectangle(
            x: this.BorderRectangle.X + this.BorderWidth + 1,
            y: this.BorderRectangle.Y + this.BorderWidth + 12,
            width: this.BorderRectangle.Width - (this.BorderWidth * 2),
            height: this.BorderRectangle.Height - (12 + (this.BorderWidth * 2))
        );

        int iWdth = miToolBarWidth;
        SolidBrush oSolidBrush;

        for (int i = 1; i >= 0; i--)
        {
            // Define Shadow Brushes Dark to Light
            oSolidBrush = new SolidBrush(
                color: Color.FromArgb(alpha: 127 * (2 - i), baseColor: this.ShadowColor)
            );
            Pen oPen = new Pen(brush: oSolidBrush);

            // Draws vertical shadow lines on the left
            aoGraphics.DrawLine(
                pen: oPen,
                x1: oRcInterior.X,
                y1: oRcInterior.Y,
                x2: oRcInterior.X,
                y2: oRcInterior.Bottom
            );

            // Draws horizontal shadow line till the Toolbar
            aoGraphics.DrawLine(
                pen: oPen,
                x1: oRcInterior.X,
                y1: oRcInterior.Y,
                x2: oRcInterior.Right - iWdth - (Single)(mosizeBorderPixelIndent.Width / 2),
                y2: oRcInterior.Y
            );

            // Draws Shadow for the downward arc
            aoGraphics.DrawArc(
                pen: oPen,
                x: oRcInterior.Right - iWdth - (Single)(mosizeBorderPixelIndent.Width / 2),
                y: oRcInterior.Top - (Single)(mosizeBorderPixelIndent.Height / 2),
                width: mosizeBorderPixelIndent.Width,
                height: mosizeBorderPixelIndent.Height,
                startAngle: 180,
                sweepAngle: -90
            );

            // Draws the horizontal shadow line after the curve
            aoGraphics.DrawLine(
                pen: oPen,
                x1: oRcInterior.Right - iWdth - 1,
                y1: oRcInterior.Y + (Single)(mosizeBorderPixelIndent.Height / 2),
                x2: oRcInterior.Right,
                y2: oRcInterior.Y + (Single)(mosizeBorderPixelIndent.Height / 2)
            );

            // Increasing the X and Y postion of the rectangle
            oRcInterior.X += 1;
            oRcInterior.Y += 1;

            // Reducing the height and width of the rectangle
            oRcInterior.Width -= 2;
            oRcInterior.Height -= 2;
        }

        // Brush of LinearGradient type is created to draw gradient
        IGradientContainer oConatiner = this;
        LinearGradientBrush oGradientBrush = new LinearGradientBrush(
            rect: oRcInterior,
            color1: oConatiner.BackgroundTopColor,
            color2: oConatiner.BackgroundBottomColor,
            linearGradientMode: LinearGradientMode.Vertical
        );

        // Blend is used to define the blend of the gradient
        Blend oBlend = new Blend();
        oBlend.Factors = this.IARR_RELATIVEINTENSITIES;
        oBlend.Positions = this.IARR_RELATIVEPOSITIONS;
        oGradientBrush.Blend = oBlend;

        // Fill the rectangle using Gradient Brush created above
        aoGraphics.FillPath(
            brush: oGradientBrush,
            path: GetInteriorRoundedRectanglarPath(
                aoRectangle: oRcInterior,
                iBarWidth: miToolBarWidth,
                sz: mosizeBorderPixelIndent
            )
        );
    }
    #endregion
}
