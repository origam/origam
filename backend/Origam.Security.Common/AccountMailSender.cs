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
    protected static readonly ILog log = LogManager.GetLogger(type: typeof(AccountMailSender));
    private static readonly Guid LANGUAGE_TAGIETF_LOOKUP = new Guid(
        g: "7823d8af-4968-48c3-a772-287475d429e1"
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
            new KeyValuePair<string, string>(
                key: "<%Token%>",
                value: Uri.EscapeDataString(stringToEscape: token)
            ),
            new KeyValuePair<string, string>(key: "<%UserId%>", value: userId),
            new KeyValuePair<string, string>(key: "<%UserName%>", value: username),
            new KeyValuePair<string, string>(key: "<%PortalBaseUrl%>", value: portalBaseUrl),
            new KeyValuePair<string, string>(
                key: "<%EscapedUserName%>",
                value: Uri.EscapeDataString(stringToEscape: username)
            ),
            new KeyValuePair<string, string>(key: "<%Name%>", value: name),
            new KeyValuePair<string, string>(
                key: "<%EscapedName%>",
                value: Uri.EscapeDataString(stringToEscape: name)
            ),
            new KeyValuePair<string, string>(key: "<%FirstName%>", value: firstName),
            new KeyValuePair<string, string>(
                key: "<%EscapedFirstName%>",
                value: Uri.EscapeDataString(stringToEscape: firstName)
            ),
            new KeyValuePair<string, string>(key: "<%UserEmail%>", value: email),
            new KeyValuePair<string, string>(
                key: "<%EscapedUserEmail%>",
                value: Uri.EscapeDataString(stringToEscape: email)
            ),
        };
        // PORTAL_BASE_URL is mandatory if using default template
        if (
            string.IsNullOrWhiteSpace(value: portalBaseUrl)
            && string.IsNullOrEmpty(value: registerNewUserFilename)
        )
        {
            log.Error(
                message: "'PortalBaseUrl' not configured while default template"
                    + "is used. Can't send a new registration email confirmation."
            );
            throw new Exception(message: Resources.RegisterNewUser_PortalBaseUrlNotConfigured);
        }
        MailMessage mail = null;
        string userLangIETF = System
            .Threading
            .Thread
            .CurrentThread
            .CurrentUICulture
            .IetfLanguageTag;
        using (LanguageSwitcher langSwitcher = new LanguageSwitcher(langIetf: userLangIETF))
        {
            mail = GenerateMail(
                userEmail: email,
                fromAddress: fromAddress,
                templateFilename: registerNewUserFilename,
                templateFromResources: Resources.RegisterNewUserTemplate,
                subjectFromConfig: registerNewUserSubject,
                userLangIETF: userLangIETF,
                replacements: replacements
            );
        }
        try
        {
            SendMailByAWorkflow(mail: mail);
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError(message: "Failed to send new user registration mail", ex: ex);
            }
            throw new Exception(message: Resources.FailedToSendNewUserRegistrationMail);
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
        string userLangIETF = ResolveIetfTagFromOrigamLanguageId(languageId: languageId);
        // build template replacements
        List<KeyValuePair<string, string>> replacements = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>(key: "<%UserName%>", value: username),
            new KeyValuePair<string, string>(key: "<%FirstNameAndName%>", value: firstNameAndName),
        };
        MailMessage userUnlockNotificationMail;
        using (LanguageSwitcher langSwitcher = new LanguageSwitcher(langIetf: userLangIETF))
        {
            try
            {
                userUnlockNotificationMail = GenerateMail(
                    userEmail: email,
                    fromAddress: fromAddress,
                    templateFilename: userUnlockNotificationBodyFilename,
                    templateFromResources: Resources.UserUnlockNotificationTemplate,
                    subjectFromConfig: userUnlockNotificationSubject,
                    userLangIETF: userLangIETF,
                    replacements: replacements
                );
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                {
                    log.ErrorFormat(
                        format: "Unlocking user: Failed to generate a mail"
                            + " for a user '{0}' to '{1}': {2}",
                        arg0: username,
                        arg1: email,
                        arg2: ex
                    );
                }
                throw;
            }
        }
        try
        {
            SendMailByAWorkflow(mail: userUnlockNotificationMail);
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.ErrorFormat(
                    format: "Unlocking user: Failed to send a mail"
                        + " for a user '{0}' to '{1}': {2}",
                    arg0: username,
                    arg1: email,
                    arg2: ex
                );
            }
            throw new Exception(message: Resources.FailedToSendUserUnlockNotification);
        }
        finally
        {
            userUnlockNotificationMail.Dispose();
        }
        if (log.IsDebugEnabled)
        {
            log.DebugFormat(
                format: "User '{0}' has been unlocked and the"
                    + " notification mail has been sent to '{1}'.",
                arg0: username,
                arg1: email
            );
        }
        return true;
    }

    private static string ResolveIetfTagFromOrigamLanguageId(string languageId)
    {
        IDataLookupService ls =
            ServiceManager.Services.GetService(serviceType: typeof(IDataLookupService))
            as IDataLookupService;
        string userLangIETF = "";
        if (!string.IsNullOrEmpty(value: languageId))
        {
            object ret = ls.GetDisplayText(
                lookupId: LANGUAGE_TAGIETF_LOOKUP,
                lookupValue: languageId,
                useCache: false,
                returnMessageIfNull: false,
                transactionId: null
            );
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
            new KeyValuePair<string, string>(
                key: "<%AuthenticationCode%>",
                value: Uri.EscapeDataString(stringToEscape: code)
            ),
            new KeyValuePair<string, string>(key: "<%UserEmail%>", value: email),
        };
        // PORTAL_BASE_URL is mandatory if using default template
        if (
            string.IsNullOrWhiteSpace(value: portalBaseUrl)
            && string.IsNullOrEmpty(value: registerNewUserFilename)
        )
        {
            log.Error(
                message: "'PortalBaseUrl' not configured while default template"
                    + "is used. Can't send a new registration email confirmation."
            );
            throw new Exception(message: Resources.RegisterNewUser_PortalBaseUrlNotConfigured);
        }
        MailMessage mail;
        string userLangIETF = System
            .Threading
            .Thread
            .CurrentThread
            .CurrentUICulture
            .IetfLanguageTag;
        using (new LanguageSwitcher(langIetf: userLangIETF))
        {
            mail = GenerateMail(
                userEmail: email,
                fromAddress: fromAddress,
                templateFilename: mfaTemplateFileName,
                templateFromResources: Resources.RegisterNewUserTemplate,
                subjectFromConfig: mfaSubject,
                userLangIETF: userLangIETF,
                replacements: replacements
            );
        }
        try
        {
            SendMailByAWorkflow(mail: mail);
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError(
                    message: "Failed to send multi factor authentication mail",
                    ex: ex
                );
            }
            throw new Exception(message: Resources.FailedToSendMultiFactorAuthCode);
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
        string userLangIETF = ResolveIetfTagFromOrigamLanguageId(languageId: languageId);
        if (userLangIETF == "")
        {
            userLangIETF = System.Threading.Thread.CurrentThread.CurrentUICulture.IetfLanguageTag;
        }
        var replacements = new List<KeyValuePair<string, string>>
        {
            new(key: "<%Token%>", value: Uri.EscapeDataString(stringToEscape: token)),
            new(key: "<%TokenValidityHours%>", value: tokenValidityHours.ToString()),
            new(key: "<%UserName%>", value: username),
            new(key: "<%EscapedUserName%>", value: Uri.EscapeDataString(stringToEscape: username)),
            new(key: "<%Name%>", value: name),
            new(key: "<%EscapedName%>", value: Uri.EscapeDataString(stringToEscape: name)),
            new(key: "<%UserEmail%>", value: email),
            new(key: "<%EscapedUserEmail%>", value: Uri.EscapeDataString(stringToEscape: email)),
            new(key: "<%PortalBaseUrl%>", value: portalBaseUrl),
            new(key: "<%ReturnUrl%>", value: Uri.EscapeDataString(stringToEscape: returnUrl)),
        };
        if (firstName != null)
        {
            replacements.AddRange(
                collection: new List<KeyValuePair<string, string>>
                {
                    new(key: "<%FirstName%>", value: firstName),
                    new(
                        key: "<%EscapedFirstName%>",
                        value: Uri.EscapeDataString(stringToEscape: firstName)
                    ),
                }
            );
        }
        // PORTAL_BASE_URL is mandatory if using default template
        if (string.IsNullOrWhiteSpace(value: portalBaseUrl))
        {
            log.Error(
                message: "'PortalBaseUrl' not configured while default template"
                    + "is used. Can't send a password reset email."
            );
            throw new Exception(message: Resources.ResetPasswordMail_PortalBaseUrlNotConfigured);
        }
        if (
            string.IsNullOrEmpty(value: ResetPasswordSubject)
            || string.IsNullOrEmpty(value: ResetPasswordBodyFilename)
        )
        {
            log.Error(
                message: "'ResetPasswordMailSubject' or 'ResetPasswordMailBodyFileName' "
                    + "not configured while template"
                    + "is used for specific language. Can't send a password reset email."
            );
            throw new Exception(message: Resources.ResetPasswordMail_PortalBaseUrlNotConfigured);
        }
        MailMessage mail = null;
        using (LanguageSwitcher langSwitcher = new LanguageSwitcher(langIetf: userLangIETF))
        {
            try
            {
                mail = GenerateMail(
                    userEmail: email,
                    fromAddress: fromAddress,
                    templateFilename: ResetPasswordBodyFilename,
                    templateFromResources: Resources.ResetPasswordMailTemplate,
                    subjectFromConfig: ResetPasswordSubject,
                    userLangIETF: userLangIETF,
                    replacements: replacements
                );
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                {
                    log.ErrorFormat(
                        format: "Failed to generate a password reset mail "
                            + " for the user '{0}' to email '{1}': {2}",
                        arg0: username,
                        arg1: email,
                        arg2: ex
                    );
                }
                resultMessage = Resources.FailedToSendPasswordResetToken;
                return;
            }
        }
        try
        {
            SendMailByAWorkflow(mail: mail);
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError(
                    message: string.Format(
                        format: "Failed to send password reset "
                            + "mail for username '{0}', email '{1}'",
                        arg0: username,
                        arg1: email
                    ),
                    ex: ex
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
                format: "A new password for the user '{0}' "
                    + "successfully generated and sent to '{1}'.",
                arg0: username,
                arg1: email
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
        MailMessage passwordRecoveryMail = new MailMessage(from: fromAddress, to: userEmail);
        string templateContent =
            (String.IsNullOrEmpty(value: templateFilename))
                ? templateFromResources
                : GetLocalizedMailTemplateText(
                    templateFilename: templateFilename,
                    languageIETF: userLangIETF
                );
        string[] subjectAndBody = processMailTemplate(
            templateContent: templateContent,
            replacements: replacements
        );
        passwordRecoveryMail.Subject = subjectAndBody[0];
        passwordRecoveryMail.Body = subjectAndBody[1];
        if (string.IsNullOrWhiteSpace(value: passwordRecoveryMail.Subject))
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
        string filePath = Path.Combine(path1: applicationBasePath, path2: templateFilename);
        return File.ReadAllText(
                path: FindBestLocalizedFile(filePath: filePath, languageIETF: languageIETF)
            )
            .Trim();
    }

    private static string[] processMailTemplate(
        string templateContent,
        List<KeyValuePair<string, string>> replacements
    )
    {
        string subject = null;
        if (templateContent.ToLower().StartsWith(value: "subject:"))
        {
            subject = templateContent
                .Substring(startIndex: 8, length: templateContent.IndexOf(value: '\n') - 8)
                .Trim();
            foreach (KeyValuePair<string, string> replacement in replacements)
            {
                subject = subject.Replace(oldValue: replacement.Key, newValue: replacement.Value);
            }
            templateContent = templateContent
                .Substring(startIndex: templateContent.IndexOf(value: '\n'))
                .TrimStart();
        }
        foreach (KeyValuePair<string, string> replacement in replacements)
        {
            templateContent = templateContent.Replace(
                oldValue: replacement.Key,
                newValue: replacement.Value
            );
        }
        return new string[] { subject, templateContent };
    }

    public static string FindBestLocalizedFile(string filePath, string languageIETF)
    {
        if (String.IsNullOrEmpty(value: languageIETF))
        {
            // language not sent, use current thread one
            languageIETF = System.Threading.Thread.CurrentThread.CurrentUICulture.IetfLanguageTag;
        }
        // find the last '.'
        int lastDotIndex = filePath.LastIndexOf(value: '.');
        // create a localized file candidate ( password_reset.de-DE.txt )
        /*
            password_reset.txt -> password_reset.de-DE.txt
            password_reset -> password_reset.de-DE
         */
        string candidate;
        if (lastDotIndex == -1)
        {
            // dot not found
            candidate = String.Format(format: "{0}.{1}", arg0: filePath, arg1: languageIETF);
        }
        else
        {
            candidate = String.Format(
                format: "{0}.{1}{2}",
                arg0: filePath.Substring(startIndex: 0, length: lastDotIndex),
                arg1: languageIETF,
                arg2: filePath.Substring(startIndex: lastDotIndex)
            );
        }
        if (File.Exists(path: candidate))
        {
            return candidate;
        }
        // try better
        /*
            password_reset.txt -> password_reset.de.txt
            password_reset -> password_reset.de
         */
        string[] splittedIETF = languageIETF.Split(separator: '-');
        if (splittedIETF.Length == 2)
        {
            if (lastDotIndex == -1)
            {
                candidate = String.Format(format: "{0}.{1}", arg0: filePath, arg1: splittedIETF[0]);
            }
            else
            {
                candidate = String.Format(
                    format: "{0}.{1}{2}",
                    arg0: filePath.Substring(startIndex: 0, length: lastDotIndex),
                    arg1: splittedIETF[0],
                    arg2: filePath.Substring(startIndex: lastDotIndex)
                );
            }
            if (File.Exists(path: candidate))
            {
                return candidate;
            }
        }
        // fallback
        return filePath;
    }

    private void SendMailByAWorkflow(MailMessage mail)
    {
        // send mail - by a workflow located at root package
        QueryParameterCollection pms = new QueryParameterCollection();
        pms.Add(value: new QueryParameter(_parameterName: "subject", value: mail.Subject));
        pms.Add(value: new QueryParameter(_parameterName: "body", value: mail.Body));
        pms.Add(
            value: new QueryParameter(
                _parameterName: "recipientEmail",
                value: mail.To.First().Address
            )
        );
        pms.Add(value: new QueryParameter(_parameterName: "senderEmail", value: mail.From.Address));
        if (!string.IsNullOrWhiteSpace(value: mail.From.DisplayName))
        {
            pms.Add(
                value: new QueryParameter(
                    _parameterName: "senderName",
                    value: mail.From.DisplayName
                )
            );
        }
        if (!string.IsNullOrWhiteSpace(value: mailQueueName))
        {
            pms.Add(
                value: new QueryParameter(_parameterName: "MailWorkQueueName", value: mailQueueName)
            );
        }
#if DEBUG
        SaveToDebugMailLog(pms: pms);
#endif
        WorkflowService.ExecuteWorkflow(
            workflowId: new Guid(g: "6e6d4e02-812a-4c95-afd1-eb2428802e2b"),
            parameters: pms,
            transactionId: null
        );
    }

    private static void SaveToDebugMailLog(QueryParameterCollection pms)
    {
        string header =
            $"\n\n--------------------------------------------------------------------------------\n"
            + $"Sent:{DateTime.Now}\n";
        string mailString = pms.Cast<QueryParameter>()
            .Select(selector: parameter => $"{parameter.Name}: {parameter.Value}")
            .Aggregate(seed: header, func: (x, y) => x + "\n\n" + y);
        log.Info(message: mailString);
    }
}
