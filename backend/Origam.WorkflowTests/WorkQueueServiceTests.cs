using NUnit.Framework;
using Origam.OrigamEngine;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;
using Origam.Workflow.WorkQueue;

namespace Origam.WorkflowTests;

[TestFixture]
public class WorkQueueTests
{
    private readonly SqlManager sqlManager = new (DataService.Instance);
    
    [Test]
    public void ShouldTestAllWorkQueueEntriesAreProcessed()
    {
        // ConnectRuntime should start a timer which will cause the work queues
        // to be processed automatically
        OrigamEngine.OrigamEngine.ConnectRuntime(
            customServiceFactory: new TestRuntimeServiceFactory());
        
        List<Guid> createdWorkQueueEntryIds = sqlManager.InsertWorkQueueEntries();
        
        Thread.Sleep(500);
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
    }
}

class TestRuntimeServiceFactory: RuntimeServiceFactory {
    protected override IWorkQueueService CreateWorkQueueService()
    {
        return new WorkQueueService(queueProcessIntervalMillis: 100);
    }
}

