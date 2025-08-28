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

using System.Threading;
using Origam.OrigamEngine;
using Origam.Workbench.Services;
using Origam.Workflow;
using Origam.Workflow.WorkQueue;

namespace Origam.Utils;
public class QueueProcessor
{
    private static readonly log4net.ILog log 
        = log4net.LogManager
            .GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
    private readonly TaskRunner taskRunner;
    public QueueProcessor(string queueRefCode, int parallelism, int forceWait_ms)
    {
        OrigamEngine.OrigamEngine.ConnectRuntime();
        var workQueueService = ServiceManager.Services
            .GetService(typeof(WorkQueueService)) as WorkQueueService;
        taskRunner = workQueueService.GetAutoProcessorForQueue(
            queueRefCode,
            parallelism,
            forceWait_ms
        );
    }
    public void Cancel()
    {
        taskRunner.Cancel();
        while (true)
        {
            Thread.Sleep(200);
            if (taskRunner.AllTasksFinished())
            {
                break;
            }
        }
        taskRunner.CleanUp();
    }
    public void Run()
    {
        taskRunner.Run();
        taskRunner.Wait();
        log.Info("DONE");
    }
}
