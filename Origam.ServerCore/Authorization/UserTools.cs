#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using Origam.Security.Common;

namespace Origam.ServerCore.Authorization
{
    public static class UserTools
    {
        public static IOrigamUser Create(DataRow origamUserRow, DataRow businessPartnerRow)
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
            user.FailedPasswordAttemptCount = (int)origamUserRow["FailedPasswordAttemptCount"];
            
            user.LanguageId = businessPartnerRow["refLanguageId"] is DBNull
                ? Guid.Empty 
                :(Guid)businessPartnerRow["refLanguageId"];
            user.Email = (string)businessPartnerRow["UserEmail"];
            user.Name = (string)businessPartnerRow["Name"];
            user.FirstName = (string)businessPartnerRow["FirstName"];

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
            origamUserRow["FailedPasswordAttemptCount"] = user.FailedPasswordAttemptCount;
            origamUserRow["RecordCreatedBy"] = SecurityManager.CurrentUserProfile().Id;
        }
        

        public static void UpdateOrigamUserRow(IOrigamUser user, DataRow origamUserRow)
        {
            origamUserRow["EmailConfirmed"] = user.EmailConfirmed;
            origamUserRow["Is2FAEnforced"] = user.Is2FAEnforced;
            SetDate(origamUserRow,"LastLockoutDate", user.LastLockoutDate);
            SetDate(origamUserRow,"LastLoginDate",user.LastLoginDate);
            origamUserRow["IsLockedOut"] = user.IsLockedOut;
            origamUserRow["Is2FAEnforced"] = user.Is2FAEnforced;
            origamUserRow["Password"] = user.PasswordHash;
            origamUserRow["RecordUpdated"] = DateTime.Now;
            origamUserRow["FailedPasswordAttemptCount"] = user.FailedPasswordAttemptCount;
            origamUserRow["RecordUpdatedBy"] = SecurityManager.CurrentUserProfile().Id;
        }

        private static void SetDate(DataRow row,string columnName, DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue) return;
            row[columnName] = dateTime;
        } 
        
        private static void SetDate(DataRow row,string columnName, DateTime? dateTime)
        {
            if (!dateTime.HasValue) return;
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