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
using System.Data;
using Origam.Rule;
using Origam.Schema.EntityModel;

namespace Origam.Gui.Win;
public class AsTextBoxStyleColumn : DataGridTextBoxColumn
{
	public AsTextBox AsTextBox;
	private bool _isEditing;
	public AsTextBoxStyleColumn()
	{
		_isEditing = false;
        AsTextBox = new NoKeyUpTextBox();
		AsTextBox.CaptionPosition = CaptionPosition.None;
		TextBox.TabStop =false;
		this.AsTextBox.TabStop = false;
		this.TextBox.Visible = false;
		this.TextBox.ReadOnly = true;
		this.TextBox.VisibleChanged += TextBox_VisibleChanged;
		AsTextBox.KeyPress += AsTextBox_KeyPress;
		AsTextBox.KeyDown += AsTextBox_KeyDown;
	}
	private bool _alwaysReadOnly = false;
	public bool AlwaysReadOnly
	{
		get => _alwaysReadOnly;
		set
		{
			_alwaysReadOnly = value;
			this.AsTextBox.ReadOnly = value;
		}
	}
	
	
	protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
	{
		Brush myBackBrush = backBrush;
		Brush myForeBrush = foreBrush;
		EntityFormatting formatting = DataGridColumnStyleHelper.Formatting(this, source, rowNum);
		if(formatting != null)
		{
			if(!formatting.UseDefaultBackColor)
            {
                myBackBrush = new SolidBrush(formatting.BackColor);
            }

            if (!formatting.UseDefaultForeColor)
            {
                myForeBrush = new SolidBrush(formatting.ForeColor);
            }
        }
		try
		{
			string text = this.GetText(this.GetColumnValueAtRow(source, rowNum));
            RuleEngine ruleEngine = GetRuleEngine();
            if (ruleEngine != null)
            {
                if (IsReadDenied(((DataView)source.List)[rowNum].Row, ruleEngine))
                {
                    text = "";
                }
            }
			Rectangle rect = bounds;
			StringFormat format = new StringFormat();
			if (alignToRight)
			{
				format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			}
			format.Alignment = (this.Alignment == HorizontalAlignment.Left) ? StringAlignment.Near : ((this.Alignment == HorizontalAlignment.Center) ? StringAlignment.Center : StringAlignment.Far);
			//format.FormatFlags |= StringFormatFlags.NoWrap;
			g.FillRectangle(myBackBrush, rect);
			rect.Offset(0, 2 * 1);
			rect.Height -= 2 * 1;
			g.DrawString((text.Length > 100000 ? text.Substring(0, 100000) : text), this.DataGridTableStyle.DataGrid.Font, myForeBrush, rect, format);
			format.Dispose();
		}
		catch
		{
		}
	}
	
	private string GetText(object value)
	{
		if (value is DBNull)
		{
			return this.NullText;
		}
		if (((this.Format != null) && (this.Format.Length != 0)) && (value is IFormattable) && !(value is Guid))
		{
			try
			{
				return ((IFormattable) value).ToString(this.Format, this.FormatInfo);
			}
			catch (Exception)
			{
				goto Label_0084;
			}
		}
		if ((System.ComponentModel.TypeDescriptor.GetConverter(this.PropertyDescriptor.PropertyType) != null) && System.ComponentModel.TypeDescriptor.GetConverter(this.PropertyDescriptor.PropertyType).CanConvertTo(typeof(string)))
		{
			return (string) System.ComponentModel.TypeDescriptor.GetConverter(this.PropertyDescriptor.PropertyType).ConvertTo(value, typeof(string));
		}
		Label_0084:
			if (value == null)
			{
				return "";
			}
		return value.ToString();
	}
	
	protected override void ConcedeFocus()
	{
		AsTextBox.Bounds = Rectangle.Empty;
		base.ConcedeFocus();
	}
	protected override void Abort(int rowNum)
	{
		_isEditing = false;
//			AsTextBox.ModifiedChanged -= new EventHandler(AsTextBox_ModifiedChanged);
		Invalidate();
		if(this.AsTextBox.IsDisposed)
        {
            return;
        }

        base.Abort(rowNum);
	}
	protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
	{
		if(cellIsVisible)
		{
			if(! AlwaysReadOnly)
			{
                RuleEngine ruleEngine = GetRuleEngine();
				if(ruleEngine != null)
				{
                    if (IsReadDenied((source.Current as DataRowView).Row, ruleEngine))
                    {
                        this.AsTextBox.Bounds = Rectangle.Empty;
                        return;
                    }
                    AsTextBox.ReadOnly = !ruleEngine.EvaluateRowLevelSecurityState((source.Current as DataRowView).Row, this.MappingName, CredentialType.Update);
				}
			}
			else
			{
				AsTextBox.ReadOnly = true;
			}
			//WORKAROUND: bug in datagrid - if there is a calculated column, the textbox hangs in the grid after changing the row
			if(((DataView)source.List).Table.Columns[this.MappingName].ReadOnly) 
			{
				base.Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);
				return;
			}
			this.AsTextBox.Enabled = true;
			base.Edit(source,rowNum, bounds, readOnly, instantText , cellIsVisible);
			AsTextBox.Parent = this.TextBox.Parent;
			AsTextBox.Location = bounds.Location;
			
			int height = (bounds.Height < AsTextBox.PreferredHeight ? AsTextBox.PreferredHeight : bounds.Height);
			AsTextBox.Size = new Size(this.TextBox.Size.Width, height);
	
			//AsTextBox.Value = this.TextBox.Text;
			AsTextBox.Value = this.GetColumnValueAtRow(source, rowNum);
        			
			AsTextBox.Visible = true;
			AsTextBox.BringToFront();
			AsTextBox.Focus();	
//				AsTextBox.ModifiedChanged += new EventHandler(AsTextBox_ModifiedChanged);
		}
		else
		{
			this.AsTextBox.Bounds = Rectangle.Empty;
			this.AsTextBox.Visible = false;
//				this.AsTextBox.Enabled = false;
		}
		if (AsTextBox.Visible)
        {
            DataGridTableStyle.DataGrid.Invalidate(bounds);
        }
    }
    private RuleEngine GetRuleEngine()
    {
        RuleEngine ruleEngine = (this.DataGridTableStyle.DataGrid.FindForm() as AsForm).FormGenerator.FormRuleEngine;
        return ruleEngine;
    }
    private bool IsReadDenied(DataRow row, RuleEngine ruleEngine)
    {
        return !ruleEngine.EvaluateRowLevelSecurityState(row, this.MappingName, CredentialType.Read);
    }
	protected override bool Commit(CurrencyManager dataSource, int rowNum)
	{
        this.AsTextBox.Bounds = Rectangle.Empty;
        this.AsTextBox.Hide();
		if(_isEditing)
		{
			try 
			{
				object val = (AsTextBox.Text == "" ? DBNull.Value : AsTextBox.Value);
				SetColumnValueAtRow(dataSource, rowNum, val);
                // force form items to reread data values to prevent loss of data
                DataBindingTools.UpdateBindedFormComponent(
                    dataSource.Bindings, MappingName);
			} 
			catch
			{
				Abort(rowNum);
				return false;
			}
		}
		else
		{
			Abort(rowNum);
            return true;
		}
		_isEditing = false;
		Invalidate();
		return true;
	}
	protected override void ReleaseHostedControl()
	{
		base.ReleaseHostedControl ();
		this.AsTextBox.Parent = null;
	}
	protected override void Dispose(bool disposing)
	{
		if(disposing)
		{
			if(this.AsTextBox != null)
			{
				AsTextBox.ModifiedChanged -= AsTextBox_ModifiedChanged;
				AsTextBox.KeyPress -= AsTextBox_KeyPress;
				
				this.AsTextBox.Dispose();
				this.AsTextBox = null;
			}
		}
		base.Dispose (disposing);
	}
	private void AsTextBox_ModifiedChanged(object sender, EventArgs e)
	{
		if(this.AsTextBox.ReadOnly)
        {
            return;
        }

        _isEditing = true;
		try
		{
			ColumnStartedEditing((Control) sender);
		}
		catch 
		{
			_isEditing = false;
			this.AsTextBox.Bounds = Rectangle.Empty;
		}
	}
	private void TextBox_VisibleChanged(object sender, EventArgs e)
	{
		this.TextBox.Visible = false;
	}
	private void AsTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if(e.KeyChar == (char)Keys.Escape)
        {
            return;
        }

        bool canEdit = false;
		if((Control.ModifierKeys & Keys.Control) == Keys.Control & (e.KeyChar == 'V' | e.KeyChar == 'v' | e.KeyChar == 'X' | e.KeyChar == 'x' | e.KeyChar == 22 | e.KeyChar == 24))
		{
			canEdit = true;
		}
		if(!canEdit)
		{
			canEdit = (((e.KeyChar != ' ') || ((Control.ModifierKeys & Keys.Shift) != Keys.Shift))) && (((Control.ModifierKeys & Keys.Control) != Keys.Control) || ((Control.ModifierKeys & Keys.Alt) != Keys.None));
		}
		if (canEdit)
		{
			if(this.AsTextBox.ReadOnly)
            {
                return;
            }

            _isEditing = true;
			try
			{
				ColumnStartedEditing((Control) sender);
			}
			catch 
			{
				_isEditing = false;
				this.AsTextBox.Bounds = Rectangle.Empty;
			}
		}
	}
	private void AsTextBox_KeyDown(object sender, KeyEventArgs e)
	{
		if(e.KeyCode == Keys.Delete | e.KeyCode == Keys.Back)
		{
			if(this.AsTextBox.ReadOnly)
            {
                return;
            }

            _isEditing = true;
			try
			{
				ColumnStartedEditing((Control) sender);
			}
			catch 
			{
				_isEditing = false;
				this.AsTextBox.Bounds = Rectangle.Empty;
			}
		}
	}
}
internal class NoKeyUpTextBox : AsTextBox 
{
	private const int WM_KEYUP = 0x101;
	private const int WM_KEYDOWN = 0x100;
	protected override bool ProcessKeyMessage(ref Message m)
	{
		// ignore cursor keys and tab key
		if(m.Msg == WM_KEYDOWN)
		{
			if(
				m.WParam.ToInt32() == 37
				| m.WParam.ToInt32() == 39
				)
			{
				return false;
			}
		}
		
		if(m.Msg == WM_KEYUP)
		{
			if(
				m.WParam.ToInt32() == 9
				| m.WParam.ToInt32() == 38
				| m.WParam.ToInt32() == 40
				)
			{
				return true;
			}
			if(
				m.WParam.ToInt32() == 37
				| m.WParam.ToInt32() == 39
				)
			{
				return false;
			}
		}
		return base.ProcessKeyMessage (ref m);
	}
}
