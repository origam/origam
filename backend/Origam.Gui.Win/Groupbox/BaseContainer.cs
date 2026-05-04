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
/// Summary description for BaseContainer.
/// </summary>
public abstract class BaseContainer : System.Windows.Forms.GroupBox, IGradientContainer
{
    #region Constants
    private const int IDEFAULT_BORDERWIDTH = 3; // Default value of  BorderWidth Property
    private Size ODEFAULT_SIZEBORDERPIXELINDET = new Size(width: 16, height: 16); // Default value of moSizeBorderPixelIndent
    private static Color ODEFAULT_GRADIENTTOPCOLOR = Color.FromArgb(
        red: 225,
        green: 225,
        blue: 183
    ); // Default value of GradientTopColor Property
    private static Color ODEFAULT_GRADIENTBOTTOMCOLOR = Color.FromArgb(
        red: 167,
        green: 168,
        blue: 127
    ); // Default value of GradientBottomColor Property
    private static Color ODEFAULT_HEADINGTEXTCOLOR = Color.FromArgb(red: 57, green: 66, blue: 1); // Default value of HeaderTextColor Property
    private static Color ODEFAULT_INTERIORTOPCOLOR = Color.FromArgb(
        red: 245,
        green: 243,
        blue: 219
    ); // Default value of InteriorGradientTopColor Property
    private static Color ODEFAULT_INTERIORBOTTOMCOLOR = Color.FromArgb(
        red: 214,
        green: 209,
        blue: 153
    ); // Default value of InteriorGradientBottomColor Property
    private static Color ODEFAULT_SHADOWCOLOR = Color.FromArgb(red: 142, green: 143, blue: 116); // Default value of ShadowColor Property

    // These values are used in LinerGradientBrush's blend property to specify the Factor and Postion
    // When the values are changed the gradient is drawn differently
    protected Single[] IARR_RELATIVEINTENSITIES = { 0.0F, 0.32F, 1.0F }; // Values for Factor property of blend
    protected Single[] IARR_RELATIVEPOSITIONS = { 0.0F, 0.44F, 1.0F }; // Values for Position property of blend
    #endregion
    #region Private Data Members
    // Defining the data member corresponding to different Properties and initializing default values
    private int miBorderWidth = IDEFAULT_BORDERWIDTH; // BorderWidth Property
    private Color moGradientTopColor = ODEFAULT_GRADIENTTOPCOLOR; // GradientTopColor Property
    private Color moGradientBottomColor = ODEFAULT_GRADIENTBOTTOMCOLOR; // GradientBottomColor Property
    private Color moHeadingTextColor = ODEFAULT_HEADINGTEXTCOLOR; // HeaderTextColor Property
    private Color moInteriorTopColor = ODEFAULT_INTERIORTOPCOLOR; // InteriorTopColor Property
    private Color moInteriorBottomColor = ODEFAULT_INTERIORBOTTOMCOLOR; // InteriorBottomColor Property
    private Color moShadowColor = ODEFAULT_SHADOWCOLOR; // ShadowColor Property
    #endregion
    #region Protected Members
    protected Size mosizeBorderPixelIndent; // Size of the radius of the curves at the corners
    protected SizeF moTextSize; // Size(In Floating Point) of the text in pixels based on the font

    // This property defines the border within which the whole control is to be drawn.
    protected Rectangle BorderRectangle
    {
        get
        {
            Rectangle rc = this.ClientRectangle; // We reduce the size of drawing to show everything properly.
            return new Rectangle(x: 1, y: 1, width: rc.Width - 3, height: rc.Height - 3);
        }
    }

    // This property defines the color of shadow of the control
    public Color ShadowColor
    {
        get { return moShadowColor; }
        set
        {
            moShadowColor = value;
            this.Invalidate();
        }
    }

    // This property defines the color of the header text
    public Color FontColor
    {
        get { return moHeadingTextColor; }
        set { moHeadingTextColor = value; }
    }

    // This property defines the Top Color of the BorderGradient
    Color IGradientBorderColor.BorderTopColor
    {
        get { return moGradientTopColor; }
        set { moGradientTopColor = value; }
    }

    // This property defines the Bottom Color of the BorderGradient
    Color IGradientBorderColor.BorderBottomColor
    {
        get { return moGradientBottomColor; }
        set { moGradientBottomColor = value; }
    }

    // This property defines the Top Color of the Background Gradient
    Color IGradientBackgroundColor.BackgroundTopColor
    {
        get { return moInteriorTopColor; }
        set { moInteriorTopColor = value; }
    }

    // This property defines the Bottom Color of the Background Gradient
    Color IGradientBackgroundColor.BackgroundBottomColor
    {
        get { return moInteriorBottomColor; }
        set { moInteriorBottomColor = value; }
    }
    #endregion
    #region Public Property
    // The colorscheme property which is to be implemented by the Child Classes
    public abstract EnmColorScheme ColorScheme { get; set; }

    // The BorderWidth Values are accessed and intialised using this property
    public int BorderWidth
    {
        get { return miBorderWidth; }
        set
        {
            miBorderWidth = value;
            this.Invalidate();
        }
    }
    #endregion

    #region Overridable Properties
    // This property is being used in the OnPaint Method to paint the border
    protected virtual Brush InteriorRegionPathBrush
    {
        get
        {
            // Brush of LinearGradient type is created to draw gradient
            System.Drawing.Drawing2D.LinearGradientBrush brush =
                new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect: this.ClientRectangle,
                    color1: moGradientTopColor,
                    color2: moGradientBottomColor,
                    linearGradientMode: LinearGradientMode.Vertical
                );
            // Blend is used to define the blending method for the gradient
            Blend blend = new Blend();
            blend.Factors = IARR_RELATIVEINTENSITIES;
            blend.Positions = IARR_RELATIVEPOSITIONS;
            brush.Blend = blend;
            return brush;
        }
    }

    // This Property is used in the OnPaint Method to define the region property of the control
    protected virtual GraphicsPath ExteriorRegionPath
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
                height: mosizeBorderPixelIndent.Height + 2
            );
            return this.GetRoundedRectanglarPath(aoRectangle: oRectangle, aoSize: oSize);
        }
    }

    // This property is Used in the OnPaint Method to define path to draw the control
    protected virtual GraphicsPath InteriorRegionPath
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
            return this.GetRoundedRectanglarPath(aoRectangle: oRectangle, aoSize: oSize);
        }
    }
    #endregion
    public BaseContainer()
        : base()
    {
        // This method is to specify to the OS that this control has its own OnPaint Method and
        // to use it. The double buffering is used so that the control does not flicker when the
        // Invalidate method is called.
        this.SetStyle(
            flag: System.Windows.Forms.ControlStyles.UserPaint
                | System.Windows.Forms.ControlStyles.AllPaintingInWmPaint
                | System.Windows.Forms.ControlStyles.DoubleBuffer,
            value: true
        );
        mosizeBorderPixelIndent = ODEFAULT_SIZEBORDERPIXELINDET;
    }

    #region Overridable Methods
    // This procedure draws the Shadows for the outer Borders and gets called from OnPaint Method
    protected virtual void DrawBorder(Graphics aoGraphics, Rectangle aoRectangle)
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
        SizeF szText = aoGraphics.MeasureString(text: this.Text, font: this.Font);
        // We are looping 3 times for a 3 pixel wide shadow.
        for (int i = 0; i < 3; i++)
        {
            // Creates a pen to draw Lines and Arcs Dark To Light
            oPen = new Pen(
                color: Color.FromArgb(alpha: (2 - i + 1) * 64, baseColor: moShadowColor)
            );
            // Draws a shadow arc for the Top Right corner
            aoGraphics.DrawArc(
                pen: oPen,
                x: oRectangle.Right - oSize.Width,
                y: oRectangle.Top + 2,
                width: oSize.Width,
                height: oSize.Height,
                startAngle: 270,
                sweepAngle: 90
            );

            // Draws a vertical shadow line for the right side
            aoGraphics.DrawLine(
                pen: oPen,
                x1: oRectangle.Right,
                y1: oRectangle.Top + (Single)(oSize.Height / 2),
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
            // Draws a horizontal shadow line for the bottom
            aoGraphics.DrawLine(
                pen: oPen,
                x1: oRectangle.Right - (Single)(oSize.Width / 2),
                y1: oRectangle.Bottom,
                x2: oRectangle.Left + (Single)(oSize.Width / 2),
                y2: oRectangle.Bottom
            );

            // Creates a pen to draw lines and arcs Light to Dark
            oPen = new Pen(color: Color.FromArgb(alpha: (2 - i) * 127, baseColor: moShadowColor));
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
            // Reducing the radius size of the arcs to draw the arcs properly
            oSize.Height -= 2;
            oSize.Width -= 2;
        }
    }

    // This Method is called from OnPaint Method to draw the Interior Part
    protected virtual void DrawInterior(Graphics aoGraphics)
    {
        // Create rectangle to draw interior
        Rectangle oRcInterior = new Rectangle(
            x: this.BorderRectangle.X + miBorderWidth + 1,
            y: this.BorderRectangle.Y + 12 + miBorderWidth,
            width: this.BorderRectangle.Width - (miBorderWidth * 2),
            height: this.BorderRectangle.Height - (12 + (miBorderWidth * 2))
        );
        SolidBrush oSolidBrush;
        for (int Index = 1; Index >= 0; Index--)
        {
            // Define Shadow Brushes Dark to Light
            oSolidBrush = new SolidBrush(
                color: Color.FromArgb(alpha: 127 * (2 - Index), baseColor: moShadowColor)
            );
            Pen oPen = new Pen(brush: oSolidBrush);
            // Draws vertical line on Left side
            aoGraphics.DrawLine(
                pen: oPen,
                x1: oRcInterior.X,
                y1: oRcInterior.Y,
                x2: oRcInterior.X,
                y2: oRcInterior.Bottom
            );

            // Draws horizontal lines on the top
            aoGraphics.DrawLine(
                pen: oPen,
                x1: oRcInterior.X,
                y1: oRcInterior.Y,
                x2: oRcInterior.Right,
                y2: oRcInterior.Y
            );
            // Increasing the X and Y postion of the rectangle
            oRcInterior.X += 1;
            oRcInterior.Y += 1;

            // Reducing the height and width of the rectangle
            oRcInterior.Width -= 2;
            oRcInterior.Height -= 2;
            oPen.Dispose();
            oSolidBrush.Dispose();
        }
        // Brush of LinearGradient type is created to draw gradient
        LinearGradientBrush oLinearGradient = new LinearGradientBrush(
            rect: oRcInterior,
            color1: moInteriorTopColor,
            color2: moInteriorBottomColor,
            linearGradientMode: LinearGradientMode.Vertical
        );
        // Blend is used to define the blend of the gradient
        Blend oBlend = new Blend();
        oBlend.Factors = IARR_RELATIVEINTENSITIES;
        oBlend.Positions = IARR_RELATIVEPOSITIONS;
        oLinearGradient.Blend = oBlend;
        // Fill the rectangle using Gradient Brush created above
        aoGraphics.FillRectangle(brush: oLinearGradient, rect: oRcInterior);
        oLinearGradient.Dispose();
    }
    #endregion
    #region Private methods
    // This function is used to get Rectangular GraphicsPath with Rounded Corner
    private GraphicsPath GetRoundedRectanglarPath(Rectangle aoRectangle, Size aoSize)
    {
        GraphicsPath oExteriorGraphicPath = new GraphicsPath();

        // Add top horizontal line to the Graphics Path Object
        oExteriorGraphicPath.AddLine(
            x1: aoRectangle.Left + (Single)(aoSize.Height / 2),
            y1: aoRectangle.Top,
            x2: aoRectangle.Right - (Single)(aoSize.Height / 2),
            y2: aoRectangle.Top
        );

        // Add arc for the top right corner curve to the Graphics Path object
        oExteriorGraphicPath.AddArc(
            x: aoRectangle.Right - aoSize.Width,
            y: aoRectangle.Top,
            width: aoSize.Width,
            height: aoSize.Height,
            startAngle: 270,
            sweepAngle: 90
        );

        // Add right vertical line to the Graphics Path object
        oExteriorGraphicPath.AddLine(
            x1: aoRectangle.Right,
            y1: aoRectangle.Top + aoSize.Height,
            x2: aoRectangle.Right,
            y2: aoRectangle.Bottom - (Single)(aoSize.Height / 2)
        );

        // Add the bottom right corner curve to the Graphics object
        oExteriorGraphicPath.AddArc(
            x: aoRectangle.Right - aoSize.Width,
            y: aoRectangle.Bottom - aoSize.Height,
            width: aoSize.Width,
            height: aoSize.Height,
            startAngle: 0,
            sweepAngle: 90
        );

        // Add the bottom horizontal line to the Graphics Path object
        oExteriorGraphicPath.AddLine(
            x1: aoRectangle.Right - (Single)(aoSize.Width / 2),
            y1: aoRectangle.Bottom,
            x2: aoRectangle.Left + (Single)(aoSize.Width / 2),
            y2: aoRectangle.Bottom
        );

        // Add arc for the bottom left curve to the Graphics object
        oExteriorGraphicPath.AddArc(
            x: aoRectangle.Left,
            y: aoRectangle.Bottom - aoSize.Height,
            width: aoSize.Width,
            height: aoSize.Height,
            startAngle: 90,
            sweepAngle: 90
        );

        // Add left vertical line to the Graphics Path object
        oExteriorGraphicPath.AddLine(
            x1: aoRectangle.Left,
            y1: aoRectangle.Bottom - (Single)(aoSize.Height / 2),
            x2: aoRectangle.Left,
            y2: aoRectangle.Top + (Single)(aoSize.Height / 2)
        );

        // Add arc for the top left curve to the Graphics object
        oExteriorGraphicPath.AddArc(
            x: aoRectangle.Left,
            y: aoRectangle.Top,
            width: aoSize.Width,
            height: aoSize.Height,
            startAngle: 180,
            sweepAngle: 90
        );
        return oExteriorGraphicPath;
    }
    #endregion
    #region Overriden Events
    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
    {
        // Get the size of the string in pixels for the string for a font
        this.moTextSize = e.Graphics.MeasureString(text: this.Text, font: this.Font);
        // Original Smoothing is Saved and Smoothing mode mode is change to AntiAlias
        SmoothingMode oldSmooting = e.Graphics.SmoothingMode;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Draws shadow border for the control
        DrawBorder(aoGraphics: e.Graphics, aoRectangle: this.BorderRectangle);

        // Fill the rectangle that represents the border with gradient
        e.Graphics.FillPath(brush: this.InteriorRegionPathBrush, path: this.InteriorRegionPath);

        // Draws the gradient background with shadows
        DrawInterior(aoGraphics: e.Graphics);
        // Defines string format to center the string
        StringFormat oStringFormat = new StringFormat();

        // The rectangle where the text is to be drawn
        RectangleF oRectangleF = new RectangleF(
            x: this.BorderRectangle.X + (Single)(this.mosizeBorderPixelIndent.Width / 2) + 8,
            y: this.BorderRectangle.Y + 2,
            width: moTextSize.Width + (Single)(this.mosizeBorderPixelIndent.Width / 2),
            height: moTextSize.Height
        );
        // Drawing the string in the rectangle
        using (SolidBrush brush = new SolidBrush(color: moHeadingTextColor))
        {
            e.Graphics.DrawString(
                s: this.Text,
                font: this.Font,
                brush: brush,
                layoutRectangle: oRectangleF,
                format: oStringFormat
            );
        }
        // Reseting the smoothingmode back to original for OS purposes.
        e.Graphics.SmoothingMode = oldSmooting;

        // Using the graphics path property regionpath to define the non rectangular shape for the control
        this.Region = new Region(path: this.ExteriorRegionPath);
    }
    #endregion
}
