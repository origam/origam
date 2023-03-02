using System;
using System.Data;

namespace Origam.Workflow.WorkQueue;

public class RetryManager
{
    public void SetEntryRetryData(DataRow queueEntryRow,
        WorkQueueData.WorkQueueRow queue, string message)
    {
        var (retryType, maxRetries, retryIntervalSeconds) = GetRetryData(queue);
                
        var failureTime = DateTime.Now;
        queueEntryRow ["ErrorText"] = failureTime + ": " + message;
        queueEntryRow["LastAttemptTime"] = failureTime;
        int attemptCountAfterFailure = GetAttemptCount(queueEntryRow) + 1;
        queueEntryRow["AttemptCount"] = attemptCountAfterFailure;
            
        if(Equals(retryType, WorkQueueRetryType.NoRetry) ||
           attemptCountAfterFailure >= maxRetries )
        {
            queueEntryRow["NextAttemptTime"] = DateTime.MaxValue;
            return;
        }
            
        if (queue["RetryIntervalSeconds"] == DBNull.Value)
        {
            throw new ArgumentException($"RetryIntervalSeconds in queue {queue["Name"]} is null while the retry type is not NoRetry");
        }
            
        if(Equals(retryType, WorkQueueRetryType.LinearRetry))
        {
            queueEntryRow["NextAttemptTime"] =
                failureTime.AddSeconds(retryIntervalSeconds);
            return;
        }
        if(Equals(retryType, WorkQueueRetryType.ExponentialRetry))
        {
            // queueEntryRow["NextAttemptTime"] =
            //     failureTime.AddSeconds(retryIntervalSeconds);
            return;
        }

        throw new NotImplementedException(
            $"retryType: {retryType} not implemented");
    }
    
    public bool CanRunNow(DataRow queueEntryRow,
        WorkQueueData.WorkQueueRow queue)
    {
        var (retryType, maxRetries, _) = GetRetryData(queue);
        int attemptCount = GetAttemptCount(queueEntryRow);
        if (attemptCount == 0)
        {
            return true;
        }

        if (Equals(retryType, WorkQueueRetryType.NoRetry))
        {
            return false;
        }

        if (attemptCount >= maxRetries)
        {
            return false;
        }

        if (queueEntryRow["NextAttemptTime"] == DBNull.Value)
        {
            throw new Exception($"NextAttemptTime is not set on WorkQueueEntry {queueEntryRow["Id"]} while WorkQueueRetryType is not NoRetry and the AttemptCount is not null or 0");
        }

        return (DateTime)queueEntryRow["NextAttemptTime"] <= DateTime.Now;
    }

    private (Guid, int, int) GetRetryData(WorkQueueData.WorkQueueRow queue)
    {
        if (queue["refWorkQueueRetryTypeId"] == DBNull.Value)
        {
            throw new Exception($"refWorkQueueRetryTypeId not set in queue {queue.Name}");
        }
        Guid retryType = (Guid)queue["refWorkQueueRetryTypeId"];
        int maxRetries = queue["MaxRetries"] == DBNull.Value 
            ? 0 
            : (int)queue["MaxRetries"];
        int retryIntervalSeconds;
        if (queue["RetryIntervalSeconds"] == DBNull.Value ||
            (int)queue["RetryIntervalSeconds"] == 0)
        {
            if (retryType != WorkQueueRetryType.NoRetry)
            {
                throw new ArgumentException($"RetryIntervalSeconds in queue {queue["Name"]} is null while the retry type is not NoRetry");
            }
            retryIntervalSeconds = 0;
        }
        else
        {
            retryIntervalSeconds = (int)queue["RetryIntervalSeconds"];
        }

        return (
            retryType,
            maxRetries,
            retryIntervalSeconds
        );
    }
    private int GetAttemptCount(DataRow queueEntryRow)
    {
        return queueEntryRow["AttemptCount"] == DBNull.Value
            ? 0
            : (int)queueEntryRow["AttemptCount"];
    }
}