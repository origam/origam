using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Origam.Security.Common;
using Origam.Security.Identity;
using Origam.ServerCore.Resources;
using Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore.Authorization
{
    public class CoreUserManager : UserManager<IOrigamUser>
    {
        private readonly IStringLocalizer<SharedResources> localizer;

        public CoreUserManager(
            IUserStore<IOrigamUser> store, 
            IOptions<IdentityOptions> optionsAccessor, 
            IPasswordHasher<IOrigamUser> passwordHasher, 
            IEnumerable<IUserValidator<IOrigamUser>> userValidators, 
            IEnumerable<IPasswordValidator<IOrigamUser>> passwordValidators, 
            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, 
            IServiceProvider services, 
            ILogger<UserManager<IOrigamUser>> logger,
            IStringLocalizer<SharedResources> localizer) : 
            base(store, optionsAccessor, passwordHasher, userValidators, 
                passwordValidators, keyNormalizer, errors, services, logger)
        {
            this.localizer = localizer;
        }

        // invoked, when e-mail is changed (comes also with EmailConfirmed change)
        // since we're going to change only OrigamUser - Only EmailConfirmed is
        // relevant and this info comes in IsApproved
        public override async Task<IdentityResult> UpdateAsync(IOrigamUser user)
        {
            var origamUserDataSet = UserStore.GetOrigamUserDataSet(
                UserStore.GET_ORIGAM_USER_BY_USER_NAME,
                "OrigamUser_parUserName", user.UserName, user.TransactionId);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "Error", 
                    Description = localizer["ErrorUserNotFound"].ToString()
                });
            }
            var origamUserRow = origamUserDataSet.Tables["OrigamUser"].Rows[0];
            origamUserRow["EmailConfirmed"] = user.IsApproved;
            origamUserRow["RecordUpdated"] = DateTime.Now;
            origamUserRow["RecordUpdatedBy"] 
                = SecurityManager.CurrentUserProfile().Id;
            DataService.StoreData(UserStore.ORIGAM_USER_DATA_STRUCTURE, 
                origamUserDataSet, false, 
                user.TransactionId);
            return IdentityResult.Success;
        }
    }
}
