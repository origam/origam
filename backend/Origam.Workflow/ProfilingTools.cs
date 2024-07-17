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
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using log4net;
using Origam.DA.Service;

namespace Origam.Workflow;
public static class ProfilingTools
{
    private static readonly ILog workflowProfilingLog =
        LogManager.GetLogger(typeof(WorkflowProfiling));
    public static void LogDuration(string logEntryType, IWorkflowStep task,
        Stopwatch stoppedStopwatch)
    {
        if (task == null) return;
        (string id, string path) = GetIdAndPath(task, logEntryType);
        LogDuration(logEntryType, path, id, stoppedStopwatch);
    }
    public static void LogDuration(string logEntryType, string path,
        string id, Stopwatch stoppedStopwatch)
    {
        string typeWithDoubleColon = $"{logEntryType}:";
        workflowProfilingLog.Debug(string.Format(
            "{0,-18}{1,-80} Id: {2}  Duration: {3,7:0.0} ms",
            typeWithDoubleColon,
            path,
            id,
            stoppedStopwatch.Elapsed.TotalMilliseconds));
    }
    public static void LogWorkFlowEnd()
    {
        workflowProfilingLog.Debug(" ");
    }
    public static void ExecuteAndLogDuration(Action action,
        string logEntryType, string path,
        string id, Func<bool> logOnlyIf = null)
    {
        bool FuncToExecute()
        {
            action();
            return false;
        }
        ExecuteAndLogDuration(FuncToExecute, logEntryType, path, id,
            logOnlyIf);
    }
    public static void ExecuteAndLogDuration(Action action,
        string logEntryType, IWorkflowStep task,
        Func<bool> logOnlyIf = null)
    {
        (string id, string path) = GetIdAndPath(task, logEntryType);
        ExecuteAndLogDuration(action, logEntryType, path, id, logOnlyIf);
    }
    public static bool ExecuteAndLogDuration(Func<bool> funcToExecute,
        string logEntryType, string path,
        string id, Func<bool> logOnlyIf = null)
    {
        bool result;
        if (IsDebugEnabled)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            result = funcToExecute();
            stopwatch.Stop();
            if (ShouldLog(logOnlyIf))
            {
                LogDuration(
                    logEntryType: logEntryType,
                    path: path,
                    id: id,
                    stoppedStopwatch: stopwatch);
            }
        } else
        {
            result = funcToExecute();
        }
        return result;
    }
    public static bool IsDebugEnabled =>
        workflowProfilingLog.IsDebugEnabled;
    public static void ClearThreadLoggingContext()
    {
        ThreadContext.Properties["currentTaskPath"] = null;
        ThreadContext.Properties["currentTaskId"] = null;
        ThreadContext.Properties["ServiceMethodName"] = null;
    }
    public static void SetCurrentTaskToThreadLoggingContext(
        ServiceMethodCallTask task)
    {
        ThreadContext.Properties["currentTaskPath"] = task.Path;
        ThreadContext.Properties["currentTaskId"] = task.NodeId;
        ThreadContext.Properties["ServiceMethodName"] =
            task.ServiceMethod.Name;
    }
    private static (string id, string path) GetIdAndPath(IWorkflowStep task,
        string logEntryType)
    {
        string taskPath = task is ISchemaItem schemaItem
            ? schemaItem.Path
            : "";
        string id = task == null ? "" : task.NodeId;
        string path = taskPath + "/" + logEntryType;
        return ( id,  path);
    }
    private static bool ShouldLog(Func<bool> logOnlyIf)
    {
        if (logOnlyIf == null) return true;
        return logOnlyIf();
    }
}
public class OperationTimer
{
    private static OperationTimer instance;
    public static OperationTimer Global =>
         instance ?? (instance = new OperationTimer());
    
    private readonly Dictionary<int,OperationData> runningOperations =
        new Dictionary<int, OperationData>();
    public void Start(int hash)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var operationData = new OperationData(stopwatch);
        runningOperations.Add(hash,operationData);
    }
    
    public void Start(string logEntryType, string path, string id, int hash,
        Stopwatch stopwatch = null)
    {
        if (stopwatch == null)
        {
             stopwatch  = new Stopwatch();
             stopwatch.Start();
        }
        var operationData = new OperationData(logEntryType, path,id, stopwatch);
        runningOperations.Add(hash,operationData);
    }
    public Stopwatch Stop(int hash)
    {
        if (!runningOperations.ContainsKey(hash)) return new Stopwatch();
        Stopwatch stopwatch = runningOperations[hash].Stopwatch;
        stopwatch.Stop();
        runningOperations.Remove(hash);
        return stopwatch;
    }
    public void StopAndLog(int hash)
    {
        if (!runningOperations.ContainsKey(hash)) return;
        
        ProfilingTools.LogDuration(
            logEntryType: runningOperations[hash].LogEntryType,
            path: runningOperations[hash].Path,
            id: runningOperations[hash].Id,
            stoppedStopwatch: runningOperations[hash].Stopwatch);
        runningOperations.Remove(hash);
    }
    private class OperationData
    {
        public string LogEntryType { get; }
        public string Path { get;  }
        public string Id { get;  }
        public Stopwatch Stopwatch { get; }

        public OperationData(Stopwatch stopwatch)
        {
            LogEntryType = "";
            Path = "";
            Id = "";
            Stopwatch = stopwatch;
        }
        
        public OperationData(string logEntryType, string path, string id, 
            Stopwatch stopwatch)
        {
            LogEntryType = logEntryType;
            this.Path = path;
            Id = id;
            Stopwatch = stopwatch;
        }
    }
}
