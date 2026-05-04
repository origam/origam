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
using System.Data;
using System.Windows.Forms;
using Origam.Rule;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for DataGridBuilder.
/// </summary>
public class DataGridBuilder : IGridBuilder
{
    private AsDataGrid grid;
    private Guid _panelId;
    private IDocumentationService _documentationService;
    private bool _useUserConfig = true;
    private AsForm _form;
    private Control parentControl;
    private List<DataGridColumnStyleHolder> _styles;

    public DataGridBuilder()
    {
        _documentationService =
            ServiceManager.Services.GetService(serviceType: typeof(IDocumentationService))
            as IDocumentationService;
    }

    #region IGridFactory Members
    public AsDataGrid CreateGrid(
        object dataSource,
        string dataMember,
        Control control,
        Guid panelId,
        RuleEngine ruleEngine
    )
    {
        parentControl = control;
        _form = control.FindForm() as AsForm;
        grid = InitGrid();
        _panelId = panelId;
        control.Controls.Add(value: grid);
        grid.TableStyles.Clear();
        grid.TableStyles.Add(
            table: SetUpGridStyle(
                grid: grid,
                control: control,
                ds: dataSource as DataSet,
                member: dataMember,
                ruleEngine: ruleEngine
            )
        );
        grid.TableStyles[index: 0].RowHeaderWidthChanged += DataGridBuilder_RowHeaderWidthChanged;
        grid.Scroll += grid_Scroll;
        return grid;
    }

    public void UpdateDataSource(Control grid, object dataSource, string dataMember)
    {
        if (!(grid is AsDataGrid asGrid))
        {
            throw new NullReferenceException(message: ResourceUtils.GetString(key: "ErrorNoGrid"));
        }
        if (asGrid.DataSource != dataSource | asGrid.DataMember != dataMember)
        {
            {
                if (dataSource == null & asGrid.CurrencyManager != null)
                {
                    try
                    {
                        if (asGrid.CurrencyManager.Position > -1)
                        {
                            asGrid.CurrencyManager.CancelCurrentEdit();
                        }
                    }
                    catch { }
                }
                try
                {
                    asGrid.SuspendLayout();
                    asGrid.SetDataBinding(dataSource: dataSource, dataMember: dataMember);
                    int columnNumber = asGrid.CurrentCell.ColumnNumber;
                    int rowNumber = asGrid.CurrentCell.RowNumber;
                    if (dataSource != null && !this._form.FormGenerator.IgnoreDataChanges)
                    {
                        try
                        {
                            object layout = Reflector.GetValue(
                                type: typeof(DataGrid),
                                instance: asGrid,
                                memberName: "layout"
                            );
                            if (layout != null)
                            {
                                bool dirty = (bool)
                                    Reflector.GetValue(
                                        type: layout.GetType(),
                                        instance: layout,
                                        memberName: "dirty"
                                    );
                                if (!dirty)
                                {
                                    asGrid.CurrentCell = new DataGridCell(r: 0, c: columnNumber);
                                    asGrid.CurrentCell = new DataGridCell(
                                        r: rowNumber,
                                        c: columnNumber
                                    );
                                }
                            }
                        }
                        catch { }
                    }
                    if (dataSource != null)
                    {
                        asGrid.HorizontalScrollPosition = FilterFactory.ScrollOffset;
                    }
                }
                finally
                {
                    asGrid.ResumeLayout(performLayout: false);
                }
            }
        }
    }

    public DataGridFilterFactory FilterFactory { get; set; }
    public object Grid => grid;
    #endregion
    #region Public Methods
    public string GetSortColumn(string columnName)
    {
        DataTable table = (grid.DataSource as DataSet).Tables[
            name: FormTools.FindTableByDataMember(
                ds: grid.DataSource as DataSet,
                member: grid.DataMember
            )
        ];
        return (parentControl as AsPanel).GetSortColumn(
            originalColumnName: columnName,
            table: table
        );
    }
    #endregion
    #region Private Methods
    private AsDataGrid InitGrid()
    {
        AsDataGrid grid = new AsDataGrid();
        ((System.ComponentModel.ISupportInitialize)(grid)).BeginInit();
        grid.AllowSorting = false;
        grid.AllowNavigation = false;
        grid.DataMember = "";
        grid.FlatMode = true;
        grid.Size = new System.Drawing.Size(width: 176, height: 72);
        grid.Location = new System.Drawing.Point(x: -1000, y: -1000);
        grid.Name = "grid";
        grid.ReadOnly = true;
        grid.TabIndex = 0;
        grid.TabStop = false;
        grid.CaptionVisible = false;
        grid.DataSource = null;
        grid.AlternatingBackColor = OrigamColorScheme.GridAlternatingBackColor;
        grid.BackgroundColor = OrigamColorScheme.FormBackgroundColor;
        grid.BorderStyle = BorderStyle.None;
        grid.Font = new System.Drawing.Font(familyName: "Microsoft Sans Serif", emSize: 8F);
        grid.ForeColor = OrigamColorScheme.GridForeColor;
        grid.GridLineColor = OrigamColorScheme.GridLineColor;
        grid.GridLineStyle = DataGridLineStyle.Solid;
        grid.HeaderBackColor = OrigamColorScheme.GridHeaderBackColor;
        grid.HeaderFont = new System.Drawing.Font(familyName: "Microsoft Sans Serif", emSize: 8F);
        grid.HeaderForeColor = OrigamColorScheme.GridHeaderForeColor;
        grid.SelectionBackColor = OrigamColorScheme.GridSelectionBackColor;
        grid.SelectionForeColor = OrigamColorScheme.GridSelectionForeColor;
        grid.SizeChanged += grid_SizeChanged;
        grid.MouseDown += grid_MouseDown;
        grid.MouseUp += grid_MouseUp;
        grid.AfterMouseMove += grid_MouseMove;
        grid.KeyPress += grid_KeyPress;
        ((System.ComponentModel.ISupportInitialize)(grid)).EndInit();
        return grid;
    }

    private DataGridTableStyle SetUpGridStyle(
        DataGrid grid,
        Control control,
        DataSet ds,
        string member,
        RuleEngine ruleEngine
    )
    {
        //cretate new style
        DataGridTableStyle tableStyle = new DataGridTableStyle();
        tableStyle.AllowSorting = false;
        //all necessary style attrib. are copied into new one
        CopyDefaultTableStyle(datagrid: grid, ts: tableStyle);
        UserProfile profile = SecurityManager.CurrentUserProfile();
        OrigamPanelColumnConfig userConfig = null;

        if (_useUserConfig)
        {
            userConfig = OrigamPanelColumnConfigDA.LoadUserConfig(
                panelId: _panelId,
                profileId: profile.Id
            );
        }
        //Go through all controls and find any bound columns
        _styles = GetColumnStylesFromControls(
            control: control,
            offset: 0,
            userConfig: userConfig,
            ruleEngine: ruleEngine
        );

        //mapping name to grid
        tableStyle.MappingName = FormTools.FindTableByDataMember(ds: ds, member: member);
        UpdateColumns(grid: grid, tableStyle: tableStyle, firstTime: true, ds: ds);
        return tableStyle;
    }

    private void UpdateColumns(
        DataGrid grid,
        DataGridTableStyle tableStyle,
        bool firstTime,
        DataSet ds
    )
    {
        tableStyle.GridColumnStyles.Clear();
        bool allHidden = true;
        foreach (DataGridColumnStyleHolder style in _styles)
        {
            if (!style.Hidden)
            {
                allHidden = false;
            }

            if (style.Style.Width == 0)
            {
                style.Style.Width = 100;
            }
        }
        _styles.Sort();
        if (allHidden & _styles.Count > 0)
        {
            _styles[index: 0].Hidden = false;
        }
        foreach (DataGridColumnStyleHolder style in _styles)
        {
            if (firstTime)
            {
                if (!tableStyle.GridColumnStyles.Contains(name: style.Style.MappingName))
                {
                    tableStyle.GridColumnStyles.Add(column: style.Style);
                    style.Style.WidthChanged += Style_WidthChanged;
                    Guid columnId = (Guid)
                        ds.Tables[name: tableStyle.MappingName]
                            .Columns[name: style.Style.MappingName]
                            .ExtendedProperties[key: "Id"];
                    string tipText = _documentationService.GetDocumentation(
                        schemaItemId: columnId,
                        docType: DocumentationType.USER_LONG_HELP
                    );
                    _form.FormGenerator.SetTooltip(style: style.Style, tipText: tipText);
                    if (style.Hidden)
                    {
                        tableStyle.GridColumnStyles.Remove(column: style.Style);
                    }
                }
            }
            else
            {
                if (!style.Hidden)
                {
                    if (!tableStyle.GridColumnStyles.Contains(name: style.Style.MappingName))
                    {
                        tableStyle.GridColumnStyles.Add(column: style.Style);
                    }
                }

                // not for the first time, so the config has changed -> we must persist the changes
                StoreColumnConfig(
                    columnStyle: style.Style,
                    position: style.Index,
                    hidden: style.Hidden
                );
            }
        }
    }

    private void CopyDefaultTableStyle(DataGrid datagrid, DataGridTableStyle ts)
    {
        //ts.AllowSorting = datagrid.AllowSorting;
        ts.AlternatingBackColor = datagrid.AlternatingBackColor;
        ts.BackColor = datagrid.BackColor;
        ts.ColumnHeadersVisible = datagrid.ColumnHeadersVisible;
        ts.ForeColor = datagrid.ForeColor;
        ts.GridLineColor = datagrid.GridLineColor;
        ts.GridLineStyle = datagrid.GridLineStyle;
        ts.HeaderBackColor = datagrid.HeaderBackColor;
        ts.HeaderFont = datagrid.HeaderFont;
        ts.HeaderForeColor = datagrid.HeaderForeColor;
        ts.LinkColor = datagrid.LinkColor;
        ts.PreferredColumnWidth = datagrid.PreferredColumnWidth;
        ts.PreferredRowHeight = datagrid.PreferredRowHeight;
        ts.ReadOnly = datagrid.ReadOnly;
        ts.RowHeadersVisible = datagrid.RowHeadersVisible;
        ts.RowHeaderWidth = datagrid.RowHeaderWidth;
        ts.SelectionBackColor = datagrid.SelectionBackColor;
        ts.SelectionForeColor = datagrid.SelectionForeColor;
    }

    private List<DataGridColumnStyleHolder> GetColumnStylesFromControls(
        Control control,
        int offset,
        OrigamPanelColumnConfig userConfig,
        RuleEngine ruleEngine
    )
    {
        var styles = new List<DataGridColumnStyleHolder>();

        //go through all control and their controls
        foreach (Control item in control.Controls)
        {
            if (item is IAsControl)
            {
                DataGridColumnStyle colStyle = CreateColumnStyle(
                    control: item,
                    userConfig: userConfig,
                    ruleEngine: ruleEngine
                );

                if (colStyle != null)
                {
                    int position = offset + item.TabIndex;
                    bool hidden = false;
                    if (_useUserConfig)
                    {
                        DataRow[] configRow = null;
                        configRow = userConfig.PanelColumnConfig.Select(
                            filterExpression: "ColumnName = '" + colStyle.MappingName + "'"
                        );
                        if (configRow != null && configRow.Length > 0)
                        {
                            OrigamPanelColumnConfig.PanelColumnConfigRow conf =
                                configRow[0] as OrigamPanelColumnConfig.PanelColumnConfigRow;
                            if (!conf.IsPositionNull())
                            {
                                position = conf.Position;
                            }
                            hidden = conf.IsHidden;
                        }
                    }
                    styles.Add(
                        item: new DataGridColumnStyleHolder(
                            style: colStyle,
                            index: position,
                            hidden: hidden
                        )
                    );
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(message: "NULL CONTROL STYLE");
                }
            }
            if (item.Controls.Count > 0)
            {
                styles.AddRange(
                    collection: GetColumnStylesFromControls(
                        control: item,
                        offset: offset + item.TabIndex,
                        userConfig: userConfig,
                        ruleEngine: ruleEngine
                    )
                );
            }
        }
        return styles;
    }

    private DataGridColumnStyle CreateColumnStyle(
        Control control,
        OrigamPanelColumnConfig userConfig,
        RuleEngine ruleEngine
    )
    {
        string defProperty = (control as IAsControl).DefaultBindableProperty;
        if (defProperty == null)
        {
            return null;
        }

        DataColumn column = GetDataColumn(control: control);
        if (column == null)
        {
            return null;
        }

        DataGridColumnStyle columnStyle = MakeDataGridColumnStyle(
            control: control,
            ruleEngine: ruleEngine,
            column: column
        );

        columnStyle.HeaderText = SelectCaption(column: column, control: control);
        columnStyle.MappingName = column.ColumnName;

        if (column.ReadOnly)
        {
            columnStyle.ReadOnly = true;
        }

        SetColumnWidth(control: control, userConfig: userConfig, columnStyle: columnStyle);
        columnStyle.NullText = "";

        FilterFactory.AddControlToPanel(
            control: control,
            column: column,
            controlWidth: columnStyle.Width
        );

        return columnStyle;
    }

    private void SetColumnWidth(
        Control control,
        OrigamPanelColumnConfig userConfig,
        DataGridColumnStyle columnStyle
    )
    {
        DataRow[] configRow = null;
        if (_useUserConfig)
        {
            configRow = userConfig.PanelColumnConfig.Select(
                filterExpression: "ColumnName = '" + columnStyle.MappingName + "'"
            );
        }
        if (configRow != null && configRow.Length > 0)
        {
            // if it exists in user config, we use user config
            columnStyle.Width = (
                configRow[0] as OrigamPanelColumnConfig.PanelColumnConfigRow
            ).ColumnWidth;
        }
        else if (
            (control is IAsCaptionControl)
            && (control as IAsCaptionControl).GridColumnWidth > 0
        )
        {
            // if control has set a specific width, we use the control's GridColumnWidth
            columnStyle.Width = (control as IAsCaptionControl).GridColumnWidth;
        }
        else
        {
            // otherwise we use default width
            columnStyle.Width = 100;
        }
    }

    private DataGridColumnStyle MakeDataGridColumnStyle(
        Control control,
        RuleEngine ruleEngine,
        DataColumn column
    )
    {
        DataGridColumnStyle result;
        if (control is AsDropDown dropDown)
        {
            var dataGridDropdownColumn = new DataGridDropdownColumn(
                dropDown: dropDown,
                columnName: column.ColumnName,
                ruleEngine: ruleEngine
            );

            grid.WatchClicksToRaiseEditorDoubleClicked(gridEditor: dataGridDropdownColumn.DropDown);
            result = dataGridDropdownColumn;
        }
        else if (control is BlobControl)
        {
            result = new DataGridBlobColumn(
                blobControl: control as BlobControl,
                ruleEngine: ruleEngine
            );
        }
        else if (control is ImageBox)
        {
            result = new DataGridImageColumn(width: control.Width, height: control.Height);
        }
        else if (control is AsDateBox dateBox)
        {
            var asDataViewColumn = new AsDataViewColumn();
            asDataViewColumn.AlwaysReadOnly = dateBox.ReadOnly;
            asDataViewColumn.AsDateBox.Format = dateBox.Format;
            asDataViewColumn.AsDateBox.CustomFormat = dateBox.CustomFormat;
            asDataViewColumn.Format = dateBox.EditControl.CustomFormat;
            asDataViewColumn.FormatInfo = null;

            grid.WatchClicksToRaiseEditorDoubleClicked(gridEditor: asDataViewColumn.AsDateBox);

            result = asDataViewColumn;
        }
        else if (control is AsCheckBox checkBox)
        {
            var asCheckStyleColumn = new AsCheckStyleColumn();
            asCheckStyleColumn.AlwaysReadOnly = checkBox.ReadOnly;
            result = asCheckStyleColumn;
        }
        else
        {
            result = new AsTextBoxStyleColumn();
            var asTextBoxStyleColumn = result as AsTextBoxStyleColumn;
            asTextBoxStyleColumn.FormatInfo = null;
            if (control is AsTextBox asTextBox)
            {
                asTextBoxStyleColumn.AlwaysReadOnly = asTextBox.ReadOnly;
                asTextBoxStyleColumn.TextBox.MaxLength = asTextBox.MaxLength;
                asTextBoxStyleColumn.AsTextBox.MaxLength = asTextBox.MaxLength;
                asTextBoxStyleColumn.TextBox.Multiline = false;
                asTextBoxStyleColumn.AsTextBox.Multiline = asTextBox.Multiline;
                asTextBoxStyleColumn.AsTextBox.AcceptsTab = false;
                asTextBoxStyleColumn.AsTextBox.DataType = asTextBox.DataType;
                asTextBoxStyleColumn.AsTextBox.CustomFormat = asTextBox.CustomFormat;
                if (
                    asTextBoxStyleColumn.AsTextBox.DataType == typeof(string)
                    | asTextBoxStyleColumn.AsTextBox.DataType == typeof(object)
                )
                {
                    asTextBoxStyleColumn.TextBox.TextAlign = HorizontalAlignment.Left;
                    asTextBoxStyleColumn.AsTextBox.TextAlign = HorizontalAlignment.Left;
                }
                else
                {
                    asTextBoxStyleColumn.TextBox.TextAlign = HorizontalAlignment.Right;
                    asTextBoxStyleColumn.AsTextBox.TextAlign = HorizontalAlignment.Right;
                    if (!string.IsNullOrEmpty(value: asTextBoxStyleColumn.AsTextBox.CustomFormat))
                    {
                        asTextBoxStyleColumn.Format = asTextBoxStyleColumn.AsTextBox.CustomFormat;
                    }
                    else if (asTextBoxStyleColumn.AsTextBox.DataType == typeof(int))
                    {
                        asTextBoxStyleColumn.Format = "d";
                    }
                    else
                    {
                        asTextBoxStyleColumn.Format = "N2";
                    }
                }
                asTextBoxStyleColumn.Alignment = asTextBoxStyleColumn.AsTextBox.TextAlign;

                grid.WatchClicksToRaiseEditorDoubleClicked(
                    gridEditor: asTextBoxStyleColumn.AsTextBox
                );
            }
        }
        return result;
    }

    private static DataColumn GetDataColumn(Control control)
    {
        Binding controlDataBinding = control.DataBindings[index: 0];
        if (controlDataBinding == null)
        {
            return null;
        }

        var bindingField = controlDataBinding.BindingMemberInfo.BindingField;
        if (controlDataBinding.DataSource is DataView dataView)
        {
            return dataView.Table.Columns[name: bindingField];
        }
        if (controlDataBinding.DataSource is DataSet dataSet)
        {
            string tableName = FormTools.FindTableByDataMember(
                ds: dataSet,
                member: controlDataBinding.BindingMemberInfo.BindingPath
            );
            return dataSet.Tables[name: tableName].Columns[name: bindingField];
        }
        if (controlDataBinding.DataSource is DataTable dataTable)
        {
            return dataTable.Columns[name: bindingField];
        }
        return null;
    }

    private string SelectCaption(DataColumn column, Control control)
    {
        string result = "";
        if (column == null || control == null)
        {
            return result;
        }
        if (control is IAsCaptionControl iAsCaptionControl)
        {
            if (!string.IsNullOrEmpty(value: iAsCaptionControl.GridColumnCaption))
            {
                return ((IAsCaptionControl)control).GridColumnCaption;
            }

            if (!string.IsNullOrEmpty(value: iAsCaptionControl.Caption))
            {
                return ((IAsCaptionControl)control).Caption;
            }
        }
        return column.Caption;
    }
    #endregion
    #region Event Handlers
    private void Style_WidthChanged(object sender, EventArgs e)
    {
        if (sender is DataGridColumnStyle columnStyle)
        {
            if (columnStyle.DataGridTableStyle != null)
            {
                this.FilterFactory.PlotControls(
                    style: columnStyle.DataGridTableStyle,
                    offset: (
                        columnStyle.DataGridTableStyle.DataGrid as AsDataGrid
                    ).HorizontalScrollPosition
                );
            }
            foreach (DataGridColumnStyleHolder holder in _styles)
            {
                if (holder.Style.Equals(obj: columnStyle))
                {
                    StoreColumnConfig(
                        columnStyle: columnStyle,
                        position: holder.Index,
                        hidden: holder.Hidden
                    );
                    break;
                }
            }
        }
    }

    private void StoreColumnConfig(DataGridColumnStyle columnStyle, int position, bool hidden)
    {
        OrigamPanelColumnConfigDA.PersistColumnConfig(
            panelId: _panelId,
            columnName: columnStyle.MappingName,
            position: position,
            width: columnStyle.Width,
            hidden: hidden
        );
    }

    private void DataGridBuilder_RowHeaderWidthChanged(object sender, EventArgs e)
    {
        this.FilterFactory.PlotControls(
            style: sender as DataGridTableStyle,
            offset: ((sender as DataGridTableStyle).DataGrid as AsDataGrid).HorizontalScrollPosition
        );
    }

    private void grid_SizeChanged(object sender, EventArgs e)
    {
        this.FilterFactory.PlotControls(
            style: (sender as DataGrid).TableStyles[index: 0],
            offset: (sender as AsDataGrid).HorizontalScrollPosition
        );
    }

    private void grid_Scroll(object sender, EventArgs e)
    {
        this.FilterFactory.PlotControls(
            style: (sender as DataGrid).TableStyles[index: 0],
            offset: (sender as AsDataGrid).HorizontalScrollPosition
        );
    }

    private void grid_MouseDown(object sender, MouseEventArgs e)
    {
        AsDataGrid grid = sender as AsDataGrid;
        DataGrid.HitTestInfo hti = (sender as DataGrid).HitTest(x: e.X, y: e.Y);
        if (hti.Type == DataGrid.HitTestType.ColumnHeader)
        {
            // if user clicked on column header while inserting a new row, we ignore this action,
            // otherwise user would loose his new row after resorting
            if (
                grid.CurrencyManager.Position >= 0
                && (grid.CurrencyManager.Current as DataRowView).IsNew
            )
            {
                return;
            }
            string sortColumnName = this.grid
                .TableStyles[index: 0]
                .GridColumnStyles[index: hti.Column]
                .MappingName;
            AsPanel panel = parentControl as AsPanel;
            if (e.Button == MouseButtons.Left)
            {
                if (panel != null)
                {
                    if (Control.ModifierKeys == Keys.Shift)
                    {
                        if (panel.IsColumnSorted(columnName: sortColumnName))
                        {
                            panel.ReverseSort(columnName: sortColumnName);
                        }
                        else
                        {
                            panel.AddSort(
                                columnName: sortColumnName,
                                sortDirection: Schema
                                    .EntityModel
                                    .DataStructureColumnSortDirection
                                    .Ascending
                            );
                        }
                    }
                    else
                    {
                        if (panel.IsColumnSorted(columnName: sortColumnName))
                        {
                            Schema.EntityModel.DataStructureColumnSortDirection dir = Schema
                                .EntityModel
                                .DataStructureColumnSortDirection
                                .Ascending;
                            if (
                                panel.ColumnSortDirection(columnName: sortColumnName)
                                == Schema.EntityModel.DataStructureColumnSortDirection.Ascending
                            )
                            {
                                dir = Schema
                                    .EntityModel
                                    .DataStructureColumnSortDirection
                                    .Descending;
                            }
                            panel.Sort(columnName: sortColumnName, sortDirection: dir);
                        }
                        else
                        {
                            panel.Sort(
                                columnName: sortColumnName,
                                sortDirection: Schema
                                    .EntityModel
                                    .DataStructureColumnSortDirection
                                    .Ascending
                            );
                        }
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                DataGridColumnConfig configForm = new DataGridColumnConfig();
                configForm.Hide();
                configForm.TableStyle = grid.TableStyles[index: 0];
                configForm.Columns = _styles;
                configForm.ShowDialog(owner: (this.Grid as Control).FindForm());
                UpdateColumns(
                    grid: grid,
                    tableStyle: grid.TableStyles[index: 0],
                    firstTime: false,
                    ds: null
                );
                this.FilterFactory.PlotControls(
                    style: (sender as DataGrid).TableStyles[index: 0],
                    offset: (sender as AsDataGrid).HorizontalScrollPosition
                );
            }
        }
        if (
            hti.Column >= 0
            && hti.Row >= 0
            && hti.Row
                < grid.BindingContext[
                    dataSource: grid.DataSource,
                    dataMember: grid.DataMember
                ].Count
        )
        {
            try
            {
                // If the user clicked on the checkbox column, we change the value of this column.
                if (
                    grid.TableStyles[index: 0].GridColumnStyles[index: hti.Column]
                        is AsCheckStyleColumn
                    && grid.TableStyles[index: 0].GridColumnStyles[index: hti.Column].ReadOnly
                        == false
                )
                {
                    CurrencyManager cm =
                        grid.BindingContext[
                            dataSource: grid.DataSource,
                            dataMember: grid.DataMember
                        ] as CurrencyManager;
                    RuleEngine ruleEngine = _form.FormGenerator.FormRuleEngine;
                    switch (e.Button)
                    {
                        case MouseButtons.Left: // By left button we only change the clicked cell.
                        {
                            if (grid.DataSource != null)
                            {
                                // set the current cell to the checkbox column, otherwise datagrid will
                                // scroll to a wrong column after changing the value
                                grid.CurrentCell = new DataGridCell(r: hti.Row, c: hti.Column);
                                // then we end any edits made so far
                                grid.BindingContext[
                                        dataSource: grid.DataSource,
                                        dataMember: grid.DataMember
                                    ]
                                    .EndCurrentEdit();
                            }

                            // Now we change the value.
                            if (
                                grid.DataSource != null
                                && hti.Row
                                    < grid.BindingContext[
                                        dataSource: grid.DataSource,
                                        dataMember: grid.DataMember
                                    ].Count
                            )
                            {
                                bool canEdit = ruleEngine.EvaluateRowLevelSecurityState(
                                    row: (cm.Current as DataRowView).Row,
                                    field: grid.TableStyles[index: 0]
                                        .GridColumnStyles[index: hti.Column]
                                        .MappingName,
                                    type: Schema.EntityModel.CredentialType.Update
                                );
                                if (canEdit)
                                {
                                    // revert the value of the checkbox
                                    grid[rowIndex: hti.Row, columnIndex: hti.Column] =
                                        !Convert.ToBoolean(
                                            value: grid[rowIndex: hti.Row, columnIndex: hti.Column]
                                        );
                                    // force value to form component to prevent loss of data
                                    DataBindingTools.UpdateBindedFormComponent(
                                        bindings: grid.BindingContext[
                                            dataSource: grid.DataSource,
                                            dataMember: grid.DataMember
                                        ].Bindings,
                                        mappingName: grid.TableStyles[index: 0]
                                            .GridColumnStyles[index: hti.Column]
                                            .MappingName
                                    );
                                    // End edit - so the checkbox immediately gets its value and the user
                                    // does not have to commit it. Useful for all kinds of selection dialogs etc. where
                                    // checkboxes are mainly used.
                                    grid.BindingContext[
                                            dataSource: grid.DataSource,
                                            dataMember: grid.DataMember
                                        ]
                                        .EndCurrentEdit();
                                }
                            }

                            break;
                        }

                        case MouseButtons.Right: // By right button we reverse all values in the current column.
                        {
                            var selectedRows = new List<DataRow>();
                            int count = cm.Count;
                            for (int i = 0; i < count; i++)
                            {
                                if (grid.IsSelected(row: i))
                                {
                                    selectedRows.Add(item: (cm.List[index: i] as DataRowView).Row);
                                }
                            }
                            string columnName = grid.TableStyles[index: 0]
                                .GridColumnStyles[index: hti.Column]
                                .PropertyDescriptor
                                .Name;
                            foreach (DataRow row in selectedRows)
                            {
                                bool canEdit = ruleEngine.EvaluateRowLevelSecurityState(
                                    row: row,
                                    field: columnName,
                                    type: Schema.EntityModel.CredentialType.Update
                                );
                                if (canEdit)
                                {
                                    row[columnName: columnName] = !Convert.ToBoolean(
                                        value: row[columnName: columnName]
                                    );
                                }
                            }

                            break;
                        }
                    }
                }
            }
            catch { }
        }
    }

    private void grid_MouseUp(object sender, MouseEventArgs e)
    {
        AsDataGrid grid = sender as AsDataGrid;
        DataGrid.HitTestInfo hti = (sender as DataGrid).HitTest(x: e.X, y: e.Y);
        if (hti.Column < 0 || hti.Row < 0)
        {
            return;
        }

        if (
            grid.TableStyles[index: 0].GridColumnStyles[index: hti.Column] is DataGridDropdownColumn
            && (
                (Control.ModifierKeys & Keys.Control) == Keys.Control
                & e.Button == MouseButtons.Left
            )
        )
        {
            DataGridDropdownColumn col =
                grid.TableStyles[index: 0].GridColumnStyles[index: hti.Column]
                as DataGridDropdownColumn;
            IDataLookupService lookupService =
                ServiceManager.Services.GetService(serviceType: typeof(IDataLookupService))
                as IDataLookupService;
            CurrencyManager cm =
                grid.BindingContext[dataSource: grid.DataSource, dataMember: grid.DataMember]
                as CurrencyManager;
            object value = (cm.List[index: hti.Row] as DataRowView).Row[
                columnName: col.MappingName
            ];
            object linkTarget = lookupService.LinkTarget(lookupControl: col.DropDown, value: value);
            Dictionary<string, object> parameters = lookupService.LinkParameters(
                linkTarget: linkTarget,
                value: value
            );
            Workbench.WorkbenchSingleton.Workbench.ProcessGuiLink(
                sourceForm: _form,
                linkTarget: linkTarget,
                parameters: parameters
            );
        }
    }

    private void grid_MouseMove(object sender, MouseEventArgs e)
    {
        DataGrid.HitTestInfo hti = (sender as DataGrid).HitTest(x: e.X, y: e.Y);
        AsDataGrid grid = sender as AsDataGrid;
        if (hti.Column < 0 || hti.Row < 0)
        {
            return;
        }

        DataGridDropdownColumn col =
            grid.TableStyles[index: 0].GridColumnStyles[index: hti.Column]
            as DataGridDropdownColumn;
        if (col != null && (Control.ModifierKeys & Keys.Control) == Keys.Control)
        {
            CurrencyManager cm =
                grid.BindingContext[dataSource: grid.DataSource, dataMember: grid.DataMember]
                as CurrencyManager;
            IDataLookupService lookupService =
                ServiceManager.Services.GetService(serviceType: typeof(IDataLookupService))
                as IDataLookupService;
            object value = (cm.List[index: hti.Row] as DataRowView).Row[
                columnName: col.MappingName
            ];
            object linkTarget = lookupService.LinkTarget(lookupControl: col.DropDown, value: value);
            if (linkTarget != null && value != DBNull.Value)
            {
                grid.Cursor = Cursors.Hand;
            }
        }
    }

    /// <summary>
    /// Handle checkbox select/deselect by keyboard
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void grid_KeyPress(object sender, KeyPressEventArgs e)
    {
        AsDataGrid grid = sender as AsDataGrid;
        if (grid.DataSource == null)
        {
            return;
        }

        if (!(grid.CurrentCell.ColumnNumber >= 0 & grid.CurrentCell.RowNumber >= 0))
        {
            return;
        }

        if (
            grid.TableStyles[index: 0].GridColumnStyles[index: grid.CurrentCell.ColumnNumber]
                is AsCheckStyleColumn
            && grid.TableStyles[index: 0]
                .GridColumnStyles[index: grid.CurrentCell.ColumnNumber]
                .ReadOnly == false
        )
        {
            switch (e.KeyChar)
            {
                case (char)32: // space
                {
                    grid[cell: grid.CurrentCell] = !Convert.ToBoolean(
                        value: grid[cell: grid.CurrentCell]
                    );
                    e.Handled = true;
                    break;
                }

                case (char)42: // *
                {
                    for (
                        int i = 0;
                        i
                            < grid.BindingContext[
                                dataSource: grid.DataSource,
                                dataMember: grid.DataMember
                            ].Count;
                        i++
                    )
                    {
                        if (grid.IsSelected(row: i))
                        {
                            grid[rowIndex: i, columnIndex: grid.CurrentCell.ColumnNumber] =
                                !Convert.ToBoolean(
                                    value: grid[
                                        rowIndex: i,
                                        columnIndex: grid.CurrentCell.ColumnNumber
                                    ]
                                );
                        }
                    }
                    break;
                }
            }
        }
    }
    #endregion
    #region IDisposable Members
    public void Dispose()
    {
        if (grid == null)
        {
            return;
        }

        parentControl = null;
        FilterFactory = null;
        grid.MouseDown -= grid_MouseDown;
        grid.MouseUp -= grid_MouseUp;
        grid.AfterMouseMove -= grid_MouseMove;
        if (grid.TableStyles.Count > 0)
        {
            grid.TableStyles[index: 0].RowHeaderWidthChanged -=
                DataGridBuilder_RowHeaderWidthChanged;
        }
        if (_styles != null)
        {
            foreach (DataGridColumnStyleHolder style in _styles)
            {
                style.Style.WidthChanged -= Style_WidthChanged;
                _form.FormGenerator.SetTooltip(style: style.Style, tipText: null);
            }
        }
        grid.Scroll -= grid_Scroll;
        grid.SizeChanged -= grid_SizeChanged;
        grid = null;
        _form = null;
    }
    #endregion
}
