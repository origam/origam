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
using System.Data;
using Origam.Schema;
using Origam.Schema.WorkflowModel;

namespace Origam.Workflow;

/// <summary>
/// Summary description for WorkflowServiceAgent.
/// </summary>
public class WorkflowServiceAgent : AbstractServiceAgent, IAsyncAgent
{
    public event EventHandler<AsyncReturnValues> AsyncCallFinished;

    public WorkflowServiceAgent() { }

    #region Private Methods
    private object ExecuteWorkflow(Guid workflowId, Hashtable parameters)
    {
        bool invalidWorkflowDefinition = false;
        IWorkflow wf = null;
        try
        {
            wf =
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: workflowId)
                ) as IWorkflow;
        }
        catch
        {
            invalidWorkflowDefinition = true;
        }
        if (wf == null || invalidWorkflowDefinition)
        {
            throw new ArgumentOutOfRangeException(
                paramName: "workflowId",
                actualValue: workflowId,
                message: ResourceUtils.GetString(key: "ErrorWorkflowDefinition")
            );
        }
        WorkflowEngine engine = new WorkflowEngine();
        engine.PersistenceProvider = this.PersistenceProvider;
        engine.WorkflowBlock = wf;
        engine.TransactionBehavior = wf.TransactionBehavior;
        engine.SetTransactionId(
            transactionId: TransactionId,
            transactionBehavior: wf.TransactionBehavior
        );
        engine.WorkflowInstanceId = this.TraceWorkflowId;
        engine.CallingWorkflow = this.WorkflowEngine as WorkflowEngine;
        engine.Name = string.IsNullOrEmpty(value: wf.Name) ? "WorkFlow" : wf.Name;
        engine.Trace = this.Trace;
        workflowUniqueId = engine.WorkflowUniqueId;
        // input parameters
        foreach (DictionaryEntry entry in parameters)
        {
            ISchemaItem context = wf.GetChildByName(
                name: (string)entry.Key,
                itemType: ContextStore.CategoryConst
            );

            if (context == null)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "name",
                    actualValue: entry.Key,
                    message: ResourceUtils.GetString(
                        key: "ErrorWorkflowContext",
                        args: ((ISchemaItem)wf).Path
                    )
                );
            }
            object contextValue = entry.Value;
            if (contextValue is DataSet)
            {
                contextValue = DataDocumentFactory.New(dataSet: contextValue as DataSet);
            }

            engine.InputContexts.Add(key: context.PrimaryKey, value: contextValue);
        }
        var host = GetHost();
        host.WorkflowFinished += OnHostOnWorkflowFinished;
        host.WorkflowMessage += Host_WorkflowMessage;

        host.ExecuteWorkflow(engine: engine);
        if (engine.WorkflowException != null)
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
        if (e.Engine.WorkflowUniqueId.Equals(g: workflowUniqueId))
        {
            UnsubscribeEvents();
            AsyncCallFinished?.Invoke(
                sender: null,
                e: new AsyncReturnValues { Result = e.Engine.ReturnValue, Exception = e.Exception }
            );
        }
    }

    private void Host_WorkflowMessage(object sender, WorkflowHostMessageEventArgs e)
    {
        if (e.Engine.WorkflowUniqueId.Equals(g: workflowUniqueId))
        {
            if (e.Exception != null)
            {
                UnsubscribeEvents();
                AsyncCallFinished?.Invoke(
                    sender: null,
                    e: new AsyncReturnValues { Exception = e.Exception }
                );
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
        get { return _result; }
    }

    public override void Run()
    {
        switch (this.MethodName)
        {
            case "ExecuteWorkflow":
            {
                // Check input parameters
                if (!(this.Parameters[key: "Workflow"] is Guid))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorWorkflowNotGuid")
                    );
                }

                if (!(this.Parameters[key: "Parameters"] is Hashtable))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorNotHashtable")
                    );
                }

                _result = this.ExecuteWorkflow(
                    workflowId: (Guid)this.Parameters[key: "Workflow"],
                    parameters: (Hashtable)this.Parameters[key: "Parameters"]
                );
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "MethodName",
                    actualValue: this.MethodName,
                    message: ResourceUtils.GetString(key: "InvalidMethodName")
                );
            }
        }
    }

    public override IList<string> ExpectedParameterNames(
        ISchemaItem item,
        string method,
        string parameter
    )
    {
        var result = new List<string>();
        IWorkflow wf = item as IWorkflow;
        ServiceMethodCallTask task = item as ServiceMethodCallTask;
        if (task != null)
        {
            wf = ResolveServiceMethodCallTask(task: task);
        }
        if (wf != null && method == "ExecuteWorkflow" && parameter == "Parameters")
        {
            foreach (
                var cs in wf.ChildItemsByType<ContextStore>(itemType: ContextStore.CategoryConst)
            )
            {
                result.Add(item: cs.Name);
            }
        }
        return result;
    }

    private IWorkflow ResolveServiceMethodCallTask(ServiceMethodCallTask task)
    {
        ISchemaItem wfParam = task.GetChildByName(name: "Workflow");
        if (wfParam.ChildItems.Count == 1)
        {
            WorkflowReference wfRef = wfParam.ChildItems[index: 0] as WorkflowReference;
            if (wfRef != null)
            {
                return wfRef.Workflow;
            }
        }
        return null;
    }
    #endregion
}
