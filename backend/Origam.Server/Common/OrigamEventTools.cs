#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
using Origam.DA;
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