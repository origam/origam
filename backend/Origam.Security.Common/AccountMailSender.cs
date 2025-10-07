#region license

/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using log4net;
using Origam.DA;
using Origam.Extensions;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Security.Common;

public class AccountMailSender
{
    protected static readonly ILog log = LogManager.GetLogger(typeof(AccountMailSender));
    private static readonly Guid LANGUAGE_TAGIETF_LOOKUP = new Guid(
        "7823d8af-4968-48c3-a772-287475d429e1"
    );
    private readonly string portalBaseUrl;
    private readonly string registerNewUserFilename;
    private readonly string fromAddress;
    private readonly string registerNewUserSubject;
    private readonly string userUnlockNotificationBodyFilename;
    private readonly string userUnlockNotificationSubject;
    private readonly string mfaTemplateFileName;
    private readonly string mfaSubject;
    private readonly string mailQueueName;
    private readonly string applicationBasePath;
    public string ResetPasswordBodyFilename { get; set; }
    public string ResetPasswordSubject { get; set; }

    public AccountMailSender(
        string portalBaseUrl,
        string registerNewUserFilename,
        string fromAddress,
        string registerNewUserSubject,
        string userUnlockNotificationBodyFilename,
        string userUnlockNotificationSubject,
        string resetPwdBodyFilename,
        string resetPwdSubject,
        string applicationBasePath,
        string mfaTemplateFileName,
        string mfaSubject,
        string mailQueueName = null
    )
    {
        this.portalBaseUrl = portalBaseUrl;
        this.registerNewUserFilename = registerNewUserFilename;
        this.fromAddress = fromAddress;
        this.registerNewUserSubject = registerNewUserSubject;
        this.userUnlockNotificationBodyFilename = userUnlockNotificationBodyFilename;
        this.userUnlockNotificationSubject = userUnlockNotificationSubject;
        this.ResetPasswordBodyFilename = resetPwdBodyFilename;
        this.ResetPasswordSubject = resetPwdSubject;
        this.mailQueueName = mailQueueName;
        this.applicationBasePath = applicationBasePath;
        this.mfaTemplateFileName = mfaTemplateFileName;
        this.mfaSubject = mfaSubject;
    }

    public void SendNewUserToken(
        string userId,
        string email,
        string username,
        string name,
        string firstName,
        string token
    )
    {
        List<KeyValuePair<string, string>> replacements = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("<%Token%>", Uri.EscapeDataString(token)),
            new KeyValuePair<string, string>("<%UserId%>", userId),
            new KeyValuePair<string, string>("<%UserName%>", username),
            new KeyValuePair<string, string>("<%PortalBaseUrl%>", portalBaseUrl),
            new KeyValuePair<string, string>("<%EscapedUserName%>", Uri.EscapeDataString(username)),
            new KeyValuePair<string, string>("<%Name%>", name),
            new KeyValuePair<string, string>("<%EscapedName%>", Uri.EscapeDataString(name)),
            new KeyValuePair<string, string>("<%FirstName%>", firstName),
            new KeyValuePair<string, string>(
                "<%EscapedFirstName%>",
                Uri.EscapeDataString(firstName)
            ),
            new KeyValuePair<string, string>("<%UserEmail%>", email),
            new KeyValuePair<string, string>("<%EscapedUserEmail%>", Uri.EscapeDataString(email)),
        };
        // PORTAL_BASE_URL is mandatory if using default template
        if (
            string.IsNullOrWhiteSpace(portalBaseUrl)
            && string.IsNullOrEmpty(registerNewUserFilename)
        )
        {
            log.Error(
                "'PortalBaseUrl' not configured while default template"
                    + "is used. Can't send a new registration email confirmation."
            );
            throw new Exception(Resources.RegisterNewUser_PortalBaseUrlNotConfigured);
        }
        MailMessage mail = null;
        string userLangIETF = System
            .Threading
            .Thread
            .CurrentThread
            .CurrentUICulture
            .IetfLanguageTag;
        using (LanguageSwitcher langSwitcher = new LanguageSwitcher(userLangIETF))
        {
            mail = GenerateMail(
                email,
                fromAddress,
                registerNewUserFilename,
                Resources.RegisterNewUserTemplate,
                registerNewUserSubject,
                userLangIETF,
                replacements
            );
        }
        try
        {
            SendMailByAWorkflow(mail);
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError("Failed to send new user registration mail", ex);
            }
            throw new Exception(Resources.FailedToSendNewUserRegistrationMail);
        }
        finally
        {
            mail.Dispose();
        }
    }

    public bool SendUserUnlockingNotification(
        string username,
        string email,
        string languageId,
        string firstNameAndName
    )
    {
        string userLangIETF = ResolveIetfTagFromOrigamLanguageId(languageId);
        // build template replacements
        List<KeyValuePair<string, string>> replacements = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("<%UserName%>", username),
            new KeyValuePair<string, string>("<%FirstNameAndName%>", firstNameAndName),
        };
        MailMessage userUnlockNotificationMail;
        using (LanguageSwitcher langSwitcher = new LanguageSwitcher(userLangIETF))
        {
            try
            {
                userUnlockNotificationMail = GenerateMail(
                    email,
                    fromAddress,
                    userUnlockNotificationBodyFilename,
                    Resources.UserUnlockNotificationTemplate,
                    userUnlockNotificationSubject,
                    userLangIETF,
                    replacements
                );
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                {
                    log.ErrorFormat(
                        "Unlocking user: Failed to generate a mail"
                            + " for a user '{0}' to '{1}': {2}",
                        username,
                        email,
                        ex
                    );
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
                log.ErrorFormat(
                    "Unlocking user: Failed to send a mail" + " for a user '{0}' to '{1}': {2}",
                    username,
                    email,
                    ex
                );
            }
            throw new Exception(Resources.FailedToSendUserUnlockNotification);
        }
        finally
        {
            userUnlockNotificationMail.Dispose();
        }
        if (log.IsDebugEnabled)
        {
            log.DebugFormat(
                "User '{0}' has been unlocked and the"
                    + " notification mail has been sent to '{1}'.",
                username,
                email
            );
        }
        return true;
    }

    private static string ResolveIetfTagFromOrigamLanguageId(string languageId)
    {
        IDataLookupService ls =
            ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
        string userLangIETF = "";
        if (!string.IsNullOrEmpty(languageId))
        {
            object ret = ls.GetDisplayText(LANGUAGE_TAGIETF_LOOKUP, languageId, false, false, null);
            if (ret != null)
            {
                userLangIETF = (string)ret;
            }
        }
        return userLangIETF;
    }

    public void SendMultiFactorAuthCode(string email, string code)
    {
        List<KeyValuePair<string, string>> replacements = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("<%AuthenticationCode%>", Uri.EscapeDataString(code)),
            new KeyValuePair<string, string>("<%UserEmail%>", email),
        };
        // PORTAL_BASE_URL is mandatory if using default template
        if (
            string.IsNullOrWhiteSpace(portalBaseUrl)
            && string.IsNullOrEmpty(registerNewUserFilename)
        )
        {
            log.Error(
                "'PortalBaseUrl' not configured while default template"
                    + "is used. Can't send a new registration email confirmation."
            );
            throw new Exception(Resources.RegisterNewUser_PortalBaseUrlNotConfigured);
        }
        MailMessage mail;
        string userLangIETF = System
            .Threading
            .Thread
            .CurrentThread
            .CurrentUICulture
            .IetfLanguageTag;
        using (new LanguageSwitcher(userLangIETF))
        {
            mail = GenerateMail(
                email,
                fromAddress,
                mfaTemplateFileName,
                Resources.RegisterNewUserTemplate,
                mfaSubject,
                userLangIETF,
                replacements
            );
        }
        try
        {
            SendMailByAWorkflow(mail);
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError("Failed to send multi factor authentication mail", ex);
            }
            throw new Exception(Resources.FailedToSendMultiFactorAuthCode);
        }
        finally
        {
            mail.Dispose();
        }
    }

    public void SendPasswordResetToken(
        string username,
        string name,
        string email,
        string languageId,
        string firstName,
        string returnUrl,
        string token,
        int tokenValidityHours,
        out string resultMessage
    )
    {
        string userLangIETF = ResolveIetfTagFromOrigamLanguageId(languageId);
        if (userLangIETF == "")
        {
            userLangIETF = System.Threading.Thread.CurrentThread.CurrentUICulture.IetfLanguageTag;
        }
        var replacements = new List<KeyValuePair<string, string>>
        {
            new("<%Token%>", Uri.EscapeDataString(token)),
            new("<%TokenValidityHours%>", tokenValidityHours.ToString()),
            new("<%UserName%>", username),
            new("<%EscapedUserName%>", Uri.EscapeDataString(username)),
            new("<%Name%>", name),
            new("<%EscapedName%>", Uri.EscapeDataString(name)),
            new("<%UserEmail%>", email),
            new("<%EscapedUserEmail%>", Uri.EscapeDataString(email)),
            new("<%PortalBaseUrl%>", portalBaseUrl),
            new("<%ReturnUrl%>", Uri.EscapeDataString(returnUrl)),
        };
        if (firstName != null)
        {
            replacements.AddRange(
                new List<KeyValuePair<string, string>>
                {
                    new("<%FirstName%>", firstName),
                    new("<%EscapedFirstName%>", Uri.EscapeDataString(firstName)),
                }
            );
        }
        // PORTAL_BASE_URL is mandatory if using default template
        if (string.IsNullOrWhiteSpace(portalBaseUrl))
        {
            log.Error(
                "'PortalBaseUrl' not configured while default template"
                    + "is used. Can't send a password reset email."
            );
            throw new Exception(Resources.ResetPasswordMail_PortalBaseUrlNotConfigured);
        }
        if (
            string.IsNullOrEmpty(ResetPasswordSubject)
            || string.IsNullOrEmpty(ResetPasswordBodyFilename)
        )
        {
            log.Error(
                "'ResetPasswordMailSubject' or 'ResetPasswordMailBodyFileName' "
                    + "not configured while template"
                    + "is used for specific language. Can't send a password reset email."
            );
            throw new Exception(Resources.ResetPasswordMail_PortalBaseUrlNotConfigured);
        }
        MailMessage mail = null;
        using (LanguageSwitcher langSwitcher = new LanguageSwitcher(userLangIETF))
        {
            try
            {
                mail = GenerateMail(
                    email,
                    fromAddress,
                    ResetPasswordBodyFilename,
                    Resources.ResetPasswordMailTemplate,
                    ResetPasswordSubject,
                    userLangIETF,
                    replacements
                );
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                {
                    log.ErrorFormat(
                        "Failed to generate a password reset mail "
                            + " for the user '{0}' to email '{1}': {2}",
                        username,
                        email,
                        ex
                    );
                }
                resultMessage = Resources.FailedToSendPasswordResetToken;
                return;
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
                log.LogOrigamError(
                    string.Format(
                        "Failed to send password reset " + "mail for username '{0}', email '{1}'",
                        username,
                        email
                    ),
                    ex
                );
            }
            resultMessage = Resources.FailedToSendPassword;
            return;
        }
        finally
        {
            mail.Dispose();
        }
        if (log.IsDebugEnabled)
        {
            log.DebugFormat(
                "A new password for the user '{0}' " + "successfully generated and sent to '{1}'.",
                username,
                email
            );
        }
        resultMessage = Resources.PasswordResetMailSent;
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
    private MailMessage GenerateMail(
        string userEmail,
        string fromAddress,
        string templateFilename,
        string templateFromResources,
        string subjectFromConfig,
        string userLangIETF,
        List<KeyValuePair<string, string>> replacements
    )
    {
        MailMessage passwordRecoveryMail = new MailMessage(fromAddress, userEmail);
        string templateContent =
            (String.IsNullOrEmpty(templateFilename))
                ? templateFromResources
                : GetLocalizedMailTemplateText(templateFilename, userLangIETF);
        string[] subjectAndBody = processMailTemplate(templateContent, replacements);
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
    private string GetLocalizedMailTemplateText(string templateFilename, string languageIETF = "")
    {
        string filePath = Path.Combine(applicationBasePath, templateFilename);
        return File.ReadAllText(FindBestLocalizedFile(filePath, languageIETF)).Trim();
    }

    private static string[] processMailTemplate(
        string templateContent,
        List<KeyValuePair<string, string>> replacements
    )
    {
        string subject = null;
        if (templateContent.ToLower().StartsWith("subject:"))
        {
            subject = templateContent.Substring(8, templateContent.IndexOf('\n') - 8).Trim();
            foreach (KeyValuePair<string, string> replacement in replacements)
            {
                subject = subject.Replace(replacement.Key, replacement.Value);
            }
            templateContent = templateContent.Substring(templateContent.IndexOf('\n')).TrimStart();
        }
        foreach (KeyValuePair<string, string> replacement in replacements)
        {
            templateContent = templateContent.Replace(replacement.Key, replacement.Value);
        }
        return new string[] { subject, templateContent };
    }

    public static string FindBestLocalizedFile(string filePath, string languageIETF)
    {
        if (String.IsNullOrEmpty(languageIETF))
        {
            // language not sent, use current thread one
            languageIETF = System.Threading.Thread.CurrentThread.CurrentUICulture.IetfLanguageTag;
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
            candidate = String.Format(
                "{0}.{1}{2}",
                filePath.Substring(0, lastDotIndex),
                languageIETF,
                filePath.Substring(lastDotIndex)
            );
        }
        if (File.Exists(candidate))
            return candidate;
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
                candidate = String.Format("{0}.{1}", filePath, splittedIETF[0]);
            }
            else
            {
                candidate = String.Format(
                    "{0}.{1}{2}",
                    filePath.Substring(0, lastDotIndex),
                    splittedIETF[0],
                    filePath.Substring(lastDotIndex)
                );
            }
            if (File.Exists(candidate))
                return candidate;
        }
        // fallback
        return filePath;
    }

    private void SendMailByAWorkflow(MailMessage mail)
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
        if (!string.IsNullOrWhiteSpace(mailQueueName))
        {
            pms.Add(new QueryParameter("MailWorkQueueName", mailQueueName));
        }
#if DEBUG
        SaveToDebugMailLog(pms);
#endif
        WorkflowService.ExecuteWorkflow(
            new Guid("6e6d4e02-812a-4c95-afd1-eb2428802e2b"),
            pms,
            null
        );
    }

    private static void SaveToDebugMailLog(QueryParameterCollection pms)
    {
        string header =
            $"\n\n--------------------------------------------------------------------------------\n"
            + $"Sent:{DateTime.Now}\n";
        string mailString = pms.Cast<QueryParameter>()
            .Select(parameter => $"{parameter.Name}: {parameter.Value}")
            .Aggregate(header, (x, y) => x + "\n\n" + y);
        log.Info(mailString);
    }
}
