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
using System.IO;
using System.Text;
using System.Xml;
using System.Collections;
using System.Data;
using System.Diagnostics;
using Origam.DA;
using Origam.DA.Service;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;
using Origam.Rule;
using Origam.Schema.RuleModel;
using log4net;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using Origam.Extensions;
using Origam.Service.Core;

namespace Origam.Workflow
{
	/// <summary>
	/// Summary description for Engine
	/// </summary>
	public class WorkflowEngine : IDisposable
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const string WORKFLOW_STATE_DATASTRUCTURE_ID = "fc6ab1cb-e6a0-43b1-ba9e-73b43b4ce787";

		//		Origam.Workbench.Pads.OutputPad _output  = WorkbenchSingleton.Workbench.GetPad(typeof(Origam.Workbench.Pads.OutputPad)) as Origam.Workbench.Pads.OutputPad;
		private DatasetGenerator _datasetGenerator = new DatasetGenerator(true);
		private IDocumentationService _documentationService;
		private Hashtable _taskResults = new Hashtable();
		private ITracingService _tracingService = ServiceManager.Services.GetService(typeof(ITracingService)) as ITracingService;
		private IParameterService _parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
		private Exception _exception;
		private Exception _caughtException;
		private readonly WorkflowStackTrace workflowStackTrace = new();
        public Boolean Trace { get; set; } = false;
        private readonly OperationTimer localOperationTimer = new OperationTimer();

        public WorkflowEngine(string transactionId = null)
		{
			this.WorkflowUniqueId = Guid.NewGuid();
			this.WorkflowInstanceId = this.WorkflowUniqueId;
			this.transactionId = transactionId;

			_documentationService = ServiceManager.Services.GetService(typeof(IDocumentationService)) as IDocumentationService;
		}

		#region Properties
		public Hashtable TaskResults
		{
			get
			{
				return _taskResults;
			}
		}

		public Exception Exception
		{
			get
			{
				return _exception;
			}
			set
			{
				_exception = value;
			}
		}

		public Exception CaughtException
		{
			get
			{
				return _caughtException;
			}
			set
			{
				_caughtException = value;
			}
		}

		Guid _id;
		public Guid WorkflowInstanceId
		{
			get
			{
				return _id;
			}
			set
			{
				_id = value;
			}
		}

		Guid _uniqueId;
		public Guid WorkflowUniqueId
		{
			get
			{
				return _uniqueId;
			}
			set
			{
				_uniqueId = value;
			}
		}

		string _name = "";
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		public string TransactionId
		{
			get => transactionId;
		}

		public void SetTransactionId(string transactionId, WorkflowTransactionBehavior transactionBehavior)
		{
			if (transactionBehavior ==
			    WorkflowTransactionBehavior.InheritExisting)
			{
				this.transactionId = transactionId;
			}
		}

		WorkflowTransactionBehavior _transactionBehavior 
            = WorkflowTransactionBehavior.InheritExisting;

        public WorkflowTransactionBehavior TransactionBehavior
        {
            get { return _transactionBehavior; }
            set { _transactionBehavior = value; }
        }

		IWorkflowBlock _workflowBlock;
		public IWorkflowBlock WorkflowBlock
		{
			get
			{
				return _workflowBlock;
			}
			set
			{
				_workflowBlock = value;
			}
		}

		WorkflowHost _host;
		public WorkflowHost Host
		{
			get
			{
				return _host;
			}
			set
			{
				_host = value;
			}
		}

		WorkflowEngine _callingWorkflow;
		public WorkflowEngine CallingWorkflow
		{
			get
			{
				return _callingWorkflow;
			}
			set
			{
				_callingWorkflow = value;
				
				if(_callingWorkflow != null)
				{
					// Inherit caller's workflow instance description
					if(this.RuntimeDescription == "")
					{
						this.RuntimeDescription = _callingWorkflow.RuntimeDescription;
					}

					if(this.Notification == "")
					{
						this.Notification = _callingWorkflow.Notification;
					}

					if(this.ResultMessage == "")
					{
						this.ResultMessage = _callingWorkflow.ResultMessage;
					}
				}
			}
		}

		RuleEngine _ruleEngine;
		public RuleEngine RuleEngine
		{
			get
			{
				return _ruleEngine;
			}
		}

		Hashtable _inputContexts = new Hashtable();
		/// <summary>
		/// Input context stores when this block is called as a subworkflow
		/// </summary>
		public Hashtable InputContexts
		{
			get
			{
				return _inputContexts;
			}
		}

		public object ReturnValue
		{
			get
			{
				foreach(IContextStore resultContext in this.WorkflowBlock.ChildItemsByType(ContextStore.CategoryConst))
				{
					if(resultContext.IsReturnValue)
					{
						return this.RuleEngine.GetContext(resultContext);
					}
				}

				return null;
			}
		}

		Hashtable _parentContexts = new Hashtable();
		/// <summary>
		/// Context stores of the parent block that this block will use
		/// </summary>
		public Hashtable ParentContexts
		{
			get
			{
				return _parentContexts;
			}
		}

		IPersistenceProvider _persistence;
		public IPersistenceProvider PersistenceProvider
		{
			get
			{
				return _persistence;
			}
			set
			{
				_persistence = value;
			}
		}

		int _iterationNumber = 0;
		public int IterationNumber
		{
			get
			{
				return _iterationNumber;
			}
			set
			{
				_iterationNumber = value;
			}
		}

		int _iterationTotal = 0;
		public int IterationTotal
		{
			get
			{
				return _iterationTotal;
			}
			set
			{
				_iterationTotal = value;
			}
		}

		string _runtimeDescription = "";
		public string RuntimeDescription
		{
			get
			{
				return _runtimeDescription;
			}
			set
			{
				_runtimeDescription = value;
			}
		}

		string _notification = "";
		public string Notification
		{
			get
			{
				return _notification;
			}
			set
			{
				_notification = value;
			}
		}

		string _resultMessage = "";
		public string ResultMessage
		{
			get
			{
				return _resultMessage;
			}
			set
			{
				_resultMessage = value;
			}
		}

		private bool _isRepeatable = false;
		private string transactionId = null;

		public bool IsRepeatable
		{
			get
			{
				return _isRepeatable;
			}
			set
			{
				_isRepeatable = value;
			}
		}

		#endregion

		#region Public Static Methods
		public static WorkflowEngine PrepareWorkflow(
            IWorkflow wf, Hashtable parameters, bool isRepeatable, 
            string titleName)
		{
			IPersistenceService persistence 
                = ServiceManager.Services.GetService(
                typeof(IPersistenceService)) as IPersistenceService;
			WorkflowEngine wfEngine = new WorkflowEngine();
			wfEngine.PersistenceProvider = persistence.SchemaProvider;
			wfEngine.IsRepeatable = isRepeatable;
			foreach(DictionaryEntry entry in parameters)
			{
				string name = (string)entry.Key;
				AbstractSchemaItem context = wf.GetChildByName(
                    name, ContextStore.CategoryConst);
				if(context == null)
				{
					throw new ArgumentOutOfRangeException(
                        "name", name, 
                        string.Format(
                        ResourceUtils.GetString("ErrorWorkflowParameterNotFound"), 
                        ((AbstractSchemaItem)wf).Path));
				}
				wfEngine.InputContexts.Add(context.PrimaryKey, entry.Value);
			}
            wfEngine.TransactionBehavior = wf.TransactionBehavior;
			wfEngine.WorkflowBlock = wf;
			wfEngine.Name = titleName;
			return wfEngine;
		}
		#endregion

		#region Public Methods
		public void RunWorkflowFromHost()
		{
			if (ProfilingTools.IsDebugEnabled)
			{
				localOperationTimer.Start(GetHashCode());
			}
			RunWorkflow();
		}

		private void LogWorkflowEnd(Stopwatch stopwatch)
		{
			ProfilingTools.LogDuration(
				logEntryType: "WF",
				path: Name, 
				id: _id.ToString(), 
				stoppedStopwatch: stopwatch);
			if (this.CallingWorkflow == null)
			{
				ProfilingTools.LogWorkFlowEnd();
			}
		}

		private void LogBlockIteration(Stopwatch stopwatch)
		{
			ProfilingTools.LogDuration(
				logEntryType: "Iteration",
				path: $"{((AbstractSchemaItem) WorkflowBlock).Path}/{_iterationNumber}",
				id: _id.ToString(),
				stoppedStopwatch: stopwatch);
		}

		private void RunWorkflow()
		{
			if (this.WorkflowBlock == null)
				throw new InvalidOperationException(
					ResourceUtils.GetString("ErrorNoWorkflow"));
			if (this.Host == null)
				throw new InvalidOperationException(
					ResourceUtils.GetString("ErrorNoHost"));

			_taskResults.Clear();

			try
			{
				if (log.IsDebugEnabled)
				{
					log.Debug("---------------------------------------------------------------------------------------");
					log.Debug("------------------- Starting workflow: " +this.WorkflowBlock.Name);
					log.Debug("------------------- Transaction ID: " +this.TransactionId);
					log.Debug("---------------------------------------------------------------------------------------");
				}
				if (this.CallingWorkflow == null)
				{
					this.RuntimeDescription = "";
					this.Notification = "";
					this.ResultMessage = "";

					if (IsTrace(this.WorkflowBlock))
					{
						_tracingService.TraceWorkflow(this.WorkflowInstanceId,
							(Guid) this.WorkflowBlock.PrimaryKey["Id"],
							this.WorkflowBlock.Name);
					}
				}

				// Initialize all context stores (resume paused ones?)
				Hashtable stores = new Hashtable();

				// Initialize RuleEngine for this session
				Guid tracingWorkflowId = IsTrace(WorkflowBlock) 
					? WorkflowInstanceId 
					: Guid.Empty;
				_ruleEngine = RuleEngine.Create(stores, TransactionId, tracingWorkflowId);

				foreach (IContextStore store in this.WorkflowBlock.ChildItemsByType(
					ContextStore.CategoryConst))
				{
					if (log.IsDebugEnabled)
					{
						log.Debug("Initializing data store: " + store?.Name);
					}
					// Otherwise we generate an empty store
					if (store.DataType == OrigamDataType.Xml)
					{
						DataStructure dataStructure = store.Structure as DataStructure;
						if (dataStructure != null)
						{
							DataSet dataset =
								_datasetGenerator.CreateDataSet(dataStructure,
									store.DefaultSet);
							if (store.DisableConstraints)
							{
								dataset.EnforceConstraints = false;
							}

							stores.Add(store.PrimaryKey, DataDocumentFactory.New(dataset));
						} else if (store.Structure is XsdDataStructure)
						{
							stores.Add(store.PrimaryKey, new XmlContainer());
						} else if (store.Structure == null)
						{
							throw new NullReferenceException(
								ResourceUtils.GetString("ErrorNoXmlStore"));
						} else
						{
							throw new ArgumentOutOfRangeException("DataType",
								store.DataType,
								ResourceUtils.GetString("ErrorUnsupportedXmlStore"));
						}
					} else
					{
						stores.Add(store.PrimaryKey, null);
					}

					if (this.InputContexts.ContainsKey(store.PrimaryKey))
					{
						// If we have input data, we use them
						if (log.IsDebugEnabled)
						{
							log.Debug("Passing input context");
						}
						if (IsTrace(this.WorkflowBlock))
						{
							_tracingService.TraceStep(this.WorkflowInstanceId,
								(this.WorkflowBlock as AbstractSchemaItem).Path,
								Guid.Empty, "Input Context", store.Name, "",
								ContextData(this.InputContexts[store.PrimaryKey]), null,
								null);
						}

						MergeContext(store.PrimaryKey,
							this.InputContexts[store.PrimaryKey], null, store.Name,
							ServiceOutputMethod.AppendMergeExisting);
					}
				}

				// Include all contexts from the parent block
				foreach (DictionaryEntry entry in this.ParentContexts)
				{
					stores.Add(entry.Key, entry.Value);
				}

				ArrayList tasks =
					this.WorkflowBlock.ChildItemsByType(WorkflowTask.CategoryConst);

				// Set states of each task to "not run"
				foreach (IWorkflowStep task in tasks)
				{
					SetStepStatus(task, WorkflowStepResult.Ready);
				}

				// clear input contexts - they will not be needed anymore
				this.InputContexts.Clear();

				ResumeWorkflow();
			} catch (Exception ex)
			{
				HandleWorkflowException(ex);
			}
		}

        public bool IsTrace(IWorkflowStep workflowStep)
        {
			// step can be null e.g. when called from workflow screen in Architect
			if(workflowStep == null)
            {
				return false;
            }

	        if (workflowStep is Schema.WorkflowModel.Workflow &&
	            workflowStep.TraceLevel == Origam.Trace.InheritFromParent)
	        {
		        return Trace;
	        }

	        switch (workflowStep.Trace)
            {
                // when all workflow has InheritFromParent then gets Trace from Parent Workflow
                case Origam.Trace.InheritFromParent:
                    return Trace;
                case Origam.Trace.Yes:
                    return true;
                case Origam.Trace.No:
                    return false;
                default:
                    return false;
            }
        }

        private void ResumeWorkflow()
		{
			ArrayList tasks = this.WorkflowBlock.ChildItemsByType(WorkflowTask.CategoryConst);

			if(tasks.Count == 0)
			{
				FinishWorkflow(null);
				return;
			}

			for(int i = 0; i < tasks.Count; i++)
			{
				if(WorkflowCompleted())
				{
					FinishWorkflow(null);
					break;
				}

				// Check if the task is ready to run by start event
				if(CanStepRun(tasks[i] as IWorkflowStep))
				{
					// Now check if the task will ever run by startup rule
					if(EvaluateStartRuleTimed(tasks[i] as IWorkflowStep))
					{
						IWorkflowStep currentModelStep = tasks[i] as IWorkflowStep;

						IWorkflowEngineTask engineTask = WorkflowTaskFactory.GetTask(currentModelStep);
						engineTask.Engine = this;
						engineTask.Step = currentModelStep;

						if(log.IsDebugEnabled)
						{
							log.Debug("---------------------------------------------------------------------------------------");
							log.Debug("Starting " + engineTask.GetType().Name + ": " + currentModelStep?.Name);
						}
						workflowStackTrace.RecordStepStart(WorkflowBlock.Name, currentModelStep?.Name);
						SetStepStatus(currentModelStep, WorkflowStepResult.Running);
						engineTask.Finished += new WorkflowEngineTaskFinished(engineTask_Finished);
						engineTask.Execute();
						break;
					}
					else
					{
						// Task will never run, startup rule returned false
						SetStepStatus(tasks[i] as IWorkflowStep, WorkflowStepResult.NotRun);
					}
				}	// end CanStepRun

				if(i == tasks.Count - 1)
				{
					// let's start over
					i = -1;
				}
			}	// end loop through steps
		}

		private void HandleStepException(IWorkflowStep step, Exception ex)
		{
			SetStepStatus(step, WorkflowStepResult.Failure);
			if(log.IsErrorEnabled)
			{
				log.Error($"{step?.GetType().Name} {(step as AbstractSchemaItem)?.Path} failed.");
			}
			// Trace the error
			if(IsTrace(step))
			{
				_tracingService.TraceStep(this.WorkflowInstanceId, (step as AbstractSchemaItem).Path, (step as AbstractSchemaItem).Id, "Process", "Error", null, null, null, ex.Message);
			}

			// suppress all tasks that had not run yet and have no dependencies
			ArrayList tasks = this.WorkflowBlock.ChildItemsByType(WorkflowTask.CategoryConst);
			for(int i = 0; i < tasks.Count; i++)
			{
				IWorkflowStep siblingStep = tasks[i] as IWorkflowStep;

				if(siblingStep.Dependencies.Count == 0 && (WorkflowStepResult)_taskResults[siblingStep.PrimaryKey] == WorkflowStepResult.Ready)
				{
					SetStepStatus(siblingStep, WorkflowStepResult.NotRun);
				}
			}

			if(IsFailureHandled(step))
			{
				this.CaughtException = ex;
				return;
			}

			// cancel the workflow and rethrow the exception up, if root workflow
			HandleWorkflowException(GetStepException(ex, step.Name));
		}

		/// <summary>
		/// Returns true if there is a task in the workflow that handles failures.
		/// </summary>
		/// <returns></returns>
		private bool IsFailureHandled(IWorkflowStep failedStep)
		{
			ArrayList tasks = WorkflowBlock.ChildItemsByType(WorkflowTask.CategoryConst);
			foreach (IWorkflowStep step in tasks)
			{
				var dependencyOnFailedStep = step.Dependencies
					.Cast<WorkflowTaskDependency>()
					.FirstOrDefault(dependency => dependency.Task == failedStep);
				if (dependencyOnFailedStep != null &&
				    dependencyOnFailedStep.StartEvent == WorkflowStepStartEvent.Failure)
				{
					return true;
				}
			}

			return false;
		}

		private void HandleWorkflowException(Exception exception)
		{
			Exception = exception;
			var keys = new ArrayList(_taskResults.Keys);
			foreach (object key in keys)
			{
				if ((WorkflowStepResult)_taskResults[key] 
                        == WorkflowStepResult.Ready)
				{
					_taskResults[key] = WorkflowStepResult.NotRun;
				}
			}
			if (IsTrace(WorkflowBlock))
			{
				string recursiveExceptionText = exception.Message;
				Exception recursiveEx = exception;
				while (recursiveEx.InnerException != null)
				{
					recursiveExceptionText += Environment.NewLine 
						   + "-------------------------------- " 
						   + Environment.NewLine 
						   + recursiveEx.InnerException.Message;
					recursiveEx = recursiveEx.InnerException;
				}
				_tracingService.TraceStep(
					workflowInstanceId: WorkflowInstanceId, 
					stepPath: (WorkflowBlock as AbstractSchemaItem)?.Path, 
					stepId: (Guid)WorkflowBlock.PrimaryKey["Id"], 
					category: "Process", 
					subCategory: "Error", 
					remark: null, 
					data1: recursiveExceptionText, 
					data2: recursiveEx.StackTrace, 
					exception.Message);
			}
            if (exception is not WorkflowCancelledByUserException 
                && log.IsErrorEnabled)
            {
	            log.LogOrigamError(
		            $"{exception.Message}\n{workflowStackTrace}", exception);
            }
			FinishWorkflow(exception);
		}

		private void SetStepStatus(IWorkflowStep step, WorkflowStepResult status)
		{
			_taskResults[step.PrimaryKey] = status;
		}

		private WorkflowStepResult StepStatus(IWorkflowStep step)
		{
			return (WorkflowStepResult)_taskResults[step.PrimaryKey];
		}

		public void EvaluateEndRule(IEndRule rule, object data)
		{
			EvaluateEndRule(rule, data, null);
		}

		public void EvaluateEndRuleTimed(IEndRule rule, object data, IWorkflowStep step)
		{
			ProfilingTools.ExecuteAndLogDuration(
				action: () => EvaluateEndRule(rule, data, step),
				logEntryType: "Validation Rule",
				task: step);
		}

		public void EvaluateEndRule(IEndRule rule, object data, IWorkflowStep step)
		{
			if(rule == null) return;

			RuleExceptionDataCollection result = this.RuleEngine.EvaluateEndRule(
				rule: rule, 
				data: data, 
				parentIsTracing: IsTrace(step)
			);

			if(step != null && IsTrace(step))
			{
				_tracingService.TraceStep(
					this.WorkflowInstanceId,
					(step as AbstractSchemaItem).Path,
					(Guid)step.PrimaryKey["Id"],
					"End Rule",
					"Input",
					step.ValidationRuleContextStore.Name,
					ContextData(data),
					null,
					null);
			}

			// if there are some exceptions, we actually throw them
			if(result != null && result.Count != 0)
			{
				throw new RuleException(result);
			}		
		}

		public Exception GetStepException(Exception ex, string stepName)
		{
			if(ex is WorkflowCancelledByUserException || ex is RuleException)
			{
				return ex;
			}

			return new OrigamException(ex.Message, stepName, ex);
		}
		#endregion

		#region Private Methods
		private bool WorkflowCompleted()
		{
			foreach(DictionaryEntry entry in _taskResults)
			{
				WorkflowStepResult result = (WorkflowStepResult)entry.Value;

				if(result == WorkflowStepResult.Ready || result == WorkflowStepResult.Running)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Checks if specified task is ready to run.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		private bool CanStepRun(IWorkflowStep step)
		{
			// Check if this task has been already completed, don't run it again
			if(StepStatus(step) != WorkflowStepResult.Ready)
				return false;

			bool result = true;

			foreach(WorkflowTaskDependency dependency in step.Dependencies)
			{
				try
				{
					if(!_taskResults.Contains(dependency.Task.PrimaryKey))
					{
						throw new Exception("Workflow task dependency invalid. Task: " + step.Name);
					}
				}
				catch(Exception ex)
				{
					throw new Exception("Workflow task dependency invalid. Task: " + step.Name, ex);
				}

				WorkflowStepResult dependencyResult = StepStatus(dependency.Task);

				if(dependencyResult == WorkflowStepResult.Running)
				{
					return false;
				}

				if(dependencyResult == WorkflowStepResult.FailureNotRun)
				{
					SetStepStatus(step, WorkflowStepResult.FailureNotRun);
					return false;
				}
				
				if(dependencyResult == WorkflowStepResult.NotRun)
				{
					// If dependent task did not run and we don't care about result, it's ok
					if(dependency.StartEvent != WorkflowStepStartEvent.Finish)
					{
						// We check if any of tasks we depend on has state NotRun.
						// In that case current task will not run as well.
						SetStepStatus(step, WorkflowStepResult.NotRun);
						return false;
					}
				}
				else if((dependencyResult == WorkflowStepResult.Success | dependencyResult == WorkflowStepResult.NotRun) &
					dependency.StartEvent == WorkflowStepStartEvent.Failure)
				{
					// for failures we only start tasks marked to start on failure
					SetStepStatus(step, WorkflowStepResult.FailureNotRun);
					return false;
				}
				else if(dependencyResult == WorkflowStepResult.Failure & dependency.StartEvent != WorkflowStepStartEvent.Failure)
				{
					SetStepStatus(step, WorkflowStepResult.FailureNotRun);
					return false;
				}
				else if(dependencyResult == WorkflowStepResult.Ready)
				{
					// The dependent task did not run, yet. So we still have to wait a bit.
					return false;
				}

			}

			return result;
		}

		private bool EvaluateStartRuleTimed(IWorkflowStep task)
		{
			string path = task is AbstractSchemaItem schemItem ? schemItem.Path : "";
			
			return ProfilingTools.ExecuteAndLogDuration(
				funcToExecute: () => EvaluateStartRule(task),
				logEntryType: "Start Rule",
				path: path + "/Start Rule",
				id: task.NodeId,
				logOnlyIf: () => task.StartConditionRule != null);
		}

		private bool EvaluateStartRule(IWorkflowStep task)
		{
// check features
			if (!_parameterService.IsFeatureOn(task.Features))
			{
				if (log.IsDebugEnabled)
				{
					log.Debug(
						"Step will not execute because of feature being turned off.");
				}
				return false;
			}

			if (!SecurityManager.GetAuthorizationProvider()
				.Authorize(SecurityManager.CurrentPrincipal, task.Roles))
			{
				if (log.IsDebugEnabled)
				{
					log.Debug(
						"Step will not execute because the user has not been authorized.");
				}
				return false;
			}

			// If there is no start rule, we always start the task
			if (task.StartConditionRule == null)
				return true;

			bool result = true;

			if (log.IsDebugEnabled)
			{
				log.Debug("Evaluating startup rule for step " + task?.Name);
			}
			result = (bool) this.RuleEngine.EvaluateRule(
				task.StartConditionRule, task.StartConditionRuleContextStore, null,
				IsTrace(task));
			if (log.IsDebugEnabled)
			{
				log.Debug("Rule evaluated and returned " + result.ToString());
			}
			return result;
		}

		internal void EvaluateEndRule(IWorkflowStep step)
		{
			// If there is no validation rule, we return
			if(step.ValidationRule == null)
				return;

			if(log.IsDebugEnabled)
			{
				log.Debug("Evaluating validation rule for step " + step?.Name);
			}
			RuleExceptionDataCollection result = 
				this.RuleEngine.EvaluateEndRule(step.ValidationRule, step.ValidationRuleContextStore);

			if (result == null)
			{
				throw new OrigamException(String.Format("Programming error: there is not any " +
					"RuleExceptionDataCollection in the output of the validation rule `{0}' ({1}). " +
					"Please review the rule and add <RuleExceptionDataCollection> tag.",
                    step.ValidationRule.Name, step.ValidationRule.PrimaryKey));
			}

			// if there are some exceptions, we actually throw them
			if(result.Count != 0)
			{
				throw new RuleException(result);
			}
		}

		internal WorkflowEngine GetSubEngine(IWorkflowBlock block, WorkflowTransactionBehavior transactionBehavior)
		{
			WorkflowEngine call = new WorkflowEngine();

			// Set same properties as we have
			//call.DataService = this.DataService;
			call.PersistenceProvider = this.PersistenceProvider;
			call.CallingWorkflow = this;
			call.WorkflowBlock = block;
			call.Host = this.Host;
            call.TransactionBehavior = transactionBehavior;
			call.WorkflowInstanceId = this.WorkflowInstanceId;
			call.SetTransactionId(TransactionId, transactionBehavior);

			call.IterationTotal = this.IterationTotal;
			call.IterationNumber = this.IterationNumber;
            call.Trace = this.Trace;
			return call;
		}

		internal void ExecuteSubEngineWorkflow(WorkflowEngine subEngine)
		{
			this.Host.ExecuteWorkflow(subEngine);
		}

		private object CloneContext(object context)
		{
			return CloneContext(context, false);
		}

		internal object CloneContext(object context, bool returnDataSet)
		{
			if(context is IDataDocument)
			{
				if(returnDataSet)
				{
					return (context as IDataDocument).DataSet.Copy();
				}
				else
				{
					return DataDocumentFactory.New((context as IDataDocument).DataSet.Copy());
				}
			}
			else if(context is IXmlContainer)
			{
				return (context as IXmlContainer).Clone();

			}
			else
			{
				// value types - we return the value itself, don't need a clone
				return context;
			}
		}

		public IWorkflowStep Step(Key key)
		{
			IWorkflowStep item = this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key) as IWorkflowStep;
			return item;
		}

		internal string ContextStoreName(Key key)
		{
			ISchemaItem item = this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key) as ISchemaItem;

			return item.Name;
		}

		internal DataStructureRuleSet ContextStoreRuleSet(Key key)
		{
			ContextStore cs = this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key) as ContextStore;

			return cs.RuleSet;
		}

		internal OrigamDataType ContextStoreType(Key key)
		{
			ContextStore cs = this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key) as ContextStore;

			return cs.DataType;
		}

		internal bool MergeContext(Key resultContextKey, object inputContext, IWorkflowStep step, string contextName, ServiceOutputMethod method)
		{
			if(method == ServiceOutputMethod.Ignore) return false;

			object resultContext = this.RuleEngine.GetContext(resultContextKey);
			bool changed = false;

			string stepName = "";
			if(step != null) stepName = "Step '" + step.Name + "'";

			if(log.IsInfoEnabled)
			{
				string stepNameLog = "";
				if(step != null) stepNameLog = ", Step '" + (step as AbstractSchemaItem)?.Path + "'";
				log.Info("Merging context '" + contextName + "'" + stepNameLog);
			}

			try
			{
				if(step != null && IsTrace(step))
				{
					_tracingService.TraceStep(
						this.WorkflowInstanceId,
						(step as AbstractSchemaItem).Path,
						(Guid)step.PrimaryKey["Id"],
						"Merge Context",
						"Input",
						contextName,
						ContextData(inputContext),
						null,
						null);
				}
                var inputDataDoc = inputContext as IDataDocument;
                var inputXmlDoc = inputContext as IXmlContainer;
                var resultDataDoc = resultContext as IDataDocument;
                var resultXmlDoc = resultContext as IXmlContainer;


                if (inputContext == null || inputContext == DBNull.Value)
				{
					return changed;
				}
				else if(inputDataDoc != null 
                    && resultDataDoc != null)
				{
					DataSet input = inputDataDoc.DataSet;
					DataSet output = resultDataDoc.DataSet;

					if(method == ServiceOutputMethod.AppendMergeExisting 
                        || method == ServiceOutputMethod.FullMerge)
					{
						changed = this.RuleEngine.Merge(output, input, 
                            method == ServiceOutputMethod.FullMerge, false, false, true);
					}
					else if(method == ServiceOutputMethod.DeleteMatches)
					{
						foreach(DataTable it in input.Tables)
						{
							if(output.Tables.Contains(it.TableName))
							{
								DataTable ot = output.Tables[it.TableName];

								foreach(DataRow ir in it.Rows)
								{
									object[] irPK = DatasetTools.PrimaryKey(ir);
									DataRow toDelete = ot.Rows.Find(irPK);
									if(toDelete != null)
									{
										toDelete.Delete();
                                        changed = true;
									}
								}
							}
						}
					}
					else
					{
						throw new ArgumentOutOfRangeException("method", method, "Unsupported merge method.");
					}
				}
				else if(inputXmlDoc != null)
				{
					IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
					ContextStore cs = persistence.SchemaProvider.RetrieveInstance(typeof(ContextStore), resultContextKey) as ContextStore;
					if(cs.DataType == OrigamDataType.String)
					{
						RuleEngine.SetContext(resultContextKey, inputXmlDoc.Xml.InnerText);
					}
					else
					{
						if(resultXmlDoc == null)
                            throw new Exception("Cannot merge data into a context, which is not XML type. Context: " 
						        + contextName
						        + ", type: " + (resultContext == null ? "NULL" : resultContext.GetType().ToString()));
						changed = true;
						if(inputXmlDoc.Xml.DocumentElement != null)
						{
							// copy document element, if it does not exist already
							if(resultXmlDoc.Xml.DocumentElement == null)
							{
								if (resultDataDoc != null)
								{
									bool previousEnforceConstraints = false;
									previousEnforceConstraints = resultDataDoc.DataSet.EnforceConstraints;
                                    resultDataDoc.DataSet.EnforceConstraints = false;
                                    resultDataDoc.AppendChild(inputXmlDoc.Xml.DocumentElement, true);
									try {
                                        resultDataDoc.DataSet.EnforceConstraints = previousEnforceConstraints;
									} 
									catch (Exception ex)
									{
										throw new Exception(DebugClass.ListRowErrors(resultDataDoc.DataSet), ex);
									}
								}
								else
								{
									XmlNode newDoc = resultXmlDoc.Xml.ImportNode(inputXmlDoc.Xml.DocumentElement, true);
									resultXmlDoc.Xml.AppendChild(newDoc);
								}
							}
							else
							{
								// otherwise copy each sub node
								foreach(XmlNode node in inputXmlDoc.Xml.DocumentElement.ChildNodes)
								{
									if(!(node is XmlDeclaration))
									{
										resultXmlDoc.DocumentElementAppendChild(node);
									}
								}
							}
						}
					}
				}
				else
				{
					// Web Service support - they send XML as string
					changed = true;

				    IXmlContainer resultXml = this.RuleEngine.GetContext(resultContextKey) as IXmlContainer;
				    IDataDocument xmlDataDoc = resultXml as IDataDocument;

                    bool previousEnforceConstraints = false;

					if(xmlDataDoc != null)
					{
						previousEnforceConstraints = xmlDataDoc.DataSet.EnforceConstraints;
						xmlDataDoc.DataSet.EnforceConstraints = false;
					}

					string inputString = inputContext as string;

					if(resultXml != null & inputString != null)
					{
						resultXml.LoadXml(inputString);

						if(xmlDataDoc != null)
						{
							// set default values (loading xml will not set them automatically)
							foreach(DataTable t in xmlDataDoc.DataSet.Tables)
							{
								foreach(DataRow r in t.Rows)
								{
									foreach(DataColumn c in t.Columns)
									{
										if(c.AllowDBNull == false && c.DefaultValue != null && r[c] == DBNull.Value)
										{
											r[c] = c.DefaultValue;
										}
									}
								}
							}

							try
							{
								xmlDataDoc.DataSet.EnforceConstraints = previousEnforceConstraints;
							}
							catch(Exception ex)
							{
								throw new Exception(DebugClass.ListRowErrors(xmlDataDoc.DataSet), ex);
							}

							object profileId = SecurityManager.CurrentUserProfile().Id;

							foreach(DataTable t in xmlDataDoc.DataSet.Tables)
							{
								foreach(DataRow r in t.Rows)
								{
									DatasetTools.UpdateOrigamSystemColumns(r, true, profileId);
								}
							}
						}
					}
						// everything else (simple data types) - we just copy the value
					else
					{
						OrigamDataType contextType = ContextStoreType(resultContextKey);
						RuleEngine.ConvertStringValueToContextValue(contextType, inputString, ref inputContext);
						this.RuleEngine.SetContext(resultContextKey, inputContext);
					}
				}

				if(step != null && IsTrace(step))
				{
					_tracingService.TraceStep(
						this.WorkflowInstanceId,
						(step as AbstractSchemaItem).Path,
						(Guid)step.PrimaryKey["Id"],
						"Merge Context",
						"Result",
						contextName,
						changed ? ContextData(this.RuleEngine.GetContext(resultContextKey)) : "-- no change --",
						null,
						null);
				}

				DataStructureRuleSet ruleSet = ContextStoreRuleSet(resultContextKey);

				if(changed & ruleSet != null && (step == null || step is IWorkflowTask || step is CheckRuleStep) && !(step is UIFormTask))
				{
					
					ProcessRulesTimed(resultContextKey, ruleSet, step);

					if(step != null && IsTrace(step))
					{
						_tracingService.TraceStep(
							this.WorkflowInstanceId,
							(step as AbstractSchemaItem).Path,
							(Guid)step.PrimaryKey["Id"],
							"Rule Processing",
							"Result",
							contextName,
							ContextData(this.RuleEngine.GetContext(resultContextKey)),
							null,
							null);
					}

					if(step == null && IsTrace(this.WorkflowBlock))
					{
						_tracingService.TraceStep(
							this.WorkflowInstanceId,
							(this.WorkflowBlock as AbstractSchemaItem).Path,
							(Guid)this.WorkflowBlock.PrimaryKey["Id"],
							"Rule Processing",
							"Result",
							contextName,
							ContextData(this.RuleEngine.GetContext(resultContextKey)),
							null,
							null);
					}
				}
			}
			catch (Exception ex)
			{
				string stepNameLog = "";
				if(step != null) stepNameLog = ", Step '" + (step as AbstractSchemaItem).Path + "'";
				string inputString = inputContext as string;
				if (inputString == null)
				{
					inputString = "";
				}
				throw new Exception("Merge context '" + contextName + "'" + stepNameLog 
					+ " failed. InputContextValue: " + inputString 
					+ ". Original exception message: " + ex.Message, ex);
			}

			if(log.IsInfoEnabled)
			{
				string stepNameLog = "";
				if(step != null) stepNameLog = ", Step '" + (step as AbstractSchemaItem)?.Path + "'";
				log.Info("Finished merging context '" + contextName + "'" + stepNameLog);
			}
			return changed;
		}

        private void ProcessRulesTimed(Key resultContextKey,
			DataStructureRuleSet ruleSet,IWorkflowStep step)
		{				
			ProfilingTools.ExecuteAndLogDuration(
				action: () =>
				{
					_ruleEngine.ProcessRules(
						data: this.RuleEngine.GetContext(resultContextKey) 
							as IDataDocument,
						ruleSet: ruleSet,
						contextRow: null);
				},
				logEntryType: "Context RuleSet",
				task:step);
		}

		public static string ContextData(object context)
		{
			if(context is IXmlContainer)
			{
				StringBuilder b = new StringBuilder();
				StringWriter swr = new StringWriter(b);
				XmlTextWriter xwr = new XmlTextWriter(swr);
				xwr.Formatting = Formatting.Indented;
				(context as IXmlContainer).Xml.WriteTo(xwr);
				xwr.Close();
				swr.Close();

				return b.ToString();
			}
			else if(context is DataSet)
			{
				return (context as DataSet).GetXml();
			}
			else if(context == null)
			{
				return "";
			}
			else
			{
				return context.ToString();
			}
		}

		internal string GetTaskDescription(IWorkflowStep task)
		{
			string append = this.RuntimeDescription;
			if(append != "") append = " " + append;

			IDocumentationService documentationService = ServiceManager.Services.GetService(typeof(IDocumentationService)) as IDocumentationService;

			if(documentationService == null)
			{
				return task.Name + append;
			}
			else
			{
				string doc = documentationService.GetDocumentation(
					(Guid)task.PrimaryKey["Id"],
					DocumentationType.USER_WFSTEP_DESCRIPTION
					);

				if(string.IsNullOrEmpty(doc))
				{
					doc = task.Name;
				}

				doc += append;

				if(this.IterationTotal > 0)
				{
					doc += " (" + this.IterationNumber.ToString() + "/" + this.IterationTotal.ToString() + ")";
				}

				return doc;
			}
		}

		#endregion

		private void engineTask_Finished(object sender, WorkflowEngineTaskEventArgs e)
		{
			IWorkflowEngineTask engineTask = sender as IWorkflowEngineTask;
			IWorkflowStep currentModelStep = engineTask.Step;

			try
			{
				if(e.Exception == null)
				{
					IWorkflowTask task = currentModelStep as IWorkflowTask;

					// Check if results are ok with the EndRule
					if(currentModelStep.ValidationRule != null)
					{
						if(currentModelStep.ValidationRuleContextStore == null) throw new NullReferenceException("End Rule Context Store is not set. Task '" + task.Name + "'");

						object validationRuleStore = this.RuleEngine.GetContext(currentModelStep.ValidationRuleContextStore);

						if(task != null && task.OutputContextStore.PrimaryKey.Equals(task.ValidationRuleContextStore.PrimaryKey))
						{
							validationRuleStore = engineTask.Result;
						}

						try
						{
							EvaluateEndRuleTimed(currentModelStep.ValidationRule, validationRuleStore, currentModelStep);
						}
						catch(RuleException ruleException)
						{
							if(ruleException.IsSeverityHigh) throw ruleException;
						}
					}

					if(task != null)
					{
						if(task.OutputContextStore == null && task.OutputMethod != ServiceOutputMethod.Ignore)
						{
							throw new NullReferenceException(ResourceUtils.GetString("ErrorNoOutputContext", task.Name));
						}

						// nothing has happen after evaluating end rule, we process the results
						bool doMerge = true;
#if ORIGAM_SERVER
						if(task is UIFormTask && task.OutputMethod == ServiceOutputMethod.FullMerge)
						{
							doMerge = false;
						}
#endif
						if(doMerge)
						{
							ProfilingTools.ExecuteAndLogDuration(
								action: () =>
								{
									MergeContext(
										task.OutputContextStore.PrimaryKey,
										engineTask.Result,
										task, 
										task.OutputContextStore.Name,
										task.OutputMethod);
								},
								logEntryType: "Merge",
								task: task);
						}
					}

					SetStepStatus(currentModelStep, WorkflowStepResult.Success);

					if(log.IsDebugEnabled)
					{
						log.Debug(engineTask.GetType().Name + " " + currentModelStep?.Name + " finished successfully.");
					}

					if(this.Host.SupportsUI)
					{
						ProfilingTools.ExecuteAndLogDuration(
							action: () =>
							{
								this.Host.OnWorkflowUserMessage(
									this,
									this.GetTaskDescription(currentModelStep), 
									null,
									false);
							},
							logEntryType: "Documentation",
							task: task);
					}
				}
				else
				{
					HandleStepException(currentModelStep, e.Exception);
				}
			}
			catch(Exception ex)
			{
				HandleStepException(currentModelStep, ex);
			}
			finally
			{
				engineTask.Engine = null;
				engineTask.Step = null;
			}

			ResumeWorkflow();
		}

		private void FinishWorkflow(Exception exception)
		{
			if(this.Host == null) return; // already disposed from the host from a preceding task

			if(log.IsDebugEnabled)
			{
				log.Debug("Block '" + this.WorkflowBlock?.Name + "' completed");

				// Show finish screen if this is the root workflow
				if(this.CallingWorkflow == null)
				{
					log.Debug("Workflow completed");
				}
			}
			if (ProfilingTools.IsDebugEnabled)
			{
				Stopwatch stopwatch = localOperationTimer.Stop(GetHashCode());
				if (string.IsNullOrEmpty(Name))
				{
					LogBlockIteration(stopwatch);
				} else
				{
					LogWorkflowEnd(stopwatch);
				}
				OperationTimer.Global.StopAndLog(GetHashCode());
			}
			if(exception == null && this.CaughtException != null)
			{
				if(log.IsDebugEnabled)
				{
					log.Debug("Throwing caught exception " + this.CaughtException);
				}
				this.Exception = this.CaughtException;
				exception = this.CaughtException;
			}
			string path = string.IsNullOrEmpty(Name)
				? $"{((AbstractSchemaItem) WorkflowBlock).Path}/{_iterationNumber}"
				: Name ;

			this.Host.OnWorkflowFinished(this, exception);
		}

		#region IDisposable Members

		public void Dispose()
		{
			_host = null;
			_callingWorkflow = null;
			_datasetGenerator = null;
			_documentationService = null;
			_exception = null;
			_inputContexts.Clear();
			_parameterService = null;
			_parentContexts.Clear();
			_persistence = null;
			_ruleEngine = null;
			_taskResults.Clear();
			_tracingService = null;
			_workflowBlock = null;
		}

		#endregion
	}
}