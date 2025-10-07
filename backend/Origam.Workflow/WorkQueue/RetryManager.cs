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
    private readonly Func<DateTime> getTimeNow;
    private static readonly Random RandomGenerator = new();

    public RetryManager(Func<DateTime> getTimeNow = null)
    {
        DateTime GetTimeNowDefault()
        {
            return DateTime.Now;
        }

        this.getTimeNow = getTimeNow ?? GetTimeNowDefault;
    }

    public void SetEntryRetryData(
        DataRow queueEntryRow,
        WorkQueueData.WorkQueueRow queue,
        string errorMessage
    )
    {
        Guid retryType = queue.refWorkQueueRetryTypeId;
        int maxRetries = queue.MaxRetries;
        int retryIntervalSeconds = queue.RetryIntervalSeconds;
        double exponentialRetryBase = decimal.ToDouble(queue.ExponentialRetryBase);

        var failureTime = getTimeNow();
        queueEntryRow["ErrorText"] = failureTime + ": " + errorMessage;
        queueEntryRow["LastAttemptTime"] = failureTime;
        int attemptCount = GetAttemptCount(queueEntryRow);
        int newAttemptCount = attemptCount + 1; // = 1 after the first failure
        int retryNumber = newAttemptCount;
        queueEntryRow["AttemptCount"] = newAttemptCount;

        if (Equals(retryType, WorkQueueRetryType.NoRetry) || retryNumber > maxRetries)
        {
            queueEntryRow["InRetry"] = false;
            queueEntryRow["NextAttemptTime"] = DateTime.MaxValue;
            return;
        }
        if (Equals(retryType, WorkQueueRetryType.LinearRetry))
        {
            queueEntryRow["InRetry"] = true;
            queueEntryRow["NextAttemptTime"] = failureTime.AddSeconds(retryIntervalSeconds);
            return;
        }
        if (Equals(retryType, WorkQueueRetryType.ExponentialRetry))
        {
            int minInterval =
                (int)Math.Pow(exponentialRetryBase, retryNumber - 1) * retryIntervalSeconds;
            int maxInterval =
                (int)Math.Pow(exponentialRetryBase, retryNumber) * retryIntervalSeconds;
            int waitTimeSeconds = RandomGenerator.Next(minInterval, maxInterval);

            queueEntryRow["InRetry"] = true;
            queueEntryRow["NextAttemptTime"] = failureTime.AddSeconds(waitTimeSeconds);
            return;
        }

        throw new NotImplementedException($"retryType: {retryType} not implemented");
    }

    public bool CanRunNow(
        DataRow queueEntryRow,
        WorkQueueData.WorkQueueRow queue,
        bool processErrors
    )
    {
        Guid retryType = queue.refWorkQueueRetryTypeId;
        int maxRetries = queue.MaxRetries;
        int attemptCount = GetAttemptCount(queueEntryRow);
        bool inRetry = (bool)queueEntryRow["InRetry"];

        if (
            !inRetry
            && !processErrors
            && !queueEntryRow.IsNull("ErrorText")
            && (string)queueEntryRow["ErrorText"] != ""
        )
        {
            return false;
        }

        if (attemptCount == 0)
        {
            return true;
        }

        if (Equals(retryType, WorkQueueRetryType.NoRetry))
        {
            return false;
        }
        int retryNumber = attemptCount - 1;
        if (retryNumber > maxRetries)
        {
            return false;
        }

        if (queueEntryRow["NextAttemptTime"] == DBNull.Value)
        {
            throw new Exception(
                $"NextAttemptTime is not set on WorkQueueEntry {queueEntryRow["Id"]} while WorkQueueRetryType is not NoRetry and the AttemptCount is not null or 0"
            );
        }

        return (DateTime)queueEntryRow["NextAttemptTime"] <= getTimeNow();
    }

    public void ResetEntry(DataRow queueEntryRow)
    {
        queueEntryRow["AttemptCount"] = 0;
        queueEntryRow["LastAttemptTime"] = DBNull.Value;
        queueEntryRow["NextAttemptTime"] = DBNull.Value;
        queueEntryRow["InRetry"] = false;
    }

    private int GetAttemptCount(DataRow queueEntryRow)
    {
        return queueEntryRow["AttemptCount"] == DBNull.Value
            ? 0
            : (int)queueEntryRow["AttemptCount"];
    }
}
