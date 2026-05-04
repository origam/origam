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

using System.Data;
using NUnit.Framework;
using Origam.Workflow.WorkQueue;

namespace Origam.WorkflowTests;

public record TestData(
    Guid RetryType,
    int RetryIntervalSeconds,
    int MaxRetries,
    DateTime ExpectedNextAttempt
);

[TestFixture]
public class RetryManagerTests
{
    private static DateTime GetTimeNow()
    {
        return new DateTime(year: 2000, month: 1, day: 1, hour: 0, minute: 0, second: 0);
    }

    private static TestData[] testDataArray = new[]
    {
        new TestData(
            RetryType: WorkQueueRetryType.NoRetry,
            RetryIntervalSeconds: 0,
            MaxRetries: 0,
            ExpectedNextAttempt: DateTime.MaxValue
        ),
        new TestData(
            RetryType: WorkQueueRetryType.LinearRetry,
            RetryIntervalSeconds: 20,
            MaxRetries: 1,
            ExpectedNextAttempt: GetTimeNow().AddSeconds(value: 20)
        ),
    };

    [Test]
    public void ShouldAssignCorrectRetryTimeBasedOnRetryType(
        [ValueSource(sourceName: nameof(testDataArray))] TestData testData
    )
    {
        var (retryType, retryIntervalSeconds, maxRetries, expectedNextAttempt) = testData;

        var queueRow = CreateEmptyQueueRow();
        queueRow.refWorkQueueRetryTypeId = retryType;
        queueRow.MaxRetries = maxRetries;
        queueRow.RetryIntervalSeconds = retryIntervalSeconds;

        var queueEntryRow = CreateEmptyEntryRow();

        var sut = new RetryManager(getTimeNow: GetTimeNow);
        sut.SetEntryRetryData(queueEntryRow: queueEntryRow, queue: queueRow, errorMessage: "Test");

        DateTime nextAttempt = (DateTime)queueEntryRow[columnName: "NextAttemptTime"];
        Assert.That(actual: nextAttempt, expression: Is.EqualTo(expected: expectedNextAttempt));
    }

    [Test]
    public void ShouldAssignCorrectExponentialRetryTime()
    {
        WorkQueueData.WorkQueueRow queueRow = CreateEmptyQueueRow();
        queueRow.refWorkQueueRetryTypeId = WorkQueueRetryType.ExponentialRetry;
        queueRow.MaxRetries = 5;
        queueRow.RetryIntervalSeconds = 35;
        queueRow.ExponentialRetryBase = 2.0m;

        DataRow queueEntryRow = CreateEmptyEntryRow();

        var sut = new RetryManager(getTimeNow: GetTimeNow);

        var expectedDelayLimits = new[]
        {
            (35, 70),
            (70, 140),
            (140, 280),
            (280, 560),
            (560, 1120),
        };

        for (int i = 0; i < 5; i++)
        {
            var (minDelay, maxDelay) = expectedDelayLimits[i];
            var expectedMax = GetTimeNow().AddSeconds(value: maxDelay);
            var expectedMin = GetTimeNow().AddSeconds(value: minDelay);
            sut.SetEntryRetryData(
                queueEntryRow: queueEntryRow,
                queue: queueRow,
                errorMessage: "Test"
            );
            DateTime nextAttempt = (DateTime)queueEntryRow[columnName: "NextAttemptTime"];
            Assert.That(
                actual: nextAttempt,
                expression: Is.LessThanOrEqualTo(expected: expectedMax)
            );
            Assert.That(
                actual: nextAttempt,
                expression: Is.GreaterThanOrEqualTo(expected: expectedMin)
            );
        }
    }

    private static WorkQueueData.WorkQueueRow CreateEmptyQueueRow()
    {
        var workQueueTable = new WorkQueueData.WorkQueueDataTable();
        var queueRow = workQueueTable.NewWorkQueueRow();
        queueRow.MaxRetries = 0;
        queueRow.RetryIntervalSeconds = 0;
        queueRow.ExponentialRetryBase = 2;
        return queueRow;
    }

    private static DataRow CreateEmptyEntryRow()
    {
        var queueEntryTable = new DataTable();
        queueEntryTable.Columns.Add(
            column: new DataColumn(columnName: "InRetry", dataType: typeof(bool))
        );
        queueEntryTable.Columns.Add(
            column: new DataColumn(columnName: "ErrorText", dataType: typeof(string))
        );
        queueEntryTable.Columns.Add(
            column: new DataColumn(columnName: "LastAttemptTime", dataType: typeof(DateTime))
        );
        queueEntryTable.Columns.Add(
            column: new DataColumn(columnName: "AttemptCount", dataType: typeof(int))
        );
        queueEntryTable.Columns.Add(
            column: new DataColumn(columnName: "NextAttemptTime", dataType: typeof(DateTime))
        );
        var queueEntryRow = queueEntryTable.NewRow();
        return queueEntryRow;
    }
}
