#region license
/*
Copyright 2005 - 2023 Advantage Solutions, s. r. o.

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
using System.Data;
using System.Threading;
using Origam.DA;
using Origam.Schema.WorkflowModel;
using WorkQueueRow = Origam.Workflow.WorkQueue.WorkQueueData.WorkQueueRow;

namespace Origam.Workflow.WorkQueue;

public class LinearProcessor : IWorkQueueProcessor
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    private readonly WorkQueueUtils workQueueUtils;
    private readonly RetryManager retryManager;
    private readonly WorkQueueThrottle workQueueThrottle;
    private readonly Action<WorkQueueRow, DataRow> itemProcessAction;

    public LinearProcessor(
        Action<WorkQueueRow, DataRow> itemProcessAction,
        WorkQueueUtils workQueueUtils,
        RetryManager retryManager,
        WorkQueueThrottle workQueueThrottle
    )
    {
        this.itemProcessAction = itemProcessAction;
        this.workQueueUtils = workQueueUtils;
        this.retryManager = retryManager;
        this.workQueueThrottle = workQueueThrottle;
    }

    public virtual void Run(IEnumerable<WorkQueueRow> queues, CancellationToken cancellationToken)
    {
        foreach (WorkQueueRow queue in queues)
        {
            ProcessAutoQueueCommands(
                queue: queue,
                cancellationToken: cancellationToken,
                maxItemsToProcess: null
            );
        }
    }

    public int ProcessAutoQueueCommands(
        WorkQueueRow queue,
        CancellationToken cancellationToken,
        int? maxItemsToProcess = null,
        int forceWaitMillis = 0
    )
    {
        var processErrors = IsAnyCommandSetToAutoProcessedWithErrors(queue);
        int itemsProcessed = 0;
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                log.Info($"Stopping worker on thread " + $"{Thread.CurrentThread.ManagedThreadId}");
                cancellationToken.ThrowIfCancellationRequested();
            }
            var queueItemRow = GetNextItem(queue, null, processErrors, cancellationToken);
            if (queueItemRow == null)
            {
                return itemsProcessed;
            }
            if (log.IsDebugEnabled)
            {
                string errorText = queueItemRow.IsNull("ErrorText")
                    ? "NULL"
                    : queueItemRow["ErrorText"].ToString();
                log.Debug(
                    $"Checking whether processing failed - IsLocked: {queueItemRow["IsLocked"]}, ErrorText: {errorText}"
                );
            }
            // we have to store the item id now because later the queue entry can be removed by HandleRemove() command
            itemProcessAction(queue, queueItemRow);
            itemsProcessed++;
            if (maxItemsToProcess.HasValue && itemsProcessed >= maxItemsToProcess.Value)
            {
                return itemsProcessed;
            }

            if (forceWaitMillis != 0)
            {
                if (log.IsInfoEnabled)
                {
                    log.Info(
                        $"forceWait parameter causes worker on thread "
                            + $"{Thread.CurrentThread.ManagedThreadId} to sleep for: "
                            + $"{forceWaitMillis} ms"
                    );
                }
                Thread.Sleep(forceWaitMillis);
            }
        }
    }

    private bool IsAnyCommandSetToAutoProcessedWithErrors(WorkQueueRow queue)
    {
        bool processErrors = false;
        foreach (WorkQueueData.WorkQueueCommandRow cmd in queue.GetWorkQueueCommandRows())
        {
            if (cmd.IsAutoProcessedWithErrors)
            {
                processErrors = true;
                break;
            }
        }
        return processErrors;
    }

    public DataRow GetNextItem(
        WorkQueueRow queue,
        string transactionId,
        bool processErrors,
        CancellationToken cancellationToken
    )
    {
        const int pageSize = 10;
        int pageNumber = 1;
        WorkQueueClass workQueueClass = workQueueUtils.WorkQueueClass(queue.Id);
        DataRow result = null;
        do
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            DataSet queueItems = workQueueUtils.LoadWorkQueueData(
                workQueueClass.Name,
                queue.Id,
                pageSize,
                pageNumber,
                transactionId
            );
            DataTable queueTable = queueItems.Tables[0];
            if (queueTable.Rows.Count > 0)
            {
                foreach (DataRow queueRow in queueTable.Rows)
                {
                    if (
                        (bool)queueRow["IsLocked"] == false
                        && retryManager.CanRunNow(queueRow, queue, processErrors)
                        && workQueueThrottle.CanRunNow(queue)
                    )
                    {
                        result = DatasetTools.CloneRow(queueRow);
                        if (workQueueUtils.LockQueueItems(workQueueClass, result.Table))
                        {
                            // item successfully locked, we finish
                            break;
                        }
                        else
                        {
                            // could not be locked, we start over
                            pageNumber = 0;
                            result = null;
                            break;
                        }
                    }
                }
                pageNumber++;
            }
            else
            {
                // no more queue items
                break;
            }
        } while (result == null);
        return result;
    }
}
