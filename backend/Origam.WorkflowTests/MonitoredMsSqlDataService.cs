using System.Data;
using System.Security.Principal;
using Origam.DA;
using Origam.DA.Service;

namespace Origam.WorkflowTests;

public class MonitoredMsSqlDataService : MsSqlDataService
{
    public List<Operation> Operations { get; } = new();

    private void AddOperation(Operation operation)
    {
        Console.WriteLine(operation);
        Operations.Add(operation);
    }

    public override int UpdateData(
        DataStructureQuery query, IPrincipal userProfile, DataSet dataset,
        string transactionId, bool forceBulkInsert)
    {
        var deletesWorkQueueEntry = 
            dataset != null &&
            dataset.Tables.Count == 1 &&
            dataset.Tables[0].TableName == "WorkQueueEntry" &&
            dataset.Tables[0].Rows.Count == 1 &&
            dataset.Tables[0].Rows[0].RowState == DataRowState.Deleted;
        if (deletesWorkQueueEntry)
        {
            dataset.Tables[0].Rows[0].RejectChanges();
            var deletedRowId = (Guid)dataset.Tables[0].Rows[0]["Id"];
            dataset.Tables[0].Rows[0].Delete();

            AddOperation(
                new DeleteWorkQueueEntryOperation("UpdateData",
                    new Dictionary<string, object>
                    {
                        { "query.DataSourceId", query.DataSourceId },
                        { "deletedRowId", deletedRowId },
                    }));
        }

        return base.UpdateData(query, userProfile, dataset,
            transactionId, forceBulkInsert);
    }
}

public record Operation(string Name, Dictionary<string, object> Parameters)
{
    public override string ToString()
    {
        string parameters = string.Join(", ",
            Parameters.Select(x => $"[{x.Key}, {x.Value}]"));
        return $"Operation: {Name}, Parameters: {parameters}";
    }
}

public record DeleteWorkQueueEntryOperation(string Name,
        Dictionary<string, object> Parameters)
    : Operation(Name, Parameters)
{
    public Guid RowId => (Guid)Parameters["deletedRowId"];
}