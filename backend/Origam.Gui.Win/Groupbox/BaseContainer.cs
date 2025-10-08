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
    private Size ODEFAULT_SIZEBORDERPIXELINDET = new Size(16, 16); // Default value of moSizeBorderPixelIndent
    private static Color ODEFAULT_GRADIENTTOPCOLOR = Color.FromArgb(225, 225, 183); // Default value of GradientTopColor Property
    private static Color ODEFAULT_GRADIENTBOTTOMCOLOR = Color.FromArgb(167, 168, 127); // Default value of GradientBottomColor Property
    private static Color ODEFAULT_HEADINGTEXTCOLOR = Color.FromArgb(57, 66, 1); // Default value of HeaderTextColor Property
    private static Color ODEFAULT_INTERIORTOPCOLOR = Color.FromArgb(245, 243, 219); // Default value of InteriorGradientTopColor Property
    private static Color ODEFAULT_INTERIORBOTTOMCOLOR = Color.FromArgb(214, 209, 153); // Default value of InteriorGradientBottomColor Property
    private static Color ODEFAULT_SHADOWCOLOR = Color.FromArgb(142, 143, 116); // Default value of ShadowColor Property

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
            return new Rectangle(1, 1, rc.Width - 3, rc.Height - 3);
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
                    this.ClientRectangle,
                    moGradientTopColor,
                    moGradientBottomColor,
                    LinearGradientMode.Vertical
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
                this.BorderRectangle.X,
                this.BorderRectangle.Y,
                this.BorderRectangle.Width + 3,
                this.BorderRectangle.Height + 3
            );
            Size oSize = new Size(
                mosizeBorderPixelIndent.Width + 2,
                mosizeBorderPixelIndent.Height + 2
            );
            return this.GetRoundedRectanglarPath(oRectangle, oSize);
        }
    }

    // This property is Used in the OnPaint Method to define path to draw the control
    protected virtual GraphicsPath InteriorRegionPath
    {
        get
        {
            Rectangle oRectangle = new Rectangle(
                this.BorderRectangle.X + 1,
                this.BorderRectangle.Y + 1,
                this.BorderRectangle.Width - 2,
                this.BorderRectangle.Height - 2
            );
            Size oSize = new Size(
                mosizeBorderPixelIndent.Width - 2,
                mosizeBorderPixelIndent.Height - 2
            );
            return this.GetRoundedRectanglarPath(oRectangle, oSize);
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
            System.Windows.Forms.ControlStyles.UserPaint
                | System.Windows.Forms.ControlStyles.AllPaintingInWmPaint
                | System.Windows.Forms.ControlStyles.DoubleBuffer,
            true
        );
        mosizeBorderPixelIndent = ODEFAULT_SIZEBORDERPIXELINDET;
    }

    #region Overridable Methods
    // This procedure draws the Shadows for the outer Borders and gets called from OnPaint Method
    protected virtual void DrawBorder(Graphics aoGraphics, Rectangle aoRectangle)
    {
        Pen oPen;
        Size oSize = new Size(mosizeBorderPixelIndent.Width, mosizeBorderPixelIndent.Height);
        Rectangle oRectangle = new Rectangle(
            aoRectangle.X,
            aoRectangle.Y,
            aoRectangle.Width,
            aoRectangle.Height
        );
        SizeF szText = aoGraphics.MeasureString(this.Text, this.Font);
        // We are looping 3 times for a 3 pixel wide shadow.
        for (int i = 0; i < 3; i++)
        {
            // Creates a pen to draw Lines and Arcs Dark To Light
            oPen = new Pen(Color.FromArgb((2 - i + 1) * 64, moShadowColor));
            // Draws a shadow arc for the Top Right corner
            aoGraphics.DrawArc(
                oPen,
                oRectangle.Right - oSize.Width,
                oRectangle.Top + 2,
                oSize.Width,
                oSize.Height,
                270,
                90
            );

            // Draws a vertical shadow line for the right side
            aoGraphics.DrawLine(
                oPen,
                oRectangle.Right,
                oRectangle.Top + (Single)(oSize.Height / 2),
                oRectangle.Right,
                oRectangle.Bottom - (Single)(oSize.Height / 2)
            );
            // Draws a shadow arc for bottom right corner
            aoGraphics.DrawArc(
                oPen,
                oRectangle.Right - oSize.Width,
                oRectangle.Bottom - oSize.Height,
                oSize.Width,
                oSize.Height,
                0,
                90
            );
            // Draws a horizontal shadow line for the bottom
            aoGraphics.DrawLine(
                oPen,
                oRectangle.Right - (Single)(oSize.Width / 2),
                oRectangle.Bottom,
                oRectangle.Left + (Single)(oSize.Width / 2),
                oRectangle.Bottom
            );

            // Creates a pen to draw lines and arcs Light to Dark
            oPen = new Pen(Color.FromArgb((2 - i) * 127, moShadowColor));
            // Draw a shadow arc for the bottom left corner
            aoGraphics.DrawArc(
                oPen,
                oRectangle.Left + 2,
                oRectangle.Bottom - oSize.Height,
                oSize.Width,
                oSize.Height,
                90,
                90
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
            this.BorderRectangle.X + miBorderWidth + 1,
            this.BorderRectangle.Y + 12 + miBorderWidth,
            this.BorderRectangle.Width - (miBorderWidth * 2),
            this.BorderRectangle.Height - (12 + miBorderWidth * 2)
        );
        SolidBrush oSolidBrush;
        for (int Index = 1; Index >= 0; Index--)
        {
            // Define Shadow Brushes Dark to Light
            oSolidBrush = new SolidBrush(Color.FromArgb(127 * (2 - Index), moShadowColor));
            Pen oPen = new Pen(oSolidBrush);
            // Draws vertical line on Left side
            aoGraphics.DrawLine(
                oPen,
                oRcInterior.X,
                oRcInterior.Y,
                oRcInterior.X,
                oRcInterior.Bottom
            );

            // Draws horizontal lines on the top
            aoGraphics.DrawLine(
                oPen,
                oRcInterior.X,
                oRcInterior.Y,
                oRcInterior.Right,
                oRcInterior.Y
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
            oRcInterior,
            moInteriorTopColor,
            moInteriorBottomColor,
            LinearGradientMode.Vertical
        );
        // Blend is used to define the blend of the gradient
        Blend oBlend = new Blend();
        oBlend.Factors = IARR_RELATIVEINTENSITIES;
        oBlend.Positions = IARR_RELATIVEPOSITIONS;
        oLinearGradient.Blend = oBlend;
        // Fill the rectangle using Gradient Brush created above
        aoGraphics.FillRectangle(oLinearGradient, oRcInterior);
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
            aoRectangle.Left + (Single)(aoSize.Height / 2),
            aoRectangle.Top,
            aoRectangle.Right - (Single)(aoSize.Height / 2),
            aoRectangle.Top
        );

        // Add arc for the top right corner curve to the Graphics Path object
        oExteriorGraphicPath.AddArc(
            aoRectangle.Right - aoSize.Width,
            aoRectangle.Top,
            aoSize.Width,
            aoSize.Height,
            270,
            90
        );

        // Add right vertical line to the Graphics Path object
        oExteriorGraphicPath.AddLine(
            aoRectangle.Right,
            aoRectangle.Top + aoSize.Height,
            aoRectangle.Right,
            aoRectangle.Bottom - (Single)(aoSize.Height / 2)
        );

        // Add the bottom right corner curve to the Graphics object
        oExteriorGraphicPath.AddArc(
            aoRectangle.Right - aoSize.Width,
            aoRectangle.Bottom - aoSize.Height,
            aoSize.Width,
            aoSize.Height,
            0,
            90
        );

        // Add the bottom horizontal line to the Graphics Path object
        oExteriorGraphicPath.AddLine(
            aoRectangle.Right - (Single)(aoSize.Width / 2),
            aoRectangle.Bottom,
            aoRectangle.Left + (Single)(aoSize.Width / 2),
            aoRectangle.Bottom
        );

        // Add arc for the bottom left curve to the Graphics object
        oExteriorGraphicPath.AddArc(
            aoRectangle.Left,
            aoRectangle.Bottom - aoSize.Height,
            aoSize.Width,
            aoSize.Height,
            90,
            90
        );

        // Add left vertical line to the Graphics Path object
        oExteriorGraphicPath.AddLine(
            aoRectangle.Left,
            aoRectangle.Bottom - (Single)(aoSize.Height / 2),
            aoRectangle.Left,
            aoRectangle.Top + (Single)(aoSize.Height / 2)
        );

        // Add arc for the top left curve to the Graphics object
        oExteriorGraphicPath.AddArc(
            aoRectangle.Left,
            aoRectangle.Top,
            aoSize.Width,
            aoSize.Height,
            180,
            90
        );
        return oExteriorGraphicPath;
    }
    #endregion
    #region Overriden Events
    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
    {
        // Get the size of the string in pixels for the string for a font
        this.moTextSize = e.Graphics.MeasureString(this.Text, this.Font);
        // Original Smoothing is Saved and Smoothing mode mode is change to AntiAlias
        SmoothingMode oldSmooting = e.Graphics.SmoothingMode;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Draws shadow border for the control
        DrawBorder(e.Graphics, this.BorderRectangle);

        // Fill the rectangle that represents the border with gradient
        e.Graphics.FillPath(this.InteriorRegionPathBrush, this.InteriorRegionPath);

        // Draws the gradient background with shadows
        DrawInterior(e.Graphics);
        // Defines string format to center the string
        StringFormat oStringFormat = new StringFormat();

        // The rectangle where the text is to be drawn
        RectangleF oRectangleF = new RectangleF(
            this.BorderRectangle.X + (Single)(this.mosizeBorderPixelIndent.Width / 2) + 8,
            this.BorderRectangle.Y + 2,
            moTextSize.Width + (Single)(this.mosizeBorderPixelIndent.Width / 2),
            moTextSize.Height
        );
        // Drawing the string in the rectangle
        using (SolidBrush brush = new SolidBrush(moHeadingTextColor))
        {
            e.Graphics.DrawString(this.Text, this.Font, brush, oRectangleF, oStringFormat);
        }
        // Reseting the smoothingmode back to original for OS purposes.
        e.Graphics.SmoothingMode = oldSmooting;

        // Using the graphics path property regionpath to define the non rectangular shape for the control
        this.Region = new Region(this.ExteriorRegionPath);
    }
    #endregion
}
