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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using log4net;
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
using Origam.Workbench.Services;
using StackExchange.Profiling;

namespace Origam.Workflow;

public class WorkflowEngine : IDisposable
{
    private static readonly ILog log = LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType
    );
    private DatasetGenerator datasetGenerator = new(userDefinedParameters: true);
    private ITracingService tracingService = ServiceManager.Services.GetService<ITracingService>();
    private IParameterService parameterService =
        ServiceManager.Services.GetService<IParameterService>();
    private readonly WorkflowStackTrace workflowStackTrace = new();
    public bool Trace { get; set; } = false;
    private readonly OperationTimer localOperationTimer = new();
    private Exception caughtException;
    private List<string> disposeCallStackTraces = new List<string>();
    private object lockObject = new object();

    public string GetDisposeCallStackTraces()
    {
        lock (lockObject)
        {
            return string.Join(separator: "\n", values: disposeCallStackTraces);
        }
    }

    public WorkflowEngine(string transactionId = null)
    {
        WorkflowUniqueId = Guid.NewGuid();
        WorkflowInstanceId = WorkflowUniqueId;
        this.transactionId = transactionId;
    }

    #region Properties
    private readonly Dictionary<Key, WorkflowStepResult> taskResults = new();
    public Dictionary<Key, WorkflowStepResult> TaskResults => taskResults;

    private Exception workflowException;
    public Exception WorkflowException
    {
        get => workflowException;
        set => workflowException = value;
    }

    private Guid workflowInstanceId;
    public Guid WorkflowInstanceId
    {
        get => workflowInstanceId;
        set => workflowInstanceId = value;
    }

    private readonly Guid workflowUniqueId;
    public Guid WorkflowUniqueId
    {
        get => workflowUniqueId;
        private init => workflowUniqueId = value;
    }

    private string name = "";
    public string Name
    {
        get => name;
        set => name = value;
    }

    private string transactionId = null;
    public string TransactionId => transactionId;

    public void SetTransactionId(
        string transactionId,
        WorkflowTransactionBehavior transactionBehavior
    )
    {
        if (transactionBehavior == WorkflowTransactionBehavior.InheritExisting)
        {
            this.transactionId = transactionId;
        }
    }

    private WorkflowTransactionBehavior transactionBehavior =
        WorkflowTransactionBehavior.InheritExisting;
    public WorkflowTransactionBehavior TransactionBehavior
    {
        get => transactionBehavior;
        set => transactionBehavior = value;
    }

    private IWorkflowBlock workflowBlock;
    public IWorkflowBlock WorkflowBlock
    {
        get => workflowBlock;
        set => workflowBlock = value;
    }

    private WorkflowHost host;
    public WorkflowHost Host
    {
        get => host;
        set => host = value;
    }

    private WorkflowEngine callingWorkflow;
    public WorkflowEngine CallingWorkflow
    {
        get => callingWorkflow;
        set
        {
            callingWorkflow = value;
            if (callingWorkflow == null)
            {
                return;
            }
            // Inherit caller's workflow instance description
            if (RuntimeDescription == "")
            {
                RuntimeDescription = callingWorkflow.RuntimeDescription;
            }
            if (Notification == "")
            {
                Notification = callingWorkflow.Notification;
            }

            if (ResultMessage == "")
            {
                ResultMessage = callingWorkflow.ResultMessage;
            }
        }
    }

    private RuleEngine ruleEngine;
    public RuleEngine RuleEngine => ruleEngine;

    private readonly Hashtable inputContexts = new();

    /// <summary>
    /// Input context stores when this block is called as a subworkflow
    /// </summary>
    public Hashtable InputContexts => inputContexts;

    public object ReturnValue
    {
        get
        {
            foreach (
                IContextStore resultContext in WorkflowBlock.ChildItemsByType<ContextStore>(
                    itemType: ContextStore.CategoryConst
                )
            )
            {
                if (resultContext.IsReturnValue)
                {
                    return RuleEngine.GetContext(contextStore: resultContext);
                }
            }
            return null;
        }
    }

    private readonly Hashtable parentContexts = new();

    /// <summary>
    /// Context stores of the parent block that this block will use
    /// </summary>
    public Hashtable ParentContexts => parentContexts;

    private IPersistenceProvider persistenceProvider;
    public IPersistenceProvider PersistenceProvider
    {
        get => persistenceProvider;
        set => persistenceProvider = value;
    }

    private int iterationNumber = 0;
    public int IterationNumber
    {
        get => iterationNumber;
        set => iterationNumber = value;
    }

    private int iterationTotal = 0;
    public int IterationTotal
    {
        get => iterationTotal;
        set => iterationTotal = value;
    }

    private string runtimeDescription = "";
    public string RuntimeDescription
    {
        get => runtimeDescription;
        set => runtimeDescription = value;
    }

    private string notification = "";
    public string Notification
    {
        get => notification;
        set => notification = value;
    }

    private string resultMessage = "";
    public string ResultMessage
    {
        get => resultMessage;
        set => resultMessage = value;
    }

    private bool isRepeatable = false;
    public bool IsRepeatable
    {
        get => isRepeatable;
        set => isRepeatable = value;
    }

    #endregion

    #region Public Static Methods
    public static WorkflowEngine PrepareWorkflow(
        IWorkflow workflow,
        Hashtable parameters,
        bool isRepeatable,
        string titleName
    )
    {
        var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        var workflowEngine = new WorkflowEngine();
        workflowEngine.PersistenceProvider = persistenceService.SchemaProvider;
        workflowEngine.IsRepeatable = isRepeatable;
        foreach (DictionaryEntry entry in parameters)
        {
            string parameterName = (string)entry.Key;
            ISchemaItem context = workflow.GetChildByName(
                name: parameterName,
                itemType: ContextStore.CategoryConst
            );
            if (context == null)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "name",
                    actualValue: parameterName,
                    message: string.Format(
                        format: ResourceUtils.GetString(key: "ErrorWorkflowParameterNotFound"),
                        arg0: ((ISchemaItem)workflow).Path
                    )
                );
            }
            workflowEngine.InputContexts.Add(key: context.PrimaryKey, value: entry.Value);
        }
        workflowEngine.TransactionBehavior = workflow.TransactionBehavior;
        workflowEngine.WorkflowBlock = workflow;
        workflowEngine.Name = titleName;
        return workflowEngine;
    }
    #endregion

    #region Public Methods
    public void RunWorkflowFromHost()
    {
        if (ProfilingTools.IsDebugEnabled)
        {
            localOperationTimer.Start(hash: GetHashCode());
        }
        RunWorkflow();
    }

    private void LogWorkflowEnd(Stopwatch stopwatch)
    {
        ProfilingTools.LogDuration(
            logEntryType: "WF",
            path: Name,
            id: workflowInstanceId.ToString(),
            stoppedStopwatch: stopwatch
        );
        if (CallingWorkflow == null)
        {
            ProfilingTools.LogWorkFlowEnd();
        }
    }

    private void LogBlockIteration(Stopwatch stopwatch)
    {
        ProfilingTools.LogDuration(
            logEntryType: "Iteration",
            path: $"{((ISchemaItem)WorkflowBlock).Path}/{iterationNumber}",
            id: workflowInstanceId.ToString(),
            stoppedStopwatch: stopwatch
        );
    }

    private void RunWorkflow()
    {
        if (WorkflowBlock == null)
        {
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(key: "ErrorNoWorkflow")
            );
        }
        if (Host == null)
        {
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(key: "ErrorNoHost")
            );
        }
        taskResults.Clear();
        try
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(
                    message: "---------------------------------------------------------------------------------------"
                );
                log.Debug(message: "------------------- Starting workflow: " + WorkflowBlock.Name);
                log.Debug(message: "------------------- Transaction ID: " + TransactionId);
                log.Debug(
                    message: "---------------------------------------------------------------------------------------"
                );
            }
            if (CallingWorkflow == null)
            {
                RuntimeDescription = "";
                Notification = "";
                ResultMessage = "";
            }
            if (IsTrace(workflowStep: WorkflowBlock))
            {
                tracingService.TraceWorkflow(
                    workflowInstanceId: WorkflowInstanceId,
                    workflowId: (Guid)WorkflowBlock.PrimaryKey[key: "Id"],
                    workflowName: WorkflowBlock.Name
                );
            }
            // Initialize all context stores (resume paused ones?)
            var contextStores = new Hashtable();
            // Initialize RuleEngine for this session
            Guid tracingWorkflowId = IsTrace(workflowStep: WorkflowBlock)
                ? WorkflowInstanceId
                : Guid.Empty;
            ruleEngine = RuleEngine.Create(
                contextStores: contextStores,
                transactionId: TransactionId,
                tracingWorkflowId: tracingWorkflowId
            );
            foreach (
                IContextStore contextStore in WorkflowBlock.ChildItemsByType<ContextStore>(
                    itemType: ContextStore.CategoryConst
                )
            )
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(message: "Initializing data store: " + contextStore?.Name);
                }
                // Otherwise we generate an empty store
                if (contextStore!.DataType == OrigamDataType.Xml)
                {
                    switch (contextStore.Structure)
                    {
                        case DataStructure dataStructure:
                        {
                            DataSet dataset = datasetGenerator.CreateDataSet(
                                ds: dataStructure,
                                defaultSet: contextStore.DefaultSet
                            );
                            if (contextStore.DisableConstraints)
                            {
                                dataset.EnforceConstraints = false;
                            }
                            contextStores.Add(
                                key: contextStore.PrimaryKey,
                                value: DataDocumentFactory.New(dataSet: dataset)
                            );
                            break;
                        }
                        case XsdDataStructure:
                        {
                            contextStores.Add(
                                key: contextStore.PrimaryKey,
                                value: new XmlContainer()
                            );
                            break;
                        }
                        case null:
                        {
                            throw new NullReferenceException(
                                message: ResourceUtils.GetString(key: "ErrorNoXmlStore")
                            );
                        }
                        default:
                        {
                            throw new ArgumentOutOfRangeException(
                                paramName: "DataType",
                                actualValue: contextStore.DataType,
                                message: ResourceUtils.GetString(key: "ErrorUnsupportedXmlStore")
                            );
                        }
                    }
                }
                else
                {
                    contextStores.Add(key: contextStore.PrimaryKey, value: null);
                }
                if (InputContexts.ContainsKey(key: contextStore.PrimaryKey))
                {
                    // If we have input data, we use them
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(message: "Passing input context");
                    }
                    if (IsTrace(workflowStep: WorkflowBlock))
                    {
                        tracingService.TraceStep(
                            workflowInstanceId: WorkflowInstanceId,
                            stepPath: WorkflowBlock.Path,
                            stepId: Guid.Empty,
                            category: "Input Context",
                            subCategory: contextStore.Name,
                            remark: "",
                            data1: ContextData(
                                context: InputContexts[key: contextStore.PrimaryKey]
                            ),
                            data2: null,
                            message: null
                        );
                    }
                    MergeContext(
                        resultContextKey: contextStore.PrimaryKey,
                        inputContext: InputContexts[key: contextStore.PrimaryKey],
                        step: null,
                        contextName: contextStore.Name,
                        method: ServiceOutputMethod.AppendMergeExisting
                    );
                }
            }
            // Include all contexts from the parent block
            foreach (DictionaryEntry entry in ParentContexts)
            {
                contextStores.Add(key: entry.Key, value: entry.Value);
            }
            List<AbstractWorkflowStep> tasks = WorkflowBlock.ChildItemsByType<AbstractWorkflowStep>(
                itemType: AbstractWorkflowStep.CategoryConst
            );
            // Set states of each task to "not run"
            foreach (AbstractWorkflowStep task in tasks)
            {
                SetStepStatus(step: task, status: WorkflowStepResult.Ready);
            }
            // clear input contexts - they will not be needed anymore
            InputContexts.Clear();
            ResumeWorkflow();
        }
        catch (Exception ex)
        {
            HandleWorkflowException(exception: ex);
        }
    }

    public bool IsTrace(IWorkflowStep workflowStep)
    {
        return workflowStep switch
        {
            // step can be null e.g. when called
            // from workflow screen in Architect
            null => false,
            Schema.WorkflowModel.Workflow
                when workflowStep.TraceLevel == Origam.Trace.InheritFromParent => Trace,
            _ => workflowStep.Trace switch
            {
                // when all workflow has InheritFromParent then gets Trace
                // from Parent Workflow
                Origam.Trace.InheritFromParent => Trace,
                Origam.Trace.Yes => true,
                Origam.Trace.No => false,
                _ => false,
            },
        };
    }

    private void ResumeWorkflow()
    {
        List<AbstractWorkflowStep> tasks = WorkflowBlock.ChildItemsByType<AbstractWorkflowStep>(
            itemType: AbstractWorkflowStep.CategoryConst
        );
        if (tasks.Count == 0)
        {
            FinishWorkflow(exception: null);
            return;
        }
        for (int i = 0; i < tasks.Count; i++)
        {
            if (WorkflowCompleted())
            {
                FinishWorkflow(exception: null);
                break;
            }
            // Check if the task is ready to run by start event
            if (CanStepRun(step: tasks[index: i] as IWorkflowStep))
            {
                // Now check if the task will ever run by startup rule
                if (EvaluateStartRuleTimed(task: tasks[index: i] as IWorkflowStep))
                {
                    var currentModelStep = tasks[index: i] as IWorkflowStep;
                    IWorkflowEngineTask engineTask = WorkflowTaskFactory.GetTask(
                        step: currentModelStep
                    );
                    engineTask.Engine = this;
                    engineTask.Step = currentModelStep;
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                            message: "---------------------------------------------------------------------------------------"
                        );
                        log.Debug(
                            message: "Starting "
                                + engineTask.GetType().Name
                                + ": "
                                + currentModelStep?.Name
                        );
                    }
                    workflowStackTrace.RecordStepStart(
                        workflowName: WorkflowBlock.Name,
                        stepName: currentModelStep?.Name
                    );
                    SetStepStatus(step: currentModelStep, status: WorkflowStepResult.Running);
                    engineTask.Finished += OnEngineTaskFinished;
                    using (
                        MiniProfiler.Current.Step(
                            name: WorkflowBlock.Name + ":" + currentModelStep?.Name
                        )
                    )
                    {
                        engineTask.Execute();
                    }
                    break;
                }
                // Task will never run, startup rule returned false
                SetStepStatus(
                    step: tasks[index: i] as IWorkflowStep,
                    status: WorkflowStepResult.NotRun
                );
            }
            if (i == tasks.Count - 1)
            {
                // let's start over
                i = -1;
            }
        }
    }

    private void HandleStepException(IWorkflowStep step, Exception exception)
    {
        SetStepStatus(step: step, status: WorkflowStepResult.Failure);
        if (exception is RuleException && log.IsDebugEnabled)
        {
            log.Debug(message: $"{step?.GetType().Name} {step?.Path} failed.");
        }
        else if (log.IsErrorEnabled)
        {
            log.Error(message: $"{step?.GetType().Name} {step?.Path} failed.");
        }
        // Trace the error
        if (IsTrace(workflowStep: step))
        {
            tracingService.TraceStep(
                workflowInstanceId: WorkflowInstanceId,
                stepPath: step!.Path,
                stepId: step!.Id,
                category: "Process",
                subCategory: "Error",
                remark: null,
                data1: null,
                data2: null,
                message: exception.Message
            );
        }
        // suppress all tasks that had not run yet and have no dependencies
        List<AbstractWorkflowStep> tasks = WorkflowBlock.ChildItemsByType<AbstractWorkflowStep>(
            itemType: AbstractWorkflowStep.CategoryConst
        );
        for (int i = 0; i < tasks.Count; i++)
        {
            var siblingStep = tasks[index: i] as IWorkflowStep;
            if (
                (siblingStep!.Dependencies.Count == 0)
                && taskResults[key: siblingStep.PrimaryKey] == WorkflowStepResult.Ready
            )
            {
                SetStepStatus(step: siblingStep, status: WorkflowStepResult.NotRun);
            }
        }
        if (IsFailureHandled(failedStep: step))
        {
            caughtException = exception;
            return;
        }
        // cancel the workflow and rethrow the exception up, if root workflow
        HandleWorkflowException(
            exception: GetStepException(exception: exception, stepName: step!.Name)
        );
    }

    /// <summary>
    /// Returns true if there is a task in the workflow that handles failures.
    /// </summary>
    /// <returns></returns>
    private bool IsFailureHandled(IWorkflowStep failedStep)
    {
        List<AbstractWorkflowStep> tasks = WorkflowBlock.ChildItemsByType<AbstractWorkflowStep>(
            itemType: AbstractWorkflowStep.CategoryConst
        );
        foreach (IWorkflowStep step in tasks)
        {
            var dependencyOnFailedStep = step.Dependencies.FirstOrDefault(predicate: dependency =>
                dependency.Task == failedStep
            );
            if (dependencyOnFailedStep is { StartEvent: WorkflowStepStartEvent.Failure })
            {
                return true;
            }
        }
        return false;
    }

    private void HandleWorkflowException(Exception exception)
    {
        WorkflowException = exception;
        var keys = taskResults.Keys.ToList();
        foreach (Key key in keys)
        {
            if (taskResults[key: key] == WorkflowStepResult.Ready)
            {
                taskResults[key: key] = WorkflowStepResult.NotRun;
            }
        }
        if (IsTrace(workflowStep: WorkflowBlock))
        {
            string recursiveExceptionText = exception.Message;
            Exception recursiveException = exception;
            while (recursiveException.InnerException != null)
            {
                recursiveExceptionText +=
                    Environment.NewLine
                    + "-------------------------------- "
                    + Environment.NewLine
                    + recursiveException.InnerException.Message;
                recursiveException = recursiveException.InnerException;
            }
            tracingService.TraceStep(
                workflowInstanceId: WorkflowInstanceId,
                stepPath: WorkflowBlock?.Path,
                stepId: (Guid)WorkflowBlock.PrimaryKey[key: "Id"],
                category: "Process",
                subCategory: "Error",
                remark: null,
                data1: recursiveExceptionText,
                data2: recursiveException.StackTrace,
                message: exception.Message
            );
        }
        if (exception is not WorkflowCancelledByUserException && log.IsErrorEnabled)
        {
            log.LogOrigamError(
                message: $"{exception.Message}\n{workflowStackTrace}",
                ex: exception
            );
        }
        FinishWorkflow(exception: exception);
    }

    private void SetStepStatus(IWorkflowStep step, WorkflowStepResult status)
    {
        taskResults[key: step.PrimaryKey] = status;
    }

    private WorkflowStepResult StepStatus(IWorkflowStep step)
    {
        return taskResults[key: step.PrimaryKey];
    }

    private void EvaluateEndRuleTimed(IEndRule rule, object data, IWorkflowStep step)
    {
        ProfilingTools.ExecuteAndLogDuration(
            action: () => EvaluateEndRule(rule: rule, data: data, step: step),
            logEntryType: "Validation Rule",
            task: step
        );
    }

    public void EvaluateEndRule(IEndRule rule, object data, IWorkflowStep step)
    {
        if (rule == null)
        {
            return;
        }
        RuleExceptionDataCollection result = RuleEngine.EvaluateEndRule(
            rule: rule,
            data: data,
            parentIsTracing: IsTrace(workflowStep: step)
        );
        if (step != null && IsTrace(workflowStep: step))
        {
            tracingService.TraceStep(
                workflowInstanceId: WorkflowInstanceId,
                stepPath: step!.Path,
                stepId: (Guid)step.PrimaryKey[key: "Id"],
                category: "End Rule",
                subCategory: "Input",
                remark: step.ValidationRuleContextStore.Name,
                data1: ContextData(context: data),
                data2: null,
                message: null
            );
        }
        // if there are some exceptions, we actually throw them
        if ((result != null) && (result.Count != 0))
        {
            throw new RuleException(result: result);
        }
    }

    private Exception GetStepException(Exception exception, string stepName)
    {
        bool shouldBeDisplayedToUser =
            exception
                is WorkflowCancelledByUserException
                    or RuleException
                    or OrigamValidationException;
        return shouldBeDisplayedToUser
            ? exception
            : new OrigamException(
                message: exception.Message,
                customStackTrace: stepName,
                innerException: exception
            );
    }
    #endregion

    #region Private Methods
    private bool WorkflowCompleted()
    {
        foreach (var entry in taskResults)
        {
            WorkflowStepResult result = entry.Value;
            if (result is WorkflowStepResult.Ready or WorkflowStepResult.Running)
            {
                return false;
            }
        }
        return true;
    }

    private bool CanStepRun(IWorkflowStep step)
    {
        // Check if this task has been already completed, don't run it again
        if (StepStatus(step: step) != WorkflowStepResult.Ready)
        {
            return false;
        }
        foreach (WorkflowTaskDependency dependency in step.Dependencies)
        {
            try
            {
                if (!taskResults.ContainsKey(key: dependency.Task.PrimaryKey))
                {
                    throw new Exception(
                        message: "Workflow task dependency invalid. Task: " + step.Name
                    );
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    message: "Workflow task dependency invalid. Task: " + step.Name,
                    innerException: ex
                );
            }
            WorkflowStepResult dependencyResult = StepStatus(step: dependency.Task);
            switch (dependencyResult)
            {
                case WorkflowStepResult.Running:
                {
                    return false;
                }
                case WorkflowStepResult.FailureNotRun:
                {
                    SetStepStatus(step: step, status: WorkflowStepResult.FailureNotRun);
                    return false;
                }
                case WorkflowStepResult.NotRun:
                {
                    // If dependent task did not run
                    // and we don't care about result, it's ok
                    if (dependency.StartEvent != WorkflowStepStartEvent.Finish)
                    {
                        // We check if any of tasks we depend on has state NotRun.
                        // In that case current task will not run as well.
                        SetStepStatus(step: step, status: WorkflowStepResult.NotRun);
                        return false;
                    }
                    break;
                }
                default:
                {
                    switch (dependencyResult)
                    {
                        case WorkflowStepResult.Success
                            when dependency.StartEvent == WorkflowStepStartEvent.Failure:
                        {
                            // for failures we only start tasks
                            // marked to start on failure
                            SetStepStatus(step: step, status: WorkflowStepResult.FailureNotRun);
                            return false;
                        }
                        case WorkflowStepResult.Failure
                            when dependency.StartEvent != WorkflowStepStartEvent.Failure:
                        {
                            SetStepStatus(step: step, status: WorkflowStepResult.FailureNotRun);
                            return false;
                        }
                        case WorkflowStepResult.Ready:
                        {
                            // The dependent task did not run, yet.
                            // So we still have to wait a bit.
                            return false;
                        }
                    }
                    break;
                }
            }
        }
        return true;
    }

    private bool EvaluateStartRuleTimed(IWorkflowStep task)
    {
        string path = task is ISchemaItem schemaItem ? schemaItem.Path : "";
        return ProfilingTools.ExecuteAndLogDuration(
            funcToExecute: () => EvaluateStartRule(task: task),
            logEntryType: "Start Rule",
            path: path + "/Start Rule",
            id: task.NodeId,
            logOnlyIf: () => task.StartConditionRule != null
        );
    }

    private bool EvaluateStartRule(IWorkflowStep task)
    {
        // check features
        if (!parameterService.IsFeatureOn(featureCode: task.Features))
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(message: "Step will not execute because of feature being turned off.");
            }
            return false;
        }
        if (
            !SecurityManager
                .GetAuthorizationProvider()
                .Authorize(principal: SecurityManager.CurrentPrincipal, context: task.Roles)
        )
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(
                    message: "Step will not execute because the user has not been authorized."
                );
            }
            return false;
        }
        // If there is no start rule, we always start the task
        if (task.StartConditionRule == null)
        {
            return true;
        }
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "Evaluating startup rule for step " + task.Name);
        }
        var result = (bool)
            RuleEngine.EvaluateRule(
                rule: task.StartConditionRule,
                data: task.StartConditionRuleContextStore,
                contextPosition: null,
                parentIsTracing: IsTrace(workflowStep: task)
            );
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "Rule evaluated and returned " + result);
        }
        return result;
    }

    internal void EvaluateEndRule(IWorkflowStep step)
    {
        // If there is no validation rule, we return
        if (step.ValidationRule == null)
        {
            return;
        }
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "Evaluating validation rule for step " + step.Name);
        }
        RuleExceptionDataCollection result = RuleEngine.EvaluateEndRule(
            rule: step.ValidationRule,
            data: step.ValidationRuleContextStore
        );
        if (result == null)
        {
            throw new OrigamException(
                message: "Programming error: there is not any "
                    + $"RuleExceptionDataCollection in the output of the validation rule `{step.ValidationRule.Name}' ({step.ValidationRule.PrimaryKey}). "
                    + "Please review the rule and add <RuleExceptionDataCollection> tag."
            );
        }
        // if there are some exceptions, we actually throw them
        if (result.Count != 0)
        {
            throw new RuleException(result: result);
        }
    }

    internal WorkflowEngine GetSubEngine(
        IWorkflowBlock block,
        WorkflowTransactionBehavior transactionBehavior
    )
    {
        var subEngine = new WorkflowEngine();
        // Set same properties as we have
        subEngine.PersistenceProvider = PersistenceProvider;
        subEngine.CallingWorkflow = this;
        subEngine.WorkflowBlock = block;
        subEngine.Host = Host;
        subEngine.TransactionBehavior = transactionBehavior;
        subEngine.WorkflowInstanceId = WorkflowInstanceId;
        subEngine.SetTransactionId(
            transactionId: TransactionId,
            transactionBehavior: transactionBehavior
        );
        subEngine.IterationTotal = IterationTotal;
        subEngine.IterationNumber = IterationNumber;
        subEngine.Trace = Trace;
        return subEngine;
    }

    internal void ExecuteSubEngineWorkflow(WorkflowEngine subEngine)
    {
        Host.ExecuteWorkflow(engine: subEngine);
    }

    internal object CloneContext(object context, bool returnDataSet)
    {
        return context switch
        {
            IDataDocument document when returnDataSet => document.DataSet.Copy(),
            IDataDocument document => DataDocumentFactory.New(dataSet: document.DataSet.Copy()),
            IXmlContainer container => container.Clone(),
            // value types - we return the value itself, don't need a clone
            _ => context,
        };
    }

    public IWorkflowStep Step(Key key)
    {
        return PersistenceProvider.RetrieveInstance<IWorkflowStep>(
            instanceId: (Guid)key[key: "Id"]
        );
    }

    private ContextStore GetContextStore(Key key)
    {
        return PersistenceProvider.RetrieveInstance<ContextStore>(instanceId: (Guid)key[key: "Id"]);
    }

    internal string ContextStoreName(Key key)
    {
        return GetContextStore(key: key).Name;
    }

    internal DataStructureRuleSet ContextStoreRuleSet(Key key)
    {
        return GetContextStore(key: key).RuleSet;
    }

    internal OrigamDataType ContextStoreType(Key key)
    {
        return GetContextStore(key: key).DataType;
    }

    internal bool MergeContext(
        Key resultContextKey,
        object inputContext,
        IWorkflowStep step,
        string contextName,
        ServiceOutputMethod method
    )
    {
        if (method == ServiceOutputMethod.Ignore)
        {
            return false;
        }
        object resultContext = RuleEngine.GetContext(key: resultContextKey);
        bool changed = false;
        if (log.IsInfoEnabled)
        {
            string stepNameLog = "";
            if (step != null)
            {
                stepNameLog = ", Step '" + step!.Path + "'";
            }
            log.Info(message: "Merging context '" + contextName + "'" + stepNameLog);
        }
        try
        {
            if ((step != null) && IsTrace(workflowStep: step))
            {
                tracingService.TraceStep(
                    workflowInstanceId: WorkflowInstanceId,
                    stepPath: step.Path,
                    stepId: (Guid)step.PrimaryKey[key: "Id"],
                    category: "Merge Context",
                    subCategory: "Input",
                    remark: contextName,
                    data1: ContextData(context: inputContext),
                    data2: null,
                    message: null
                );
            }
            var inputDataDoc = inputContext as IDataDocument;
            var inputXmlDoc = inputContext as IXmlContainer;
            var resultDataDoc = resultContext as IDataDocument;
            var resultXmlDoc = resultContext as IXmlContainer;
            if (inputContext == null || inputContext == DBNull.Value)
            {
                return false;
            }
            if (inputDataDoc != null && resultDataDoc != null)
            {
                DataSet input = inputDataDoc.DataSet;
                DataSet output = resultDataDoc.DataSet;
                switch (method)
                {
                    case ServiceOutputMethod.AppendMergeExisting or ServiceOutputMethod.FullMerge:
                    {
                        changed = RuleEngine.Merge(
                            inout_dsTarget: output,
                            in_dsSource: input,
                            in_bTrueDelete: method == ServiceOutputMethod.FullMerge,
                            in_bPreserveChanges: false,
                            in_bSourceIsFragment: false,
                            preserveNewRowState: true
                        );
                        break;
                    }
                    case ServiceOutputMethod.DeleteMatches:
                    {
                        foreach (DataTable inputTable in input.Tables)
                        {
                            if (!output.Tables.Contains(name: inputTable.TableName))
                            {
                                continue;
                            }
                            DataTable outputTable = output.Tables[name: inputTable.TableName];
                            foreach (DataRow inputRow in inputTable.Rows)
                            {
                                object[] inputRowPrimaryKey = DatasetTools.PrimaryKey(
                                    row: inputRow
                                );
                                DataRow rowToDelete = outputTable.Rows.Find(
                                    keys: inputRowPrimaryKey
                                );
                                if (rowToDelete != null)
                                {
                                    rowToDelete.Delete();
                                    changed = true;
                                }
                            }
                        }
                        break;
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "method",
                            actualValue: method,
                            message: "Unsupported merge method."
                        );
                    }
                }
            }
            else if (inputXmlDoc != null)
            {
                ContextStore contextStore = GetContextStore(key: resultContextKey);
                if (contextStore.DataType == OrigamDataType.String)
                {
                    RuleEngine.SetContext(key: resultContextKey, value: inputXmlDoc.Xml.InnerText);
                }
                else
                {
                    if (resultXmlDoc == null)
                    {
                        throw new Exception(
                            message: "Cannot merge data into a context, which is not XML type. Context: "
                                + contextName
                                + ", type: "
                                + (
                                    resultContext == null
                                        ? "NULL"
                                        : resultContext.GetType().ToString()
                                )
                        );
                    }
                    changed = true;
                    if (inputXmlDoc.Xml.DocumentElement != null)
                    {
                        // copy document element, if it does not exist already
                        if (resultXmlDoc.Xml.DocumentElement == null)
                        {
                            if (resultDataDoc != null)
                            {
                                bool previousEnforceConstraints = resultDataDoc
                                    .DataSet
                                    .EnforceConstraints;
                                resultDataDoc.DataSet.EnforceConstraints = false;
                                resultDataDoc.AppendChild(
                                    documentElement: inputXmlDoc.Xml.DocumentElement,
                                    deep: true
                                );
                                try
                                {
                                    resultDataDoc.DataSet.EnforceConstraints =
                                        previousEnforceConstraints;
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(
                                        message: DebugClass.ListRowErrors(
                                            dataSet: resultDataDoc.DataSet
                                        ),
                                        innerException: ex
                                    );
                                }
                            }
                            else
                            {
                                XmlNode newDoc = resultXmlDoc.Xml.ImportNode(
                                    node: inputXmlDoc.Xml.DocumentElement,
                                    deep: true
                                );
                                resultXmlDoc.Xml.AppendChild(newChild: newDoc);
                            }
                        }
                        else
                        {
                            // otherwise copy each sub node
                            foreach (XmlNode node in inputXmlDoc.Xml.DocumentElement.ChildNodes)
                            {
                                if (node is not XmlDeclaration)
                                {
                                    resultXmlDoc.DocumentElementAppendChild(node: node);
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
                var resultXml = RuleEngine.GetContext(key: resultContextKey) as IXmlContainer;
                var xmlDataDoc = resultXml as IDataDocument;
                bool previousEnforceConstraints = false;
                if (xmlDataDoc != null)
                {
                    previousEnforceConstraints = xmlDataDoc.DataSet.EnforceConstraints;
                    xmlDataDoc.DataSet.EnforceConstraints = false;
                }
                var inputString = inputContext as string;
                if ((resultXml != null) && (inputString != null))
                {
                    resultXml.LoadXml(xmlString: inputString);
                    if (xmlDataDoc != null)
                    {
                        // set default values (loading xml will not set them automatically)
                        SetDataSetDefaultValues(dataSet: xmlDataDoc.DataSet);
                        try
                        {
                            xmlDataDoc.DataSet.EnforceConstraints = previousEnforceConstraints;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(
                                message: DebugClass.ListRowErrors(dataSet: xmlDataDoc.DataSet),
                                innerException: ex
                            );
                        }
                        object profileId = SecurityManager.CurrentUserProfile().Id;
                        foreach (DataTable table in xmlDataDoc.DataSet.Tables)
                        {
                            foreach (DataRow row in table.Rows)
                            {
                                DatasetTools.UpdateOrigamSystemColumns(
                                    row: row,
                                    isNew: true,
                                    profileId: profileId
                                );
                            }
                        }
                    }
                }
                // everything else (simple data types) - we just copy the value
                else
                {
                    OrigamDataType contextType = ContextStoreType(key: resultContextKey);
                    RuleEngine.ConvertStringValueToContextValue(
                        origamDataType: contextType,
                        inputString: inputString,
                        contextValue: ref inputContext
                    );
                    RuleEngine.SetContext(key: resultContextKey, value: inputContext);
                }
            }
            if ((step != null) && IsTrace(workflowStep: step))
            {
                tracingService.TraceStep(
                    workflowInstanceId: WorkflowInstanceId,
                    stepPath: step.Path,
                    stepId: (Guid)step.PrimaryKey[key: "Id"],
                    category: "Merge Context",
                    subCategory: "Result",
                    remark: contextName,
                    data1: changed
                        ? ContextData(context: RuleEngine.GetContext(key: resultContextKey))
                        : "-- no change --",
                    data2: null,
                    message: null
                );
            }
            DataStructureRuleSet ruleSet = ContextStoreRuleSet(key: resultContextKey);
            if (
                changed
                && (ruleSet != null)
                && step is (null or IWorkflowTask or CheckRuleStep) and not UIFormTask
            )
            {
                ProcessRulesTimed(resultContextKey: resultContextKey, ruleSet: ruleSet, step: step);
                if ((step != null) && IsTrace(workflowStep: step))
                {
                    tracingService.TraceStep(
                        workflowInstanceId: WorkflowInstanceId,
                        stepPath: step.Path,
                        stepId: (Guid)step.PrimaryKey[key: "Id"],
                        category: "Rule Processing",
                        subCategory: "Result",
                        remark: contextName,
                        data1: ContextData(context: RuleEngine.GetContext(key: resultContextKey)),
                        data2: null,
                        message: null
                    );
                }
                if ((step == null) && IsTrace(workflowStep: WorkflowBlock))
                {
                    tracingService.TraceStep(
                        workflowInstanceId: WorkflowInstanceId,
                        stepPath: WorkflowBlock.Path,
                        stepId: (Guid)WorkflowBlock.PrimaryKey[key: "Id"],
                        category: "Rule Processing",
                        subCategory: "Result",
                        remark: contextName,
                        data1: ContextData(context: RuleEngine.GetContext(key: resultContextKey)),
                        data2: null,
                        message: null
                    );
                }
            }
        }
        catch (Exception ex)
        {
            string stepNameLog = "";
            if (step != null)
            {
                stepNameLog = ", Step '" + step.Path + "'";
            }
            string inputString = inputContext as string ?? "";
            throw new Exception(
                message: "Merge context '"
                    + contextName
                    + "'"
                    + stepNameLog
                    + " failed. InputContextValue: "
                    + inputString
                    + ". Original exception message: "
                    + ex.Message,
                innerException: ex
            );
        }
        if (log.IsInfoEnabled)
        {
            string stepNameLog = "";
            if (step != null)
            {
                stepNameLog = ", Step '" + step.Path + "'";
            }
            log.Info(message: "Finished merging context '" + contextName + "'" + stepNameLog);
        }
        return changed;
    }

    private void SetDataSetDefaultValues(DataSet dataSet)
    {
        foreach (DataTable table in dataSet.Tables)
        {
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn column in table.Columns)
                {
                    if (
                        !column.AllowDBNull
                        && (column.DefaultValue != null)
                        && (row[column: column] == DBNull.Value)
                    )
                    {
                        row[column: column] = column.DefaultValue;
                    }
                }
            }
        }
    }

    private void ProcessRulesTimed(
        Key resultContextKey,
        DataStructureRuleSet ruleSet,
        IWorkflowStep step
    )
    {
        ProfilingTools.ExecuteAndLogDuration(
            action: () =>
            {
                ruleEngine.ProcessRules(
                    data: RuleEngine.GetContext(key: resultContextKey) as IDataDocument,
                    ruleSet: ruleSet,
                    contextRow: null
                );
            },
            logEntryType: "Context RuleSet",
            task: step
        );
    }

    public static string ContextData(object context)
    {
        switch (context)
        {
            case IXmlContainer xmlContainer:
            {
                var stringBuilder = new StringBuilder();
                var stringWriter = new StringWriter(sb: stringBuilder);
                var xmlTextWriter = new XmlTextWriter(w: stringWriter);
                xmlTextWriter.Formatting = Formatting.Indented;
                xmlContainer.Xml.WriteTo(w: xmlTextWriter);
                xmlTextWriter.Close();
                stringWriter.Close();
                return stringBuilder.ToString();
            }
            case DataSet dataSet:
            {
                return dataSet.GetXml();
            }
            case null:
            {
                return "";
            }
            default:
            {
                return context.ToString();
            }
        }
    }

    internal string GetTaskDescription(IWorkflowStep task)
    {
        string append = RuntimeDescription;
        if (append != "")
        {
            append = " " + append;
        }
        var documentationService = ServiceManager.Services.GetService<IDocumentationService>();
        if (documentationService == null)
        {
            return task.Name + append;
        }
        string documentation = documentationService.GetDocumentation(
            schemaItemId: (Guid)task.PrimaryKey[key: "Id"],
            docType: DocumentationType.USER_WFSTEP_DESCRIPTION
        );
        if (string.IsNullOrEmpty(value: documentation))
        {
            documentation = task.Name;
        }
        documentation += append;
        if (IterationTotal > 0)
        {
            documentation += $" ({IterationNumber}/{IterationTotal})";
        }
        return documentation;
    }

    #endregion

    private void OnEngineTaskFinished(object sender, WorkflowEngineTaskEventArgs e)
    {
        var engineTask = sender as IWorkflowEngineTask;
        IWorkflowStep currentModelStep = engineTask?.Step;
        try
        {
            if (e.Exception == null)
            {
                IWorkflowTask task = currentModelStep as IWorkflowTask;
                // Check if results are ok with the EndRule
                if (currentModelStep?.ValidationRule != null)
                {
                    if (currentModelStep.ValidationRuleContextStore == null)
                    {
                        throw new NullReferenceException(
                            message: $"End Rule Context Store is not set. Task '{task?.Name}'"
                        );
                    }
                    object validationRuleStore = RuleEngine.GetContext(
                        contextStore: currentModelStep.ValidationRuleContextStore
                    );
                    if (
                        (task != null)
                        && task.OutputContextStore.PrimaryKey.Equals(
                            obj: task.ValidationRuleContextStore.PrimaryKey
                        )
                    )
                    {
                        validationRuleStore = engineTask.Result;
                    }
                    try
                    {
                        EvaluateEndRuleTimed(
                            rule: currentModelStep.ValidationRule,
                            data: validationRuleStore,
                            step: currentModelStep
                        );
                    }
                    catch (RuleException ruleException)
                    {
                        if (ruleException.IsSeverityHigh)
                        {
                            throw;
                        }
                    }
                }
                if (task != null)
                {
                    if (
                        (task.OutputContextStore == null)
                        && (task.OutputMethod != ServiceOutputMethod.Ignore)
                    )
                    {
                        throw new NullReferenceException(
                            message: ResourceUtils.GetString(
                                key: "ErrorNoOutputContext",
                                args: task.Name
                            )
                        );
                    }
                    // nothing has happened after evaluating end rule, we process the results
                    bool doMerge = true;
#if ORIGAM_SERVER
                    if (
                        (task is UIFormTask) && (task.OutputMethod == ServiceOutputMethod.FullMerge)
                    )
                    {
                        doMerge = false;
                    }
#endif
                    if (doMerge)
                    {
                        ProfilingTools.ExecuteAndLogDuration(
                            action: () =>
                            {
                                MergeContext(
                                    resultContextKey: task.OutputContextStore.PrimaryKey,
                                    inputContext: engineTask.Result,
                                    step: task,
                                    contextName: task.OutputContextStore.Name,
                                    method: task.OutputMethod
                                );
                            },
                            logEntryType: "Merge",
                            task: task
                        );
                    }
                }
                SetStepStatus(step: currentModelStep, status: WorkflowStepResult.Success);
                if (log.IsDebugEnabled)
                {
                    log.Debug(
                        message: $"{engineTask?.GetType().Name} {currentModelStep?.Name} finished successfully."
                    );
                }
                if (Host.SupportsUI)
                {
                    ProfilingTools.ExecuteAndLogDuration(
                        action: () =>
                        {
                            Host.OnWorkflowUserMessage(
                                engine: this,
                                message: GetTaskDescription(task: currentModelStep),
                                exception: null,
                                popup: false
                            );
                        },
                        logEntryType: "Documentation",
                        task: task
                    );
                }
            }
            else
            {
                HandleStepException(step: currentModelStep, exception: e.Exception);
            }
        }
        catch (Exception ex)
        {
            HandleStepException(step: currentModelStep, exception: ex);
        }
        finally
        {
            if (engineTask != null)
            {
                engineTask.Engine = null;
                engineTask.Step = null;
            }
        }
        ResumeWorkflow();
    }

    private void FinishWorkflow(Exception exception)
    {
        if (Host == null)
        {
            return; // already disposed from the host from a preceding task
        }
        if (log.IsDebugEnabled)
        {
            log.Debug(message: $"Block '{WorkflowBlock?.Name}' completed");
            // Show finish screen if this is the root workflow
            if (CallingWorkflow == null)
            {
                log.Debug(message: "Workflow completed");
            }
        }
        if (ProfilingTools.IsDebugEnabled)
        {
            Stopwatch stopwatch = localOperationTimer.Stop(hash: GetHashCode());
            if (string.IsNullOrEmpty(value: Name))
            {
                LogBlockIteration(stopwatch: stopwatch);
            }
            else
            {
                LogWorkflowEnd(stopwatch: stopwatch);
            }
            OperationTimer.Global.StopAndLog(hash: GetHashCode());
        }
        if ((exception == null) && (caughtException != null))
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(message: $"Throwing caught exception {caughtException}");
            }
            WorkflowException = caughtException;
            exception = caughtException;
        }
        Host.OnWorkflowFinished(engine: this, exception: exception);
    }

    #region IDisposable Members

    public void Dispose()
    {
        lock (lockObject)
        {
            disposeCallStackTraces.Add(item: Environment.StackTrace);
            host = null;
            callingWorkflow = null;
            datasetGenerator = null;
            workflowException = null;
            inputContexts.Clear();
            parameterService = null;
            parentContexts.Clear();
            persistenceProvider = null;
            ruleEngine = null;
            taskResults.Clear();
            tracingService = null;
            workflowBlock = null;
        }
    }
    #endregion
}
