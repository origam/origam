using System;
using System.Data;
using Origam.DA;
using Origam.Security.Common;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Common;

public static class OrigamEventTools
{
    public static void RecordSignInEvent()
    {
        RecordEvent(
            eventId: OrigamEvent.SignIn.EventId, 
            userId: SecurityManager.CurrentUserProfile().Id, 
            details: null);
    }

    public static void RecordSignOutEvent()
    {
        RecordEvent(
            eventId: OrigamEvent.SignOut.EventId, 
            userId: SecurityManager.CurrentUserProfile().Id, 
            details: null);
    }

    private static void RecordEvent(Guid eventId, Guid userId, string details)
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        var dataService  = DataServiceFactory.GetDataService();
        DataSet origamEventDataSet = dataService.GetEmptyDataSet(
            OrigamEvent.DataStructureId);
        DataRow origamEventRecord = origamEventDataSet.Tables[0].NewRow();
        DatasetTools.ApplyPrimaryKey(origamEventRecord);
        origamEventRecord["Timestamp"] = DateTime.Now;
        origamEventRecord["Instance"] = settings.Name;
        origamEventRecord["refOrigamEventTypeId"] = eventId;
        origamEventRecord["refBusinessPartnerId"] = userId;
        if (!string.IsNullOrEmpty(details))
        {
            origamEventRecord["Details"] = details;
        }
        origamEventDataSet.Tables[0].Rows.Add(origamEventRecord);
        DataService.Instance.StoreData(
            dataStructureId: OrigamEvent.DataStructureId,
            data: origamEventDataSet,
            loadActualValuesAfterUpdate: false,
            transactionId: null);
    }
}