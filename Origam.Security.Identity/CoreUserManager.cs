using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Origam.Security.Common;

namespace Origam.Security.Identity
{
    public class CoreUserManager: UserManager<IOrigamUser>, IFrameworkSpecificManager
    {
        public CoreUserManager(IUserStore<IOrigamUser> store, 
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<IOrigamUser> passwordHasher, 
            IEnumerable<IUserValidator<IOrigamUser>> userValidators, 
            IEnumerable<IPasswordValidator<IOrigamUser>> passwordValidators, 
            ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errors, 
            IServiceProvider services, 
            ILogger<UserManager<IOrigamUser>> logger) 
            : base(store, optionsAccessor, passwordHasher, 
                userValidators, passwordValidators, keyNormalizer,
                errors, services, logger)
        {
        }
      
        HashSet<string> emailConfirmedUserIds= new HashSet<string>();

        public void SetEmailConfirmed(string userId)
        {
            emailConfirmedUserIds.Add(userId);
        }

        public bool EmailConfirmed(string id)
        {
            return emailConfirmedUserIds.Contains(id);
        }

        public Task<string> GenerateEmailConfirmationTokenAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public bool UserExists(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GeneratePasswordResetTokenAsync1(string toString)
        {
            throw new NotImplementedException();
        }

        public int? TokenLifespan { get; }


    }
}