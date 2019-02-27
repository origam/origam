using System;
using BrockAllen.IdentityReboot;
using Microsoft.AspNetCore.Identity;
using Origam.Security.Common;

namespace Origam.ServerCore
{
    class CorePasswordHasher: IPasswordHasher<User>
    {
        private readonly InternalPasswordHasherWithLegacySupport internalHasher 
            = new InternalPasswordHasherWithLegacySupport();

        public string HashPassword(User user, string password)
        {
            return internalHasher.HashPassword(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
        {
            VerificationResult verificationResult = internalHasher.VerifyHashedPassword(hashedPassword, providedPassword);
            return ToAspNetCoreResult(verificationResult);
        }
        
        private static PasswordVerificationResult ToAspNetCoreResult(VerificationResult result)
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