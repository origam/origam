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

using Origam.UI;
using Origam.Rule;
using Origam.Schema.EntityModel;

namespace Origam.Gui.Win;
/// <summary>
/// Summary description for DataGridDropdownColumn.
/// </summary>
public class DataGridBlobColumn : DataGridTextBoxColumn
{
	private BlobControl _blobControl;
	private bool _isEditing = false;
	private RuleEngine _ruleEngine;
	public DataGridBlobColumn(BlobControl blobControl, RuleEngine ruleEngine)
	{
		_ruleEngine = ruleEngine;
		_blobControl = new BlobControl();
		_blobControl.NoKeyUp = true;	// filter key-up events, so tabbing inside grid works correctly
		_blobControl.Hide();
		_blobControl.AuthorMember = blobControl.AuthorMember;
		_blobControl.BlobLookupId = blobControl.BlobLookupId;
		_blobControl.BlobMember = blobControl.BlobMember;
		_blobControl.CompressionStateMember = blobControl.CompressionStateMember;
		_blobControl.DateCreatedMember = blobControl.DateCreatedMember;
		_blobControl.DateLastModifiedMember = blobControl.DateLastModifiedMember;
		_blobControl.DefaultCompressionConstantId = blobControl.DefaultCompressionConstantId;
		_blobControl.DisplayStorageTypeSelection = blobControl.DisplayStorageTypeSelection;
		_blobControl.OriginalPathMember = blobControl.OriginalPathMember;
		_blobControl.RemarkMember = blobControl.RemarkMember;
		_blobControl.ThumbnailHeightConstantId = blobControl.ThumbnailHeightConstantId;
		_blobControl.ThumbnailMember = blobControl.ThumbnailMember;
		_blobControl.ThumbnailWidthConstantId = blobControl.ThumbnailWidthConstantId;
		_blobControl.FileSizeMember = blobControl.FileSizeMember;
		_blobControl.CaptionPosition = CaptionPosition.None;
		this.AlwaysReadOnly = blobControl.ReadOnly;
		_blobControl.TabStop = false;
		this.TextBox.VisibleChanged += new EventHandler(TextBox_VisibleChanged);
	}
	private bool _alwaysReadOnly = false;
	public bool AlwaysReadOnly
	{
		get
		{
			return _alwaysReadOnly;
		}
		set
		{
			_alwaysReadOnly = value;
			_blobControl.ReadOnly = value;
		}
	}
	public BlobControl BlobControl => _blobControl;
	#region Event Handlers
	private void _dropDown_LookupValueChangingByUser(object sender, EventArgs e)
	{
		_isEditing = true;
		this.ColumnStartedEditing(sender as Control);
	}
	#endregion
	#region Column Overrides
	protected override void SetColumnValueAtRow(CurrencyManager source, int rowNum, object value)
	{
		base.SetColumnValueAtRow (source, rowNum, value);
	}
	protected override void Abort(int rowNum)
	{
		_isEditing = false;
		_blobControl.ValueChangingByUser -= new EventHandler(_dropDown_LookupValueChangingByUser);
		if(_blobControl.IsDisposed) return;
		base.Abort(rowNum);
	}
	protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
	{
		//base.Edit(source, rowNum, bounds, readOnly, instantText , cellIsVisible);
		if(cellIsVisible)
		{
			if(! AlwaysReadOnly)
			{
				if(_ruleEngine != null)
				{
					_blobControl.ReadOnly = ! _ruleEngine.EvaluateRowLevelSecurityState((source.Current as DataRowView).Row, this.MappingName, Schema.EntityModel.CredentialType.Update);
				}
			}
			else
			{
				_blobControl.ReadOnly = true;
			}
			_blobControl.Location = bounds.Location;
			_blobControl.Size = new Size(bounds.Width, _blobControl.Height);
			_blobControl.FileName = base.GetColumnValueAtRow(source, rowNum).ToString();
			_blobControl.Visible = true;
			_blobControl.Enabled = true;
			_blobControl.BringToFront();
			_blobControl.Focus();
			if (_blobControl.Visible)
			{
				DataGridTableStyle.DataGrid.Invalidate(bounds);
			}
			_blobControl.ValueChangingByUser += new EventHandler(_dropDown_LookupValueChangingByUser);
		}
		else
		{
			_blobControl.Bounds = Rectangle.Empty;
//				_blobControl.Visible = false;
//				_blobControl.Enabled = false;
		}
	}
	protected override bool Commit(CurrencyManager dataSource, int rowNum)
	{
		if(_blobControl.Bounds.X != 0 | _blobControl.Bounds.Y != 0 | _blobControl.Width != 0) _blobControl.Bounds = Rectangle.Empty;
		_blobControl.ValueChangingByUser -= new EventHandler(_dropDown_LookupValueChangingByUser);
		if(_isEditing)
		{
			try 
			{
				SetColumnValueAtRow(dataSource, rowNum, _blobControl.FileName);
                // force form items to reread data values to prevent loss of data
                DataBindingTools.UpdateBindedFormComponent(
                    dataSource.Bindings, MappingName);
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
		//_dropDown.Hide();
		Invalidate();
		return true;
	}
	protected override void ReleaseHostedControl()
	{
		base.ReleaseHostedControl ();
		_blobControl.Parent = null;
	}
	protected override void ConcedeFocus()
	{
		_blobControl.Bounds = Rectangle.Empty;
		base.ConcedeFocus();
	}
	protected override void SetDataGridInColumn(DataGrid value) 
	{
		//base.SetDataGridInColumn(value);
		if (_blobControl.Parent != null) 
		{
			if(_blobControl.Parent.Equals(value)) return;
			_blobControl.Parent.Controls.Remove(_blobControl);
		}
		if (value != null) 
		{
			value.Controls.Add(_blobControl);
		}
	}
	#endregion
	protected override void Paint(System.Drawing.Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
	{
		Brush myBackBrush = backBrush;
		Brush myForeBrush = new SolidBrush(OrigamColorScheme.LinkColor);
		EntityFormatting formatting = DataGridColumnStyleHelper.Formatting(this, source, rowNum);
		if(formatting != null)
		{
			if(!formatting.UseDefaultBackColor) myBackBrush = new SolidBrush(formatting.BackColor);
			if(!formatting.UseDefaultForeColor) myForeBrush = new SolidBrush(formatting.ForeColor);
		}
		string text = this.GetColumnValueAtRow(source, rowNum).ToString();
		Font font = new Font(this.DataGridTableStyle.DataGrid.Font.FontFamily, this.DataGridTableStyle.DataGrid.Font.Size, FontStyle.Underline);
		Rectangle rectangle1 = bounds;
		StringFormat format1 = new StringFormat();
		if (alignToRight)
		{
			format1.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
		}
		format1.Alignment = (this.Alignment == HorizontalAlignment.Left) ? StringAlignment.Near : ((this.Alignment == HorizontalAlignment.Center) ? StringAlignment.Center : StringAlignment.Far);
		format1.FormatFlags |= StringFormatFlags.NoWrap;
		g.FillRectangle(myBackBrush, rectangle1);
		rectangle1.Offset(0, 2);
		rectangle1.Height -= 2;
		g.DrawString(text, font, myForeBrush, (RectangleF) rectangle1, format1);
		format1.Dispose();
	}
	protected override void Dispose(bool disposing)
	{
		if(disposing)
		{
			_ruleEngine = null;
		}
		base.Dispose (disposing);
	}
	private void TextBox_VisibleChanged(object sender, EventArgs e)
	{
		this.TextBox.Bounds = Rectangle.Empty;
	}
}
