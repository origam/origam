using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Security.Common;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore
{
    public class UserStore : IUserStore<User>, IUserEmailStore<User>,
        IUserTwoFactorStore<User>, IUserPasswordStore<User>,IUserLockoutStore<User>
    {

        private int accessFailedCount;

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            DatasetGenerator dataSetGenerator = new DatasetGenerator(true);
            IPersistenceService persistenceService = ServiceManager.Services
                .GetService(typeof(IPersistenceService)) as IPersistenceService;
            DataStructure dataStructure = (DataStructure)persistenceService
                .SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), 
                    new ModelElementKey(Queries.ORIGAM_USER_DATA_STRUCTURE));
            DataSet origamUserDataSet = dataSetGenerator.CreateDataSet(
                dataStructure);
            DataRow origamUserRow 
                = origamUserDataSet.Tables["OrigamUser"].NewRow();
            origamUserRow.SetField("Id",user.Id);
            origamUserRow.SetField("UserName", user.UserName);
            origamUserRow.SetField("refBusinessPartnerId", 
                user.ProviderUserKey);
            origamUserRow.SetField("Password",  user.PasswordHash);
            origamUserRow.SetField("RecordCreated", DateTime.Now);
            origamUserRow.SetField("EmailConfirmed", user.IsApproved);
            origamUserRow["RecordCreatedBy"]  = SecurityManager.CurrentUserProfile().Id;
            origamUserDataSet.Tables["OrigamUser"].Rows.Add(origamUserRow);
            try
            {
                DataService.StoreData(Queries.ORIGAM_USER_DATA_STRUCTURE,
                    origamUserDataSet, false, user.TransactionId);
            } catch (Exception ex)
            {
//                if (log.IsErrorEnabled)
//                {
//                    log.ErrorFormat(
//                        "Failed to create a user `{0}': {1}"
//                        , user.UserName, ex.Message);
//                }
                throw ex;
            }
            return IdentityResult.Success;
        }
     
        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                Queries.GET_ORIGAM_USER_BY_USER_NAME, 
                "OrigamUser_parUserName", user.UserName);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                throw new Exception(
                    "User " + user.UserName 
                            + " already doesn't have access to the system.");
            }
            origamUserDataSet.Tables["OrigamUser"].Rows[0].Delete();
            DataService.StoreData(Queries.ORIGAM_USER_DATA_STRUCTURE, origamUserDataSet, 
                false, user.TransactionId);
            return IdentityResult.Success;
        }
     
        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                Queries.GET_ORIGAM_USER_BY_BUSINESS_PARTNER_ID, 
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
     
        public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                Queries.GET_ORIGAM_USER_BY_USER_NAME, 
                "OrigamUser_parUserName", normalizedUserName);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                // no access;
                return null;
            }
            return OrigamUserDataSetToOrigamUser(normalizedUserName, origamUserDataSet);
        }
     
        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }
     
        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }
     
        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }
     
        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
     
        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.FromResult(0);
        }
     
        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            return IdentityResult.Success;
        }
     
        public Task SetEmailAsync(User user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.FromResult(0);
        }
     
        public Task<string> GetEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }
     
        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }
     
        public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }
     
        public async Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
     
        public Task<string> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }
     
        public Task SetNormalizedEmailAsync(User user, string normalizedEmail, CancellationToken cancellationToken)
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
     
        public Task SetTwoFactorEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
        {
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }
     
        public Task<bool> GetTwoFactorEnabledAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }
     
        public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }
     
        public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }
     
        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
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
                Queries.ORIGAM_USER_DATA_STRUCTURE,
                methodId,
                Guid.Empty,
                Guid.Empty,
                transactionId,
                paramName,
                paramValue);
        }

        private User OrigamUserDataSetToOrigamUser(
            string userName, DataSet dataSet)
        {
            User user = new User(userName);
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
            user.Id = user.ProviderUserKey.ToString();
            user.Is2FAEnforced = (bool)dataSet.Tables["OrigamUser"]
                .Rows[0]["Is2FAEnforced"];
            user.PasswordHash =(string)dataSet.Tables["OrigamUser"] 
                .Rows[0]["Password"];
            return user;
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetLockoutEndDateAsync(User user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> IncrementAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(accessFailedCount++);
        }

        public Task ResetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(accessFailedCount=0);
        }

        public Task<int> GetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(accessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(User user, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() => false);
        }

        public Task SetLockoutEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}