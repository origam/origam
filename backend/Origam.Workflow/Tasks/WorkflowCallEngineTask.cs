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
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;

namespace Origam.Workflow.Tasks;

/// <summary>
/// Summary description for WorkflowCallEngineTask.
/// </summary>
public class WorkflowCallEngineTask : AbstractWorkflowEngineTask
{
    WorkflowEngine _call;

    public WorkflowCallEngineTask()
        : base() { }

    public override void Execute()
    {
        Exception exception = null;
        try
        {
            MeasuredExecution();
            SetWorkflowTrace();
            OnExecute();
        }
        catch (Exception ex)
        {
            exception = ex;
            OnFinished(e: new WorkflowEngineTaskEventArgs(exception: exception));
        }
    }

    private void SetWorkflowTrace()
    {
        WorkflowCallTask task = this.Step as WorkflowCallTask;
        switch (task.Trace)
        {
            case Trace.Yes:
            {
                _call.Trace = true;
                break;
            }

            case Trace.No:
            {
                _call.Trace = false;
                break;
            }

            case Trace.InheritFromParent:
            {
                break;
            }
        }
    }

    protected override void MeasuredExecution()
    {
        WorkflowCallTask task = this.Step as WorkflowCallTask;
        _call = this.Engine.GetSubEngine(
            block: task.Workflow,
            transactionBehavior: task.Workflow.TransactionBehavior
        );
        _call.Name = task.Workflow.Name;
        if (ProfilingTools.IsDebugEnabled)
        {
            OperationTimer.Global.Start(
                logEntryType: WorkflowItemType,
                path: Step is ISchemaItem schemItem ? schemItem.Path : "",
                id: Step.NodeId,
                hash: _call.GetHashCode()
            );
        }
    }

    protected override void OnExecute()
    {
        this.Engine.Host.WorkflowFinished += new WorkflowHostEvent(Host_WorkflowFinished);
        this.Engine.Host.WorkflowMessage += new WorkflowHostMessageEvent(Host_WorkflowMessage);
        WorkflowCallTask task = this.Step as WorkflowCallTask;
        // Fill input context stores
        foreach (
            var link in task.ChildItemsByType<ContextStoreLink>(
                itemType: ContextStoreLink.CategoryConst
            )
        )
        {
            if (link.Direction == ContextStoreLinkDirection.Input)
            {
                object val;
                try
                {
                    val = this.Engine.RuleEngine.EvaluateContext(
                        xpath: link.XPath,
                        context: this.Engine.RuleEngine.GetContext(
                            contextStore: link.CallerContextStore
                        ),
                        dataType: link.TargetContextStore.DataType,
                        targetStructure: link.TargetContextStore.Structure
                    );
                }
                catch (Exception ex)
                {
                    throw new OrigamException(
                        message: ResourceUtils.GetString(
                            key: "ErrorContextEvalFailed",
                            args: link.Path
                        ),
                        innerException: ex
                    );
                }

                _call.InputContexts.Add(key: link.TargetContextStore.PrimaryKey, value: val);
            }
        }
        // Run
        Engine.ExecuteSubEngineWorkflow(subEngine: _call);
    }

    private void Host_WorkflowFinished(object sender, WorkflowHostEventArgs e)
    {
        if (e.Engine.WorkflowUniqueId.Equals(g: _call.WorkflowUniqueId))
        {
            UnsubscribeEvents();
            if (e.Exception != null)
            {
                UnsubscribeEvents();
                OnFinished(e: new WorkflowEngineTaskEventArgs(exception: e.Exception));
                return;
            }
            WorkflowCallTask task = this.Step as WorkflowCallTask;
            // Fill output context stores
            if (task.OutputMethod != ServiceOutputMethod.Ignore)
            {
                foreach (
                    var link in task.ChildItemsByType<ContextStoreLink>(
                        itemType: ContextStoreLink.CategoryConst
                    )
                )
                {
                    if (link.Direction == ContextStoreLinkDirection.Output)
                    {
                        object val = _call.RuleEngine.EvaluateContext(
                            xpath: link.XPath,
                            context: _call.RuleEngine.GetContext(
                                contextStore: link.TargetContextStore
                            ),
                            dataType: link.CallerContextStore.DataType,
                            targetStructure: link.CallerContextStore.Structure
                        );

                        this.Engine.MergeContext(
                            resultContextKey: link.CallerContextStore.PrimaryKey,
                            inputContext: val,
                            step: task,
                            contextName: this.Engine.ContextStoreName(
                                key: link.CallerContextStore.PrimaryKey
                            ),
                            method: task.OutputMethod
                        );
                    }
                    // handle return context store
                    if (link.Direction == ContextStoreLinkDirection.Return)
                    {
                        this.Result = _call.RuleEngine.EvaluateContext(
                            xpath: link.XPath,
                            context: _call.RuleEngine.GetContext(
                                contextStore: link.TargetContextStore
                            ),
                            dataType: link.CallerContextStore.DataType,
                            targetStructure: link.CallerContextStore.Structure
                        );
                    }
                }
            }
            OnFinished(e: new WorkflowEngineTaskEventArgs(exception: e.Exception));
        }
    }

    private void Host_WorkflowMessage(object sender, WorkflowHostMessageEventArgs e)
    {
        if (e.Engine.WorkflowUniqueId.Equals(g: _call.WorkflowUniqueId))
        {
            if (e.Exception != null)
            {
                UnsubscribeEvents();
                OnFinished(e: new WorkflowEngineTaskEventArgs(exception: e.Exception));
            }
        }
    }

    private void UnsubscribeEvents()
    {
        this.Engine.Host.WorkflowFinished -= new WorkflowHostEvent(Host_WorkflowFinished);
        this.Engine.Host.WorkflowMessage -= new WorkflowHostMessageEvent(Host_WorkflowMessage);
    }
}
