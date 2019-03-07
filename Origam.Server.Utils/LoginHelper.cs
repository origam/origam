using log4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Origam.Security.Identity;
using System.Security.Claims;
using System.Web.WebPages;
using System.Web;
using Microsoft.Owin.Security;
using System.Configuration;
using Origam.Workbench.Services;
using System.Net.Mail;
using Origam;

namespace Origam.Server.Utils
{
	public class ExtraParameter 
	{
		public ExtraParameter(string Name, bool IsMandatory)
		{
			this.Name = Name;
			this.IsMandatory = IsMandatory;
		}
		public string Name { get; set; }
		public bool IsMandatory { get; set; }
	}

    public class LoginHelper
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static readonly String STRING_PASSWORD_DOESNT_MATCH
            = "PasswordsDoesntMatch";

        public static bool IsPasswordRecoverySupported()
        {
            AbstractUserManager userManager = AbstractUserManager.GetUserManager();
            return userManager.IsPasswordRecoverySupported;
        }

		public static bool IsEmailAddressValid(string emailaddress)
		{
			try
			{
				MailAddress m = new MailAddress(emailaddress);

				return true;
			}
			catch (FormatException)
			{
				return false;
			}
		}

		public static string Login(WebPage page, string action, string actionField,
            string userNameField, string passwordField, string languageField, 
            string secondFactorUrl, string setupUrl, string returnUrlField)
        {            
            log.Debug("Processing Login page...");
            string errorMessage = String.Empty;
            Microsoft.Owin.IOwinContext ctx = page.Request.GetOwinContext();
            AbstractUserManager userManager = AbstractUserManager.GetUserManager();
            if (userManager.IsInitialSetupNeeded())
            {
                page.Response.Redirect(setupUrl);
            }
            page.Validation.RequireFields(userNameField, passwordField);
            if (!String.IsNullOrEmpty(page.Request[languageField]))
            {
                page.Culture = page.Request[languageField];
                page.UICulture = page.Request[languageField];
                page.Response.SetCookie(new HttpCookie("origamLanguage", page.Request[languageField]));
                page.Response.Redirect("~/");
            }
            if ((page.Request[actionField] == action)
            && page.Validation.IsValid())
            {
                Task<OrigamUser> userTask
                    = userManager.FindAsync(page.Request[userNameField], page.Request[passwordField].TrimEnd());
                if (userTask.IsFaulted)
                {
                    log.Error("Failed to identify user.",
                        userTask.Exception.InnerException);
                    errorMessage = userTask.Exception.InnerException.Message;
                }
                else
                {
                    OrigamUser user = userTask.Result;
                    if (user == null)
                    {
                        errorMessage = Resources.LoginFailed;
                    }
                    else if (userManager.Is2FAUsed || user.Is2FAEnforced)
                    {
                        if (!userManager.IsSecondFactorAvailable())
                        {
                            errorMessage = Resources.No2FAProvider;
                        }
                        else
                        {
                            Task<string> tokenTask 
                                = userManager.GenerateTwoFactorTokenAsync(
                                user.BusinessPartnerId,
                                userManager.GetSecondFactorProvider());
                            if (tokenTask.IsFaulted)
                            {
                                log.Error("Failed to generate second factor token.",
                                    tokenTask.Exception);
                                errorMessage = Resources.LoginFailed;
                            }
                            else if (string.IsNullOrEmpty(tokenTask.Result))
                            {
                                errorMessage = Resources.LoginFailed;
                            }
                            else
                            {
                                Task<IdentityResult> notifyTask
                                    = userManager.NotifyTwoFactorTokenAsync(
                                    user.BusinessPartnerId,
                                    userManager.GetSecondFactorProvider(),
                                    tokenTask.Result);
                                if (notifyTask.IsFaulted)
                                {
                                    log.Error("Failed to notify user about token.",
                                        notifyTask.Exception);
                                }
                                else if (!notifyTask.Result.Succeeded)
                                {
                                    errorMessage = Resources.LoginFailed;
                                }
                                else
                                {
                                    ClaimsIdentity identity = new ClaimsIdentity(
                                        DefaultAuthenticationTypes.TwoFactorCookie);
                                    identity.AddClaim(
                                        new Claim(ClaimTypes.NameIdentifier,
                                            user.BusinessPartnerId));
                                    ctx.Authentication.SignIn(identity);
                                    page.Response.Redirect(secondFactorUrl);
                                }
                            }
                        }
                    }
                    else
                    {
                        var claims = new List<Claim>();
                        claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                        var id = new ClaimsIdentity(
                            claims, DefaultAuthenticationTypes.ApplicationCookie);
                        ctx.Authentication.SignIn(id);
                        OrigamUserContext.Reset();
                        if (String.IsNullOrEmpty(page.Request[returnUrlField]))
                        {
                            page.Response.Redirect("~/");
                        }
                        else
                        {
                            page.Response.Redirect(page.Request[returnUrlField]);
                        }
                    }
                }
            }
            return errorMessage;
        }

        public static string Login2(WebPage page, string action, string actionField,
            string confirmationCodeField)
        {
            log.Debug("Processing Login2 page...");

            //todo: check if partial login was performed
            string errorMessage = string.Empty;
            Microsoft.Owin.IOwinContext ctx = page.Request.GetOwinContext();

            Task<AuthenticateResult> authTask
                = ctx.Authentication.AuthenticateAsync(
                DefaultAuthenticationTypes.TwoFactorCookie);
            if (authTask.IsFaulted || (authTask.Result == null))
            {
                log.Warn("Login2 accessed without Login. Redirecting...");
                page.Response.Redirect("~/");
            }

            AbstractUserManager userManager = AbstractUserManager.GetUserManager();

            page.Validation.RequireFields(confirmationCodeField);

            if ((page.Request[actionField] == action)
            && page.Validation.IsValid())
            {
                string userId = authTask.Result.Identity.GetUserId();
                Task<bool> verifyTask = userManager.VerifyTwoFactorTokenAsync(
                    userId, userManager.GetSecondFactorProvider(),
                    page.Request[confirmationCodeField]);
                if (verifyTask.IsFaulted)
                {
                    errorMessage = Resources.InvalidCode;
                    log.Error("Two factor authentication failed:",
                        verifyTask.Exception);
                }
                else if (!verifyTask.Result)
                {
                    errorMessage = Resources.InvalidCode;
                }
                else
                {
                    ctx.Authentication.SignOut(
                        DefaultAuthenticationTypes.TwoFactorCookie);
                    Task<OrigamUser> findTask = userManager.FindByIdAsync(userId);
                    if (findTask.IsFaulted)
                    {
                        errorMessage = Resources.LoginFailed;
                        log.Error("Failed to find user:", findTask.Exception);
                    }
                    else
                    {
                        var claims = new List<Claim>();
                        claims.Add(new Claim(
                            ClaimTypes.Name, findTask.Result.UserName));
                        var id = new ClaimsIdentity(
                            claims, DefaultAuthenticationTypes.ApplicationCookie);
                        ctx.Authentication.SignIn(id);
                        OrigamUserContext.Reset();
                        page.Response.Redirect("~/");
                    }
                }
            }
            return errorMessage;
        }

        public static string RecoverPassword(WebPage page, string action, string actionField,
            string emailField)
        {
            log.Debug("Processing Recover page...");

            AbstractUserManager userManager = AbstractUserManager.GetUserManager();
            if (!userManager.IsPasswordRecoverySupported)
            {
                page.Response.Redirect("~/Login");
            }
            string resultMessage = Resources.InvokeResetPassword_Headline;
            page.Validation.RequireField(emailField);
            if ((page.Request[actionField] == action)
            && page.Validation.IsValid())
            {
                Task<string> task = userManager.RecoverPasswordAsync(page.Request[emailField]);
                if (task.IsFaulted)
                {
                    resultMessage = Resources.FailedToGeneratePasswordResetToken;
                }
                else
                {
                    resultMessage = task.Result;
                }
            }
            return resultMessage;
        }

        public static string ConfirmEmail(WebPage page, string userIdField, string tokenField,
			string successUrlField)
        {
            log.Debug("Processing ConfirmEmail page...");

            string resultMessage = "";
            string userId = page.Request.Params[userIdField];
            string token = page.Request.Params[tokenField];
			string successUrl = page.Request.Params[successUrlField];

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                resultMessage = Resources.InvalidRequest;
            }
            else
            {
                AbstractUserManager userManager = AbstractUserManager.GetUserManager();
                Task<IdentityResult> task = userManager.ConfirmEmailAsync(userId, token);
                if (task.IsFaulted)
                {
                    resultMessage = task.Exception.ToString();
                }
                else if (task.Result.Succeeded)
                {
					if (!string.IsNullOrWhiteSpace(successUrl))
					{
						page.Response.Redirect(successUrl);
					}
                    resultMessage = Resources.EmailConfirmed;
                }
                else
                {
                    resultMessage = string.Join(" ", task.Result.Errors);
                }
            }
            return resultMessage;
        }

        public static void SignOut(WebPage page)
        {
            var ctx = page.Request.GetOwinContext();
            var authenticationManager = ctx.Authentication;
            authenticationManager.SignOut();
            Origam.OrigamUserContext.Reset();
            page.Response.Redirect(ConfigUtils.SignOutUrl());
        }

        public static bool IsInitialSetupNeeded(WebPage page)
        {
            Microsoft.Owin.IOwinContext ctx = page.Request.GetOwinContext();
            AbstractUserManager userManager = AbstractUserManager.GetUserManager();
            return userManager.IsInitialSetupNeeded();
        }

        public static string Setup(WebPage page, string action, string actionField,
            string nameField, string firstNameField, string emailField,
            string userNameField, string passwordField)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Processing Setup page...");
            }

            string errorMessage = Resources.RegisterHeadline;
            Microsoft.Owin.IOwinContext ctx = page.Request.GetOwinContext();
            AbstractUserManager userManager = AbstractUserManager.GetUserManager();

            if (!userManager.IsInitialSetupNeeded())
            {
                return Resources.ApplicationSetupAlready;
            }

            page.Validation.RequireFields(nameField, firstNameField, emailField,
                userNameField, passwordField);

            if ((page.Request[actionField] == action)
            && page.Validation.IsValid())
            {
                try
                {
                    userManager.CreateInitialUser(page.Request[userNameField], 
                        page.Request[passwordField].TrimEnd(), page.Request[firstNameField], 
                        page.Request[nameField], page.Request[emailField]);
                    page.Response.Redirect("~/");
                }
                catch (Exception ex)
                {
                    if (log.IsFatalEnabled)
                    {
                        log.Fatal(ex.Message, ex);
                    }
                    errorMessage = ex.Message;
                }
            }
            return errorMessage;
        }

		// backward compatibility for old registration pages without passwd confirmation field
		public static string RegisterNewUser(WebPage page, string action, string actionField,
			string nameField, string firstNameField, string emailField,
			string userNameField, string passwordField, ref bool success)
		{
			return RegisterNewUser(page, action, actionField,
				nameField, firstNameField, emailField,
				userNameField, passwordField, passwordField, ref success, null);
		}

		public static string RegisterNewUser(WebPage page, string action, string actionField,
			string nameField, string firstNameField, string emailField,
			string userNameField, string passwordField, string confirmPasswordField, ref bool success)
		{
			return RegisterNewUser(page, action, actionField,
				nameField, firstNameField, emailField,
				userNameField, passwordField, confirmPasswordField, ref success, null);
		}

		public static string RegisterNewUser(WebPage page, string action, string actionField,
			string nameField, string firstNameField, string emailField,
			string userNameField, string passwordField, string confirmPasswordField, ref bool success,
			List<ExtraParameter> extraParameters)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug("Processing Register page...");
			}

			if (!ConfigUtils.UserRegistrationAllowed())
			{
				return "User registration is not allowed on this site.";
			}

			string errorMessage = Resources.RegisterHeadline;
			AbstractUserManager userManager = AbstractUserManager.GetUserManager();
			List<string> mandatoryFieldList = new List<string>();
			mandatoryFieldList.Add(nameField);
			mandatoryFieldList.Add(firstNameField);
			mandatoryFieldList.Add(emailField);
			mandatoryFieldList.Add(userNameField);
			mandatoryFieldList.Add(passwordField);
			mandatoryFieldList.Add(confirmPasswordField);
			if (extraParameters != null)
			{
				foreach (ExtraParameter x in extraParameters)
				{
					if (x.IsMandatory)
					{
						mandatoryFieldList.Add(x.Name);
					}
				}
			}
			
			string[] mandatoryFields = mandatoryFieldList.ToArray();

            page.Validation.RequireFields(mandatoryFields);
			if ((page.Request[actionField] == action)
			&& page.Validation.IsValid())
			{
				if (page.Request[passwordField] != page.Request[confirmPasswordField])
				{
					return Resources.PasswordMatchError;
				}
				if (!IsEmailAddressValid(page.Request[emailField]))
				{
					return Resources.EmailInvalid;
				}
                try
                {
					// build extra parameters - get values from request posted
					Dictionary<string, object> extraParamsDict = new Dictionary<string, object>();
					if (extraParameters != null)
					{
						foreach (ExtraParameter x in extraParameters )
						{
							extraParamsDict.Add(x.Name, page.Request[x.Name]);
                        }
					}
                    userManager.RegisterNewUser(page.Request[userNameField],
                        page.Request[passwordField].TrimEnd(), page.Request[firstNameField],
                        page.Request[nameField], page.Request[emailField],
                        System.Configuration.ConfigurationManager.AppSettings[
                            "userRegistration_DefaultRoleId"],
						extraParamsDict);
                    success = true;

					OrigamUser origamUser = userManager.FindByName(page.Request[userNameField]);
										
					if (userManager.EmailConfirmed(origamUser.BusinessPartnerId))
					{
						return Resources.NewUserRegistrationSucceeded;
					}
                    return Resources.NewUserRegistrationMailSent;
                }
                catch (Exception ex)
                {
                    if (log.IsFatalEnabled)
                    {
                        log.Fatal(ex.Message, ex);
                    }
                    errorMessage = ex.Message;
                }
            }
            return errorMessage;
        }


        public static string ResetPassword(WebPage page, string action, string actionField,
                    string userNameField, string newPasswordField, string confirmPasswordField,
                    ref bool success)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Processing password reset page for username {0} ...",
                    userNameField);
            }
            // get username from url
            string userName = Uri.UnescapeDataString(page.Request.Params["username"]);
            string token = Uri.UnescapeDataString(page.Request.Params["token"]);

            if (string.IsNullOrEmpty(userNameField))
            {
                return Origam.Server.Utils.Resources.UserNameRequired;
            }
            // TODO - add token required...
            string errorMessage = string.Format(Origam.Server.Utils.Resources.PasswordResetHeadline,
                userName);

            page.Validation.RequireFields(newPasswordField, confirmPasswordField);

            AbstractUserManager userManager = AbstractUserManager.GetUserManager();
            Task<IdentityResult> tokenVerificaitonTask =
                userManager.ValidatePasswordResetTokenFromUsernameAsync(userName, token);
            if (tokenVerificaitonTask.IsFaulted)
            {
                return tokenVerificaitonTask.Exception.Message;
            }
            if (!tokenVerificaitonTask.Result.Succeeded)
            {
                return string.Join(", ", tokenVerificaitonTask.Result.Errors);
            }

            if ((page.Request[actionField] == action)
                && page.Validation.IsValid())
            {
                if (page.Request[newPasswordField].Trim() != page.Request[confirmPasswordField].Trim())
                {
                    IParameterService parameterService =
                        ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

                    parameterService.GetString(STRING_PASSWORD_DOESNT_MATCH);
                    return parameterService.GetString(STRING_PASSWORD_DOESNT_MATCH);
                }
                try
                {
                    Task<IdentityResult> ResetPasswordTask =
                        userManager.ResetPasswordFromUsernameAsync(
                            userName, token, page.Request[newPasswordField].Trim()
                        );
                    if (ResetPasswordTask.IsFaulted)
                    {
                        return ResetPasswordTask.Exception.Message;
                    }
                    if (!ResetPasswordTask.Result.Succeeded)
                    {
                        return string.Join(", ", ResetPasswordTask.Result.Errors);
                    }
                    success = true;
                    return Origam.Server.Utils.Resources.PasswordResetSuccess;
                }
                catch (Exception ex)
                {
                    if (log.IsFatalEnabled)
                    {
                        log.Fatal(ex.Message, ex);
                    }
                    errorMessage = ex.Message;
                }
            }
            return errorMessage;
        }


    }
}
