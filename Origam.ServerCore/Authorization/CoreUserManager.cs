using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Origam.Security.Common;

namespace Origam.ServerCore.Authorization
{
    public class CoreUserManager: UserManager<User>, IFrameworkSpecificManager
    {
        public CoreUserManager(IUserStore<User> store, 
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<User> passwordHasher, 
            IEnumerable<IUserValidator<User>> userValidators, 
            IEnumerable<IPasswordValidator<User>> passwordValidators, 
            ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errors, 
            IServiceProvider services, 
            ILogger<UserManager<User>> logger) 
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