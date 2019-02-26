using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using BrockAllen.IdentityReboot;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Origam.Security.Common;
using Origam.ServerCore.Controllers;
using Origam.ServerCore.Extensions;
using Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore
{
    public class UserService
    {
        private readonly ILogger<AbstractController> log;
        private readonly InternalPasswordHasherWithLegacySupport passwordHasher;

        private int NumberOfInvalidPasswordAttempts { get; } = 3;
        
        public UserService(ILogger<AbstractController> log)
        {
            this.log = log;
            passwordHasher = new InternalPasswordHasherWithLegacySupport();
        }

        public Result<IOrigamUser> Authenticate(string userName, string password)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "origam_server"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("name", "origam_server"),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            Thread.CurrentPrincipal = new ClaimsPrincipal(identity);
            
            //Guid currentUserId = SecurityManager.CurrentUserProfile().Id;
            DataSet origamUserDataSet = GetOrigamUserDataSet(userName);
            if (origamUserDataSet.Tables["OrigamUser"].Rows.Count == 0)
            {
                log.InfoFormat("Attempting to authentize a username `{0}'. The username not found.",userName);
                return Result.Fail<IOrigamUser>(Resources.InvalidUsernameOrPassword);
            }
            DataRow OrigamUserRow 
                = origamUserDataSet.Tables["OrigamUser"].Rows[0];
            if ((bool)OrigamUserRow["IsLockedOut"])
            {
                log.InfoFormat("Attempting to authentize a username `{0}'. The user is locked out." , userName);
                return Result.Fail<IOrigamUser>(Resources.AccountLocked);
            }
            if (!(bool)OrigamUserRow["EmailConfirmed"])
            {
                log.InfoFormat("Attempting to authentize a username `{0}'. The user email isn't confirmed."
                    , userName);
                return Result.Fail<IOrigamUser>(Resources.AccountNotConfirmed);
            }
            string hashedPassword = (string)OrigamUserRow["Password"];

            VerificationResult verificationResult 
                = passwordHasher.VerifyHashedPassword(hashedPassword, password);
            bool goingToRehash = false;
            switch (verificationResult)
            {
                case VerificationResult.Failed:
                    if ((int)OrigamUserRow["FailedPasswordAttemptCount"] == 0)
                    {
                        OrigamUserRow["FailedPasswordAttemptWindowStart"] 
                            = DateTime.Now;
                    }
                    OrigamUserRow["FailedPasswordAttemptCount"] 
                        = (int)OrigamUserRow["FailedPasswordAttemptCount"] + 1;
                    if ((int)OrigamUserRow["FailedPasswordAttemptCount"]
                    >= NumberOfInvalidPasswordAttempts)
                    {
                        OrigamUserRow["IsLockedOut"] = true;
                        OrigamUserRow["LastLockoutDate"] = DateTime.Now;
                        OrigamUserRow["RecordUpdated"] = DateTime.Now;
                       // OrigamUserRow["RecordUpdatedBy"] = currentUserId;
                    }
//                    if (log.IsDebugEnabled)
//                    {
//                        log.DebugFormat("Attempting to authentize a username `{0}'. "
//                            + "Failed to verify a password. Attempt: {1} of max {2}.{3}",
//                            userName, OrigamUserRow["FailedPasswordAttemptCount"],
//                            NumberOfInvalidPasswordAttempts,
//                            ((int)OrigamUserRow["FailedPasswordAttemptCount"]
//                            >= NumberOfInvalidPasswordAttempts) ? 
//                                " Locking out." : ""    
//                            );
//                    }
                    DataService.StoreData(Queries.ORIGAM_USER_DATA_STRUCTURE, 
                        origamUserDataSet, false, null);
                    return Result.Fail<IOrigamUser>("Authentication failed");
                case VerificationResult.SuccessRehashNeeded:
//                    log.DebugFormat("Attempting to authentize a username `{0}'."
//                        +" Success and going to rehash the password.",
//                        userName);
                    goingToRehash = true;
                    break;
                case VerificationResult.Success:
//                    log.DebugFormat("Attempting to authentize a username `{0}'. Success.",
//                        userName);
                    break;
                default:
//                    log.ErrorFormat("Attempting to authentize a username `{0}'. An uneexpected result `{1}' returned.",
//                        userName, verificationResult.ToString());
                    return Result.Fail<IOrigamUser>("Authentication failed");
            }
            // reload data to mitigate concurrency exception
            origamUserDataSet = GetOrigamUserDataSet(userName);
            OrigamUserRow = origamUserDataSet.Tables["OrigamUser"].Rows[0];
            OrigamUserRow["FailedPasswordAttemptCount"] = 0;
            OrigamUserRow["FailedPasswordAttemptWindowStart"] = DBNull.Value;
            OrigamUserRow["LastLoginDate"] = DateTime.Now;
            if (goingToRehash)
            {
                OrigamUserRow["Password"]
                        = passwordHasher.HashPassword(password);
            }
            try {
                DataService.StoreData(Queries.ORIGAM_USER_DATA_STRUCTURE,
                    origamUserDataSet, false, null);
            } catch (Exception e)
            {
//                log.ErrorFormat("Error updating authentization info for user `{0}': {1}"
//                    ,userName, e.Message);
                throw e;
            }
            return Result.Ok(OrigamUserDataSetToOrigamUser(userName, origamUserDataSet));
        }
            
        
        private IOrigamUser OrigamUserDataSetToOrigamUser(
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



        public IOrigamUser GetById(int id)
        {
            throw new System.NotImplementedException();
        }

        public IOrigamUser Create(string user, string password)
        {
            throw new System.NotImplementedException();
        }

        public void Update(string user, string password = null)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}