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
using System.Windows.Forms;
using Origam.DA;
using Origam.Gui.Win;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;

namespace Origam.Workflow;

/// <summary>
/// Summary description for WorkflowForm.
/// </summary>
public class WorkflowForm : AsForm
{
    private Guid _taskId;
    private static readonly string messageSeparator =
        "------------------------------------------------";

    public WorkflowForm(WorkflowHost host)
    {
        InitializeComponent();
        this.CanRefreshContent = false;
        this.Host = host;
    }

    #region Windows Forms Generated Code
    private void InitializeComponent()
    {
        System.Resources.ResourceManager resources = new System.Resources.ResourceManager(
            resourceSource: typeof(WorkflowForm)
        );
        //
        // WorkflowForm
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(width: 5, height: 13);
        this.ClientSize = new System.Drawing.Size(width: 648, height: 373);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject(name: "$this.Icon")));
        this.CloseButton = false;
        this.Name = "WorkflowForm";
        this.Closing += this.WorkflowForm_Closing;
    }
    #endregion
    private WorkflowHost _host;
    public WorkflowHost Host
    {
        get => _host;
        set
        {
            if (_host != null)
            {
                this.Host.FormRequested -= Host_FormRequested;
                this.Host.WorkflowFinished -= Host_WorkflowFinished;
                this.Host.WorkflowMessage -= Host_WorkflowMessage;
            }
            _host = value;
            if (_host != null)
            {
                this.Host.FormRequested += Host_FormRequested;
                this.Host.WorkflowFinished += Host_WorkflowFinished;
                this.Host.WorkflowMessage += Host_WorkflowMessage;
            }
        }
    }
    private bool _canFinishTask;
    public bool CanFinishTask
    {
        get => _canFinishTask;
        set
        {
            _canFinishTask = value;
            Workbench.WorkbenchSingleton.Workbench.UpdateToolbar();
        }
    }
    public WorkflowEngine WorkflowEngine { get; set; } = null;
    internal bool TaskFinished { get; set; } = false;
    internal IEndRule EndRule { get; set; }

    private void AbortTask()
    {
        this.FormGenerator.UnloadForm(showCurtain: true);
        // isDirty: false is set here because it was not immediately obvious how to get the actual value.
        // Desktop Architect will be at end of life soon anyway.
        this.Host.AbortWorkflowForm(taskId: _taskId, isDirty: false);
        InvokeToolStripsRemoved();
    }

    public void FinishTask()
    {
        if (this.CanFinishTask == false)
        {
            return;
        }

        try
        {
            this.EndCurrentEdit();
        }
        catch (Exception ex)
        {
            // in case of exception we don't exit the form
            UI.AsMessageBox.ShowError(
                owner: this,
                text: ex.Message,
                caption: ResourceUtils.GetString(key: "ErrorWhenFormCheck", args: this.TitleName),
                exception: ex
            );
            return;
        }
        if (this.NextData.DataSet.HasErrors)
        {
            UI.AsMessageBox.ShowError(
                owner: this,
                text: ResourceUtils.GetString(key: "ErrorsInForm1")
                    + Environment.NewLine
                    + Environment.NewLine
                    + DatasetTools.GetDatasetErrors(dataset: this.NextData.DataSet),
                caption: ResourceUtils.GetString(key: "ErrorWhenFormCheck", args: this.TitleName),
                exception: null
            );
            return;
        }
        try
        {
            if (this.EndRule != null)
            {
                this.WorkflowEngine.EvaluateEndRule(
                    rule: this.EndRule,
                    data: this.NextData,
                    step: null
                );
            }
        }
        catch (RuleException ruleEx)
        {
            bool shouldReturn = FormGenerator.DisplayRuleException(window: this, ruleEx: ruleEx);
            if (shouldReturn)
            {
                return;
            }
        }
        catch (Exception ex)
        {
            UI.AsMessageBox.ShowError(
                owner: this,
                text: ex.Message,
                caption: ResourceUtils.GetString(key: "ErrorWhenFormCheck", args: this.TitleName),
                exception: ex
            );
            return;
        }
        this.CanFinishTask = false;
        this.FormGenerator.UnloadForm(showCurtain: true);
        this.CanAbort = false;
        this.CloseButton = false;
        Application.DoEvents();
        this.TaskFinished = true;
        this.Host.FinishWorkflowForm(taskId: _taskId, data: this.NextData);
    }

    /// <summary>
    /// Never get dirty.
    /// </summary>
    public override bool IsDirty
    {
        get => false;
        set { }
    }
    public override bool IsViewOnly => true;
    internal IDataDocument NextData { get; set; }
    internal FormControlSet NextForm { get; set; }
    internal string NextDescription { get; set; }
    internal bool CanAbort { get; set; }
    bool _finishScreen = false;

    internal void ShowFinishScreen()
    {
        this.FormGenerator.UnloadForm(showCurtain: true);
        this.DockPadding.All = 5;
        this.StatusText = ResourceUtils.GetString(key: "ProcessFinished");
        AppendProcessLog(
            text: Environment.NewLine
                + messageSeparator
                + Environment.NewLine
                + ResourceUtils.GetString(key: "ProcessFinished")
        );
        this.CanFinishTask = false;
        this.CloseButton = true;
        this.Refresh();
        this.CanAbort = true;

        Label logLabel = new Label();
        logLabel.Text = ResourceUtils.GetString(key: "ProcessFinished");
        logLabel.Dock = DockStyle.Top;
        logLabel.Font = new System.Drawing.Font(
            family: logLabel.Font.FontFamily,
            emSize: 14,
            style: System.Drawing.FontStyle.Bold
        );
        logLabel.AutoSize = true;
        logLabel.BorderStyle = BorderStyle.None;
        Panel buttonPanel = new Panel();
        buttonPanel.Height = 50;
        buttonPanel.Dock = DockStyle.Top;
        Button btnClose = new Button();
        btnClose.TabIndex = 1;
        btnClose.Top = 5;
        btnClose.Left = 0;
        btnClose.Height = 25;
        btnClose.Width = 130;
        btnClose.FlatStyle = FlatStyle.Flat;
        btnClose.Text = ResourceUtils.GetString(key: "ButtonClose");
        btnClose.Image = Origam.Workbench.Images.Delete;
        btnClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        btnClose.Click += btnClose_Click;
        buttonPanel.Controls.Add(value: btnClose);
        Button btnRepeat = null;
        if (this.WorkflowEngine.IsRepeatable)
        {
            btnRepeat = new Button();
            btnRepeat.TabIndex = 0;
            btnRepeat.Top = 5;
            btnRepeat.Left = 140;
            btnRepeat.Height = 25;
            btnRepeat.Width = 130;
            btnRepeat.FlatStyle = FlatStyle.Flat;
            btnRepeat.Text = ResourceUtils.GetString(key: "ButtonRepeat");
            btnRepeat.Image = Workbench.Images.RecurringWorkflow;
            btnRepeat.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btnRepeat.Click += btnRepeat_Click;
            buttonPanel.Controls.Add(value: btnRepeat);
        }
        TextBox logBox = new TextBox();
        logBox.TabIndex = 2;
        logBox.Left = 0;
        logBox.Top = logLabel.Height;
        logBox.Width = 500;
        logBox.Height = 500;
        logBox.Text = ResourceUtils.GetString(key: "ProcessSteps");
        logBox.Text += Environment.NewLine + messageSeparator;
        logBox.Text += Environment.NewLine + _processLog;
        logBox.ScrollBars = ScrollBars.Both;
        logBox.Multiline = true;
        logBox.BorderStyle = BorderStyle.None;
        logBox.ReadOnly = true;
        logBox.BackColor = System.Drawing.Color.White;
        logBox.Dock = DockStyle.Fill;
        this.Controls.Add(value: logBox);
        this.Controls.Add(value: logLabel);
        this.Controls.Add(value: buttonPanel);
        logLabel.BringToFront();
        buttonPanel.BringToFront();
        logBox.BringToFront();
        if (btnRepeat != null)
        {
            // workflow is repeatable, focus will be on repeating
            btnRepeat.Focus();
        }
        else
        {
            // otherwise focus is on the close button
            btnClose.Focus();
        }
        _finishScreen = true;
    }

    internal void ShowWorkflowUI()
    {
        try
        {
            this.StatusText = ResourceUtils.GetString(key: "FormLoading");
            Cursor.Current = Cursors.WaitCursor;
            this.TaskFinished = false;
            this.FormGenerator.LoadFormWithData(
                form: this,
                formData: this.NextData.DataSet,
                xmlData: this.NextData,
                formControlSet: this.NextForm
            );
            this.EndCurrentEdit();
            this.SelectNextControl(
                ctl: this,
                forward: true,
                tabStopOnly: true,
                nested: true,
                wrap: true
            );
            this.StatusText = this.NextDescription;
            if (this.NameLabel != null)
            {
                this.NameLabel.Text = this.TitleName + ": " + this.NextDescription;
            }
            this.CanFinishTask = true;
            this.CloseButton = true;
        }
        finally
        {
            Cursor.Current = Cursors.Default;
        }
    }

    delegate void ShowMessageDelegate(string message, Exception exception);

    internal void ShowMessage(string message, Exception ex)
    {
        if (this.InvokeRequired)
        {
            ShowMessageDelegate showMessage = ShowMessage;
            this.Invoke(method: showMessage, args: new object[] { message, ex });
        }
        else
        {
            if (ex is WorkflowCancelledByUserException)
            {
                MessageBox.Show(
                    owner: this,
                    text: message,
                    caption: ResourceUtils.GetString(key: "WorkflowFinishedTitle"),
                    buttons: MessageBoxButtons.OK,
                    icon: MessageBoxIcon.Information
                );
            }
            else
            {
                UI.AsMessageBox.ShowError(
                    owner: this,
                    text: message,
                    caption: ResourceUtils.GetString(
                        key: "WorkflowReportTitle",
                        args: this.TitleName
                    ),
                    exception: ex
                );
            }
            AppendProcessLog(
                text: Environment.NewLine
                    + messageSeparator
                    + Environment.NewLine
                    + message
                    + Environment.NewLine
                    + messageSeparator
            );
        }
    }

    internal void SetWorkflowDescription(string description)
    {
        this.StatusText = description;

        AppendProcessLog(text: description);
        Application.DoEvents();
    }

    private void btnFinishTask_Click(object sender, System.EventArgs e)
    {
        this.FinishTask();
    }

    private void WorkflowForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_finishScreen)
        {
            return;
        }

        AbortTask();
        e.Cancel = true;
    }

    #region Events
    public event EventHandler StatusChanged;

    void OnStatusChanged(EventArgs e)
    {
        StatusChanged?.Invoke(sender: this, e: e);
    }
    #endregion
    private string _processLog = "";

    private void AppendProcessLog(string text)
    {
        if (_processLog != "")
        {
            _processLog += Environment.NewLine;
        }
        _processLog += DateTime.Now + ": " + text;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.Host = null;
            EndRule = null;
            NextData = null;
            NextForm = null;
            WorkflowEngine = null;
        }
        base.Dispose(disposing: disposing);
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
        this.Close();
    }

    private void btnRepeat_Click(object sender, EventArgs e)
    {
        Button btn = sender as Button;
        btn.Click -= btnClose_Click;
        this.Controls.Remove(value: btn);
        this.Controls.Clear();
        this.DockPadding.All = 0;
        this.CanFinishTask = false;
        this.CloseButton = false;
        this.Refresh();
        this.CanAbort = false;
        _finishScreen = false;
        this.WorkflowEngine.WorkflowInstanceId = Guid.NewGuid();
        WorkflowHost.DefaultHost.ExecuteWorkflow(engine: this.WorkflowEngine);
    }

    private void Host_FormRequested(object sender, WorkflowHostFormEventArgs e)
    {
        if (e.Engine.WorkflowInstanceId.Equals(g: this.WorkflowEngine.WorkflowInstanceId))
        {
            this.WorkflowEngine = e.Engine;
            this.NextData = e.Data;
            this.NextDescription = e.Description;
            this.NotificationText = e.Notification;
            this.NextForm = e.Form;
            this.FormGenerator.RuleSet = e.RuleSet;
            this.EndRule = e.EndRule;
            _taskId = e.TaskId;
            this.Invoke(method: new MethodInvoker(this.ShowWorkflowUI));
        }
    }

    private void Host_WorkflowFinished(object sender, WorkflowHostEventArgs e)
    {
        if (e.Engine.WorkflowInstanceId.Equals(g: this.WorkflowEngine.WorkflowInstanceId))
        {
            if (e.Engine.CallingWorkflow == null)
            {
                WorkflowEngine = e.Engine;
                if (
                    e.Exception != null
                    && e.Exception.Data[key: "onFailure"] is not StepFailureMode.Suppress
                )
                {
                    ShowMessage(message: e.Exception.Message, ex: e.Exception);
                }
                Invoke(method: new MethodInvoker(ShowFinishScreen));
            }
        }
    }

    private void Host_WorkflowMessage(object sender, WorkflowHostMessageEventArgs e)
    {
        if (e.Engine.WorkflowInstanceId.Equals(g: this.WorkflowEngine.WorkflowInstanceId))
        {
            if (e.Popup)
            {
                this.ShowMessage(message: e.Message, ex: e.Exception);
            }
            else
            {
                this.SetWorkflowDescription(description: e.Message);
            }
        }
    }
}
