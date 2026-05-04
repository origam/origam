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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Origam.Security.Common;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Authorization;

public class CoreUserManager<TUser> : UserManager<IOrigamUser>
{
    private readonly IStringLocalizer<SharedResources> localizer;
    private readonly UserStore userStore;

    public CoreUserManager(
        IUserStore<IOrigamUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<IOrigamUser> passwordHasher,
        IEnumerable<IUserValidator<IOrigamUser>> userValidators,
        IEnumerable<IPasswordValidator<IOrigamUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<IOrigamUser>> logger,
        IStringLocalizer<SharedResources> localizer
    )
        : base(
            store: store,
            optionsAccessor: optionsAccessor,
            passwordHasher: passwordHasher,
            userValidators: userValidators,
            passwordValidators: passwordValidators,
            keyNormalizer: keyNormalizer,
            errors: errors,
            services: services,
            logger: logger
        )
    {
        this.localizer = localizer;
        this.userStore = store as UserStore;
    }
    // invoked, when e-mail is changed (comes also with EmailConfirmed change)
    // since we're going to change only OrigamUser - Only EmailConfirmed is
    // relevant and this info comes in IsApproved
#pragma warning disable 1998
    public override async Task<IdentityResult> UpdateAsync(IOrigamUser user)
#pragma warning restore 1998
    {
        var origamUserDataSet = UserStore.GetOrigamUserDataSet(
            methodId: UserStore.GET_ORIGAM_USER_BY_USER_NAME,
            paramName: "OrigamUser_parUserName",
            paramValue: user.UserName,
            transactionId: user.TransactionId
        );
        if (origamUserDataSet.Tables[name: "OrigamUser"].Rows.Count == 0)
        {
            return IdentityResult.Failed(
                errors: new IdentityError
                {
                    Code = "Error",
                    Description = localizer[name: "ErrorUserNotFound"].ToString(),
                }
            );
        }
        var origamUserRow = origamUserDataSet.Tables[name: "OrigamUser"].Rows[index: 0];
        origamUserRow[columnName: "EmailConfirmed"] = user.IsApproved;
        origamUserRow[columnName: "RecordUpdated"] = DateTime.Now;
        origamUserRow[columnName: "RecordUpdatedBy"] = SecurityManager.CurrentUserProfile().Id;
        DataService.Instance.StoreData(
            dataStructureId: UserStore.ORIGAM_USER_DATA_STRUCTURE,
            data: origamUserDataSet,
            loadActualValuesAfterUpdate: false,
            transactionId: user.TransactionId
        );
        return IdentityResult.Success;
    }

    public Task<IOrigamUser> FindByNameAsync(string name, string transactionId)
    {
        return userStore.FindByNameAsync(
            normalizedUserName: name,
            transactionId: transactionId,
            cancellationToken: CancellationToken
        );
    }
}
