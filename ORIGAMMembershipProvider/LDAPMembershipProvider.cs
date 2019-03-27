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

using System.DirectoryServices;
using System.Text.RegularExpressions;
using log4net;
using System;
using System.Web.Security;

namespace Origam.hosting.utils
{
    public class LDAPMembershipProvider : MembershipProvider
    {
        private static readonly ILog log = LogManager.GetLogger(
            typeof(LDAPMembershipProvider));

        private string defaultDomain = null;

        public override string ApplicationName
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new System.NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new System.NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new System.NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new System.NotImplementedException();
        }

        public override bool EnablePasswordReset
        {
            get { return false; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new System.NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new System.NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new System.NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new System.NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new System.NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            return new MembershipUser(this.Name, username, null, null,
                null, null, true, false, DateTime.MinValue, DateTime.MinValue,
                DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new System.NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new System.NotImplementedException();
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { throw new System.NotImplementedException(); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new System.NotImplementedException(); }
        }

        public override int MinRequiredPasswordLength
        {
            get { throw new System.NotImplementedException(); }
        }

        public override int PasswordAttemptWindow
        {
            get { throw new System.NotImplementedException(); }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new System.NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new System.NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { throw new System.NotImplementedException(); }
        }

        public override bool RequiresUniqueEmail
        {
            get { throw new System.NotImplementedException(); }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new System.NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new System.NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new System.NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
        {
            string _userName;
            string _domainName;
            log.InfoFormat("Received username [{0}] to validate.", username);
            if (string.IsNullOrEmpty(defaultDomain))
            {
                Regex userNamePattern = new Regex(
                    "^([A-Za-z][A-Za-z0-9.-]+)\\\\((?! +$)[A-Za-z0-9 ]+)$");
                if (!userNamePattern.IsMatch(username))
                {
                    log.Warn("Username has invalid format.");
                    return false;
                }
                Regex splitPattern = new Regex("\\\\");
                string[] domainUsername = splitPattern.Split(username);
                _userName = domainUsername[1];
                _domainName = domainUsername[0];
            }
            else
            {
                _userName = username;
                _domainName = defaultDomain;
            }
            try
            {
                DirectoryEntry entry = new DirectoryEntry(
                    "LDAP://" + _domainName,
                    _userName, password);
                object nativeObject = entry.NativeObject;
                log.Info("User is valid.");
                return true;
            }
            catch (Exception ex) 
            {
                log.Fatal("User is invalid or failed to validate user.", ex);
            }
            return false;
        }

        public override void Initialize(string name, 
            System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            defaultDomain = config.Get("defaultDomain");
        }
    }
}
