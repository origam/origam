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
using System.Data;

namespace Origam.Workflow.WorkQueue;

public class RetryManager
{
    private static readonly Random RandomGenerator = new();

    public void SetEntryRetryData(DataRow queueEntryRow,
        WorkQueueData.WorkQueueRow queue, string message)
    {
        Guid retryType = (Guid)queue["refWorkQueueRetryTypeId"];
        int maxRetries = (int)queue["MaxRetries"];
        int retryIntervalSeconds = (int)queue["RetryIntervalSeconds"];
        
        var failureTime = DateTime.Now;
        queueEntryRow["ErrorText"] = failureTime + ": " + message;
        queueEntryRow["LastAttemptTime"] = failureTime;
        int attemptCount = GetAttemptCount(queueEntryRow);
        int attemptCountAfterFailure = attemptCount + 1;
        queueEntryRow["AttemptCount"] = attemptCountAfterFailure;

        if (Equals(retryType, WorkQueueRetryType.NoRetry) ||
            attemptCountAfterFailure >= maxRetries)
        {
            queueEntryRow["NextAttemptTime"] = DateTime.MaxValue;
            return;
        }
        if (Equals(retryType, WorkQueueRetryType.LinearRetry))
        {
            queueEntryRow["NextAttemptTime"] =
                failureTime.AddSeconds(retryIntervalSeconds);
            return;
        }
        if (Equals(retryType, WorkQueueRetryType.ExponentialRetry))
        {
            int minInterval = 0;
            if (attemptCount != 0)
            {
                if (queue["MinRetryIntervalSeconds"] == DBNull.Value)
                {
                    minInterval = (int)Math.Pow(2, attemptCount - 1);
                }
                else
                {
                    minInterval = (int)Math.Max(
                        Math.Pow(2, attemptCount - 1),
                        (int)queue["MinRetryIntervalSeconds"]
                    );
                }

            }
            int maxInterval = queue["MaxRetryIntervalSeconds"] == DBNull.Value
                ? (int)Math.Pow(2, attemptCount)
                : (int)Math.Min(Math.Pow(2, attemptCount), (int)queue["MaxRetryIntervalSeconds"]);
            int waitTimeSeconds = RandomGenerator.Next(minInterval, maxInterval);
            queueEntryRow["NextAttemptTime"] =
                failureTime.AddSeconds(waitTimeSeconds);
            return;
        }

        throw new NotImplementedException(
            $"retryType: {retryType} not implemented");
    }

    public bool CanRunNow(DataRow queueEntryRow,
        WorkQueueData.WorkQueueRow queue)
    {
        Guid retryType = (Guid)queue["refWorkQueueRetryTypeId"];
        int maxRetries = (int)queue["MaxRetries"];
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
            throw new Exception(
                $"NextAttemptTime is not set on WorkQueueEntry {queueEntryRow["Id"]} while WorkQueueRetryType is not NoRetry and the AttemptCount is not null or 0");
        }

        return (DateTime)queueEntryRow["NextAttemptTime"] <= DateTime.Now;
    }
    
    private int GetAttemptCount(DataRow queueEntryRow)
    {
        return queueEntryRow["AttemptCount"] == DBNull.Value
            ? 0
            : (int)queueEntryRow["AttemptCount"];
    }
}