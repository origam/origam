using System;

namespace Origam.Security.Common
{
    public interface IOrigamUser
    {
        string BusinessPartnerId { get; set; }
        string UserName { get; set; }
        DateTime CreationDate { get; }
        string Email { get; set; }
        bool IsApproved { get; }
        bool IsLockedOut { get; set; }
        bool IsOnline { get; set; }
        DateTime LastActivityDate { get;  }
        DateTime LastLockoutDate { get; }
        DateTime LastLoginDate { get;  }
        DateTime LastPasswordChangedDate { get;}
        string PasswordQuestion { get; }
        Guid ProviderUserKey { get; }
        string TransactionId { get;  }
        bool Is2FAEnforced { get;  }
        string PasswordHash { get; set; }
        bool TwoFactorEnabled { get; set; }
        string NormalizedEmail { get; }
        bool EmailConfirmed { get; set; }
        string NormalizedUserName { get;}
        string RoleId { get; }
        string FirstName { get;  }
        string Name { get;  }
    }
}