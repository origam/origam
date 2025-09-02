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

namespace Origam.Security.Common;

public interface IOrigamUser
{
    string BusinessPartnerId { get; set; }
    string UserName { get; set; }
    DateTime CreationDate { get; }
    string Email { get; set; }
    bool IsApproved { get; set; }
    bool IsOnline { get; set; }
    DateTime LastActivityDate { get; }

    // TODO: UserServerCore stores lockoutEndDate into LastLockoutEndDate.
    // The LastLockoutDate should be renamed to LockoutEndDate when
    // .net4 (netFx) specific classes are removed!
    DateTime? LastLockoutDate { get; set; }
    DateTime LastLoginDate { get; }
    DateTime LastPasswordChangedDate { get; }
    string PasswordQuestion { get; set; }
    string PasswordAnswer { get; set; }
    Guid ProviderUserKey { get; set; }
    string TransactionId { get; set; }
    bool Is2FAEnforced { get; }
    string PasswordHash { get; set; }
    bool TwoFactorEnabled { get; set; }
    string NormalizedEmail { get; }
    bool EmailConfirmed { get; set; }
    string NormalizedUserName { get; }
    string RoleId { get; }
    string FirstName { get; }
    string Name { get; }
    int FailedPasswordAttemptCount { get; set; }
    Guid LanguageId { get; set; }
    string SecurityStamp { get; set; }
}
