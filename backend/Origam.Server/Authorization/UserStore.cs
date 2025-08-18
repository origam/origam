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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Security.Common;
using Origam.Server.Authorization;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server;
public sealed class UserStore : IUserStore<IOrigamUser>, IUserEmailStore<IOrigamUser>,
    IUserTwoFactorStore<IOrigamUser>, IUserPasswordStore<IOrigamUser>,IUserLockoutStore<IOrigamUser>,
    IUserPhoneNumberStore<IOrigamUser>, IUserAuthenticatorKeyStore<IOrigamUser>, IUserSecurityStampStore<IOrigamUser>
{
    public static readonly Guid ORIGAM_USER_DATA_STRUCTURE
        = new Guid("43b67a51-68f3-4696-b08d-de46ae0223ce");
    public static readonly Guid GET_ORIGAM_USER_BY_BUSINESS_PARTNER_ID
        = new Guid("982f45a9-b610-4e2f-8d7f-2b1eebe93390");
    public static readonly Guid GET_ORIGAM_USER_BY_USER_NAME
        = new Guid("a60c9817-ae18-465c-a91f-d4b8a25f15a4");
    public static readonly Guid BUSINESS_PARTNER_DATA_STRUCTURE
        = new Guid("f4c92dce-d634-4179-adb4-98876b870cc7");
    public static readonly Guid GET_BUSINESS_PARTNER_BY_USER_NAME
        = new Guid("545396e7-d88e-4315-a112-f8feda7229bf");
    public static readonly Guid GET_BUSINESS_PARTNER_BY_ID
        = new Guid("4e46424b-349f-4314-bc75-424206cd35b0");
    public static readonly Guid GET_BUSINESS_PARTNER_BY_USER_EMAIL
        = new Guid("46fd2484-4506-45a2-8a96-7855ea116210");
    
    private readonly IStringLocalizer<SharedResources> localizer;
    public UserStore(IStringLocalizer<SharedResources> localizer)
    {
        this.localizer = localizer;
    }
    public Task<IdentityResult> CreateAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        DatasetGenerator dataSetGenerator = new DatasetGenerator(true);
        IPersistenceService persistenceService = ServiceManager.Services
            .GetService(typeof(IPersistenceService)) as IPersistenceService;
        DataStructure dataStructure = (DataStructure) persistenceService
            .SchemaProvider.RetrieveInstance(typeof(ISchemaItem),
                new ModelElementKey(ORIGAM_USER_DATA_STRUCTURE));
        DataSet origamUserDataSet = dataSetGenerator.CreateDataSet(
            dataStructure);
        DataRow origamUserRow
            = origamUserDataSet.Tables["OrigamUser"].NewRow();
        UserTools.AddToOrigamUserRow(user ,origamUserRow);
        origamUserDataSet.Tables["OrigamUser"].Rows.Add(origamUserRow);
        DataService.Instance.StoreData(ORIGAM_USER_DATA_STRUCTURE,
            origamUserDataSet, false, user.TransactionId);
        return Task.FromResult(IdentityResult.Success);
    }
 
    public Task<IdentityResult> DeleteAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        DataRow origamUserRow = FindOrigamUserRowByUserName(user.UserName, user.TransactionId);
        if (origamUserRow == null)
        {
            throw new Exception($"User {user.UserName} already doesn't have access to the system.");
        }
        origamUserRow.Delete();
        DataService.Instance.StoreData(
            ORIGAM_USER_DATA_STRUCTURE,
            origamUserRow.Table.DataSet, 
            false, 
            user.TransactionId);
        return Task.FromResult(IdentityResult.Success);
    }
 
    public Task<IOrigamUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        DataRow origamUserRow = FindOrigamUserRowById(userId);
        if (origamUserRow == null) return Task.FromResult<IOrigamUser>(null);
        DataRow businessPartnerRow = FindBusinessPartnerRowById(userId);
        if (businessPartnerRow == null) return Task.FromResult<IOrigamUser>(null);
        return Task.FromResult(
            UserTools.Create(
            origamUserRow: origamUserRow, 
            businessPartnerRow: businessPartnerRow));
    }
    public Task<IOrigamUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return FindByNameAsync(normalizedUserName, null, cancellationToken);
    }
    public Task<IOrigamUser> FindByNameAsync(string normalizedUserName, string transactionId, CancellationToken cancellationToken)
    {
        var origamUserRow = FindOrigamUserRowByUserName(normalizedUserName, transactionId);
        if (origamUserRow == null) return Task.FromResult<IOrigamUser>(null);
        var businessPartnerRow = FindBusinessPartnerRowByUserName(normalizedUserName, transactionId);
        if (businessPartnerRow == null) return Task.FromResult<IOrigamUser>(null);
        return Task.FromResult(
            UserTools.Create(
            origamUserRow: origamUserRow,
            businessPartnerRow: businessPartnerRow));
    }
    public Task<string> GetNormalizedUserNameAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }
 
    public Task<string> GetUserIdAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.BusinessPartnerId);
    }
 
    public Task<string> GetUserNameAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }
 
    public Task SetNormalizedUserNameAsync(IOrigamUser user, string normalizedName, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
 
    public Task SetUserNameAsync(IOrigamUser user, string userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }
 
    public Task<IdentityResult> UpdateAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        DataRow origamUserRow = FindOrigamUserRowByUserName(user.UserName, user.TransactionId);
        if(origamUserRow == null)
        {
            return 
                Task.FromResult(
                    IdentityResult.Failed( 
                         new IdentityError{Description = localizer["ErrorUserNotFound"]}));
        }
        UserTools.UpdateOrigamUserRow(user, origamUserRow);
        DataService.Instance.StoreData(ORIGAM_USER_DATA_STRUCTURE,
            origamUserRow.Table.DataSet,
            false, null);
        return Task.FromResult(IdentityResult.Success);
    }
    public Task SetEmailAsync(IOrigamUser user, string email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }
 
    public Task<string> GetEmailAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }
 
    public Task<bool> GetEmailConfirmedAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailConfirmed);
    }
 
    public Task SetEmailConfirmedAsync(IOrigamUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }
 
    public Task<IOrigamUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        DataRow businessPartnerRow = FindBusinessPartnerRowByEmail(normalizedEmail);
        if (businessPartnerRow == null) return Task.FromResult<IOrigamUser>(null);
        string userName = businessPartnerRow["UserName"] == DBNull.Value 
            ? null 
            : (string)businessPartnerRow["UserName"];
        DataRow origamUserRow = FindOrigamUserRowByUserName(userName, null);
        if (origamUserRow == null) return Task.FromResult<IOrigamUser>(null);
        
        return Task.FromResult(
            UserTools.Create(
            origamUserRow: origamUserRow, 
            businessPartnerRow: businessPartnerRow)); 
        
    }
    public Task<string> GetNormalizedEmailAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail);
    }
 
    public Task SetNormalizedEmailAsync(IOrigamUser user, string normalizedEmail, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
 
    public Task SetTwoFactorEnabledAsync(IOrigamUser user, bool enabled, CancellationToken cancellationToken)
    {
        user.TwoFactorEnabled = enabled;
        return Task.CompletedTask;
    }
 
    public Task<bool> GetTwoFactorEnabledAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.TwoFactorEnabled);
    }
 
    public Task SetPasswordHashAsync(IOrigamUser user, string passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }
 
    public Task<string> GetPasswordHashAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }
 
    public Task<bool> HasPasswordAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash != null);
    }
 
    public void Dispose()
    {
        // Nothing to dispose.
    }
    
    private static DataRow FindBusinessPartnerRowByEmail(string email)
    {
        DataSet businessPartnerDataSet = GetBusinessPartnerDataSet(
            GET_BUSINESS_PARTNER_BY_USER_EMAIL,
            "BusinessPartner_parUserEmail", email);
        if (businessPartnerDataSet.Tables["BusinessPartner"].Rows.Count == 0)
        {
            return null;
        }
        return businessPartnerDataSet.Tables["BusinessPartner"].Rows[0];
    }
    private static DataRow FindBusinessPartnerRowByUserName(string normalizedUserName, string transactionId)
    {
        DataSet businessPartnerDataSet = GetBusinessPartnerDataSet(
            GET_BUSINESS_PARTNER_BY_USER_NAME,
            "BusinessPartner_parUserName", normalizedUserName, transactionId);
        if (businessPartnerDataSet.Tables["BusinessPartner"].Rows.Count ==
            0)
        {
            return null;
        }
        return businessPartnerDataSet.Tables["BusinessPartner"].Rows[0];
    }
    private DataRow FindOrigamUserRowByUserName(string normalizedUserName, string transactionId)
    {
        if (string.IsNullOrEmpty(normalizedUserName))
        {
            return null;
        }
        DataSet origamUserDataSet = GetOrigamUserDataSet(
            GET_ORIGAM_USER_BY_USER_NAME,
            "OrigamUser_parUserName", normalizedUserName, transactionId);
        if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
        {
            return null;
        }
        return origamUserDataSet.Tables["OrigamUser"].Rows[0];
    }
    
    private static DataRow FindBusinessPartnerRowById(string userId)
    {
        DataSet businessPartnerDataSet = GetBusinessPartnerDataSet(
            GET_BUSINESS_PARTNER_BY_ID,
            "BusinessPartner_parId", userId);
        if (businessPartnerDataSet.Tables["BusinessPartner"].Rows.Count ==
            0)
        {
            return null;
        }
        return businessPartnerDataSet.Tables["BusinessPartner"].Rows[0];
    }
    private DataRow FindOrigamUserRowById(string userId)
    {
        DataSet origamUserDataSet = GetOrigamUserDataSet(
            GET_ORIGAM_USER_BY_BUSINESS_PARTNER_ID,
            "OrigamUser_parBusinessPartnerId", userId);
        if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
        {
            return null;
        }
        return origamUserDataSet.Tables["OrigamUser"].Rows[0];
    }
    
    public static DataSet GetOrigamUserDataSet(
        Guid methodId, string paramName, object paramValue)
    {
        return GetOrigamUserDataSet(methodId, paramName, paramValue, null);
    }
    public static DataSet GetOrigamUserDataSet(
        Guid methodId, string paramName, object paramValue, 
        string transactionId)
    {
        return DataService.Instance.LoadData(
            ORIGAM_USER_DATA_STRUCTURE,
            methodId,
            Guid.Empty,
            Guid.Empty,
            transactionId,
            paramName,
            paramValue);
    }
    
    private static DataSet GetBusinessPartnerDataSet(
        Guid methodId, string paramName, object paramValue)
    {
        return GetBusinessPartnerDataSet(
            methodId, paramName, paramValue, null);
    }
    private static DataSet GetBusinessPartnerDataSet(
        Guid methodId, string paramName, object paramValue,
        string transactionId)
    {
        return DataService.Instance.LoadData(
            BUSINESS_PARTNER_DATA_STRUCTURE,
            methodId,
            Guid.Empty,
            Guid.Empty,
            transactionId,
            paramName,
            paramValue);
    }
   
    // Gets the last DateTimeOffset a user's last lockout expired, if any. Any time in the past should be indicates a user is not locked out.
    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.iuserlockoutstore-1.getlockoutenddateasync?view=aspnetcore-3.1#Microsoft_AspNetCore_Identity_IUserLockoutStore_1_GetLockoutEndDateAsync__0_System_Threading_CancellationToken_
    public Task<DateTimeOffset?> GetLockoutEndDateAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        if (!user.LastLockoutDate.HasValue || 
            user.LastLockoutDate.Value == DateTime.MinValue)
        {
            return Task.FromResult<DateTimeOffset?>(null);
        }
        return Task.FromResult<DateTimeOffset?>(
            new DateTimeOffset(user.LastLockoutDate.Value));
    }
    public Task SetLockoutEndDateAsync(IOrigamUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
        if (lockoutEnd.HasValue)
        {
            user.LastLockoutDate =  lockoutEnd.Value.LocalDateTime;
        }
        else
        {
            user.LastLockoutDate = null;
        }
        return Task.CompletedTask;
    }
    public  Task<int> IncrementAccessFailedCountAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        user.FailedPasswordAttemptCount++; 
        return  Task.FromResult(user.FailedPasswordAttemptCount);
    }
    public Task ResetAccessFailedCountAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        user.FailedPasswordAttemptCount = 0;
        return Task.CompletedTask;
    }
    public Task<int> GetAccessFailedCountAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.FailedPasswordAttemptCount);
    }
    public Task<bool> GetLockoutEnabledAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
    public Task SetLockoutEnabledAsync(IOrigamUser user, bool enabled, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    public Task<string> GetPhoneNumberAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult("1");
    }
    public Task<bool> GetPhoneNumberConfirmedAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }
    public Task SetPhoneNumberAsync(IOrigamUser user, string phoneNumber, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    public Task SetPhoneNumberConfirmedAsync(IOrigamUser user, bool confirmed, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    //https://chsakell.com/2019/08/18/asp-net-core-identity-series-two-factor-authentication/
    public Task<string> GetAuthenticatorKeyAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult<string>(null);
    }
    public Task SetAuthenticatorKeyAsync(IOrigamUser user, string key, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    //https://stackoverflow.com/questions/19487322/what-is-asp-net-identitys-iusersecuritystampstoretuser-interface
    public Task<string> GetSecurityStampAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.SecurityStamp ?? "");
    }
    public Task SetSecurityStampAsync(IOrigamUser user, string stamp, CancellationToken cancellationToken)
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }
}
