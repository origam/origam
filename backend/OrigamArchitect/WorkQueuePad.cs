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
using System.Threading;
using System.Windows.Forms;
using Origam;
using Origam.Schema.WorkflowModel;
using Origam.UI;
using Origam.Workbench;
using Origam.Workbench.Services;
using WeifenLuo.WinFormsUI.Docking;

namespace OrigamArchitect;

/// <summary>
/// Summary description for WorkQueuePad.
/// </summary>
public class WorkQueuePad : AbstractPadContent
{
    private System.Windows.Forms.Timer WQTimer;
    private System.ComponentModel.IContainer components;
    private System.Windows.Forms.DataGridTableStyle WorkQueue;
    private System.Windows.Forms.DataGridTextBoxColumn colReferenceCode;
    private System.Windows.Forms.DataGridTextBoxColumn colName;
    private System.Windows.Forms.DataGridTextBoxColumn colCntTotal;
    private System.Windows.Forms.ToolBar toolBar1;
    private System.Windows.Forms.ImageList imageList1;
    private System.Windows.Forms.ToolBarButton btnShow;
    private System.Windows.Forms.ToolBarButton btnRefresh;
    private System.Windows.Forms.DataGrid dataGrid1;
    private System.Windows.Forms.Label errorLabel;

    private DataSet _data = new DataSet();

    public WorkQueuePad()
    {
        InitializeComponent();
        this.BackColor = OrigamColorScheme.FormBackgroundColor;
        dataGrid1.BackgroundColor = OrigamColorScheme.FormBackgroundColor;
        WorkQueue.AlternatingBackColor = OrigamColorScheme.GridAlternatingBackColor;
        WorkQueue.ForeColor = OrigamColorScheme.GridForeColor;
        WorkQueue.GridLineColor = OrigamColorScheme.GridLineColor;
        WorkQueue.GridLineStyle = System.Windows.Forms.DataGridLineStyle.Solid;
        WorkQueue.HeaderBackColor = OrigamColorScheme.GridHeaderBackColor;
        WorkQueue.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
        WorkQueue.HeaderForeColor = OrigamColorScheme.GridHeaderForeColor;
        WorkQueue.SelectionBackColor = OrigamColorScheme.GridSelectionBackColor;
        WorkQueue.SelectionForeColor = OrigamColorScheme.GridSelectionForeColor;
        SchemaService schema =
            ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
        schema.SchemaLoaded += schema_SchemaLoaded;
    }

    #region Windows Form Designer generated code
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources =
            new System.ComponentModel.ComponentResourceManager(typeof(WorkQueuePad));
        this.dataGrid1 = new System.Windows.Forms.DataGrid();
        this.WorkQueue = new System.Windows.Forms.DataGridTableStyle();
        this.colReferenceCode = new System.Windows.Forms.DataGridTextBoxColumn();
        this.colName = new System.Windows.Forms.DataGridTextBoxColumn();
        this.colCntTotal = new System.Windows.Forms.DataGridTextBoxColumn();
        this.WQTimer = new System.Windows.Forms.Timer(this.components);
        this.toolBar1 = new System.Windows.Forms.ToolBar();
        this.btnShow = new System.Windows.Forms.ToolBarButton();
        this.btnRefresh = new System.Windows.Forms.ToolBarButton();
        this.imageList1 = new System.Windows.Forms.ImageList(this.components);
        this.errorLabel = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
        this.SuspendLayout();
        //
        // dataGrid1
        //
        this.dataGrid1.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.dataGrid1.CaptionVisible = false;
        this.dataGrid1.DataMember = "";
        this.dataGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dataGrid1.FlatMode = true;
        this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.dataGrid1.Location = new System.Drawing.Point(8, 26);
        this.dataGrid1.Name = "dataGrid1";
        this.dataGrid1.ParentRowsVisible = false;
        this.dataGrid1.ReadOnly = true;
        this.dataGrid1.RowHeadersVisible = false;
        this.dataGrid1.Size = new System.Drawing.Size(936, 394);
        this.dataGrid1.TabIndex = 0;
        this.dataGrid1.TableStyles.AddRange(
            new System.Windows.Forms.DataGridTableStyle[] { this.WorkQueue }
        );
        //
        // WorkQueue
        //
        this.WorkQueue.DataGrid = this.dataGrid1;
        this.WorkQueue.GridColumnStyles.AddRange(
            new System.Windows.Forms.DataGridColumnStyle[]
            {
                this.colReferenceCode,
                this.colName,
                this.colCntTotal,
            }
        );
        this.WorkQueue.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.WorkQueue.MappingName = "WorkQueue";
        this.WorkQueue.RowHeadersVisible = false;
        //
        // colReferenceCode
        //
        this.colReferenceCode.Format = "";
        this.colReferenceCode.FormatInfo = null;
        this.colReferenceCode.HeaderText = global::OrigamArchitect.strings.Code_TableColumn;
        this.colReferenceCode.MappingName = "ReferenceCode";
        this.colReferenceCode.Width = 60;
        //
        // colName
        //
        this.colName.Format = "";
        this.colName.FormatInfo = null;
        this.colName.HeaderText = global::OrigamArchitect.strings.Name_TableColumn;
        this.colName.MappingName = "Name";
        this.colName.Width = 150;
        //
        // colCntTotal
        //
        this.colCntTotal.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
        this.colCntTotal.Format = "";
        this.colCntTotal.FormatInfo = null;
        this.colCntTotal.HeaderText = global::OrigamArchitect.strings.Count_TableColumn;
        this.colCntTotal.MappingName = "CntTotal";
        this.colCntTotal.NullText = "";
        this.colCntTotal.Width = 50;
        //
        // WQTimer
        //
        this.WQTimer.Enabled = true;
        this.WQTimer.Interval = 60000;
        this.WQTimer.Tick += new System.EventHandler(this.WQTimer_Tick);
        //
        // toolBar1
        //
        this.toolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
        this.toolBar1.Buttons.AddRange(
            new System.Windows.Forms.ToolBarButton[] { this.btnShow, this.btnRefresh }
        );
        this.toolBar1.Divider = false;
        this.toolBar1.DropDownArrows = true;
        this.toolBar1.ImageList = this.imageList1;
        this.toolBar1.Location = new System.Drawing.Point(8, 0);
        this.toolBar1.Name = "toolBar1";
        this.toolBar1.ShowToolTips = true;
        this.toolBar1.Size = new System.Drawing.Size(936, 26);
        this.toolBar1.TabIndex = 1;
        this.toolBar1.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
        this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(
            this.toolBar1_ButtonClick
        );
        //
        // btnShow
        //
        this.btnShow.ImageIndex = 0;
        this.btnShow.Name = "btnShow";
        this.btnShow.Text = global::OrigamArchitect.strings.Open_Button;
        //
        // btnRefresh
        //
        this.btnRefresh.ImageIndex = 1;
        this.btnRefresh.Name = "btnRefresh";
        this.btnRefresh.Text = global::OrigamArchitect.strings.Refresh_Button;
        //
        // imageList1
        //
        this.imageList1.ImageStream = (
            (System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream"))
        );
        this.imageList1.TransparentColor = System.Drawing.Color.Magenta;
        this.imageList1.Images.SetKeyName(0, "");
        this.imageList1.Images.SetKeyName(1, "");
        //
        // errorLabel
        //
        this.errorLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.errorLabel.Location = new System.Drawing.Point(192, 30);
        this.errorLabel.Name = "errorLabel";
        this.errorLabel.Size = new System.Drawing.Size(144, 52);
        this.errorLabel.TabIndex = 2;
        this.errorLabel.Text = "error text";
        this.errorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        this.errorLabel.Visible = false;
        //
        // WorkQueuePad
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
        this.ClientSize = new System.Drawing.Size(944, 420);
        this.Controls.Add(this.errorLabel);
        this.Controls.Add(this.dataGrid1);
        this.Controls.Add(this.toolBar1);
        this.DockAreas = (
            (WeifenLuo.WinFormsUI.Docking.DockAreas)(
                (
                    (
                        (
                            (
                                WeifenLuo.WinFormsUI.Docking.DockAreas.Float
                                | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft
                            ) | WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight
                        ) | WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop
                    ) | WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom
                )
            )
        );
        this.Font = new System.Drawing.Font(
            "Microsoft Sans Serif",
            7.8F,
            System.Drawing.FontStyle.Regular,
            System.Drawing.GraphicsUnit.Point,
            ((byte)(0))
        );
        this.HideOnClose = true;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "WorkQueuePad";
        this.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
        this.ShowInTaskbar = false;
        this.TabText = global::OrigamArchitect.strings.Massages_TabText;
        this.Text = "Work Queues";
        ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
    #endregion
    private void RefreshData()
    {
        try
        {
            this.errorLabel.Hide();
            this.dataGrid1.Show();
            OrigamSettings settings = (OrigamSettings)ConfigurationManager.GetActiveConfiguration();
            if (settings == null)
                return;
            if (settings.WorkQueueListRefreshPeriod == 0)
            {
                WQTimer.Enabled = false;
            }
            else
            {
                WQTimer.Interval = settings.WorkQueueListRefreshPeriod * 1000;
            }
            IWorkQueueService wqs =
                ServiceManager.Services.GetService(typeof(IWorkQueueService)) as IWorkQueueService;
            if (wqs == null)
                return;
            SchemaService schema =
                ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
            if (!schema.IsSchemaLoaded)
                return;
            if (_data.Tables.Count == 0)
            {
                _data.Merge(wqs.UserQueueList());
            }
            else
            {
                Origam.DA.MergeParams mergeParams = new Origam.DA.MergeParams();
                mergeParams.TrueDelete = true;
                Origam.DA.DatasetTools.MergeDataSet(_data, wqs.UserQueueList(), null, mergeParams);
                _data.AcceptChanges();
            }
            if (dataGrid1.DataSource == null)
            {
                dataGrid1.DataSource = _data;
                dataGrid1.DataMember = "WorkQueue";
            }
            Thread t = new Thread(new ThreadStart(GetCounts));
            t.IsBackground = true;
            t.Start();
        }
        catch (Exception ex)
        {
            this.dataGrid1.Hide();
            errorLabel.Text = ex.Message;
            this.errorLabel.Dock = DockStyle.Fill;
            this.errorLabel.Show();
        }
    }

    private void GetCounts()
    {
        try
        {
            IDataLookupService ls =
                ServiceManager.Services.GetService(typeof(IDataLookupService))
                as IDataLookupService;
            IWorkQueueService wqs =
                ServiceManager.Services.GetService(typeof(IWorkQueueService)) as IWorkQueueService;
            foreach (DataRow row in _data.Tables[0].Rows)
            {
                object id = null;
                string wqClassName = null;

                lock (_data)
                {
                    if (row.RowState != DataRowState.Deleted)
                    {
                        id = row["Id"];
                        wqClassName = (string)row["WorkQueueClass"];
                    }
                }
                if (id != null)
                {
                    WorkQueueClass wqc = wqs.WQClass(wqClassName) as WorkQueueClass;
                    if (wqc != null && wqc.WorkQueueItemCountLookup != null)
                    {
                        long cnt = (long)
                            ls.GetDisplayText(
                                wqc.WorkQueueItemCountLookupId,
                                id,
                                false,
                                false,
                                null
                            );
                        lock (_data)
                        {
                            row["CntTotal"] = cnt;
                        }
                    }
                }
            }
        }
        catch { }
    }

    private void WQTimer_Tick(object sender, System.EventArgs e)
    {
        RefreshData();
    }

    private void toolBar1_ButtonClick(
        object sender,
        System.Windows.Forms.ToolBarButtonClickEventArgs e
    )
    {
        try
        {
            if (e.Button == btnShow)
            {
                CurrencyManager cm = this.BindingContext[_data, "WorkQueue"] as CurrencyManager;
                if (cm.Position >= 0)
                {
                    DataRow row = (cm.Current as DataRowView).Row;
                    // First we test, if the item is not opened already
                    foreach (
                        IViewContent content in WorkbenchSingleton.Workbench.ViewContentCollection
                    )
                    {
                        if (content.DisplayedItemId.Equals(row["Id"]))
                        {
                            (content as DockContent).Activate();
                            return;
                        }
                    }
                    WorkQueueWindow queueWindow = new WorkQueueWindow();
                    queueWindow.TitleName = (string)row["Name"];
                    queueWindow.QueueId = (Guid)row["Id"];
                    queueWindow.QueueClass = (string)row["WorkQueueClass"];
                    queueWindow.DisplayedItemId = Guid.Parse(row["Id"].ToString());
                    WorkbenchSingleton.Workbench.ShowView(queueWindow);
                    queueWindow.LoadQueue();
                }
            }
            else if (e.Button == btnRefresh)
            {
                RefreshData();
            }
        }
        catch (Exception ex)
        {
            AsMessageBox.ShowError(this, ex.Message, strings.WorkQueueCommand_Title, ex);
        }
    }

    private void schema_SchemaLoaded(object sender, bool isInteractive)
    {
        RefreshData();
    }
}
