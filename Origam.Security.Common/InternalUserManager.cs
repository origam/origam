using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Mail;
using System.Threading.Tasks;
using BrockAllen.IdentityReboot;
using CSharpFunctionalExtensions;
using log4net;
using Origam.DA;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Security.Common
{
    public class InternalUserManager
    {
        
         private static Guid BUSINESS_PARTNER_DATA_STRUCTURE
            = new Guid("f4c92dce-d634-4179-adb4-98876b870cc7");
        internal static Guid GET_BUSINESS_PARTNER_BY_USER_NAME
            = new Guid("545396e7-d88e-4315-a112-f8feda7229bf");
        internal static Guid GET_BUSINESS_PARTNER_BY_ID
            = new Guid("4e46424b-349f-4314-bc75-424206cd35b0");
        internal static Guid GET_BUSINESS_PARTNER_BY_USER_EMAIL
            = new Guid("46fd2484-4506-45a2-8a96-7855ea116210");
        internal readonly static Guid LANGUAGE_TAGIETF_LOOKUP
            = new Guid("7823d8af-4968-48c3-a772-287475d429e1");

//        private static readonly String FROM_ADDRESS =
//            System.Configuration.ConfigurationManager.AppSettings["mailFrom"];
//        private static readonly String RESET_PWD_SUBJECT =
//            System.Configuration.ConfigurationManager.AppSettings["ResetPasswordMail_Subject"];
//        private static readonly String RESET_PWD_BODY_FILENAME =
//            System.Configuration.ConfigurationManager.AppSettings["ResetPasswordMail_BodyFileName"];
//        private static readonly String USER_UNLOCK_NOTIFICATION_SUBJECT =
//            System.Configuration.ConfigurationManager.AppSettings["UserUnlockNotification_Subject"];
//        private static readonly String USER_UNLOCK_NOTIFICATION_BODY_FILENAME =
//            System.Configuration.ConfigurationManager.AppSettings["UserUnlockNotification_BodyFileName"];
//        private static readonly String REGISTER_NEW_USER_SUBJECT =
//            System.Configuration.ConfigurationManager.AppSettings["userRegistration_MailSubject"];
//        private static readonly String REGISTER_NEW_USER_FILENAME =
//            System.Configuration.ConfigurationManager.AppSettings["userRegistration_MailBodyFileName"];
        private const string INITIAL_SETUP_PARAMETERNAME = "InitialUserCreated";
//		private static readonly String MAIL_QUEUE_NAME =
//			System.Configuration.ConfigurationManager.AppSettings["MailQueue_Name"];
//
//		private static readonly String PORTAL_BASE_URL =
//            System.Configuration.ConfigurationManager.AppSettings["PortalBaseUrl"];
        
        
        protected static readonly ILog log 
            = LogManager.GetLogger(typeof(InternalUserManager));

        private readonly Func<string, IOrigamUser> userFactory;
        private readonly InternalPasswordHasherWithLegacySupport internalHasher =
            new InternalPasswordHasherWithLegacySupport();

        private readonly MailMan mailMan;

        private readonly int numberOfInvalidPasswordAttempts; 
        private readonly IFrameworkSpecificManager frameworkSpecificManager;
        private readonly string portalBaseUrl;
        private readonly string registerNewUserFilename;
        private readonly string fromAddress;
        private readonly string registerNewUserSubject;
        private readonly string resetPwdBodyFilename;
        private readonly string userUnlockNotificationBodyFilename;
        private readonly string userUnlockNotificationSubject;
        private readonly string resetPwdSubject;


        public InternalUserManager(Func<string, IOrigamUser> userFactory, 
            int numberOfInvalidPasswordAttempts,
            IFrameworkSpecificManager frameworkSpecificManager, 
            string mailTemplateDirectoryPath, 
            string mailQueueName, 
            string portalBaseUrl, 
            string registerNewUserFilename,
            string fromAddress, 
            string registerNewUserSubject, 
            string resetPwdBodyFilename,
            string userUnlockNotificationBodyFilename, 
            string userUnlockNotificationSubject,
            string resetPwdSubject)
        {
            this.userFactory = userFactory;
            this.numberOfInvalidPasswordAttempts = numberOfInvalidPasswordAttempts;
            this.frameworkSpecificManager = frameworkSpecificManager;
            this.portalBaseUrl = portalBaseUrl;
            this.registerNewUserFilename = registerNewUserFilename;
            this.fromAddress = fromAddress;
            this.registerNewUserSubject = registerNewUserSubject;
            this.resetPwdBodyFilename = resetPwdBodyFilename;
            this.userUnlockNotificationBodyFilename = userUnlockNotificationBodyFilename;
            this.userUnlockNotificationSubject = userUnlockNotificationSubject;
            this.resetPwdSubject = resetPwdSubject;
            mailMan = new MailMan(mailTemplateDirectoryPath, mailQueueName);
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
                    DataService.StoreData(ModelItems.ORIGAM_USER_DATA_STRUCTURE, 
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
                DataService.StoreData(ModelItems.ORIGAM_USER_DATA_STRUCTURE,
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
               ModelItems.GET_ORIGAM_USER_BY_USER_NAME,
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
               ModelItems.ORIGAM_USER_DATA_STRUCTURE,
               methodId,
               Guid.Empty,
               Guid.Empty,
               transactionId,
               paramName,
               paramValue);
       }
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
               SetInitialSetupComplete();
           }
           else
           {
               throw new Exception("Initial user has already been set up.");
           }
       }

       public void SetInitialSetupComplete()
       {
           IParameterService parameterService =
               ServiceManager.Services.GetService(typeof(IParameterService)) as
                   IParameterService;
           parameterService.SetCustomParameterValue(INITIAL_SETUP_PARAMETERNAME, true,
               Guid.Empty, 0, null, true, 0, 0, null);
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
       
       public void CreateUser(string userName, string password, string firstName,
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
		public void CreateUser(string userName, string password, string firstName, 
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
			
            if (requestEmailConfirmation && ! frameworkSpecificManager.EmailConfirmed(id))
            {
                SendNewUserToken(id, email, userName, name, firstName);
            }
        }


        public void SendNewUserToken(
            string userId, string email, string username, string name, 
            string firstName)
        {
            string token;
            Task<string> task = frameworkSpecificManager.GenerateEmailConfirmationTokenAsync(userId);
            if (task.IsFaulted)
            {
                throw task.Exception;
            }
            else
            {
                token = task.Result;
            }
            
            List<KeyValuePair<string, string>> replacements
                = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("<%Token%>",
                        Uri.EscapeDataString(token)),
                    new KeyValuePair<string, string>("<%UserId%>", userId),
                    new KeyValuePair<string, string>("<%UserName%>", username),
                    new KeyValuePair<string, string>("<%Name%>", name),
                    new KeyValuePair<string, string>("<%FirstName%>",
                        firstName),
                    new KeyValuePair<string, string>("<%PortalBaseUrl%>",
                        portalBaseUrl),
					new KeyValuePair<string, string>("<%EscapedUserName%>",
						Uri.EscapeDataString(username)),
					new KeyValuePair<string, string>("<%Name%>", name),
					new KeyValuePair<string, string>("<%EscapedName%>",
						Uri.EscapeDataString(name)),
					new KeyValuePair<string, string>("<%FirstName%>", firstName),
					new KeyValuePair<string, string>("<%EscapedFirstName%>",
						Uri.EscapeDataString(firstName)),
					new KeyValuePair<string, string>("<%UserEmail%>", email),
					new KeyValuePair<string, string>("<%EscapedUserEmail%>",
						Uri.EscapeDataString(email)),
					new KeyValuePair<string, string>("<%PortalBaseUrl%>",
						portalBaseUrl)
				};
            // PORTAL_BASE_URL is mandatory if using default template
            if (string.IsNullOrWhiteSpace(portalBaseUrl) &&  string.IsNullOrEmpty(registerNewUserFilename))
            {
                log.Error("'PortalBaseUrl' not configured while default template"
                    +"is used. Can't send a new registration email confirmation.");
                throw new Exception(Resources.RegisterNewUser_PortalBaseUrlNotConfigured);
            }
            
            
            MailMessage mail = null;
            string userLangIETF = System.Threading.Thread.CurrentThread.
                CurrentUICulture.IetfLanguageTag;
            using (LanguageSwitcher langSwitcher = new LanguageSwitcher(
                userLangIETF))
            {
                mail = mailMan.GenerateMail(email, fromAddress,
                    registerNewUserFilename,
                    Resources.RegisterNewUserTemplate,
                    registerNewUserSubject, userLangIETF, replacements);
            }
            
            try
            {
                mailMan.SendMailByAWorkflow(mail);
			}
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error("Failed to send new user registration mail", ex);
                }
                throw new Exception(Resources.FailedToSendNewUserRegistrationMail);
            }
            finally
            {
                mail.Dispose();
            }
        }

        public bool SendUserUnlockingNotification(string username,
            string email = null)
        {
            // get profile data for an unlocked user
            DataRow businessPartnerRow = getBusinessPartnerDataRow(username);
            // resolve language
            string languageId = businessPartnerRow["refLanguageId"].ToString();
            string userLangIETF = ResolveIetfTagFromOrigamLanguageId(languageId);
            // build template replacements
            List<KeyValuePair<string, string>> replacements =
                new List<KeyValuePair<string, string>>
                    { new KeyValuePair<string, string>("<%UserName%>", username) };
            replacements.AddRange(BuildReplacementsFromBusinessPartnerData(
                businessPartnerRow));
            // resolve recipient email
            string resultEmail = email;
            if (resultEmail == null)
            {
                resultEmail = (string)businessPartnerRow["UserEmail"];
            }

            MailMessage userUnlockNotificationMail;
            using (LanguageSwitcher langSwitcher = new LanguageSwitcher(
                userLangIETF))
            {
                try
                {
                    userUnlockNotificationMail = mailMan.GenerateMail(resultEmail,
                        fromAddress, userUnlockNotificationBodyFilename,
                        Resources.UserUnlockNotificationTemplate,
                        userUnlockNotificationSubject, userLangIETF, replacements);
                }
                catch (Exception ex)
                {
                    if (log.IsErrorEnabled)
                    {
                        log.ErrorFormat("Unlocking user: Failed to generate a mail"
                            + " for a user `{0}' to `{1}': {2}"
                            , username, resultEmail, ex);
                    }
                    throw ex;
                }
            }

            try
            {
				mailMan.SendMailByAWorkflow(userUnlockNotificationMail);
			}
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                {
                    log.ErrorFormat("Unlocking user: Failed to send a mail"
                        + " for a user `{0}' to `{1}': {2}"
                        , username, resultEmail, ex);
                }
                throw new Exception(
                    Resources.FailedToSendUserUnlockNotification);
            }
            finally
            {
                userUnlockNotificationMail.Dispose();
            }
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("User `{0}' has been unlocked and the"
                    + " notification mail has been sent to `{1}'.",
                    username, resultEmail);
            }
            return true;
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
                  BUSINESS_PARTNER_DATA_STRUCTURE,
                  methodId,
                  Guid.Empty,
                  Guid.Empty,
                  transactionId,
                  paramName,
                  paramValue);
          }
          
          private static string ResolveIetfTagFromOrigamLanguageId(
              string languageId)
          {
              IDataLookupService ls = ServiceManager.Services.GetService(
                  typeof(IDataLookupService)) as IDataLookupService;
              string userLangIETF = "";
              if (!string.IsNullOrEmpty(languageId))
              {
                  object ret = ls.GetDisplayText(LANGUAGE_TAGIETF_LOOKUP,
                      languageId, false, false, null);
                  if (ret != null)
                  {
                      userLangIETF = (string)ret;
                  }
              }

              return userLangIETF;
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
		  
		  
        public bool SendPasswordResetToken(string email, string username,
            ref string resultMessage)
		{
			// get profile data for an unlocked user
			DataRow businessPartnerRow = getBusinessPartnerDataRow(username);
			// resolve language
			string languageId = businessPartnerRow["refLanguageId"].ToString();
			string userLangIETF = ResolveIetfTagFromOrigamLanguageId(languageId);
			if (userLangIETF == "")
			{
				userLangIETF = System.Threading.Thread.CurrentThread.
					CurrentUICulture.IetfLanguageTag;
			}

			// generate password reset token
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

			List<KeyValuePair<string, string>> replacements
				= new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("<%Token%>",
						Uri.EscapeDataString(tokenResult.Token)),
					new KeyValuePair<string, string>("<%TokenValidityHours%>",
						tokenResult.TokenValidityHours.ToString()),
					new KeyValuePair<string, string>("<%UserName%>", tokenResult.UserName),
					new KeyValuePair<string, string>("<%EscapedUserName%>",
						Uri.EscapeDataString(tokenResult.UserName)),
					new KeyValuePair<string, string>("<%Name%>", (string)businessPartnerRow["Name"]),
					new KeyValuePair<string, string>("<%EscapedName%>",
						Uri.EscapeDataString((string)businessPartnerRow["Name"])),
					new KeyValuePair<string, string>("<%UserEmail%>",
						(string)businessPartnerRow["UserEmail"]),
					new KeyValuePair<string, string>("<%EscapedUserEmail%>",
						Uri.EscapeDataString((string)businessPartnerRow["UserEmail"])),
					new KeyValuePair<string, string>("<%PortalBaseUrl%>",
                        portalBaseUrl)
				};
			if (!DBNull.Value.Equals(businessPartnerRow["FirstName"]))
            {
				replacements.AddRange(
					new List<KeyValuePair<string, string>>
					{
						new KeyValuePair<string, string>("<%FirstName%>",
							(string)businessPartnerRow["FirstName"]),
						new KeyValuePair<string, string>("<%EscapedFirstName%>",
							Uri.EscapeDataString((string)businessPartnerRow["FirstName"]))
					}
				);
			}
			
			// PORTAL_BASE_URL is mandatory if using default template
			if (string.IsNullOrWhiteSpace(portalBaseUrl) && string.IsNullOrEmpty(resetPwdBodyFilename))
			{
				log.Error("'PortalBaseUrl' not configured while default template"
					+ "is used. Can't send a password reset email.");
				throw new Exception(Resources.ResetPasswordMail_PortalBaseUrlNotConfigured);
			}
			MailMessage mail = null;
			using (LanguageSwitcher langSwitcher = new LanguageSwitcher(
				userLangIETF))
			{
				try
				{
					mail = mailMan.GenerateMail(email, fromAddress,
						resetPwdBodyFilename,
						Resources.ResetPasswordMailTemplate,
						resetPwdSubject, userLangIETF, replacements);
				}
				catch (Exception ex)
				{
					if (log.IsErrorEnabled)
					{
						log.ErrorFormat("Failed to generate a password reset mail "
							+ " for the user `{0}' to email `{1}': {2}",
							username, email, ex);
					}
					resultMessage = Resources.FailedToSendPasswordResetToken;
					return false;
				}
			}

			try
			{
				mailMan.SendMailByAWorkflow(mail);
			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled)
				{
					log.Error(string.Format("Failed to send password reset "
						+ "mail for username `{0}', email `{1}'",
						username, email), ex);
				}
				resultMessage = Resources.FailedToSendPassword;
				return false;
			}
			finally
			{
				mail.Dispose();
			}
			if (log.IsDebugEnabled)
			{
				log.DebugFormat(
					"A new password for the user `{0}' " +
					"successfully generated and sent to `{1}'."
					, username, email);
			}
			resultMessage = Resources.PasswordResetMailSent;
			return true;
		}


        public Maybe<Tuple<Guid,string>> UserNameAndIdFromMail(string email)
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

                return null;
            }
            Guid userId = (Guid)businessPartnerDataSet.Tables["BusinessPartner"]
                .Rows[0]["Id"];
            string userName = (string)businessPartnerDataSet.Tables["BusinessPartner"]
                .Rows[0]["UserName"];
            return new Tuple<Guid, string>(userId, userName);
        }

        public async Task<TokenResult>
        GetPasswordResetTokenFromEmailAsync(string email)
        {            
//            // resolve username, id from email
//            DataSet businessPartnerDataSet = GetBusinessPartnerDataSet(
//                GET_BUSINESS_PARTNER_BY_USER_EMAIL,
//                "BusinessPartner_parUserEmail", email);
//            if (businessPartnerDataSet.Tables["BusinessPartner"].Rows.Count == 0)
//            {
//                if (log.IsErrorEnabled)
//                {
//                    log.ErrorFormat(
//                        "Generating password reset token for email {0}, "
//                        + " failed to find the user by email.",
//                        email);
//                }
//                return new TokenResult
//                    { Token = "", UserName = "",
//                    ErrorMessage = Resources.EmailInvalid,
//                    TokenValidityHours = 0};
//            }
//            Guid userId = (Guid)businessPartnerDataSet.Tables["BusinessPartner"]
//                .Rows[0]["Id"];
//            string userName = (string)businessPartnerDataSet.Tables["BusinessPartner"]
//                .Rows[0]["UserName"];

            Maybe<Tuple<Guid,string>> maybeUserInfo = UserNameAndIdFromMail(email);
            if (maybeUserInfo.HasNoValue)
            {
                return new TokenResult
                    { Token = "", UserName = "",
                    ErrorMessage = Resources.EmailInvalid,
                    TokenValidityHours = 0};
            }

            (Guid userId, string userName) = maybeUserInfo.Value;
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
            if (!frameworkSpecificManager.UserExists(userId))
            {
                return new TokenResult
                { Token = "", UserName = "", ErrorMessage = Resources.EmailInvalid,
                TokenValidityHours = 0};
            }
            // generate password reset token
            string resetToken = frameworkSpecificManager.GeneratePasswordResetTokenAsync1(
                userId.ToString()).Result; 
            if (log.IsDebugEnabled)
            {
                log.DebugFormat(
                    "Generating password reset token for email `{0}`, "
                    + "success. User=`{1}' Username=`{2}', token=`{3}'",
                    email, userId, userName, resetToken);
            }
            
            int tokenValidityHours = frameworkSpecificManager.TokenLifespan ?? 24;
            return new TokenResult
                { Token = resetToken,
                    UserName = userName, ErrorMessage = "",
                    TokenValidityHours = tokenValidityHours};
        }
    }


    public class TokenResult
    {
        public string ErrorMessage{ get; set; }
        public string Token{ get; set; }
        public int TokenValidityHours{ get; set; }
        public string UserName { get; set; }
    }

    public interface IFrameworkSpecificManager
    {
        bool EmailConfirmed(string id);
        Task<string> GenerateEmailConfirmationTokenAsync(string userId);
        bool UserExists(Guid userId);
        Task<string> GeneratePasswordResetTokenAsync1(string toString);
        int? TokenLifespan { get;}
    }
}