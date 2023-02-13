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

using NUnit.Framework;
using Origam.OrigamEngine;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;
using Origam.Workflow.WorkQueue;

namespace Origam.WorkflowTests;

// Running more than one test where ConnectRuntime is called will cause an
// error because there is not way to completely rollback all effects of the
// ConnectRuntime call. Hence the commented [Test]/[TestCase] attributes
[TestFixture]
public class WorkQueueTests
{
    private readonly SqlManager sqlManager = new (DataService.Instance);
    
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
}

class TestRuntimeServiceFactory: RuntimeServiceFactory {
    protected override IWorkQueueService CreateWorkQueueService()
    {
        return new WorkQueueService(queueProcessIntervalMillis: 100);
    }
}

