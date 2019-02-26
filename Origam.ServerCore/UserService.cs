using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using BrockAllen.IdentityReboot;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Origam.Security.Common;
using Origam.ServerCore.Controllers;
using Origam.ServerCore.Extensions;
using Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore
{
    public class UserService
    {
        private readonly ILogger<AbstractController> log;
        private readonly InternalPasswordHasherWithLegacySupport passwordHasher;
        private UserManager userManager;

        private int NumberOfInvalidPasswordAttempts { get; } = 3;
        
        public UserService(ILogger<AbstractController> log)
        {
            this.log = log;
            passwordHasher = new InternalPasswordHasherWithLegacySupport();
            userManager = new UserManager(
                userName => new User(userName),
                NumberOfInvalidPasswordAttempts);
        }

        public Result<IOrigamUser> Authenticate(string userName, string password)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "origam_server"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("name", "origam_server"),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            Thread.CurrentPrincipal = new ClaimsPrincipal(identity);
            
            return userManager.Find(userName, password);
        }

        public IOrigamUser GetById(int id)
        {
            throw new System.NotImplementedException();
        }

        public IOrigamUser Create(string user, string password)
        {
            throw new System.NotImplementedException();
        }

        public void Update(string user, string password = null)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}