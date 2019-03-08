using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Security.Common;
using Origam.Security.Identity;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore
{
    public class UserStore : IUserStore<IOrigamUser>, IUserEmailStore<IOrigamUser>,
        IUserTwoFactorStore<IOrigamUser>, IUserPasswordStore<IOrigamUser>,IUserLockoutStore<IOrigamUser>
    {

        private int accessFailedCount;

        public Task<IdentityResult> CreateAsync(IOrigamUser user, CancellationToken cancellationToken)
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
            parameters.Add(new QueryParameter("Password", user.PasswordHash));
            parameters.Add(new QueryParameter("FirstName", user.FirstName));
            parameters.Add(new QueryParameter("Name", user.Name));
            parameters.Add(new QueryParameter("Email", user.Email));
            parameters.Add(new QueryParameter("RoleId", user.RoleId));
            parameters.Add(new QueryParameter("RequestEmailConfirmation",
                !user.EmailConfirmed));
             // Will create new line in BusinessPartner and OrigamUser
            WorkflowService.ExecuteWorkflow(
                ModelItems.CREATE_USER_WORKFLOW, parameters, null);

            return Task.FromResult(IdentityResult.Success);
        }
     
        public async Task<IdentityResult> DeleteAsync(IOrigamUser user, CancellationToken cancellationToken)
        {
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                ModelItems.GET_ORIGAM_USER_BY_USER_NAME, 
                "OrigamUser_parUserName", user.UserName);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                throw new Exception(
                    "User " + user.UserName 
                            + " already doesn't have access to the system.");
            }
            origamUserDataSet.Tables["OrigamUser"].Rows[0].Delete();
            DataService.StoreData(ModelItems.ORIGAM_USER_DATA_STRUCTURE, origamUserDataSet, 
                false, user.TransactionId);
            return IdentityResult.Success;
        }
     
        public async Task<IOrigamUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                ModelItems.GET_ORIGAM_USER_BY_BUSINESS_PARTNER_ID, 
                "OrigamUser_parBusinessPartnerId", userId);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                // no access;
                return null;
            }
            return 
                OrigamUserDataSetToOrigamUser(
                (string)origamUserDataSet.Tables["OrigamUser"]
                    .Rows[0]["UserName"], origamUserDataSet);
        }
     
        public async Task<IOrigamUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                ModelItems.GET_ORIGAM_USER_BY_USER_NAME, 
                "OrigamUser_parUserName", normalizedUserName);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                // no access;
                return null;
            }
            return OrigamUserDataSetToOrigamUser(normalizedUserName, origamUserDataSet);
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
            return Task.FromResult(0);
        }
     
        public Task SetUserNameAsync(IOrigamUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.FromResult(0);
        }
     
        public async Task<IdentityResult> UpdateAsync(IOrigamUser user, CancellationToken cancellationToken)
        {
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                ModelItems.GET_ORIGAM_USER_BY_USER_NAME, 
                "OrigamUser_parUserName", user.UserName);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                return IdentityResult.Failed( 
                    new IdentityError{Description = Resources.ErrorUserNotFound});
            }
            DataRow origamUserRow 
                = origamUserDataSet.Tables["OrigamUser"].Rows[0];
            origamUserRow["EmailConfirmed"] = user.EmailConfirmed;
            origamUserRow["RecordUpdated"] = DateTime.Now;
            origamUserRow["RecordUpdatedBy"] 
                = SecurityManager.CurrentUserProfile().Id;
            DataService.StoreData(ModelItems.ORIGAM_USER_DATA_STRUCTURE, origamUserDataSet,
                false, null);
            return IdentityResult.Success;
        }
     
        public Task SetEmailAsync(IOrigamUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.FromResult(0);
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
            return Task.FromResult(0);
        }
     
        public async Task<IOrigamUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
     
        public Task<string> GetNormalizedEmailAsync(IOrigamUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }
     
        public Task SetNormalizedEmailAsync(IOrigamUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
     
//    public Task SetPhoneNumberAsync(User user, string phoneNumber, CancellationToken cancellationToken)
//    {
//        user.PhoneNumber = phoneNumber;
//        return Task.FromResult(0);
//    }
// 
//    public Task<string> GetPhoneNumberAsync(User user, CancellationToken cancellationToken)
//    {
//        return Task.FromResult(user.PhoneNumber);
//    }
// 
//    public Task<bool> GetPhoneNumberConfirmedAsync(User user, CancellationToken cancellationToken)
//    {
//        return Task.FromResult(user.PhoneNumberConfirmed);
//    }
// 
//    public Task SetPhoneNumberConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
//    {
//        user.PhoneNumberConfirmed = confirmed;
//        return Task.FromResult(0);
//    }
     
        public Task SetTwoFactorEnabledAsync(IOrigamUser user, bool enabled, CancellationToken cancellationToken)
        {
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }
     
        public Task<bool> GetTwoFactorEnabledAsync(IOrigamUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }
     
        public Task SetPasswordHashAsync(IOrigamUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
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
        
        private DataSet GetOrigamUserDataSet(
            Guid methodId, string paramName, object paramValue)
        {
            return GetOrigamUserDataSet(methodId, paramName, paramValue, null);
        }

        private DataSet GetOrigamUserDataSet(
            Guid methodId, string paramName, object paramValue, 
            string transactionId)
        {
            return DataService.LoadData(
                ModelItems.ORIGAM_USER_DATA_STRUCTURE,
                methodId,
                Guid.Empty,
                Guid.Empty,
                transactionId,
                paramName,
                paramValue);
        }

        private IOrigamUser OrigamUserDataSetToOrigamUser(
            string userName, DataSet dataSet)
        {
            IOrigamUser user = new User(userName);
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
            user.ProviderUserKey = (Guid)dataSet.Tables["OrigamUser"]
                .Rows[0]["refBusinessPartnerId"];
            user.BusinessPartnerId = user.ProviderUserKey.ToString();
            user.Is2FAEnforced = (bool)dataSet.Tables["OrigamUser"]
                .Rows[0]["Is2FAEnforced"];
            user.PasswordHash =(string)dataSet.Tables["OrigamUser"] 
                .Rows[0]["Password"];
            return user;
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(IOrigamUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetLockoutEndDateAsync(IOrigamUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            if (!lockoutEnd.HasValue)
            {
                throw new NotImplementedException();
            }
            if (lockoutEnd.Value < DateTimeOffset.Now)
            {
                user.IsLockedOut = false;
                return Task.CompletedTask;
            }
            throw new NotImplementedException();
        }

        public Task<int> IncrementAccessFailedCountAsync(IOrigamUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(accessFailedCount++);
        }

        public Task ResetAccessFailedCountAsync(IOrigamUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(accessFailedCount=0);
        }

        public Task<int> GetAccessFailedCountAsync(IOrigamUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(accessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(IOrigamUser user, CancellationToken cancellationToken)
        {
           
            return Task.Factory.StartNew(() =>  user.IsLockedOut);
        }

        public Task SetLockoutEnabledAsync(IOrigamUser user, bool enabled, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>  user.IsLockedOut = enabled);
        }
    }
}