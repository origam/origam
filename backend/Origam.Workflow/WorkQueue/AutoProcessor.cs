using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Origam.DA;
using Origam.Schema.WorkflowModel;
using WorkQueueRow = Origam.Workflow.WorkQueue.WorkQueueData.WorkQueueRow;
namespace Origam.Workflow.WorkQueue;

public class AutoProcessor
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private readonly WorkQueueUtils workQueueUtils;
    private readonly Action<WorkQueueRow, DataRow> itemProcessAction;

    public AutoProcessor(Action<WorkQueueRow, DataRow> itemProcessAction, WorkQueueUtils workQueueUtils)
    {
        this.itemProcessAction = itemProcessAction;
        this.workQueueUtils = workQueueUtils;
    }

    public void Run(IEnumerable<WorkQueueRow> queues, CancellationToken cancellationToken)
    {
        foreach (WorkQueueRow queue in queues)
        {
            ProcessAutoQueueCommands(queue, cancellationToken);
        }
    }
    
    public void ProcessAutoQueueCommands(WorkQueueRow q,
        CancellationToken cancellationToken ,int forceWait_ms=0)
    {
        DataRow queueItemRow;

        var processErrors = IsAnyCmdSetToAutoProcessedWithErrors(q);

        while(true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                log.Info($"Stopping worker on thread " +
                         $"{Thread.CurrentThread.ManagedThreadId}");
                cancellationToken.ThrowIfCancellationRequested();
            }
            queueItemRow = GetNextItem(q.Id, null, processErrors, cancellationToken);
            if (queueItemRow == null)
            {
                return;
            }
            if (log.IsDebugEnabled)
            {
                log.Debug("Checking whether processing failed - IsLocked: "
                          + queueItemRow["IsLocked"]
                          + ", ErrorText: "
                          + (queueItemRow.IsNull("ErrorText") ? "NULL" : queueItemRow["ErrorText"].ToString()));
            }
            // we have to store the item id now because later the queue entry can be removed by HandleRemove() command
            // ProcessQueueItem(q, queueItemRow, ps, wqc);
            itemProcessAction(q, queueItemRow);
                
            if (forceWait_ms != 0)
            {
                log.Info(
                    $"forceWait parameter causes worker on thread {Thread.CurrentThread.ManagedThreadId} to sleep for: {forceWait_ms} ms");
                Thread.Sleep(forceWait_ms);
            }
        }
    }
    
    public DataRow GetNextItem(Guid workQueueId, string transactionId, bool processErrors,
        CancellationToken cancellationToken)
    {
        const int pageSize = 10;
        int pageNumber = 1;
        WorkQueueClass workQueueClass = workQueueUtils.WQClass(workQueueId);
        DataRow result = null;
        do
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            DataSet queueItems = workQueueUtils.LoadWorkQueueData(
                workQueueClass.Name, workQueueId, pageSize, pageNumber,
                transactionId);
            DataTable queueTable = queueItems.Tables[0];
            if (queueTable.Rows.Count > 0)
            {
                foreach (DataRow queueRow in queueTable.Rows)
                {
                    if ((bool)queueRow["IsLocked"] == false
                        && (queueRow.IsNull("ErrorText") || processErrors))
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
    
    private static bool IsAnyCmdSetToAutoProcessedWithErrors(WorkQueueRow q)
    {
        bool processErrors = false;
        foreach (WorkQueueData.WorkQueueCommandRow cmd in
                 q.GetWorkQueueCommandRows())
        {
            if (cmd.IsAutoProcessedWithErrors)
            {
                processErrors = true;
                break;
            }
        }
        return processErrors;
    }
}