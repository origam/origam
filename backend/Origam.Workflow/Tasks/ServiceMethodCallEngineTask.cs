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
	private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	private IServiceAgent serviceAgent;
	private IServiceAgent ServiceAgent
	{
		get
		{
			if (serviceAgent == null)
			{
				ServiceMethodCallTask task = Step as ServiceMethodCallTask;
				ServiceAgentFactory serviceAgentFactory = 
					new ServiceAgentFactory(externalAgent => new ExternalAgentWrapper(externalAgent));
				serviceAgent = serviceAgentFactory.GetAgent(task.Service.Name,
					this.Engine.RuleEngine, this.Engine);
			}
			return serviceAgent;
		}
	}
	
	public ServiceMethodCallEngineTask() : base()
	{
	}
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
		catch(Exception ex)
		{
			exception = ex;
		}
		if (ServiceAgent is IAsyncAgent)
		{
			if(exception != null)
            {
				OnAsyncAgentOnAsyncCallFinished(this,
				new AsyncReturnValues { Exception = exception });
			}
		}
		else
		{
			OnFinished(new WorkflowEngineTaskEventArgs(exception));
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
		OnFinished(new WorkflowEngineTaskEventArgs(args.Exception));
	}
	protected override void OnExecute()
	{
		ServiceMethodCallTask task = Step as ServiceMethodCallTask;
		IServiceAgent agent = ServiceAgent;
		
        agent.Trace = Engine.IsTrace(task);
		agent.TraceStepName = task.Path;
		agent.TraceWorkflowId = this.Engine.WorkflowInstanceId;
		agent.TraceStepId = task.Id;
		agent.TransactionId = this.Engine.TransactionId;
		agent.Parameters.Clear();
		// Parameters
		foreach(var parameter in task.ChildItemsByType<ServiceMethodCallParameter>(ServiceMethodCallParameter.CategoryConst))
		{
			if(parameter.HasChildItems)
			{
				// If there are more than 1 values for the parameter, we will return a hashtable,
				// where Key is Name of the value
				if(parameter.ServiceMethodParameter.DataType == OrigamDataType.Array)
				{
					Hashtable paramList = new Hashtable(parameter.ChildItems.Count);
					foreach(ISchemaItem item in parameter.ChildItems)
					{
						paramList.Add(item.Name, this.Evaluate(item));
					}
					if (log.IsDebugEnabled)
					{
						log.RunHandled(() =>
						{
							log.Debug("Passing array of values into parameter '" + parameter.Name + "'");
							foreach(DictionaryEntry entry in paramList)
							{
								object v = entry.Value;
								if(v == null) v = "null";
								if(v is ArrayList)
								{
									v = "array: {";
									for(int i = 0; i < (entry.Value as ArrayList).Count; i++)
									{
										object av = (entry.Value as ArrayList)[i];
										if(i != 0) v+= ", ";
										if(av == null) av = "null";
										v += av.ToString();
									}
									v += "}";
								}
								log.Debug("     Key: '" + entry.Key + "' Value: '" + v.ToString() + "'");
							}
						});
					}
					agent.Parameters.Add(parameter.ServiceMethodParameter.Name, paramList);
				}
				else
				{
					if(parameter.ChildItems[0] is ContextReference)
					{
						// We have to evaluate the context reference here, because here we have the context data
						ContextReference contextReference = parameter.ChildItems[0] as ContextReference;
                        OrigamDataType targetType = parameter.ServiceMethodParameter.DataType;
                        if (contextReference.CastToDataType != OrigamDataType.String
                            && targetType == OrigamDataType.Object)
                        {
                            targetType = contextReference.CastToDataType;
                        }
						object result = this.Engine.RuleEngine.EvaluateContext(
							contextReference.XPath,
							this.Engine.RuleEngine.GetContext(contextReference.ContextStore),
                            targetType,
							null);
						agent.Parameters.Add(
							parameter.ServiceMethodParameter.Name,
							result	
							);
						if(log.IsDebugEnabled)
						{
							log.Debug("Passing value into parameter '" + parameter.Name + "': " + (result == null ? "NULL" : result.ToString()));
						}
					}
					else
					{
						object result = this.Engine.RuleEngine.Evaluate(parameter.ChildItems[0]);
						agent.Parameters.Add(parameter.ServiceMethodParameter.Name, result);
						if(log.IsDebugEnabled)
						{
							log.Debug("Passing value into parameter '" + parameter.Name + "': " + (result == null ? "NULL" : result.ToString()));
						}
					}					
				}
			}
			else
			{
				if(log.IsDebugEnabled)
				{
					log.Debug("Parameter '" + parameter?.Name + "' has no value set.");
				}
			}
		}
		agent.MethodName = task.ServiceMethod.Name;
		agent.OutputStructure = task.OutputContextStore.Structure;
		agent.OutputMethod = task.OutputMethod;
        agent.DisableOutputStructureConstraints = task.OutputContextStore.DisableConstraints;
		
		ProfilingTools.SetCurrentTaskToThreadLoggingContext(task);
		agent.Run();
		
		ProfilingTools.ClearThreadLoggingContext();
		this.Result = agent.Result;
	}
}
