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
using Origam.ServerCore.Authorization;
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
            DataRow origamUserRow = FindOrigamUserRowByUserName(user.UserName);
            if (origamUserRow == null)
            {
                throw new Exception($"User {user.UserName} already doesn't have access to the system.");
            }
            origamUserRow.Delete();
            DataService.StoreData(
                ModelItems.ORIGAM_USER_DATA_STRUCTURE,
                origamUserRow.Table.DataSet, 
                false, 
                user.TransactionId);
            return IdentityResult.Success;
        }
     
        public async Task<IOrigamUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            DataRow origamUserRow = FindOrigamUserRowById(userId);
            if (origamUserRow == null) return null;

            DataRow businessPartnerRow = FindBusinessPartnerRowById(userId);
            if (businessPartnerRow == null) return null;

            return UserTools.Create(
                origamUserRow: origamUserRow, 
                businessPartnerRow: businessPartnerRow);
        }

        public async Task<IOrigamUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var origamUserRow = FindOrigamUserRowByUserName(normalizedUserName);
            if (origamUserRow == null) return null;


            var businessPartnerRow = FindBusinessPartnerRowByUserName(normalizedUserName);
            if (businessPartnerRow == null) return null;

            return UserTools.Create(
                origamUserRow: origamUserRow, 
                businessPartnerRow: businessPartnerRow);
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
            DataRow origamUserRow = FindOrigamUserRowByUserName(user.UserName);
            if(origamUserRow == null)
            {
                return IdentityResult.Failed( 
                    new IdentityError{Description = Resources.ErrorUserNotFound});
            }
            UserTools.UpdateOrigamUserRow(user, origamUserRow);
            DataService.StoreData(ModelItems.ORIGAM_USER_DATA_STRUCTURE,
                origamUserRow.Table.DataSet,
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
            DataRow businessPartnerRow = FindBusinessPartnerRowByEmail(normalizedEmail);
            if (businessPartnerRow == null) return null;

            string userName = (string)businessPartnerRow["UserName"];
            DataRow origamUserRow = FindOrigamUserRowByUserName(userName);
            if (origamUserRow == null) return null;
            
            return UserTools.Create(
                origamUserRow: origamUserRow, 
                businessPartnerRow: businessPartnerRow); 
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
        
        private static DataRow FindBusinessPartnerRowByEmail(string email)
        {
            DataSet businessPartnerDataSet = GetBusinessPartnerDataSet(
                ModelItems.GET_BUSINESS_PARTNER_BY_USER_EMAIL,
                "BusinessPartner_parUserEmail", email);
            if (businessPartnerDataSet.Tables["BusinessPartner"].Rows.Count == 0)
            {
                return null;
            }
            return businessPartnerDataSet.Tables["BusinessPartner"].Rows[0];
        }

        private static DataRow FindBusinessPartnerRowByUserName(string normalizedUserName)
        {
            DataSet businessPartnerDataSet = GetBusinessPartnerDataSet(
                ModelItems.GET_BUSINESS_PARTNER_BY_USER_NAME,
                "BusinessPartner_parUserName", normalizedUserName);
            if (businessPartnerDataSet.Tables["BusinessPartner"].Rows.Count ==
                0)
            {
                return null;
            }
            return businessPartnerDataSet.Tables["BusinessPartner"].Rows[0];
        }

        private DataRow FindOrigamUserRowByUserName(string normalizedUserName)
        {
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                ModelItems.GET_ORIGAM_USER_BY_USER_NAME,
                "OrigamUser_parUserName", normalizedUserName);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                return null;
            }
            return origamUserDataSet.Tables["OrigamUser"].Rows[0];
        }
        
        private static DataRow FindBusinessPartnerRowById(string userId)
        {
            DataSet businessPartnerDataSet = GetBusinessPartnerDataSet(
                ModelItems.GET_BUSINESS_PARTNER_BY_ID,
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
                ModelItems.GET_ORIGAM_USER_BY_BUSINESS_PARTNER_ID,
                "OrigamUser_parBusinessPartnerId", userId);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                return null;
            }
            return origamUserDataSet.Tables["OrigamUser"].Rows[0];
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
        
        public static DataSet GetBusinessPartnerDataSet(
            Guid methodId, string paramName, object paramValue)
        {
            return GetBusinessPartnerDataSet(
                methodId, paramName, paramValue, null);
        }
          
        internal static DataSet GetBusinessPartnerDataSet(
            Guid methodId, string paramName, object paramValue,
            string transactionId)
        {
            return DataService.LoadData(
                ModelItems.BUSINESS_PARTNER_DATA_STRUCTURE,
                methodId,
                Guid.Empty,
                Guid.Empty,
                transactionId,
                paramName,
                paramValue);
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