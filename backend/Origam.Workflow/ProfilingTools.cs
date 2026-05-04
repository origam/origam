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
using System.Collections.Generic;
using System.Diagnostics;
using log4net;
using Origam.Schema;
using Origam.Schema.WorkflowModel;

namespace Origam.Workflow;

public class WorkflowProfiling { }

public static class ProfilingTools
{
    private static readonly ILog workflowProfilingLog = LogManager.GetLogger(
        type: typeof(WorkflowProfiling)
    );

    public static void LogDuration(
        string logEntryType,
        IWorkflowStep task,
        Stopwatch stoppedStopwatch
    )
    {
        if (task == null)
        {
            return;
        }

        (string id, string path) = GetIdAndPath(task: task, logEntryType: logEntryType);
        LogDuration(
            logEntryType: logEntryType,
            path: path,
            id: id,
            stoppedStopwatch: stoppedStopwatch
        );
    }

    public static void LogDuration(
        string logEntryType,
        string path,
        string id,
        Stopwatch stoppedStopwatch
    )
    {
        string typeWithDoubleColon = $"{logEntryType}:";
        workflowProfilingLog.Debug(
            message: string.Format(
                format: "{0,-18}{1,-80} Id: {2}  Duration: {3,7:0.0} ms",
                args: new object[]
                {
                    typeWithDoubleColon,
                    path,
                    id,
                    stoppedStopwatch.Elapsed.TotalMilliseconds,
                }
            )
        );
    }

    public static void LogWorkFlowEnd()
    {
        workflowProfilingLog.Debug(message: " ");
    }

    public static void ExecuteAndLogDuration(
        Action action,
        string logEntryType,
        string path,
        string id,
        Func<bool> logOnlyIf = null
    )
    {
        bool FuncToExecute()
        {
            action();
            return false;
        }
        ExecuteAndLogDuration(
            funcToExecute: FuncToExecute,
            logEntryType: logEntryType,
            path: path,
            id: id,
            logOnlyIf: logOnlyIf
        );
    }

    public static void ExecuteAndLogDuration(
        Action action,
        string logEntryType,
        IWorkflowStep task,
        Func<bool> logOnlyIf = null
    )
    {
        (string id, string path) = GetIdAndPath(task: task, logEntryType: logEntryType);
        ExecuteAndLogDuration(
            action: action,
            logEntryType: logEntryType,
            path: path,
            id: id,
            logOnlyIf: logOnlyIf
        );
    }

    public static bool ExecuteAndLogDuration(
        Func<bool> funcToExecute,
        string logEntryType,
        string path,
        string id,
        Func<bool> logOnlyIf = null
    )
    {
        bool result;
        if (IsDebugEnabled)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            result = funcToExecute();
            stopwatch.Stop();
            if (ShouldLog(logOnlyIf: logOnlyIf))
            {
                LogDuration(
                    logEntryType: logEntryType,
                    path: path,
                    id: id,
                    stoppedStopwatch: stopwatch
                );
            }
        }
        else
        {
            result = funcToExecute();
        }
        return result;
    }

    public static bool IsDebugEnabled => workflowProfilingLog.IsDebugEnabled;

    public static void ClearThreadLoggingContext()
    {
        ThreadContext.Properties[key: "currentTaskPath"] = null;
        ThreadContext.Properties[key: "currentTaskId"] = null;
        ThreadContext.Properties[key: "ServiceMethodName"] = null;
    }

    public static void SetCurrentTaskToThreadLoggingContext(ServiceMethodCallTask task)
    {
        ThreadContext.Properties[key: "currentTaskPath"] = task.Path;
        ThreadContext.Properties[key: "currentTaskId"] = task.NodeId;
        ThreadContext.Properties[key: "ServiceMethodName"] = task.ServiceMethod.Name;
    }

    private static (string id, string path) GetIdAndPath(IWorkflowStep task, string logEntryType)
    {
        string taskPath = task is ISchemaItem schemaItem ? schemaItem.Path : "";
        string id = task == null ? "" : task.NodeId;
        string path = taskPath + "/" + logEntryType;
        return (id, path);
    }

    private static bool ShouldLog(Func<bool> logOnlyIf)
    {
        if (logOnlyIf == null)
        {
            return true;
        }

        return logOnlyIf();
    }
}

public class OperationTimer
{
    private static OperationTimer instance;
    public static OperationTimer Global => instance ?? (instance = new OperationTimer());

    private readonly Dictionary<int, OperationData> runningOperations =
        new Dictionary<int, OperationData>();

    public void Start(int hash)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var operationData = new OperationData(stopwatch: stopwatch);
        runningOperations.Add(key: hash, value: operationData);
    }

    public void Start(
        string logEntryType,
        string path,
        string id,
        int hash,
        Stopwatch stopwatch = null
    )
    {
        if (stopwatch == null)
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }
        var operationData = new OperationData(
            logEntryType: logEntryType,
            path: path,
            id: id,
            stopwatch: stopwatch
        );
        runningOperations.Add(key: hash, value: operationData);
    }

    public Stopwatch Stop(int hash)
    {
        if (!runningOperations.ContainsKey(key: hash))
        {
            return new Stopwatch();
        }

        Stopwatch stopwatch = runningOperations[key: hash].Stopwatch;
        stopwatch.Stop();
        runningOperations.Remove(key: hash);
        return stopwatch;
    }

    public void StopAndLog(int hash)
    {
        if (!runningOperations.ContainsKey(key: hash))
        {
            return;
        }

        ProfilingTools.LogDuration(
            logEntryType: runningOperations[key: hash].LogEntryType,
            path: runningOperations[key: hash].Path,
            id: runningOperations[key: hash].Id,
            stoppedStopwatch: runningOperations[key: hash].Stopwatch
        );
        runningOperations.Remove(key: hash);
    }

    private class OperationData
    {
        public string LogEntryType { get; }
        public string Path { get; }
        public string Id { get; }
        public Stopwatch Stopwatch { get; }

        public OperationData(Stopwatch stopwatch)
        {
            LogEntryType = "";
            Path = "";
            Id = "";
            Stopwatch = stopwatch;
        }

        public OperationData(string logEntryType, string path, string id, Stopwatch stopwatch)
        {
            LogEntryType = logEntryType;
            this.Path = path;
            Id = id;
            Stopwatch = stopwatch;
        }
    }
}
