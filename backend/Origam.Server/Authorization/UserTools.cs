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

namespace Origam.Server.Authorization;

public static class UserTools
{
    private const string INITIAL_SETUP_PARAMETERNAME = "InitialUserCreated";
    public static readonly Guid CREATE_USER_WORKFLOW = new Guid(
        g: "2bd4dbcc-d01e-4c5d-bedb-a4150dcefd54"
    );

    public static IOrigamUser Create(DataRow origamUserRow, DataRow businessPartnerRow)
    {
        if (origamUserRow == null && businessPartnerRow == null)
        {
            return null;
        }
        if (origamUserRow == null)
        {
            throw new ArgumentNullException(
                paramName: $"A complete user cannot be constructed because {nameof(origamUserRow)} is null."
            );
        }
        User user = new User();
        user.SecurityStamp =
            origamUserRow[columnName: "SecurityStamp"] is DBNull
                ? ""
                : (string)origamUserRow[columnName: "SecurityStamp"];
        user.Is2FAEnforced = (bool)origamUserRow[columnName: "Is2FAEnforced"];
        user.EmailConfirmed = (bool)origamUserRow[columnName: "EmailConfirmed"];
        user.LastLockoutDate = GetDate(row: origamUserRow, propertyName: "LastLockoutDate");
        user.LastLoginDate = GetDate(row: origamUserRow, propertyName: "LastLoginDate");
        user.ProviderUserKey = (Guid)origamUserRow[columnName: "refBusinessPartnerId"];
        user.BusinessPartnerId = user.ProviderUserKey.ToString();
        user.PasswordHash = (string)origamUserRow[columnName: "Password"];
        user.FailedPasswordAttemptCount = (int)
            origamUserRow[columnName: "FailedPasswordAttemptCount"];
        user.UserName = (string)businessPartnerRow[columnName: "UserName"];
        user.LanguageId =
            businessPartnerRow[columnName: "refLanguageId"] is DBNull
                ? Guid.Empty
                : (Guid)businessPartnerRow[columnName: "refLanguageId"];
        user.Email = GetStringRow(obj: businessPartnerRow[columnName: "UserEmail"]);
        user.Name = GetStringRow(obj: businessPartnerRow[columnName: "Name"]);
        user.FirstName = GetStringRow(obj: businessPartnerRow[columnName: "FirstName"]);
        return user;
    }

    private static string GetStringRow(object obj)
    {
        return obj is DBNull ? string.Empty : (string)obj;
    }

    public static void AddToOrigamUserRow(IOrigamUser user, DataRow origamUserRow)
    {
        origamUserRow[columnName: "Id"] = Guid.NewGuid();
        origamUserRow[columnName: "UserName"] = user.UserName;
        origamUserRow[columnName: "refBusinessPartnerId"] = user.ProviderUserKey;
        origamUserRow[columnName: "RecordCreated"] = DateTime.Now;
        origamUserRow[columnName: "EmailConfirmed"] = user.EmailConfirmed;
        origamUserRow[columnName: "SecurityStamp"] = user.SecurityStamp;
        origamUserRow[columnName: "Is2FAEnforced"] = user.Is2FAEnforced;
        SetDate(row: origamUserRow, columnName: "LastLockoutDate", dateTime: user.LastLockoutDate);
        SetDate(row: origamUserRow, columnName: "LastLoginDate", dateTime: user.LastLoginDate);
        origamUserRow[columnName: "Is2FAEnforced"] = user.Is2FAEnforced;
        origamUserRow[columnName: "Password"] = user.PasswordHash;
        origamUserRow[columnName: "FailedPasswordAttemptCount"] = user.FailedPasswordAttemptCount;
        origamUserRow[columnName: "RecordCreatedBy"] = SecurityManager.CurrentUserProfile().Id;
    }

    public static void UpdateOrigamUserRow(IOrigamUser user, DataRow origamUserRow)
    {
        origamUserRow[columnName: "EmailConfirmed"] = user.EmailConfirmed;
        origamUserRow[columnName: "SecurityStamp"] = user.SecurityStamp;
        origamUserRow[columnName: "Is2FAEnforced"] = user.Is2FAEnforced;
        SetDate(row: origamUserRow, columnName: "LastLockoutDate", dateTime: user.LastLockoutDate);
        SetDate(row: origamUserRow, columnName: "LastLoginDate", dateTime: user.LastLoginDate);
        origamUserRow[columnName: "Is2FAEnforced"] = user.Is2FAEnforced;
        origamUserRow[columnName: "Password"] = user.PasswordHash;
        origamUserRow[columnName: "RecordUpdated"] = DateTime.Now;
        origamUserRow[columnName: "FailedPasswordAttemptCount"] = user.FailedPasswordAttemptCount;
        origamUserRow[columnName: "RecordUpdatedBy"] = SecurityManager.CurrentUserProfile().Id;
    }

    private static void SetDate(DataRow row, string columnName, DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue)
        {
            row[columnName: columnName] = DBNull.Value;
        }
        else
        {
            row[columnName: columnName] = dateTime;
        }
    }

    private static void SetDate(DataRow row, string columnName, DateTime? dateTime)
    {
        if (!dateTime.HasValue)
        {
            row[columnName: columnName] = DBNull.Value;
        }
        else
        {
            row[columnName: columnName] = dateTime;
        }
    }

    private static DateTime GetDate(DataRow row, string propertyName)
    {
        var value = row[columnName: propertyName];
        return value is DBNull ? new DateTime(year: 1900, month: 1, day: 1) : (DateTime)value;
    }

    public static IdentityResult RunCreateUserWorkFlow(string password, IOrigamUser user)
    {
        try
        {
            var claims = new List<Claim>
            {
                new Claim(type: ClaimTypes.Name, value: "origam_server"),
                new Claim(type: ClaimTypes.NameIdentifier, value: "1"),
                new Claim(type: "name", value: "origam_server"),
            };
            var identity = new ClaimsIdentity(claims: claims, authenticationType: "TestAuthType");
            Thread.CurrentPrincipal = new ClaimsPrincipal(identity: identity);

            user.BusinessPartnerId = Guid.NewGuid().ToString();
            QueryParameterCollection parameters = new QueryParameterCollection();
            parameters.Add(
                value: new QueryParameter(_parameterName: "Id", value: user.BusinessPartnerId)
            );
            parameters.Add(
                value: new QueryParameter(_parameterName: "UserName", value: user.UserName)
            );
            parameters.Add(value: new QueryParameter(_parameterName: "Password", value: password));
            parameters.Add(
                value: new QueryParameter(_parameterName: "FirstName", value: user.FirstName)
            );
            parameters.Add(value: new QueryParameter(_parameterName: "Name", value: user.Name));
            parameters.Add(value: new QueryParameter(_parameterName: "Email", value: user.Email));
            parameters.Add(value: new QueryParameter(_parameterName: "RoleId", value: user.RoleId));
            parameters.Add(
                value: new QueryParameter(
                    _parameterName: "RequestEmailConfirmation",
                    value: !user.EmailConfirmed
                )
            );
            parameters.Add(
                value: new QueryParameter(
                    _parameterName: "SecurityStamp",
                    value: user.SecurityStamp
                )
            );
            // Will create new line in BusinessPartner and OrigamUser
            WorkflowService.ExecuteWorkflow(
                workflowId: CREATE_USER_WORKFLOW,
                parameters: parameters,
                transactionId: null
            );
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            return IdentityResult.Failed(errors: new IdentityError { Description = e.Message });
        }
    }

    public static void SetInitialSetupComplete()
    {
        ServiceManager
            .Services.GetService<IParameterService>()
            .SetCustomParameterValue(
                parameterName: INITIAL_SETUP_PARAMETERNAME,
                value: true,
                guidValue: Guid.Empty,
                intValue: 0,
                stringValue: null,
                boolValue: true,
                floatValue: 0,
                currencyValue: 0,
                dateValue: null
            );
    }

    public static bool IsInitialSetupNeeded()
    {
        return !(bool)
            ServiceManager
                .Services.GetService<IParameterService>()
                .GetParameterValue(parameterName: INITIAL_SETUP_PARAMETERNAME);
    }
}
