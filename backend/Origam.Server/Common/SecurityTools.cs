#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using System.Threading;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Common;
public static class SecurityTools
{
    private static readonly Guid OrigamOnlineUserDataStructureId
        = new("aa4c9df9-d6da-408e-a095-fd377ffcc319");
    private static readonly Guid GetByUserNameMethodId
        = new("ece8b03a-f378-4026-b3b3-588cb58317b6");
    
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
    
    public static UserProfile CurrentUserProfile()
    {
        try
        {
            return SecurityManager.CurrentUserProfile();
        }
        catch (Exception ex)
        {
            log.DebugFormat("Couldn't get user profile for current thread {0}",
                Thread.CurrentThread.ManagedThreadId);
            throw new LoginFailedException(ex.Message, ex);
        }
    }
    
    public static void CreateUpdateOrigamOnlineUser(
        string username, SessionStats stats)
    {
        DataSet origamOnlineUserRecord = DataService.Instance.LoadData(
            dataStructureId: OrigamOnlineUserDataStructureId,
            methodId: GetByUserNameMethodId, 
            defaultSetId: Guid.Empty, 
            sortSetId: Guid.Empty, 
            transactionId: null,
            paramName1: "OrigamOnlineUser_par_UserName",
            paramValue1: username);
        DataRow row;
        if (origamOnlineUserRecord.Tables[0].Rows.Count == 0)
        {
            row = origamOnlineUserRecord.Tables[0].NewRow();
            row["Id"] = Guid.NewGuid();
            row["UserName"] = SecurityManager.CurrentPrincipal.Identity?.Name 
                              ?? string.Empty;
            row["LastOperationTimestamp"] = DateTime.Now;
            row["DirtyScreens"] = stats.DirtyScreens;
            row["RunningWorkflows"] = stats.RunningWorkflows;
            origamOnlineUserRecord.Tables[0].Rows.Add(row);
        }
        else
        {
            row = origamOnlineUserRecord.Tables[0].Rows[0];
            row["LastOperationTimestamp"] = DateTime.Now;
            row["DirtyScreens"] = stats.DirtyScreens;
            row["RunningWorkflows"] = stats.RunningWorkflows;
        }
        DataService.Instance.StoreData(
            dataStructureId: OrigamOnlineUserDataStructureId,
            data: origamOnlineUserRecord,
            loadActualValuesAfterUpdate: false,
            transactionId: null);
    }
   
    public static void RemoveOrigamOnlineUser(string username)
    {
        DataSet origamOnlineUserRecord = DataService.Instance.LoadData(
            dataStructureId: OrigamOnlineUserDataStructureId,
            methodId: GetByUserNameMethodId,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            paramName1: "OrigamOnlineUser_par_UserName",
            paramValue1: username);
        if (origamOnlineUserRecord.Tables[0].Rows.Count != 0)
        {
            origamOnlineUserRecord.Tables[0].Rows[0].Delete();
        }
        DataService.Instance.StoreData(
            dataStructureId: OrigamOnlineUserDataStructureId,
            data: origamOnlineUserRecord,
            loadActualValuesAfterUpdate: false,
            transactionId: null);
    }
    
    public static bool IsInRole(string roleName)
    {
        return SecurityManager
            .GetAuthorizationProvider()
            .Authorize(SecurityManager.CurrentPrincipal, roleName);
    }
}
