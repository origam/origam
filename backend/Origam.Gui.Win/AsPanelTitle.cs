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

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for AsPanelTitle.
/// </summary>
public class AsPanelTitle : Panel
{
    public AsPanelTitle()
        : base()
    {
        this.SetStyle(
            flag: System.Windows.Forms.ControlStyles.UserPaint
                | System.Windows.Forms.ControlStyles.AllPaintingInWmPaint
                | System.Windows.Forms.ControlStyles.DoubleBuffer,
            value: true
        );
        this.ResizeRedraw = true;
    }

    private string _panelTitle = "";
    public string PanelTitle
    {
        get { return _panelTitle; }
        set
        {
            _panelTitle = value;
            this.Invalidate(invalidateChildren: true);
        }
    }
    private Bitmap _panelIcon;
    public Bitmap PanelIcon
    {
        get { return _panelIcon; }
        set
        {
            _panelIcon = value;
            this.Invalidate(invalidateChildren: true);
        }
    }
    private Bitmap _statusIcon;
    public Bitmap StatusIcon
    {
        get { return _statusIcon; }
        set
        {
            _statusIcon = value;
            this.Invalidate(invalidateChildren: true);
        }
    }
    private Color _startColor;
    public Color StartColor
    {
        get { return _startColor; }
        set
        {
            _startColor = value;
            this.Invalidate(invalidateChildren: true);
        }
    }
    private Color _endColor;
    public Color EndColor
    {
        get { return _endColor; }
        set
        {
            _endColor = value;
            this.Invalidate(invalidateChildren: true);
        }
    }
    private Color _middleStartColor;
    public Color MiddleStartColor
    {
        get { return _middleStartColor; }
        set
        {
            _middleStartColor = value;
            this.Invalidate(invalidateChildren: true);
        }
    }
    private Color _middleEndColor;
    public Color MiddleEndColor
    {
        get { return _middleEndColor; }
        set
        {
            _middleEndColor = value;
            this.Invalidate(invalidateChildren: true);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (this.Width == 0)
        {
            return;
        }
        //base.OnPaint (e);
        // rounded rectangle
        GraphicsPath path = null;
        Graphics graphics = null;
        SolidBrush brush = null;
        LinearGradientBrush GBrush = null;
        SolidBrush drawBrush = null;
        Font font = null;
        StringFormat sf = null;
        try
        {
            graphics = e.Graphics;
            int splitPosition = GetColorSplitPosition(height: this.Height);
            //				// declare linear gradient brush for fill background of label
            //				GBrush = new LinearGradientBrush(
            //					new Point(0, 0),
            //					new Point(0, splitPosition), this.StartColor, this.MiddleEndColor);
            //					//new Point(this.Width, 0), this.StartColor, this.EndColor);
            GBrush = new LinearGradientBrush(
                point1: new Point(x: 0, y: 0),
                point2: new Point(x: 0, y: splitPosition),
                color1: this.StartColor,
                color2: this.MiddleEndColor
            );
            //				Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            //				path = RoundedRect.CreatePath(rect, 7, 0, CornerType.None);
            //				graphics.FillPath(GBrush, path);
            graphics.FillRectangle(
                brush: GBrush,
                x: 0,
                y: 0,
                width: this.Width,
                height: splitPosition
            );
            Rectangle rect = new Rectangle(
                x: 0,
                y: splitPosition,
                width: this.Width,
                height: this.Height - splitPosition + 2
            );

            GBrush = new LinearGradientBrush(
                rect: rect,
                color1: this.MiddleStartColor,
                color2: this.EndColor,
                linearGradientMode: LinearGradientMode.Vertical
            );

            graphics.FillRectangle(brush: GBrush, rect: rect);
            // draw text on label
            drawBrush = new SolidBrush(color: this.ForeColor);
            sf = new StringFormat();
            // align with center
            sf.Alignment = StringAlignment.Near;
            sf.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Show;
            // set rectangle bound text
            float x = 5;
            if (PanelIcon != null)
            {
                x = 20;
            }

            RectangleF rectF = new RectangleF(
                x: x,
                y: 12 - (Font.Height / 2),
                width: this.Width,
                height: this.Height
            );
            RectangleF rectIcon = new RectangleF(x: 3, y: 4, width: 16, height: 16);
            // output string
            font = new Font(prototype: this.Font, newStyle: FontStyle.Bold);

            if (PanelIcon != null)
            {
                graphics.DrawImage(image: this.PanelIcon, rect: rectIcon);
            }

            graphics.DrawString(
                s: this.PanelTitle,
                font: font,
                brush: drawBrush,
                layoutRectangle: rectF,
                format: sf
            );
            if (StatusIcon != null)
            {
                RectangleF rectStatusIcon = new RectangleF(
                    x: graphics
                        .MeasureString(
                            text: this.PanelTitle,
                            font: font,
                            origin: new PointF(x: x, y: 0),
                            stringFormat: sf
                        )
                        .Width + 14,
                    y: 4,
                    width: 16,
                    height: 16
                );
                graphics.DrawImage(image: this.StatusIcon, rect: rectStatusIcon);
            }
        }
        finally
        {
            if (path != null)
            {
                path.Dispose();
            }
            //			//graphics.Dispose();
            if (brush != null)
            {
                brush.Dispose();
            }

            if (GBrush != null)
            {
                GBrush.Dispose();
            }

            if (drawBrush != null)
            {
                drawBrush.Dispose();
            }

            if (sf != null)
            {
                sf.Dispose();
            }

            if (font != null)
            {
                font.Dispose();
            }
        }
    }

    private int GetColorSplitPosition(int height)
    {
        return Convert.ToInt32(value: (height / 2.5));
    }
}
