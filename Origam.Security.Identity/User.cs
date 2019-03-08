using System;
using System.Data;
using Origam.Security.Common;

namespace Origam.Security.Identity
{
    public class User: IOrigamUser//, IUser<string>
    {
        public string BusinessPartnerId { get; set; }
        public string UserName { get; set; }
        public DateTime CreationDate { get; set; }
        public string Email { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastActivityDate { get; set; }
        public DateTime LastLockoutDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastPasswordChangedDate { get; set; }
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        public Guid ProviderUserKey { get; set; }
        public string TransactionId { get; set; }
        public string SecurityStamp { get; set; }
        public bool Is2FAEnforced { get; set; }
        
        public string NormalizedUserName => UserName;//.ToUpper();
        public bool EmailConfirmed { get; set; }
        public string NormalizedEmail => Email.ToUpper();
        public string PasswordHash { get; set; }
        public bool TwoFactorEnabled
        {
            get => Is2FAEnforced;
            set => Is2FAEnforced = value;
        }

        public string RoleId { get; set; }
        public string FirstName { get; set; }
        public string Name { get; set; }

        public User(string userName)
        {
            UserName = userName;
            IsApproved = true;
        }

        public User()
        {
        }
        
        public static User Create( DataSet dataSet)
        {
            User user = new User();
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
            user.UserName = (string)dataSet.Tables["OrigamUser"].Rows[0]["UserName"];
            user.Is2FAEnforced = (bool)dataSet.Tables["OrigamUser"].Rows[0]["Is2FAEnforced"];
            user.EmailConfirmed = (bool)dataSet.Tables["OrigamUser"].Rows[0]["EmailConfirmed"];
            user.LastLockoutDate = GetValue<DateTime>(dataSet,"LastLockoutDate" );
            user.LastLoginDate = GetValue<DateTime>(dataSet,"LastLoginDate");
            user.IsLockedOut = (bool)dataSet.Tables["OrigamUser"].Rows[0]["IsLockedOut"];
            user.ProviderUserKey = (Guid)dataSet.Tables["OrigamUser"].Rows[0]["refBusinessPartnerId"];
            user.BusinessPartnerId = user.ProviderUserKey.ToString();
            user.Is2FAEnforced = (bool)dataSet.Tables["OrigamUser"].Rows[0]["Is2FAEnforced"];
            user.PasswordHash =(string)dataSet.Tables["OrigamUser"] .Rows[0]["Password"];
            return user;
        }

        private static T GetValue<T>(DataSet dataSet, string propertyName)
        {
            var value = dataSet.Tables["OrigamUser"].Rows[0][propertyName];
            return value is DBNull 
                ? default 
                : (T)value;
        }
    }
}