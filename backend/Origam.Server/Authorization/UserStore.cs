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

public sealed class UserStore
    : IUserStore<IOrigamUser>,
        IUserEmailStore<IOrigamUser>,
        IUserTwoFactorStore<IOrigamUser>,
        IUserPasswordStore<IOrigamUser>,
        IUserLockoutStore<IOrigamUser>,
        IUserPhoneNumberStore<IOrigamUser>,
        IUserAuthenticatorKeyStore<IOrigamUser>,
        IUserSecurityStampStore<IOrigamUser>
{
    public static readonly Guid ORIGAM_USER_DATA_STRUCTURE = new Guid(
        g: "43b67a51-68f3-4696-b08d-de46ae0223ce"
    );
    public static readonly Guid GET_ORIGAM_USER_BY_BUSINESS_PARTNER_ID = new Guid(
        g: "982f45a9-b610-4e2f-8d7f-2b1eebe93390"
    );
    public static readonly Guid GET_ORIGAM_USER_BY_USER_NAME = new Guid(
        g: "a60c9817-ae18-465c-a91f-d4b8a25f15a4"
    );
    public static readonly Guid BUSINESS_PARTNER_DATA_STRUCTURE = new Guid(
        g: "f4c92dce-d634-4179-adb4-98876b870cc7"
    );
    public static readonly Guid GET_BUSINESS_PARTNER_BY_USER_NAME = new Guid(
        g: "545396e7-d88e-4315-a112-f8feda7229bf"
    );
    public static readonly Guid GET_BUSINESS_PARTNER_BY_ID = new Guid(
        g: "4e46424b-349f-4314-bc75-424206cd35b0"
    );
    public static readonly Guid GET_BUSINESS_PARTNER_BY_USER_EMAIL = new Guid(
        g: "46fd2484-4506-45a2-8a96-7855ea116210"
    );

    private readonly IStringLocalizer<SharedResources> localizer;

    public UserStore(IStringLocalizer<SharedResources> localizer)
    {
        this.localizer = localizer;
    }

    public Task<IdentityResult> CreateAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        DatasetGenerator dataSetGenerator = new DatasetGenerator(userDefinedParameters: true);
        IPersistenceService persistenceService =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        DataStructure dataStructure = (DataStructure)
            persistenceService.SchemaProvider.RetrieveInstance(
                type: typeof(ISchemaItem),
                primaryKey: new ModelElementKey(id: ORIGAM_USER_DATA_STRUCTURE)
            );
        DataSet origamUserDataSet = dataSetGenerator.CreateDataSet(ds: dataStructure);
        DataRow origamUserRow = origamUserDataSet.Tables[name: "OrigamUser"].NewRow();
        UserTools.AddToOrigamUserRow(user: user, origamUserRow: origamUserRow);
        origamUserDataSet.Tables[name: "OrigamUser"].Rows.Add(row: origamUserRow);
        DataService.Instance.StoreData(
            dataStructureId: ORIGAM_USER_DATA_STRUCTURE,
            data: origamUserDataSet,
            loadActualValuesAfterUpdate: false,
            transactionId: user.TransactionId
        );
        return Task.FromResult(result: IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        DataRow origamUserRow = FindOrigamUserRowByUserName(
            normalizedUserName: user.UserName,
            transactionId: user.TransactionId
        );
        if (origamUserRow == null)
        {
            throw new Exception(
                message: $"User {user.UserName} already doesn't have access to the system."
            );
        }
        origamUserRow.Delete();
        DataService.Instance.StoreData(
            dataStructureId: ORIGAM_USER_DATA_STRUCTURE,
            data: origamUserRow.Table.DataSet,
            loadActualValuesAfterUpdate: false,
            transactionId: user.TransactionId
        );
        return Task.FromResult(result: IdentityResult.Success);
    }

    public Task<IOrigamUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        DataRow origamUserRow = FindOrigamUserRowById(userId: userId);
        if (origamUserRow == null)
        {
            return Task.FromResult<IOrigamUser>(result: null);
        }

        DataRow businessPartnerRow = FindBusinessPartnerRowById(userId: userId);
        if (businessPartnerRow == null)
        {
            return Task.FromResult<IOrigamUser>(result: null);
        }

        return Task.FromResult(
            result: UserTools.Create(
                origamUserRow: origamUserRow,
                businessPartnerRow: businessPartnerRow
            )
        );
    }

    public Task<IOrigamUser> FindByNameAsync(
        string normalizedUserName,
        CancellationToken cancellationToken
    )
    {
        return FindByNameAsync(
            normalizedUserName: normalizedUserName,
            transactionId: null,
            cancellationToken: cancellationToken
        );
    }

    public Task<IOrigamUser> FindByNameAsync(
        string normalizedUserName,
        string transactionId,
        CancellationToken cancellationToken
    )
    {
        var origamUserRow = FindOrigamUserRowByUserName(
            normalizedUserName: normalizedUserName,
            transactionId: transactionId
        );
        if (origamUserRow == null)
        {
            return Task.FromResult<IOrigamUser>(result: null);
        }

        var businessPartnerRow = FindBusinessPartnerRowByUserName(
            normalizedUserName: normalizedUserName,
            transactionId: transactionId
        );
        if (businessPartnerRow == null)
        {
            return Task.FromResult<IOrigamUser>(result: null);
        }

        return Task.FromResult(
            result: UserTools.Create(
                origamUserRow: origamUserRow,
                businessPartnerRow: businessPartnerRow
            )
        );
    }

    public Task<string> GetNormalizedUserNameAsync(
        IOrigamUser user,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(result: user.NormalizedUserName);
    }

    public Task<string> GetUserIdAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(result: user.BusinessPartnerId);
    }

    public Task<string> GetUserNameAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(result: user.UserName);
    }

    public Task SetNormalizedUserNameAsync(
        IOrigamUser user,
        string normalizedName,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(
        IOrigamUser user,
        string userName,
        CancellationToken cancellationToken
    )
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> UpdateAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        DataRow origamUserRow = FindOrigamUserRowByUserName(
            normalizedUserName: user.UserName,
            transactionId: user.TransactionId
        );
        if (origamUserRow == null)
        {
            return Task.FromResult(
                result: IdentityResult.Failed(
                    errors: new IdentityError { Description = localizer[name: "ErrorUserNotFound"] }
                )
            );
        }
        UserTools.UpdateOrigamUserRow(user: user, origamUserRow: origamUserRow);
        DataService.Instance.StoreData(
            dataStructureId: ORIGAM_USER_DATA_STRUCTURE,
            data: origamUserRow.Table.DataSet,
            loadActualValuesAfterUpdate: false,
            transactionId: null
        );
        return Task.FromResult(result: IdentityResult.Success);
    }

    public Task SetEmailAsync(IOrigamUser user, string email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string> GetEmailAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(result: user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(result: user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(
        IOrigamUser user,
        bool confirmed,
        CancellationToken cancellationToken
    )
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task<IOrigamUser> FindByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken
    )
    {
        DataRow businessPartnerRow = FindBusinessPartnerRowByEmail(email: normalizedEmail);
        if (businessPartnerRow == null)
        {
            return Task.FromResult<IOrigamUser>(result: null);
        }

        string userName =
            businessPartnerRow[columnName: "UserName"] == DBNull.Value
                ? null
                : (string)businessPartnerRow[columnName: "UserName"];
        DataRow origamUserRow = FindOrigamUserRowByUserName(
            normalizedUserName: userName,
            transactionId: null
        );
        if (origamUserRow == null)
        {
            return Task.FromResult<IOrigamUser>(result: null);
        }

        return Task.FromResult(
            result: UserTools.Create(
                origamUserRow: origamUserRow,
                businessPartnerRow: businessPartnerRow
            )
        );
    }

    public Task<string> GetNormalizedEmailAsync(
        IOrigamUser user,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(result: user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(
        IOrigamUser user,
        string normalizedEmail,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }

    public Task SetTwoFactorEnabledAsync(
        IOrigamUser user,
        bool enabled,
        CancellationToken cancellationToken
    )
    {
        user.TwoFactorEnabled = enabled;
        return Task.CompletedTask;
    }

    public Task<bool> GetTwoFactorEnabledAsync(
        IOrigamUser user,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(result: user.TwoFactorEnabled);
    }

    public Task SetPasswordHashAsync(
        IOrigamUser user,
        string passwordHash,
        CancellationToken cancellationToken
    )
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string> GetPasswordHashAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(result: user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(result: user.PasswordHash != null);
    }

    public void Dispose()
    {
        // Nothing to dispose.
    }

    private static DataRow FindBusinessPartnerRowByEmail(string email)
    {
        DataSet businessPartnerDataSet = GetBusinessPartnerDataSet(
            methodId: GET_BUSINESS_PARTNER_BY_USER_EMAIL,
            paramName: "BusinessPartner_parUserEmail",
            paramValue: email
        );
        if (businessPartnerDataSet.Tables[name: "BusinessPartner"].Rows.Count == 0)
        {
            return null;
        }
        return businessPartnerDataSet.Tables[name: "BusinessPartner"].Rows[index: 0];
    }

    private static DataRow FindBusinessPartnerRowByUserName(
        string normalizedUserName,
        string transactionId
    )
    {
        DataSet businessPartnerDataSet = GetBusinessPartnerDataSet(
            methodId: GET_BUSINESS_PARTNER_BY_USER_NAME,
            paramName: "BusinessPartner_parUserName",
            paramValue: normalizedUserName,
            transactionId: transactionId
        );
        if (businessPartnerDataSet.Tables[name: "BusinessPartner"].Rows.Count == 0)
        {
            return null;
        }
        return businessPartnerDataSet.Tables[name: "BusinessPartner"].Rows[index: 0];
    }

    private DataRow FindOrigamUserRowByUserName(string normalizedUserName, string transactionId)
    {
        if (string.IsNullOrEmpty(value: normalizedUserName))
        {
            return null;
        }
        DataSet origamUserDataSet = GetOrigamUserDataSet(
            methodId: GET_ORIGAM_USER_BY_USER_NAME,
            paramName: "OrigamUser_parUserName",
            paramValue: normalizedUserName,
            transactionId: transactionId
        );
        if (origamUserDataSet.Tables[name: "OrigamUser"].Rows.Count == 0)
        {
            return null;
        }
        return origamUserDataSet.Tables[name: "OrigamUser"].Rows[index: 0];
    }

    private static DataRow FindBusinessPartnerRowById(string userId)
    {
        DataSet businessPartnerDataSet = GetBusinessPartnerDataSet(
            methodId: GET_BUSINESS_PARTNER_BY_ID,
            paramName: "BusinessPartner_parId",
            paramValue: userId
        );
        if (businessPartnerDataSet.Tables[name: "BusinessPartner"].Rows.Count == 0)
        {
            return null;
        }
        return businessPartnerDataSet.Tables[name: "BusinessPartner"].Rows[index: 0];
    }

    private DataRow FindOrigamUserRowById(string userId)
    {
        DataSet origamUserDataSet = GetOrigamUserDataSet(
            methodId: GET_ORIGAM_USER_BY_BUSINESS_PARTNER_ID,
            paramName: "OrigamUser_parBusinessPartnerId",
            paramValue: userId
        );
        if (origamUserDataSet.Tables[name: "OrigamUser"].Rows.Count == 0)
        {
            return null;
        }
        return origamUserDataSet.Tables[name: "OrigamUser"].Rows[index: 0];
    }

    public static DataSet GetOrigamUserDataSet(Guid methodId, string paramName, object paramValue)
    {
        return GetOrigamUserDataSet(
            methodId: methodId,
            paramName: paramName,
            paramValue: paramValue,
            transactionId: null
        );
    }

    public static DataSet GetOrigamUserDataSet(
        Guid methodId,
        string paramName,
        object paramValue,
        string transactionId
    )
    {
        return DataService.Instance.LoadData(
            dataStructureId: ORIGAM_USER_DATA_STRUCTURE,
            methodId: methodId,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: transactionId,
            paramName1: paramName,
            paramValue1: paramValue
        );
    }

    private static DataSet GetBusinessPartnerDataSet(
        Guid methodId,
        string paramName,
        object paramValue
    )
    {
        return GetBusinessPartnerDataSet(
            methodId: methodId,
            paramName: paramName,
            paramValue: paramValue,
            transactionId: null
        );
    }

    private static DataSet GetBusinessPartnerDataSet(
        Guid methodId,
        string paramName,
        object paramValue,
        string transactionId
    )
    {
        return DataService.Instance.LoadData(
            dataStructureId: BUSINESS_PARTNER_DATA_STRUCTURE,
            methodId: methodId,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: transactionId,
            paramName1: paramName,
            paramValue1: paramValue
        );
    }

    // Gets the last DateTimeOffset a user's last lockout expired, if any. Any time in the past should be indicates a user is not locked out.
    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.iuserlockoutstore-1.getlockoutenddateasync?view=aspnetcore-3.1#Microsoft_AspNetCore_Identity_IUserLockoutStore_1_GetLockoutEndDateAsync__0_System_Threading_CancellationToken_
    public Task<DateTimeOffset?> GetLockoutEndDateAsync(
        IOrigamUser user,
        CancellationToken cancellationToken
    )
    {
        if (!user.LastLockoutDate.HasValue || user.LastLockoutDate.Value == DateTime.MinValue)
        {
            return Task.FromResult<DateTimeOffset?>(result: null);
        }
        return Task.FromResult<DateTimeOffset?>(
            result: new DateTimeOffset(dateTime: user.LastLockoutDate.Value)
        );
    }

    public Task SetLockoutEndDateAsync(
        IOrigamUser user,
        DateTimeOffset? lockoutEnd,
        CancellationToken cancellationToken
    )
    {
        if (lockoutEnd.HasValue)
        {
            user.LastLockoutDate = lockoutEnd.Value.LocalDateTime;
        }
        else
        {
            user.LastLockoutDate = null;
        }
        return Task.CompletedTask;
    }

    public Task<int> IncrementAccessFailedCountAsync(
        IOrigamUser user,
        CancellationToken cancellationToken
    )
    {
        user.FailedPasswordAttemptCount++;
        return Task.FromResult(result: user.FailedPasswordAttemptCount);
    }

    public Task ResetAccessFailedCountAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        user.FailedPasswordAttemptCount = 0;
        return Task.CompletedTask;
    }

    public Task<int> GetAccessFailedCountAsync(
        IOrigamUser user,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(result: user.FailedPasswordAttemptCount);
    }

    public Task<bool> GetLockoutEnabledAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(result: true);
    }

    public Task SetLockoutEnabledAsync(
        IOrigamUser user,
        bool enabled,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }

    public Task<string> GetPhoneNumberAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(result: "1");
    }

    public Task<bool> GetPhoneNumberConfirmedAsync(
        IOrigamUser user,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(result: false);
    }

    public Task SetPhoneNumberAsync(
        IOrigamUser user,
        string phoneNumber,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }

    public Task SetPhoneNumberConfirmedAsync(
        IOrigamUser user,
        bool confirmed,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }

    //https://chsakell.com/2019/08/18/asp-net-core-identity-series-two-factor-authentication/
    public Task<string> GetAuthenticatorKeyAsync(
        IOrigamUser user,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult<string>(result: null);
    }

    public Task SetAuthenticatorKeyAsync(
        IOrigamUser user,
        string key,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }

    //https://stackoverflow.com/questions/19487322/what-is-asp-net-identitys-iusersecuritystampstoretuser-interface
    public Task<string> GetSecurityStampAsync(IOrigamUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(result: user.SecurityStamp ?? "");
    }

    public Task SetSecurityStampAsync(
        IOrigamUser user,
        string stamp,
        CancellationToken cancellationToken
    )
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }
}
