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