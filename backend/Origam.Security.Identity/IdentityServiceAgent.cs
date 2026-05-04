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
using System.Threading.Tasks;
using System.Xml;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Origam.Security.Common;
using Origam.Service.Core;
using Origam.Workflow;

namespace Origam.Security.Identity;

// class is sealed because of a simplified IDisposable pattern implementation
public sealed class IdentityServiceAgent : AbstractServiceAgent, IDisposable
{
    private static readonly ILog log = LogManager.GetLogger(type: typeof(IdentityServiceAgent));
    private IManager userManager;
    private IServiceScope serviceScope;

    public IdentityServiceAgent()
    {
        // according to
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1
        // we can get scoped RequestServices collection from HttpContext
        userManager = SecurityManager
            .DIServiceProvider.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()
            .HttpContext?.RequestServices?.GetService<IManager>();
        if (userManager == null)
        {
            serviceScope = SecurityManager.DIServiceProvider.CreateScope();
            userManager = serviceScope.ServiceProvider.GetService<IManager>();
        }
    }

    private object result;
    public override object Result
    {
        get
        {
            object temp = result;
            result = null;
            return temp;
        }
    }

    public override void Run()
    {
        switch (this.MethodName)
        {
            case "GetUserData":
            {
                GetUserData();
                break;
            }

            case "ChangePasswordAnswerAndQuestion":
            {
                ChangeUserPasswordQuestionAndAnswer();
                break;
            }

            case "UnlockUser":
            {
                UnlockUser();
                break;
            }

            case "ChangePassword":
            {
                ChangePassword();
                break;
            }

            case "ResetPassword":
            {
                ResetPassword();
                break;
            }

            case "GetPasswordAttributes":
            {
                GetPasswordAttributes();
                break;
            }

            case "DeleteUser":
            {
                DeleteUser();
                break;
            }

            case "UpdateUser":
            {
                UpdateUser();
                break;
            }

            case "CreateUser":
            {
                CreateUser();
                break;
            }

            case "SendEmailConfirmationToken":
            {
                SendEmailConfirmationToken();
                break;
            }

            case "ConfirmEmail":
            {
                ConfirmEmail();
                break;
            }

            case "ForceConfirmEmail":
            {
                ForceConfirmEmail();
                break;
            }

            case "GetEmailConfirmationToken":
            {
                GetEmailConfirmationToken();
                break;
            }

            case "GetPasswordResetToken":
            {
                GetPasswordResetToken();
                break;
            }

            case "GetPasswordResetTokenFromEmail":
            {
                GetPasswordResetTokenFromEmail();
                break;
            }

            case "IsEmailConfirmed":
            {
                IsEmailConfirmed();
                break;
            }

            case "IsLockedOut":
            {
                IsLockedOut();
                break;
            }

            case "Is2FAEnforced":
            {
                Is2FAEnforced();
                break;
            }

            case "Set2FAEnforcement":
            {
                Set2FAEnforcement();
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "MethodName",
                    actualValue: this.MethodName,
                    message: Origam.Workflow.ResourceUtils.GetString(key: "InvalidMethodName")
                );
            }
        }
    }

    private void GetUserData()
    {
        if (!(Parameters[key: "Username"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorUsernameNotString);
        }
        Task<IOrigamUser> task = userManager.FindByNameAsync(
            name: Parameters[key: "Username"].ToString(),
            transactionId: TransactionId
        );
        if (task.IsFaulted)
        {
            throw task.Exception;
        }

        result = GetUserDataXml(user: task.Result);
    }

    private void ChangeUserPasswordQuestionAndAnswer()
    {
        if (!(Parameters[key: "Username"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorUsernameNotString);
        }
        if (!(Parameters[key: "Password"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorPasswordNotString);
        }
        if (!(Parameters[key: "NewQuestion"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorNewQuestionNotString);
        }
        if (!(Parameters[key: "NewAnswer"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorNewAnswerNotString);
        }
        Task<bool> task = userManager.ChangePasswordQuestionAndAnswerAsync(
            userName: Parameters[key: "Username"].ToString(),
            password: Parameters[key: "Password"].ToString().TrimEnd(),
            question: Parameters[key: "NewQuestion"].ToString(),
            answer: Parameters[key: "NewAnswer"].ToString()
        );
        if (task.IsFaulted)
        {
            throw task.Exception;
        }

        result = task.Result;
    }

    private void IsLockedOut()
    {
        if (!(Parameters[key: "UserId"] is System.Guid))
        {
            throw new InvalidCastException(message: Resources.ErrorUserIdNotGuid);
        }
        Task<bool> task = userManager.IsLockedOutAsync(
            userId: Parameters[key: "UserId"].ToString()
        );
        if (task.IsFaulted)
        {
            throw task.Exception;
        }

        result = task.Result;
    }

    private void Is2FAEnforced()
    {
        if (!(Parameters[key: "UserId"] is System.Guid))
        {
            throw new InvalidCastException(message: Resources.ErrorUserIdNotGuid);
        }
        Task<bool> task = userManager.GetTwoFactorEnabledAsync(
            userId: Parameters[key: "UserId"].ToString()
        );
        if (task.IsFaulted)
        {
            throw task.Exception;
        }

        result = task.Result;
    }

    private void Set2FAEnforcement()
    {
        if (!(Parameters[key: "UserId"] is System.Guid))
        {
            throw new InvalidCastException(message: Resources.ErrorUserIdNotGuid);
        }
        if (!(Parameters[key: "Enforce"] is Boolean))
        {
            throw new InvalidCastException(message: Resources.ErrorEnforceNotBool);
        }
        Task<bool> task = userManager.SetTwoFactorEnabledAsync(
            userId: Parameters[key: "UserId"].ToString(),
            enabled: (Boolean)Parameters[key: "Enforce"]
        );
        if (task.IsFaulted)
        {
            throw task.Exception;
        }

        result = true;
    }

    private void IsEmailConfirmed()
    {
        if (!(Parameters[key: "UserId"] is System.Guid))
        {
            throw new InvalidCastException(message: Resources.ErrorUserIdNotGuid);
        }
        Task<bool> task = userManager.IsEmailConfirmedAsync(
            userId: Parameters[key: "UserId"].ToString()
        );
        if (task.IsFaulted)
        {
            throw task.Exception;
        }

        result = task.Result;
    }

    private void UnlockUser()
    {
        if (!(Parameters[key: "Username"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorUsernameNotString);
        }
        Task<bool> task = userManager.UnlockUserAsync(
            userName: Parameters[key: "Username"].ToString()
        );
        if (task.IsFaulted)
        {
            throw task.Exception;
        }

        result = true;
    }

    private void ForceConfirmEmail()
    {
        if (!(Parameters[key: "UserId"] is System.Guid))
        {
            throw new InvalidCastException(message: Resources.ErrorUserIdNotGuid);
        }
        Task<InternalIdentityResult> task = userManager.ConfirmEmailAsync(
            userId: Parameters[key: "UserId"].ToString()
        );
        RuleException ex = new RuleException();
        if (task.IsFaulted)
        {
            RuleExceptionData rd = new RuleExceptionData();
            rd.Severity = RuleExceptionSeverity.High;
            rd.EntityName = "";
            rd.FieldName = "";
            rd.Message = task.Exception.Message;
            ex.RuleResult.Add(value: rd);
            throw ex;
        }

        if (!task.Result.Succeeded)
        {
            RuleExceptionData rd2 = new RuleExceptionData();
            foreach (object o in task.Result.Errors)
            {
                RuleExceptionData rd = new RuleExceptionData();
                rd2.Severity = RuleExceptionSeverity.High;
                rd2.EntityName = "";
                rd2.FieldName = "";
                rd2.Message = (string)o;
                ex.RuleResult.Add(value: rd2);
            }
            throw ex;
        }

        result = true;
    }

    private void ConfirmEmail()
    {
        if (!(Parameters[key: "UserId"] is System.Guid))
        {
            throw new InvalidCastException(message: Resources.ErrorUserIdNotGuid);
        }
        if (!(Parameters[key: "Token"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorTokenNotString);
        }
        Task<InternalIdentityResult> task = userManager.ConfirmEmailAsync(
            userId: Parameters[key: "UserId"].ToString(),
            token: Parameters[key: "Token"].ToString()
        );
        RuleException ex = new RuleException();
        if (task.IsFaulted)
        {
            RuleExceptionData rd = new RuleExceptionData();
            rd.Severity = RuleExceptionSeverity.High;
            rd.EntityName = "";
            rd.FieldName = "";
            rd.Message = task.Exception.Message;
            ex.RuleResult.Add(value: rd);
            throw ex;
        }

        if (!task.Result.Succeeded)
        {
            RuleExceptionData rd2 = new RuleExceptionData();
            foreach (object o in task.Result.Errors)
            {
                RuleExceptionData rd = new RuleExceptionData();
                rd2.Severity = RuleExceptionSeverity.High;
                rd2.EntityName = "";
                rd2.FieldName = "";
                rd2.Message = (string)o;
                ex.RuleResult.Add(value: rd2);
            }
            throw ex;
        }

        result = true;
    }

    private void ChangePassword()
    {
        if (!(Parameters[key: "Username"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorUsernameNotString);
        }
        if (!(Parameters[key: "OldPassword"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorOldPasswordNotString);
        }
        if (!(Parameters[key: "NewPassword"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorNewPasswordNotString);
        }
        IOrigamUser user = FindUser();
        Task<InternalIdentityResult> task = userManager.ChangePasswordAsync(
            userId: user.BusinessPartnerId,
            currentPassword: Parameters[key: "OldPassword"].ToString().TrimEnd(),
            newPassword: Parameters[key: "NewPassword"].ToString().TrimEnd()
        );
        if (task.IsFaulted)
        {
            throw task.Exception;
        }

        if (!task.Result.Succeeded)
        {
            throw new Exception(message: string.Join(separator: " ", values: task.Result.Errors));
        }

        result = true;
    }

    private IOrigamUser FindUser()
    {
        IOrigamUser user = null;
        Task<IOrigamUser> taskFindUser = userManager.FindByNameAsync(
            name: Parameters[key: "Username"].ToString(),
            transactionId: TransactionId
        );
        if (taskFindUser.IsFaulted)
        {
            throw taskFindUser.Exception;
        }

        user = taskFindUser.Result;
        if (user == null)
        {
            throw new Exception(message: Resources.ErrorUserNotFound);
        }
        return user;
    }

    private void ResetPassword()
    {
        if (!(Parameters[key: "UserName"] is String))
        {
            throw new InvalidCastException(message: Resources.ErrorUsernameNotString);
        }
        if (!(Parameters[key: "Token"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorTokenNotString);
        }
        if (!(Parameters[key: "NewPassword"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorNewPasswordNotString);
        }
        Task<InternalIdentityResult> resetPasswordTask = userManager.ResetPasswordFromUsernameAsync(
            userName: Parameters[key: "UserName"].ToString(),
            token: Parameters[key: "Token"].ToString(),
            newPassword: Parameters[key: "NewPassword"].ToString().TrimEnd()
        );
        if (resetPasswordTask.IsFaulted)
        {
            throw resetPasswordTask.Exception;
        }

        if (!resetPasswordTask.Result.Succeeded)
        {
            //throw new Exception(string.Join(" ", resetPasswordTask.Result.Errors));
            RuleExceptionDataCollection reCol = new RuleExceptionDataCollection();
            foreach (string err in resetPasswordTask.Result.Errors)
            {
                reCol.Add(value: new RuleExceptionData(message: err));
            }
            throw new RuleException(result: reCol);
        }

        result = true;
    }

    private void DeleteUser()
    {
        if (!(Parameters[key: "Username"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorUsernameNotString);
        }
        IOrigamUser user = FindUser();
        user.TransactionId = TransactionId;
        Task<InternalIdentityResult> task = userManager.DeleteAsync(user: user);
        if (task.IsFaulted)
        {
            throw task.Exception;
        }
        result = task.Result.Succeeded;

        OrigamUserContext.Reset(username: user.UserName);
    }

    private void UpdateUser()
    {
        // Check input parameters
        if (!(Parameters[key: "Username"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorUsernameNotString);
        }
        if (Parameters.ContainsKey(key: "Email") && !(Parameters[key: "Email"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorEmailNotString);
        }
        if (
            Parameters.ContainsKey(key: "IsApproved") && !(Parameters[key: "IsApproved"] is Boolean)
        )
        {
            throw new InvalidCastException(message: Resources.ErrorIsApprovedNotBool);
        }
        IOrigamUser user = FindUser();
        user.Email = Parameters.ContainsKey(key: "Email")
            ? Parameters[key: "Email"].ToString()
            : null;
        user.IsApproved = Parameters.ContainsKey(key: "IsApproved")
            ? (Boolean)Parameters[key: "IsApproved"]
            : false;
        user.TransactionId = TransactionId;
        Task<InternalIdentityResult> task = userManager.UpdateAsync(user: user);
        if (task.IsFaulted)
        {
            throw task.Exception;
        }

        result = task.Result.Succeeded;
    }

    private void SendEmailConfirmationToken()
    {
        // Check input parameters
        if (!(Parameters[key: "Username"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorUsernameNotString);
        }
        try
        {
            userManager.SendNewUserToken(userName: (string)this.Parameters[key: "Username"]);
        }
        catch (Exception e)
        {
            if (log.IsErrorEnabled)
            {
                log.ErrorFormat(format: "Can't send a confirmation email: {0}", arg0: e);
            }
            // convert to rule exception
            RuleExceptionData red = new RuleExceptionData();
            red.Message = e.Message;
            red.Severity = RuleExceptionSeverity.High;
            red.EntityName = "SendEmailConfirmationTokenError";
            red.FieldName = "";
            throw new RuleException(result: new RuleExceptionDataCollection() { red });
        }
        result = true;
    }

    private void CreateUser()
    {
        // Check input parameters
        if (!(Parameters[key: "Username"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorUsernameNotString);
        }
        if (Parameters.ContainsKey(key: "Password") && !(Parameters[key: "Password"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorPasswordNotString);
        }
        if (!(Parameters[key: "Email"] is string))
        {
            throw new InvalidCastException(message: Resources.ErrorEmailNotString);
        }
        if (
            Parameters.ContainsKey(key: "ProviderUserKey")
            && !(Parameters[key: "ProviderUserKey"] is System.Guid)
        )
        {
            throw new InvalidCastException(message: Resources.ErrorProviderUserKeyNotGuid);
        }
        if (
            Parameters.ContainsKey(key: "PasswordQuestion")
            && !(Parameters[key: "PasswordQuestion"] is string)
        )
        {
            throw new InvalidCastException(message: Resources.ErrorPasswordQuestionNotString);
        }
        if (
            Parameters.ContainsKey(key: "PasswordAnswer")
            && !(Parameters[key: "PasswordAnswer"] is string)
        )
        {
            throw new InvalidCastException(message: Resources.ErrorPasswordAnswerNotString);
        }
        bool emailConfirmed = false;
        if (Parameters.ContainsKey(key: "EmailConfirmed"))
        {
            if (!(Parameters[key: "EmailConfirmed"] is bool))
            {
                throw new InvalidCastException(message: Resources.ErrorEmailConfirmedNotBool);
            }

            emailConfirmed = (bool)Parameters[key: "EmailConfirmed"];
        }
        IOrigamUser user = userManager.CreateUserObject(
            userName: Parameters[key: "Username"].ToString()
        );
        user.Email = Parameters[key: "Email"].ToString();
        if (Parameters.ContainsKey(key: "PasswordQuestion"))
        {
            user.PasswordQuestion = Parameters[key: "PasswordQuestion"].ToString();
        }
        if (Parameters.ContainsKey(key: "PasswordAnswer"))
        {
            user.PasswordAnswer = Parameters[key: "PasswordAnswer"].ToString();
        }
        if (Parameters.ContainsKey(key: "ProviderUserKey"))
        {
            user.ProviderUserKey = (Guid)Parameters[key: "ProviderUserKey"];
        }
        user.EmailConfirmed = emailConfirmed;
        user.TransactionId = TransactionId;
        Task<InternalIdentityResult> task = userManager.CreateAsync(
            user: user,
            password: Parameters[key: "Password"].ToString().TrimEnd()
        );
        if (task.IsFaulted)
        {
            throw task.Exception;
        }

        if (!task.Result.Succeeded)
        {
            throw new OrigamValidationException(
                message: string.Join(separator: " ", values: task.Result.Errors)
            );
        }
        OrigamUserContext.Reset(username: Parameters[key: "Username"].ToString());
        result = user.UserName;
    }

    private void GetEmailConfirmationToken()
    {
        if (!(Parameters[key: "UserId"] is System.Guid))
        {
            throw new InvalidCastException(message: Resources.ErrorUserIdNotGuid);
        }
        Task<string> task = userManager.GenerateEmailConfirmationTokenAsync(
            userId: Parameters[key: "UserId"].ToString()
        );
        if (task.IsFaulted)
        {
            throw task.Exception;
        }

        result = task.Result;
    }

    private void GetPasswordResetTokenFromEmail()
    {
        if (!(Parameters[key: "Email"] is System.String))
        {
            throw new InvalidCastException(message: Resources.ErrorEmailNotString);
        }
        Task<TokenResult> generateTask = userManager.GetPasswordResetTokenFromEmailAsync(
            email: (string)Parameters[key: "Email"]
        );
        if (generateTask.IsFaulted)
        {
            throw generateTask.Exception;
        }
        // render xml
        XmlDocument xmlDoc = new XmlDocument();
        XmlNode rootNode = xmlDoc.CreateElement(name: "ROOT");
        xmlDoc.AppendChild(newChild: rootNode);
        XmlNode tokenResultNode = xmlDoc.CreateElement(name: "TokenResult");
        // token
        XmlAttribute tokenAttr = xmlDoc.CreateAttribute(name: "Token");
        tokenAttr.Value = generateTask.Result.Token;
        tokenResultNode.Attributes.Append(node: tokenAttr);
        // userId
        XmlAttribute userNameAttr = xmlDoc.CreateAttribute(name: "UserName");
        userNameAttr.Value = generateTask.Result.UserName.ToString();
        tokenResultNode.Attributes.Append(node: userNameAttr);
        // tokenValidityHours
        XmlAttribute tokenValidityAttr = xmlDoc.CreateAttribute(name: "TokenValidityHours");
        tokenValidityAttr.Value = generateTask.Result.TokenValidityHours.ToString();
        tokenResultNode.Attributes.Append(node: tokenValidityAttr);
        // errorMessage
        XmlAttribute errorMessageAttr = xmlDoc.CreateAttribute(name: "ErrorMessage");
        errorMessageAttr.Value = generateTask.Result.ErrorMessage;
        tokenResultNode.Attributes.Append(node: errorMessageAttr);
        rootNode.AppendChild(newChild: tokenResultNode);

        result = new XmlContainer(xmlDocument: xmlDoc);
    }

    private void GetPasswordResetToken()
    {
        if (!(Parameters[key: "UserId"] is System.Guid))
        {
            throw new InvalidCastException(message: Resources.ErrorUserIdNotGuid);
        }
        Task<string> task = userManager.GeneratePasswordResetTokenAsync(
            userId: Parameters[key: "UserId"].ToString()
        );
        if (task.IsFaulted)
        {
            throw task.Exception;
        }

        result = task.Result;
    }

    private void GetPasswordAttributes()
    {
        Task<XmlDocument> task = userManager.GetPasswordAttributesAsync();
        if (task.IsFaulted)
        {
            throw task.Exception;
        }

        result = task.Result;
    }

    private XmlDocument GetUserDataXml(IOrigamUser user)
    {
        XmlDocument xmlDoc = new System.Xml.XmlDocument();
        XmlNode root = xmlDoc.CreateElement(name: "ROOT");
        XmlNode userData = xmlDoc.CreateElement(name: "UserData");
        XmlAttribute creationDate = xmlDoc.CreateAttribute(name: "CreationDate");
        creationDate.Value = user.CreationDate.ToString();
        userData.Attributes.Append(node: creationDate);
        XmlAttribute email = xmlDoc.CreateAttribute(name: "Email");
        email.Value = user.Email.ToString();
        userData.Attributes.Append(node: email);
        XmlAttribute isApproved = xmlDoc.CreateAttribute(name: "IsApproved");
        isApproved.Value = user.IsApproved.ToString();
        userData.Attributes.Append(node: isApproved);
        XmlAttribute isOnline = xmlDoc.CreateAttribute(name: "IsOnline");
        userData.Attributes.Append(node: isOnline);
        isOnline.Value = user.IsOnline.ToString();
        XmlAttribute lastActivityDate = xmlDoc.CreateAttribute(name: "LastActivityDate");
        lastActivityDate.Value = user.LastActivityDate.ToString();
        userData.Attributes.Append(node: lastActivityDate);
        XmlAttribute lastLockoutDate = xmlDoc.CreateAttribute(name: "LastLockoutDate");
        lastLockoutDate.Value = user.LastLockoutDate.ToString();
        userData.Attributes.Append(node: lastLockoutDate);
        XmlAttribute lastLoginDate = xmlDoc.CreateAttribute(name: "LastLoginDate");
        lastLoginDate.Value = user.LastLoginDate.ToString();
        userData.Attributes.Append(node: lastLoginDate);
        XmlAttribute lastPasswordChangedDate = xmlDoc.CreateAttribute(
            name: "LastPasswordChangedDate"
        );
        lastPasswordChangedDate.Value = user.LastPasswordChangedDate.ToString();
        userData.Attributes.Append(node: lastPasswordChangedDate);
        XmlAttribute passwordQuestion = xmlDoc.CreateAttribute(name: "PasswordQuestion");
        passwordQuestion.Value = user.PasswordQuestion;
        userData.Attributes.Append(node: passwordQuestion);
        root.AppendChild(newChild: userData);
        xmlDoc.AppendChild(newChild: root);
        return xmlDoc;
    }

    public void Dispose() => serviceScope?.Dispose();
}
