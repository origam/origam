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
using System.Threading.Tasks;
using System.Web.Security;
using System.Xml;
using Origam.Rule;
using Microsoft.AspNet.Identity;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using Origam.Security.Common;

namespace Origam.Security.Identity
{
    public class NetMembershipUserManager : AbstractUserManager
    {
        public NetMembershipUserManager(IUserStore<OrigamUser> store)
            : base(store)
        {
            UnlocksOnPasswordReset = false;
            IsPasswordRecoverySupported = true;
        }

        public string InjectDomain { get; set; }
        public bool UnlocksOnPasswordReset { get; set; }

        public static AbstractUserManager Create()
        {
            AbstractUserManager manager = new NetMembershipUserManager(new OrigamUserStore());
            return manager;
        }

        override public Task<OrigamUser> FindAsync(
            string userName, string password)
        {
            if(!string.IsNullOrEmpty(InjectDomain) && !userName.Contains("\\"))
            {
                userName = InjectDomain + "\\" + userName;
            }
            if (Membership.ValidateUser(userName, password))
            {
                MembershipUser membershipUser = Membership.GetUser(userName);
                return Task.FromResult(
                    MembershipUserToOrigamUser(membershipUser));
            }
            else
            {
                return Task.FromResult<OrigamUser>(null);
            }
        }

        override public Task<OrigamUser> FindByIdAsync(string userId)
        {
            MembershipUser membershipUser 
                = Membership.GetUser(new Guid(userId));
            if (membershipUser != null)
            {
                return Task.FromResult(
                    MembershipUserToOrigamUser(membershipUser));
            }
            return Task.FromResult<OrigamUser>(null);
        }

        public override Task<OrigamUser> FindByNameAsync(string userName)
        {
            MembershipUser membershipUser = Membership.GetUser(userName);
            if (membershipUser != null)
            {
                return Task.FromResult(
                    MembershipUserToOrigamUser(membershipUser));
            }
            return Task.FromResult<OrigamUser>(null);
        }

        public override bool SupportsUserSecurityStamp
        {
            get
            {
                return true;
            }
        }

        override public Task<string> GetSecurityStampAsync(string userId)
        {
            MembershipUser mu 
                = Membership.GetUser(new Guid(userId));
            string hash = null;
            if (mu != null)
            {
                using (MD5 md5hash = MD5.Create())
                {
                    hash = GetMd5Hash(md5hash,
                        string.Format("{0}{1}{2}{3}{4}"
                            , mu.LastPasswordChangedDate
                            , mu.IsApproved
                            , mu.IsLockedOut
                            , mu.LastLockoutDate
                            , mu.Email
                            )
                    );
                }
            }
            return Task.FromResult<string>(hash);
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        override public Task<string> GetEmailAsync(string userId)
        {
            MembershipUser membershipUser 
                = Membership.GetUser(new Guid(userId));
            if (membershipUser != null)
            {
                return Task.FromResult(membershipUser.Email);
            }
            return Task.FromResult<string>(null); 
        }

        override public Task<string> RecoverPasswordAsync(string email)
        {
            string username = Membership.GetUserNameByEmail(email);
            if (string.IsNullOrEmpty(username))
            {
                return Task.FromResult(Resources.EmailInvalid);
            }
            MembershipUser user = Membership.GetUser(username);
            string resultMessage = "";
            SendPasswordResetToken(email, username, ref resultMessage);
            return Task.FromResult(resultMessage);
        }

        override public Task<bool> ChangePasswordQuestionAndAnswerAsync(
            string username, string password, string question, string answer)
        {
            MembershipUser user = Membership.GetUser(username);
            return Task.FromResult(user.ChangePasswordQuestionAndAnswer(
                password, question, answer));
        }

        override public Task<bool> UnlockUserAsync(string userName)
        {
            MembershipUser user = Membership.GetUser(userName);
            bool result = user.UnlockUser();
            if (result)
            {
                result = SendUserUnlockingNotification(userName, user.Email);
            }
            return Task.FromResult(result);
        }

        override public Task<IdentityResult> ChangePasswordAsync(
            string userId, string currentPassword, string newPassword)
        {
            MembershipUser user = Membership.GetUser(new Guid(userId));
            if (!user.ChangePassword(currentPassword, newPassword))
            {
                throw new Exception("Failed to change password.");
            }
            return Task.FromResult(IdentityResult.Success);
        }

        public override async Task<IdentityResult> ResetPasswordAsync(
            string userId, string token, string newPassword)
        {
            //OrigamUser ou = await FindByIdAsync(userId);
            MembershipUser mu = Membership.GetUser(new Guid(userId));
            if (mu == null)
            {
                if (log.IsErrorEnabled)
                {
                    log.ErrorFormat(
                        "Reseting password for user {0}. User not found."
                        , userId);
                }
                // user not found;
                return IdentityResult.Failed(Resources.UserIdNotFound);
            }

            // verify password-reset token
            bool verificationResult = await
                VerifyUserTokenAsync(userId, "ResetPassword", token);
            if (!verificationResult)
            {
                if (log.IsInfoEnabled)
                {
                    log.InfoFormat(
                        "Reseting password for user {0}: Invalid token `{1}'."
                        , userId, token);
                }
                return IdentityResult.Failed(
                    Resources.PasswordResetTokenInvalid);
            }
            // validate password length, special chars, etc
            // have to do here in advance
            // otherwise it would reset the password
            // later and the token would get invalidated
            OrigamPasswordValidator passwordValidator
                = new OrigamPasswordValidator(
                    Membership.MinRequiredPasswordLength,
                    Membership.MinRequiredNonAlphanumericCharacters);
            Task<IdentityResult> validationTask
                = passwordValidator.ValidateAsync(newPassword);
            if (validationTask.IsFaulted)
            {
                throw validationTask.Exception;
            }
            if (!validationTask.Result.Succeeded)
            {
                return validationTask.Result;
            }
            if (!mu.ChangePassword(mu.ResetPassword(), newPassword))
            {
                if (log.IsErrorEnabled)
                {
                    log.ErrorFormat("Reseting password for user {0} failed.",
                        userId);
                }
                throw new Exception("Failed to change password.");
            }
            if (UnlocksOnPasswordReset)
            {
                mu.UnlockUser();
            }
            if (log.IsDebugEnabled)
            {
                log.InfoFormat("Reseting password for user {0}: Success.",
                    userId);
            }
            return IdentityResult.Success;
        }


        override public Task<IdentityResult> DeleteAsync(OrigamUser user)
        {
            Membership.DeleteUser(user.UserName);
            return Task.FromResult(IdentityResult.Success);
        }

        override public Task<IdentityResult> UpdateAsync(OrigamUser user)
        {
            MembershipUser membershipUser = Membership.GetUser(user.UserName);
            membershipUser.IsApproved = user.IsApproved;
            if ((user.Email != null) 
            && (user.Email.ToString().Trim().Length > 0))
            {
                membershipUser.Email = user.Email.ToString().Trim();
            }
            Membership.UpdateUser(membershipUser);
            return Task.FromResult(IdentityResult.Success);
        }

        override public Task<IdentityResult> CreateAsync(
            OrigamUser user, string password)
        {
            MembershipCreateStatus status;
            MembershipUser membershipUser = Membership.CreateUser(
                user.UserName,
                password,
                user.Email,
                user.PasswordQuestion,
                user.PasswordAnswer,
                user.IsApproved,
                user.ProviderUserKey,
                out status);
            if (membershipUser == null)
            {
                if (log.IsWarnEnabled)
                {
                    log.WarnFormat(
                        "Failed to create user through Membership: {0}.", 
                        status);
                }
                RuleException ex = new RuleException();
                RuleExceptionData rd = new RuleExceptionData(
                    string.Format(Resources.ErrorCreationOfUserFailed, 
                    GetErrorMessage(status)));
                rd.EntityName = "BusinessPartner";
                rd.FieldName = "";
                rd.Severity = RuleExceptionSeverity.High;
                ex.RuleResult.Add(rd);
                throw ex;
            }
            return Task.FromResult(IdentityResult.Success);
        }

        private string GetErrorMessage(MembershipCreateStatus status)
        {
            switch (status)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return Resources.MembershipCreateStatus_DuplicateUserName;
                case MembershipCreateStatus.DuplicateEmail:
                    return Resources.MembershipCreateStatus_DuplicateEmail;
                case MembershipCreateStatus.InvalidPassword:
                    return Resources.MembershipCreateStatus_InvalidPassword;
                case MembershipCreateStatus.InvalidEmail:
                    return Resources.MembershipCreateStatus_InvalidEmail;
                case MembershipCreateStatus.InvalidAnswer:
                    return Resources.MembershipCreateStatus_InvalidAnswer;
                case MembershipCreateStatus.InvalidQuestion:
                    return Resources.MembershipCreateStatus_InvalidQuestion;
                case MembershipCreateStatus.InvalidUserName:
                    return Resources.MembershipCreateStatus_InvalidUserName;
                case MembershipCreateStatus.ProviderError:
                    return Resources.MembershipCreateStatus_ProviderError;
                case MembershipCreateStatus.UserRejected:
                    return Resources.MembershipCreateStatus_UserRejected;
                default:
                    return Resources.MembershipCreateStatus_Unknown;
            }
        }

        private OrigamUser MembershipUserToOrigamUser(
            MembershipUser membershipUser)
        {
            OrigamUser origamUser = new OrigamUser(membershipUser.UserName);
            origamUser.CreationDate = membershipUser.CreationDate;
            origamUser.Email = membershipUser.Email;
            origamUser.IsApproved = membershipUser.IsApproved;
            origamUser.IsLockedOut = membershipUser.IsLockedOut;
            origamUser.IsOnline = membershipUser.IsOnline;
            origamUser.LastActivityDate = membershipUser.LastActivityDate;
            origamUser.LastLockoutDate = membershipUser.LastLockoutDate;
            origamUser.LastLoginDate = membershipUser.LastLoginDate;
            origamUser.LastPasswordChangedDate 
                = membershipUser.LastPasswordChangedDate;
            origamUser.PasswordQuestion = membershipUser.PasswordQuestion;
            // find user in the internal BusinessPartner table - if not found
            // we refuse access
            DataSet data = GetBusinessPartnerDataSet(
                GET_BUSINESS_PARTNER_BY_USER_NAME, "BusinessPartner_parUserName", membershipUser.UserName);
            DataTable table = data.Tables["BusinessPartner"];
            if (table.Rows.Count == 0)
            {
                // no access
                return null;
            }
            DataRow businessPartnerRow = table.Rows[0];
            origamUser.BusinessPartnerId = businessPartnerRow["Id"].ToString();
            if (!businessPartnerRow.IsNull("UserEmail"))
            {
                // set email set in the BusinessPartner table instead of the one
                // stored in the membership db
                origamUser.Email = (string)businessPartnerRow["UserEmail"];
            }
            return origamUser;
        }

        protected override void FillPasswordAttributes(
            XmlDocument document, XmlNode attributes)
        {
            XmlAttribute minLength = document.CreateAttribute(
                "MinRequiredPasswordLength");
            minLength.Value = Membership.MinRequiredPasswordLength.ToString();
            attributes.AppendChild(minLength);
            XmlAttribute regEx = document.CreateAttribute(
                "PasswordStrengthRegularExpression");
            regEx.Value = Membership.PasswordStrengthRegularExpression;
            attributes.AppendChild(regEx);
            XmlAttribute minNonAlphaChars = document.CreateAttribute(
                "MinRequiredNonAlphanumericCharacters");
            minNonAlphaChars.Value 
                = Membership.MinRequiredNonAlphanumericCharacters.ToString();
            attributes.AppendChild(minNonAlphaChars);
            XmlAttribute maxInvalidAttempts = document.CreateAttribute(
                "MaxInvalidPasswordAttempts");
            maxInvalidAttempts.Value 
                = Membership.MaxInvalidPasswordAttempts.ToString();            
            attributes.AppendChild(maxInvalidAttempts);
        }

        public override async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            MembershipUser membershipUser
                = Membership.GetUser(new Guid(userId));
            if (membershipUser == null)
            {
                return IdentityResult.Failed(Resources.UserIdNotFound);
            }
            if (!await VerifyUserTokenAsync(userId, "Confirmation", token))
            {
                return IdentityResult.Failed(Resources.TokenInvalid);
            }
            membershipUser.IsApproved = true;
            Membership.UpdateUser(membershipUser);
            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> ConfirmEmailAsync(string userId)
        {
            MembershipUser membershipUser
                = Membership.GetUser(new Guid(userId));
            if (membershipUser == null)
            {
                return IdentityResult.Failed(Resources.UserIdNotFound);
            }
            membershipUser.IsApproved = true;
            Membership.UpdateUser(membershipUser);
            return IdentityResult.Success;
        }
    }
}