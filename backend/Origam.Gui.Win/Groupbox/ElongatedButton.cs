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
                return new Rectangle(1, 1, rc.Width - 3, rc.Height - 2);
            }
            else
            {
                return new Rectangle(1, 1, rc.Width - 3, rc.Height - 3);
            }
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
            ColorScheme oColorScheme = new ColorScheme(mColorScheme);
            oColorScheme.SetColorScheme(this);
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
            if (Enum.IsDefined(typeof(DialogResult), value))
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
            this.OnClick(EventArgs.Empty);
        }
    }
    #endregion
    public ElongatedButton()
        : base()
    {
        this.SetStyle(
            ControlStyles.UserPaint
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.DoubleBuffer,
            true
        );
        this.Height = 17;
        this.Font = new Font("Tahoma", 8);
        clrBackground1 = Color.FromArgb(248, 245, 224);
        clrBackground2 = Color.FromArgb(194, 168, 120);

        clrFontDisabled = Color.FromArgb(156, 147, 113);
        clrFontMouseUp = Color.FromArgb(96, 83, 43);
        clrFontMouseDown = Color.Black;

        clrBorder1 = Color.FromArgb(229, 219, 196);
        clrBorder2 = Color.FromArgb(194, 168, 120);

        clrDefaultBorder = Color.FromArgb(189, 153, 74);

        clrDisabledBackground1 = Color.FromArgb(241, 236, 212);
        clrDisabledBackground2 = Color.FromArgb(216, 198, 159);
    }

    #region Private Methods

    //Gets the Shadow colors which are the alpha colors of the
    private Brush[] GetShadowBrushes()
    {
        int cintShadow = 2;
        Brush[] arrBrushes = new Brush[cintShadow - 1];
        int intAlphaOffset = 35;
        int intMaxAlpha = cintShadow * intAlphaOffset;

        for (int intIndex = 0; intIndex <= arrBrushes.GetUpperBound(0); intIndex++)
        {
            arrBrushes[intIndex] = new SolidBrush(
                Color.FromArgb(intMaxAlpha - (intIndex * intAlphaOffset), 174, 167, 124)
            );
        }

        return arrBrushes;
    }

    private void OnDrawDefault(Graphics g)
    {
        Rectangle rcBorder = this.BorderRectangle;
        GraphicsPath myPath = GetGraphicPath(rcBorder);

        LinearGradientBrush brushBackGround = new LinearGradientBrush(
            rcBorder,
            clrBackground1,
            clrBackground2,
            LinearGradientMode.Vertical
        );
        Single[] relativeIntensisities = new Single[] { 0.0F, 0.08F, 1.0F };
        Single[] relativePositions = new Single[] { 0.0F, 0.44F, 1.0F };
        Blend blend = new Blend();

        blend.Factors = relativeIntensisities;
        blend.Positions = relativePositions;
        brushBackGround.Blend = blend;

        g.FillPath(brushBackGround, myPath);

        //Draw dark to light border for default button
        SolidBrush brushPen = new SolidBrush(clrDefaultBorder);
        Pen ps = new Pen(brushPen);

        DrawBorder(g, ps, this.BorderRectangle);
        brushPen = new SolidBrush(Color.FromArgb(128, clrDefaultBorder));
        ps = new Pen(brushPen);
        Rectangle rc = new Rectangle(
            this.BorderRectangle.X + 1,
            this.BorderRectangle.Y + 1,
            this.BorderRectangle.Width - 2,
            this.BorderRectangle.Height - 2
        );
        DrawBorder(g, ps, rc);
        rc.X += 1;
        rc.Y += 1;
        rc.Width -= 2;
        rc.Height -= 2;
        brushPen = new SolidBrush(Color.FromArgb(64, clrDefaultBorder));
        ps = new Pen(brushPen);
        DrawBorder(g, ps, rc);
    }

    private void OnDrawNormal(Graphics g)
    {
        Rectangle rcBorder = this.BorderRectangle;
        GraphicsPath myPath = GetGraphicPath(rcBorder);
        Region rgn = new Region(this.BorderRectangle);
        rgn.Intersect(myPath);
        LinearGradientBrush brushBackGround = new LinearGradientBrush(
            rcBorder,
            clrBackground1,
            clrBackground2,
            LinearGradientMode.Vertical
        );
        Single[] relativeIntensisities = new Single[] { 0.0F, 0.08F, 1.0F };
        Single[] relativePositions = new Single[] { 0.0F, 0.44F, 1.0F };
        Blend blend = new Blend();
        blend.Factors = relativeIntensisities;
        blend.Positions = relativePositions;
        brushBackGround.Blend = blend;

        g.FillRegion(brushBackGround, rgn);

        LinearGradientBrush brushPen = new LinearGradientBrush(
            this.BorderRectangle,
            clrBorder1,
            clrBorder2,
            LinearGradientMode.ForwardDiagonal
        );

        brushPen.Blend = blend;
        Pen ps = new Pen(brushPen);

        DrawBorder(g, ps, this.BorderRectangle);
    }

    //Create Grahics Path for the elongated buttons
    private GraphicsPath GetGraphicPath(Rectangle rc)
    {
        int adjust = rc.Height % 2 == 0 ? 0 : 1;

        GraphicsPath Mypath = new GraphicsPath();

        //Add Top Line
        Mypath.AddLine(
            rc.Left + (Single)(rc.Height / 2),
            rc.Top,
            rc.Right - (Single)(rc.Height / 2),
            rc.Top
        );
        //Add Right Semi Circle
        Mypath.AddArc(rc.Right - rc.Height, rc.Top, rc.Height, rc.Height, 270, 180);
        //Add Bottom Line
        Mypath.AddLine(
            rc.Right - (Single)(rc.Height / 2) - adjust,
            rc.Bottom,
            rc.Left + (Single)(rc.Height / 2) + adjust,
            rc.Bottom
        );
        //Add Left Semi Circle
        Mypath.AddArc(rc.Left, rc.Top, rc.Height, rc.Height, 90, 180);

        return Mypath;
    }

    private void DrawBorder(Graphics g, Pen p, Rectangle rc)
    {
        int adjust = rc.Height % 2 == 0 ? 0 : 1;

        g.DrawLine(
            p,
            rc.Left + (Single)(rc.Height / 2),
            rc.Top,
            rc.Right - (Single)(rc.Height / 2),
            rc.Top
        );
        g.DrawArc(p, rc.Right - rc.Height, rc.Top, rc.Height, rc.Height, 270, 180);
        g.DrawLine(
            p,
            rc.Right - (Single)(rc.Height / 2) - adjust,
            rc.Bottom,
            rc.Left + (Single)(rc.Height / 2) + adjust,
            rc.Bottom
        );
        g.DrawArc(p, rc.Left, rc.Top, rc.Height, rc.Height, 90, 180);
    }

    private void OnDrawDisabled(Graphics g)
    {
        Rectangle rcBorder = this.BorderRectangle;
        GraphicsPath myPath = GetGraphicPath(rcBorder);

        LinearGradientBrush brushBackGround = new LinearGradientBrush(
            rcBorder,
            clrDisabledBackground1,
            clrDisabledBackground2,
            LinearGradientMode.Vertical
        );

        Single[] relativeIntensisities = new Single[] { 0.0F, 0.08F, 1.0F };
        Single[] relativePositions = new Single[] { 0.0F, 0.44F, 1.0F };

        Blend blend = new Blend();
        blend.Factors = relativeIntensisities;
        blend.Positions = relativePositions;
        brushBackGround.Blend = blend;
        g.FillPath(brushBackGround, myPath);
        LinearGradientBrush brushPen = new LinearGradientBrush(
            this.BorderRectangle,
            clrBorder1,
            clrBorder2,
            LinearGradientMode.ForwardDiagonal
        );
        brushPen.Blend = blend;
        Pen ps = new Pen(brushPen);
        DrawBorder(g, ps, this.BorderRectangle);
    }

    private void OnDrawPressed(Graphics g)
    {
        Rectangle rcBorder = this.BorderRectangle;
        GraphicsPath myPath = GetGraphicPath(rcBorder);

        LinearGradientBrush brushBackGround = new LinearGradientBrush(
            rcBorder,
            clrBackground2,
            clrBackground1,
            LinearGradientMode.Vertical
        );

        Single[] relativeIntensisities = new Single[] { 0.0F, 0.32F, 1.0F };
        Single[] relativePositions = new Single[] { 0.0F, 0.02F, 1.0F };

        Blend blend = new Blend();
        blend.Factors = relativeIntensisities;
        blend.Positions = relativePositions;
        brushBackGround.Blend = blend;
        g.FillPath(brushBackGround, myPath);
        LinearGradientBrush brushPen = new LinearGradientBrush(
            this.BorderRectangle,
            clrBorder1,
            clrBorder2,
            LinearGradientMode.ForwardDiagonal
        );

        brushPen.Blend = blend;
        Pen ps = new Pen(brushPen);
        DrawBorder(g, ps, this.BorderRectangle);
    }

    private void OnDrawText(Graphics g)
    {
        SizeF sz = g.MeasureString(this.Text, this.Font);
        Brush[] br = GetShadowBrushes();
        RectangleF rcText = new RectangleF(
            this.BorderRectangle.Left + ((this.BorderRectangle.Width - sz.Width) / 2),
            this.BorderRectangle.Top + ((this.BorderRectangle.Height - sz.Height) / 2),
            sz.Width,
            sz.Height
        );

        for (int intIndex = 0; intIndex <= br.GetUpperBound(0); intIndex++)
        {
            g.DrawString(
                this.Text,
                this.Font,
                br[intIndex],
                rcText.X + (br.GetUpperBound(0) - intIndex),
                rcText.Y + (br.GetUpperBound(0) - intIndex)
            );
        }
        if (enmState == ControlState.Normal)
        {
            if (this.Enabled)
            {
                g.DrawString(this.Text, this.Font, new SolidBrush(clrFontMouseUp), rcText);
            }
            else
            {
                g.DrawString(this.Text, this.Font, new SolidBrush(clrFontDisabled), rcText);
            }
        }
        else
        {
            g.DrawString(this.Text, this.Font, new SolidBrush(clrFontMouseDown), rcText);
        }
    }
    #endregion

    #region Overridden Methods

    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
    {
        this.OnPaintBackground(e);
        SmoothingMode oldSmothing = e.Graphics.SmoothingMode;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        switch (enmState)
        {
            case ControlState.Normal:
                if (this.Enabled)
                {
                    if (this.Focused || this.IsDefault)
                    {
                        //when the control has the focus this method is called
                        OnDrawDefault(e.Graphics);
                    }
                    else
                    {
                        //when the contrl does not have the focus this method is acalled
                        OnDrawNormal(e.Graphics);
                    }
                }
                else
                {
                    //when the button is disabled this method is called
                    OnDrawDisabled(e.Graphics);
                }
                break;
            case ControlState.Pressed:
                //when the mouse is pressed over the button
                OnDrawPressed(e.Graphics);
                break;
        }
        OnDrawText(e.Graphics);

        Rectangle rc = new Rectangle(
            this.BorderRectangle.X - 1,
            this.BorderRectangle.Y - 1,
            this.BorderRectangle.Width + 2,
            this.BorderRectangle.Height + 2
        );
        this.Region = new Region(GetGraphicPath(rc));
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
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && e.Clicks == 1)
        {
            enmState = ControlState.Pressed;
            this.Invalidate();
        }
    }

    //Change the state to normal
    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        enmState = ControlState.Normal;
        this.Invalidate();
    }
    #endregion
}
