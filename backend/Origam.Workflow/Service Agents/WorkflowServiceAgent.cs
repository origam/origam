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
using System.Xml;
using System.Collections;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Origam.Workflow;
/// <summary>
/// Summary description for WorkflowServiceAgent.
/// </summary>
public class WorkflowServiceAgent : AbstractServiceAgent, IAsyncAgent
{
	public event EventHandler<AsyncReturnValues> AsyncCallFinished;
	public WorkflowServiceAgent()
	{
	}
	#region Private Methods
	private object ExecuteWorkflow(Guid workflowId, Hashtable parameters)
	{
		bool invalidWorkflowDefinition = false;
		IWorkflow wf = null;
		try
		{
			wf = this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), new ModelElementKey(workflowId)) as IWorkflow;
		}
		catch
		{
			invalidWorkflowDefinition = true;
		}
		if(wf == null || invalidWorkflowDefinition)
		{
			throw new ArgumentOutOfRangeException("workflowId", workflowId, ResourceUtils.GetString("ErrorWorkflowDefinition"));
		}
		WorkflowEngine engine = new WorkflowEngine();
		engine.PersistenceProvider = this.PersistenceProvider;
		engine.WorkflowBlock = wf;
        engine.TransactionBehavior = wf.TransactionBehavior;
        engine.SetTransactionId(TransactionId, wf.TransactionBehavior);
		engine.WorkflowInstanceId = this.TraceWorkflowId;
		engine.CallingWorkflow = this.WorkflowEngine as WorkflowEngine;
	    engine.Name = string.IsNullOrEmpty(wf.Name) ?
	        "WorkFlow" : wf.Name;
		engine.Trace = this.Trace;
		workflowUniqueId = engine.WorkflowUniqueId;
        // input parameters
        foreach (DictionaryEntry entry in parameters)
		{
			ISchemaItem context = wf.GetChildByName((string)entry.Key, ContextStore.CategoryConst);
				
			if(context == null)
			{
				throw new ArgumentOutOfRangeException("name", entry.Key, ResourceUtils.GetString("ErrorWorkflowContext", ((ISchemaItem)wf).Path));
			}
			object contextValue = entry.Value;
			if(contextValue is DataSet)
			{
				contextValue = DataDocumentFactory.New(contextValue as DataSet);
			}
				
			engine.InputContexts.Add(context.PrimaryKey, contextValue);
		}
		var host = GetHost();
		host.WorkflowFinished += OnHostOnWorkflowFinished;
        host.WorkflowMessage += Host_WorkflowMessage;
        
        host.ExecuteWorkflow(engine);
        if(engine.WorkflowException != null)
		{
			throw engine.WorkflowException;
		}
		return engine.ReturnValue;
	}
	
	private WorkflowHost GetHost()
	{
		return WorkflowEngine != null 
			? (WorkflowEngine as WorkflowEngine).Host 
			: WorkflowHost.DefaultHost;
	}
	private void OnHostOnWorkflowFinished(object sender, WorkflowHostEventArgs e)
	{
		if (e.Engine.WorkflowUniqueId.Equals(workflowUniqueId))
		{
			UnsubscribeEvents();
			AsyncCallFinished?.Invoke(null, new AsyncReturnValues
			{
				Result = e.Engine.ReturnValue,
				Exception = e.Exception
			});
		}
	}
	private void Host_WorkflowMessage(object sender, WorkflowHostMessageEventArgs e)
	{
		if(e.Engine.WorkflowUniqueId.Equals(workflowUniqueId))
		{
			if(e.Exception != null)
			{
				UnsubscribeEvents();
				AsyncCallFinished?.Invoke(
					null, 
					new AsyncReturnValues{Exception = e.Exception});
			}
		}
	}
	private void UnsubscribeEvents()
	{
		var host = GetHost();
		host.WorkflowFinished -= OnHostOnWorkflowFinished;
		host.WorkflowMessage -= Host_WorkflowMessage;
	}
	#endregion
	#region IServiceAgent Members
	private object _result;
	private Guid workflowUniqueId;
	public override object Result
	{
		get
		{
			return _result;
		}
	}
	public override void Run()
	{
		switch(this.MethodName)
		{
			case "ExecuteWorkflow":
				// Check input parameters
				if(! (this.Parameters["Workflow"] is Guid))
					throw new InvalidCastException(ResourceUtils.GetString("ErrorWorkflowNotGuid"));
				
				if(! (this.Parameters["Parameters"] is Hashtable))
					throw new InvalidCastException(ResourceUtils.GetString("ErrorNotHashtable"));
				_result = this.ExecuteWorkflow((Guid)this.Parameters["Workflow"],
					(Hashtable)this.Parameters["Parameters"]);
				break;
			default:
				throw new ArgumentOutOfRangeException("MethodName", this.MethodName, ResourceUtils.GetString("InvalidMethodName"));
		}
	}
	public override IList<string> ExpectedParameterNames(ISchemaItem item, string method, string parameter)
	{
		var result = new List<string>();
		IWorkflow wf = item as IWorkflow;
		ServiceMethodCallTask task = item as ServiceMethodCallTask;
		if(task != null)
		{
			wf = ResolveServiceMethodCallTask(task);
		}
		if(wf != null && method == "ExecuteWorkflow" && parameter == "Parameters")
		{
			foreach(var cs in wf.ChildItemsByType<ContextStore>(ContextStore.CategoryConst))
			{
				result.Add(cs.Name);
			}
		}
		return result;
	}
	private IWorkflow ResolveServiceMethodCallTask(ServiceMethodCallTask task)
	{
		ISchemaItem wfParam = task.GetChildByName("Workflow");
		if(wfParam.ChildItems.Count == 1)
		{
			WorkflowReference wfRef = wfParam.ChildItems[0] as WorkflowReference;
			if(wfRef != null)
			{
				return wfRef.Workflow;
			}
		}
		return null;
	}
	#endregion
}
