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
using System.Windows.Forms;
using Origam;
using Origam.Gui;
using Origam.Gui.Win;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.WorkflowModel;
using Origam.UI;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace OrigamArchitect;

/// <summary>
/// Summary description for WorkQueuePad.
/// </summary>
public class WorkQueueWindow : AsForm
{
    private Origam.Gui.Win.CollapsibleSplitter collapsibleSplitter1;
    private System.Windows.Forms.Panel panel1;
    private Origam.Gui.Win.AsPanelTitle label2;
    private System.Windows.Forms.Label lblTitle;
    private AsPanel _dataPanel;

    public WorkQueueWindow()
    {
        InitializeComponent();
        this.BackColor = OrigamColorScheme.FormBackgroundColor;
        this.label2.StartColor = OrigamColorScheme.TitleActiveStartColor;
        this.label2.EndColor = OrigamColorScheme.TitleActiveEndColor;
        this.label2.MiddleStartColor = OrigamColorScheme.TitleActiveMiddleStartColor;
        this.label2.MiddleEndColor = OrigamColorScheme.TitleActiveMiddleEndColor;
        this.label2.ForeColor = OrigamColorScheme.TitleActiveForeColor;
        this.TitleNameChanged += new EventHandler(WorkQueueWindow_TitleNameChanged);
        this.collapsibleSplitter1.BackColor = OrigamColorScheme.SplitterBackColor;
        Console.WriteLine(strings.Commands_PanelTitle);
    }

    #region Windows Form Designer generated code
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources =
            new System.ComponentModel.ComponentResourceManager(typeof(WorkQueueWindow));
        this.collapsibleSplitter1 = new Origam.Gui.Win.CollapsibleSplitter();
        this.panel1 = new System.Windows.Forms.Panel();
        this.label2 = new Origam.Gui.Win.AsPanelTitle();
        this.lblTitle = new System.Windows.Forms.Label();
        this.panel1.SuspendLayout();
        this.SuspendLayout();
        //
        // collapsibleSplitter1
        //
        this.collapsibleSplitter1.AnimationDelay = 20;
        this.collapsibleSplitter1.AnimationStep = 20;
        this.collapsibleSplitter1.BorderStyle3D = System.Windows.Forms.Border3DStyle.Flat;
        this.collapsibleSplitter1.ControlToHide = this.panel1;
        this.collapsibleSplitter1.ExpandParentForm = false;
        this.collapsibleSplitter1.Location = new System.Drawing.Point(192, 35);
        this.collapsibleSplitter1.Name = "collapsibleSplitter1";
        this.collapsibleSplitter1.TabIndex = 0;
        this.collapsibleSplitter1.TabStop = false;
        this.collapsibleSplitter1.UseAnimations = false;
        this.collapsibleSplitter1.VisualStyle = Origam.Gui.Win.VisualStyles.XP;
        //
        // panel1
        //
        this.panel1.BackColor = System.Drawing.Color.White;
        this.panel1.Controls.Add(this.label2);
        this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
        this.panel1.Location = new System.Drawing.Point(0, 35);
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size(192, 385);
        this.panel1.TabIndex = 1;
        //
        // label2
        //
        this.label2.BackColor = System.Drawing.Color.Orange;
        this.label2.Dock = System.Windows.Forms.DockStyle.Top;
        this.label2.EndColor = System.Drawing.Color.Empty;
        this.label2.ForeColor = System.Drawing.Color.White;
        this.label2.Location = new System.Drawing.Point(0, 0);
        this.label2.MiddleEndColor = System.Drawing.Color.Empty;
        this.label2.MiddleStartColor = System.Drawing.Color.Empty;
        this.label2.Name = "label2";
        this.label2.PanelIcon = null;
        this.label2.PanelTitle = strings.Commands_PanelTitle;
        this.label2.Size = new System.Drawing.Size(192, 28);
        this.label2.StartColor = System.Drawing.Color.Empty;
        this.label2.StatusIcon = null;
        this.label2.TabIndex = 0;
        //
        // lblTitle
        //
        this.lblTitle.BackColor = System.Drawing.Color.Transparent;
        this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblTitle.Font = new System.Drawing.Font(
            "Microsoft Sans Serif",
            14.25F,
            System.Drawing.FontStyle.Bold,
            System.Drawing.GraphicsUnit.Point,
            ((byte)(238))
        );
        this.lblTitle.Location = new System.Drawing.Point(0, 0);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new System.Drawing.Size(944, 35);
        this.lblTitle.TabIndex = 2;
        this.lblTitle.Text = "label1";
        //
        // WorkQueueWindow
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
        this.ClientSize = new System.Drawing.Size(944, 420);
        this.Controls.Add(this.collapsibleSplitter1);
        this.Controls.Add(this.panel1);
        this.Controls.Add(this.lblTitle);
        this.Font = new System.Drawing.Font(
            "Microsoft Sans Serif",
            7.8F,
            System.Drawing.FontStyle.Regular,
            System.Drawing.GraphicsUnit.Point,
            ((byte)(0))
        );
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "WorkQueueWindow";
        this.ShowInTaskbar = false;
        this.panel1.ResumeLayout(false);
        this.ResumeLayout(false);
    }
    #endregion
    private void WorkQueueWindow_TitleNameChanged(object sender, EventArgs e)
    {
        lblTitle.Text = this.TitleName;
    }

    private Guid _workQueueId;
    public Guid QueueId
    {
        get { return _workQueueId; }
        set { _workQueueId = value; }
    }
    private string _workQueueClass;
    public string QueueClass
    {
        get { return _workQueueClass; }
        set { _workQueueClass = value; }
    }

    public void LoadQueue()
    {
        IWorkQueueService wqs =
            ServiceManager.Services.GetService(typeof(IWorkQueueService)) as IWorkQueueService;
        WorkQueueClass wqc = (WorkQueueClass)wqs.WQClass(this.QueueClass);

        BuildUI(wqc, this.QueueId);
        BuildCommands();
    }

    private void BuildCommands()
    {
        DataSet data = DataService.Instance.LoadData(
            new Guid("1d33b667-ca76-4aaa-a47d-0e404ed6f8a6"),
            new Guid("421aec03-1eec-43f9-b0bb-17cfc24510a0"),
            Guid.Empty,
            Guid.Empty,
            null,
            "WorkQueueCommand_parWorkQueueId",
            this.QueueId
        );
        int x = 8;
        int y = 32;
        int step = 24;
        IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
        foreach (DataRow row in data.Tables["WorkQueueCommand"].Rows)
        {
            if (
                row.IsNull("Roles")
                || !auth.Authorize(SecurityManager.CurrentPrincipal, (string)row["Roles"])
            )
            {
                continue;
            }
            LinkLabel link = new LinkLabel();
            link.Left = x;
            link.Top = y;
            link.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            link.Text = (string)row["Text"];
            link.Links.Add(0, link.Text.Length, row);
            link.LinkClicked += new LinkLabelLinkClickedEventHandler(link_LinkClicked);
            panel1.Controls.Add(link);
            y += step;
        }
    }

    private void BuildUI(WorkQueueClass wqc, Guid queueId)
    {
        this.FormGenerator = new FormGenerator();
        this.FormGenerator.MainFormDataStructureId = wqc.WorkQueueStructureId;
        this.FormGenerator.MainFormMethodId = wqc.WorkQueueStructureUserListMethodId;
        IWorkQueueService wqs =
            ServiceManager.Services.GetService(typeof(IWorkQueueService)) as IWorkQueueService;

        DataSet data = wqs.LoadWorkQueueData(wqc.Name, queueId);
        Origam.DA.DatasetTools.AddSortColumns(data);
        DataTable table = data.Tables["WorkQueueEntry"];
        this.FormGenerator.DataSet = data;
        this.FormGenerator.Form = this;
        this.FormGenerator.SelectionParameters.Add("WorkQueueEntry_parWorkQueueId", queueId);
        AsPanel panel = new AsPanel();
        _dataPanel = panel;
        panel.BeginInit();
        panel.Visible = false;
        panel.PanelTitle = strings.Messages_PanelTitle;
        panel.Generator = this.FormGenerator;
        panel.PanelUniqueId = this.QueueId;
        panel.DataMember = "WorkQueueEntry";
        panel.Dock = DockStyle.Fill;
        panel.ShowDeleteButton = false;
        panel.ShowNewButton = false;
        panel.ShowAuditLogButton = false;
        panel.ShowGridButton = false;
        panel.ShowAttachmentsButton = true;
        panel.GridVisible = true;
        int lastPos = 0;
        foreach (
            DataStructureColumn col in (
                (DataStructureEntity)wqc.WorkQueueStructure.Entities[0]
            ).Columns
        )
        {
            if (
                col.Name != "Id"
                & col.Name != "RecordCreated"
                & col.Name != "RecordUpdated"
                & col.Name != "RecordCreatedBy"
                & col.Name != "RecordUpdatedBy"
                & col.Name != "refWorkQueueId"
            )
            {
                lastPos += 20;
                switch (col.Field.DataType)
                {
                    case OrigamDataType.Float:
                    case OrigamDataType.Integer:
                    case OrigamDataType.Currency:
                    case OrigamDataType.Memo:
                    case OrigamDataType.String:

                        AsTextBox tb = new AsTextBox();
                        tb.ReadOnly = true;
                        tb.Caption = col.Caption == "" ? col.Field.Caption : col.Caption;
                        tb.CaptionPosition = CaptionPosition.Left;
                        tb.CaptionLength = 100;
                        tb.DataType = table.Columns[col.Name].DataType;
                        tb.Size = new System.Drawing.Size(100, 16);
                        tb.Location = new System.Drawing.Point(110, lastPos);
                        if (col.Field.DataType == OrigamDataType.Float)
                        {
                            //tb.FormatType = C1.Win.C1Input.FormatTypeEnum.CustomFormat;
                            tb.CustomFormat = "###,###,###,###,##0.##################";
                        }
                        else if (col.Field.DataType == OrigamDataType.Currency)
                        {
                            //tb.FormatType = C1.Win.C1Input.FormatTypeEnum.StandardNumber;
                            tb.CustomFormat = "###,###,###,###,##0.##################";
                        }
                        else if (col.Field.DataType == OrigamDataType.Integer)
                        {
                            tb.CustomFormat = "###,###,###,###,###";
                            //tb.FormatType = C1.Win.C1Input.FormatTypeEnum.Integer;
                        }
                        else if (col.Field.DataType == OrigamDataType.Memo)
                        {
                            tb.Multiline = true;
                        }
                        panel.Controls.Add(tb);
                        Binding binding = new Binding(
                            tb.DefaultBindableProperty,
                            data,
                            panel.DataMember + "." + col.Name
                        );
                        //this.FormGenerator.SetTooltip(tb, "", tb.Caption);
                        this.FormGenerator.ControlBindings.Add(tb, binding);
                        tb.BindingContext = FormGenerator.BindingContext;
                        break;
                    case OrigamDataType.UniqueIdentifier:
                        if (col.FinalLookup != null)
                        {
                            AsDropDown dd = new AsDropDown();
                            dd.ReadOnly = true;
                            dd.Caption = col.Caption == "" ? col.Field.Caption : col.Caption;
                            dd.CaptionPosition = CaptionPosition.Left;
                            dd.CaptionLength = 100;
                            dd.Size = new System.Drawing.Size(100, 16);
                            dd.Location = new System.Drawing.Point(110, lastPos);
                            dd.LookupId = (Guid)col.FinalLookup.PrimaryKey["Id"];
                            panel.Controls.Add(dd);
                            binding = new Binding(
                                dd.DefaultBindableProperty,
                                data,
                                panel.DataMember + "." + col.Name
                            );
                            //this.FormGenerator.SetTooltip(dd, "", dd.Caption);
                            this.FormGenerator.ControlBindings.Add(dd, binding);
                            ServiceManager
                                .Services.GetService<IControlsLookUpService>()
                                .AddLookupControl(dd, this, true);
                        }
                        break;
                    case OrigamDataType.Date:
                        AsDateBox db = new AsDateBox();
                        db.ReadOnly = true;
                        db.Caption = col.Caption == "" ? col.Field.Caption : col.Caption;
                        db.CaptionPosition = CaptionPosition.Left;
                        db.CaptionLength = 100;
                        db.Size = new System.Drawing.Size(100, 16);
                        db.Location = new System.Drawing.Point(110, lastPos);

                        panel.Controls.Add(db);
                        binding = new Binding(
                            db.DefaultBindableProperty,
                            data,
                            panel.DataMember + "." + col.Name
                        );
                        //this.FormGenerator.SetTooltip(db, "", db.Caption);
                        this.FormGenerator.ControlBindings.Add(db, binding);
                        break;
                }
            }
        }
        this.Controls.Add(panel);
        this.FormGenerator.DataConsumers.Add(panel, data);
        panel.EndInit();
        panel.BringToFront();
        this.FormGenerator.BindControls();
        this.FormGenerator.SetDataSourceToConsumers();
        panel.Visible = true;
        panel.Focus();
    }

    private void link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        DataRow row = (DataRow)e.Link.LinkData;
        DataGrid grid = _dataPanel.Grid;
        if (grid != null && grid.DataSource != null)
        {
            CurrencyManager cm =
                this.BindingContext[grid.DataSource, grid.DataMember] as CurrencyManager;
            DataSet result = (grid.DataSource as DataSet).Clone();
            DataTable resultTable = result.Tables["WorkQueueEntry"];
            int count = cm.Count;
            for (int i = 0; i < count; i++)
            {
                if (grid.IsSelected(i))
                {
                    resultTable.LoadDataRow((cm.List[i] as DataRowView).Row.ItemArray, true);
                }
            }
            // no multiple selection - so we take the currently active record
            if (resultTable.Rows.Count == 0 && cm.Position >= 0)
            {
                resultTable.LoadDataRow((cm.Current as DataRowView).Row.ItemArray, true);
            }
            try
            {
                IWorkQueueService wqs =
                    ServiceManager.Services.GetService(typeof(IWorkQueueService))
                    as IWorkQueueService;
                wqs.HandleAction(
                    (Guid)row["Id"],
                    this.QueueClass,
                    resultTable,
                    (Guid)row["refWorkQueueCommandTypeId"],
                    row.IsNull("Command") ? null : (string)row["Command"],
                    row.IsNull("Param1") ? null : (string)row["Param1"],
                    row.IsNull("Param2") ? null : (string)row["Param2"],
                    row.IsNull("refErrorWorkQueueId") ? null : row["refErrorWorkQueueId"]
                );
            }
            catch (Exception ex)
            {
                AsMessageBox.ShowError(
                    this,
                    ex.Message,
                    strings.ErrorWhenProcessingWorkQueueCommand_Message,
                    ex
                );
            }
            this.RefreshContent();
        }
    }
}
