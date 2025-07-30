using Origam.DA;
using Origam.Workbench.Services.CoreServices;

namespace Origam.WorkflowTests;

public abstract class SqlManager(ICoreDataService dataService)
{
    public static readonly Guid TestQueue1 = new("751BD582-2604-4259-B560-6AFB8A772FCA");
    public static readonly Guid TestQueue2 = new("5AB2B7E6-1181-4CA2-8EB3-39DF505D8EB3");
    public static readonly Guid TestQueue3 = new("E776D7F7-482D-4AFF-B32A-444A9A9959E5");
    public static readonly Guid FailingQueue = new("0AB10C2F-386E-4DD1-992E-5E3765A28447");
    public static readonly Guid RetryQueue = new("8527E8C1-D480-4B12-81A6-F3858B37DC73");
    public static readonly Guid ErrorQueue = new("6584869B-8FD6-44E6-A6C9-5C03D2555A2C");
    public static readonly Guid FailingEntryId = new("0A4B2890-D0D6-4599-A92A-A5E79BA5DCC7");

    protected readonly ICoreDataService dataService = dataService;

    public abstract List<Guid> InsertWorkQueueEntries();
    public abstract List<Guid> InsertFourEntriesToTestQueue3();
    public abstract void EnableThrottlingOnTestQueue3(int throttlingIntervalSeconds, int throttlingItemsPerInterval);
    public abstract void DisableThrottlingOnTestQueue3();
    public abstract void WaitTillWorkQueueEntryTableIsEmptyOrThrow();

    public abstract void SetupQueue(Guid queueId, Guid retryType, int maxRetries,
        int retryIntervalSeconds, Guid? errorQueueId);

    public abstract void CreateWorkQueueModificationTrigger();
    public abstract Dictionary<Guid, int> GetAttemptCountsInQueues(Guid entryId);
    public abstract void DeleteWorkQueueModificationTrigger();
    public abstract void InsertOneEntryIntoFailingQueue();
    public abstract int GetFailingQueueEntryAttempts();
    public abstract int GetEntryCount(Guid workQueueId);
    public abstract void DeleteWorkQueueEntries();
    public abstract int GetWorkQueueEntryCount();
}

public static class SqlManagerFactory
{
    public static SqlManager Create(ICoreDataService coreDataService, IDataService dataService)
    {
        if (dataService is MonitoredMsSqlDataService)
        {
            return new MsSqlManager(coreDataService);
        }

        if (dataService is MonitoredPgSqlDataService)
        {
            return new PgSqlManager(coreDataService);
        }

        throw new NotImplementedException();
    }
}