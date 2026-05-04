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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for AsTabControl.
/// </summary>
public class AsTabControl : System.Windows.Forms.TabControl
{
    #region "    Variables "
    private TabControlDisplayManager _DisplayManager = TabControlDisplayManager.Framework;
    private IPersistenceService _persistence =
        ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
        as IPersistenceService;
    #endregion

    #region "    Properties "
    [
        System.ComponentModel.DefaultValue(
            type: typeof(TabControlDisplayManager),
            value: "Framework"
        ),
        System.ComponentModel.Category(category: "Appearance")
    ]
    public TabControlDisplayManager DisplayManager
    {
        get { return _DisplayManager; }
        set
        {
            _DisplayManager = value;
            if (this._DisplayManager.Equals(obj: TabControlDisplayManager.Framework))
            {
                this.SetStyle(flag: ControlStyles.UserPaint, value: true);
                this.ItemSize = new Size(width: 0, height: 15);
                this.Padding = new Point(x: 9, y: 0);
            }
            else
            {
                this.ItemSize = new Size(width: 0, height: 0);
                this.Padding = new Point(x: 6, y: 3);
                this.SetStyle(flag: ControlStyles.UserPaint, value: false);
            }
        }
    }
    #endregion

    #region "    Constructor "
    public AsTabControl()
        : base()
    {
        if (this._DisplayManager.Equals(obj: TabControlDisplayManager.Framework))
        {
            this.SetStyle(flag: ControlStyles.UserPaint, value: true);
            this.ItemSize = new Size(width: 0, height: 15);
            this.Padding = new Point(x: 9, y: 0);
        }
        this.SetStyle(flag: ControlStyles.SupportsTransparentBackColor, value: true);
        this.SetStyle(flag: ControlStyles.DoubleBuffer, value: true);
        this.SetStyle(flag: ControlStyles.ResizeRedraw, value: true);
        this.ResizeRedraw = true;
    }
    #endregion

    #region "    Private Methods "
    private System.Drawing.Drawing2D.GraphicsPath GetPath(int index)
    {
        System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
        path.Reset();
        Rectangle rect = this.GetTabRect(index: index);
        if (index == 0)
        {
            path.AddLine(
                x1: rect.Left + 1,
                y1: rect.Bottom + 1,
                x2: rect.Left + rect.Height,
                y2: rect.Top + 2
            );
            path.AddLine(
                x1: rect.Left + rect.Height + 4,
                y1: rect.Top,
                x2: rect.Right - 3,
                y2: rect.Top
            );
            path.AddLine(
                x1: rect.Right - 1,
                y1: rect.Top + 2,
                x2: rect.Right - 1,
                y2: rect.Bottom + 1
            );
        }
        else
        {
            if (index == this.SelectedIndex)
            {
                path.AddLine(
                    x1: rect.Left + 5 - rect.Height,
                    y1: rect.Bottom + 1,
                    x2: rect.Left + 4,
                    y2: rect.Top + 2
                );
                path.AddLine(x1: rect.Left + 8, y1: rect.Top, x2: rect.Right - 3, y2: rect.Top);
                path.AddLine(
                    x1: rect.Right - 1,
                    y1: rect.Top + 2,
                    x2: rect.Right - 1,
                    y2: rect.Bottom + 1
                );
                path.AddLine(
                    x1: rect.Right - 1,
                    y1: rect.Bottom + 1,
                    x2: rect.Left + 5 - rect.Height,
                    y2: rect.Bottom + 1
                );
            }
            else
            {
                path.AddLine(x1: rect.Left, y1: rect.Top + 6, x2: rect.Left + 4, y2: rect.Top + 2);
                path.AddLine(x1: rect.Left + 8, y1: rect.Top, x2: rect.Right - 3, y2: rect.Top);
                path.AddLine(
                    x1: rect.Right - 1,
                    y1: rect.Top + 2,
                    x2: rect.Right - 1,
                    y2: rect.Bottom + 1
                );
                path.AddLine(
                    x1: rect.Right - 1,
                    y1: rect.Bottom + 1,
                    x2: rect.Left,
                    y2: rect.Bottom + 1
                );
            }
        }
        return path;
    }

    private System.Drawing.Drawing2D.GraphicsPath GetActiveHeaderPath(int index)
    {
        System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
        path.Reset();
        Rectangle rect = this.GetTabRect(index: index);
        if (index == 0)
        {
            path.AddLine(
                x1: rect.Left + rect.Height + 4,
                y1: rect.Top + 1,
                x2: rect.Right - 3,
                y2: rect.Top + 1
            );
            path.AddLine(
                x1: rect.Left + rect.Height + 3,
                y1: rect.Top + 2,
                x2: rect.Right - 1,
                y2: rect.Top + 2
            );
        }
        else
        {
            path.AddLine(x1: rect.Left + 8, y1: rect.Top + 1, x2: rect.Right - 3, y2: rect.Top + 1);
            path.AddLine(x1: rect.Left + 7, y1: rect.Top + 2, x2: rect.Right - 1, y2: rect.Top + 2);
        }
        return path;
    }

    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
    {
        //   Paint the Background
        this.PaintTransparentBackground(g: e.Graphics, clipRect: this.ClientRectangle);
        //   Paint all the tabs
        if (this.TabCount > 0)
        {
            for (int index = 0; index <= this.TabCount - 1; index++)
            {
                this.PaintTab(e: e, index: index);
            }
        }
        //   paint a border round the pagebox
        //   We can't make the border disappear so have to do it like this.
        if (this.TabCount > 0)
        {
            Rectangle borderRect = this.TabPages[index: 0].Bounds;
            if (this.SelectedTab != null)
            {
                borderRect = this.SelectedTab.Bounds;
            }
            borderRect.Inflate(width: 1, height: 1);
            ControlPaint.DrawBorder(
                graphics: e.Graphics,
                bounds: borderRect,
                color: ThemedColors.ToolBorder,
                style: ButtonBorderStyle.Solid
            );
        }
        //   repaint the bit where the selected tab is
        switch (this.SelectedIndex)
        {
            case -1:
            {
                break;
            }
            case 0:
            {
                Rectangle selrect = this.GetTabRect(index: this.SelectedIndex);
                int selrectRight = selrect.Right;
                e.Graphics.DrawLine(
                    pen: new Pen(color: OrigamColorScheme.TabActiveEndColor),
                    x1: selrect.Left + 2,
                    y1: selrect.Bottom + 1,
                    x2: selrectRight - 2,
                    y2: selrect.Bottom + 1
                );
                break;
            }

            default:
            {
                Rectangle selrect = this.GetTabRect(index: this.SelectedIndex);
                int selrectRight = selrect.Right;
                e.Graphics.DrawLine(
                    pen: new Pen(color: OrigamColorScheme.TabActiveEndColor),
                    x1: selrect.Left + 6 - selrect.Height,
                    y1: selrect.Bottom + 1,
                    x2: selrectRight - 2,
                    y2: selrect.Bottom + 1
                );
                break;
            }
        }
    }

    protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs pevent)
    {
        if (this.DesignMode)
        {
            System.Drawing.Drawing2D.LinearGradientBrush backBrush =
                new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect: this.Bounds,
                    color1: SystemColors.ControlLightLight,
                    color2: SystemColors.ControlLight,
                    linearGradientMode: LinearGradientMode.Vertical
                );
            pevent.Graphics.FillRectangle(brush: backBrush, rect: this.Bounds);
            backBrush.Dispose();
        }
        else
        {
            this.PaintTransparentBackground(g: pevent.Graphics, clipRect: this.ClientRectangle);
        }
    }

    protected void PaintTransparentBackground(System.Drawing.Graphics g, Rectangle clipRect)
    {
        if ((this.Parent != null))
        {
            clipRect.Offset(pos: this.Location);
            PaintEventArgs e = new PaintEventArgs(graphics: g, clipRect: clipRect);
            GraphicsState state = g.Save();
            g.SmoothingMode = SmoothingMode.HighSpeed;
            try
            {
                g.TranslateTransform(dx: (float)-this.Location.X, dy: (float)-this.Location.Y);
                this.InvokePaintBackground(c: this.Parent, e: e);
                this.InvokePaint(c: this.Parent, e: e);
            }
            finally
            {
                g.Restore(gstate: state);
                clipRect.Offset(x: -this.Location.X, y: -this.Location.Y);
            }
        }
        else
        {
            System.Drawing.Drawing2D.LinearGradientBrush backBrush =
                new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect: this.Bounds,
                    color1: SystemColors.ControlLightLight,
                    color2: SystemColors.ControlLight,
                    linearGradientMode: LinearGradientMode.Vertical
                );
            g.FillRectangle(brush: backBrush, rect: this.Bounds);
            backBrush.Dispose();
        }
    }

    private void PaintTab(System.Windows.Forms.PaintEventArgs e, int index)
    {
        System.Drawing.Drawing2D.GraphicsPath path = this.GetPath(index: index);
        this.PaintTabBackground(graph: e.Graphics, index: index, path: path);
        this.PaintTabBorder(graph: e.Graphics, index: index, path: path);
        this.PaintTabText(graph: e.Graphics, index: index);
        this.PaintTabImage(graph: e.Graphics, index: index);
    }

    private void PaintTabBackground(
        System.Drawing.Graphics graph,
        int index,
        System.Drawing.Drawing2D.GraphicsPath path
    )
    {
        Rectangle rect = this.GetTabRect(index: index);
        if (rect.Height > 1 && rect.Width > 1)
        {
            System.Drawing.Brush buttonBrush;
            if (index == this.SelectedIndex)
            {
                buttonBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect: rect,
                    color1: OrigamColorScheme.TabActiveStartColor,
                    color2: OrigamColorScheme.TabActiveEndColor,
                    linearGradientMode: LinearGradientMode.Vertical
                );
            }
            else
            {
                buttonBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect: rect,
                    color1: OrigamColorScheme.TabInactiveStartColor,
                    color2: OrigamColorScheme.TabInactiveEndColor,
                    linearGradientMode: LinearGradientMode.Vertical
                );
            }
            graph.FillPath(brush: buttonBrush, path: path);
            buttonBrush.Dispose();
            //				if(index == this.SelectedIndex)
            //				{
            //					using(Brush headerBrush = new System.Drawing.SolidBrush(Color.Orange))
            //					{
            //						graph.FillPath(headerBrush, GetActiveHeaderPath(index));
            //					}
            //				}
        }
    }

    private void PaintTabBorder(
        System.Drawing.Graphics graph,
        int index,
        System.Drawing.Drawing2D.GraphicsPath path
    )
    {
        Pen borderPen = new Pen(color: SystemColors.ControlDark);
        if (index == this.SelectedIndex)
        {
            borderPen = new Pen(color: ThemedColors.ToolBorder);
        }
        graph.DrawPath(pen: borderPen, path: path);
        borderPen.Dispose();
    }

    private void PaintTabImage(System.Drawing.Graphics graph, int index)
    {
        Image tabImage = null;
        if (this.TabPages[index: index].ImageIndex > -1 && this.ImageList != null)
        {
            tabImage = this.ImageList.Images[index: this.TabPages[index: index].ImageIndex];
        }
        //			else if (this.TabPages[index].ImageKey.Trim().Length > 0 && this.ImageList != null)
        //			{
        //				tabImage = this.ImageList.Images(this.TabPages[index].ImageKey);
        //			}
        if (tabImage != null)
        {
            Rectangle rect = this.GetTabRect(index: index);
            graph.DrawImage(
                image: tabImage,
                x: rect.Right - rect.Height - 4,
                y: 4,
                width: rect.Height - 2,
                height: rect.Height - 2
            );
        }
    }

    private void PaintTabText(System.Drawing.Graphics graph, int index)
    {
        Rectangle rect = this.GetTabRect(index: index);
        Rectangle rect2 = new Rectangle(
            x: rect.Left + 8,
            y: rect.Top + 1,
            width: rect.Width - 6,
            height: rect.Height
        );
        if (index == 0)
        {
            rect2 = new Rectangle(
                x: rect.Left + rect.Height,
                y: rect.Top + 1,
                width: rect.Width - rect.Height,
                height: rect.Height
            );
        }

        string tabtext = this.TabPages[index: index].Text;
        System.Drawing.StringFormat format = new System.Drawing.StringFormat();
        format.Alignment = StringAlignment.Near;
        format.LineAlignment = StringAlignment.Center;
        format.Trimming = StringTrimming.EllipsisCharacter;
        Brush forebrush = null;
        if (this.TabPages[index: index].Enabled == false)
        {
            forebrush = SystemBrushes.ControlDark;
        }
        else
        {
            forebrush = new SolidBrush(color: OrigamColorScheme.TabInactiveForeColor);
        }
        Font tabFont = this.Font;
        if (index == this.SelectedIndex)
        {
            tabFont = new Font(prototype: this.Font, newStyle: FontStyle.Bold);
            if (index == 0)
            {
                rect2 = new Rectangle(
                    x: rect.Left + rect.Height,
                    y: rect.Top + 1,
                    width: rect.Width - rect.Height + 5,
                    height: rect.Height
                );
            }
            // p�id�l�no
            forebrush = new SolidBrush(color: OrigamColorScheme.TabActiveForeColor);
        }
        graph.DrawString(
            s: tabtext,
            font: tabFont,
            brush: forebrush,
            layoutRectangle: rect2,
            format: format
        );
    }
    #endregion
    public enum TabControlDisplayManager : int
    {
        Default,
        Framework,
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (this.TabCount > 0)
        {
            if (keyData == (Keys.Tab | Keys.Control))
            {
                if (this.SelectedIndex < (this.TabCount - 1))
                {
                    this.SelectedIndex += 1;
                }
                else
                {
                    this.SelectedIndex = 0;
                }
                this.SelectedTab.SelectNextControl(
                    ctl: this.SelectedTab,
                    forward: true,
                    tabStopOnly: true,
                    nested: true,
                    wrap: true
                );
                return true;
            }
            if (keyData == (Keys.Tab | Keys.Control | Keys.Shift))
            {
                if (this.SelectedIndex > 0)
                {
                    this.SelectedIndex -= 1;
                }
                else
                {
                    this.SelectedIndex = this.TabCount - 1;
                }
                this.SelectedTab.SelectNextControl(
                    ctl: this.SelectedTab,
                    forward: true,
                    tabStopOnly: true,
                    nested: true,
                    wrap: true
                );
                return true;
            }
        }
        return base.ProcessCmdKey(msg: ref msg, keyData: keyData);
    }

    protected override void OnSelectedIndexChanged(EventArgs e)
    {
        try
        {
            base.OnSelectedIndexChanged(e: e);
        }
        catch { }
        Invalidate();
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
}

public class ThemedColors
{
    #region "    Variables and Constants "
    private const string NormalColor = "NormalColor";
    private const string HomeStead = "HomeStead";
    private const string Metallic = "Metallic";
    private const string NoTheme = "NoTheme";
    private static Color[] _toolBorder;
    #endregion
    #region "    Properties "
    public static int CurrentThemeIndex
    {
        get { return ThemedColors.GetCurrentThemeIndex(); }
    }
    public static string CurrentThemeName
    {
        get { return ThemedColors.GetCurrentThemeName(); }
    }
    public static Color ToolBorder
    {
        get { return ThemedColors._toolBorder[ThemedColors.CurrentThemeIndex]; }
    }
    #endregion
    #region "    Constructors "
    private ThemedColors() { }

    static ThemedColors()
    {
        Color[] colorArray1;
        colorArray1 = new Color[]
        {
            Color.FromArgb(red: 127, green: 157, blue: 185),
            Color.FromArgb(red: 164, green: 185, blue: 127),
            Color.FromArgb(red: 165, green: 172, blue: 178),
            Color.FromArgb(red: 132, green: 130, blue: 132),
        };
        ThemedColors._toolBorder = colorArray1;
    }
    #endregion
    private static int GetCurrentThemeIndex()
    {
        int theme = (int)ColorScheme.NoTheme;
        //			if (VisualStyleInformation.IsSupportedByOS && VisualStyleInformation.IsEnabledByUser && Application.RenderWithVisualStyles)
        //			{
        //				switch (VisualStyleInformation.ColorScheme)
        //				{
        //					case NormalColor:
        //						theme = (int)ColorScheme.NormalColor;
        //						break;
        //					case HomeStead:
        //						theme = (int)ColorScheme.HomeStead;
        //						break;
        //					case Metallic:
        //						theme = (int)ColorScheme.Metallic;
        //						break;
        //					default:
        //						theme = (int)ColorScheme.NoTheme;
        //						break;
        //				}
        //			}
        return theme;
    }

    private static string GetCurrentThemeName()
    {
        string theme = NoTheme;
        //			if (VisualStyleInformation.IsSupportedByOS && VisualStyleInformation.IsEnabledByUser && Application.RenderWithVisualStyles)
        //			{
        //				theme = VisualStyleInformation.ColorScheme;
        //			}
        return theme;
    }

    public enum ColorScheme
    {
        NormalColor = 0,
        HomeStead = 1,
        Metallic = 2,
        NoTheme = 3,
    }
}
