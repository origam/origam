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

public record TestData(Guid RetryType, int RetryIntervalSeconds, int MaxRetries, DateTime ExpectedNextAttempt);

[TestFixture]
public class RetryManagerTests
{
    private static DateTime GetTimeNow()
    {
        return new DateTime(2000, 1, 1, 0, 0, 0);
    }

    
    private static TestData[] testDataArray = new[]{
        new TestData (WorkQueueRetryType.NoRetry, 0, 0,DateTime.MaxValue),
        new TestData(WorkQueueRetryType.LinearRetry, 20, 1,GetTimeNow().AddSeconds(20)),
    };

    [Test]
    public void ShouldAssignCorrectRetryTimeBasedOnRetryType(
        [ValueSource(nameof(testDataArray))]TestData testData)
    {
        var (retryType,
            retryIntervalSeconds, 
            maxRetries, 
            expectedNextAttempt) = testData;
        
        var queueRow = CreateEmptyQueueRow();
        queueRow["refWorkQueueRetryTypeId"] = retryType;
        queueRow["MaxRetries"] = maxRetries;
        queueRow["RetryIntervalSeconds"] = retryIntervalSeconds;
        
        var queueEntryRow = CreateEmptyEntryRow();
        
        var sut = new RetryManager(GetTimeNow);
        sut.SetEntryRetryData(queueEntryRow, queueRow, "Test");

        DateTime nextAttempt = (DateTime)queueEntryRow["NextAttemptTime"];
        Assert.That(nextAttempt, Is.EqualTo(expectedNextAttempt));
    }

    private static WorkQueueData.WorkQueueRow CreateEmptyQueueRow()
    {
        var workQueueTable = new WorkQueueData.WorkQueueDataTable();
        workQueueTable.Columns.Add(
            new DataColumn("MaxRetryIntervalSeconds", typeof(int)));   
        workQueueTable.Columns.Add(
            new DataColumn("MinRetryIntervalSeconds", typeof(int)));
        var queueRow = workQueueTable.NewWorkQueueRow();
        queueRow["MaxRetries"] = 0;
        queueRow["RetryIntervalSeconds"] = 0;
        return queueRow;
    }

    private static DataRow CreateEmptyEntryRow()
    {
        var queueEntryTable = new DataTable();
        queueEntryTable.Columns.Add(
            new DataColumn("ErrorText", typeof(string)));
        queueEntryTable.Columns.Add(
            new DataColumn("LastAttemptTime", typeof(DateTime)));
        queueEntryTable.Columns.Add(
            new DataColumn("AttemptCount", typeof(int)));
        queueEntryTable.Columns.Add(
            new DataColumn("NextAttemptTime", typeof(DateTime)));
        var queueEntryRow = queueEntryTable.NewRow();
        return queueEntryRow;
    }
}