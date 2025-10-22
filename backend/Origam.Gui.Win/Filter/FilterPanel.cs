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
using System.Text;
using System.Windows.Forms;
using Origam.UI;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for FilterPanel.
/// </summary>
public class FilterPanel : System.Windows.Forms.UserControl
{
    public event DataViewQueryChanged QueryChanged;
    private const int TOP = 16;
    private const int CAPTION_TOP = 2;
    private Hashtable _filterParts = new Hashtable();
    private bool _ignoreQueryChange = false;
    private string _query;
    private DataGridTableStyle _lastTableStyle;
    private int _lastOffset = 0;
    private System.Windows.Forms.Timer keyboardTimer;
    private System.ComponentModel.IContainer components;

    public FilterPanel()
    {
        // This call is required by the Windows.Forms Form Designer.
        InitializeComponent();
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _ignoreQueryChange = true;
            this.Controls.Clear();
            if (_filterParts != null)
            {
                foreach (FilterPart part in _filterParts.Values)
                {
                    foreach (Control c in part.FilterControls)
                    {
                        c.Enter -= new EventHandler(filterControl_Enter);
                    }
                    part.QueryChanged -= new DataViewQueryChanged(part_QueryChanged);
                    part.ControlsChanged -= new EventHandler(part_ControlsChanged);
                    part.Dispose();
                }
                _filterParts.Clear();
            }
            _lastTableStyle = null;
            if (components != null)
            {
                components.Dispose();
            }
        }
        base.Dispose(disposing);
    }

    #region Component Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.keyboardTimer = new System.Windows.Forms.Timer(this.components);
        //
        // keyboardTimer
        //
        this.keyboardTimer.Interval = 400;
        this.keyboardTimer.Tick += new System.EventHandler(this.keyboardTimer_Tick);
        //
        // FilterPanel
        //
        this.Name = "FilterPanel";
        this.Size = new System.Drawing.Size(656, 48);
        this.Enter += new System.EventHandler(this.FilterPanel_Enter);
        this.Leave += new System.EventHandler(this.FilterPanel_Leave);
    }
    #endregion
    #region Properties
    public string Query
    {
        get { return _query; }
        set
        {
            if (_query != value)
            {
                _query = value;
                OnQueryChanged(_query);
            }
        }
    }
    public bool FilterActive
    {
        get { return _query != null & _query != ""; }
    }
    public int ScrollOffset
    {
        get { return _lastOffset; }
    }
    #endregion
    #region Public Methods
    public void AddFilterPart(FilterPart part)
    {
        if (!_filterParts.Contains(part.GridColumnName))
        {
            _filterParts.Add(part.GridColumnName, part);
            part.QueryChanged += new DataViewQueryChanged(part_QueryChanged);
            part.ControlsChanged += new EventHandler(part_ControlsChanged);
            PlotControls(part);
        }
    }

    public void ClearQuery()
    {
        _ignoreQueryChange = true;
        try
        {
            foreach (FilterPart part in _filterParts.Values)
            {
                part.Value1 = null;
                part.Value2 = null;
                part.Operator = part.DefaultOperator;
                part.LoadValues();
            }
        }
        finally
        {
            _ignoreQueryChange = false;
            this.Query = "";
        }
    }

    public void SizeControls(DataGridTableStyle tableStyle, int offset)
    {
        if (tableStyle == null)
        {
            return;
        }

        _lastTableStyle = tableStyle;
        _lastOffset = offset;
        this.SuspendLayout();
        int left = tableStyle.RowHeaderWidth - offset;
        int tabIndex = 0;
        int height = 0;
        foreach (DataGridColumnStyle columnStyle in tableStyle.GridColumnStyles)
        {
            FilterPart part = _filterParts[columnStyle.MappingName] as FilterPart;
            if (part != null)
            {
                tabIndex = SizeControls(part, left, columnStyle.Width, tabIndex, ref height);
            }
            left += columnStyle.Width;
        }
        foreach (FilterPart part in _filterParts.Values)
        {
            if (!tableStyle.GridColumnStyles.Contains(part.GridColumnName))
            {
                HideControls(part);
            }
        }
        this.Height = height;
        this.ResumeLayout(true);
    }

    public void ApplyFilter(OrigamPanelFilter.PanelFilterRow filter)
    {
        if (filter == null)
        {
            ClearQuery();
        }
        else
        {
            try
            {
                _ignoreQueryChange = true;
                foreach (
                    OrigamPanelFilter.PanelFilterDetailRow detail in filter.GetPanelFilterDetailRows()
                )
                {
                    if (_filterParts.ContainsKey(detail.ColumnName))
                    {
                        FilterPart part = _filterParts[detail.ColumnName] as FilterPart;
                        part.Operator = (FilterOperator)detail.Operator;
                        part.Value1 = OrigamPanelFilterDA.StoredFilterValue(
                            detail,
                            part.DataType,
                            1
                        );
                        part.Value2 = OrigamPanelFilterDA.StoredFilterValue(
                            detail,
                            part.DataType,
                            2
                        );
                        part.LoadValues();
                    }
                }
            }
            finally
            {
                _ignoreQueryChange = false;
                this.Query = GetQuery();
            }
        }
    }

    public void AddFilterDetails(
        OrigamPanelFilter filterDS,
        OrigamPanelFilter.PanelFilterRow filter,
        Guid profileId
    )
    {
        foreach (FilterPart part in _filterParts.Values)
        {
            if (part.Query != null)
            {
                OrigamPanelFilterDA.AddPanelFilterDetailRow(
                    filterDS,
                    profileId,
                    filter.Id,
                    part.GridColumnName,
                    (int)part.Operator,
                    part.Value1,
                    part.Value2
                );
            }
        }
    }
    #endregion
    #region Private Methods
    private void PlotControls()
    {
        this.SuspendLayout();
        foreach (FilterPart part in _filterParts.Values)
        {
            PlotControls(part);
        }
        this.ResumeLayout(true);
    }

    private void PlotControls(FilterPart part)
    {
        if (!this.Controls.Contains(part.LabelControl))
        {
            this.Controls.Add(part.LabelControl);
        }
        if (!this.Controls.Contains(part.OperatorLabelControl))
        {
            this.Controls.Add(part.OperatorLabelControl);
        }
        part.OperatorLabelControl.BringToFront();
        foreach (Control c in part.FilterControls)
        {
            if (!this.Controls.Contains(c))
            {
                c.Enter += new EventHandler(filterControl_Enter);
                this.Controls.Add(c);
            }
        }
    }

    private void HideControls(FilterPart part)
    {
        part.LabelControl.Hide();
        part.OperatorLabelControl.Hide();
        foreach (Control c in part.FilterControls)
        {
            c.Hide();
        }
    }

    private int SizeControls(
        FilterPart part,
        int left,
        int width,
        int startingTabIndex,
        ref int height
    )
    {
        part.LabelControl.Visible = true;
        part.LabelControl.Top = 2;
        part.LabelControl.Left = left;
        part.LabelControl.Width = width;
        part.OperatorLabelControl.Visible = true;
        part.OperatorLabelControl.Top = 16;
        part.OperatorLabelControl.Height = 14;
        part.OperatorLabelControl.Left = left;
        part.OperatorLabelControl.Width = width;

        int newHeight = part.OperatorLabelControl.Top + part.OperatorLabelControl.Height;
        if (newHeight > height)
        {
            height = newHeight;
        }

        int top = 30;
        foreach (Control c in part.FilterControls)
        {
            if ((bool)c.Tag)
            {
                c.Visible = true;
                c.Top = top;
                c.Left = left;
                c.Width = width;
                c.TabIndex = startingTabIndex;
                c.Enabled = true;
                c.BringToFront();
                top += c.Height;
                newHeight = c.Top + c.Height;
                if (newHeight > height)
                {
                    height = newHeight;
                }

                startingTabIndex++;
            }
            else
            {
                c.Visible = false;
            }
        }
        return startingTabIndex;
    }

    private void OnQueryChanged(string query)
    {
        if (this.QueryChanged != null)
        {
            this.QueryChanged(this, query);
        }
    }

    internal string GetQuery()
    {
        StringBuilder result = new StringBuilder();
        foreach (FilterPart part in _filterParts.Values)
        {
            if (part.Query != null & part.Query != "")
            {
                AsPanel panel = this.Parent as AsPanel;
                if (panel != null & !(part is DropDownFilterPart))
                {
                    // dynamicaly add temp sort column, e.g. if column is parametrized lookup
                    panel.GetSortColumn(part.GridColumnName);
                }
                if (result.Length > 0)
                {
                    result.Append(" AND ");
                }

                result.AppendFormat("({0})", part.Query);
            }
        }
        return result.ToString();
    }
    #endregion
    #region Event Handlers
    private void part_QueryChanged(object sender, string query)
    {
        if (_ignoreQueryChange)
        {
            return;
        }

        keyboardTimer.Enabled = false;
        keyboardTimer.Enabled = true;
    }

    private void part_ControlsChanged(object sender, EventArgs e)
    {
        SizeControls(_lastTableStyle, _lastOffset);
    }

    private void keyboardTimer_Tick(object sender, System.EventArgs e)
    {
        this.Query = GetQuery();
        keyboardTimer.Enabled = false;
    }

    private void FilterPanel_Enter(object sender, System.EventArgs e)
    {
        this.BackColor = OrigamColorScheme.FilterPanelActiveBackColor;
        if (this.ActiveControl == null)
        {
            // we activate the filter part on which the user clicked otherwise
            // the focus could be stolen by a grid, resulting in lost focus and
            // writing into another control
            Control child = this.GetChildAtPoint(this.PointToClient(MousePosition));
            if (child == null)
            {
                System.Diagnostics.Debug.WriteLine(this.PointToClient(MousePosition));
                child = this.Controls[0];
            }
            this.ActiveControl = child;
        }
        AsDataGrid grid = GetGrid();
        if (grid == null)
        {
            return;
        }
        grid.EnhancedFocusControl = false;
    }

    private void FilterPanel_Leave(object sender, System.EventArgs e)
    {
        this.BackColor = OrigamColorScheme.FilterPanelInactiveBackColor;
        AsDataGrid grid = GetGrid();
        if (grid == null)
        {
            return;
        }
        grid.EnhancedFocusControl = true;
    }
    #endregion
    private void filterControl_Enter(object sender, EventArgs e)
    {
        AsDataGrid grid = GetGrid();
        if (grid == null)
        {
            return;
        }

        Control c = sender as Control;
        if (c != null)
        {
            int maxWidth = c.Left + c.Width;
            int minLeft = c.Left - _lastTableStyle.RowHeaderWidth;
            if (maxWidth > this.Width)
            {
                _lastOffset = _lastOffset + maxWidth - this.Width;
            }
            else if (minLeft < 0)
            {
                _lastOffset = _lastOffset + minLeft;
            }
            else
            {
                return;
            }
            SizeControls(_lastTableStyle, this.ScrollOffset);
            grid.HorizontalScrollPosition = this.ScrollOffset;
            c.Focus();
        }
    }

    private AsDataGrid GetGrid()
    {
        AsDataGrid grid;
        if (_lastTableStyle == null)
        {
            grid = null;
        }
        else
        {
            grid = _lastTableStyle.DataGrid as AsDataGrid;
        }
        return grid;
    }
}
