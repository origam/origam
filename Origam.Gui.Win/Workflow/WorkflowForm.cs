using System;
using System.Windows.Forms;
using System.Xml;

using Origam.DA;
using Origam.Gui.Win;
using Origam.Schema.GuiModel;
using Origam.Schema.RuleModel;

namespace Origam.Workflow
{
	/// <summary>
	/// Summary description for WorkflowForm.
	/// </summary>
	public class WorkflowForm : AsForm
	{
		private Guid _taskId;

		public WorkflowForm(WorkflowHost host)
		{
			InitializeComponent();

			this.CanRefreshContent = false;
			this.Host = host;
		}

		#region Windows Forms Generated Code
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WorkflowForm));
			// 
			// WorkflowForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(648, 373);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.CloseButton = false;
			this.Name = "WorkflowForm";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.WorkflowForm_Closing);

		}
		#endregion

		private WorkflowHost _host;
		public WorkflowHost Host
		{
			get
			{
				return _host;
			}
			set
			{
				if(_host != null)
				{
					this.Host.FormRequested -= new WorkflowHostFormEvent(Host_FormRequested);
					this.Host.WorkflowFinished -= new WorkflowHostEvent(Host_WorkflowFinished);
					this.Host.WorkflowMessage -= new WorkflowHostMessageEvent(Host_WorkflowMessage);
				}

				_host = value;

				if(_host != null)
				{
					this.Host.FormRequested += new WorkflowHostFormEvent(Host_FormRequested);
					this.Host.WorkflowFinished += new WorkflowHostEvent(Host_WorkflowFinished);
					this.Host.WorkflowMessage += new WorkflowHostMessageEvent(Host_WorkflowMessage);
				}
			}
		}

		private bool _canFinishTask;
		public bool CanFinishTask
		{
			get
			{
				return _canFinishTask;
			}
			set
			{
				_canFinishTask = value;
				Origam.Workbench.WorkbenchSingleton.Workbench.UpdateToolbar();
			}
		}

		private WorkflowEngine _workflowEngine = null;
		public WorkflowEngine WorkflowEngine
		{
			get
			{
				return _workflowEngine;
			}
			set
			{
				// We set the new workflow engine (sub-engine)
				_workflowEngine = value;
			}
		}

		private bool _taskFinished = false;
		internal bool TaskFinished
		{
			get
			{
				return _taskFinished;
			}
			set
			{
				_taskFinished = value;
			}
		}

		private IEndRule _endRule;
		internal IEndRule EndRule
		{
			get
			{
				return _endRule;
			}
			set
			{
				_endRule = value;
			}
		}

		private void AbortTask()
		{
			this.FormGenerator.UnloadForm(true);
			this.Host.AbortWorkflowForm(_taskId);
			InvokeToolStripsRemoved();
		}

		public void FinishTask()
		{
			if(this.CanFinishTask == false) return;

			try
			{
				this.EndCurrentEdit();
			}
			catch(Exception ex)
			{
				// in case of exception we don't exit the form
				Origam.UI.AsMessageBox.ShowError(this, ex.Message, ResourceUtils.GetString("ErrorWhenFormCheck", this.TitleName), ex);
				return;
			}

			if(this.NextData.DataSet.HasErrors)
			{
				Origam.UI.AsMessageBox.ShowError(this, ResourceUtils.GetString("ErrorsInForm1") 
					+ Environment.NewLine 
					+ Environment.NewLine 
					+ DatasetTools.GetDatasetErrors(this.NextData.DataSet), ResourceUtils.GetString("ErrorWhenFormCheck", this.TitleName),
					null);

				return;
			}

			try
			{
				if(this.EndRule != null)
				{
					//DataSet tempData = this.NextData.Copy();

					//this.WorkflowEngine.EvaluateEndRule(this.EndRule, new XmlDataDocument(tempData));
					this.WorkflowEngine.EvaluateEndRule(this.EndRule, this.NextData);
				}
			}
			catch(Rule.RuleException ruleEx)
			{
                bool shouldReturn = FormGenerator.DisplayRuleException(this, ruleEx);
                if (shouldReturn)
                {
                    return;
                }
			}
			catch(Exception ex)
			{
				Origam.UI.AsMessageBox.ShowError(this, ex.Message, ResourceUtils.GetString("ErrorWhenFormCheck", this.TitleName), ex);
				return;
			}

			this.CanFinishTask = false;

			this.FormGenerator.UnloadForm(true);

			this.CanAbort = false;
			this.CloseButton = false;
			Application.DoEvents();
			this.TaskFinished = true;

			this.Host.FinishWorkflowForm(_taskId, this.NextData);
		}

        /// <summary>
		/// Never get dirty.
		/// </summary>
		public override bool IsDirty
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public override bool IsViewOnly
		{
			get
			{
				return true;
			}
		}
        internal IDataDocument NextData { get; set; }

        private FormControlSet _nextForm;
		internal FormControlSet NextForm
		{
			get
			{
				return _nextForm;
			}
			set
			{
				_nextForm = value;
			}
		}

		private string _nextDescription;
		internal string NextDescription
		{
			get
			{
				return _nextDescription;
			}
			set
			{
				_nextDescription = value;
			}
		}

		private bool _canAbort;
		internal bool CanAbort
		{
			get
			{
				return _canAbort;
			}
			set
			{
				_canAbort = value;
			}
		}

		bool _finishScreen = false;

		internal void ShowFinishScreen()
		{
			this.FormGenerator.UnloadForm(true);

			this.DockPadding.All = 5;

			this.StatusText = ResourceUtils.GetString("ProcessFinished");
			AppendProcessLog(Environment.NewLine + "----------------------------------" + Environment.NewLine + "Proces dokonèen.");

			this.CanFinishTask = false;
			this.CloseButton = true;
			this.Refresh();
			this.CanAbort = true;
			
			Label logLabel = new Label();
			logLabel.Text = ResourceUtils.GetString("ProcessFinished");
			logLabel.Dock = DockStyle.Top;
			logLabel.Font = new System.Drawing.Font(logLabel.Font.FontFamily, 14, System.Drawing.FontStyle.Bold);
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
			btnClose.Text = ResourceUtils.GetString("ButtonClose");
			btnClose.Image = Origam.Workbench.Images.Delete;
			btnClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			btnClose.Click += new EventHandler(btnClose_Click);
			buttonPanel.Controls.Add(btnClose);

			Button btnRepeat = null;

			if(this.WorkflowEngine.IsRepeatable)
			{
				btnRepeat = new Button();
				btnRepeat.TabIndex = 0;
				btnRepeat.Top = 5;
				btnRepeat.Left = 140;
				btnRepeat.Height = 25;
				btnRepeat.Width = 130;
				btnRepeat.FlatStyle = FlatStyle.Flat;
				btnRepeat.Text = ResourceUtils.GetString("ButtonRepeat");
				btnRepeat.Image = Origam.Workbench.Images.RecurringWorkflow;
				btnRepeat.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
				btnRepeat.Click += new EventHandler(btnRepeat_Click);
				buttonPanel.Controls.Add(btnRepeat);
			}

			TextBox logBox = new TextBox();
			logBox.TabIndex = 2;
			logBox.Left = 0;
			logBox.Top = logLabel.Height;
			logBox.Width = 500;
			logBox.Height = 500;
			logBox.Text = ResourceUtils.GetString("ProcessSteps");
			logBox.Text += Environment.NewLine + "------------------------------------------------";
			logBox.Text += Environment.NewLine + _processLog;
			logBox.ScrollBars = ScrollBars.Both;
			logBox.Multiline = true;
			logBox.BorderStyle = BorderStyle.None;
			logBox.ReadOnly = true;
			logBox.BackColor = System.Drawing.Color.White;
			logBox.Dock = DockStyle.Fill;

			this.Controls.Add(logBox);
			this.Controls.Add(logLabel);
			this.Controls.Add(buttonPanel);

			logLabel.BringToFront();
			buttonPanel.BringToFront();
			logBox.BringToFront();

			if(btnRepeat != null)
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
			//CurtainForm curtain = new CurtainForm();
			try
			{
				//curtain.Show(this);
				this.StatusText = ResourceUtils.GetString("FormLoading");
				Cursor.Current = Cursors.WaitCursor;
				//this.SuspendLayout();
				this.TaskFinished = false;
				this.FormGenerator.LoadFormWithData(this, this.NextData.DataSet, this.NextData, this.NextForm);
				this.EndCurrentEdit();
				this.SelectNextControl(this, true, true, true, true);
				this.StatusText = this.NextDescription;
				//AppendProcessLog(this.NextDescription);
				if(this.NameLabel != null)
				{
					this.NameLabel.Text = this.TitleName + ": " + this.NextDescription;
				}
				this.CanFinishTask = true;
				this.CloseButton = true; //this.CanAbort;
				//curtain.Fade();
			}
			finally
			{
				//this.ResumeLayout(true);
				//this.OnResize(EventArgs.Empty);
				Cursor.Current = Cursors.Default;
			}
		}

        delegate void ShowMessageDelegate(string message, Exception exception);
        internal void ShowMessage(string message, Exception ex)
        {
            if (this.InvokeRequired)
            {
                ShowMessageDelegate showMessage = new ShowMessageDelegate(ShowMessage);
                this.Invoke(showMessage, new object[] { message, ex });
            }
            else
            {
                if (ex is WorkflowCancelledByUserException)
                {
                    MessageBox.Show(this, message, ResourceUtils.GetString("WorkflowFinishedTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Origam.UI.AsMessageBox.ShowError(this, message, ResourceUtils.GetString("WorkflowReportTitle", this.TitleName), ex);
                }

                AppendProcessLog(Environment.NewLine + "----------------------------------" + Environment.NewLine + message + Environment.NewLine + "----------------------------------");
            }
        }

		internal void SetWorkflowDescription(string description)
		{
			this.StatusText = description;
			
			AppendProcessLog(description);
			Application.DoEvents();
		}

		private void btnFinishTask_Click(object sender, System.EventArgs e)
		{
			this.FinishTask();
		}

		private void WorkflowForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(_finishScreen) return;

//			if(this.CanAbort)
//			{
				AbortTask();
				e.Cancel = true;
//			}
//			else
//			{
//				e.Cancel = true;
//
//				MessageBox.Show("Není možno zavøít okno, protože bìží proces. Nejdøíve proces dokonèete.");
//			}
		}

		#region Events
		public event EventHandler StatusChanged;
		void OnStatusChanged(EventArgs e)
		{
			if (StatusChanged != null) 
			{
				StatusChanged(this, e);
			}
		}
		#endregion

		private string _processLog = "";
		private void AppendProcessLog(string text)
		{
			if(_processLog != "")
			{
				_processLog += Environment.NewLine;
			}

			_processLog += DateTime.Now.ToString() + ": " + text;
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				this.Host = null;
				_endRule = null;
				NextData = null;
				_nextForm = null;
				_workflowEngine = null;
			}

			base.Dispose (disposing);
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void btnRepeat_Click(object sender, EventArgs e)
		{
			Button btn = sender as Button;
			btn.Click -= new EventHandler(btnClose_Click);
			this.Controls.Remove(btn);

			this.Controls.Clear();
			this.DockPadding.All = 0;
			this.CanFinishTask = false;
			this.CloseButton = false;
			this.Refresh();
			this.CanAbort = false;
			_finishScreen = false;

			this.WorkflowEngine.WorkflowInstanceId = Guid.NewGuid();
			WorkflowHost.DefaultHost.ExecuteWorkflow(this.WorkflowEngine);
		}

		private void Host_FormRequested(object sender, WorkflowHostFormEventArgs e)
		{
			if(e.Engine.WorkflowInstanceId.Equals(this.WorkflowEngine.WorkflowInstanceId))
			{
				this.WorkflowEngine = e.Engine;
				this.NextData = e.Data;
				this.NextDescription = e.Description;
				this.NotificationText = e.Notification;
				this.NextForm = e.Form;
				this.FormGenerator.RuleSet = e.RuleSet;
				this.EndRule = e.EndRule;
				_taskId = e.TaskId;

				this.Invoke(new MethodInvoker(this.ShowWorkflowUI));
			}
		}

        private void Host_WorkflowFinished(object sender, WorkflowHostEventArgs e)
		{
			if(e.Engine.WorkflowInstanceId.Equals(this.WorkflowEngine.WorkflowInstanceId))
			{
				if(e.Engine.CallingWorkflow == null)
				{
					this.WorkflowEngine = e.Engine;

					if(e.Exception != null)
					{
                        this.ShowMessage(e.Exception.Message, e.Exception);
					}

					this.Invoke(new MethodInvoker(this.ShowFinishScreen));
				}
			}
		}

		private void Host_WorkflowMessage(object sender, WorkflowHostMessageEventArgs e)
		{
			if(e.Engine.WorkflowInstanceId.Equals(this.WorkflowEngine.WorkflowInstanceId))
			{
				if(e.Popup)
				{
					this.ShowMessage(e.Message, e.Exception);
				}
				else
				{
					this.SetWorkflowDescription(e.Message); 
				}
			}
		}
	}
}
