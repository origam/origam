#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using BrockAllen.IdentityReboot;
using Microsoft.AspNet.Identity;
using Origam.Security.Common;

// legacy password verification based on: 
// http://www.asp.net/identity/overview/migrations/migrating-an-existing-website-from-sql-membership-to-aspnet-identity
namespace Origam.Security.Identity
{

    public class AdaptivePasswordHasherWithLegacySupport : IPasswordHasher
    {
        private readonly InternalPasswordHasherWithLegacySupport internalHasher =
            new InternalPasswordHasherWithLegacySupport();

        public int IterationCount
        {
            get
            {
                return internalHasher.IterationCount;
            }
            set
            {
                internalHasher.IterationCount = value;
            }
        }

        public string HashPassword(string password)
        {
            return internalHasher.HashPassword(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            VerificationResult verificationResult =
                internalHasher.VerifyHashedPassword(hashedPassword, providedPassword);
            return ToAspNetResult(verificationResult);
        }

        private static PasswordVerificationResult ToAspNetResult(VerificationResult result)
        {
            switch (result)
            {
                case VerificationResult.Failed:
                    return PasswordVerificationResult.Failed;
                case VerificationResult.Success:
                    return PasswordVerificationResult.Success;
                case VerificationResult.SuccessRehashNeeded:
                    return PasswordVerificationResult.SuccessRehashNeeded;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}