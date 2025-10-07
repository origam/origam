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
using Origam.Security.Common;

namespace Origam.Server.Authorization;

public class User : IOrigamUser
{
    public string BusinessPartnerId { get; set; }
    public string UserName { get; set; }
    public DateTime CreationDate { get; set; }
    public string Email { get; set; }
    public bool IsApproved { get; set; }
    public bool IsOnline { get; set; }
    public DateTime LastActivityDate { get; set; }
    public DateTime? LastLockoutDate { get; set; }
    public DateTime LastLoginDate { get; set; }
    public DateTime LastPasswordChangedDate { get; set; }
    public string PasswordQuestion { get; set; }
    public string PasswordAnswer { get; set; }
    public Guid ProviderUserKey { get; set; }
    public string TransactionId { get; set; }
    public string SecurityStamp { get; set; }
    public bool Is2FAEnforced { get; set; }

    public string NormalizedUserName => UserName; //.ToUpper();
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
    public int FailedPasswordAttemptCount { get; set; }
    public Guid LanguageId { get; set; }

    public User(string userName)
    {
        UserName = userName;
        IsApproved = true;
    }

    public User() { }
}
