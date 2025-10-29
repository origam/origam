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

using System.Reflection;
using log4net.Config;
using NUnit.Framework;
using Origam.OrigamEngine;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;
using Origam.Workflow.WorkQueue;

namespace Origam.WorkflowTests;

[TestFixture]
public class WorkQueueIntegrationTests
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        MethodBase.GetCurrentMethod()?.DeclaringType);
    public WorkQueueIntegrationTests()
    {
        XmlConfigurator.Configure(new FileInfo("log4net.config"));
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        BasicConfigurator.Configure();
    }

    [TearDown]
    public void TearDown()
    {
        SqlManager sqlManager = SqlManagerFactory.Create(
            DataService.Instance,
            DataServiceFactory.GetDataService()
        );
        sqlManager.DeleteWorkQueueEntries();
        Console.WriteLine("\nRunning DisconnectRuntime.");
        OrigamEngine.OrigamEngine.DisconnectRuntime();
    }

    [TestCase("LinearWorkQueueProcessor")]
    [TestCase("RoundRobinWorkQueueProcessor")]
    public void ShouldTestAllWorkQueueEntriesAreProcessed(string configName)
    {
        // ConnectRuntime should start a timer which will cause the work queues
        // to be processed automatically
        OrigamEngine.OrigamEngine.ConnectRuntime(
            configName: configName,
            customServiceFactory: new TestRuntimeServiceFactory()
        );
        SqlManager sqlManager = SqlManagerFactory.Create(
            DataService.Instance,
            DataServiceFactory.GetDataService()
        );
        List<Guid> createdWorkQueueEntryIds = sqlManager.InsertWorkQueueEntries();

        Thread.Sleep(1000);
        sqlManager.WaitTillWorkQueueEntryTableIsEmptyOrThrow();

        // MonitoredMsSqlDataService/MonitoredPgSqlDataService must be set in "DataDataService" element
        // in OrigamSettings.config
        var dataService = (ITraceService)DataServiceFactory.GetDataService();
        var deletedWorkQueueEntryIds = dataService
            .Operations.OfType<DeleteWorkQueueEntryOperation>()
            .Select(x => x.RowId)
            .Reverse()
            .ToList();

        CollectionAssert.AreEquivalent(createdWorkQueueEntryIds, deletedWorkQueueEntryIds);
    }

    [Test]
    public void ShouldTestWorkQueueEntriesAreProcessedInTheRightOrder()
    {
        // ConnectRuntime should start a timer which will cause the work queues
        // to be processed automatically
        OrigamEngine.OrigamEngine.ConnectRuntime(
            configName: "RoundRobinWorkQueueProcessor",
            customServiceFactory: new TestRuntimeServiceFactory()
        );

        // MonitoredMsSqlDataService/MonitoredPgSqlDataService must be set in "DataDataService" element
        // in OrigamSettings.config
        var dataService = DataServiceFactory.GetDataService();
        SqlManager sqlManager = SqlManagerFactory.Create(
            DataService.Instance,
            dataService
        );
        // Insert 19 entries into TestWorkQueue1, TestWorkQueue2 and TestWorkQueue3
        List<Guid> createdWorkQueueEntryIds = sqlManager.InsertWorkQueueEntries();

        Thread.Sleep(1000);
        sqlManager.WaitTillWorkQueueEntryTableIsEmptyOrThrow();
        ITraceService traceService = (ITraceService)dataService;
        log.Debug("operations: "+ traceService.Operations.Count);
        var deleteOperations = traceService
            .Operations.OfType<DeleteWorkQueueEntryOperation>()
            .ToList();
        var deletedWorkQueueEntryIds = deleteOperations.Select(x => x.RowId).Reverse().ToList();

        CollectionAssert.AreEquivalent(createdWorkQueueEntryIds, deletedWorkQueueEntryIds);

        int batchSize = ConfigurationManager.GetActiveConfiguration().RoundRobinBatchSize;

        int numberOfGroups = deleteOperations.Count / batchSize;
        int numberOfGroupsWhereWeExpectEntriesFromASingleQueue = numberOfGroups - 3;
        for (int i = 0; i < numberOfGroupsWhereWeExpectEntriesFromASingleQueue; i++)
        {
            var numberOfWorkQueuesInTheBatchCall = deleteOperations
                .Skip(batchSize * i)
                .Take(batchSize)
                .Select(operation => operation.Parameters["refWorkQueueId"])
                .Distinct()
                .Count();
            Assert.That(numberOfWorkQueuesInTheBatchCall, Is.EqualTo(1));
        }
    }

    [Test]
    public void ShouldTestThrottling()
    {
        const int throttlingIntervalSeconds = 20;
        const int throttlingItemsPerInterval = 3;

        // ConnectRuntime should start a timer which will cause the work queues
        // to be processed automatically
        OrigamEngine.OrigamEngine.ConnectRuntime(
            configName: "LinearWorkQueueProcessor",
            customServiceFactory: new TestRuntimeServiceFactory()
        );
        SqlManager sqlManager = SqlManagerFactory.Create(
            DataService.Instance,
            DataServiceFactory.GetDataService()
        );
        try
        {
            sqlManager.EnableThrottlingOnTestQueue3(
                throttlingIntervalSeconds: throttlingIntervalSeconds,
                throttlingItemsPerInterval: throttlingItemsPerInterval
            );
            List<Guid> createdWorkQueueEntryIds = sqlManager.InsertFourEntriesToTestQueue3();

            Thread.Sleep(1000);
            sqlManager.WaitTillWorkQueueEntryTableIsEmptyOrThrow();

            // MonitoredMsSqlDataService/MonitoredPgSqlDataService must be set in "DataDataService" element
            // in OrigamSettings.config
            var dataService = (ITraceService)DataServiceFactory.GetDataService();
            var deleteOperations = dataService
                .Operations.OfType<DeleteWorkQueueEntryOperation>()
                .OrderBy(operation => operation.ExecutedAt)
                .ToList();

            var firstIntervalOperations = deleteOperations
                .Take(throttlingItemsPerInterval)
                .ToList();
            var firstOperationInFirstInterval = firstIntervalOperations.First();

            var firstOperationInSecondInterval = deleteOperations
                .Skip(throttlingItemsPerInterval)
                .First();
            DateTime expectedSecondIntervalStart =
                firstOperationInFirstInterval.ExecutedAt.AddSeconds(throttlingIntervalSeconds);

            Assert.That(firstOperationInSecondInterval.ExecutedAt > expectedSecondIntervalStart);
            foreach (var operation in firstIntervalOperations)
            {
                Assert.That(operation.ExecutedAt < expectedSecondIntervalStart);
            }

            var deletedWorkQueueEntryIds = deleteOperations.Select(x => x.RowId).Reverse().ToList();

            CollectionAssert.AreEquivalent(createdWorkQueueEntryIds, deletedWorkQueueEntryIds);
        }
        finally
        {
            sqlManager.DisableThrottlingOnTestQueue3();
        }
    }

    [Test]
    public void ShouldRetryProcessingOfFailingQueue()
    {
        // ConnectRuntime should start a timer which will cause the work queues
        // to be processed automatically
        OrigamEngine.OrigamEngine.ConnectRuntime(
            configName: "LinearWorkQueueProcessor",
            customServiceFactory: new TestRuntimeServiceFactory()
        );
        SqlManager sqlManager = SqlManagerFactory.Create(
            DataService.Instance,
            DataServiceFactory.GetDataService()
        );
        int maxRetries = 3;
        sqlManager.SetupQueue(
            queueId: SqlManager.FailingQueue,
            retryType: WorkQueueRetryType.LinearRetry,
            maxRetries: maxRetries,
            retryIntervalSeconds: 1,
            errorQueueId: null
        );
        sqlManager.InsertOneEntryIntoFailingQueue();
        int attempts = 0;
        for (int i = 0; i < 10; i++)
        {
            Thread.Sleep(500);
            attempts = sqlManager.GetFailingQueueEntryAttempts();
            if (attempts + 1 == maxRetries)
            {
                Thread.Sleep(1000);
                if (attempts + 1 != maxRetries)
                {
                    Assert.Fail(
                        $"The failing queue entry was not retried "
                            + $"expected number of times ({maxRetries}). "
                            + $"Number of attempts is {attempts}"
                    );
                }
                return;
            }
        }
        Assert.Fail(
            $"The failing queue entry was not retried expected"
                + $" number of times ({maxRetries}). Number of attempts is {attempts}"
        );
    }

    [Test]
    public void ShouldNotRetryProcessingOfFailingQueueIfNoRetry()
    {
        // ConnectRuntime should start a timer which will cause the work queues
        // to be processed automatically
        OrigamEngine.OrigamEngine.ConnectRuntime(
            configName: "LinearWorkQueueProcessor",
            customServiceFactory: new TestRuntimeServiceFactory()
        );
        SqlManager sqlManager = SqlManagerFactory.Create(
            DataService.Instance,
            DataServiceFactory.GetDataService()
        );
        sqlManager.SetupQueue(
            queueId: SqlManager.FailingQueue,
            retryType: WorkQueueRetryType.NoRetry,
            maxRetries: 3,
            retryIntervalSeconds: 1,
            errorQueueId: null
        );
        sqlManager.InsertOneEntryIntoFailingQueue();
        Thread.Sleep(3000);
        int attempts = sqlManager.GetFailingQueueEntryAttempts();
        Assert.That(attempts, Is.EqualTo(1));
    }

    [Test]
    public void ShouldMoveWorkQueueEntryToErrorQueueAfterError()
    {
        // ConnectRuntime should start a timer which will cause the work queues
        // to be processed automatically
        OrigamEngine.OrigamEngine.ConnectRuntime(
            configName: "LinearWorkQueueProcessor",
            customServiceFactory: new TestRuntimeServiceFactory()
        );
        SqlManager sqlManager = SqlManagerFactory.Create(
            DataService.Instance,
            DataServiceFactory.GetDataService()
        );
        try
        {
            sqlManager.CreateWorkQueueModificationTrigger();
            int maxRetries = 3;
            int expectedAttempts = maxRetries + 1;
            sqlManager.SetupQueue(
                queueId: SqlManager.FailingQueue,
                retryType: WorkQueueRetryType.LinearRetry,
                maxRetries: maxRetries,
                retryIntervalSeconds: 1,
                errorQueueId: SqlManager.RetryQueue
            );
            sqlManager.SetupQueue(
                queueId: SqlManager.RetryQueue,
                retryType: WorkQueueRetryType.LinearRetry,
                maxRetries: maxRetries,
                retryIntervalSeconds: 1,
                errorQueueId: SqlManager.ErrorQueue
            );
            sqlManager.InsertOneEntryIntoFailingQueue();
            Thread.Sleep(15_000);

            int entriesInFailingQueue = sqlManager.GetEntryCount(SqlManager.FailingQueue);
            int entriesInRetryQueue = sqlManager.GetEntryCount(SqlManager.RetryQueue);
            int entriesInErrorQueue = sqlManager.GetEntryCount(SqlManager.ErrorQueue);
            Assert.That(entriesInFailingQueue, Is.EqualTo(0));
            Assert.That(entriesInRetryQueue, Is.EqualTo(0));
            Assert.That(entriesInErrorQueue, Is.EqualTo(1));

            Dictionary<Guid, int> attempts = sqlManager.GetAttemptCountsInQueues(
                SqlManager.FailingEntryId
            );
            Assert.That(attempts[SqlManager.FailingQueue], Is.EqualTo(expectedAttempts));
            Assert.That(attempts[SqlManager.RetryQueue], Is.EqualTo(expectedAttempts));
            Assert.That(attempts[SqlManager.ErrorQueue], Is.EqualTo(0));
        }
        finally
        {
            sqlManager.DeleteWorkQueueModificationTrigger();
        }
    }
}

class TestRuntimeServiceFactory : RuntimeServiceFactory
{
    protected override IWorkQueueService CreateWorkQueueService()
    {
        return new WorkQueueService(queueProcessIntervalMillis: 1000);
    }
}
