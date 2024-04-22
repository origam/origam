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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Origam.Extensions;

namespace Origam.Workflow;

public class TaskRunner
{
    private static readonly log4net.ILog log 
        = log4net.LogManager
            .GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
    private readonly List<Task> tasks = new List<Task>();
    private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
    private readonly Action<CancellationToken> funcToRun;
    private readonly int workerCount;


    public TaskRunner(Action<CancellationToken> funcToRun, int workerCount)
    {
            this.funcToRun = funcToRun;
            this.workerCount = workerCount;
        }

    public void Run()
    {
            var cancToken = tokenSource.Token;

            for (int i = 0; i < workerCount; i++)
            {
                tasks.Add(Task.Factory.StartNew(
                    ()=> funcToRun(cancToken),
                    cancToken)
                );
            }
        }

    public void CleanUp()
    {
            tasks
                .Where(t => t.Exception != null)
                .Select(t => t.Exception)
                .Peek(aggrEx => aggrEx.Flatten())
                .SelectMany(aggrEx => aggrEx.InnerExceptions)
                .ToList()
                .ForEach(LogTaskException);
        }

    public void Cancel()
    {
            tokenSource.Cancel();
        }

    public bool AllTasksFinished()
    {
            return tasks.All(task => task.IsCompleted || task.IsCanceled );
        }

    public void Wait()
    {
            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException aggrEx)
            {
                aggrEx.Flatten()
                    .InnerExceptions
                    .AsEnumerable()
                    .ToList()
                    .ForEach(LogTaskException);
            }
        }

    private void LogTaskException(Exception ex)
    {
            log.Error("THIS UNHANDLED EXCEPTION OCCURED IN A TASK WHEN IT WAS RUNNING:");
            log.Error(ex);
            log.Error(ex.Message);
            log.Error("---------------------------------------");
        }
}