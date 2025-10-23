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
using System.Collections;
using System.Data;
using System.Windows.Forms;
using Origam.DA;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Gui.Win;

public delegate void DataViewQueryChanged(object sender, string query);

/// <summary>
/// This Factory Fill Controls into AsPanel
/// </summary>
public class DataGridFilterFactory : IDisposable
{
    public event DataViewQueryChanged DataViewQueryChanged;

    private IServiceAgent _dataServiceAgent;
    private ISchemaService _schema =
        ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
    private FilterPanel pnlFilter = null;
    private FormGenerator formGenerator = null;
    private string panelDataMember = "";
    private Guid _panelId;
    private string query = "";

    public DataGridFilterFactory(
        FilterPanel panel,
        FormGenerator generator,
        string datamember,
        Guid panelId,
        IGridBuilder gridBuilder
    )
    {
        _dataServiceAgent = (
            ServiceManager.Services.GetService(typeof(IBusinessServicesService))
            as IBusinessServicesService
        ).GetAgent("DataService", null, null);

        this.pnlFilter = panel;
        this.pnlFilter.QueryChanged += new DataViewQueryChanged(pnlFilter_QueryChanged);
        this.formGenerator = generator;
        this.panelDataMember = datamember;
        _panelId = panelId;
        this.GridBuilder = gridBuilder;
    }

    public string Query
    {
        get { return this.query; }
        set
        {
            this.query = value;

            AsForm form = this.pnlFilter.FindForm() as AsForm;
            if (form != null)
            {
                form.IsFiltering = true;
            }

            if (DataViewQueryChanged != null)
            {
                DataViewQueryChanged(this, this.query);
            }
            if (form != null)
            {
                form.IsFiltering = false;
            }
        }
    }
    private OrigamPanelFilter.PanelFilterRow _currentStoredFilter;
    public OrigamPanelFilter.PanelFilterRow CurrentStoredFilter
    {
        get { return _currentStoredFilter; }
        set
        {
            ApplyFilter(value);
            _currentStoredFilter = value;
        }
    }
    private IGridBuilder _gridBuilder;
    public IGridBuilder GridBuilder
    {
        get { return _gridBuilder; }
        set { _gridBuilder = value; }
    }
    private Hashtable _filterItems = new Hashtable();
    public Hashtable FilterItems
    {
        get { return _filterItems; }
    }
    public int ScrollOffset
    {
        get { return pnlFilter.ScrollOffset; }
    }

    public void AddControlToPanel(Control control, DataColumn column, int controlWidth)
    {
        FilterPart part = null;
        string caption = null;

        IAsCaptionControl captionControl = control as IAsCaptionControl;
        if (captionControl != null)
        {
            caption = captionControl.GridColumnCaption;
            if (caption == "" | caption == null)
            {
                caption = captionControl.Caption;
            }
        }
        if (caption == "" | caption == null)
        {
            caption = column.Caption;
        }

        AsDropDown dropDown = control as AsDropDown;
        AsTextBox textBox = control as AsTextBox;
        AsTextBox tb = new AsTextBox();
        if (dropDown != null)
        {
            if (dropDown.ParameterMappings.Count > 0)
            {
                string lookUpColumnName = DatasetTools.SortColumnName(column.ColumnName);
                part = new StringFilterPart(
                    tb,
                    typeof(string),
                    lookUpColumnName,
                    column.ColumnName,
                    caption,
                    formGenerator
                );
            }
            else
            {
                part = new DropDownFilterPart(
                    dropDown,
                    column.DataType,
                    column.ColumnName,
                    column.ColumnName,
                    caption,
                    formGenerator
                );
            }
        }
        else if (textBox != null)
        {
            if (column.DataType == typeof(string))
            {
                part = new StringFilterPart(
                    textBox,
                    column.DataType,
                    column.ColumnName,
                    column.ColumnName,
                    caption,
                    formGenerator
                );
            }
            else if (
                column.DataType == typeof(int)
                | column.DataType == typeof(float)
                | column.DataType == typeof(decimal)
                | column.DataType == typeof(long)
            )
            {
                part = new NumberFilterPart(
                    textBox,
                    column.DataType,
                    column.ColumnName,
                    column.ColumnName,
                    caption,
                    formGenerator
                );
            }
        }
        else if (control is AsDateBox)
        {
            part = new DateFilterPart(
                control as AsDateBox,
                column.DataType,
                column.ColumnName,
                column.ColumnName,
                caption,
                formGenerator
            );
        }
        else if (control is BlobControl)
        {
            part = new StringFilterPart(
                tb,
                typeof(string),
                column.ColumnName,
                column.ColumnName,
                caption,
                formGenerator
            );
        }
        else if (control is AsCheckBox)
        {
            part = new BoolFilterPart(
                control as AsCheckBox,
                column.DataType,
                column.ColumnName,
                column.ColumnName,
                caption,
                formGenerator
            );
        }
        else
        {
            part = new DummyFilterPart(
                control,
                column.DataType,
                column.ColumnName,
                column.ColumnName,
                caption,
                formGenerator
            );
        }
        if (part != null)
        {
            pnlFilter.AddFilterPart(part);
        }
    }

    public void PlotControls(DataGridTableStyle style, int offset)
    {
        this.pnlFilter.SizeControls(style, offset);
    }

    //		private void ControlValueChanged(object sender, EventArgs e)
    //		{
    //			int position = (GridBuilder.Grid as AsDataGrid).HorizontalScrollPosition;
    //
    //			QueryChanged(sender);
    //
    //			(GridBuilder.Grid as AsDataGrid).HorizontalScrollPosition = position;
    //			(sender as Control).Select();
    //		}
    private OrigamPanelFilter _storedFilters = null;
    public OrigamPanelFilter StoredFilters
    {
        get
        {
            if (_storedFilters == null)
            {
                _storedFilters = OrigamPanelFilterDA.LoadFilters(_panelId);
            }
            return _storedFilters;
        }
    }

    public void PersistFilters()
    {
        PersistFilter(_storedFilters);
    }

    public void PersistFilter(OrigamPanelFilter filter)
    {
        OrigamPanelFilterDA.PersistFilter(filter);
    }

    public OrigamPanelFilter LoadFilter(Guid id)
    {
        return OrigamPanelFilterDA.LoadFilter(id);
    }

    public void DeleteFilter(OrigamPanelFilter.PanelFilterRow filter)
    {
        filter.Delete();
        PersistFilters();
        _currentStoredFilter = null;
    }

    public void StoreCurrentFilter(string name, bool global)
    {
        // add new filter to stored filters
        OrigamPanelFilter.PanelFilterRow filter = _storedFilters.PanelFilter.NewPanelFilterRow();
        filter.Id = Guid.NewGuid();

        GetFilterFromCurrent(filter, name, global, false, _panelId);

        PersistFilters();
        _currentStoredFilter = filter;
    }

    public void GetFilterFromCurrent(
        OrigamPanelFilter.PanelFilterRow filter,
        string name,
        bool global,
        bool isDefault,
        Guid panelId
    )
    {
        UserProfile profile = SecurityManager.CurrentUserProfile();
        filter.Name = name;
        filter.IsGlobal = global;
        filter.IsDefault = isDefault;
        filter.PanelId = panelId;
        filter.ProfileId = profile.Id;

        if (!filter.IsRecordCreatedNull())
        {
            filter.RecordUpdated = DateTime.Now;
        }

        if (!filter.IsRecordCreatedByNull())
        {
            filter.RecordUpdatedBy = profile.Id;
        }

        if (filter.IsRecordCreatedNull())
        {
            filter.RecordCreated = DateTime.Now;
        }

        if (filter.IsRecordCreatedByNull())
        {
            filter.RecordCreatedBy = profile.Id;
        }

        if (filter.RowState == DataRowState.Detached)
        {
            (filter.Table.DataSet as OrigamPanelFilter).PanelFilter.AddPanelFilterRow(filter);
        }
        // delete current filter, if exists
        foreach (DataRow row in filter.GetPanelFilterDetailRows())
        {
            row.Delete();
        }
        AddFilterDetails(filter.Table.DataSet as OrigamPanelFilter, filter, profile.Id);
    }

    public void AddFilterDetails(
        OrigamPanelFilter filterDS,
        OrigamPanelFilter.PanelFilterRow filter,
        Guid profileId
    )
    {
        pnlFilter.AddFilterDetails(filterDS, filter, profileId);
    }

    public void ApplyFilter(OrigamPanelFilter.PanelFilterRow filter)
    {
        pnlFilter.ApplyFilter(filter);
    }

    public void FocusFilterPanel()
    {
        this.pnlFilter.SelectNextControl(this.pnlFilter, true, true, true, true);
    }

    #region MakeQueryString
    private string GetControlOperator(Control item, string columnName)
    {
        switch (item.GetType().Name)
        {
            case "AsDateBox":
            {
                return ">=";
            }
            case "AsTextBox":
            {
                if (
                    (item as AsTextBox).DataType == typeof(string)
                    | (item as AsTextBox).DataType == typeof(object)
                )
                {
                    return "LIKE";
                }

                return "=";
            }

            default:
            {
                return "=";
            }
        }
    }
    #endregion

    #region Clear Filter

    public void ClearQueryFields()
    {
        pnlFilter.ClearQuery();
        _currentStoredFilter = null;
    }
    #endregion
    #region IDisposable Members
    public void Dispose()
    {
        this.pnlFilter.QueryChanged -= new DataViewQueryChanged(pnlFilter_QueryChanged);
        this.formGenerator = null;
        _currentStoredFilter = null;
        _dataServiceAgent = null;
        _schema = null;
        pnlFilter = null;
        _gridBuilder = null;

        this._filterItems.Clear();
    }
    #endregion
    private void pnlFilter_QueryChanged(object sender, string query)
    {
        this.Query = query;
        _currentStoredFilter = null;
    }

    public void RefreshFilter()
    {
        this.Query = pnlFilter.GetQuery();
    }
}
