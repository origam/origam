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
        double exponentialRetryBase = decimal.ToDouble(d: queue.ExponentialRetryBase);

        var failureTime = getTimeNow();
        queueEntryRow[columnName: "ErrorText"] = failureTime + ": " + errorMessage;
        queueEntryRow[columnName: "LastAttemptTime"] = failureTime;
        int attemptCount = GetAttemptCount(queueEntryRow: queueEntryRow);
        int newAttemptCount = attemptCount + 1; // = 1 after the first failure
        int retryNumber = newAttemptCount;
        queueEntryRow[columnName: "AttemptCount"] = newAttemptCount;

        if (Equals(objA: retryType, objB: WorkQueueRetryType.NoRetry) || retryNumber > maxRetries)
        {
            queueEntryRow[columnName: "InRetry"] = false;
            queueEntryRow[columnName: "NextAttemptTime"] = DateTime.MaxValue;
            return;
        }
        if (Equals(objA: retryType, objB: WorkQueueRetryType.LinearRetry))
        {
            queueEntryRow[columnName: "InRetry"] = true;
            queueEntryRow[columnName: "NextAttemptTime"] = failureTime.AddSeconds(
                value: retryIntervalSeconds
            );
            return;
        }
        if (Equals(objA: retryType, objB: WorkQueueRetryType.ExponentialRetry))
        {
            int minInterval =
                (int)Math.Pow(x: exponentialRetryBase, y: retryNumber - 1) * retryIntervalSeconds;
            int maxInterval =
                (int)Math.Pow(x: exponentialRetryBase, y: retryNumber) * retryIntervalSeconds;
            int waitTimeSeconds = RandomGenerator.Next(
                minValue: minInterval,
                maxValue: maxInterval
            );

            queueEntryRow[columnName: "InRetry"] = true;
            queueEntryRow[columnName: "NextAttemptTime"] = failureTime.AddSeconds(
                value: waitTimeSeconds
            );
            return;
        }

        throw new NotImplementedException(message: $"retryType: {retryType} not implemented");
    }

    public bool CanRunNow(
        DataRow queueEntryRow,
        WorkQueueData.WorkQueueRow queue,
        bool processErrors
    )
    {
        Guid retryType = queue.refWorkQueueRetryTypeId;
        int maxRetries = queue.MaxRetries;
        int attemptCount = GetAttemptCount(queueEntryRow: queueEntryRow);
        bool inRetry = (bool)queueEntryRow[columnName: "InRetry"];

        if (
            !inRetry
            && !processErrors
            && !queueEntryRow.IsNull(columnName: "ErrorText")
            && (string)queueEntryRow[columnName: "ErrorText"] != ""
        )
        {
            return false;
        }

        if (attemptCount == 0)
        {
            return true;
        }

        if (Equals(objA: retryType, objB: WorkQueueRetryType.NoRetry))
        {
            return false;
        }
        int retryNumber = attemptCount - 1;
        if (retryNumber > maxRetries)
        {
            return false;
        }

        if (queueEntryRow[columnName: "NextAttemptTime"] == DBNull.Value)
        {
            throw new Exception(
                message: $"NextAttemptTime is not set on WorkQueueEntry {queueEntryRow[columnName: "Id"]} while WorkQueueRetryType is not NoRetry and the AttemptCount is not null or 0"
            );
        }

        return (DateTime)queueEntryRow[columnName: "NextAttemptTime"] <= getTimeNow();
    }

    public void ResetEntry(DataRow queueEntryRow)
    {
        queueEntryRow[columnName: "AttemptCount"] = 0;
        queueEntryRow[columnName: "LastAttemptTime"] = DBNull.Value;
        queueEntryRow[columnName: "NextAttemptTime"] = DBNull.Value;
        queueEntryRow[columnName: "InRetry"] = false;
    }

    private int GetAttemptCount(DataRow queueEntryRow)
    {
        return queueEntryRow[columnName: "AttemptCount"] == DBNull.Value
            ? 0
            : (int)queueEntryRow[columnName: "AttemptCount"];
    }
}
