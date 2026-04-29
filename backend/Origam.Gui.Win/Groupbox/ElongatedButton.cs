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
using System.Windows.Forms;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for ElongatedButton.
/// </summary>
public class ElongatedButton
    : System.Windows.Forms.ButtonBase,
        IGradientButtonColor,
        System.Windows.Forms.IButtonControl
{
    #region Enums
    private enum ControlState
    {
        Normal,
        Pressed,
    }
    #endregion
    #region Private Data Members
    private ControlState enmState = ControlState.Normal;
    private EnmColorScheme mColorScheme = EnmColorScheme.Yellow;
    private Color clrBackground1;
    private Color clrBackground2;
    private Color clrDisabledBackground1;
    private Color clrDisabledBackground2;
    private Color clrBorder1;
    private Color clrBorder2;
    private Color clrDefaultBorder;
    private Color clrFontMouseUp;
    private Color clrFontMouseDown;
    private Color clrFontDisabled;
    private DialogResult myDialogResult;
    #region Private Properties
    private Rectangle BorderRectangle
    {
        get
        {
            Rectangle rc = this.ClientRectangle;
            if (rc.Height % 2 == 0)
            {
                return new Rectangle(x: 1, y: 1, width: rc.Width - 3, height: rc.Height - 2);
            }

            return new Rectangle(x: 1, y: 1, width: rc.Width - 3, height: rc.Height - 3);
        }
    }
    #endregion
    #endregion

    #region Public Properties
    public override Color BackColor
    {
        get { return base.BackColor; }
        set { base.BackColor = Color.Transparent; }
    }
    public new FlatStyle FlatStyle
    {
        get { return base.FlatStyle; }
        set { base.FlatStyle = FlatStyle.Standard; }
    }

    public EnmColorScheme ColorScheme
    {
        get { return mColorScheme; }
        set
        {
            mColorScheme = value;
            ColorScheme oColorScheme = new ColorScheme(aoColorScheme: mColorScheme);
            oColorScheme.SetColorScheme(aCtrl: this);
        }
    }
    #endregion
    #region Interface Implementation
    Color IGradientBackgroundColor.BackgroundBottomColor
    {
        get { return clrBackground2; }
        set
        {
            clrBackground2 = value;
            this.Invalidate();
        }
    }

    Color IGradientBackgroundColor.BackgroundTopColor
    {
        get { return clrBackground1; }
        set
        {
            clrBackground1 = value;
            this.Invalidate();
        }
    }
    Color IGradientBorderColor.BorderBottomColor
    {
        get { return clrBorder1; }
        set
        {
            clrBorder1 = value;
            this.Invalidate();
        }
    }
    Color IGradientBorderColor.BorderTopColor
    {
        get { return clrBorder2; }
        set
        {
            clrBorder2 = value;
            this.Invalidate();
        }
    }
    Color IGradientDisabledColor.DisbaledBottomColor
    {
        get { return clrDisabledBackground2; }
        set
        {
            clrDisabledBackground2 = value;
            this.Invalidate();
        }
    }

    Color IGradientDisabledColor.DisabledTopColor
    {
        get { return clrDisabledBackground1; }
        set
        {
            clrDisabledBackground1 = value;
            this.Invalidate();
        }
    }
    Color IFontColor.FontColor
    {
        get { return clrFontMouseUp; }
        set
        {
            clrFontMouseUp = value;
            this.Invalidate();
        }
    }
    Color IGradientButtonColor.PressedFontColor
    {
        get { return clrFontMouseDown; }
        set { clrFontMouseDown = value; }
    }
    Color IGradientDisabledColor.DisabledFontColor
    {
        get { return clrFontDisabled; }
        set
        {
            clrFontDisabled = value;
            this.Invalidate();
        }
    }

    System.Drawing.Color IGradientButtonColor.DefaultBorderColor
    {
        get { return clrDefaultBorder; }
        set
        {
            clrDefaultBorder = value;
            this.Invalidate();
        }
    }

    // Add implementation to the IButtonControl.DialogResult property.
    public DialogResult DialogResult
    {
        get { return this.myDialogResult; }
        set
        {
            if (Enum.IsDefined(enumType: typeof(DialogResult), value: value))
            {
                this.myDialogResult = value;
            }
        }
    }

    // Add implementation to the IButtonControl.NotifyDefault method.
    public void NotifyDefault(bool value)
    {
        if (this.IsDefault != value)
        {
            this.IsDefault = value;
        }
    }

    // Add implementation to the IButtonControl.PerformClick method.
    public void PerformClick()
    {
        if (this.CanSelect)
        {
            this.OnClick(e: EventArgs.Empty);
        }
    }
    #endregion
    public ElongatedButton()
        : base()
    {
        this.SetStyle(
            flag: ControlStyles.UserPaint
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.DoubleBuffer,
            value: true
        );
        this.Height = 17;
        this.Font = new Font(familyName: "Tahoma", emSize: 8);
        clrBackground1 = Color.FromArgb(red: 248, green: 245, blue: 224);
        clrBackground2 = Color.FromArgb(red: 194, green: 168, blue: 120);

        clrFontDisabled = Color.FromArgb(red: 156, green: 147, blue: 113);
        clrFontMouseUp = Color.FromArgb(red: 96, green: 83, blue: 43);
        clrFontMouseDown = Color.Black;

        clrBorder1 = Color.FromArgb(red: 229, green: 219, blue: 196);
        clrBorder2 = Color.FromArgb(red: 194, green: 168, blue: 120);

        clrDefaultBorder = Color.FromArgb(red: 189, green: 153, blue: 74);

        clrDisabledBackground1 = Color.FromArgb(red: 241, green: 236, blue: 212);
        clrDisabledBackground2 = Color.FromArgb(red: 216, green: 198, blue: 159);
    }

    #region Private Methods

    //Gets the Shadow colors which are the alpha colors of the
    private Brush[] GetShadowBrushes()
    {
        int cintShadow = 2;
        Brush[] arrBrushes = new Brush[cintShadow - 1];
        int intAlphaOffset = 35;
        int intMaxAlpha = cintShadow * intAlphaOffset;

        for (int intIndex = 0; intIndex <= arrBrushes.GetUpperBound(dimension: 0); intIndex++)
        {
            arrBrushes[intIndex] = new SolidBrush(
                color: Color.FromArgb(
                    alpha: intMaxAlpha - (intIndex * intAlphaOffset),
                    red: 174,
                    green: 167,
                    blue: 124
                )
            );
        }

        return arrBrushes;
    }

    private void OnDrawDefault(Graphics g)
    {
        Rectangle rcBorder = this.BorderRectangle;
        GraphicsPath myPath = GetGraphicPath(rc: rcBorder);

        LinearGradientBrush brushBackGround = new LinearGradientBrush(
            rect: rcBorder,
            color1: clrBackground1,
            color2: clrBackground2,
            linearGradientMode: LinearGradientMode.Vertical
        );
        Single[] relativeIntensisities = new Single[] { 0.0F, 0.08F, 1.0F };
        Single[] relativePositions = new Single[] { 0.0F, 0.44F, 1.0F };
        Blend blend = new Blend();

        blend.Factors = relativeIntensisities;
        blend.Positions = relativePositions;
        brushBackGround.Blend = blend;

        g.FillPath(brush: brushBackGround, path: myPath);

        //Draw dark to light border for default button
        SolidBrush brushPen = new SolidBrush(color: clrDefaultBorder);
        Pen ps = new Pen(brush: brushPen);

        DrawBorder(g: g, p: ps, rc: this.BorderRectangle);
        brushPen = new SolidBrush(color: Color.FromArgb(alpha: 128, baseColor: clrDefaultBorder));
        ps = new Pen(brush: brushPen);
        Rectangle rc = new Rectangle(
            x: this.BorderRectangle.X + 1,
            y: this.BorderRectangle.Y + 1,
            width: this.BorderRectangle.Width - 2,
            height: this.BorderRectangle.Height - 2
        );
        DrawBorder(g: g, p: ps, rc: rc);
        rc.X += 1;
        rc.Y += 1;
        rc.Width -= 2;
        rc.Height -= 2;
        brushPen = new SolidBrush(color: Color.FromArgb(alpha: 64, baseColor: clrDefaultBorder));
        ps = new Pen(brush: brushPen);
        DrawBorder(g: g, p: ps, rc: rc);
    }

    private void OnDrawNormal(Graphics g)
    {
        Rectangle rcBorder = this.BorderRectangle;
        GraphicsPath myPath = GetGraphicPath(rc: rcBorder);
        Region rgn = new Region(rect: this.BorderRectangle);
        rgn.Intersect(path: myPath);
        LinearGradientBrush brushBackGround = new LinearGradientBrush(
            rect: rcBorder,
            color1: clrBackground1,
            color2: clrBackground2,
            linearGradientMode: LinearGradientMode.Vertical
        );
        Single[] relativeIntensisities = new Single[] { 0.0F, 0.08F, 1.0F };
        Single[] relativePositions = new Single[] { 0.0F, 0.44F, 1.0F };
        Blend blend = new Blend();
        blend.Factors = relativeIntensisities;
        blend.Positions = relativePositions;
        brushBackGround.Blend = blend;

        g.FillRegion(brush: brushBackGround, region: rgn);

        LinearGradientBrush brushPen = new LinearGradientBrush(
            rect: this.BorderRectangle,
            color1: clrBorder1,
            color2: clrBorder2,
            linearGradientMode: LinearGradientMode.ForwardDiagonal
        );

        brushPen.Blend = blend;
        Pen ps = new Pen(brush: brushPen);

        DrawBorder(g: g, p: ps, rc: this.BorderRectangle);
    }

    //Create Grahics Path for the elongated buttons
    private GraphicsPath GetGraphicPath(Rectangle rc)
    {
        int adjust = rc.Height % 2 == 0 ? 0 : 1;

        GraphicsPath Mypath = new GraphicsPath();

        //Add Top Line
        Mypath.AddLine(
            x1: rc.Left + (Single)(rc.Height / 2),
            y1: rc.Top,
            x2: rc.Right - (Single)(rc.Height / 2),
            y2: rc.Top
        );
        //Add Right Semi Circle
        Mypath.AddArc(
            x: rc.Right - rc.Height,
            y: rc.Top,
            width: rc.Height,
            height: rc.Height,
            startAngle: 270,
            sweepAngle: 180
        );
        //Add Bottom Line
        Mypath.AddLine(
            x1: rc.Right - (Single)(rc.Height / 2) - adjust,
            y1: rc.Bottom,
            x2: rc.Left + (Single)(rc.Height / 2) + adjust,
            y2: rc.Bottom
        );
        //Add Left Semi Circle
        Mypath.AddArc(
            x: rc.Left,
            y: rc.Top,
            width: rc.Height,
            height: rc.Height,
            startAngle: 90,
            sweepAngle: 180
        );

        return Mypath;
    }

    private void DrawBorder(Graphics g, Pen p, Rectangle rc)
    {
        int adjust = rc.Height % 2 == 0 ? 0 : 1;

        g.DrawLine(
            pen: p,
            x1: rc.Left + (Single)(rc.Height / 2),
            y1: rc.Top,
            x2: rc.Right - (Single)(rc.Height / 2),
            y2: rc.Top
        );
        g.DrawArc(
            pen: p,
            x: rc.Right - rc.Height,
            y: rc.Top,
            width: rc.Height,
            height: rc.Height,
            startAngle: 270,
            sweepAngle: 180
        );
        g.DrawLine(
            pen: p,
            x1: rc.Right - (Single)(rc.Height / 2) - adjust,
            y1: rc.Bottom,
            x2: rc.Left + (Single)(rc.Height / 2) + adjust,
            y2: rc.Bottom
        );
        g.DrawArc(
            pen: p,
            x: rc.Left,
            y: rc.Top,
            width: rc.Height,
            height: rc.Height,
            startAngle: 90,
            sweepAngle: 180
        );
    }

    private void OnDrawDisabled(Graphics g)
    {
        Rectangle rcBorder = this.BorderRectangle;
        GraphicsPath myPath = GetGraphicPath(rc: rcBorder);

        LinearGradientBrush brushBackGround = new LinearGradientBrush(
            rect: rcBorder,
            color1: clrDisabledBackground1,
            color2: clrDisabledBackground2,
            linearGradientMode: LinearGradientMode.Vertical
        );

        Single[] relativeIntensisities = new Single[] { 0.0F, 0.08F, 1.0F };
        Single[] relativePositions = new Single[] { 0.0F, 0.44F, 1.0F };

        Blend blend = new Blend();
        blend.Factors = relativeIntensisities;
        blend.Positions = relativePositions;
        brushBackGround.Blend = blend;
        g.FillPath(brush: brushBackGround, path: myPath);
        LinearGradientBrush brushPen = new LinearGradientBrush(
            rect: this.BorderRectangle,
            color1: clrBorder1,
            color2: clrBorder2,
            linearGradientMode: LinearGradientMode.ForwardDiagonal
        );
        brushPen.Blend = blend;
        Pen ps = new Pen(brush: brushPen);
        DrawBorder(g: g, p: ps, rc: this.BorderRectangle);
    }

    private void OnDrawPressed(Graphics g)
    {
        Rectangle rcBorder = this.BorderRectangle;
        GraphicsPath myPath = GetGraphicPath(rc: rcBorder);

        LinearGradientBrush brushBackGround = new LinearGradientBrush(
            rect: rcBorder,
            color1: clrBackground2,
            color2: clrBackground1,
            linearGradientMode: LinearGradientMode.Vertical
        );

        Single[] relativeIntensisities = new Single[] { 0.0F, 0.32F, 1.0F };
        Single[] relativePositions = new Single[] { 0.0F, 0.02F, 1.0F };

        Blend blend = new Blend();
        blend.Factors = relativeIntensisities;
        blend.Positions = relativePositions;
        brushBackGround.Blend = blend;
        g.FillPath(brush: brushBackGround, path: myPath);
        LinearGradientBrush brushPen = new LinearGradientBrush(
            rect: this.BorderRectangle,
            color1: clrBorder1,
            color2: clrBorder2,
            linearGradientMode: LinearGradientMode.ForwardDiagonal
        );

        brushPen.Blend = blend;
        Pen ps = new Pen(brush: brushPen);
        DrawBorder(g: g, p: ps, rc: this.BorderRectangle);
    }

    private void OnDrawText(Graphics g)
    {
        SizeF sz = g.MeasureString(text: this.Text, font: this.Font);
        Brush[] br = GetShadowBrushes();
        RectangleF rcText = new RectangleF(
            x: this.BorderRectangle.Left + ((this.BorderRectangle.Width - sz.Width) / 2),
            y: this.BorderRectangle.Top + ((this.BorderRectangle.Height - sz.Height) / 2),
            width: sz.Width,
            height: sz.Height
        );

        for (int intIndex = 0; intIndex <= br.GetUpperBound(dimension: 0); intIndex++)
        {
            g.DrawString(
                s: this.Text,
                font: this.Font,
                brush: br[intIndex],
                x: rcText.X + (br.GetUpperBound(dimension: 0) - intIndex),
                y: rcText.Y + (br.GetUpperBound(dimension: 0) - intIndex)
            );
        }
        if (enmState == ControlState.Normal)
        {
            if (this.Enabled)
            {
                g.DrawString(
                    s: this.Text,
                    font: this.Font,
                    brush: new SolidBrush(color: clrFontMouseUp),
                    layoutRectangle: rcText
                );
            }
            else
            {
                g.DrawString(
                    s: this.Text,
                    font: this.Font,
                    brush: new SolidBrush(color: clrFontDisabled),
                    layoutRectangle: rcText
                );
            }
        }
        else
        {
            g.DrawString(
                s: this.Text,
                font: this.Font,
                brush: new SolidBrush(color: clrFontMouseDown),
                layoutRectangle: rcText
            );
        }
    }
    #endregion

    #region Overridden Methods

    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
    {
        this.OnPaintBackground(pevent: e);
        SmoothingMode oldSmothing = e.Graphics.SmoothingMode;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        switch (enmState)
        {
            case ControlState.Normal:
            {
                if (this.Enabled)
                {
                    if (this.Focused || this.IsDefault)
                    {
                        //when the control has the focus this method is called
                        OnDrawDefault(g: e.Graphics);
                    }
                    else
                    {
                        //when the contrl does not have the focus this method is acalled
                        OnDrawNormal(g: e.Graphics);
                    }
                }
                else
                {
                    //when the button is disabled this method is called
                    OnDrawDisabled(g: e.Graphics);
                }
                break;
            }

            case ControlState.Pressed:
            {
                //when the mouse is pressed over the button
                OnDrawPressed(g: e.Graphics);
                break;
            }
        }
        OnDrawText(g: e.Graphics);

        Rectangle rc = new Rectangle(
            x: this.BorderRectangle.X - 1,
            y: this.BorderRectangle.Y - 1,
            width: this.BorderRectangle.Width + 2,
            height: this.BorderRectangle.Height + 2
        );
        this.Region = new Region(path: GetGraphicPath(rc: rc));
        e.Graphics.SmoothingMode = oldSmothing;
    }

    //Redraw control when the button is resized
    protected override void OnResize(EventArgs e)
    {
        this.Invalidate();
    }

    //Change the state to pressed
    protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
    {
        base.OnMouseDown(mevent: e);
        if (e.Button == MouseButtons.Left && e.Clicks == 1)
        {
            enmState = ControlState.Pressed;
            this.Invalidate();
        }
    }

    //Change the state to normal
    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e: e);
        enmState = ControlState.Normal;
        this.Invalidate();
    }
    #endregion
}
