using System;
using System.Data;
using Origam.Security.Common;

namespace Origam.ServerCore.Authorization
{
    public static class UserTools
    {
        public static User Create(DataRow origamUserRow, DataRow businessPartnerRow)
        {
            User user = new User();
            if (origamUserRow["RecordUpdated"] != null)
            {
                user.SecurityStamp = origamUserRow["RecordUpdated"].ToString();
            } 
            else if (origamUserRow["RecordCreated"] 
                     != null)
            {
                user.SecurityStamp = origamUserRow["RecordCreated"].ToString();
            }
            user.UserName = (string)origamUserRow["UserName"];
            user.Is2FAEnforced = (bool)origamUserRow["Is2FAEnforced"];
            user.EmailConfirmed = (bool)origamUserRow["EmailConfirmed"];
            user.LastLockoutDate = GetValue<DateTime>(origamUserRow,"LastLockoutDate" );
            user.LastLoginDate = GetValue<DateTime>(origamUserRow,"LastLoginDate");
            user.IsLockedOut = (bool)origamUserRow["IsLockedOut"];
            user.ProviderUserKey = (Guid)origamUserRow["refBusinessPartnerId"];
            user.BusinessPartnerId = user.ProviderUserKey.ToString();
            user.Is2FAEnforced = (bool)origamUserRow["Is2FAEnforced"];
            user.PasswordHash =(string)origamUserRow["Password"];

            user.Email = (string)businessPartnerRow["UserEmail"];
            
            return user;
        }

        public static void AddToOrigamUserRow(IOrigamUser user, DataRow origamUserRow)
        {
            origamUserRow["Id"] = Guid.NewGuid();
            origamUserRow["UserName"] = user.UserName;
            origamUserRow["refBusinessPartnerId"] = user.ProviderUserKey;
            origamUserRow["RecordCreated"] = DateTime.Now;
            origamUserRow["EmailConfirmed"] = user.EmailConfirmed;
            origamUserRow["Is2FAEnforced"] = user.Is2FAEnforced;
            SetDate(origamUserRow,"LastLockoutDate", user.LastLockoutDate);
            SetDate(origamUserRow,"LastLoginDate",user.LastLoginDate);
            origamUserRow["IsLockedOut"] = user.IsLockedOut;
            origamUserRow["Is2FAEnforced"] = user.Is2FAEnforced;
            origamUserRow["Password"] = user.PasswordHash;
            origamUserRow["RecordCreatedBy"] = SecurityManager.CurrentUserProfile().Id;
        }
        

        public static void UpdateOrigamUserRow(IOrigamUser user, DataRow origamnUserRow)
        {
            origamnUserRow["EmailConfirmed"] = user.EmailConfirmed;
            origamnUserRow["Is2FAEnforced"] = user.Is2FAEnforced;
            SetDate(origamnUserRow,"LastLockoutDate", user.LastLockoutDate);
            SetDate(origamnUserRow,"LastLoginDate",user.LastLoginDate);
            origamnUserRow["IsLockedOut"] = user.IsLockedOut;
            origamnUserRow["Is2FAEnforced"] = user.Is2FAEnforced;
            origamnUserRow["Password"] = user.PasswordHash;
            origamnUserRow["RecordUpdated"] = DateTime.Now;
            origamnUserRow["RecordUpdatedBy"] = SecurityManager.CurrentUserProfile().Id;
        }

        private static void SetDate(DataRow row,string columnName, DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue) return;
            row[columnName] = dateTime;
        }

        private static T GetValue<T>(DataRow row, string propertyName)
        {
            var value = row[propertyName];
            return value is DBNull 
                ? default 
                : (T)value;
        }
    }
}