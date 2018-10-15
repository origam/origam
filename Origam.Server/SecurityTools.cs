using System;
using System.Data;
using Origam;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.Server
{
    static class SecurityTools
    {
        internal static UserProfile CurrentUserProfile()
        {
            try
            {
                return SecurityManager.CurrentUserProfile();
            }
            catch (Exception ex)
            {
                throw new LoginFailedException(ex.Message, ex);
            }
        }
        
        internal static void CreateUpdateOrigamOnlineUser(
            string username, SessionStats stats)
        {
            DataSet data = core.DataService.LoadData(
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
            core.DataService.StoreData(
                new Guid("aa4c9df9-d6da-408e-a095-fd377ffcc319"),
                data,
                false,
                null);
        }

       
        internal static void RemoveOrigamOnlineUser(string username)
        {
            DataSet data = core.DataService.LoadData(
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
            core.DataService.StoreData(
                new Guid("aa4c9df9-d6da-408e-a095-fd377ffcc319"),
                data,
                false,
                null);
        }
    }
}