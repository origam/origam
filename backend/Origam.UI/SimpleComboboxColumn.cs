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
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Origam.UI;

// Step 1. Derive a custom column style from DataGridTextBoxColumn
//	a) add a ComboBox member
//  b) track when the combobox has focus in Enter and Leave events
//  c) override Edit to allow the ComboBox to replace the TextBox
//  d) override Commit to save the changed data
public class DataGridComboBoxColumn : DataGridTextBoxColumn
{
    public NoKeyUpCombo ColumnComboBox;
    private bool _isEditing;

    public DataGridComboBoxColumn()
        : base()
    {
        _isEditing = false;

        ColumnComboBox = new NoKeyUpCombo();
        ColumnComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
    }

    private void ComboStartEditing(object sender, EventArgs e)
    {
        _isEditing = true;
        base.ColumnStartedEditing((Control)sender);
    }

    protected override void Edit(
        System.Windows.Forms.CurrencyManager source,
        int rowNum,
        System.Drawing.Rectangle bounds,
        bool readOnly,
        string instantText,
        bool cellIsVisible
    )
    {
        if (cellIsVisible)
        {
            this.ColumnComboBox.Enabled = true;
            ColumnComboBox.SelectedValueChanged += new EventHandler(ComboStartEditing);
            base.Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);
            ColumnComboBox.Parent = this.TextBox.Parent;
            ColumnComboBox.Location = this.TextBox.Location;
            ColumnComboBox.Size = new Size(this.TextBox.Size.Width, ColumnComboBox.Size.Height);
            ColumnComboBox.SelectedIndex = ColumnComboBox.FindStringExact(this.TextBox.Text);
            ColumnComboBox.Text = this.TextBox.Text;
            this.TextBox.Visible = false;
            ColumnComboBox.Visible = true;

            ColumnComboBox.BringToFront();
            ColumnComboBox.Focus();
        }
        else
        {
            this.ColumnComboBox.Visible = false;
            this.ColumnComboBox.Enabled = false;
        }
    }

    protected override bool Commit(System.Windows.Forms.CurrencyManager dataSource, int rowNum)
    {
        ColumnComboBox.SelectedValueChanged -= new EventHandler(ComboStartEditing);
        if (_isEditing)
        {
            try
            {
                SetColumnValueAtRow(dataSource, rowNum, ColumnComboBox.Text);
            }
            catch (Exception)
            {
                Abort(rowNum);
                return false;
            }
        }
        else
        {
            Abort(rowNum);
        }
        _isEditing = false;
        this.ColumnComboBox.Hide();
        Invalidate();
        return true;
    }

    protected override void ReleaseHostedControl()
    {
        base.ReleaseHostedControl();
        this.ColumnComboBox.Parent = null;
    }

    protected override void ConcedeFocus()
    {
        this.ColumnComboBox.Hide();
        base.ConcedeFocus();
    }

    protected override void Abort(int rowNum)
    {
        _isEditing = false;
        ColumnComboBox.SelectedValueChanged -= new EventHandler(ComboStartEditing);
        Invalidate();
        base.Abort(rowNum);
    }

    protected override object GetColumnValueAtRow(
        System.Windows.Forms.CurrencyManager source,
        int rowNum
    )
    {
        object s = base.GetColumnValueAtRow(source, rowNum);
        DataView dv = (DataView)this.ColumnComboBox.DataSource;
        int rowCount = dv.Count;
        int i = 0;
        //if things are slow, you could order your dataview
        //& use binary search instead of this linear one
        while (i < rowCount)
        {
            if (s.Equals(dv[i][this.ColumnComboBox.ValueMember]))
            {
                break;
            }

            ++i;
        }

        if (i < rowCount)
        {
            return dv[i][this.ColumnComboBox.DisplayMember];
        }

        return DBNull.Value;
    }

    protected override void SetColumnValueAtRow(
        System.Windows.Forms.CurrencyManager source,
        int rowNum,
        object value
    )
    {
        if (_isEditing)
        {
            object s = value;
            DataView dv = (DataView)this.ColumnComboBox.DataSource;
            int rowCount = dv.Count;
            int i = 0;
            //if things are slow, you could order your dataview
            //& use binary search instead of this linear one
            while (i < rowCount)
            {
                if (s.Equals(dv[i][this.ColumnComboBox.DisplayMember]))
                {
                    break;
                }

                ++i;
            }
            if (i < rowCount)
            {
                s = dv[i][this.ColumnComboBox.ValueMember];
            }
            else
            {
                s = DBNull.Value;
            }

            base.SetColumnValueAtRow(source, rowNum, s);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (this.ColumnComboBox != null)
            {
                ColumnComboBox.SelectedValueChanged -= new EventHandler(ComboStartEditing);
                this.ColumnComboBox.Dispose();
                this.ColumnComboBox = null;
            }
        }
        base.Dispose(disposing);
    }
}

public class NoKeyUpCombo : ComboBox
{
    private const int WM_KEYUP = 0x101;
    private const int WM_KEYDOWN = 0x100;

    protected override bool ProcessKeyMessage(ref Message m)
    {
        // ignore cursor keys and tab key
        if (m.Msg == WM_KEYDOWN)
        {
            if (m.WParam.ToInt32() == 37 | m.WParam.ToInt32() == 39)
            {
                return false;
            }
        }

        if (m.Msg == WM_KEYUP)
        {
            if (m.WParam.ToInt32() == 9 | m.WParam.ToInt32() == 38 | m.WParam.ToInt32() == 40)
            {
                return true;
            }

            if (m.WParam.ToInt32() == 37 | m.WParam.ToInt32() == 39)
            {
                return false;
            }
        }
        return base.ProcessKeyMessage(ref m);
    }
}
