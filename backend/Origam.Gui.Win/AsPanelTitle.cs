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
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;


namespace Origam.Gui.Win;

/// <summary>
/// Summary description for AsPanelTitle.
/// </summary>
public class AsPanelTitle : Panel
{
	public AsPanelTitle() : base()
	{
			this.SetStyle(System.Windows.Forms.ControlStyles.UserPaint | System.Windows.Forms.ControlStyles.AllPaintingInWmPaint | System.Windows.Forms.ControlStyles.DoubleBuffer,true);
			this.ResizeRedraw = true;
		}

	private string _panelTitle = "";
	public string PanelTitle
	{
		get
		{
				return _panelTitle;
			}
		set
		{
				_panelTitle = value;
				this.Invalidate(true);
			}
	}

	private Bitmap _panelIcon;
	public Bitmap PanelIcon
	{
		get
		{
				return _panelIcon;
			}
		set
		{
				_panelIcon = value;
				this.Invalidate(true);
			}
	}

	private Bitmap _statusIcon;
	public Bitmap StatusIcon
	{
		get
		{
				return _statusIcon;
			}
		set
		{
				_statusIcon = value;
				this.Invalidate(true);
			}
	}

	private Color _startColor;
	public Color StartColor
	{
		get
		{
				return _startColor;
			}
		set
		{
				_startColor = value;
				this.Invalidate(true);
			}
	}

	private Color _endColor;
	public Color EndColor
	{
		get
		{
				return _endColor;
			}
		set
		{
				_endColor = value;
				this.Invalidate(true);
			}
	}

	private Color _middleStartColor;
	public Color MiddleStartColor
	{
		get
		{
				return _middleStartColor;
			}
		set
		{
				_middleStartColor = value;
				this.Invalidate(true);
			}
	}

	private Color _middleEndColor;
	public Color MiddleEndColor
	{
		get
		{
				return _middleEndColor;
			}
		set
		{
				_middleEndColor = value;
				this.Invalidate(true);
			}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
			if(this.Width == 0) return;
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
				int splitPosition = GetColorSplitPosition(this.Height);

//				// declare linear gradient brush for fill background of label
//				GBrush = new LinearGradientBrush(
//					new Point(0, 0),
//					new Point(0, splitPosition), this.StartColor, this.MiddleEndColor);
//					//new Point(this.Width, 0), this.StartColor, this.EndColor);

				GBrush = new LinearGradientBrush(
					new Point(0, 0),
					new Point(0, splitPosition), this.StartColor, this.MiddleEndColor);

//				Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
//				path = RoundedRect.CreatePath(rect, 7, 0, CornerType.None);
//				graphics.FillPath(GBrush, path);

				graphics.FillRectangle(GBrush, 0, 0, this.Width, splitPosition);

				Rectangle rect = new Rectangle(0, splitPosition, this.Width, this.Height-splitPosition+2);
				
				GBrush = new LinearGradientBrush(rect, this.MiddleStartColor, this.EndColor, LinearGradientMode.Vertical);
	
				graphics.FillRectangle(GBrush, rect);

				// draw text on label
				drawBrush = new SolidBrush(this.ForeColor);
				sf = new StringFormat();
				// align with center
				sf.Alignment = StringAlignment.Near;
				sf.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Show;

				// set rectangle bound text
				float x = 5;
				if(PanelIcon != null) x = 20;

				RectangleF rectF = new RectangleF(x, 12-Font.Height/2, this.Width, this.Height);
				RectangleF rectIcon = new RectangleF(3, 4, 16, 16);
				// output string
				font = new Font(this.Font, FontStyle.Bold);
				
				if(PanelIcon != null) graphics.DrawImage(this.PanelIcon, rectIcon);
				graphics.DrawString(this.PanelTitle, font, drawBrush, rectF, sf);

				if(StatusIcon != null) 
				{
					RectangleF rectStatusIcon = new RectangleF(graphics.MeasureString(this.PanelTitle, font, new PointF(x, 0), sf).Width + 14, 4, 16, 16);
					graphics.DrawImage(this.StatusIcon, rectStatusIcon);
				}
			}
			finally
			{
				if(path != null) path.Dispose();
				//			//graphics.Dispose();
				if(brush != null) brush.Dispose();
				if(GBrush != null) GBrush.Dispose();
				if(drawBrush != null) drawBrush.Dispose();
				if(sf != null) sf.Dispose();
				if(font != null) font.Dispose();
			}
		}

	private int GetColorSplitPosition(int height)
	{
			return Convert.ToInt32((height / 2.5));
		}
}