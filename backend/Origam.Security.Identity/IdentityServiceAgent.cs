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
using Origam.Workflow;
using Origam.Rule;
using log4net;
using Origam.Security.Common;
using Microsoft.Extensions.DependencyInjection;
using Origam.Service.Core;

namespace Origam.Security.Identity;
// class is sealed because of a simplified IDisposable pattern implementation
public sealed class IdentityServiceAgent : AbstractServiceAgent, IDisposable
{
    private static readonly ILog log
		= LogManager.GetLogger(typeof(IdentityServiceAgent));
    private IManager userManager;
    private IServiceScope serviceScope;
    public IdentityServiceAgent()
    {
        // according to
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1
        // we can get scoped RequestServices collection from HttpContext
        userManager = SecurityManager.DIServiceProvider
            .GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()
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
                GetUserData();
                break;
            case "ChangePasswordAnswerAndQuestion":
                ChangeUserPasswordQuestionAndAnswer();
                break;
            case "UnlockUser":
                UnlockUser();
                break;
            case "ChangePassword":
                ChangePassword();
                break;
            case "ResetPassword":
                ResetPassword();
                break;
            case "GetPasswordAttributes":
                GetPasswordAttributes();
                break;
            case "DeleteUser":
                DeleteUser();
                break;
            case "UpdateUser":
                UpdateUser();
                break;
            case "CreateUser":
                CreateUser();
                break;
			case "SendEmailConfirmationToken":
				SendEmailConfirmationToken();
				break;
            case "ConfirmEmail":
                ConfirmEmail();
                break;
            case "ForceConfirmEmail":
                ForceConfirmEmail();
                break;
            case "GetEmailConfirmationToken":
                GetEmailConfirmationToken();
                break;
            case "GetPasswordResetToken":
                GetPasswordResetToken();
                break;
            case "GetPasswordResetTokenFromEmail":
                GetPasswordResetTokenFromEmail();
                break;
            case "IsEmailConfirmed":
                IsEmailConfirmed();
                break;
            case "IsLockedOut":
                IsLockedOut();
                break;
            case "Is2FAEnforced":
                Is2FAEnforced();
                break;
            case "Set2FAEnforcement":
                Set2FAEnforcement();
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    "MethodName", this.MethodName,
                    Origam.Workflow.ResourceUtils.GetString(
                    "InvalidMethodName"));
        }
    }
    private void GetUserData()
    {
        if (!(Parameters["Username"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorUsernameNotString);
        }
        Task<IOrigamUser> task = userManager.FindByNameAsync(
            Parameters["Username"].ToString(), TransactionId);
        if (task.IsFaulted)
        {
            throw task.Exception;
        }
        else
        {
            result = GetUserDataXml(task.Result);
        }
    }
    private void ChangeUserPasswordQuestionAndAnswer()
    {
        if (!(Parameters["Username"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorUsernameNotString);
        }
        if (!(Parameters["Password"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorPasswordNotString);
        }
        if (!(Parameters["NewQuestion"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorNewQuestionNotString);
        }
        if (!(Parameters["NewAnswer"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorNewAnswerNotString);
        }
        Task<bool> task = userManager.ChangePasswordQuestionAndAnswerAsync(
            Parameters["Username"].ToString(),
            Parameters["Password"].ToString().TrimEnd(),
            Parameters["NewQuestion"].ToString(),
            Parameters["NewAnswer"].ToString());
        if (task.IsFaulted)
        {
            throw task.Exception;
        }
        else
        {
            result = task.Result;
        }
    }
    
    private void IsLockedOut()
    {
        if (!(Parameters["UserId"] is System.Guid))
        {
            throw new InvalidCastException(
                Resources.ErrorUserIdNotGuid);
        }
        Task<bool> task = userManager.IsLockedOutAsync(
            Parameters["UserId"].ToString());
        if (task.IsFaulted)
        {
            throw task.Exception;
        }
        else
        {
            result = task.Result;
        }
    }
    private void Is2FAEnforced()
    {
        if (!(Parameters["UserId"] is System.Guid))
        {
            throw new InvalidCastException(
                Resources.ErrorUserIdNotGuid);
        }
        Task<bool> task = userManager.GetTwoFactorEnabledAsync(
            Parameters["UserId"].ToString());
        if (task.IsFaulted)
        {
            throw task.Exception;
        }
        else
        {
            result = task.Result;
        }
    }
    private void Set2FAEnforcement()
    {
        if (!(Parameters["UserId"] is System.Guid))
        {
            throw new InvalidCastException(
                Resources.ErrorUserIdNotGuid);
        }
        if (!(Parameters["Enforce"] is Boolean))
        {
            throw new InvalidCastException(
                Resources.ErrorEnforceNotBool);
        }
        Task<bool> task = userManager.SetTwoFactorEnabledAsync(
           Parameters["UserId"].ToString(), (Boolean) Parameters["Enforce"]);
        if (task.IsFaulted)
        {
            throw task.Exception;
        }
        else
        {
            result = true;
        }
    }
    
    private void IsEmailConfirmed()
    {
        if (!(Parameters["UserId"] is System.Guid))
        {
            throw new InvalidCastException(
                Resources.ErrorUserIdNotGuid);
        }
        Task<bool> task = userManager.IsEmailConfirmedAsync(
            Parameters["UserId"].ToString());
        if (task.IsFaulted)
        {
            throw task.Exception;
        }
        else
        {
            result = task.Result;
        }
    }
    private void UnlockUser()
    {
        if (!(Parameters["Username"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorUsernameNotString);
        }
        Task<bool> task = userManager.UnlockUserAsync(
            Parameters["Username"].ToString());
        if (task.IsFaulted)
        {
            throw task.Exception;
        }
        else
        {
            result = true;
        }
    }
    
    private void ForceConfirmEmail()
    {
        if (!(Parameters["UserId"] is System.Guid))
        {
            throw new InvalidCastException(
                Resources.ErrorUserIdNotGuid);
        }
        Task<InternalIdentityResult> task = userManager.ConfirmEmailAsync(
            Parameters["UserId"].ToString());
		RuleException ex = new RuleException();
        if (task.IsFaulted)
        {
			RuleExceptionData rd = new RuleExceptionData();                    
			rd.Severity = RuleExceptionSeverity.High;
			rd.EntityName = "";
			rd.FieldName = "";
			rd.Message = task.Exception.Message;
			ex.RuleResult.Add(rd);       
            throw ex;
        }
        else if (!task.Result.Succeeded)
        {
			RuleExceptionData rd2 = new RuleExceptionData();                    
			foreach (object o in task.Result.Errors) {
				RuleExceptionData rd = new RuleExceptionData();                    
				rd2.Severity = RuleExceptionSeverity.High;
				rd2.EntityName = "";
				rd2.FieldName = "";
				rd2.Message = (string) o;
				ex.RuleResult.Add(rd2);
			}
			throw ex;
        }
        else
        {
            result = true;
        }
    }
    
    private void ConfirmEmail()
    {
        if (!(Parameters["UserId"] is System.Guid))
        {
            throw new InvalidCastException(
                Resources.ErrorUserIdNotGuid);
        }
        if (!(Parameters["Token"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorTokenNotString);
        }
        Task<InternalIdentityResult> task = userManager.ConfirmEmailAsync(
            Parameters["UserId"].ToString()
            , Parameters["Token"].ToString());
		RuleException ex = new RuleException();
        if (task.IsFaulted)
        {
			RuleExceptionData rd = new RuleExceptionData();                    
			rd.Severity = RuleExceptionSeverity.High;
			rd.EntityName = "";
			rd.FieldName = "";
			rd.Message = task.Exception.Message;
			ex.RuleResult.Add(rd);       
            throw ex;
        }
        else if (!task.Result.Succeeded)
        {
			RuleExceptionData rd2 = new RuleExceptionData();                    
			foreach (object o in task.Result.Errors) {
				
				RuleExceptionData rd = new RuleExceptionData();                    
				rd2.Severity = RuleExceptionSeverity.High;
				rd2.EntityName = "";
				rd2.FieldName = "";
				rd2.Message = (string) o;
				ex.RuleResult.Add(rd2);
			}
			throw ex;
        }
        else
        {
            result = true;
        }
    }
    private void ChangePassword()
    {
        if (!(Parameters["Username"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorUsernameNotString);
        }
        if (!(Parameters["OldPassword"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorOldPasswordNotString);
        }
        if (!(Parameters["NewPassword"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorNewPasswordNotString);
        }
        IOrigamUser user = FindUser();
        Task<InternalIdentityResult> task = userManager.ChangePasswordAsync(
            user.BusinessPartnerId,
            Parameters["OldPassword"].ToString().TrimEnd(),
            Parameters["NewPassword"].ToString().TrimEnd());
        if (task.IsFaulted)
        {
            throw task.Exception;
        }
        else if (!task.Result.Succeeded)
        {
            throw new Exception(string.Join(" ", task.Result.Errors));
        }
        else
        {
            result = true;
        }
    }
    private IOrigamUser FindUser()
    {
        IOrigamUser user = null;
        Task<IOrigamUser> taskFindUser = userManager.FindByNameAsync(
            Parameters["Username"].ToString(),TransactionId);
        if (taskFindUser.IsFaulted)
        {
            throw taskFindUser.Exception;
        }
        else
        {
            user = taskFindUser.Result;
        }
        if (user == null)
        {
            throw new Exception(Resources.ErrorUserNotFound);
        }
        return user;
    }
    private void ResetPassword()
    {
        if (!(Parameters["UserName"] is String))
        {
            throw new InvalidCastException(
                Resources.ErrorUsernameNotString);
        }
        if (!(Parameters["Token"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorTokenNotString);
        }
        if (!(Parameters["NewPassword"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorNewPasswordNotString);
        }
        Task<InternalIdentityResult> resetPasswordTask
            = userManager.ResetPasswordFromUsernameAsync(
            Parameters["UserName"].ToString(),
            Parameters["Token"].ToString(),
            Parameters["NewPassword"].ToString().TrimEnd());
        if (resetPasswordTask.IsFaulted)
        {                
            throw resetPasswordTask.Exception;
        }
        else if (!resetPasswordTask.Result.Succeeded)
        {
            //throw new Exception(string.Join(" ", resetPasswordTask.Result.Errors));
            RuleExceptionDataCollection reCol = new RuleExceptionDataCollection();
            foreach (string err in resetPasswordTask.Result.Errors)
            {
                reCol.Add(new RuleExceptionData(err));
            }
            throw new RuleException(reCol);
        }
        else
        {
            result = true;
        }
    }
    private void DeleteUser()
    {
        if (!(Parameters["Username"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorUsernameNotString);
        }
        IOrigamUser user = FindUser();
        user.TransactionId = TransactionId;
        Task<InternalIdentityResult> task = userManager.DeleteAsync(user);
        if (task.IsFaulted)
        {
            throw task.Exception;
        }
        else
        {
            result = task.Result.Succeeded;
            OrigamUserContext.Reset(user.UserName);
        }
    }
    private void UpdateUser()
    {
        // Check input parameters
        if (!(Parameters["Username"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorUsernameNotString);
        }
        if (Parameters.ContainsKey("Email")
        && !(Parameters["Email"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorEmailNotString);
        }
        if (Parameters.ContainsKey("IsApproved")
        && !(Parameters["IsApproved"] is Boolean))
        {
            throw new InvalidCastException(
                Resources.ErrorIsApprovedNotBool);
        }
        IOrigamUser user = FindUser();
        user.Email = Parameters.ContainsKey("Email") 
            ? Parameters["Email"].ToString() : null;
        user.IsApproved = Parameters.ContainsKey("IsApproved") 
            ? (Boolean)Parameters["IsApproved"] : false;
        user.TransactionId = TransactionId;
        Task<InternalIdentityResult> task = userManager.UpdateAsync(user);
        if (task.IsFaulted)
        {
            throw task.Exception;
        }
        else
        {
            result = task.Result.Succeeded;
        }
    }
	private void SendEmailConfirmationToken()
	{
		// Check input parameters
		if (!(Parameters["Username"] is string))
		{
			throw new InvalidCastException(
				Resources.ErrorUsernameNotString);
		}
		try {
			userManager.SendNewUserToken(
				(string)this.Parameters["Username"]);
			
		}
		catch (Exception e)
		{
			if (log.IsErrorEnabled)
			{
				log.ErrorFormat("Can't send a confirmation email: {0}", e);
			}
			// convert to rule exception
			RuleExceptionData red = new RuleExceptionData();
			red.Message = e.Message;
			red.Severity = RuleExceptionSeverity.High;
			red.EntityName = "SendEmailConfirmationTokenError";
			red.FieldName = "";
			throw new RuleException(new RuleExceptionDataCollection() { red });
		}
		result = true;
	}
	private void CreateUser()
    {
        // Check input parameters
        if (!(Parameters["Username"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorUsernameNotString);
        }
        if (Parameters.ContainsKey("Password")
        && !(Parameters["Password"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorPasswordNotString);
        }
        if (!(Parameters["Email"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorEmailNotString);
        }
        if (Parameters.ContainsKey("ProviderUserKey")
        && !(Parameters["ProviderUserKey"] is System.Guid))
        {
            throw new InvalidCastException(
                Resources.ErrorProviderUserKeyNotGuid);
        }
        if (Parameters.ContainsKey("PasswordQuestion")
        && !(Parameters["PasswordQuestion"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorPasswordQuestionNotString);
        }
        if (Parameters.ContainsKey("PasswordAnswer")
        && !(Parameters["PasswordAnswer"] is string))
        {
            throw new InvalidCastException(
                Resources.ErrorPasswordAnswerNotString);
        }
		bool emailConfirmed = false;
		if (Parameters.ContainsKey("EmailConfirmed"))
		{
			if (!(Parameters["EmailConfirmed"] is bool))
			{
				throw new InvalidCastException(
					Resources.ErrorEmailConfirmedNotBool);
			}
			else
			{
				emailConfirmed = (bool)Parameters["EmailConfirmed"];
			}
		}
        IOrigamUser user = userManager.CreateUserObject(
            Parameters["Username"].ToString());
        user.Email = Parameters["Email"].ToString();
        if (Parameters.ContainsKey("PasswordQuestion")) {
            user.PasswordQuestion 
                = Parameters["PasswordQuestion"].ToString();
        }
        if (Parameters.ContainsKey("PasswordAnswer"))
        {
            user.PasswordAnswer = Parameters["PasswordAnswer"].ToString();
        }
        if (Parameters.ContainsKey("ProviderUserKey"))
        {
            user.ProviderUserKey = (Guid)Parameters["ProviderUserKey"];
        }
        user.EmailConfirmed = emailConfirmed;            
        user.TransactionId = TransactionId;
        Task<InternalIdentityResult> task = userManager.CreateAsync(
            user, Parameters["Password"].ToString().TrimEnd());
		if (task.IsFaulted)
		{
			throw task.Exception;
		}
		else if (!task.Result.Succeeded)
		{
			throw new Exception(string.Join(" ", task.Result.Errors));
		} 
        OrigamUserContext.Reset(Parameters["Username"].ToString());
		result = user.UserName;            
    }
    private void GetEmailConfirmationToken()
    {
        if (!(Parameters["UserId"] is System.Guid))
        {
            throw new InvalidCastException(
                Resources.ErrorUserIdNotGuid);
        }
        Task<string> task = userManager
            .GenerateEmailConfirmationTokenAsync(
            Parameters["UserId"].ToString());
        if (task.IsFaulted)
        {
            throw task.Exception;
        }
        else
        {
            result = task.Result;
        }
    }
    private void GetPasswordResetTokenFromEmail()
    {
        if (!(Parameters["Email"] is System.String))
        {
            throw new InvalidCastException(
                Resources.ErrorEmailNotString);
        }
        Task<TokenResult> generateTask = 
            userManager.GetPasswordResetTokenFromEmailAsync(
                (string)Parameters["Email"]);
        if (generateTask.IsFaulted)
        {
            throw generateTask.Exception;
        }
        else
        {
            // render xml
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("ROOT");
            xmlDoc.AppendChild(rootNode);
            XmlNode tokenResultNode = xmlDoc.CreateElement("TokenResult");
            // token
            XmlAttribute tokenAttr = xmlDoc.CreateAttribute("Token");
            tokenAttr.Value = generateTask.Result.Token;
            tokenResultNode.Attributes.Append(tokenAttr);
            // userId
            XmlAttribute userNameAttr = xmlDoc.CreateAttribute("UserName");
            userNameAttr.Value = generateTask.Result.UserName.ToString();
            tokenResultNode.Attributes.Append(userNameAttr);
            // tokenValidityHours
            XmlAttribute tokenValidityAttr = xmlDoc.CreateAttribute("TokenValidityHours");
            tokenValidityAttr.Value = generateTask.Result.TokenValidityHours.ToString();
            tokenResultNode.Attributes.Append(tokenValidityAttr);
            // errorMessage
            XmlAttribute errorMessageAttr = xmlDoc.CreateAttribute("ErrorMessage");
            errorMessageAttr.Value = generateTask.Result.ErrorMessage;
            tokenResultNode.Attributes.Append(errorMessageAttr);
            rootNode.AppendChild(tokenResultNode);
            result = new XmlContainer(xmlDoc);
        }
    }
    private void GetPasswordResetToken()
    {
        if (!(Parameters["UserId"] is System.Guid))
        {
            throw new InvalidCastException(
                Resources.ErrorUserIdNotGuid);
        }
        Task<string> task = userManager
            .GeneratePasswordResetTokenAsync(
            Parameters["UserId"].ToString());
        if (task.IsFaulted)
        {
            throw task.Exception;
        }
        else
        {
            result = task.Result;
        }
    }
    private void GetPasswordAttributes()
    {
        Task<XmlDocument> task = userManager.GetPasswordAttributesAsync();
        if (task.IsFaulted)
        {
            throw task.Exception;
        }
        else
        {
            result = task.Result;
        }
    }
    private XmlDocument GetUserDataXml(IOrigamUser user)
    {
        XmlDocument xmlDoc = new System.Xml.XmlDocument();
        XmlNode root = xmlDoc.CreateElement("ROOT");
        XmlNode userData = xmlDoc.CreateElement("UserData");
        XmlAttribute creationDate = xmlDoc.CreateAttribute("CreationDate");
        creationDate.Value = user.CreationDate.ToString();
        userData.Attributes.Append(creationDate);
        XmlAttribute email = xmlDoc.CreateAttribute("Email");
        email.Value = user.Email.ToString();
        userData.Attributes.Append(email);
        XmlAttribute isApproved = xmlDoc.CreateAttribute("IsApproved");
        isApproved.Value = user.IsApproved.ToString();
        userData.Attributes.Append(isApproved);
        XmlAttribute isOnline = xmlDoc.CreateAttribute("IsOnline");
        userData.Attributes.Append(isOnline);
        isOnline.Value = user.IsOnline.ToString();
        XmlAttribute lastActivityDate = xmlDoc.CreateAttribute(
            "LastActivityDate");
        lastActivityDate.Value = user.LastActivityDate.ToString();
        userData.Attributes.Append(lastActivityDate);
        XmlAttribute lastLockoutDate = xmlDoc.CreateAttribute(
            "LastLockoutDate");
        lastLockoutDate.Value = user.LastLockoutDate.ToString();
        userData.Attributes.Append(lastLockoutDate);
        XmlAttribute lastLoginDate = xmlDoc.CreateAttribute(
            "LastLoginDate");
        lastLoginDate.Value = user.LastLoginDate.ToString();
        userData.Attributes.Append(lastLoginDate);
        XmlAttribute lastPasswordChangedDate = xmlDoc.CreateAttribute(
            "LastPasswordChangedDate");
        lastPasswordChangedDate.Value = user.LastPasswordChangedDate.ToString();
        userData.Attributes.Append(lastPasswordChangedDate);
        XmlAttribute passwordQuestion = xmlDoc.CreateAttribute(
            "PasswordQuestion");
        passwordQuestion.Value = user.PasswordQuestion;
        userData.Attributes.Append(passwordQuestion);
        root.AppendChild(userData);
        xmlDoc.AppendChild(root);
        return xmlDoc;
    }
    public void Dispose() => serviceScope?.Dispose();        
}
