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
using System.Diagnostics;
using System.Security.Policy;
using log4net;
using Origam.Schema;
using Origam.Schema.WorkflowModel;

namespace Origam.Workflow.Tasks;

/// <summary>
/// Summary description for CheckRuleTask.
/// </summary>
public abstract class AbstractWorkflowEngineTask : IWorkflowEngineTask
{
    private WorkflowEngine _engine;
    private IWorkflowStep _step;
    private object _result;

    public AbstractWorkflowEngineTask() { }

    #region IWorkflowEngineTask Members
    public event WorkflowEngineTaskFinished Finished;

    protected virtual void OnFinished(WorkflowEngineTaskEventArgs e)
    {
        if (Step == null)
        {
            // if e.Exception is not null, it was handled in ServiceMethodCallEngineTask
            // in the Execute method. So it is safe to do nothing here.
            return;
        }
        if (this.Finished != null)
        {
            if (e.Exception != null && Step.OnFailure == StepFailureMode.Suppress)
            {
                e.Exception.Data["onFailure"] = Step.OnFailure;
            }
            this.Finished(this, e);
        }
    }

    public WorkflowEngine Engine
    {
        get { return _engine; }
        set { _engine = value; }
    }
    public IWorkflowStep Step
    {
        get { return _step; }
        set { _step = value; }
    }
    public object Result
    {
        get { return _result; }
        set { _result = value; }
    }
    protected virtual string WorkflowItemType => "Task";
    protected abstract void OnExecute();

    protected virtual void MeasuredExecution()
    {
        ProfilingTools.ExecuteAndLogDuration(
            action: OnExecute,
            logEntryType: WorkflowItemType,
            path: _step is ISchemaItem schemItem1 ? schemItem1.Path : "",
            id: _step.NodeId
        );
    }

    public virtual void Execute()
    {
        Exception exception = null;
        try
        {
            MeasuredExecution();
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        OnFinished(new WorkflowEngineTaskEventArgs(exception));
    }

    internal object Evaluate(ISchemaItem item)
    {
        try
        {
            ContextReference contextReference = item as ContextReference;
            if (contextReference != null)
            {
                // We have to evaluate the context reference here, because here we have the context data
                if (contextReference.ContextStore == null)
                {
                    throw new NullReferenceException(
                        ResourceUtils.GetString("ErrorNoContextStore", contextReference.Path)
                    );
                }
                return this.Engine.RuleEngine.EvaluateContext(
                    contextReference.XPath,
                    this.Engine.RuleEngine.GetContext(contextReference.ContextStore),
                    contextReference.CastToDataType,
                    null
                );
            }
            else
            {
                return this.Engine.RuleEngine.Evaluate(item);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed evaluating " + item.Path, ex);
        }
    }
    #endregion
}
