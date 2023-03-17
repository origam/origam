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
    private readonly SqlManager sqlManager = new (DataService.Instance);

    public WorkQueueIntegrationTests()
    {
        XmlConfigurator.Configure(new FileInfo("log4net.config")); 
    }
    
    [SetUp]
    public void Setup()
    {
        BasicConfigurator.Configure();
    }

    [TestCase("LinearWorkQueueProcessor")]
    [TestCase("RoundRobinWorkQueueProcessor")]
    public void ShouldTestAllWorkQueueEntriesAreProcessed(string configName)
    {
        // ConnectRuntime should start a timer which will cause the work queues
        // to be processed automatically
        OrigamEngine.OrigamEngine.ConnectRuntime(
            configName: configName,
            customServiceFactory: new TestRuntimeServiceFactory());
        
        List<Guid> createdWorkQueueEntryIds = sqlManager.InsertWorkQueueEntries();
        
        Thread.Sleep(1000);
        sqlManager.WaitTillWorkQueueEntryTableIsEmptyOrThrow();
        
        // MonitoredMsSqlDataService must be set in "DataDataService" element
        // in OrigamSettings.config
        var dataService = DataServiceFactory.GetDataService() as MonitoredMsSqlDataService;
        var deletedWorkQueueEntryIds = dataService.Operations.OfType<DeleteWorkQueueEntryOperation>()
            .Select(x => x.RowId)
            .Reverse()
            .ToList();
        
        CollectionAssert.AreEquivalent(
            createdWorkQueueEntryIds,
            deletedWorkQueueEntryIds);
        
        Console.WriteLine("\nRunning DisconnectRuntime. There might be some errors logged here. These are probably not a problem.\n");
        OrigamEngine.OrigamEngine.DisconnectRuntime();
        Thread.Sleep(1000);
    }
    
    [Test]
    public void ShouldTestWorkQueueEntriesAreProcessedInTheRightOrder()
    {
        // ConnectRuntime should start a timer which will cause the work queues
        // to be processed automatically
        OrigamEngine.OrigamEngine.ConnectRuntime(
            configName: "RoundRobinWorkQueueProcessor",
            customServiceFactory: new TestRuntimeServiceFactory());
        
        List<Guid> createdWorkQueueEntryIds = sqlManager.InsertWorkQueueEntries();
        
        Thread.Sleep(1000);
        sqlManager.WaitTillWorkQueueEntryTableIsEmptyOrThrow();
        
        // MonitoredMsSqlDataService must be set in "DataDataService" element
        // in OrigamSettings.config
        var dataService = DataServiceFactory.GetDataService() as MonitoredMsSqlDataService;
        var deleteOperations = dataService.Operations
            .OfType<DeleteWorkQueueEntryOperation>()
            .ToList();
        var deletedWorkQueueEntryIds = deleteOperations
            .Select(x => x.RowId)
            .Reverse()
            .ToList();
        
        CollectionAssert.AreEquivalent(
            createdWorkQueueEntryIds,
            deletedWorkQueueEntryIds);
        
        int batchSize = ConfigurationManager
            .GetActiveConfiguration()
            .RoundRobinBatchSize;

        int numberOfGroups = deleteOperations.Count / batchSize;
        int numberOfGroupsWhereWeExpectEntriesFromASingleQueue =
            numberOfGroups - 3; 
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

        OrigamEngine.OrigamEngine.DisconnectRuntime();
    }
    
    [Test]
    public void ShouldTestThrottling()
    {
        int throttlingIntervalSeconds = 20;
        int throttlingItemsPerInterval = 3;
        
        // ConnectRuntime should start a timer which will cause the work queues
        // to be processed automatically
        OrigamEngine.OrigamEngine.ConnectRuntime(
            configName: "LinearWorkQueueProcessor",
            customServiceFactory: new TestRuntimeServiceFactory());
        try
        {
            sqlManager.EnableThrottling(
                throttlingIntervalSeconds: throttlingIntervalSeconds, 
                throttlingItemsPerInterval: throttlingItemsPerInterval);
            List<Guid> createdWorkQueueEntryIds = sqlManager.InsertThrottlingTestWorkQueueEntries();
            
            Thread.Sleep(1000);
            sqlManager.WaitTillWorkQueueEntryTableIsEmptyOrThrow();
            
            // MonitoredMsSqlDataService must be set in "DataDataService" element
            // in OrigamSettings.config
            var dataService = DataServiceFactory.GetDataService() as MonitoredMsSqlDataService;
            var deleteOperations = dataService.Operations
                .OfType<DeleteWorkQueueEntryOperation>()
                .OrderBy(operation => operation.ExecutedAt)
                .ToList();
            
            var firstIntervalOperations = deleteOperations
                .Take(throttlingItemsPerInterval)
                .ToList();
            var firstOperationInFirstInterval = firstIntervalOperations
                .First();
            
            var firstOperationInSecondInterval = deleteOperations
                .Skip(throttlingItemsPerInterval)
                .First();
            DateTime expectedSecondIntervalStart = firstOperationInFirstInterval
                .ExecutedAt
                .AddSeconds(throttlingIntervalSeconds);
            
            Assert.That(
                firstOperationInSecondInterval.ExecutedAt > expectedSecondIntervalStart);
            foreach (var operation in firstIntervalOperations)
            {
                Assert.That(operation.ExecutedAt < expectedSecondIntervalStart);
            }
            
            var deletedWorkQueueEntryIds = deleteOperations
                .Select(x => x.RowId)
                .Reverse()
                .ToList();
        
            CollectionAssert.AreEquivalent(
                createdWorkQueueEntryIds,
                deletedWorkQueueEntryIds);
        }
        finally
        {
            sqlManager.DisableThrottling();
            OrigamEngine.OrigamEngine.DisconnectRuntime();
        }
    }
    
    [Test]
    public void ShouldRetryProcessingOfFailingQueue()
    {
        // ConnectRuntime should start a timer which will cause the work queues
        // to be processed automatically
        OrigamEngine.OrigamEngine.ConnectRuntime(
            configName: "LinearWorkQueueProcessor",
            customServiceFactory: new TestRuntimeServiceFactory());
        try
        {
            int maxRetries = 3;
            sqlManager.SetupFailingQueue(
                retryType: WorkQueueRetryType.LinearRetry,
                maxRetries: maxRetries,
                retryIntervalSeconds: 1,
                moveToErrorQueue: false);
            sqlManager.InsertEntriesIntoFailingQueue();
            int attempts = 0;
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(500);
                attempts = sqlManager.GetFailingQueueEntryAttempts();
                if (attempts + 1 == maxRetries)
                {
                    return;
                }
            }
            Assert.Fail($"The failing queue entry was not retried expected number of times ({maxRetries}). Number of attempts is {attempts}");
        }
        finally
        {
            sqlManager.ClearFailingQueue();
        }
    }
    
    [Test]
    public void ShouldNotRetryProcessingOfFailingQueueIfNoRetry()
    {
        // ConnectRuntime should start a timer which will cause the work queues
        // to be processed automatically
        OrigamEngine.OrigamEngine.ConnectRuntime(
            configName: "LinearWorkQueueProcessor",
            customServiceFactory: new TestRuntimeServiceFactory());
        try
        {
            sqlManager.SetupFailingQueue(
                retryType: WorkQueueRetryType.NoRetry,
                maxRetries: 3,
                retryIntervalSeconds: 1,
                moveToErrorQueue: false);
            sqlManager.InsertEntriesIntoFailingQueue();
            Thread.Sleep(3000);
            int attempts = sqlManager.GetFailingQueueEntryAttempts();
            Assert.That(attempts, Is.EqualTo(1));
        }
        finally
        {
            sqlManager.ClearFailingQueue();
        }
    }
}

class TestRuntimeServiceFactory: RuntimeServiceFactory {
    protected override IWorkQueueService CreateWorkQueueService()
    {
        return new WorkQueueService(queueProcessIntervalMillis: 1000);
    }
}

