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
using System.Collections.Generic;
using System.Windows.Forms;
using Origam.UI;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for FilterPart.
/// </summary>
public abstract class FilterPart : IDisposable
{
    #region Events
    public event DataViewQueryChanged QueryChanged;
    public event EventHandler ControlsChanged;
    #endregion
    #region Private Fields

    private FormGenerator _formGenerator;
    private Control _filteredControl;
    private Type _dataType;
    private string _dataMember;
    private string _gridColumnName;
    private object _value1;
    private object _value2;
    private bool _loadingValues = false;
    private FilterOperator _operator;
    private string _query = null;
    private Label _label = new Label();
    private Label _operatorLabelControl = new Label();
    private ContextMenu _operatorContextMenu = new ContextMenu();

    #endregion
    #region Constructor
    public FilterPart(
        Control filteredControl,
        Type dataType,
        string dataMember,
        string gridColumnName,
        string label,
        FormGenerator formGenerator
    )
    {
        this.OperatorLabelControl.Font = new System.Drawing.Font(
            prototype: _operatorLabelControl.Font,
            newStyle: System.Drawing.FontStyle.Italic
        );
        this.OperatorLabelControl.MouseEnter += new EventHandler(OperatorLabelControl_MouseEnter);
        this.OperatorLabelControl.MouseLeave += new EventHandler(OperatorLabelControl_MouseLeave);
        this.OperatorLabelControl.Click += new EventHandler(OperatorLabelControl_Click);
        _formGenerator = formGenerator;
        _filteredControl = filteredControl;
        _dataType = dataType;
        _dataMember = dataMember;
        _gridColumnName = gridColumnName;
        _label.Text = label;
        this.Operator = this.DefaultOperator;

        foreach (FilterOperator op in this.AllowedOperators)
        {
            FilterOperatorMenuItem menu = new FilterOperatorMenuItem(
                text: OperatorLabelText(oper: op),
                eventHandler: new EventHandler(operatorMenu_Click),
                oper: op
            );
            _operatorContextMenu.MenuItems.Add(item: menu);
        }
        CreateFilterControls();
    }
    #endregion
    #region Properties
    public FormGenerator FormGenerator
    {
        get { return _formGenerator; }
    }
    public Control FilteredControl
    {
        get { return _filteredControl; }
    }
    public Type DataType
    {
        get { return _dataType; }
    }
    public string DataMember
    {
        get { return _dataMember; }
    }
    public string GridColumnName
    {
        get { return _gridColumnName; }
    }
    public abstract FilterOperator[] AllowedOperators { get; }
    public abstract FilterOperator DefaultOperator { get; }
    public Label LabelControl
    {
        get { return _label; }
    }
    public Label OperatorLabelControl
    {
        get { return _operatorLabelControl; }
    }
    public List<Control> FilterControls { get; } = new();

    public object Value1
    {
        get { return _value1; }
        set
        {
            _value1 = value;
            if (!_loadingValues)
            {
                RefreshQuery();
            }
        }
    }
    public object Value2
    {
        get { return _value2; }
        set
        {
            _value2 = value;
            if (!_loadingValues)
            {
                RefreshQuery();
            }
        }
    }
    public FilterOperator Operator
    {
        get { return _operator; }
        set
        {
            _operator = value;
            _operatorLabelControl.Text = OperatorLabelText(oper: _operator);
            this.CreateFilterControls();
            RefreshQuery();
        }
    }
    public string Query
    {
        get { return _query; }
    }
    #endregion
    #region public Methods
    public abstract void CreateFilterControls();
    public abstract void LoadValues();

    public void ApplyStoredFilter(OrigamPanelFilter.PanelFilterDetailRow filterRow)
    {
        string columnName1 = StoredFilterColumn1();
        string columnName2 = StoredFilterColumn2();
        if (filterRow.IsNull(columnName: columnName1))
        {
            _value1 = DBNull.Value;
        }
        else
        {
            _value1 = filterRow[columnName: columnName1];
        }
        if (filterRow.IsNull(columnName: columnName2))
        {
            _value2 = DBNull.Value;
        }
        else
        {
            _value2 = filterRow[columnName: columnName2];
        }
        _loadingValues = true;
        try
        {
            this.LoadValues();
        }
        finally
        {
            _loadingValues = false;
        }
        RefreshQuery();
    }

    public void OnQueryChanged(string query)
    {
        if (query == null)
        {
            _label.Font = new System.Drawing.Font(
                prototype: _label.Font,
                newStyle: System.Drawing.FontStyle.Regular
            );
        }
        else
        {
            _label.Font = new System.Drawing.Font(
                prototype: _label.Font,
                newStyle: System.Drawing.FontStyle.Bold
            );
        }
        if (QueryChanged != null)
        {
            this.QueryChanged(sender: this, query: query);
        }
    }

    public void OnControlsChanged()
    {
        if (ControlsChanged != null)
        {
            this.ControlsChanged(sender: this, e: EventArgs.Empty);
        }
    }
    #endregion
    #region Private Methods
    private string StoredFilterColumn1()
    {
        return StoredFilterColumn(type: this.DataType, position: 1);
    }

    private string StoredFilterColumn2()
    {
        return StoredFilterColumn(type: this.DataType, position: 2);
    }

    private static string StoredFilterColumn(Type type, int position)
    {
        if (type == typeof(string))
        {
            return "StringValue" + position.ToString();
        }

        if (type == typeof(int))
        {
            return "IntValue" + position.ToString();
        }

        if (type == typeof(Guid))
        {
            return "GuidValue" + position.ToString();
        }

        if (type == typeof(DateTime))
        {
            return "DateValue" + position.ToString();
        }

        throw new ArgumentOutOfRangeException(
            paramName: "type",
            actualValue: type,
            message: "Unrecognized type. Cannot read stored filter."
        );
    }

    private void RefreshQuery()
    {
        if (this.Operator == FilterOperator.None)
        {
            return;
        }

        string field = "[" + this.DataMember + "]";
        string v1 = QueryValue(value: this.Value1, oper: this.Operator, dataType: this.DataType);
        if (this.Operator == FilterOperator.IsNull | this.Operator == FilterOperator.NotIsNull)
        {
            string op = QueryOperator(oper: this.Operator);
            _query = field + " " + op;
            if (this.DataType == typeof(string))
            {
                if (this.Operator == FilterOperator.IsNull)
                {
                    string op2 = QueryOperator(oper: FilterOperator.Equals);
                    _query += " OR " + field + " " + op2 + " ''";
                }
                else
                {
                    string op2 = QueryOperator(oper: FilterOperator.NotEquals);
                    _query += " AND " + field + " " + op2 + " ''";
                }
            }
        }
        else if (v1 == null)
        {
            _query = null;
        }
        else if (this.Operator == FilterOperator.Between)
        {
            string v2 = QueryValue(
                value: this.Value2,
                oper: this.Operator,
                dataType: this.DataType
            );
            if (v2 == null)
            {
                _query = null;
            }
            else
            {
                _query =
                    field
                    + " "
                    + QueryOperator(oper: FilterOperator.GreaterOrEqualThan)
                    + " "
                    + v1
                    + " AND "
                    + field
                    + " "
                    + QueryOperator(oper: FilterOperator.LessOrEqualThan)
                    + " "
                    + v2;
            }
        }
        else if (this.Operator == FilterOperator.NotBetween)
        {
            string v2 = QueryValue(
                value: this.Value2,
                oper: this.Operator,
                dataType: this.DataType
            );
            if (v2 == null)
            {
                _query = null;
            }
            else
            {
                _query =
                    field
                    + " "
                    + QueryOperator(oper: FilterOperator.LessThan)
                    + " "
                    + v1
                    + " OR "
                    + field
                    + " "
                    + QueryOperator(oper: FilterOperator.GreaterThan)
                    + " "
                    + v2;
            }
        }
        else
        {
            _query = field + " " + QueryOperator(oper: this.Operator) + " " + v1;
        }
        OnQueryChanged(query: _query);
    }

    private static string QueryValue(object value, FilterOperator oper, Type dataType)
    {
        string result;

        if (value == null | value == DBNull.Value)
        {
            return null;
        }

        if (value is DateTime)
        {
            result = Origam.DA.DatasetTools.DateExpression(dateValue: value);
        }
        else if (value is int | value is float | value is decimal | value is long)
        {
            result = Origam.DA.DatasetTools.NumberExpression(numberValue: value);
        }
        else
        {
            result = value.ToString();
        }
        if (oper == FilterOperator.BeginsWith | oper == FilterOperator.NotBeginsWith)
        {
            result = result + "*";
        }
        if (oper == FilterOperator.EndsWith | oper == FilterOperator.NotEndsWith)
        {
            result = "*" + result;
        }
        if (oper == FilterOperator.Contains | oper == FilterOperator.NotContains)
        {
            result = "*" + result + "*";
        }
        if (dataType == typeof(string))
        {
            result = Origam.DA.DatasetTools.TextExpression(text: result);
        }
        else if (dataType == typeof(Guid))
        {
            result = "'" + result + "'";
        }
        return result;
    }

    private static string QueryOperator(FilterOperator oper)
    {
        switch (oper)
        {
            case FilterOperator.BeginsWith:
            case FilterOperator.Contains:
            case FilterOperator.EndsWith:
            {
                return "LIKE";
            }
            case FilterOperator.Equals:
            {
                return "=";
            }
            case FilterOperator.Between:
            {
                return "BETWEEN";
            }
            case FilterOperator.GreaterOrEqualThan:
            {
                return ">=";
            }
            case FilterOperator.GreaterThan:
            {
                return ">";
            }
            case FilterOperator.LessOrEqualThan:
            {
                return "<=";
            }
            case FilterOperator.LessThan:
            {
                return "<";
            }
            case FilterOperator.NotBeginsWith:
            case FilterOperator.NotContains:
            case FilterOperator.NotEndsWith:
            {
                return "NOT LIKE";
            }
            case FilterOperator.NotBetween:
            {
                return "NOT BETWEEN";
            }
            case FilterOperator.NotEquals:
            {
                return "<>";
            }
            case FilterOperator.IsNull:
            {
                return "IS NULL";
            }
            case FilterOperator.NotIsNull:
            {
                return "IS NOT NULL";
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "operator",
                    actualValue: oper,
                    message: ResourceUtils.GetString(key: "ErrorUnknownFilterOperator")
                );
            }
        }
    }

    private static string OperatorLabelText(FilterOperator oper)
    {
        switch (oper)
        {
            case FilterOperator.None:
            {
                return "";
            }
            case FilterOperator.BeginsWith:
            {
                return "zaèíná na";
            }
            case FilterOperator.Contains:
            {
                return "obsahuje";
            }
            case FilterOperator.EndsWith:
            {
                return "konèí na";
            }
            case FilterOperator.Equals:
            {
                return "=";
            }
            case FilterOperator.Between:
            {
                return "je mezi";
            }
            case FilterOperator.GreaterOrEqualThan:
            {
                return ">=";
            }
            case FilterOperator.GreaterThan:
            {
                return ">";
            }
            case FilterOperator.LessOrEqualThan:
            {
                return "<=";
            }
            case FilterOperator.LessThan:
            {
                return "<";
            }
            case FilterOperator.NotBeginsWith:
            {
                return "nezaèíná na";
            }
            case FilterOperator.NotContains:
            {
                return "neobsahuje";
            }
            case FilterOperator.NotEndsWith:
            {
                return "nekonèí na";
            }
            case FilterOperator.NotBetween:
            {
                return "není mezi";
            }
            case FilterOperator.NotEquals:
            {
                return "není rovno";
            }
            case FilterOperator.IsNull:
            {
                return "je prázdné";
            }
            case FilterOperator.NotIsNull:
            {
                return "není prázdné";
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "oper",
                    actualValue: oper,
                    message: ResourceUtils.GetString(key: "ErrorUnknownFilterOperatorLabel")
                );
            }
        }
    }
    #endregion
    #region Event Handlers
    private void operatorMenu_Click(object sender, EventArgs e)
    {
        FilterOperatorMenuItem m = (FilterOperatorMenuItem)sender;
        this.Operator = m.Operator;
    }

    private void OperatorLabelControl_MouseEnter(object sender, EventArgs e)
    {
        (sender as Label).BackColor = OrigamColorScheme.FilterOperatorActiveBackColor;
        (sender as Label).ForeColor = OrigamColorScheme.FilterOperatorActiveForeColor;
    }

    private void OperatorLabelControl_MouseLeave(object sender, EventArgs e)
    {
        (sender as Label).BackColor = System.Drawing.Color.Transparent;
        (sender as Label).ForeColor = System.Drawing.SystemColors.ControlText;
    }
    #endregion
    #region IDisposable Members
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(obj: this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (this)
            {
                this.OperatorLabelControl.MouseEnter -= new EventHandler(
                    OperatorLabelControl_MouseEnter
                );
                this.OperatorLabelControl.MouseLeave -= new EventHandler(
                    OperatorLabelControl_MouseLeave
                );
                this.OperatorLabelControl.Click -= new EventHandler(OperatorLabelControl_Click);
                if (_operatorContextMenu != null)
                {
                    foreach (MenuItem item in _operatorContextMenu.MenuItems)
                    {
                        item.Click -= new EventHandler(operatorMenu_Click);
                    }
                    _operatorContextMenu.Dispose();
                    _operatorContextMenu = null;
                }
                if (_label != null)
                {
                    ReleaseControl(control: _label);
                    _label = null;
                }
                if (_operatorLabelControl != null)
                {
                    ReleaseControl(control: _operatorLabelControl);
                    _operatorLabelControl = null;
                }
                foreach (Control c in this.FilterControls)
                {
                    ReleaseControl(control: c);
                }
                this.FilterControls.Clear();
                _filteredControl = null;
                _formGenerator = null;
            }
        }
    }

    private void ReleaseControl(Control control)
    {
        if (control != null)
        {
            if (control.Parent != null)
            {
                control.Parent.Controls.Remove(value: control);
                control.Parent = null;
            }
            control.Dispose();
        }
    }
    #endregion
    private void OperatorLabelControl_Click(object sender, EventArgs e)
    {
        _operatorContextMenu.Show(
            control: this.OperatorLabelControl,
            pos: new System.Drawing.Point(x: 0, y: this.OperatorLabelControl.Height)
        );
    }
}
