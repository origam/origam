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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for GroupBoxWithChamfer.
/// </summary>
[ToolboxBitmap(t: typeof(GroupBoxWithChamfer))]
public class GroupBoxWithChamfer : BaseContainer
{
    #region Private Data Members
    // The enum object to store the colorscheme value
    private EnmColorScheme meColorScheme = EnmColorScheme.Origam;
    private IPersistenceService _persistence =
        ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
        as IPersistenceService;
    #endregion
    public GroupBoxWithChamfer()
        : base()
    {
        this.ColorScheme = meColorScheme;
    }

    private Guid _styleId;

    [Browsable(browsable: false)]
    public Guid StyleId
    {
        get { return _styleId; }
        set { _styleId = value; }
    }

    [TypeConverter(type: typeof(StylesConverter))]
    public UIStyle Style
    {
        get
        {
            return (UIStyle)
                _persistence.SchemaProvider.RetrieveInstance(
                    type: typeof(UIStyle),
                    primaryKey: new ModelElementKey(id: this.StyleId)
                );
        }
        set { this.StyleId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]); }
    }
    #region Overridden Properties
    public override EnmColorScheme ColorScheme
    {
        get { return meColorScheme; }
        set
        {
            // Create object of ColorScheme Class
            ColorScheme oColor = new ColorScheme(aoColorScheme: value);

            // Set the controls Diffrent color properties depending on the
            // Color Scheme selected
            oColor.SetColorScheme(aCtrl: this);
            meColorScheme = value;
        }
    }

    // This property is being used in the BaseClass's OnPaint method to
    // get the drawing path of the control
    protected override GraphicsPath InteriorRegionPath
    {
        get
        {
            Rectangle oRectangle = new Rectangle(
                x: this.BorderRectangle.X + 1,
                y: this.BorderRectangle.Y + 1,
                width: this.BorderRectangle.Width - 2,
                height: this.BorderRectangle.Height - 2
            );
            Size oSize = new Size(
                width: mosizeBorderPixelIndent.Width - 2,
                height: mosizeBorderPixelIndent.Height - 2
            );
            return this.GetRoundedRectanglarPath(
                aoRectangle: oRectangle,
                aoTextSize: moTextSize,
                aoCurveSize: oSize
            );
        }
    }

    // This property is being used in the Base Class's OnPaint method to
    // get the region path of the control to shape the control that way
    protected override GraphicsPath ExteriorRegionPath
    {
        get
        {
            Rectangle oRectangle = new Rectangle(
                x: this.BorderRectangle.X,
                y: this.BorderRectangle.Y,
                width: this.BorderRectangle.Width + 3,
                height: this.BorderRectangle.Height + 3
            );
            Size oSize = new Size(
                width: mosizeBorderPixelIndent.Width + 2,
                height: mosizeBorderPixelIndent.Width + 2
            );
            return this.GetRoundedRectanglarPath(
                aoRectangle: oRectangle,
                aoTextSize: new SizeF(width: moTextSize.Width + 3, height: moTextSize.Height),
                aoCurveSize: oSize
            );
        }
    }
    #endregion
    #region Private Methods
    // This Function Gets the Graphic Path to draw the Rectangle with Chamfer
    private GraphicsPath GetRoundedRectanglarPath(
        Rectangle aoRectangle,
        SizeF aoTextSize,
        Size aoCurveSize
    )
    {
        GraphicsPath oGraphicPath = new GraphicsPath();
        //=======================================================================
        // Following code adds path for the chamfer to be drawn
        //-----------------------------------------------------------------------

        // Add arc for the top left corner curve to the graphic path
        oGraphicPath.AddArc(
            x: aoRectangle.Left,
            y: aoRectangle.Top,
            width: aoCurveSize.Width,
            height: aoCurveSize.Height,
            startAngle: 180,
            sweepAngle: 90
        );

        // Add top horizontal line for chamfer to the graphic path
        oGraphicPath.AddLine(
            x1: aoRectangle.Left + (Single)(aoCurveSize.Height / 2),
            y1: aoRectangle.Top,
            x2: aoRectangle.Left + (Single)(aoCurveSize.Height / 2) + aoTextSize.Width + 2,
            y2: aoRectangle.Top
        );

        // Add Right side arc for the chamfer to the Graphics object
        oGraphicPath.AddArc(
            x: aoRectangle.Left + aoTextSize.Width + 7,
            y: aoRectangle.Top,
            width: aoCurveSize.Width,
            height: aoCurveSize.Height,
            startAngle: 270,
            sweepAngle: 90
        );
        //=======================================================================

        // Add Top Horizontal line below the chamfer to the graphic path object
        oGraphicPath.AddLine(
            x1: aoRectangle.Left + aoTextSize.Width + (Single)(aoCurveSize.Width + 7),
            y1: aoRectangle.Top + (Single)(aoCurveSize.Height / 2),
            x2: aoRectangle.Right - (Single)(aoCurveSize.Height / 2),
            y2: aoRectangle.Top + (Single)(aoCurveSize.Height / 2)
        );

        // Add arc for the top right corner curve to the Graphics Path object
        oGraphicPath.AddArc(
            x: aoRectangle.Right - aoCurveSize.Width,
            y: aoRectangle.Top + (Single)(aoCurveSize.Height / 2),
            width: aoCurveSize.Width,
            height: aoCurveSize.Height,
            startAngle: 270,
            sweepAngle: 90
        );
        // Add Right Vertical Line to the Graphics Path object
        oGraphicPath.AddLine(
            x1: aoRectangle.Right,
            y1: aoRectangle.Top + aoCurveSize.Height,
            x2: aoRectangle.Right,
            y2: aoRectangle.Bottom - (Single)(aoCurveSize.Height / 2)
        );

        // Add arc for the bottom right corner curve to the Graphics Path object
        oGraphicPath.AddArc(
            x: aoRectangle.Right - aoCurveSize.Width,
            y: aoRectangle.Bottom - aoCurveSize.Height,
            width: aoCurveSize.Width,
            height: aoCurveSize.Height,
            startAngle: 0,
            sweepAngle: 90
        );

        // Add Bottom Horizontal line to the Graphics Path object
        oGraphicPath.AddLine(
            x1: aoRectangle.Right - (Single)(aoCurveSize.Width / 2),
            y1: aoRectangle.Bottom,
            x2: aoRectangle.Left + (Single)(aoCurveSize.Width / 2),
            y2: aoRectangle.Bottom
        );

        // Add arc for the bottom left corner curve to the graphics path object
        oGraphicPath.AddArc(
            x: aoRectangle.Left,
            y: aoRectangle.Bottom - aoCurveSize.Height,
            width: aoCurveSize.Width,
            height: aoCurveSize.Height,
            startAngle: 90,
            sweepAngle: 90
        );

        // Add Left Vertical Line to the GraphicsPath object
        oGraphicPath.AddLine(
            x1: aoRectangle.Left,
            y1: aoRectangle.Bottom - (Single)(aoCurveSize.Height / 2),
            x2: aoRectangle.Left,
            y2: aoRectangle.Top + (Single)(aoCurveSize.Height / 2)
        );
        return oGraphicPath;
    }
    #endregion
    #region Overridden Methods
    // This method is called in the OnPaint Method of the base class
    protected override void DrawBorder(System.Drawing.Graphics aoGraphics, Rectangle aoRectangle)
    {
        Pen oPen;
        Size oSize = new Size(
            width: mosizeBorderPixelIndent.Width,
            height: mosizeBorderPixelIndent.Height
        );

        Rectangle oRectangle = new Rectangle(
            x: aoRectangle.X,
            y: aoRectangle.Y,
            width: aoRectangle.Width,
            height: aoRectangle.Height
        );
        SizeF aotextsize = aoGraphics.MeasureString(text: this.Text, font: this.Font);
        // Draw the shadows for the borders
        for (int i = 0; i <= 2; i++)
        {
            // Creates a pen to draw Lines and Arcs Dark To Light
            oPen = new Pen(
                color: Color.FromArgb(alpha: (2 - i + 1) * 64, baseColor: this.ShadowColor)
            );

            // Draws a shadow arc for the Chamfer's right hand side
            aoGraphics.DrawArc(
                pen: oPen,
                x: oRectangle.Left + aotextsize.Width - i,
                y: oRectangle.Top + 2,
                width: oSize.Width,
                height: oSize.Height,
                startAngle: 270,
                sweepAngle: 90
            );

            // Draws a shadow arc for the Top Right corner
            aoGraphics.DrawArc(
                pen: oPen,
                x: oRectangle.Right - oSize.Width,
                y: oRectangle.Top + (Single)(oSize.Height / 2) + 2,
                width: oSize.Width,
                height: oSize.Height,
                startAngle: 270,
                sweepAngle: 90
            );

            // Draws a vertical shadow line for the right side
            aoGraphics.DrawLine(
                pen: oPen,
                x1: oRectangle.Right,
                y1: oRectangle.Top + oSize.Height,
                x2: oRectangle.Right,
                y2: oRectangle.Bottom - (Single)(oSize.Height / 2)
            );

            // Draws a shadow arc for bottom right corner
            aoGraphics.DrawArc(
                pen: oPen,
                x: oRectangle.Right - oSize.Width,
                y: oRectangle.Bottom - oSize.Height,
                width: oSize.Width,
                height: oSize.Height,
                startAngle: 0,
                sweepAngle: 90
            );

            // Creates a pen to draw lines and arcs Light to Dark
            oPen = new Pen(
                color: Color.FromArgb(alpha: (2 - i) * 127, baseColor: this.ShadowColor)
            );

            // Draws a horizontal shadow line for the bottom
            aoGraphics.DrawLine(
                pen: oPen,
                x1: oRectangle.Right - (Single)(oSize.Width / 2),
                y1: oRectangle.Bottom,
                x2: oRectangle.Left + (Single)(oSize.Width / 2),
                y2: oRectangle.Bottom
            );

            // Draw a shadow arc for the bottom left corner
            aoGraphics.DrawArc(
                pen: oPen,
                x: oRectangle.Left + 2,
                y: oRectangle.Bottom - oSize.Height,
                width: oSize.Width,
                height: oSize.Height,
                startAngle: 90,
                sweepAngle: 90
            );

            // Increasing the Rectangles X and Y position
            oRectangle.X += 1;
            oRectangle.Y += 1;

            // Reducing Height and width of the rectangle
            oRectangle.Width -= 2;
            oRectangle.Height -= 2;

            // Reducing the size of the arcs to draw the arcs properly
            oSize.Height -= 2;
            oSize.Width -= 2;
            oPen.Dispose();
        }
    }
    #endregion
}
