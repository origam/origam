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
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Identity;
using Origam.DA;
using Origam.Security.Common;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Authorization
{
    public static class UserTools
    {
        private const string INITIAL_SETUP_PARAMETERNAME = "InitialUserCreated";
        public static readonly Guid CREATE_USER_WORKFLOW 
            = new Guid("2bd4dbcc-d01e-4c5d-bedb-a4150dcefd54");
        
        public static IOrigamUser Create(DataRow origamUserRow, DataRow businessPartnerRow)
        {
            if(origamUserRow == null && businessPartnerRow == null)
            {
                return null;
            }

            if (origamUserRow == null)
            {
                throw new ArgumentNullException($"A complete user cannot be constructed because {nameof(origamUserRow)} is null.");
            }

            User user = new User();
            user.SecurityStamp = origamUserRow["SecurityStamp"] is DBNull
                ? ""
                :(string)origamUserRow["SecurityStamp"];
            user.Is2FAEnforced = (bool)origamUserRow["Is2FAEnforced"];
            user.EmailConfirmed = (bool)origamUserRow["EmailConfirmed"];
            user.LastLockoutDate = GetDate(origamUserRow,"LastLockoutDate" );
            user.LastLoginDate = GetDate(origamUserRow,"LastLoginDate");
            user.ProviderUserKey = (Guid)origamUserRow["refBusinessPartnerId"];
            user.BusinessPartnerId = user.ProviderUserKey.ToString();
            user.PasswordHash = (string)origamUserRow["Password"];
            user.FailedPasswordAttemptCount = (int)origamUserRow["FailedPasswordAttemptCount"];

            user.UserName = (string)businessPartnerRow["UserName"];
            user.LanguageId = businessPartnerRow["refLanguageId"] is DBNull
                ? Guid.Empty 
                :(Guid)businessPartnerRow["refLanguageId"];
            user.Email = GetStringRow(businessPartnerRow["UserEmail"]);
            user.Name = GetStringRow(businessPartnerRow["Name"]);
            user.FirstName = GetStringRow(businessPartnerRow["FirstName"]);

            return user;
        }
        private static string GetStringRow(object obj)
        {
            return obj is DBNull ? string.Empty : (string)obj;
        }

        public static void AddToOrigamUserRow(IOrigamUser user, DataRow origamUserRow)
        {
            origamUserRow["Id"] = Guid.NewGuid();
            origamUserRow["UserName"] = user.UserName;
            origamUserRow["refBusinessPartnerId"] = user.ProviderUserKey;
            origamUserRow["RecordCreated"] = DateTime.Now;
            origamUserRow["EmailConfirmed"] = user.EmailConfirmed;
            origamUserRow["SecurityStamp"] = user.SecurityStamp;
            origamUserRow["Is2FAEnforced"] = user.Is2FAEnforced;
            SetDate(origamUserRow,"LastLockoutDate", user.LastLockoutDate);
            SetDate(origamUserRow,"LastLoginDate",user.LastLoginDate);
            origamUserRow["Is2FAEnforced"] = user.Is2FAEnforced;
            origamUserRow["Password"] = user.PasswordHash;
            origamUserRow["FailedPasswordAttemptCount"] = user.FailedPasswordAttemptCount;
            origamUserRow["RecordCreatedBy"] = SecurityManager.CurrentUserProfile().Id;
        }
        

        public static void UpdateOrigamUserRow(IOrigamUser user, DataRow origamUserRow)
        {
            origamUserRow["EmailConfirmed"] = user.EmailConfirmed;
            origamUserRow["SecurityStamp"] = user.SecurityStamp;
            origamUserRow["Is2FAEnforced"] = user.Is2FAEnforced;
            SetDate(origamUserRow,"LastLockoutDate", user.LastLockoutDate);
            SetDate(origamUserRow,"LastLoginDate",user.LastLoginDate);
            origamUserRow["Is2FAEnforced"] = user.Is2FAEnforced;
            origamUserRow["Password"] = user.PasswordHash;
            origamUserRow["RecordUpdated"] = DateTime.Now;
            origamUserRow["FailedPasswordAttemptCount"] = user.FailedPasswordAttemptCount;
            origamUserRow["RecordUpdatedBy"] = SecurityManager.CurrentUserProfile().Id;
        }

        private static void SetDate(DataRow row,string columnName, DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue)
            {
                row[columnName] = DBNull.Value;
            }
            else
            {
                row[columnName] = dateTime;
            }
        } 
        
        private static void SetDate(DataRow row,string columnName, DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                row[columnName] = DBNull.Value;
            }
            else
            {
                row[columnName] = dateTime;
            }
        }

        private static DateTime GetDate(DataRow row, string propertyName)
        {
            var value = row[propertyName];
            return value is DBNull 
                ? new DateTime(1900,1,1) 
                : (DateTime)value;
        }

        public static IdentityResult RunCreateUserWorkFlow(string password, IOrigamUser user)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "origam_server"),
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim("name", "origam_server"),
                };
                var identity = new ClaimsIdentity(claims, "TestAuthType");
                Thread.CurrentPrincipal = new ClaimsPrincipal(identity);
                
                user.BusinessPartnerId = Guid.NewGuid().ToString();

                QueryParameterCollection parameters = new QueryParameterCollection();
                parameters.Add(new QueryParameter("Id", user.BusinessPartnerId));
                parameters.Add(new QueryParameter("UserName", user.UserName));
                parameters.Add(new QueryParameter("Password", password));
                parameters.Add(new QueryParameter("FirstName", user.FirstName));
                parameters.Add(new QueryParameter("Name", user.Name));
                parameters.Add(new QueryParameter("Email", user.Email));
                parameters.Add(new QueryParameter("RoleId", user.RoleId));
                parameters.Add(new QueryParameter("RequestEmailConfirmation", !user.EmailConfirmed));
                parameters.Add(new QueryParameter("SecurityStamp", user.SecurityStamp));
                // Will create new line in BusinessPartner and OrigamUser
                WorkflowService.ExecuteWorkflow(
                    CREATE_USER_WORKFLOW, parameters, null);
                return IdentityResult.Success;
            }
            catch (Exception e)
            {
                return IdentityResult.Failed(
                    new IdentityError {Description = e.Message}
                );
            }
        }
        
                
        public static void SetInitialSetupComplete()
        {
            ServiceManager.Services
                .GetService<IParameterService>()
                .SetCustomParameterValue(INITIAL_SETUP_PARAMETERNAME, true,
                    Guid.Empty, 0, null, true, 0, 0, null);
        }
        public static bool IsInitialSetupNeeded()
        {
            return !(bool)ServiceManager.Services
                .GetService<IParameterService>()
                .GetParameterValue(INITIAL_SETUP_PARAMETERNAME);
        }

    }
}