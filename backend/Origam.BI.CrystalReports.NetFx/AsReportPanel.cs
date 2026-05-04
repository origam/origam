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
using System.Collections.Generic;
using System.ComponentModel;
//using System.Reflection;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using CrystalDecisions.CrystalReports.Engine;
using Origam.Gui.Win;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.UI;

namespace Origam.BI.CrystalReports;

/// <summary>
/// Summary description for AsReportPanel.
/// </summary>
///
[System.ComponentModel.Designer(designerType: typeof(ControlDesigner))]
[ToolboxBitmap(t: typeof(AsReportPanel))]
public class AsReportPanel
    : System.Windows.Forms.UserControl,
        IAsDataConsumer,
        IOrigamMetadataConsumer
{
    BindingManagerBase _bindMan = null;
    private DataSet _reportdata;
    private System.Windows.Forms.Button btnTisk;
    private CrystalDecisions.Windows.Forms.CrystalReportViewer crViewer;
    private Origam.BI.CrystalReports.ReportToolbar pnlToolbar;
    private System.Windows.Forms.Button btnInitialRefresh;
    private System.ComponentModel.IContainer components = null;

    public AsReportPanel()
    {
        // This call is required by the Windows.Forms Form Designer.
        InitializeComponent();

        SetButtons(small: this._buttonsOnly);
        pnlToolbar.StartColor = OrigamColorScheme.TitleInactiveStartColor;
        pnlToolbar.EndColor = OrigamColorScheme.TitleInactiveEndColor;
        pnlToolbar.ForeColor = OrigamColorScheme.TitleInactiveForeColor;
        pnlToolbar.MiddleStartColor = OrigamColorScheme.TitleInactiveMiddleStartColor;
        pnlToolbar.MiddleEndColor = OrigamColorScheme.TitleInactiveMiddleEndColor;
        this.BackColor = OrigamColorScheme.FormBackgroundColor;
        this.crViewer.BackColor = this.BackColor;
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.pnlToolbar.ReportRefreshRequested -= new System.EventHandler(
                this.pnlToolbar_ReportRefreshRequested
            );
            if (_bindMan != null)
            {
                _bindMan.PositionChanged -= new EventHandler(_bindMan_PositionChanged);
                _bindMan.CurrentChanged -= new EventHandler(_bindMan_CurrentChanged);
            }
            _bindMan = null;
            _origamMetadata = null;
            _dataSource = null;
            _parameterMappingCollection = null;
            _reportdata = null;
            ClearReportSource();
            if (components != null)
            {
                components.Dispose();
            }
        }
        base.Dispose(disposing: disposing);
    }

    #region Component Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.Resources.ResourceManager resources = new System.Resources.ResourceManager(
            typeof(AsReportPanel)
        );
        this.pnlToolbar = new Origam.BI.CrystalReports.ReportToolbar();
        this.crViewer = new CrystalDecisions.Windows.Forms.CrystalReportViewer();
        this.btnTisk = new System.Windows.Forms.Button();
        this.btnInitialRefresh = new System.Windows.Forms.Button();
        this.SuspendLayout();
        //
        // pnlToolbar
        //
        this.pnlToolbar.Caption = "";
        this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlToolbar.EndColor = System.Drawing.Color.FromArgb(
            ((System.Byte)(254)),
            ((System.Byte)(225)),
            ((System.Byte)(122))
        );
        this.pnlToolbar.Location = new System.Drawing.Point(0, 0);
        this.pnlToolbar.MiddleEndColor = System.Drawing.Color.FromArgb(
            ((System.Byte)(255)),
            ((System.Byte)(187)),
            ((System.Byte)(132))
        );
        this.pnlToolbar.MiddleStartColor = System.Drawing.Color.FromArgb(
            ((System.Byte)(255)),
            ((System.Byte)(171)),
            ((System.Byte)(63))
        );
        this.pnlToolbar.Name = "pnlToolbar";
        this.pnlToolbar.ReportViewer = this.crViewer;
        this.pnlToolbar.ShowRefreshButton = true;
        this.pnlToolbar.Size = new System.Drawing.Size(704, 24);
        this.pnlToolbar.StartColor = System.Drawing.Color.FromArgb(
            ((System.Byte)(255)),
            ((System.Byte)(217)),
            ((System.Byte)(170))
        );
        this.pnlToolbar.TabIndex = 4;
        this.pnlToolbar.ReportRefreshRequested += new System.EventHandler(
            this.pnlToolbar_ReportRefreshRequested
        );
        //
        // crViewer
        //
        this.crViewer.ActiveViewIndex = -1;
        this.crViewer.ToolPanelView = CrystalDecisions.Windows.Forms.ToolPanelViewType.None;
        this.crViewer.DisplayToolbar = false;
        this.crViewer.Location = new System.Drawing.Point(0, 208);
        this.crViewer.Name = "crViewer";
        this.crViewer.ReportSource = null;
        this.crViewer.Size = new System.Drawing.Size(500, 192);
        this.crViewer.TabIndex = 3;
        //
        // btnTisk
        //
        this.btnTisk.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.btnTisk.Image = ((System.Drawing.Image)(resources.GetObject("btnTisk.Image")));
        this.btnTisk.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.btnTisk.Location = new System.Drawing.Point(8, 80);
        this.btnTisk.Name = "btnTisk";
        this.btnTisk.Size = new System.Drawing.Size(88, 24);
        this.btnTisk.TabIndex = 1;
        this.btnTisk.Text = "Tisk";
        this.btnTisk.Click += new System.EventHandler(this.btnTisk_Click);
        //
        // btnInitialRefresh
        //
        this.btnInitialRefresh.Anchor = (
            (System.Windows.Forms.AnchorStyles)(
                (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)
            )
        );
        this.btnInitialRefresh.BackColor = System.Drawing.SystemColors.ControlLightLight;
        this.btnInitialRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.btnInitialRefresh.Image = (
            (System.Drawing.Image)(resources.GetObject("btnInitialRefresh.Image"))
        );
        this.btnInitialRefresh.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.btnInitialRefresh.Location = new System.Drawing.Point(232, 128);
        this.btnInitialRefresh.Name = "btnInitialRefresh";
        this.btnInitialRefresh.Size = new System.Drawing.Size(280, 32);
        this.btnInitialRefresh.TabIndex = 0;
        this.btnInitialRefresh.Text = "Z&obrazit sestavu";
        this.btnInitialRefresh.Click += new System.EventHandler(this.btnInitialRefresh_Click);
        //
        // AsReportPanel
        //
        this.Controls.Add(this.btnInitialRefresh);
        this.Controls.Add(this.crViewer);
        this.Controls.Add(this.pnlToolbar);
        this.Controls.Add(this.btnTisk);
        this.Name = "AsReportPanel";
        this.Size = new System.Drawing.Size(704, 400);
        this.SizeChanged += new System.EventHandler(this.AsReportPanel_SizeChanged);
        this.Enter += new System.EventHandler(this.AsReportPanel_Enter);
        this.Paint += new System.Windows.Forms.PaintEventHandler(this.AsReportPanel_Paint);
        this.Leave += new System.EventHandler(this.AsReportPanel_Leave);
        this.ResumeLayout(false);
    }
    #endregion
    #region Properties
    private bool _buttonsOnly = false;

    [Browsable(browsable: true)]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    public bool ButtonsOnly
    {
        get { return _buttonsOnly; }
        set
        {
            _buttonsOnly = value;
            SetButtons(small: value);
        }
    }
    private string _progressText = "";
    public string ProgressText
    {
        get { return _progressText; }
        set
        {
            _progressText = value;
            this.Invalidate();
            Application.DoEvents();
        }
    }

    private void SetButtons(bool small)
    {
        if (small)
        {
            pnlToolbar.Hide();
            crViewer.Hide();
            btnInitialRefresh.Hide();
            btnTisk.Show();
            btnTisk.Dock = DockStyle.Fill;

            //				if(this.DesignMode)
            //				{
            //					this.Height = 55;
            //					this.Width = 200;
            //				}
        }
        else
        {
            pnlToolbar.Show();
            btnInitialRefresh.Show();
            crViewer.Hide();
            btnTisk.Hide();
            crViewer.Dock = DockStyle.Fill;
            //				if(this.DesignMode)
            //				{
            //					this.Height = 400;
            //					this.Width  = 500;
            //				}
        }
        //			if(small)
        //			{
        //				btnRefreshX.Top = - 500;
        //				btnRefreshX.Left = - 500;
        //				btnTisk.Enabled =true;
        //				btnTisk.Visible =true;
        //				oldHeight=this.Height;
        //				oldWidth =this.Width;
        //				this.Height = 55;
        //				this.Width  = 200;
        //			}
        //			else
        //			{
        //				btnRefreshX.Top = 32;
        //				btnRefreshX.Left = 8;
        //
        //				btnTisk.Enabled =false;
        //				btnTisk.Visible =false;
        //
        //				this.crViewer.ShowRefreshButton = false;
        //				this.crViewer.DisplayGroupTree = false;
        //
        //
        //				if(oldHeight > 0 && oldWidth>0)
        //				{
        //
        //					this.Height = oldHeight;
        //					this.Width  = oldHeight;
        //				}
        //				else
        //				{
        //					this.Height = 400;
        //					this.Width  = 500;
        //				}
        //			}
    }

    public DataSet ReportData
    {
        get { return _reportdata; }
    }
    private bool _itemsLoaded = false;
    private bool _fillingParameterCache = false;

    private void FillParameterCache(ControlSetItem controlItem)
    {
        if (controlItem == null)
        {
            return;
        }

        _fillingParameterCache = true;
        ParameterMappings.Clear();

        foreach (
            var mapInfo in controlItem.ChildItemsByType<ColumnParameterMapping>(
                itemType: ColumnParameterMapping.CategoryConst
            )
        )
        {
            if (!mapInfo.IsDeleted) // skip any deleted mapping infos
            {
                ParameterMappings.Add(value: mapInfo);
            }
        }
        _fillingParameterCache = false;
    }

    private object _dataSource;
    private string _dataMember;

    [Category(category: "Data")]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [TypeConverter(
        typeName: "System.Windows.Forms.Design.DataSourceConverter, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
    )]
    public object DataSource
    {
        get { return _dataSource; }
        set
        {
            _dataSource = ConvertInputData(data: value);
            SetDataBinding(dataSource: _dataSource, dataMember: _dataMember);
        }
    }

    [Category(category: "Data")]
    [Editor(
        typeName: "System.Windows.Forms.Design.DataMemberListEditor, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
        baseType: typeof(System.Drawing.Design.UITypeEditor)
    )]
    public string DataMember
    {
        get { return _dataMember; }
        set { _dataMember = value; }
    }
    private Guid _reportId;

    [Browsable(browsable: false)]
    public Guid ReportId
    {
        get { return _reportId; }
        set
        {
            _reportId = value;
            SetReportText();
        }
    }

    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [TypeConverter(type: typeof(ReportConverter))]
    public CrystalReport CrystalReport
    {
        get
        {
            if (this._origamMetadata == null)
            {
                return null;
            }

            return (CrystalReport)
                this._origamMetadata.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.ReportId)
                );
        }
        set
        {
            if (value == null)
            {
                this.ReportId = Guid.Empty;
                ClearMappingItemsOnly();
            }
            else
            {
                //if newly added Crystal Report are the same as actually asigned,
                //no reaction provided
                if (this.ReportId == (Guid)value.PrimaryKey[key: "Id"])
                {
                    return;
                }
                this.ReportId = (Guid)value.PrimaryKey[key: "Id"];
                //ClearMappingItemsOnly();
                CreateMappingItemsCollection();
            }
        }
    }

    private void SetReportText()
    {
        if (this.CrystalReport == null)
        {
            pnlToolbar.Caption = "";
            btnTisk.Text = "";
        }
        else
        {
            pnlToolbar.Caption = (
                this.CrystalReport.Caption == null | this.CrystalReport.Caption == ""
                    ? this.CrystalReport.Name
                    : this.CrystalReport.Caption
            );
            btnTisk.Text = (
                this.CrystalReport.Caption == null | this.CrystalReport.Caption == ""
                    ? this.CrystalReport.Name
                    : this.CrystalReport.Caption
            );
        }
    }

    private void ClearMappingItemsOnly()
    {
        try
        {
            if (!_itemsLoaded)
            {
                return;
            }

            var col = _origamMetadata
                .ChildItemsByType<ColumnParameterMapping>(
                    itemType: ColumnParameterMapping.CategoryConst
                )
                .ToList();
            foreach (ColumnParameterMapping mapping in col)
            {
                mapping.IsDeleted = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(message: "AsReportPanel:ERROR=>" + ex.ToString());
        }
    }

    private void CreateMappingItemsCollection()
    {
        if (this.CrystalReport == null)
        {
            return;
        }
        // create any missing parameter mappings
        foreach (var entry in this.CrystalReport.ParameterReferences)
        {
            string parameterName = entry.Key;
            if (this._origamMetadata.GetChildByName(name: parameterName) == null)
            {
                ColumnParameterMapping mapping = _origamMetadata.NewItem<ColumnParameterMapping>(
                    schemaExtensionId: _origamMetadata.SchemaExtensionId,
                    group: null
                );
                mapping.Name = parameterName;
            }
        }
        // create any missing report's own parameters
        foreach (
            var param in CrystalReport.ChildItemsByType<SchemaItemParameter>(
                itemType: SchemaItemParameter.CategoryConst
            )
        )
        {
            if (this._origamMetadata.GetChildByName(name: param.Name) == null)
            {
                ColumnParameterMapping mapping = _origamMetadata.NewItem<ColumnParameterMapping>(
                    schemaExtensionId: _origamMetadata.SchemaExtensionId,
                    group: null
                );
                mapping.Name = param.Name;
            }
        }
        var toDelete = new List<ISchemaItem>();
        // delete all parameter mappings from the report, if they do not exist in the data structure anymore
        foreach (
            var mapping in _origamMetadata.ChildItemsByType<ColumnParameterMapping>(
                itemType: ColumnParameterMapping.CategoryConst
            )
        )
        {
            if (
                !this.CrystalReport.ParameterReferences.ContainsKey(key: mapping.Name)
                & this.CrystalReport.GetChildByName(name: mapping.Name) == null
            )
            {
                toDelete.Add(item: mapping);
            }
        }
        foreach (ISchemaItem mapping in toDelete)
        {
            mapping.IsDeleted = true;
        }
        //Refill Parameter collection (and dictionary)
        FillParameterCache(controlItem: this._origamMetadata as ControlSetItem);
    }
    #endregion
    #region Methods
    public void SetDataBinding(object dataSource, string dataMember)
    {
        if (dataSource == null || dataMember == null || dataMember == "" || this.DesignMode)
        {
            return;
        }

        _dataSource = dataSource;
        _dataMember = (null == dataMember) ? "" : dataMember;
        if (_bindMan != null)
        {
            _bindMan.PositionChanged -= new EventHandler(_bindMan_PositionChanged);
            _bindMan.CurrentChanged -= new EventHandler(_bindMan_CurrentChanged);
        }
        _bindMan = GetBindingManager();
        if (_bindMan != null)
        {
            _bindMan.PositionChanged += new EventHandler(_bindMan_PositionChanged);
            _bindMan.CurrentChanged += new EventHandler(_bindMan_CurrentChanged);
        }
        //TODO dodelat update tlacitek
        // kontrolovata jesli je k dispozici report udelat datastructure...
    }
    #endregion
    #region Private members
    private BindingManagerBase GetBindingManager()
    {
        if (null != _dataSource && null != BindingContext)
        {
            return BindingContext[dataSource: _dataSource, dataMember: _dataMember];
        }

        return null;
    }

    private DataSet ConvertInputData(object data)
    {
        //all other datasources has to be implemented
        // DataTable, DataView, DataSet, DataViewManager,
        // IListSource, IList
        if (data is DataSet)
        {
            return (DataSet)data;
        }

        return null;
    }
    #endregion
    /// <summary>
    /// When Report DataStructure is changer then reloaded
    /// paremeter chache (parameter chache contains ColumnMapping
    /// </summary>
    /// <param name="query"></param>
    private Hashtable GetParameters(CurrencyManager cm)
    {
        if (cm == null)
        {
            throw new NullReferenceException(
                message: Origam.BI.CrystalReports.ResourceUtils.GetString(
                    key: "ErrorInvalidReportSource"
                )
            );
        }

        Hashtable result = new Hashtable();

        if (ParameterMappings.Count == 0)
        {
            return result;
        }

        if (cm.Position < 0)
        {
            return result;
        }

        DataRowView drv = cm.Current as DataRowView;
        foreach (ColumnParameterMapping colMap in ParameterMappings)
        {
            if (colMap.ColumnName != "" && colMap.ColumnName != null) // maybe the parameter was not bound, we let report service to try the default value
            {
                if (drv.Row.Table.Columns.Contains(name: colMap.ColumnName))
                {
                    result.Add(key: colMap.Name, value: drv.Row[columnName: colMap.ColumnName]);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(
                        paramName: "ColumnName",
                        actualValue: colMap.ColumnName,
                        message: Origam.BI.CrystalReports.ResourceUtils.GetString(
                            key: "ErrorBindParam"
                        )
                    );
                }
            }
        }
        return result;
    }

    private void btnTisk_Click(object sender, System.EventArgs e)
    {
        PrintReport();
    }

    private void PrintReport()
    {
        try
        {
            this.Cursor = Cursors.WaitCursor;
            ReportViewer frm = new ReportViewer();

            frm.TitleName = this.CrystalReport.Caption;
            frm.ReportSource = CreateReport(report: this.CrystalReport);
            Origam.Workbench.WorkbenchSingleton.Workbench.ShowView(content: frm);
        }
        catch (Exception ex)
        {
            Origam.UI.AsMessageBox.ShowError(
                owner: this.FindForm(),
                text: ex.Message,
                caption: Origam.BI.CrystalReports.ResourceUtils.GetString(
                    key: "ErrorReportShow",
                    args: this.CrystalReport.Name
                ),
                exception: ex
            );
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }

    private ReportDocument CreateReport(CrystalReport report)
    {
        if (
            ParameterMappings.Count > 0
            && !(_bindMan != null && _bindMan.Count > 0 && _bindMan.Position >= 0)
        )
        {
            SetButtons(small: this._buttonsOnly);
            throw new InvalidOperationException(
                message: Origam.BI.CrystalReports.ResourceUtils.GetString(
                    key: "ErrorReportNoRecord0"
                )
                    + Environment.NewLine
                    + Environment.NewLine
                    + Origam.BI.CrystalReports.ResourceUtils.GetString(key: "ErrorReportNoRecord1")
                    + Environment.NewLine
                    + Origam.BI.CrystalReports.ResourceUtils.GetString(key: "ErrorReportNoRecord2")
                    + Environment.NewLine
                    + Origam.BI.CrystalReports.ResourceUtils.GetString(key: "ErrorReportNoRecord3")
                    + Environment.NewLine
                    + Origam.BI.CrystalReports.ResourceUtils.GetString(key: "ErrorReportNoRecord4")
            );
        }

        CrystalReportHelper helper = new CrystalReportHelper();
        Hashtable parameters = GetParameters(cm: _bindMan as CurrencyManager);
        return helper.CreateReport(
            reportId: report.Id,
            parameters: parameters,
            transactionId: null
        );
    }

    private void btnRefresh_Click(object sender, System.EventArgs e)
    {
        if (this.btnInitialRefresh.Visible)
        {
            btnInitialRefresh_Click(sender: this, e: EventArgs.Empty);
        }
        else
        {
            RefreshReport();
        }
    }

    private void RefreshReport()
    {
        if (this.CrystalReport == null)
        {
            Origam.UI.AsMessageBox.ShowError(
                owner: this.FindForm(),
                text: Origam.BI.CrystalReports.ResourceUtils.GetString(
                    key: "ErrorReportPanelDefinition"
                ),
                caption: Origam.BI.CrystalReports.ResourceUtils.GetString(
                    key: "ReportShowTitle",
                    args: this.FindForm().Text
                ),
                exception: null
            );
            return;
        }
        try
        {
            this.Cursor = Cursors.WaitCursor;
            btnInitialRefresh.Hide();
            crViewer.Hide();
            Application.DoEvents();
            this.ShowProgress();
            ClearReportSource();
            object reportSource = CreateReport(report: this.CrystalReport);
            crViewer.Location = new Point(x: 0, y: 0);
            crViewer.Size = this.Size;
            crViewer.Show();
            this.crViewer.ReportSource = reportSource;
            this.crViewer.Zoom(ZoomLevel: 1);
            this.crViewer.Focus();
        }
        catch (Exception ex)
        {
            Origam.UI.AsMessageBox.ShowError(
                owner: this.FindForm(),
                text: ex.Message,
                caption: Origam.BI.CrystalReports.ResourceUtils.GetString(
                    key: "ErrorReportRefresh",
                    args: this.CrystalReport.Name
                ),
                exception: ex
            );
        }
        finally
        {
            this.HideProgress();
            this.Cursor = Cursors.Default;
        }
    }

    #region IColumnParameterMappingConsumer Members
    private ColumnParameterMappingCollection _parameterMappingCollection =
        new ColumnParameterMappingCollection();

    [TypeConverter(type: typeof(ColumnParameterMappingCollectionConverter))]
    public ColumnParameterMappingCollection ParameterMappings
    {
        get
        {
            if (!_fillingParameterCache)
            {
                CreateMappingItemsCollection();
            }
            return _parameterMappingCollection;
        }
    }
    #endregion
    #region IOrigamMetadataConsumer Members
    private ISchemaItem _origamMetadata;

    private void pnlToolbar_ReportRefreshRequested(object sender, System.EventArgs e)
    {
        this.RefreshReport();
    }

    private void AsReportPanel_Enter(object sender, System.EventArgs e)
    {
        this.pnlToolbar.StartColor = OrigamColorScheme.TitleActiveStartColor;
        this.pnlToolbar.MiddleStartColor = OrigamColorScheme.TitleActiveMiddleStartColor;
        this.pnlToolbar.MiddleEndColor = OrigamColorScheme.TitleActiveMiddleEndColor;
        this.pnlToolbar.EndColor = OrigamColorScheme.TitleActiveEndColor;
        this.pnlToolbar.ForeColor = OrigamColorScheme.TitleActiveForeColor;
        if (this.DesignMode == false & (crViewer.Visible | btnInitialRefresh.Visible))
        {
            if (btnInitialRefresh.Visible)
            {
                btnInitialRefresh.Focus();
            }
            else
            {
                crViewer.Focus();
            }
        }
    }

    private void AsReportPanel_Leave(object sender, System.EventArgs e)
    {
        pnlToolbar.StartColor = OrigamColorScheme.TitleInactiveStartColor;
        pnlToolbar.EndColor = OrigamColorScheme.TitleInactiveEndColor;
        pnlToolbar.ForeColor = OrigamColorScheme.TitleInactiveForeColor;
        pnlToolbar.MiddleStartColor = OrigamColorScheme.TitleInactiveMiddleStartColor;
        pnlToolbar.MiddleEndColor = OrigamColorScheme.TitleInactiveMiddleEndColor;
    }

    public ISchemaItem OrigamMetadata
    {
        get { return _origamMetadata; }
        set
        {
            _origamMetadata = value;
            _itemsLoaded = true;
            FillParameterCache(controlItem: _origamMetadata as ControlSetItem);
            SetReportText();
        }
    }
    #endregion
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (pnlToolbar.ProcessCommand(msg: ref msg, keyData: keyData))
        {
            return true;
        }
        return base.ProcessCmdKey(msg: ref msg, keyData: keyData);
    }

    private void btnInitialRefresh_Click(object sender, System.EventArgs e)
    {
        this.RefreshReport();
    }

    private void AsReportPanel_SizeChanged(object sender, System.EventArgs e)
    {
        btnInitialRefresh.SetBounds(
            x: (this.Width / 2) - (btnInitialRefresh.Width / 2),
            y: (this.Height / 2) - (btnInitialRefresh.Height / 2),
            width: btnInitialRefresh.Width,
            height: btnInitialRefresh.Height,
            specified: BoundsSpecified.Location
        );
    }

    private void _bindMan_PositionChanged(object sender, EventArgs e)
    {
        ClearReportSource();
        SetButtons(small: this._buttonsOnly);
    }

    private void _bindMan_CurrentChanged(object sender, EventArgs e)
    {
        ClearReportSource();
        SetButtons(small: this._buttonsOnly);
    }

    MRG.Controls.UI.LoadingCircle circ = new MRG.Controls.UI.LoadingCircle();

    private void ShowProgress()
    {
        this.ProgressText = "Naèítá se tisková sestava...";
        Control parent = this;
        int top = 0;
        int left = 0;
        top = parent.Height / 3;
        left = (parent.Width / 2) - (16);
        circ.Top = top;
        circ.Left = left;
        circ.Height = 32;
        circ.Width = 32;
        circ.RotationSpeed = 100;
        circ.ParentControl = parent;
        circ.Color = OrigamColorScheme.FormLoadingStatusColor;
        circ.StylePreset = MRG.Controls.UI.LoadingCircle.StylePresets.MacOSX;
        circ.Active = true;
    }

    private void HideProgress()
    {
        this.ProgressText = "";
        if (!circ.Active)
        {
            return;
        }

        circ.Active = false;
        circ.ParentControl.Invalidate();
        circ.ParentControl = null;
    }

    private void ClearReportSource()
    {
        IDisposable rs = this.crViewer.ReportSource as IDisposable;

        if (rs != null)
        {
            // sometimes this throws an exception
            try
            {
                this.crViewer.ReportSource = null;
            }
            catch { }
            Form f = this.FindForm();
            if (f != null)
            {
                f.SuspendLayout();
            }
            rs.Dispose();
            if (f != null)
            {
                f.ResumeLayout(performLayout: false);
            }
        }
    }

    private void AsReportPanel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
    {
        try
        {
            System.Drawing.Graphics g = e.Graphics;
            string text = this.ProgressText;
            System.Drawing.Font font = new System.Drawing.Font(
                family: this.Font.FontFamily,
                emSize: 10,
                style: System.Drawing.FontStyle.Bold
            );
            float stringWidth = g.MeasureString(text: text, font: font).Width;
            float stringHeight = g.MeasureString(text: text, font: font).Height;
            g.Clear(color: this.BackColor);
            g.DrawString(
                s: text,
                font: font,
                brush: new System.Drawing.SolidBrush(
                    color: OrigamColorScheme.FormLoadingStatusColor
                ),
                x: (this.Width / 2) - (stringWidth / 2),
                y: (this.Height / 3) + 64
            );
        }
        catch { }
    }
}
