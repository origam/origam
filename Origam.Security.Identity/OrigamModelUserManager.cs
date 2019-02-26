using System;
using System.Data;
using System.Threading.Tasks;
using System.Web.Security;
using BrockAllen.IdentityReboot;
using CSharpFunctionalExtensions;
using Origam;
using Origam.DA.Service;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;
using Microsoft.AspNet.Identity;
using Origam.Security.Common;

namespace Origam.Security.Identity
{
    public class OrigamModelUserManager : AbstractUserManager
    {
        private UserManager internalUserManager;
        public int MinimumPasswordLength { get; set; }
        public int NumberOfRequiredNonAlphanumericCharsInPassword { get; set; }
        public int NumberOfInvalidPasswordAttempts { get; set; }
        public bool UnlocksOnPasswordReset { get; set; }

        public OrigamModelUserManager(IUserStore<OrigamUser> store)
            : base(store)
        {
            PasswordHasher = new AdaptivePasswordHasherWithLegacySupport();
            MinimumPasswordLength = 12;
            NumberOfRequiredNonAlphanumericCharsInPassword = 6;
            NumberOfInvalidPasswordAttempts = 3;
            UnlocksOnPasswordReset = false;
            IsPasswordRecoverySupported = true;
            internalUserManager = new UserManager(
                userName => new OrigamUser(userName),
                NumberOfInvalidPasswordAttempts);
        }

        public static AbstractUserManager Create()
        {
            AbstractUserManager manager 
                = new OrigamModelUserManager(new OrigamModelUserStore());
            return manager;
        }

        public override Task<OrigamUser> FindAsync(
            string userName, string password)
        {
            Result<IOrigamUser> userResult = internalUserManager.Find(userName, password);
            if (userResult.IsFailure)
            {
                throw new Exception(userResult.Error);
            }
            return Task.FromResult((OrigamUser)userResult.Value);
        }
        
        override public Task<IdentityResult> CreateAsync(
            OrigamUser user, string password)
        {
            OrigamPasswordValidator passwordValidator
                = new OrigamPasswordValidator(
                    MinimumPasswordLength,
                    NumberOfRequiredNonAlphanumericCharsInPassword);
            Task<IdentityResult> validationTask 
                = passwordValidator.ValidateAsync(password);
            if (validationTask.IsFaulted)
            {
                throw validationTask.Exception;
            }
            if (!validationTask.Result.Succeeded)
            {
                return Task.FromResult(validationTask.Result);
            }
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
            origamUserRow.SetField("Id", Guid.NewGuid());
            origamUserRow.SetField("UserName", user.UserName);
            origamUserRow.SetField("refBusinessPartnerId", 
                user.ProviderUserKey);
            origamUserRow.SetField("Password", 
                PasswordHasher.HashPassword(password));
            origamUserRow.SetField("RecordCreated", DateTime.Now);
            origamUserRow.SetField("EmailConfirmed", user.IsApproved);
            origamUserRow["RecordCreatedBy"] 
                = SecurityManager.CurrentUserProfile().Id;
            origamUserDataSet.Tables["OrigamUser"].Rows.Add(origamUserRow);
            try
            {
                DataService.StoreData(Queries.ORIGAM_USER_DATA_STRUCTURE,
                    origamUserDataSet, false, user.TransactionId);
            } catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                {
                    log.ErrorFormat(
                        "Failed to create a user `{0}': {1}"
                        , user.UserName, ex.Message);
                }
                throw ex;
            }
            if (log.IsInfoEnabled)
            {
                log.DebugFormat(
                    "User `{0}' successfully "
                    + "created (emailConfirmed: {0})."
                    , user.UserName, user.IsApproved);
            }
            return Task.FromResult(IdentityResult.Success);
        }
        
        override public Task<string> RecoverPasswordAsync(string email)
        {
            DataSet businessPartnerDataSet = GetBusinessPartnerDataSet(
                GET_BUSINESS_PARTNER_BY_USER_EMAIL, 
                "BusinessPartner_parUserEmail", email);
            if (businessPartnerDataSet.Tables["BusinessPartner"].Rows.Count == 0) 
            {
                return Task.FromResult(Resources.EmailInvalid);
            }
			object userNameObj = businessPartnerDataSet.Tables["BusinessPartner"]
				.Rows[0]["UserName"];
            if (DBNull.Value.Equals(userNameObj))
			{
				return Task.FromResult(Resources.EmailInvalid);
			}
            string userName = (string)userNameObj;
            if (string.IsNullOrEmpty(userName))
            {
                return Task.FromResult(Resources.EmailInvalid);
            }            
            string resultMessage = "";
            SendPasswordResetToken(email, userName, 
                ref resultMessage);

            return Task.FromResult(resultMessage);
        }
		
        override public Task<bool> IsLockedOutAsync(string userId)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Finding out if the user {0} is locked out.", userId);
            }
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                Queries.GET_ORIGAM_USER_BY_BUSINESS_PARTNER_ID, 
                "OrigamUser_parBusinessPartnerId", userId);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("User not found...");
                }
                return Task.FromResult(false);
            }
            bool result = (bool)origamUserDataSet.Tables[
                "OrigamUser"].Rows[0]["IsLockedOut"];
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("User {0} is {1}", userId, (result)?"locked":"not locked");
            }
            return Task.FromResult(result);                
        }

        public override Task<bool> GetTwoFactorEnabledAsync(string userId)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Finding out if the user {0} has 2FA enforced.",
                    userId);
            }
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                Queries.GET_ORIGAM_USER_BY_BUSINESS_PARTNER_ID, 
                "OrigamUser_parBusinessPartnerId", userId);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("User not found...");
                }
                return Task.FromResult(false);
            }
            bool result = (bool)origamUserDataSet.Tables[
                "OrigamUser"].Rows[0]["Is2FAEnforced"];
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("User {0} {1}",
                    userId, (result) ? "has 2FA enforced." : "hasn't 2FA enforced.");
            }
            return Task.FromResult(result);
        }

        public override Task<IdentityResult> SetTwoFactorEnabledAsync(
            string userId, bool enabled)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Setting 2FA enforcement ({1}) for user {0}.",
                    userId, enabled);
            }
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                Queries.GET_ORIGAM_USER_BY_BUSINESS_PARTNER_ID, 
                "OrigamUser_parBusinessPartnerId", userId);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("User not found...");
                }
                return Task.FromResult(IdentityResult.Success);
            }
            DataRow origamUserRow 
                = origamUserDataSet.Tables["OrigamUser"].Rows[0];
            origamUserRow["Is2FAEnforced"] = enabled;
            origamUserRow["RecordUpdated"] = DateTime.Now;
            origamUserRow["RecordUpdatedBy"] 
                = SecurityManager.CurrentUserProfile().Id;
            DataService.StoreData(Queries.ORIGAM_USER_DATA_STRUCTURE, origamUserDataSet,
                false, null);
            return Task.FromResult(IdentityResult.Success);
        }
        
        override public Task<bool> IsEmailConfirmedAsync(string userId)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Finding out if the user {0} email is confirmed.", 
                    userId);
            }
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                Queries.GET_ORIGAM_USER_BY_BUSINESS_PARTNER_ID, 
                "OrigamUser_parBusinessPartnerId", userId);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("User not found...");
                }
                return Task.FromResult(true);
            }
            bool result = (bool)origamUserDataSet.Tables[
                "OrigamUser"].Rows[0]["EmailConfirmed"];
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("User's {0} email is {1}",
                    userId, (result) ? "confirmed" : "not confirmed");
            }
            return Task.FromResult(result);
        }
        
        override public Task<bool> UnlockUserAsync(string userName)
        {
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                Queries.GET_ORIGAM_USER_BY_USER_NAME, 
                "OrigamUser_parUserName", userName);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                // no access;
                return Task.FromResult(false);
            }
            DataRow origamUserRow 
                = origamUserDataSet.Tables["OrigamUser"].Rows[0];
            origamUserRow["IsLockedOut"] = false;
            origamUserRow["FailedPasswordAttemptCount"] = 0;
            origamUserRow["FailedPasswordAttemptWindowStart"] = DBNull.Value;
            origamUserRow["RecordUpdated"] = DateTime.Now;
            origamUserRow["RecordUpdatedBy"] 
                = SecurityManager.CurrentUserProfile().Id;
            DataService.StoreData(Queries.ORIGAM_USER_DATA_STRUCTURE, origamUserDataSet,
                false, null);
            bool result = SendUserUnlockingNotification(
                (string) origamUserRow["UserName"]);
            return Task.FromResult(result);
        }

        override public Task<IdentityResult> DeleteAsync(OrigamUser user)
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
            return Task.FromResult(IdentityResult.Success);
        }

        override public Task<IdentityResult> UpdateAsync(OrigamUser user)
        {
            // no need to do anything, email is in BusinessPartner
            return Task.FromResult(IdentityResult.Success);
        }

        override public Task<OrigamUser> FindByIdAsync(string userId)
        {
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                Queries.GET_ORIGAM_USER_BY_BUSINESS_PARTNER_ID, 
                "OrigamUser_parBusinessPartnerId", userId);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                // no access;
                return Task.FromResult<OrigamUser>(null);
            }
            return Task.FromResult(OrigamUserDataSetToOrigamUser(
                (string)origamUserDataSet.Tables["OrigamUser"]
                .Rows[0]["UserName"], origamUserDataSet));
        }

        override public Task<OrigamUser> FindByNameAsync(string userName)
        {
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                Queries.GET_ORIGAM_USER_BY_USER_NAME, 
                "OrigamUser_parUserName", userName);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                // no access;
                return Task.FromResult<OrigamUser>(null);
            }
            return Task.FromResult(
                OrigamUserDataSetToOrigamUser(userName, origamUserDataSet));
        }

        public override Task<string> GetSecurityStampAsync(string userId)
        {
            IDataLookupService _lookupService 
                = ServiceManager.Services.GetService(
                typeof(IDataLookupService)) as IDataLookupService;
            string securityStamp = "";
            object res = _lookupService.GetDisplayText(
                Queries.LOOKUP_ORIGAM_USER_SECURITY_STAMP_BY_BUSINESSPARTNER_ID,
                Guid.Parse(userId),
                false,
                false, // return message if null
                null);
            if (DBNull.Value.Equals(res))
            {
                // no access;
                return Task.FromResult<string>(null);
            }
            else
            {
                securityStamp = ((DateTime)res).ToString();
            }
            return Task.FromResult(securityStamp);
        }

        public override bool SupportsUserSecurityStamp
        {
            get
            {
                return true;
            }
        }

        override public Task<string> GetEmailAsync(string userId)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Getting email address for user {0}", userId);
            }
            DataSet businessPartnerDataSet = GetBusinessPartnerDataSet(
                GET_BUSINESS_PARTNER_BY_ID, 
                "BusinessPartner_parId", userId);
            if (businessPartnerDataSet.Tables["BusinessPartner"].Rows.Count 
            == 0) 
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("User not found...");
                }
                return Task.FromResult<string>(null);
            }
            string email = (string)businessPartnerDataSet.Tables[
                "BusinessPartner"].Rows[0]["UserEmail"];
            log.DebugFormat("Email address for user {0} is '{1}'.", userId, email);
            return Task.FromResult(email);
        }

        override public async Task<IdentityResult> ConfirmEmailAsync(
            string userId, string token)
        {
            OrigamUser user = await FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(Resources.UserIdNotFound);
            }
            if (!await VerifyUserTokenAsync(userId, "Confirmation", token))
            {
                return IdentityResult.Failed(Resources.TokenInvalid);
            }
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                Queries.GET_ORIGAM_USER_BY_USER_NAME, 
                "OrigamUser_parUserName", user.UserName);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                return IdentityResult.Failed(Resources.ErrorUserNotFound);
            }
            DataRow origamUserRow 
                = origamUserDataSet.Tables["OrigamUser"].Rows[0];
            origamUserRow["EmailConfirmed"] = true;
            origamUserRow["RecordUpdated"] = DateTime.Now;
            origamUserRow["RecordUpdatedBy"] 
                = SecurityManager.CurrentUserProfile().Id;
            DataService.StoreData(Queries.ORIGAM_USER_DATA_STRUCTURE, origamUserDataSet,
                false, null);
            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> ResetPasswordAsync(
            string userId, string token, string newPassword)
        {
            // get OrigamUser Dataset with row
            OrigamUser origamUser = await FindByIdAsync(userId);
            if (origamUser == null)
            {
                if (log.IsErrorEnabled)
                {
                    log.ErrorFormat(
                        "Reseting password for user {0}. User not found."
                        , userId);
                }
                // user not found;
                return IdentityResult.Failed(Resources.UserIdNotFound);
            }
            // verify password-reset token
            Task<bool> tokenVerificationTask =
                VerifyUserTokenAsync(userId, "ResetPassword", token);
            if (tokenVerificationTask.IsFaulted)
            {
                throw tokenVerificationTask.Exception;
            }
            else
            {
                if (!tokenVerificationTask.Result)
                {
                    if (log.IsInfoEnabled)
                    {
                        log.InfoFormat(
                            "Reseting password for user {0}: Invalid token `{1}'."
                            , userId, token);
                    }
                    return IdentityResult.Failed(
                        Resources.PasswordResetTokenInvalid);
                }
            }
            // validate password length, special chars, etc
            OrigamPasswordValidator passwordValidator
                = new OrigamPasswordValidator(
                    MinimumPasswordLength,
                    NumberOfRequiredNonAlphanumericCharsInPassword);
            Task<IdentityResult> validationTask
                = passwordValidator.ValidateAsync(newPassword);
            if (validationTask.IsFaulted)
            {
                throw validationTask.Exception;
            }
            if (!validationTask.Result.Succeeded)
            {
                return validationTask.Result;
            }
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                Queries.GET_ORIGAM_USER_BY_USER_NAME,
                "OrigamUser_parUserName", origamUser.UserName);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                return IdentityResult.Failed(Resources.ErrorUserNotFound);
            }
            DataRow origamUserRow
                = origamUserDataSet.Tables["OrigamUser"].Rows[0];
            // set the new password and save
            origamUserRow["Password"]
                = PasswordHasher.HashPassword(newPassword);
            origamUserRow["RecordUpdated"] = DateTime.Now;
            origamUserRow["RecordUpdatedBy"]
                = SecurityManager.CurrentUserProfile().Id;
            if (UnlocksOnPasswordReset)
            {
                origamUserRow["IsLockedOut"] = false;
                origamUserRow["FailedPasswordAttemptCount"] = 0;
                origamUserRow["FailedPasswordAttemptWindowStart"] = DBNull.Value;
            }
            try
            {
                DataService.StoreData(Queries.ORIGAM_USER_DATA_STRUCTURE,
                    origamUserDataSet, false, null);
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                {
                    log.ErrorFormat("Reseting password for user {0} failed: {1}",
                        userId, e.Message);
                }
                throw e;
            }
            if (log.IsDebugEnabled)
            {
                log.InfoFormat("Reseting password for user {0}: Success.",
                    userId);
            }
            return IdentityResult.Success;
        }

        public override Task<IdentityResult> ChangePasswordAsync(
            string userId, string currentPassword, string newPassword)
        {
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                Queries.GET_ORIGAM_USER_BY_BUSINESS_PARTNER_ID, 
                "OrigamUser_parBusinessPartnerId", userId);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                // no access;
                return Task.FromResult(
                    IdentityResult.Failed(Resources.UserIdNotFound));
            }
            DataRow origamUserRow 
                = origamUserDataSet.Tables["OrigamUser"].Rows[0];
            string hashedPassword = (string)origamUserRow["Password"];
            PasswordVerificationResult verificationResult 
                = PasswordHasher.VerifyHashedPassword(
                hashedPassword, currentPassword);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return Task.FromResult(
                    IdentityResult.Failed(Resources.OldPasswordIncorrect));
            }
            OrigamPasswordValidator passwordValidator
                = new OrigamPasswordValidator(
                    MinimumPasswordLength,
                    NumberOfRequiredNonAlphanumericCharsInPassword);
            Task<IdentityResult> validationTask 
                = passwordValidator.ValidateAsync(newPassword);
            if (validationTask.IsFaulted)
            {
                throw validationTask.Exception;
            }
            if (!validationTask.Result.Succeeded)
            {
                return Task.FromResult(validationTask.Result);
            }
            origamUserRow["Password"] 
                = PasswordHasher.HashPassword(newPassword);
            origamUserRow["RecordUpdated"] = DateTime.Now;
            origamUserRow["RecordUpdatedBy"] 
                = SecurityManager.CurrentUserProfile().Id;
            DataService.StoreData(Queries.ORIGAM_USER_DATA_STRUCTURE, 
                origamUserDataSet, false, null);
            return Task.FromResult(IdentityResult.Success);
        }
        
        override public async Task<IdentityResult> ConfirmEmailAsync(string userId)
        {
            OrigamUser user = await FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(Resources.UserIdNotFound);
            }            
            DataSet origamUserDataSet = GetOrigamUserDataSet(
                Queries.GET_ORIGAM_USER_BY_USER_NAME, 
                "OrigamUser_parUserName", user.UserName);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                return IdentityResult.Failed(Resources.ErrorUserNotFound);
            }
            DataRow origamUserRow 
                = origamUserDataSet.Tables["OrigamUser"].Rows[0];
            origamUserRow["EmailConfirmed"] = true;
            origamUserRow["RecordUpdated"] = DateTime.Now;
            origamUserRow["RecordUpdatedBy"] 
                = SecurityManager.CurrentUserProfile().Id;
            DataService.StoreData(Queries.ORIGAM_USER_DATA_STRUCTURE, origamUserDataSet,
                false, null);
            return IdentityResult.Success;
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

        private OrigamUser OrigamUserDataSetToOrigamUser(
            string userName, DataSet dataSet)
        {
            OrigamUser user = new OrigamUser(userName);
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
            return user;
        }

        protected override void CheckEmailUniqueness()
        {
            if (log.IsInfoEnabled)
            {
                log.Info("Checking user e-mail uniqueness...");
            }
            IPersistenceService persistenceService 
                = ServiceManager.Services.GetService(typeof(IPersistenceService)) 
                as IPersistenceService;
            DataEntityIndex uniqueEmailIndex 
                = persistenceService.SchemaProvider.RetrieveInstance(
                typeof(DataEntityIndex), 
                new ModelElementKey(
                    new Guid("83aae595-a7bb-46cc-9e1b-0a461a8003e3"))) 
                    as DataEntityIndex;
            if (uniqueEmailIndex == null)
            {
                throw new Exception(
                    "Definition for UniqueEmail index is missing in the model.");
            }
            OrigamSettings settings 
                = ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
            MsSqlDataService dataService = new MsSqlDataService(
                settings.DataConnectionString, 
                settings.DataBulkInsertThreshold, 
                settings.DataUpdateBatchSize);
            bool isIndexInDatabase 
                = dataService.IsSchemaItemInDatabase(uniqueEmailIndex);
            if (!isIndexInDatabase)
            {
                log.Info(
                    "UniqueEmail index not found in the database. Attempting setup...");
            }
            else
            {
                log.Info("UniqueEmail index is in place.");
            }
            emailUniquenessChecked = true;
        }
    }
}