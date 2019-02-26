using System;
using System.Data;
using System.Threading.Tasks;
using BrockAllen.IdentityReboot;
using CSharpFunctionalExtensions;
using log4net;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Security.Common
{
    public class UserManager
    {
        protected static readonly ILog log 
            = LogManager.GetLogger(typeof(UserManager));

        private readonly Func<string, IOrigamUser> userFactory;
        private readonly InternalPasswordHasherWithLegacySupport internalHasher =
            new InternalPasswordHasherWithLegacySupport();

        public int numberOfInvalidPasswordAttempts { get;}
        
        public UserManager(Func<string, IOrigamUser> userFactory, int numberOfInvalidPasswordAttempts)
        {
            this.userFactory = userFactory;
            this.numberOfInvalidPasswordAttempts = numberOfInvalidPasswordAttempts;
        }

        public Result<IOrigamUser> Find(
            string userName, string password)
        {
            Guid currentUserId = SecurityManager.CurrentUserProfile().Id;
            DataSet origamUserDataSet = GetOrigamUserDataSet(userName);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                // a user with the given username not found;
                if (log.IsInfoEnabled)
                {
                    log.InfoFormat("Attempting to authentize a username `{0}'. The username not found."
                        , userName);
                }
                return Result.Fail<IOrigamUser>(Resources.InvalidUsernameOrPassword);
            }
            DataRow origamUserRow 
                = origamUserDataSet.Tables["OrigamUser"].Rows[0];
            if ((bool)origamUserRow["IsLockedOut"])
            {
                // locked out
                if (log.IsInfoEnabled)
                {
                    log.InfoFormat("Attempting to authentize a username `{0}'. The user is locked out."
                        , userName);
                }
                Result.Fail<IOrigamUser>(Resources.AccountLocked);
            }
            if (!(bool)origamUserRow["EmailConfirmed"])
            {
                // email not confirmed
                if (log.IsInfoEnabled)
                {
                    log.InfoFormat("Attempting to authentize a username `{0}'. The user email isn't confirmed."
                        , userName);
                }
                Result.Fail<IOrigamUser>(Resources.AccountNotConfirmed);
            }
            string hashedPassword = (string)origamUserRow["Password"];
            VerificationResult verificationResult 
                = internalHasher.VerifyHashedPassword(hashedPassword, password);
            bool goingToRehash = false;
            switch (verificationResult)
            {
                case VerificationResult.Failed:
                    if ((int)origamUserRow["FailedPasswordAttemptCount"] == 0)
                    {
                        origamUserRow["FailedPasswordAttemptWindowStart"] 
                            = DateTime.Now;
                    }
                    origamUserRow["FailedPasswordAttemptCount"] 
                        = (int)origamUserRow["FailedPasswordAttemptCount"] + 1;
                    if ((int)origamUserRow["FailedPasswordAttemptCount"]
                    >= numberOfInvalidPasswordAttempts)
                    {
                        origamUserRow["IsLockedOut"] = true;
                        origamUserRow["LastLockoutDate"] = DateTime.Now;
                        origamUserRow["RecordUpdated"] = DateTime.Now;
                        origamUserRow["RecordUpdatedBy"] = currentUserId;
                    }
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Attempting to authentize a username `{0}'. "
                            + "Failed to verify a password. Attempt: {1} of max {2}.{3}",
                            userName, origamUserRow["FailedPasswordAttemptCount"],
                            numberOfInvalidPasswordAttempts,
                            ((int)origamUserRow["FailedPasswordAttemptCount"]
                            >= numberOfInvalidPasswordAttempts) ? 
                                " Locking out." : ""    
                            );
                    }
                    DataService.StoreData(Queries.ORIGAM_USER_DATA_STRUCTURE, 
                        origamUserDataSet, false, null);
                    return Result.Fail<IOrigamUser>("Verification failed.");
                case VerificationResult.SuccessRehashNeeded:
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Attempting to authentize a username `{0}'."
                            +" Success and going to rehash the password.",
                            userName);
                    }
                    goingToRehash = true;
                    break;
                case VerificationResult.Success:
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Attempting to authentize a username `{0}'. Success.",
                            userName);
                    }
                    break;
                default:
                    if (log.IsErrorEnabled)
                    {
                        log.ErrorFormat("Attempting to authentize a username `{0}'. An uneexpected result `{1}' returned.",
                            userName, verificationResult.ToString());
                    }
                    throw new ArgumentOutOfRangeException("verificationResult", 
                        verificationResult, "Unknown result");
            }
            // reload data to mitigate concurrency exception
            origamUserDataSet = GetOrigamUserDataSet(userName);
            origamUserRow = origamUserDataSet.Tables["OrigamUser"].Rows[0];
            origamUserRow["FailedPasswordAttemptCount"] = 0;
            origamUserRow["FailedPasswordAttemptWindowStart"] = DBNull.Value;
            origamUserRow["LastLoginDate"] = DateTime.Now;
            if (goingToRehash)
            {
                origamUserRow["Password"]
                        = internalHasher.HashPassword(password);
            }
            try {
                DataService.StoreData(Queries.ORIGAM_USER_DATA_STRUCTURE,
                    origamUserDataSet, false, null);
            } catch (Exception e)
            {
                if (log.IsErrorEnabled)
                {
                    log.ErrorFormat("Error updating authentization info for user `{0}': {1}"
                        ,userName, e.Message);
                }
                throw e;
            }
            return Result.Ok(OrigamUserDataSetToOrigamUser(userName, origamUserDataSet));
        }

        private IOrigamUser OrigamUserDataSetToOrigamUser(
            string userName, DataSet dataSet)
        {
            IOrigamUser user = userFactory(userName);
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
        
           private DataSet GetOrigamUserDataSet(string userName)
           {
               DataSet origamUserDataSet = GetOrigamUserDataSet(
                   Queries.GET_ORIGAM_USER_BY_USER_NAME,
                   "OrigamUser_parUserName", userName);
               return origamUserDataSet;
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
    }
}