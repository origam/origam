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
using Origam.DA;
using Origam.Rule;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for DataGridDropdownColumn.
/// </summary>
public class DataGridDropdownColumn : DataGridTextBoxColumn
{
    private AsDropDown _dropDown;
    private bool _isEditing = false;
    private RuleEngine _ruleEngine;
    private DataTable _handledTable;

    public DataGridDropdownColumn(AsDropDown dropDown, string columnName, RuleEngine ruleEngine)
    {
        _dropDown = new AsDropDown();
        _dropDown.NoKeyUp = true; // filter key-up events, so tabbing inside grid works correctly
        _dropDown.Hide();
        _dropDown.LookupId = dropDown.LookupId;
        _dropDown.CaptionPosition = CaptionPosition.None;
        _dropDown.OrigamMetadata = dropDown.OrigamMetadata;
        _dropDown.LookupShowEditButton = false;
        _dropDown.ShowUniqueValues = dropDown.ShowUniqueValues;
        this.AlwaysReadOnly = dropDown.ReadOnly;
        _dropDown.ColumnName = columnName;
        _dropDown.TabStop = false;
        _ruleEngine = ruleEngine;
        ServiceManager
            .Services.GetService<IControlsLookUpService>()
            .AddLookupControl(_dropDown, dropDown.FindForm(), true);
        this.TextBox.VisibleChanged += new EventHandler(TextBox_VisibleChanged);
    }

    private bool _alwaysReadOnly = false;
    public bool AlwaysReadOnly
    {
        get { return _alwaysReadOnly; }
        set
        {
            _alwaysReadOnly = value;
            _dropDown.ReadOnly = value;
        }
    }
    public AsDropDown DropDown
    {
        get { return _dropDown; }
    }

    #region Event Handlers
    private void _dropDown_LookupValueChangingByUser(object sender, EventArgs e)
    {
        _isEditing = true;
        this.ColumnStartedEditing(sender as Control);
    }

    private void Table_ColumnChanging(object sender, DataColumnChangeEventArgs e)
    {
        OrigamDataRow row = e.Row as OrigamDataRow;
        if (!row.IsColumnWithValidChange(e.Column))
            return;
        if (!row.Equals(this.DropDown.CurrentRow))
            return;
        if (this.DataGridTableStyle == null)
            return;
        DataGrid grid = this.DataGridTableStyle.DataGrid;
        foreach (ColumnParameterMapping mapping in _dropDown.ParameterMappings)
        {
            if (mapping.ColumnName == e.Column.ColumnName)
            {
                // we have to call base.GetColumn... because we need to get the original value in the table, not the looked up text
                object currentValue = base.GetColumnValueAtRow(
                    grid.BindingContext[grid.DataSource, grid.DataMember] as CurrencyManager,
                    grid.CurrentRowIndex
                );
                // column on which we are dependent has changed, so we clear
                if (currentValue != DBNull.Value) // if not cleared already
                {
                    SetColumnValueAtRow(
                        grid.BindingContext[grid.DataSource, grid.DataMember] as CurrencyManager,
                        grid.CurrentRowIndex,
                        DBNull.Value
                    );
                }
                return;
            }
        }
    }
    #endregion
    #region Column Overrides
    protected override void SetColumnValueAtRow(CurrencyManager source, int rowNum, object value)
    {
        base.SetColumnValueAtRow(source, rowNum, value);
    }

    protected override void Abort(int rowNum)
    {
        _isEditing = false;
        _dropDown.LookupValueChangingByUser -= new EventHandler(
            _dropDown_LookupValueChangingByUser
        );
        if (_dropDown.IsDisposed)
            return;
        base.Abort(rowNum);
    }

    protected override void Edit(
        CurrencyManager source,
        int rowNum,
        Rectangle bounds,
        bool readOnly,
        string instantText,
        bool cellIsVisible
    )
    {
        //base.Edit(source, rowNum, bounds, readOnly, instantText , cellIsVisible);
        if (cellIsVisible)
        {
            if (!AlwaysReadOnly)
            {
                if (_ruleEngine != null)
                {
                    _dropDown.ReadOnly = !_ruleEngine.EvaluateRowLevelSecurityState(
                        (source.Current as DataRowView).Row,
                        this.MappingName,
                        Schema.EntityModel.CredentialType.Update
                    );
                }
            }
            else
            {
                _dropDown.ReadOnly = true;
            }
            _dropDown.Location = bounds.Location;
            _dropDown.Size = new Size(bounds.Width, _dropDown.Height);
            _dropDown.LookupValue = base.GetColumnValueAtRow(source, rowNum);
            _dropDown.Visible = true;
            _dropDown.Enabled = true;
            _dropDown.BringToFront();
            _dropDown.Focus();
            if (_dropDown.Visible)
            {
                DataGridTableStyle.DataGrid.Invalidate(bounds);
            }
            _dropDown.LookupValueChangingByUser += new EventHandler(
                _dropDown_LookupValueChangingByUser
            );
        }
        else
        {
            _dropDown.Bounds = Rectangle.Empty;
            //				_dropDown.Visible = false;
            //				_dropDown.Enabled = false;
        }
    }

    protected override object GetColumnValueAtRow(CurrencyManager source, int rowNum)
    {
        object val = base.GetColumnValueAtRow(source, rowNum);
        IDataLookupService lookupManager =
            ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
        return lookupManager.GetDisplayText(_dropDown.LookupId, val, null);
    }

    protected override bool Commit(CurrencyManager dataSource, int rowNum)
    {
        if (_dropDown.Bounds.X != 0 | _dropDown.Bounds.Y != 0 | _dropDown.Width != 0)
            _dropDown.Bounds = Rectangle.Empty;
        _dropDown.LookupValueChangingByUser -= new EventHandler(
            _dropDown_LookupValueChangingByUser
        );
        if (_isEditing)
        {
            try
            {
                SetColumnValueAtRow(dataSource, rowNum, _dropDown.LookupValue);
                // force form items to reread data values to prevent loss of data
                DataBindingTools.UpdateBindedFormComponent(dataSource.Bindings, MappingName);
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
        base.ReleaseHostedControl();
        _dropDown.Parent = null;
    }

    protected override void ConcedeFocus()
    {
        _dropDown.Bounds = Rectangle.Empty;
        base.ConcedeFocus();
    }

    protected override void SetDataGridInColumn(DataGrid value)
    {
        //base.SetDataGridInColumn(value);
        if (_dropDown.Parent != null)
        {
            if (_dropDown.Parent.Equals(value))
                return;
            _dropDown.Parent.Controls.Remove(_dropDown);
        }
        if (value != null)
        {
            value.Controls.Add(_dropDown);
            if (value.BindingContext == null)
            {
                value.BindingContextChanged += new EventHandler(grid_BindingContextChanged);
            }
            else
            {
                CatchGridContext();
            }
            if (value.DataSource == null)
            {
                value.DataSourceChanged += new EventHandler(grid_DataSourceChanged);
            }
        }
    }

    private void grid_BindingContextChanged(object sender, EventArgs e)
    {
        CatchGridContext();
    }

    private void grid_DataSourceChanged(object sender, EventArgs e)
    {
        CatchGridContext();
    }

    private void CatchGridContext()
    {
        if (this.DataGridTableStyle == null)
            return;
        DataGrid grid = this.DataGridTableStyle.DataGrid;
        if (grid.DataSource == null | grid.DataMember == "" | grid.BindingContext == null)
            return;
        CurrencyManager cm =
            grid.BindingContext[grid.DataSource, grid.DataMember] as CurrencyManager;
        if (_handledTable != null)
        {
            _handledTable.ColumnChanging -= new DataColumnChangeEventHandler(Table_ColumnChanging);
        }
        _handledTable = (cm.List as DataView).Table;
        _handledTable.ColumnChanging += new DataColumnChangeEventHandler(Table_ColumnChanging);
    }
    #endregion
    protected override void Paint(
        System.Drawing.Graphics g,
        Rectangle bounds,
        CurrencyManager source,
        int rowNum,
        Brush backBrush,
        Brush foreBrush,
        bool alignToRight
    )
    {
        Brush myBackBrush = backBrush;
        Brush myForeBrush = (
            DropDown.LookupCanEditSourceRecord
                ? new SolidBrush(OrigamColorScheme.LinkColor)
                : foreBrush
        );
        EntityFormatting formatting = DataGridColumnStyleHelper.Formatting(this, source, rowNum);
        if (formatting != null)
        {
            if (!formatting.UseDefaultBackColor)
                myBackBrush = new SolidBrush(formatting.BackColor);
            if (!formatting.UseDefaultForeColor)
                myForeBrush = new SolidBrush(formatting.ForeColor);
        }
        if (DropDown.LookupCanEditSourceRecord)
        {
            string text = this.GetColumnValueAtRow(source, rowNum).ToString();
            Font font = new Font(
                this.DataGridTableStyle.DataGrid.Font.FontFamily,
                this.DataGridTableStyle.DataGrid.Font.Size,
                FontStyle.Underline
            );
            Rectangle rectangle1 = bounds;
            StringFormat format1 = new StringFormat();
            if (alignToRight)
            {
                format1.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
            }
            format1.Alignment =
                (this.Alignment == HorizontalAlignment.Left)
                    ? StringAlignment.Near
                    : (
                        (this.Alignment == HorizontalAlignment.Center)
                            ? StringAlignment.Center
                            : StringAlignment.Far
                    );
            format1.FormatFlags |= StringFormatFlags.NoWrap;
            g.FillRectangle(myBackBrush, rectangle1);
            rectangle1.Offset(0, 2);
            rectangle1.Height -= 2;
            g.DrawString(text, font, myForeBrush, (RectangleF)rectangle1, format1);
            format1.Dispose();
        }
        else
        {
            base.Paint(g, bounds, source, rowNum, myBackBrush, myForeBrush, alignToRight);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ServiceManager
                .Services.GetService<IControlsLookUpService>()
                .RemoveLookupControl(_dropDown);
            _ruleEngine = null;
            if (_handledTable != null)
            {
                _handledTable.ColumnChanging -= new DataColumnChangeEventHandler(
                    Table_ColumnChanging
                );
                _handledTable = null;
            }
        }
        base.Dispose(disposing);
    }

    private void TextBox_VisibleChanged(object sender, EventArgs e)
    {
        this.TextBox.Bounds = Rectangle.Empty;
    }
}
