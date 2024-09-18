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
using CoreServices = Origam.Workbench.Services.CoreServices;
using System.Threading;

namespace Origam.Server;
public static class SecurityTools
{
    internal static readonly log4net.ILog log =
        log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
        DataSet data = CoreServices.DataService.Instance.LoadData(
            new Guid("aa4c9df9-d6da-408e-a095-fd377ffcc319"),
            new Guid("ece8b03a-f378-4026-b3b3-588cb58317b6"), 
            Guid.Empty, 
            Guid.Empty, 
            null,
            "OrigamOnlineUser_par_UserName",
            username);
        DataRow row;
        if (data.Tables[0].Rows.Count == 0)
        {
            row = data.Tables[0].NewRow();
            row["Id"] = Guid.NewGuid();
            row["UserName"] 
                = SecurityManager.CurrentPrincipal.Identity.Name;
            row["LastOperationTimestamp"] = DateTime.Now;
            row["DirtyScreens"] = stats.DirtyScreens;
            row["RunningWorkflows"] = stats.RunningWorkflows;
            data.Tables[0].Rows.Add(row);
        }
        else
        {
            row = data.Tables[0].Rows[0];
            row["LastOperationTimestamp"] = DateTime.Now;
            row["DirtyScreens"] = stats.DirtyScreens;
            row["RunningWorkflows"] = stats.RunningWorkflows;
        }
        CoreServices.DataService.Instance.StoreData(
            new Guid("aa4c9df9-d6da-408e-a095-fd377ffcc319"),
            data,
            false,
            null);
    }
   
    public static void RemoveOrigamOnlineUser(string username)
    {
        DataSet data = CoreServices.DataService.Instance.LoadData(
            new Guid("aa4c9df9-d6da-408e-a095-fd377ffcc319"),
            new Guid("ece8b03a-f378-4026-b3b3-588cb58317b6"),
            Guid.Empty,
            Guid.Empty,
            null,
            "OrigamOnlineUser_par_UserName",
            username);
        if (data.Tables[0].Rows.Count != 0)
        {
            data.Tables[0].Rows[0].Delete();
        }
        CoreServices.DataService.Instance.StoreData(
            new Guid("aa4c9df9-d6da-408e-a095-fd377ffcc319"),
            data,
            false,
            null);
    }
}
