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
        ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
    #endregion

    #region "    Properties "
    [
        System.ComponentModel.DefaultValue(typeof(TabControlDisplayManager), "Framework"),
        System.ComponentModel.Category("Appearance")
    ]
    public TabControlDisplayManager DisplayManager
    {
        get { return _DisplayManager; }
        set
        {
            _DisplayManager = value;
            if (this._DisplayManager.Equals(TabControlDisplayManager.Framework))
            {
                this.SetStyle(ControlStyles.UserPaint, true);
                this.ItemSize = new Size(0, 15);
                this.Padding = new Point(9, 0);
            }
            else
            {
                this.ItemSize = new Size(0, 0);
                this.Padding = new Point(6, 3);
                this.SetStyle(ControlStyles.UserPaint, false);
            }
        }
    }
    #endregion

    #region "    Constructor "
    public AsTabControl()
        : base()
    {
        if (this._DisplayManager.Equals(TabControlDisplayManager.Framework))
        {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.ItemSize = new Size(0, 15);
            this.Padding = new Point(9, 0);
        }
        this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        this.SetStyle(ControlStyles.DoubleBuffer, true);
        this.SetStyle(ControlStyles.ResizeRedraw, true);
        this.ResizeRedraw = true;
    }
    #endregion

    #region "    Private Methods "
    private System.Drawing.Drawing2D.GraphicsPath GetPath(int index)
    {
        System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
        path.Reset();
        Rectangle rect = this.GetTabRect(index);
        if (index == 0)
        {
            path.AddLine(rect.Left + 1, rect.Bottom + 1, rect.Left + rect.Height, rect.Top + 2);
            path.AddLine(rect.Left + rect.Height + 4, rect.Top, rect.Right - 3, rect.Top);
            path.AddLine(rect.Right - 1, rect.Top + 2, rect.Right - 1, rect.Bottom + 1);
        }
        else
        {
            if (index == this.SelectedIndex)
            {
                path.AddLine(
                    rect.Left + 5 - rect.Height,
                    rect.Bottom + 1,
                    rect.Left + 4,
                    rect.Top + 2
                );
                path.AddLine(rect.Left + 8, rect.Top, rect.Right - 3, rect.Top);
                path.AddLine(rect.Right - 1, rect.Top + 2, rect.Right - 1, rect.Bottom + 1);
                path.AddLine(
                    rect.Right - 1,
                    rect.Bottom + 1,
                    rect.Left + 5 - rect.Height,
                    rect.Bottom + 1
                );
            }
            else
            {
                path.AddLine(rect.Left, rect.Top + 6, rect.Left + 4, rect.Top + 2);
                path.AddLine(rect.Left + 8, rect.Top, rect.Right - 3, rect.Top);
                path.AddLine(rect.Right - 1, rect.Top + 2, rect.Right - 1, rect.Bottom + 1);
                path.AddLine(rect.Right - 1, rect.Bottom + 1, rect.Left, rect.Bottom + 1);
            }
        }
        return path;
    }

    private System.Drawing.Drawing2D.GraphicsPath GetActiveHeaderPath(int index)
    {
        System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
        path.Reset();
        Rectangle rect = this.GetTabRect(index);
        if (index == 0)
        {
            path.AddLine(rect.Left + rect.Height + 4, rect.Top + 1, rect.Right - 3, rect.Top + 1);
            path.AddLine(rect.Left + rect.Height + 3, rect.Top + 2, rect.Right - 1, rect.Top + 2);
        }
        else
        {
            path.AddLine(rect.Left + 8, rect.Top + 1, rect.Right - 3, rect.Top + 1);
            path.AddLine(rect.Left + 7, rect.Top + 2, rect.Right - 1, rect.Top + 2);
        }
        return path;
    }

    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
    {
        //   Paint the Background
        this.PaintTransparentBackground(e.Graphics, this.ClientRectangle);
        //   Paint all the tabs
        if (this.TabCount > 0)
        {
            for (int index = 0; index <= this.TabCount - 1; index++)
            {
                this.PaintTab(e, index);
            }
        }
        //   paint a border round the pagebox
        //   We can't make the border disappear so have to do it like this.
        if (this.TabCount > 0)
        {
            Rectangle borderRect = this.TabPages[0].Bounds;
            if (this.SelectedTab != null)
            {
                borderRect = this.SelectedTab.Bounds;
            }
            borderRect.Inflate(1, 1);
            ControlPaint.DrawBorder(
                e.Graphics,
                borderRect,
                ThemedColors.ToolBorder,
                ButtonBorderStyle.Solid
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
                Rectangle selrect = this.GetTabRect(this.SelectedIndex);
                int selrectRight = selrect.Right;
                e.Graphics.DrawLine(
                    new Pen(OrigamColorScheme.TabActiveEndColor),
                    selrect.Left + 2,
                    selrect.Bottom + 1,
                    selrectRight - 2,
                    selrect.Bottom + 1
                );
                break;
            }

            default:
            {
                Rectangle selrect = this.GetTabRect(this.SelectedIndex);
                int selrectRight = selrect.Right;
                e.Graphics.DrawLine(
                    new Pen(OrigamColorScheme.TabActiveEndColor),
                    selrect.Left + 6 - selrect.Height,
                    selrect.Bottom + 1,
                    selrectRight - 2,
                    selrect.Bottom + 1
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
                    this.Bounds,
                    SystemColors.ControlLightLight,
                    SystemColors.ControlLight,
                    LinearGradientMode.Vertical
                );
            pevent.Graphics.FillRectangle(backBrush, this.Bounds);
            backBrush.Dispose();
        }
        else
        {
            this.PaintTransparentBackground(pevent.Graphics, this.ClientRectangle);
        }
    }

    protected void PaintTransparentBackground(System.Drawing.Graphics g, Rectangle clipRect)
    {
        if ((this.Parent != null))
        {
            clipRect.Offset(this.Location);
            PaintEventArgs e = new PaintEventArgs(g, clipRect);
            GraphicsState state = g.Save();
            g.SmoothingMode = SmoothingMode.HighSpeed;
            try
            {
                g.TranslateTransform((float)-this.Location.X, (float)-this.Location.Y);
                this.InvokePaintBackground(this.Parent, e);
                this.InvokePaint(this.Parent, e);
            }
            finally
            {
                g.Restore(state);
                clipRect.Offset(-this.Location.X, -this.Location.Y);
            }
        }
        else
        {
            System.Drawing.Drawing2D.LinearGradientBrush backBrush =
                new System.Drawing.Drawing2D.LinearGradientBrush(
                    this.Bounds,
                    SystemColors.ControlLightLight,
                    SystemColors.ControlLight,
                    LinearGradientMode.Vertical
                );
            g.FillRectangle(backBrush, this.Bounds);
            backBrush.Dispose();
        }
    }

    private void PaintTab(System.Windows.Forms.PaintEventArgs e, int index)
    {
        System.Drawing.Drawing2D.GraphicsPath path = this.GetPath(index);
        this.PaintTabBackground(e.Graphics, index, path);
        this.PaintTabBorder(e.Graphics, index, path);
        this.PaintTabText(e.Graphics, index);
        this.PaintTabImage(e.Graphics, index);
    }

    private void PaintTabBackground(
        System.Drawing.Graphics graph,
        int index,
        System.Drawing.Drawing2D.GraphicsPath path
    )
    {
        Rectangle rect = this.GetTabRect(index);
        if (rect.Height > 1 && rect.Width > 1)
        {
            System.Drawing.Brush buttonBrush;
            if (index == this.SelectedIndex)
            {
                buttonBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect,
                    OrigamColorScheme.TabActiveStartColor,
                    OrigamColorScheme.TabActiveEndColor,
                    LinearGradientMode.Vertical
                );
            }
            else
            {
                buttonBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect,
                    OrigamColorScheme.TabInactiveStartColor,
                    OrigamColorScheme.TabInactiveEndColor,
                    LinearGradientMode.Vertical
                );
            }
            graph.FillPath(buttonBrush, path);
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
        Pen borderPen = new Pen(SystemColors.ControlDark);
        if (index == this.SelectedIndex)
        {
            borderPen = new Pen(ThemedColors.ToolBorder);
        }
        graph.DrawPath(borderPen, path);
        borderPen.Dispose();
    }

    private void PaintTabImage(System.Drawing.Graphics graph, int index)
    {
        Image tabImage = null;
        if (this.TabPages[index].ImageIndex > -1 && this.ImageList != null)
        {
            tabImage = this.ImageList.Images[this.TabPages[index].ImageIndex];
        }
        //			else if (this.TabPages[index].ImageKey.Trim().Length > 0 && this.ImageList != null)
        //			{
        //				tabImage = this.ImageList.Images(this.TabPages[index].ImageKey);
        //			}
        if (tabImage != null)
        {
            Rectangle rect = this.GetTabRect(index);
            graph.DrawImage(
                tabImage,
                rect.Right - rect.Height - 4,
                4,
                rect.Height - 2,
                rect.Height - 2
            );
        }
    }

    private void PaintTabText(System.Drawing.Graphics graph, int index)
    {
        Rectangle rect = this.GetTabRect(index);
        Rectangle rect2 = new Rectangle(rect.Left + 8, rect.Top + 1, rect.Width - 6, rect.Height);
        if (index == 0)
        {
            rect2 = new Rectangle(
                rect.Left + rect.Height,
                rect.Top + 1,
                rect.Width - rect.Height,
                rect.Height
            );
        }

        string tabtext = this.TabPages[index].Text;
        System.Drawing.StringFormat format = new System.Drawing.StringFormat();
        format.Alignment = StringAlignment.Near;
        format.LineAlignment = StringAlignment.Center;
        format.Trimming = StringTrimming.EllipsisCharacter;
        Brush forebrush = null;
        if (this.TabPages[index].Enabled == false)
        {
            forebrush = SystemBrushes.ControlDark;
        }
        else
        {
            forebrush = new SolidBrush(OrigamColorScheme.TabInactiveForeColor);
        }
        Font tabFont = this.Font;
        if (index == this.SelectedIndex)
        {
            tabFont = new Font(this.Font, FontStyle.Bold);
            if (index == 0)
            {
                rect2 = new Rectangle(
                    rect.Left + rect.Height,
                    rect.Top + 1,
                    rect.Width - rect.Height + 5,
                    rect.Height
                );
            }
            // p�id�l�no
            forebrush = new SolidBrush(OrigamColorScheme.TabActiveForeColor);
        }
        graph.DrawString(tabtext, tabFont, forebrush, rect2, format);
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
                this.SelectedTab.SelectNextControl(this.SelectedTab, true, true, true, true);
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
                this.SelectedTab.SelectNextControl(this.SelectedTab, true, true, true, true);
                return true;
            }
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnSelectedIndexChanged(EventArgs e)
    {
        try
        {
            base.OnSelectedIndexChanged(e);
        }
        catch { }
        Invalidate();
    }

    private Guid _styleId;

    [Browsable(false)]
    public Guid StyleId
    {
        get { return _styleId; }
        set { _styleId = value; }
    }

    [TypeConverter(typeof(StylesConverter))]
    public UIStyle Style
    {
        get
        {
            return (UIStyle)
                _persistence.SchemaProvider.RetrieveInstance(
                    typeof(UIStyle),
                    new ModelElementKey(this.StyleId)
                );
        }
        set { this.StyleId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]); }
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
            Color.FromArgb(127, 157, 185),
            Color.FromArgb(164, 185, 127),
            Color.FromArgb(165, 172, 178),
            Color.FromArgb(132, 130, 132),
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
