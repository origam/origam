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

using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for DataGridImageColumn.
/// </summary>
public class DataGridImageColumn : DataGridColumnStyle
{
    private int _width = 0;
    private int _height = 0;

    public DataGridImageColumn(int width, int height)
        : base()
    {
        _width = width;
        _height = height;
    }

    protected override int GetPreferredHeight(System.Drawing.Graphics g, object value)
    {
        return _height;
    }

    protected override int GetMinimumHeight()
    {
        return _height;
    }

    protected override Size GetPreferredSize(Graphics g, object value)
    {
        return new Size(width: _width, height: _height);
    }

    protected override void Paint(
        Graphics g,
        Rectangle bounds,
        CurrencyManager source,
        int rowNum,
        bool alignToRight
    )
    {
        this.Paint(g: g, bounds: bounds, source: source, rowNum: rowNum);
    }

    protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum)
    {
        object imageData = this.GetColumnValueAtRow(source: source, rowNum: rowNum);
        try
        {
            if (imageData is byte[])
            {
                byte[] byteArray = (byte[])imageData;
                MemoryStream ms = new MemoryStream(buffer: byteArray);
                using (Image i = Image.FromStream(stream: ms))
                {
                    using (Bitmap bm = new Bitmap(width: bounds.Width, height: bounds.Height))
                    {
                        using (Graphics bmgr = Graphics.FromImage(image: bm))
                        {
                            bmgr.Clear(color: Color.White);
                            bmgr.DrawImage(image: i, x: 0, y: 0, width: i.Width, height: i.Height);
                        }
                        g.DrawImageUnscaled(image: bm, rect: bounds);
                    }
                }
                ms.Close();
            }
            else
            {
                g.FillRectangle(
                    brush: new SolidBrush(color: this.DataGridTableStyle.BackColor),
                    rect: bounds
                );
            }
        }
        catch
        {
            g.FillRectangle(
                brush: new SolidBrush(color: this.DataGridTableStyle.BackColor),
                rect: bounds
            );
        }
    }

    protected override void Abort(int rowNum) { }

    protected override bool Commit(CurrencyManager dataSource, int rowNum)
    {
        return true;
    }

    protected override void Edit(
        CurrencyManager source,
        int rowNum,
        Rectangle bounds,
        bool readOnly,
        string instantText,
        bool cellIsVisible
    ) { }
}
