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
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.RuleModel;
using System.Globalization;
using Origam.Extensions;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;

namespace Origam.Workflow;
/// <summary>
/// Summary description for WorkflowHost.
/// </summary>
public class WorkflowHost : IDisposable
{
	private static readonly log4net.ILog log =
		log4net.LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	private static WorkflowHost _defaultHost = new WorkflowHost();
	private List<WorkflowEngine> _runningWorkflows = new ();
	private Dictionary<Guid, Tasks.UIEngineTask> _runningForms = new ();
	private bool _supportsUI = false;
	public event WorkflowHostEvent WorkflowFinished;
	public event WorkflowHostMessageEvent WorkflowMessage;
	public event WorkflowHostFormEvent FormRequested;
	public WorkflowHost()
	{
	}
	public static WorkflowHost DefaultHost
	{
		get
		{
			return _defaultHost;
		}
	}
	public bool SupportsUI
	{
		get
		{
			return _supportsUI;
		}
		set
		{
			_supportsUI = value;
		}
	}
	public List<WorkflowEngine> RunningWorkflows
	{
		get
		{
			return _runningWorkflows;
		}
	}
	public void ExecuteWorkflow(WorkflowEngine engine)
	{
        lock(_runningWorkflows)
        {
            _runningWorkflows.Add(engine);
        }
		engine.Host = this;
		engine.RunWorkflowFromHost();
	}
	internal void OnWorkflowFinished(WorkflowEngine engine, Exception exception)
	{
		try
		{
			// pass notification texts up to the calling workflow
			if(engine.CallingWorkflow != null && exception == null)
			{
				engine.CallingWorkflow.Notification = engine.Notification;
				engine.CallingWorkflow.ResultMessage = engine.ResultMessage;
			}
			// fire event
			if(WorkflowFinished != null)
			{
				Exception exceptionToPassOn =
					PassExceptionOn(engine, exception)
						? exception
						: null;
				WorkflowFinished(this, new WorkflowHostEventArgs(engine, exceptionToPassOn));
                if(exception != null)
                {
                    log.LogOrigamError(exception);
                }
			}
		}
		finally
		{
            lock(_runningWorkflows)
            {
                _runningWorkflows.Remove(engine);
            }
			engine.Host = null;
		}
	}
	private bool PassExceptionOn(WorkflowEngine engine, Exception exception)
	{
		if (engine.WorkflowBlock is not Schema.WorkflowModel.Workflow)
		{
			return true;
		}
		return
			exception != null &&
			exception.Data["onFailure"] is not StepFailureMode.Suppress;
	}
	internal void OnWorkflowUserMessage(WorkflowEngine engine, string message, Exception exception, bool popup)
	{
		UICheck();
		if(WorkflowMessage != null)
		{
			this.WorkflowMessage(this, new WorkflowHostMessageEventArgs(engine, message, exception, popup));
		}
	}
	internal void OnWorkflowForm(Tasks.UIEngineTask task, IDataDocument data, string description, 
		string notification, FormControlSet form, DataStructureRuleSet ruleSet, IEndRule endRule, 
		bool isFinalForm, bool allowSave, bool isAutoNext, AbstractDataStructure structure,
		DataStructureMethod refreshMethod, DataStructureSortSet refreshSort, bool isRefreshSuppressedBeforeFirstSave,
		IEndRule saveConfirmationRule, AbstractDataStructure saveStructure, Hashtable parameters,
        bool refreshPortalAfterSave)
	{
		UICheck();
		Guid taskId = Guid.NewGuid();
		_runningForms.Add(taskId, task);
		// fire event
		if(FormRequested != null)
		{
			this.FormRequested(this, new WorkflowHostFormEventArgs(taskId, task.Engine, data, 
				description, notification, form, ruleSet, endRule, structure, 
				refreshMethod, refreshSort, saveStructure, isFinalForm, allowSave, isAutoNext, parameters,
				isRefreshSuppressedBeforeFirstSave, saveConfirmationRule, refreshPortalAfterSave));
		}
	}
	public void AbortWorkflowForm(Guid taskId)
    {
        Tasks.UIEngineTask task = _runningForms[taskId];
        if (task == null)
        {
            throw new ArgumentOutOfRangeException(ResourceUtils.GetString("ErrorTaskNotRunning", taskId.ToString()));
        }
        _runningForms.Remove(taskId);
        Thread thread = new Thread(task.Abort);
        PrepareAndStartThread(thread, task);
    }
    public void FinishWorkflowForm(Guid taskId, IDataDocument data)
	{
		Tasks.UIEngineTask task = _runningForms[taskId];
		if (task == null)
		{
			throw new ArgumentOutOfRangeException(ResourceUtils.GetString("ErrorTaskNotRunning", taskId.ToString()));
		}
		_runningForms.Remove(taskId);
        
		task.Result = data;
        Thread thread = new Thread(task.Finish);
        PrepareAndStartThread(thread, task);
	}
    private static void PrepareAndStartThread(Thread thread, Tasks.UIEngineTask task)
    {            
        thread.Name = "Workflow " + task.Engine.WorkflowInstanceId.ToString();
        thread.IsBackground = true;
        thread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
        thread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
        thread.Start();
    }
    private void UICheck()
	{
		if(! SupportsUI)
		{
			throw new NullReferenceException(ResourceUtils.GetString("ErrorNoWorkflowUI"));
		}
	}
	#region IDisposable Members
	public void Dispose()
	{
	}
	#endregion
}
