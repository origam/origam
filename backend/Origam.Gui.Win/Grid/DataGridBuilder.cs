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
            ServiceManager.Services.GetService(typeof(IDocumentationService))
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
        control.Controls.Add(grid);
        grid.TableStyles.Clear();
        grid.TableStyles.Add(
            SetUpGridStyle(grid, control, dataSource as DataSet, dataMember, ruleEngine)
        );
        grid.TableStyles[0].RowHeaderWidthChanged += DataGridBuilder_RowHeaderWidthChanged;
        grid.Scroll += grid_Scroll;
        return grid;
    }

    public void UpdateDataSource(Control grid, object dataSource, string dataMember)
    {
        if (!(grid is AsDataGrid asGrid))
        {
            throw new NullReferenceException(ResourceUtils.GetString("ErrorNoGrid"));
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
                    asGrid.SetDataBinding(dataSource, dataMember);
                    int columnNumber = asGrid.CurrentCell.ColumnNumber;
                    int rowNumber = asGrid.CurrentCell.RowNumber;
                    if (dataSource != null && !this._form.FormGenerator.IgnoreDataChanges)
                    {
                        try
                        {
                            object layout = Reflector.GetValue(typeof(DataGrid), asGrid, "layout");
                            if (layout != null)
                            {
                                bool dirty = (bool)
                                    Reflector.GetValue(layout.GetType(), layout, "dirty");
                                if (!dirty)
                                {
                                    asGrid.CurrentCell = new DataGridCell(0, columnNumber);
                                    asGrid.CurrentCell = new DataGridCell(rowNumber, columnNumber);
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
                    asGrid.ResumeLayout(false);
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
            FormTools.FindTableByDataMember(grid.DataSource as DataSet, grid.DataMember)
        ];
        return (parentControl as AsPanel).GetSortColumn(columnName, table);
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
        grid.Size = new System.Drawing.Size(176, 72);
        grid.Location = new System.Drawing.Point(-1000, -1000);
        grid.Name = "grid";
        grid.ReadOnly = true;
        grid.TabIndex = 0;
        grid.TabStop = false;
        grid.CaptionVisible = false;
        grid.DataSource = null;
        grid.AlternatingBackColor = OrigamColorScheme.GridAlternatingBackColor;
        grid.BackgroundColor = OrigamColorScheme.FormBackgroundColor;
        grid.BorderStyle = BorderStyle.None;
        grid.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
        grid.ForeColor = OrigamColorScheme.GridForeColor;
        grid.GridLineColor = OrigamColorScheme.GridLineColor;
        grid.GridLineStyle = DataGridLineStyle.Solid;
        grid.HeaderBackColor = OrigamColorScheme.GridHeaderBackColor;
        grid.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
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
        CopyDefaultTableStyle(grid, tableStyle);
        UserProfile profile = SecurityManager.CurrentUserProfile();
        OrigamPanelColumnConfig userConfig = null;

        if (_useUserConfig)
        {
            userConfig = OrigamPanelColumnConfigDA.LoadUserConfig(_panelId, profile.Id);
        }
        //Go through all controls and find any bound columns
        _styles = GetColumnStylesFromControls(control, 0, userConfig, ruleEngine);

        //mapping name to grid
        tableStyle.MappingName = FormTools.FindTableByDataMember(ds, member);
        UpdateColumns(grid, tableStyle, true, ds);
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
            _styles[0].Hidden = false;
        }
        foreach (DataGridColumnStyleHolder style in _styles)
        {
            if (firstTime)
            {
                if (!tableStyle.GridColumnStyles.Contains(style.Style.MappingName))
                {
                    tableStyle.GridColumnStyles.Add(style.Style);
                    style.Style.WidthChanged += Style_WidthChanged;
                    Guid columnId = (Guid)
                        ds.Tables[tableStyle.MappingName]
                            .Columns[style.Style.MappingName]
                            .ExtendedProperties["Id"];
                    string tipText = _documentationService.GetDocumentation(
                        columnId,
                        DocumentationType.USER_LONG_HELP
                    );
                    _form.FormGenerator.SetTooltip(style.Style, tipText);
                    if (style.Hidden)
                    {
                        tableStyle.GridColumnStyles.Remove(style.Style);
                    }
                }
            }
            else
            {
                if (!style.Hidden)
                {
                    if (!tableStyle.GridColumnStyles.Contains(style.Style.MappingName))
                    {
                        tableStyle.GridColumnStyles.Add(style.Style);
                    }
                }

                // not for the first time, so the config has changed -> we must persist the changes
                StoreColumnConfig(style.Style, style.Index, style.Hidden);
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
                DataGridColumnStyle colStyle = CreateColumnStyle(item, userConfig, ruleEngine);

                if (colStyle != null)
                {
                    int position = offset + item.TabIndex;
                    bool hidden = false;
                    if (_useUserConfig)
                    {
                        DataRow[] configRow = null;
                        configRow = userConfig.PanelColumnConfig.Select(
                            "ColumnName = '" + colStyle.MappingName + "'"
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
                    styles.Add(new DataGridColumnStyleHolder(colStyle, position, hidden));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("NULL CONTROL STYLE");
                }
            }
            if (item.Controls.Count > 0)
            {
                styles.AddRange(
                    GetColumnStylesFromControls(
                        item,
                        offset + item.TabIndex,
                        userConfig,
                        ruleEngine
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

        DataColumn column = GetDataColumn(control);
        if (column == null)
        {
            return null;
        }

        DataGridColumnStyle columnStyle = MakeDataGridColumnStyle(control, ruleEngine, column);

        columnStyle.HeaderText = SelectCaption(column, control);
        columnStyle.MappingName = column.ColumnName;

        if (column.ReadOnly)
        {
            columnStyle.ReadOnly = true;
        }

        SetColumnWidth(control, userConfig, columnStyle);
        columnStyle.NullText = "";

        FilterFactory.AddControlToPanel(control, column, columnStyle.Width);

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
                "ColumnName = '" + columnStyle.MappingName + "'"
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
                dropDown,
                column.ColumnName,
                ruleEngine
            );

            grid.WatchClicksToRaiseEditorDoubleClicked(dataGridDropdownColumn.DropDown);
            result = dataGridDropdownColumn;
        }
        else if (control is BlobControl)
        {
            result = new DataGridBlobColumn(control as BlobControl, ruleEngine);
        }
        else if (control is ImageBox)
        {
            result = new DataGridImageColumn(control.Width, control.Height);
        }
        else if (control is AsDateBox dateBox)
        {
            var asDataViewColumn = new AsDataViewColumn();
            asDataViewColumn.AlwaysReadOnly = dateBox.ReadOnly;
            asDataViewColumn.AsDateBox.Format = dateBox.Format;
            asDataViewColumn.AsDateBox.CustomFormat = dateBox.CustomFormat;
            asDataViewColumn.Format = dateBox.EditControl.CustomFormat;
            asDataViewColumn.FormatInfo = null;

            grid.WatchClicksToRaiseEditorDoubleClicked(asDataViewColumn.AsDateBox);

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
                    if (!string.IsNullOrEmpty(asTextBoxStyleColumn.AsTextBox.CustomFormat))
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

                grid.WatchClicksToRaiseEditorDoubleClicked(asTextBoxStyleColumn.AsTextBox);
            }
        }
        return result;
    }

    private static DataColumn GetDataColumn(Control control)
    {
        Binding controlDataBinding = control.DataBindings[0];
        if (controlDataBinding == null)
        {
            return null;
        }

        var bindingField = controlDataBinding.BindingMemberInfo.BindingField;
        if (controlDataBinding.DataSource is DataView dataView)
        {
            return dataView.Table.Columns[bindingField];
        }
        if (controlDataBinding.DataSource is DataSet dataSet)
        {
            string tableName = FormTools.FindTableByDataMember(
                dataSet,
                controlDataBinding.BindingMemberInfo.BindingPath
            );
            return dataSet.Tables[tableName].Columns[bindingField];
        }
        if (controlDataBinding.DataSource is DataTable dataTable)
        {
            return dataTable.Columns[bindingField];
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
            if (!string.IsNullOrEmpty(iAsCaptionControl.GridColumnCaption))
            {
                return ((IAsCaptionControl)control).GridColumnCaption;
            }

            if (!string.IsNullOrEmpty(iAsCaptionControl.Caption))
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
                    columnStyle.DataGridTableStyle,
                    (columnStyle.DataGridTableStyle.DataGrid as AsDataGrid).HorizontalScrollPosition
                );
            }
            foreach (DataGridColumnStyleHolder holder in _styles)
            {
                if (holder.Style.Equals(columnStyle))
                {
                    StoreColumnConfig(columnStyle, holder.Index, holder.Hidden);
                    break;
                }
            }
        }
    }

    private void StoreColumnConfig(DataGridColumnStyle columnStyle, int position, bool hidden)
    {
        OrigamPanelColumnConfigDA.PersistColumnConfig(
            _panelId,
            columnStyle.MappingName,
            position,
            columnStyle.Width,
            hidden
        );
    }

    private void DataGridBuilder_RowHeaderWidthChanged(object sender, EventArgs e)
    {
        this.FilterFactory.PlotControls(
            sender as DataGridTableStyle,
            ((sender as DataGridTableStyle).DataGrid as AsDataGrid).HorizontalScrollPosition
        );
    }

    private void grid_SizeChanged(object sender, EventArgs e)
    {
        this.FilterFactory.PlotControls(
            (sender as DataGrid).TableStyles[0],
            (sender as AsDataGrid).HorizontalScrollPosition
        );
    }

    private void grid_Scroll(object sender, EventArgs e)
    {
        this.FilterFactory.PlotControls(
            (sender as DataGrid).TableStyles[0],
            (sender as AsDataGrid).HorizontalScrollPosition
        );
    }

    private void grid_MouseDown(object sender, MouseEventArgs e)
    {
        AsDataGrid grid = sender as AsDataGrid;
        DataGrid.HitTestInfo hti = (sender as DataGrid).HitTest(e.X, e.Y);
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
                .TableStyles[0]
                .GridColumnStyles[hti.Column]
                .MappingName;
            AsPanel panel = parentControl as AsPanel;
            if (e.Button == MouseButtons.Left)
            {
                if (panel != null)
                {
                    if (Control.ModifierKeys == Keys.Shift)
                    {
                        if (panel.IsColumnSorted(sortColumnName))
                        {
                            panel.ReverseSort(sortColumnName);
                        }
                        else
                        {
                            panel.AddSort(
                                sortColumnName,
                                Schema.EntityModel.DataStructureColumnSortDirection.Ascending
                            );
                        }
                    }
                    else
                    {
                        if (panel.IsColumnSorted(sortColumnName))
                        {
                            Schema.EntityModel.DataStructureColumnSortDirection dir = Schema
                                .EntityModel
                                .DataStructureColumnSortDirection
                                .Ascending;
                            if (
                                panel.ColumnSortDirection(sortColumnName)
                                == Schema.EntityModel.DataStructureColumnSortDirection.Ascending
                            )
                            {
                                dir = Schema
                                    .EntityModel
                                    .DataStructureColumnSortDirection
                                    .Descending;
                            }
                            panel.Sort(sortColumnName, dir);
                        }
                        else
                        {
                            panel.Sort(
                                sortColumnName,
                                Schema.EntityModel.DataStructureColumnSortDirection.Ascending
                            );
                        }
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                DataGridColumnConfig configForm = new DataGridColumnConfig();
                configForm.Hide();
                configForm.TableStyle = grid.TableStyles[0];
                configForm.Columns = _styles;
                configForm.ShowDialog((this.Grid as Control).FindForm());
                UpdateColumns(grid, grid.TableStyles[0], false, null);
                this.FilterFactory.PlotControls(
                    (sender as DataGrid).TableStyles[0],
                    (sender as AsDataGrid).HorizontalScrollPosition
                );
            }
        }
        if (
            hti.Column >= 0
            && hti.Row >= 0
            && hti.Row < grid.BindingContext[grid.DataSource, grid.DataMember].Count
        )
        {
            try
            {
                // If the user clicked on the checkbox column, we change the value of this column.
                if (
                    grid.TableStyles[0].GridColumnStyles[hti.Column] is AsCheckStyleColumn
                    && grid.TableStyles[0].GridColumnStyles[hti.Column].ReadOnly == false
                )
                {
                    CurrencyManager cm =
                        grid.BindingContext[grid.DataSource, grid.DataMember] as CurrencyManager;
                    RuleEngine ruleEngine = _form.FormGenerator.FormRuleEngine;
                    switch (e.Button)
                    {
                        case MouseButtons.Left: // By left button we only change the clicked cell.
                        {
                            if (grid.DataSource != null)
                            {
                                // set the current cell to the checkbox column, otherwise datagrid will
                                // scroll to a wrong column after changing the value
                                grid.CurrentCell = new DataGridCell(hti.Row, hti.Column);
                                // then we end any edits made so far
                                grid.BindingContext[grid.DataSource, grid.DataMember]
                                    .EndCurrentEdit();
                            }

                            // Now we change the value.
                            if (
                                grid.DataSource != null
                                && hti.Row
                                    < grid.BindingContext[grid.DataSource, grid.DataMember].Count
                            )
                            {
                                bool canEdit = ruleEngine.EvaluateRowLevelSecurityState(
                                    (cm.Current as DataRowView).Row,
                                    grid.TableStyles[0].GridColumnStyles[hti.Column].MappingName,
                                    Schema.EntityModel.CredentialType.Update
                                );
                                if (canEdit)
                                {
                                    // revert the value of the checkbox
                                    grid[hti.Row, hti.Column] = !Convert.ToBoolean(
                                        grid[hti.Row, hti.Column]
                                    );
                                    // force value to form component to prevent loss of data
                                    DataBindingTools.UpdateBindedFormComponent(
                                        grid.BindingContext[
                                            grid.DataSource,
                                            grid.DataMember
                                        ].Bindings,
                                        grid.TableStyles[0].GridColumnStyles[hti.Column].MappingName
                                    );
                                    // End edit - so the checkbox immediately gets its value and the user
                                    // does not have to commit it. Useful for all kinds of selection dialogs etc. where
                                    // checkboxes are mainly used.
                                    grid.BindingContext[grid.DataSource, grid.DataMember]
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
                                if (grid.IsSelected(i))
                                {
                                    selectedRows.Add((cm.List[i] as DataRowView).Row);
                                }
                            }
                            string columnName = grid.TableStyles[0]
                                .GridColumnStyles[hti.Column]
                                .PropertyDescriptor
                                .Name;
                            foreach (DataRow row in selectedRows)
                            {
                                bool canEdit = ruleEngine.EvaluateRowLevelSecurityState(
                                    row,
                                    columnName,
                                    Schema.EntityModel.CredentialType.Update
                                );
                                if (canEdit)
                                {
                                    row[columnName] = !Convert.ToBoolean(row[columnName]);
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
        DataGrid.HitTestInfo hti = (sender as DataGrid).HitTest(e.X, e.Y);
        if (hti.Column < 0 || hti.Row < 0)
        {
            return;
        }

        if (
            grid.TableStyles[0].GridColumnStyles[hti.Column] is DataGridDropdownColumn
            && (
                (Control.ModifierKeys & Keys.Control) == Keys.Control
                & e.Button == MouseButtons.Left
            )
        )
        {
            DataGridDropdownColumn col =
                grid.TableStyles[0].GridColumnStyles[hti.Column] as DataGridDropdownColumn;
            IDataLookupService lookupService =
                ServiceManager.Services.GetService(typeof(IDataLookupService))
                as IDataLookupService;
            CurrencyManager cm =
                grid.BindingContext[grid.DataSource, grid.DataMember] as CurrencyManager;
            object value = (cm.List[hti.Row] as DataRowView).Row[col.MappingName];
            object linkTarget = lookupService.LinkTarget(col.DropDown, value);
            Dictionary<string, object> parameters = lookupService.LinkParameters(linkTarget, value);
            Workbench.WorkbenchSingleton.Workbench.ProcessGuiLink(_form, linkTarget, parameters);
        }
    }

    private void grid_MouseMove(object sender, MouseEventArgs e)
    {
        DataGrid.HitTestInfo hti = (sender as DataGrid).HitTest(e.X, e.Y);
        AsDataGrid grid = sender as AsDataGrid;
        if (hti.Column < 0 || hti.Row < 0)
        {
            return;
        }

        DataGridDropdownColumn col =
            grid.TableStyles[0].GridColumnStyles[hti.Column] as DataGridDropdownColumn;
        if (col != null && (Control.ModifierKeys & Keys.Control) == Keys.Control)
        {
            CurrencyManager cm =
                grid.BindingContext[grid.DataSource, grid.DataMember] as CurrencyManager;
            IDataLookupService lookupService =
                ServiceManager.Services.GetService(typeof(IDataLookupService))
                as IDataLookupService;
            object value = (cm.List[hti.Row] as DataRowView).Row[col.MappingName];
            object linkTarget = lookupService.LinkTarget(col.DropDown, value);
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
            grid.TableStyles[0].GridColumnStyles[grid.CurrentCell.ColumnNumber]
                is AsCheckStyleColumn
            && grid.TableStyles[0].GridColumnStyles[grid.CurrentCell.ColumnNumber].ReadOnly == false
        )
        {
            switch (e.KeyChar)
            {
                case (char)32: // space
                {
                    grid[grid.CurrentCell] = !Convert.ToBoolean(grid[grid.CurrentCell]);
                    e.Handled = true;
                    break;
                }

                case (char)42: // *
                {
                    for (
                        int i = 0;
                        i < grid.BindingContext[grid.DataSource, grid.DataMember].Count;
                        i++
                    )
                    {
                        if (grid.IsSelected(i))
                        {
                            grid[i, grid.CurrentCell.ColumnNumber] = !Convert.ToBoolean(
                                grid[i, grid.CurrentCell.ColumnNumber]
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
            grid.TableStyles[0].RowHeaderWidthChanged -= DataGridBuilder_RowHeaderWidthChanged;
        }
        if (_styles != null)
        {
            foreach (DataGridColumnStyleHolder style in _styles)
            {
                style.Style.WidthChanged -= Style_WidthChanged;
                _form.FormGenerator.SetTooltip(style.Style, null);
            }
        }
        grid.Scroll -= grid_Scroll;
        grid.SizeChanged -= grid_SizeChanged;
        grid = null;
        _form = null;
    }
    #endregion
}
