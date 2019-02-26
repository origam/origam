using System;

namespace Origam.Security.Common
{
    public interface IOrigamUser
    {
        string Id { get; set; }
        string UserName { get; set; }
        DateTime CreationDate { get; set; }
        string Email { get; set; }
        bool IsApproved { get; set; }
        bool IsLockedOut { get; set; }
        bool IsOnline { get; set; }
        DateTime LastActivityDate { get; set; }
        DateTime LastLockoutDate { get; set; }
        DateTime LastLoginDate { get; set; }
        DateTime LastPasswordChangedDate { get; set; }
        string PasswordQuestion { get; set; }
        string PasswordAnswer { get; set; }
        Guid ProviderUserKey { get; set; }
        string TransactionId { get; set; }
        string SecurityStamp { get; set; }
        bool Is2FAEnforced { get; set; }
    }
}