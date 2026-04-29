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
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;

namespace Origam.Workflow.Tasks;

/// <summary>
/// Summary description for ServiceMethodCallTask.
/// </summary>
public class ServiceMethodCallEngineTask : AbstractWorkflowEngineTask
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    private IServiceAgent serviceAgent;
    private IServiceAgent ServiceAgent
    {
        get
        {
            if (serviceAgent == null)
            {
                ServiceMethodCallTask task = Step as ServiceMethodCallTask;
                ServiceAgentFactory serviceAgentFactory = new ServiceAgentFactory(
                    fromExternalAgent: externalAgent => new ExternalAgentWrapper(
                        externalServiceAgent: externalAgent
                    )
                );
                serviceAgent = serviceAgentFactory.GetAgent(
                    serviceName: task.Service.Name,
                    ruleEngine: this.Engine.RuleEngine,
                    workflowEngine: this.Engine
                );
            }
            return serviceAgent;
        }
    }

    public ServiceMethodCallEngineTask()
        : base() { }

    public override void Execute()
    {
        Exception exception = null;
        if (ServiceAgent is IAsyncAgent asyncAgent)
        {
            asyncAgent.AsyncCallFinished += OnAsyncAgentOnAsyncCallFinished;
        }

        try
        {
            MeasuredExecution();
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        if (ServiceAgent is IAsyncAgent)
        {
            if (exception != null)
            {
                OnAsyncAgentOnAsyncCallFinished(
                    sender: this,
                    args: new AsyncReturnValues { Exception = exception }
                );
            }
        }
        else
        {
            OnFinished(e: new WorkflowEngineTaskEventArgs(exception: exception));
        }
        if (ServiceAgent is IDisposable disposableServiceAgent)
        {
            disposableServiceAgent.Dispose();
        }
    }

    private void OnAsyncAgentOnAsyncCallFinished(object sender, AsyncReturnValues args)
    {
        if (ServiceAgent is IAsyncAgent asyncAgent)
        {
            asyncAgent.AsyncCallFinished -= OnAsyncAgentOnAsyncCallFinished;
        }
        Result = args.Result;
        OnFinished(e: new WorkflowEngineTaskEventArgs(exception: args.Exception));
    }

    protected override void OnExecute()
    {
        ServiceMethodCallTask task = Step as ServiceMethodCallTask;
        IServiceAgent agent = ServiceAgent;

        agent.Trace = Engine.IsTrace(workflowStep: task);
        agent.TraceStepName = task.Path;
        agent.TraceWorkflowId = this.Engine.WorkflowInstanceId;
        agent.TraceStepId = task.Id;
        agent.TransactionId = this.Engine.TransactionId;
        agent.Parameters.Clear();
        // Parameters
        foreach (
            var parameter in task.ChildItemsByType<ServiceMethodCallParameter>(
                itemType: ServiceMethodCallParameter.CategoryConst
            )
        )
        {
            if (parameter.HasChildItems)
            {
                // If there are more than 1 values for the parameter, we will return a hashtable,
                // where Key is Name of the value
                if (parameter.ServiceMethodParameter.DataType == OrigamDataType.Array)
                {
                    Hashtable paramList = new Hashtable(capacity: parameter.ChildItems.Count);
                    foreach (ISchemaItem item in parameter.ChildItems)
                    {
                        paramList.Add(key: item.Name, value: this.Evaluate(item: item));
                    }
                    if (log.IsDebugEnabled)
                    {
                        log.RunHandled(loggingAction: () =>
                        {
                            log.Debug(
                                message: "Passing array of values into parameter '"
                                    + parameter.Name
                                    + "'"
                            );
                            foreach (DictionaryEntry entry in paramList)
                            {
                                object v = entry.Value ?? "null";
                                if (v is IList list)
                                {
                                    v = "array: {";
                                    for (int i = 0; i < list.Count; i++)
                                    {
                                        object av = list[index: i];
                                        if (i != 0)
                                        {
                                            v += ", ";
                                        }

                                        if (av == null)
                                        {
                                            av = "null";
                                        }

                                        v += av.ToString();
                                    }
                                    v += "}";
                                }
                                log.Debug(
                                    message: "     Key: '" + entry.Key + "' Value: '" + v + "'"
                                );
                            }
                        });
                    }
                    agent.Parameters.Add(
                        key: parameter.ServiceMethodParameter.Name,
                        value: paramList
                    );
                }
                else
                {
                    if (parameter.ChildItems[index: 0] is ContextReference)
                    {
                        // We have to evaluate the context reference here, because here we have the context data
                        ContextReference contextReference =
                            parameter.ChildItems[index: 0] as ContextReference;
                        OrigamDataType targetType = parameter.ServiceMethodParameter.DataType;
                        if (
                            contextReference.CastToDataType != OrigamDataType.String
                            && targetType == OrigamDataType.Object
                        )
                        {
                            targetType = contextReference.CastToDataType;
                        }
                        object result = this.Engine.RuleEngine.EvaluateContext(
                            xpath: contextReference.XPath,
                            context: this.Engine.RuleEngine.GetContext(
                                contextStore: contextReference.ContextStore
                            ),
                            dataType: targetType,
                            targetStructure: null
                        );
                        agent.Parameters.Add(
                            key: parameter.ServiceMethodParameter.Name,
                            value: result
                        );
                        if (log.IsDebugEnabled)
                        {
                            log.Debug(
                                message: "Passing value into parameter '"
                                    + parameter.Name
                                    + "': "
                                    + (result == null ? "NULL" : result.ToString())
                            );
                        }
                    }
                    else
                    {
                        object result = this.Engine.RuleEngine.Evaluate(
                            item: parameter.ChildItems[index: 0]
                        );
                        agent.Parameters.Add(
                            key: parameter.ServiceMethodParameter.Name,
                            value: result
                        );
                        if (log.IsDebugEnabled)
                        {
                            log.Debug(
                                message: "Passing value into parameter '"
                                    + parameter.Name
                                    + "': "
                                    + (result == null ? "NULL" : result.ToString())
                            );
                        }
                    }
                }
            }
            else
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(message: "Parameter '" + parameter?.Name + "' has no value set.");
                }
            }
        }
        agent.MethodName = task.ServiceMethod.Name;
        agent.OutputStructure = task.OutputContextStore.Structure;
        agent.OutputMethod = task.OutputMethod;
        agent.DisableOutputStructureConstraints = task.OutputContextStore.DisableConstraints;

        ProfilingTools.SetCurrentTaskToThreadLoggingContext(task: task);
        agent.Run();

        ProfilingTools.ClearThreadLoggingContext();
        this.Result = agent.Result;
    }
}
