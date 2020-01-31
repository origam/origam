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
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Origam.Security.Common;
using Origam.Security.Identity;
using Origam.ServerCore.Authorization;
using Origam.ServerCore.Configuration;
using Origam.ServerCore.Extensions;
using Origam.ServerCore.Model.User;

namespace Origam.ServerCore.Controller
{
    [ApiController]
    [Authorize]
    [Route("internalApi/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly CoreUserManager userManager;
        private readonly IMailService mailService;
        private readonly UserConfig userConfig;
        private readonly IServiceProvider serviceProvider;

        public UserController(CoreUserManager userManager, IServiceProvider serviceProvider,
            IMailService mailService, IOptions<UserConfig> userConfig)
        {
            this.userManager = userManager;
            this.mailService = mailService;
            this.userConfig = userConfig.Value;
            this.serviceProvider = serviceProvider;
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateInitialUser([FromBody] NewUserData userData)
        {
            if (userData.Password != userData.RePassword)
            {
                return BadRequest("Passwords don't match");
            }
            IdentityServiceAgent.ServiceProvider = serviceProvider;
            if (!userManager.IsInitialSetupNeeded())
            {
                return BadRequest("Initial user already exists");
            }
            var newUser = new User
            {
                FirstName = userData.FirstName,
                Name = userData.Name,
                UserName = userData.UserName,
                Email = userData.Email,
                RoleId = SecurityManager.BUILTIN_SUPER_USER_ROLE
            };
            var userCreationResult = await userManager.CreateAsync(newUser, userData.Password);
            if (!userCreationResult.Succeeded)
            {
                return BadRequest(userCreationResult.Errors.ToErrorMessage());
            }
            userManager.SetInitialSetupComplete();
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateNewUser([FromBody] NewUserData userData)
        {
            if (userData.Password != userData.RePassword)
            {
                return BadRequest("Passwords don't match");
            }
            IdentityServiceAgent.ServiceProvider = serviceProvider;
            var newUser = new User 
            {
                FirstName = userData.FirstName,
                Name = userData.Name,
                UserName = userData.UserName,
                Email = userData.Email,
                RoleId = userConfig.NewUserRoleId
            };
            var userCreationResult = await userManager.CreateAsync(newUser, userData.Password);
            if (!userCreationResult.Succeeded)
            {
                return BadRequest(userCreationResult.Errors.ToErrorMessage());
            }
            await SendMailWithVerificationToken(newUser);         
            return Ok();
        }

        private async Task SendMailWithVerificationToken(User user)
        {
            string token =
                await userManager.GenerateEmailConfirmationTokenAsync(user);
            mailService.SendNewUserToken(user,token);
        }



        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> VerifyEmail( [FromBody]VerifyEmailData verifyEmailData)
        {
            var user = await userManager.FindByIdAsync(verifyEmailData.Id);
            if (user == null)
                return BadRequest();
            var emailConfirmationResult = await userManager.ConfirmEmailAsync(user, verifyEmailData.Token);
            if (!emailConfirmationResult.Succeeded)
            {
                return Content(emailConfirmationResult.Errors.ToErrorMessage());
            }

            return Content("Email confirmed, you can now log in");
        }
    }
}