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
﻿using System;
using System.Web.Security;
using Origam.Rule;

namespace Origam.Workflow.NetMembershipService
{

    /// <summary>
    /// Summary description for NetMembershipAgentService
    /// </summary>
    public class NetMembershipServiceAgent : AbstractServiceAgent
    {
        //private SqlMembershipProvider _membership;

        public NetMembershipServiceAgent()
        {
            //
            // TODO: Add constructor logic here
            //
            //string configPath = "~/web.config";
            /*
            string configPath = "/origam-standa";
            Configuration config = WebConfigurationManager.OpenWebConfiguration(configPath);
            MembershipSection section = (MembershipSection)config.GetSection("system.web/membership");
            ProviderSettingsCollection settings = section.Providers;
            NameValueCollection membershipParams = settings[section.DefaultProvider].Parameters;
            _membership = new SqlMembershipProvider();
            _membership.Initialize(section.DefaultProvider, membershipParams);
             */
        }

        #region IServiceAgent Members

        private object _result;
        public override object Result
        {
            get
            {
                object temp = _result;
                _result = null;

                return temp;
            }
        }

        public static System.Xml.XmlDocument createPasswordAttributes()
        {
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            System.Xml.XmlNode root = xmlDoc.CreateElement("ROOT");
            System.Xml.XmlNode passwordAttributes = xmlDoc.CreateElement("PasswordAttributes");
            
            // MinLength
            System.Xml.XmlAttribute minLength = xmlDoc.CreateAttribute("MinRequiredPasswordLength");
            minLength.Value = System.Web.Security.Membership.MinRequiredPasswordLength.ToString();

            System.Xml.XmlAttribute regEx = xmlDoc.CreateAttribute("PasswordStrengthRegularExpression");
            regEx.Value = System.Web.Security.Membership.PasswordStrengthRegularExpression;

            System.Xml.XmlAttribute minNonAlphaChars = xmlDoc.CreateAttribute("MinRequiredNonAlphanumericCharacters");
            minNonAlphaChars.Value = System.Web.Security.Membership.MinRequiredNonAlphanumericCharacters.ToString();

            System.Xml.XmlAttribute maxInvalidAttempts = xmlDoc.CreateAttribute("MaxInvalidPasswordAttempts");
            maxInvalidAttempts.Value = System.Web.Security.Membership.MaxInvalidPasswordAttempts.ToString();            
            
            passwordAttributes.Attributes.Append(minLength);
            passwordAttributes.Attributes.Append(regEx);
            passwordAttributes.Attributes.Append(minNonAlphaChars);
            passwordAttributes.Attributes.Append(maxInvalidAttempts);
            root.AppendChild(passwordAttributes);
            xmlDoc.AppendChild(root);
            //attr.    "Attribute", "MinLength", System.Web.Security.Membership.MinRequiredPasswordLength.ToString()));
            return xmlDoc;
        }

        public static System.Xml.XmlDocument getUserData(MembershipUser user)
        {
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            System.Xml.XmlNode root = xmlDoc.CreateElement("ROOT");
            System.Xml.XmlNode userData = xmlDoc.CreateElement("UserData");

            
            System.Xml.XmlAttribute creationDate = xmlDoc.CreateAttribute("CreationDate");
            creationDate.Value = user.CreationDate.ToString();

            System.Xml.XmlAttribute email = xmlDoc.CreateAttribute("Email");
            email.Value = user.Email.ToString();

            System.Xml.XmlAttribute isApproved = xmlDoc.CreateAttribute("IsApproved");
            isApproved.Value = user.IsApproved.ToString();
            System.Xml.XmlAttribute isLockedOut = xmlDoc.CreateAttribute("IsLockedOut");
            isLockedOut.Value = user.IsLockedOut.ToString();
            System.Xml.XmlAttribute isOnline = xmlDoc.CreateAttribute("IsOnline");
            isOnline.Value = user.IsOnline.ToString();
            System.Xml.XmlAttribute lastActivityDate = xmlDoc.CreateAttribute("LastActivityDate");
            lastActivityDate.Value = user.LastActivityDate.ToString();
            System.Xml.XmlAttribute lastLockoutDate = xmlDoc.CreateAttribute("LastLockoutDate");
            lastLockoutDate.Value = user.LastLockoutDate.ToString();
            System.Xml.XmlAttribute lastLoginDate = xmlDoc.CreateAttribute("LastLoginDate");
            lastLoginDate.Value = user.LastLoginDate.ToString();
            System.Xml.XmlAttribute lastPasswordChangedDate = xmlDoc.CreateAttribute("LastPasswordChangedDate");
            lastPasswordChangedDate.Value = user.LastPasswordChangedDate.ToString();
            System.Xml.XmlAttribute passwordQuestion = xmlDoc.CreateAttribute("PasswordQuestion");
            passwordQuestion.Value = user.PasswordQuestion;            

            userData.Attributes.Append(creationDate);
            userData.Attributes.Append(email);
            userData.Attributes.Append(isApproved);
            userData.Attributes.Append(isLockedOut);
            userData.Attributes.Append(isOnline);
            userData.Attributes.Append(lastActivityDate);
            userData.Attributes.Append(lastLockoutDate);
            userData.Attributes.Append(lastLoginDate);
            userData.Attributes.Append(lastPasswordChangedDate);
            userData.Attributes.Append(passwordQuestion);           
            
            root.AppendChild(userData);
            xmlDoc.AppendChild(root);            
            return xmlDoc;
        }

        public static MembershipUser GetMembershipUser(string username)
        {
            MembershipUser user = Membership.GetUser(username);
            if (user == null)
            {
                throw new ArgumentOutOfRangeException("Username", username, strings.ErrorUserNotFound);
            }
            return user;
        }

        public static void updateUser(string username, string email=null, Boolean setApproval = false, Boolean isApproved = false)
        {
            MembershipUser user = GetMembershipUser(username);
            
            if (setApproval)
            {
                user.IsApproved = isApproved;
            }

            if (email != null && email.ToString().Trim().Length > 0)
            {
                user.Email = email.ToString().Trim();
            }
            Membership.UpdateUser(user);
        }

        public override void Run()
        {
            MembershipUser user = null;
            switch (this.MethodName)            
            {   
                
                case "GetUserData" :
                    if (!(this.Parameters["Username"] is string))
                        throw new InvalidCastException(strings.ErrorUsernameNotString);
                    user = GetMembershipUser(this.Parameters["Username"].ToString());
                    _result = getUserData(user);
                    break;
                case "ChangePasswordAnswerAndQuestion" :
                    if (!(this.Parameters["Username"] is string))
                        throw new InvalidCastException(strings.ErrorUsernameNotString);                    
                    if (!(this.Parameters["Password"] is string))
                        throw new InvalidCastException(strings.ErrorPasswordNotString);
                    if (!(this.Parameters["NewQuestion"] is string))
                        throw new InvalidCastException(strings.ErrorNewQuestionNotString);
                    if (!(this.Parameters["NewAnswer"] is string))
                        throw new InvalidCastException(strings.ErrorNewAnswerNotString);

                    user = GetMembershipUser(this.Parameters["Username"].ToString());
                    _result = user.ChangePasswordQuestionAndAnswer(
                            this.Parameters["Password"].ToString(),
                            this.Parameters["NewQuestion"].ToString(),
                            this.Parameters["NewAnswer"].ToString());
                    break;
                case "UnlockUser" :
                    if (!(this.Parameters["Username"] is string))
                        throw new InvalidCastException(strings.ErrorUsernameNotString);
                    user = GetMembershipUser(this.Parameters["Username"].ToString());                    
                    _result = user.UnlockUser();
                    break;
                case "ChangePassword" :
                    if (!(this.Parameters["Username"] is string))
                        throw new InvalidCastException(strings.ErrorUsernameNotString);
                    if (!(this.Parameters["OldPassword"] is string))
                        throw new InvalidCastException(strings.ErrorOldPasswordNotString);
                    if (!(this.Parameters["NewPassword"] is string))
                        throw new InvalidCastException(strings.ErrorNewPasswordNotString);

                    user = GetMembershipUser(this.Parameters["Username"].ToString());
                    _result = user.ChangePassword(this.Parameters["OldPassword"].ToString(), this.Parameters["NewPassword"].ToString());
                    break;
                case "GetPasswordAttributes" :
                    _result = createPasswordAttributes();
                    break;
                case "DeleteUser" :
                    if (!(this.Parameters["Username"] is string))
                        throw new InvalidCastException(strings.ErrorUsernameNotString);                    
                    _result = Membership.DeleteUser(this.Parameters["Username"].ToString());
                    break;
                case "UpdateUser" :
                    // Check input parameters
                    if (!(this.Parameters["Username"] is string))
                        throw new InvalidCastException(strings.ErrorUsernameNotString);
                    if (this.Parameters.ContainsKey("Email") && !(this.Parameters["Email"] is string))
                        throw new InvalidCastException(strings.ErrorEmailNotString);
                    if (this.Parameters.ContainsKey("IsApproved") && !(this.Parameters["IsApproved"] is Boolean))
                        throw new InvalidCastException(strings.ErrorIsApprovedNotBool);
                    updateUser(
                        Parameters["Username"].ToString()
                        , Parameters.ContainsKey("Email") ? Parameters["Email"].ToString() : null
                        , Parameters.ContainsKey("IsApproved") ? true : false
                        , Parameters.ContainsKey("IsApproved") ? (Boolean) Parameters["IsApproved"] : false
                    );
                    _result = true;
                    break;
                case "CreateUser":
                    // Check input parameters
                    if (!(this.Parameters["Username"] is string))
                        throw new InvalidCastException(strings.ErrorUsernameNotString);
                    if (this.Parameters.ContainsKey("Password") && !(this.Parameters["Password"] is string))
                        throw new InvalidCastException(strings.ErrorPasswordNotString);
                    if (!(this.Parameters["Email"] is string))
                        throw new InvalidCastException(strings.ErrorEmailNotString);
                    if (this.Parameters.ContainsKey("ProviderUserKey") && !(this.Parameters["ProviderUserKey"] is System.Guid))
                        throw new InvalidCastException(strings.ErrorProviderUserKeyNotGuid);
                    if (this.Parameters.ContainsKey("PasswordQuestion") && !(this.Parameters["PasswordQuestion"] is string))
                        throw new InvalidCastException(strings.ErrorPasswordQuestionNotString);
                    if (this.Parameters.ContainsKey("PasswordAnswer") && !(this.Parameters["PasswordAnswer"] is string))
                        throw new InvalidCastException(strings.ErrorPasswordAnswerNotString);

                    MembershipCreateStatus status;
                    try
                    {
                        MembershipUser newUser = System.Web.Security.Membership.CreateUser(
                            this.Parameters["Username"] as string,
                            this.Parameters["Password"] as string,
                            this.Parameters["Email"] as string,
                            (this.Parameters.ContainsKey("PasswordQuestion")) ?
                                this.Parameters["PasswordQuestion"] as string
                                : null,
                            (this.Parameters.ContainsKey("PasswordAnswer")) ?
                                this.Parameters["PasswordAnswer"] as string
                                : null,
                            true,
                            (this.Parameters.ContainsKey("ProviderUserKey")) ?
                                this.Parameters["ProviderUserKey"]
                                : null,                            
                            out status);
                        
                        if (newUser == null)
                        {
							RuleException ex = new RuleException();
							RuleExceptionData rd = new RuleExceptionData(string.Format(strings.ErrorCreationOfUserFailed, GetErrorMessage(status)));
							rd.EntityName = "BusinessPartner";
				            rd.FieldName = "";
							rd.Severity = RuleExceptionSeverity.High;
							ex.RuleResult.Add(rd);
							throw ex;                            
                        }
                        _result = newUser.UserName;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException("MethodName", this.MethodName, ResourceUtils.GetString("InvalidMethodName"));
            }
        }


        #endregion

        public string GetErrorMessage(MembershipCreateStatus status)
        {
            switch (status)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return strings.MembershipCreateStatus_DuplicateUserName;

                case MembershipCreateStatus.DuplicateEmail:
                    return strings.MembershipCreateStatus_DuplicateEmail;

                case MembershipCreateStatus.InvalidPassword:
                    return strings.MembershipCreateStatus_InvalidPassword;

                case MembershipCreateStatus.InvalidEmail:
                    return strings.MembershipCreateStatus_InvalidEmail;

                case MembershipCreateStatus.InvalidAnswer:
                    return strings.MembershipCreateStatus_InvalidAnswer;

                case MembershipCreateStatus.InvalidQuestion:
                    return strings.MembershipCreateStatus_InvalidQuestion;

                case MembershipCreateStatus.InvalidUserName:
                    return strings.MembershipCreateStatus_InvalidUserName;

                case MembershipCreateStatus.ProviderError:
                    return strings.MembershipCreateStatus_ProviderError;

                case MembershipCreateStatus.UserRejected:
                    return strings.MembershipCreateStatus_UserRejected;

                default:
                    return strings.MembershipCreateStatus_Unknown;
            }
        }
    }
    
}
