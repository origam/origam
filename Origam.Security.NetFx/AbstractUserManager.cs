#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Security.Principal;
using log4net;
using Microsoft.AspNet.Identity;
using Origam;
using Origam.DA;
using Origam.Workbench.Services.CoreServices;
using Origam.Workbench.Services;
using System.Data;
using System.Collections.Generic;
using System.Security;
using Origam.Security.Common;

namespace Origam.Security.Identity
{
    public abstract class AbstractUserManager : UserManager<OrigamUser>
    {
        private static Guid BUSINESS_PARTNER_DATA_STRUCTURE
            = new Guid("f4c92dce-d634-4179-adb4-98876b870cc7");
        internal static Guid GET_BUSINESS_PARTNER_BY_USER_NAME
            = new Guid("545396e7-d88e-4315-a112-f8feda7229bf");
        internal static Guid GET_BUSINESS_PARTNER_BY_ID
            = new Guid("4e46424b-349f-4314-bc75-424206cd35b0");
        internal static Guid GET_BUSINESS_PARTNER_BY_USER_EMAIL
            = new Guid("46fd2484-4506-45a2-8a96-7855ea116210");

        private const string INITIAL_SETUP_PARAMETERNAME = "InitialUserCreated";

        protected static readonly ILog log 
            = LogManager.GetLogger(typeof(AbstractUserManager));

        private readonly AccountMailSender accountMailSender;
        
        private static Func<AbstractUserManager> createUserManagerCallback = null;

        public static void RegisterCreateUserManagerCallback(
            Func<AbstractUserManager> callback)
        {
            createUserManagerCallback = callback;
        }

        protected static bool emailUniquenessChecked = false;

        public static AbstractUserManager GetUserManager()
        {
            if (createUserManagerCallback == null)
            {
                throw new Exception(Resources.UserManagerNotSet);
            }
            return createUserManagerCallback();
        }

        public bool UniqueEmailRequired
        {
            set
            {
                if (value && !emailUniquenessChecked)
                {
                    CheckEmailUniqueness();
                }
            }
        }

        private bool _Is2FAUsed = false;

        public bool Is2FAUsed
        {
            get { return _Is2FAUsed; }
            set { _Is2FAUsed = value; }
        }

        protected bool _exposeLoginAttemptsInfo = false;
        public bool ExposeLoginAttemptsInfo
        {
            get { return GetExposeLoginAttemptsInfo(); }
            set { SetExposeLoginAttemptsInfo(value); }
        }
        // the getter can't be overriden
        private bool GetExposeLoginAttemptsInfo()
        {
            return _exposeLoginAttemptsInfo;
        }
        // by default, user managers doesn't allow this functionality
        // (unless they correctly implement GetAccessFailedCountAsync)
        protected virtual void SetExposeLoginAttemptsInfo(bool value)
        {
            throw new NotSupportedException();
        }

        private bool _IsPasswordRecoverySupported = false;

        public bool IsPasswordRecoverySupported
        {
            get { return _IsPasswordRecoverySupported; }
            set { _IsPasswordRecoverySupported = value; }
        }

        public AbstractUserManager(IUserStore<OrigamUser> store)
            : base(store)
        {
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            accountMailSender = new AccountMailSender(
                fromAddress: appSettings["mailFrom"],
                resetPwdSubject: appSettings["ResetPasswordMail_Subject"],
                resetPwdBodyFilename: appSettings["ResetPasswordMail_BodyFileName"],
                userUnlockNotificationSubject: appSettings["UserUnlockNotification_Subject"],
                userUnlockNotificationBodyFilename: appSettings["UserUnlockNotification_BodyFileName"],
                registerNewUserSubject: appSettings["userRegistration_MailSubject"],
                registerNewUserFilename: appSettings["userRegistration_MailBodyFileName"],
                mailQueueName: appSettings["MailQueue_Name"],
                portalBaseUrl: appSettings["PortalBaseUrl"],
                applicationBasePath: AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
        }

        /// <summary>
        /// Indicates wheter the initial user has been set up.
        /// </summary>
        /// <returns>True if there are no users set up yet and a welcome page needs
        /// to be displayed where the user can enter his user name and password.</returns>
        public bool IsInitialSetupNeeded()
        {
            IParameterService parameterService =
                ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

            return !(bool)parameterService.GetParameterValue(INITIAL_SETUP_PARAMETERNAME);
        }

        /// <summary>
        /// Creates the first user in the application.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="firstName"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        public void CreateInitialUser(string userName, string password,
            string firstName, string name, string email)
        {
            if (IsInitialSetupNeeded())
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug("Creating an initial user.");
                }
                string roleId = SecurityManager.BUILTIN_SUPER_USER_ROLE;
                CreateUser(userName, password, firstName, name, email, roleId, false);
                IParameterService parameterService =
                    ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
                parameterService.SetCustomParameterValue(INITIAL_SETUP_PARAMETERNAME, true, Guid.Empty, 0, null, true, 0, 0, null);
            }
            else
            {
                throw new Exception("Initial user has already been set up.");
            }
        }

		public void RegisterNewUser(string userName, string password, string firstName,
			string name, string email, string roleId)
		{
			CreateUser(userName, password, firstName, name, email, roleId, true);
		}


		public void RegisterNewUser(string userName, string password, string firstName,
			string name, string email, string roleId,
			Dictionary<string, object> additionalWFParameters)
		{
			CreateUser(userName, password, firstName, name, email, roleId, true,
				additionalWFParameters);
		}

		private void CreateUser(string userName, string password, string firstName,
			string name, string email, string roleId, bool requestEmailConfirmation)
		{
			CreateUser(userName, password, firstName, name, email,
				roleId, requestEmailConfirmation, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <param name="firstName"></param>
		/// <param name="name"></param>
		/// <param name="email"></param>
		/// <param name="roleId"></param>
		/// <param name="requestEmailConfirmation"></param>
		/// <param name="additionalWFParameters">A dictionary with parameters (key is name of the parameter) to be passed into Identity_CreateUser workflow. Can be used for extra BusinessPartner fields.</param>
		private void CreateUser(string userName, string password, string firstName, 
            string name, string email, string roleId, bool requestEmailConfirmation,
			Dictionary<string, object> additionalWFParameters)
        {
            string id = Guid.NewGuid().ToString();
            QueryParameterCollection parameters = new QueryParameterCollection();
            parameters.Add(new QueryParameter("Id", id));
            parameters.Add(new QueryParameter("UserName", userName));
            parameters.Add(new QueryParameter("Password", password));
            parameters.Add(new QueryParameter("FirstName", firstName));
            parameters.Add(new QueryParameter("Name", name));
            parameters.Add(new QueryParameter("Email", email));
            parameters.Add(new QueryParameter("RoleId", roleId));
            parameters.Add(new QueryParameter("RequestEmailConfirmation",
                requestEmailConfirmation));
			// add extra parameters
			if (additionalWFParameters != null)
			{
				foreach (KeyValuePair<string, object> x in additionalWFParameters)
				{
					parameters.Add(new QueryParameter(x.Key, x.Value));
                }
			}
            WorkflowService.ExecuteWorkflow(
                new Guid("2bd4dbcc-d01e-4c5d-bedb-a4150dcefd54"), parameters, null);
			
            if (requestEmailConfirmation && !this.IsEmailConfirmed(id))
            {
                SendNewUserToken(id, email, userName, name, firstName);
            }
        }

        public virtual bool IsSecondFactorAvailable()
        {
            return TwoFactorProviders.Count > 0;
        }

        public virtual string GetSecondFactorProvider()
        {
            return TwoFactorProviders.First().Key;
        }

        public virtual Task<string> RecoverPasswordAsync(string email)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<IdentityResult> ResetPasswordFromUsernameAsync(
            string username, string token, string newPassword)
        {
            OrigamUser origamUser = await FindByNameAsync(username);
            if (origamUser == null)
            {
                if (log.IsErrorEnabled)
                {
                    log.ErrorFormat(
                        "Reseting password for user with username `{0}'. User not found."
                        , username);
                }
                return IdentityResult.Failed(Resources.ErrorUserNotFound);
            }
            return await ResetPasswordAsync(origamUser.Id, token, newPassword);
        }

        public virtual async Task<IdentityResult> ValidatePasswordResetTokenFromUsernameAsync(
            string username, string token)
        {
            OrigamUser origamUser = await FindByNameAsync(username);
            if (origamUser == null)
            {
                if (log.IsErrorEnabled)
                {
                    log.ErrorFormat(
                        "Reseting password for user with username `{0}'. User not found."
                        , username);
                }
                return IdentityResult.Failed(Resources.PasswordResetTokenInvalid);
            }

            // verify password-reset token
            bool verificationResult = await
                VerifyUserTokenAsync(origamUser.Id.ToString(), "ResetPassword", token);
            if (!verificationResult)
            {
                if (log.IsInfoEnabled)
                {
                    log.InfoFormat(
                        "Reseting password for user {0}: Invalid token `{1}'."
                        , username, token);
                }
                return IdentityResult.Failed(
                    Resources.PasswordResetTokenInvalid);
            }
            return IdentityResult.Success;
        }

        public virtual async Task<string> GetPasswordResetTokenAsync(string userId)
        {
            // find out if user exists in identity
            OrigamUser user = await FindByIdAsync(userId);
            if (user == null)
            {
                // user doesn't exist in identity => empty token returned
                return "";
            }

            // generate password reset token
            string passwordResetToken = await GeneratePasswordResetTokenAsync
                (userId.ToString()); 
            {
                return passwordResetToken;
            }
        }

        public virtual async Task<TokenResult>
            GetPasswordResetTokenFromEmailAsync(string email)
        {            
            // resolve username, id from email
            DataSet businessPartnerDataSet = GetBusinessPartnerDataSet(
                GET_BUSINESS_PARTNER_BY_USER_EMAIL,
                "BusinessPartner_parUserEmail", email);
            if (businessPartnerDataSet.Tables["BusinessPartner"].Rows.Count == 0)
            {
                if (log.IsErrorEnabled)
                {
                    log.ErrorFormat(
                        "Generating password reset token for email {0}, "
                        + " failed to find the user by email.",
                        email);
                }
                return new TokenResult
                    { Token = "", UserName = "",
                    ErrorMessage = Resources.EmailInvalid,
                    TokenValidityHours = 0};
            }
            Guid userId = (Guid)businessPartnerDataSet.Tables["BusinessPartner"]
                .Rows[0]["Id"];
            string userName = (string)businessPartnerDataSet.Tables["BusinessPartner"]
                .Rows[0]["UserName"];
            if (string.IsNullOrEmpty(userName))
            {
                if (log.IsErrorEnabled)
                {
                    log.ErrorFormat(
                        "Generating password reset token for email `{0}', "
                        + " failed. Business partner `{1}' is not a user.",
                        email, userId);
                }
                return new TokenResult
                { Token = "", UserName = "",
                    ErrorMessage = Resources.EmailInvalid,
                    TokenValidityHours = 0};
            }
            
            // find out if user exists in identity
            OrigamUser origamUser = await FindByIdAsync(userId.ToString());
            if (origamUser == null)
            {
                return new TokenResult
                { Token = "", UserName = "", ErrorMessage = Resources.EmailInvalid,
                TokenValidityHours = 0};
            }
            // generate password reset token
            string resetToken = await GeneratePasswordResetTokenAsync(
                userId.ToString()); 
            if (log.IsDebugEnabled)
            {
                log.DebugFormat(
                    "Generating password reset token for email `{0}`, "
                    + "success. User=`{1}' Username=`{2}', token=`{3}'",
                    email, userId, userName, resetToken);
            }
            OrigamTokenProvider origamTokenProvider =
                UserTokenProvider as OrigamTokenProvider;
            int tokenValidityHours = (origamTokenProvider == null) ?
                24
                : (int) origamTokenProvider.TokenLifespan.TotalHours;
            return new TokenResult
                { Token = resetToken,
                    UserName = userName, ErrorMessage = "",
                    TokenValidityHours = tokenValidityHours};
        }

        public virtual Task<bool> ChangePasswordQuestionAndAnswerAsync(
            string username, string password, string question, string answer)
        {
            return Task.FromResult(false);
        }

        public virtual Task<bool> UnlockUserAsync(string userName)
        {
            return Task.FromResult(false);
        }
        
        public virtual Task<IdentityResult> ConfirmEmailAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public virtual Task<XmlDocument> GetPasswordAttributesAsync()
        {
            XmlDocument document = new XmlDocument();
            XmlNode root = document.CreateElement("ROOT");
            document.AppendChild(root);
            XmlNode passwordAttributes = document.CreateElement("PasswordAttributes");
            FillPasswordAttributes(document, passwordAttributes);
            return Task.FromResult(document);
        }

        protected virtual void FillPasswordAttributes(
            XmlDocument document, XmlNode attributes)
        {
        }


        protected bool SendUserUnlockingNotification(string username,
            string email = null)
        {
            // get profile data for an unlocked user
            DataRow businessPartnerRow = getBusinessPartnerDataRow(username);
            // resolve language
            string languageId = businessPartnerRow["refLanguageId"].ToString();
            string userMail = (string)businessPartnerRow["UserEmail"];
            string firstNameAndName =
                businessPartnerRow["FirstNameAndName"].ToString();

            return accountMailSender.SendUserUnlockingNotification(username, email, languageId, firstNameAndName, userMail);
        }

     
        private static List<KeyValuePair<string, string> > 
            BuildReplacementsFromBusinessPartnerData(DataRow businessPartnerRow)
        {
            string firstNameAndName =
                businessPartnerRow["FirstNameAndName"].ToString();

            return new List<KeyValuePair<string, string>>
                {                   
                    new KeyValuePair<string, string>("<%FirstNameAndName%>",
                    firstNameAndName)
                };
        }


        private static DataRow getBusinessPartnerDataRow(string username)
        {
            DataSet data = GetBusinessPartnerDataSet(
                GET_BUSINESS_PARTNER_BY_USER_NAME,
                "BusinessPartner_parUserName", username);
            DataTable table = data.Tables["BusinessPartner"];
            if (table.Rows.Count == 0)
            {
                // user to unlock doesn't exist in business partner
                throw new Exception(String.Format(
                    Resources.BusinessPartnerNotFoundForTheUser, username));
            }
            DataRow businessPartnerRow = table.Rows[0];
            return businessPartnerRow;
        }

        protected bool SendPasswordResetToken(string email, string username,
            ref string resultMessage)
		{
			// get profile data for an unlocked user
			DataRow businessPartnerRow = getBusinessPartnerDataRow(username);
			// resolve language
			string languageId = businessPartnerRow["refLanguageId"].ToString();
            string name = (string)businessPartnerRow["Name"];
            string userFirstName = DBNull.Value.Equals(businessPartnerRow["FirstName"])
                ? null 
                : (string)businessPartnerRow["FirstName"];
            
            Task<TokenResult> getTokenTask = GetPasswordResetTokenFromEmailAsync(email);
            if (getTokenTask.IsFaulted)
            {
                throw getTokenTask.Exception;
            }

            TokenResult tokenResult = getTokenTask.Result;

            if (!string.IsNullOrEmpty(tokenResult.ErrorMessage))
            {
                resultMessage = tokenResult.ErrorMessage;
                return false;
            }
			return accountMailSender.SendPasswordResetToken(username, name,
                email,languageId, userFirstName, tokenResult.Token, tokenResult.TokenValidityHours, out resultMessage);
        }
        
		public void SendNewUserToken(string username
			, string transactionId = null)
		{
			// get profile data for an unlocked user
			DataRow businessPartnerRow = getBusinessPartnerDataRow(username);
			SendNewUserToken(
				((Guid) businessPartnerRow["Id"]).ToString(),
				(string) businessPartnerRow["UserEmail"],
				username,
				(string) businessPartnerRow["Name"],
				(string) businessPartnerRow["firstName"]);
		}

        internal void SendNewUserToken(
            string userId, string email, string username, string name, 
            string firstName)
        {
            string token;
            Task<string> task = GenerateEmailConfirmationTokenAsync(userId);
            if (task.IsFaulted)
            {
                throw task.Exception;
            }
            else
            {
                token = task.Result;
            }
            
            accountMailSender.SendNewUserToken(userId, email, username, name, firstName, token);
        }

        internal static DataSet GetBusinessPartnerDataSet(
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
                BUSINESS_PARTNER_DATA_STRUCTURE,
                methodId,
                Guid.Empty,
                Guid.Empty,
                transactionId,
                paramName,
                paramValue);
        }

        public override async Task<bool> IsInRoleAsync(string userId,
            string role)
        {
            IOrigamAuthorizationProvider provider =
                SecurityManager.GetAuthorizationProvider();
            OrigamUser user = await FindByIdAsync(userId);
            GenericPrincipal principal =
                new GenericPrincipal(new GenericIdentity(user.UserName), null);
            return provider.Authorize(principal, role);
        }

        protected virtual void CheckEmailUniqueness()
        {
            emailUniquenessChecked = true;
        }


        /// <summary>
        /// Returns user's failed password attempt count
        /// </summary>
        /// <param name="username">user's username</param>
        /// <returns>null when exposing is not switched on, or user not found</returns>
        public int? GetFailedPasswordAttemptCount(string username)
        {
            int? attemptedCount = null;
            if (ExposeLoginAttemptsInfo)
            {
                // resolve user id from username
                Task<OrigamUser> findByNameTask = FindByNameAsync(username);
                if (!findByNameTask.IsFaulted && findByNameTask.Result != null)
                {
                    Task<int> failedCountTask = GetAccessFailedCountAsync(findByNameTask.Result.Id);
                    // get current count
                    if (!failedCountTask.IsFaulted && failedCountTask.Result != -1)
                    {
                        attemptedCount = failedCountTask.Result;
                    }
                }
            }

            return attemptedCount;
        }
    }
}
