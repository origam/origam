using System;
using System.Data;
using Origam.Security.Common;

namespace Origam.Security.Identity
{
    public static class UserTools
    {
        public static User Create( DataSet dataSet)
        {
            User user = new User();
            if (dataSet.Tables["OrigamUser"].Rows[0]["RecordUpdated"] != null)
            {
                user.SecurityStamp = dataSet.Tables["OrigamUser"] 
                    .Rows[0]["RecordUpdated"].ToString();
            } 
            else if (dataSet.Tables["OrigamUser"].Rows[0]["RecordCreated"] 
                     != null)
            {
                user.SecurityStamp = dataSet.Tables["OrigamUser"] 
                    .Rows[0]["RecordCreated"].ToString();
            }
            user.UserName = (string)dataSet.Tables["OrigamUser"].Rows[0]["UserName"];
            user.Is2FAEnforced = (bool)dataSet.Tables["OrigamUser"].Rows[0]["Is2FAEnforced"];
            user.EmailConfirmed = (bool)dataSet.Tables["OrigamUser"].Rows[0]["EmailConfirmed"];
            user.LastLockoutDate = GetValue<DateTime>(dataSet,"LastLockoutDate" );
            user.LastLoginDate = GetValue<DateTime>(dataSet,"LastLoginDate");
            user.IsLockedOut = (bool)dataSet.Tables["OrigamUser"].Rows[0]["IsLockedOut"];
            user.ProviderUserKey = (Guid)dataSet.Tables["OrigamUser"].Rows[0]["refBusinessPartnerId"];
            user.BusinessPartnerId = user.ProviderUserKey.ToString();
            user.Is2FAEnforced = (bool)dataSet.Tables["OrigamUser"].Rows[0]["Is2FAEnforced"];
            user.PasswordHash =(string)dataSet.Tables["OrigamUser"] .Rows[0]["Password"];
            return user;
        }

        private static T GetValue<T>(DataSet dataSet, string propertyName)
        {
            var value = dataSet.Tables["OrigamUser"].Rows[0][propertyName];
            return value is DBNull 
                ? default 
                : (T)value;
        }
        
        public static void AddToDataRow(IOrigamUser user, DataRow row)
        {
            row["Id"] = Guid.NewGuid();
            row["UserName"] = user.UserName;
            row["refBusinessPartnerId"] = user.ProviderUserKey;
            row["RecordCreated"] = DateTime.Now;
            row["EmailConfirmed"] = user.EmailConfirmed;
            row["Is2FAEnforced"] = user.Is2FAEnforced;
            SetDate(row,"LastLockoutDate", user.LastLockoutDate);
            SetDate(row,"LastLoginDate",user.LastLoginDate);
            row["IsLockedOut"] = user.IsLockedOut;
            row["Is2FAEnforced"] = user.Is2FAEnforced;
            row["Password"] = user.PasswordHash;
            row["RecordCreatedBy"] = SecurityManager.CurrentUserProfile().Id;
        }

        public static void UpdateRow(IOrigamUser user, DataRow row)
        {
            row["EmailConfirmed"] = user.EmailConfirmed;
            row["Is2FAEnforced"] = user.Is2FAEnforced;
            SetDate(row,"LastLockoutDate", user.LastLockoutDate);
            SetDate(row,"LastLoginDate",user.LastLoginDate);
            row["IsLockedOut"] = user.IsLockedOut;
            row["Is2FAEnforced"] = user.Is2FAEnforced;


            row["Password"] = user.PasswordHash;
            row["RecordUpdated"] = DateTime.Now;
            row["RecordUpdatedBy"] = SecurityManager.CurrentUserProfile().Id;
        }
        private static void SetDate(DataRow row,string columnName, DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue) return;
            row[columnName] = dateTime;
        }
    }
}