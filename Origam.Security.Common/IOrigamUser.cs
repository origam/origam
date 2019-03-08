using System;

namespace Origam.Security.Common
{
    public interface IOrigamUser
    {
        string BusinessPartnerId { get; set; }
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
        string PasswordHash { get; set; }
        bool TwoFactorEnabled { get; set; }
        string NormalizedEmail { get; }
        bool EmailConfirmed { get; set; }
        string NormalizedUserName { get;}
        string RoleId { get; set; }
        string FirstName { get; set; }
        string Name { get; set; }
    }
}