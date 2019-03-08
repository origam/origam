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
        internal readonly static Guid LANGUAGE_TAGIETF_LOOKUP
            = new Guid("7823d8af-4968-48c3-a772-287475d429e1");

        private static readonly String FROM_ADDRESS =
            System.Configuration.ConfigurationManager.AppSettings["mailFrom"];
        private static readonly String RESET_PWD_SUBJECT =
            System.Configuration.ConfigurationManager.AppSettings["ResetPasswordMail_Subject"];
        private static readonly String RESET_PWD_BODY_FILENAME =
            System.Configuration.ConfigurationManager.AppSettings["ResetPasswordMail_BodyFileName"];
        private static readonly String USER_UNLOCK_NOTIFICATION_SUBJECT =
            System.Configuration.ConfigurationManager.AppSettings["UserUnlockNotification_Subject"];
        private static readonly String USER_UNLOCK_NOTIFICATION_BODY_FILENAME =
            System.Configuration.ConfigurationManager.AppSettings["UserUnlockNotification_BodyFileName"];
        private static readonly String REGISTER_NEW_USER_SUBJECT =
            System.Configuration.ConfigurationManager.AppSettings["userRegistration_MailSubject"];
        private static readonly String REGISTER_NEW_USER_FILENAME =
            System.Configuration.ConfigurationManager.AppSettings["userRegistration_MailBodyFileName"];
        private const string INITIAL_SETUP_PARAMETERNAME = "InitialUserCreated";
		private static readonly String MAIL_QUEUE_NAME =
			System.Configuration.ConfigurationManager.AppSettings["MailQueue_Name"];

		private static readonly String PORTAL_BASE_URL =
            System.Configuration.ConfigurationManager.AppSettings["PortalBaseUrl"];

        protected static readonly ILog log 
            = LogManager.GetLogger(typeof(AbstractUserManager));

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

        private bool _IsPasswordRecoverySupported = false;

        public bool IsPasswordRecoverySupported
        {
            get { return _IsPasswordRecoverySupported; }
            set { _IsPasswordRecoverySupported = value; }
        }

        public AbstractUserManager(IUserStore<OrigamUser> store)
            : base(store)
        {
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

//        public class TokenResult
//        {
//            public string Token;
//            public string UserName;
//            public string ErrorMessage;
//            public int TokenValidityHours;
//        }

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
                    userUnlockNotificationMail = GenerateMail(resultEmail,
                        FROM_ADDRESS, USER_UNLOCK_NOTIFICATION_BODY_FILENAME,
                        Resources.UserUnlockNotificationTemplate,
                        USER_UNLOCK_NOTIFICATION_SUBJECT, userLangIETF, replacements);
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
				SendMailByAWorkflow(userUnlockNotificationMail);
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

        public static string FindBestLocalizedFile(string filePath,
            string languageIETF)
        {
            if (String.IsNullOrEmpty(languageIETF))
            {
                // language not sent, use current thread one
                languageIETF = System.Threading.Thread.CurrentThread.
                    CurrentUICulture.IetfLanguageTag;
            }

            // find the last '.'
            int lastDotIndex = filePath.LastIndexOf('.');
            // create a localized file candidate ( password_reset.de-DE.txt )
            /*
                password_reset.txt -> password_reset.de-DE.txt
                password_reset -> password_reset.de-DE
             */
            string candidate;
            if (lastDotIndex == -1)
            {
                // dot not found
                candidate = String.Format("{0}.{1}", filePath, languageIETF);
            }
            else
            {
                candidate = String.Format("{0}.{1}{2}", filePath.Substring(0,
                    lastDotIndex), languageIETF,
                    filePath.Substring(lastDotIndex));
            }
            if (File.Exists(candidate)) return candidate;
            // try better
            /*
                password_reset.txt -> password_reset.de.txt
                password_reset -> password_reset.de
             */
            string[] splittedIETF = languageIETF.Split('-');
            if (splittedIETF.Length == 2)
            {
                if (lastDotIndex == -1)
                {
                    candidate = String.Format("{0}.{1}", filePath,
                        splittedIETF[0]);
                }
                else
                {
                    candidate = String.Format("{0}.{1}{2}", filePath.Substring(
                        0, lastDotIndex), splittedIETF[0],
                        filePath.Substring(lastDotIndex));
                }
                if (File.Exists(candidate)) return candidate;
            }
            // fallback
            return filePath;            
        }

        protected bool SendPasswordResetToken(string email, string username,
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
						PORTAL_BASE_URL)
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
			if (string.IsNullOrWhiteSpace(PORTAL_BASE_URL) && string.IsNullOrEmpty(RESET_PWD_BODY_FILENAME))
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
					mail = GenerateMail(email, FROM_ADDRESS,
						RESET_PWD_BODY_FILENAME,
						Resources.ResetPasswordMailTemplate,
						RESET_PWD_SUBJECT, userLangIETF, replacements);
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
				SendMailByAWorkflow(mail);
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

		private static void SendMailByAWorkflow(MailMessage mail)
		{
			// send mail - by a workflow located at root package			
			QueryParameterCollection pms = new QueryParameterCollection();
			pms.Add(new QueryParameter("subject", mail.Subject));
			pms.Add(new QueryParameter("body", mail.Body));
			pms.Add(new QueryParameter("recipientEmail", mail.To.First().Address));
			pms.Add(new QueryParameter("senderEmail", mail.From.Address));
			if (!string.IsNullOrWhiteSpace(mail.From.DisplayName))
			{
				pms.Add(new QueryParameter("senderName", mail.From.DisplayName));
			}
			if (!string.IsNullOrWhiteSpace(MAIL_QUEUE_NAME))
			{
				pms.Add(new QueryParameter("MailWorkQueueName", MAIL_QUEUE_NAME));
			}
			WorkflowService.ExecuteWorkflow(new Guid("6e6d4e02-812a-4c95-afd1-eb2428802e2b"), pms, null);
		}

		/// <summary>
		/// Generates and return MailMessage ready to send with smtp.
		/// Firstly try to use a template from configured filename.
		/// It searches the most accurate language version of filename
		/// (e.g. when filename is 'password_reset.txt', then it finds
		/// 'password_rest.de.txt' when language is 'de-DE' and the latter file
		/// exists, but 'password_rest.de-DE.txt' doesn't exist.)
		/// If file not configured, than try to use template from resources.
		/// It tries to parse a subject from template (when a first line
		/// starts with 'Subject:', it parses the rest as a subject of an email.
		/// Then it process (replace placeholeder) a template for body
		/// and subject as well. If subject has not been found in a template, 
		/// it tries to get it from configuration.
		/// </summary>
		/// <param name="userEmail">Recipient email address</param>
		/// <param name="fromAddress">Sender email address</param>
		/// <param name="templateFilename">base name of template filename,
		/// a filename has to be located in the root of application
		/// diractory</param>
		/// <param name="templateFromResources">a template content to be used
		/// as default one</param>
		/// <param name="subjectFromConfig">a text of subject to be used as
		/// a default one</param>
		/// <param name="userLangIETF">language IETF tag of recipient user
		/// to resolve name of the most proper template filename</param>
		/// <param name="replacements">list of template replacements
		/// (key-value pairs) (key:placeholer string, value:new value)</param>
		/// <returns></returns>
		private static MailMessage GenerateMail(string userEmail, string fromAddress,
            string templateFilename, string templateFromResources,
            string subjectFromConfig,
            string userLangIETF, List<KeyValuePair<string, string>> replacements)
        {
            MailMessage passwordRecoveryMail = new MailMessage(fromAddress,
                userEmail);
            string templateContent =
                (String.IsNullOrEmpty(templateFilename)) ?
                    templateFromResources
                    : GetLocalizedMailTemplateText(templateFilename,
                         userLangIETF);

            string[] subjectAndBody = processMailTemplate(templateContent,
                replacements);
            passwordRecoveryMail.Subject = subjectAndBody[0];
            passwordRecoveryMail.Body = subjectAndBody[1];

            if (string.IsNullOrWhiteSpace(passwordRecoveryMail.Subject))
            {
                passwordRecoveryMail.Subject = subjectFromConfig;
            }
            return passwordRecoveryMail;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="replacements">List of substitutions to be made in 
        /// email template. The key of KeyValuePair is a name of placeholder in
        /// a template, the value is a new value that replace a placeholder.
        /// </param>
        /// <param name="languageIETF"></param>
        /// <returns></returns>
        private static string GetLocalizedMailTemplateText(
            string templateFilename,
            string languageIETF = "")
        {
            string filePath = Path.Combine(
                AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                templateFilename);
            return File.ReadAllText(
                FindBestLocalizedFile(filePath, languageIETF)).Trim();     
        }

        private static string[] processMailTemplate(string templateContent,
            List<KeyValuePair<string, string>> replacements)
        {
            string subject = null;
            if (templateContent.ToLower().StartsWith("subject:"))
            {
                subject = templateContent.Substring(8,
                    templateContent.IndexOf('\n') - 8).Trim();
                foreach (KeyValuePair<string, string> replacement
                    in replacements)
                {
                    subject = subject.Replace(replacement.Key, replacement.Value);
                }
                templateContent = templateContent.Substring(
                    templateContent.IndexOf('\n')).TrimStart();
            }
            foreach (KeyValuePair<string, string> replacement in replacements)
            {
                templateContent = templateContent.Replace(replacement.Key,
                    replacement.Value);
            }
            return new string[] { subject, templateContent };
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
                        PORTAL_BASE_URL),
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
						PORTAL_BASE_URL)
				};
            // PORTAL_BASE_URL is mandatory if using default template
            if (string.IsNullOrWhiteSpace(PORTAL_BASE_URL) &&  string.IsNullOrEmpty(REGISTER_NEW_USER_FILENAME))
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
                mail = GenerateMail(email, FROM_ADDRESS,
                    REGISTER_NEW_USER_FILENAME,
                    Resources.RegisterNewUserTemplate,
                    REGISTER_NEW_USER_SUBJECT, userLangIETF, replacements);
            }
            
            try
            {
				SendMailByAWorkflow(mail);
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
    }
}
