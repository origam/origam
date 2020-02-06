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
        private readonly SignInManager<IOrigamUser> signInManager;
        private readonly IMailService mailService;
        private readonly UserConfig userConfig;
        private readonly IConfiguration configuration;
        private readonly IServiceProvider serviceProvider;

        public UserController(CoreUserManager userManager, SignInManager<IOrigamUser> signInManager,
            IConfiguration configuration,
            IMailService mailService, IOptions<UserConfig> userConfig)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.mailService = mailService;
            this.userConfig = userConfig.Value;
            this.configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateInitialUser([FromBody] NewUserData userData)
        {
            if (userData.Password != userData.RePassword)
            {
                return BadRequest("Passwords don't match");
            }
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

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody]LoginData loginData)
        {
            var user = await userManager.FindByNameAsync(loginData.UserName);
            if (user == null)
            {
                return BadRequest("Invalid login");
            }
            if (!user.EmailConfirmed)
            {
                return BadRequest("Confirm your email first");
            }
            var passwordSignInResult =
                await signInManager.CheckPasswordSignInAsync(user, loginData.Password, false);
            if (!passwordSignInResult.Succeeded)
            {                
                await userManager.AccessFailedAsync(user);
                return BadRequest("Invalid login");
            }
            return Ok(GenerateToken(loginData.UserName));
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> RequestPasswordReset([FromBody]RequestPasswordResetData passwordData) 
        {
            var user = await userManager.FindByEmailAsync(passwordData.Email);
            if (user == null)
            {
                return Content("User with that email was not found");
            }
            var passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            mailService.SendPasswordResetToken( user, passwordResetToken, 24 );           
            return Content("Check your email for a password reset link");
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> ResetPassword([FromBody]ResetPasswordData passwordData)
        {           
            var user = await userManager.FindByIdAsync(passwordData.Id);
            if (user == null)
            {
                throw new InvalidOperationException();
            }
            if (passwordData.Password != passwordData.RePassword)
            {
                return BadRequest("Passwords do not match");
            }
            var resetPasswordResult = 
                await userManager.ResetPasswordAsync(user, passwordData.Token, passwordData.Password);
            if (!resetPasswordResult.Succeeded)
            {
                return BadRequest(resetPasswordResult.Errors.ToErrorMessage());
            }
            return Content("Password updated");
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok();
        }  
                
        private string GenerateToken(string username)
        {
            var claims = new []
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString()),
            };
            var token = new JwtSecurityToken(
                new JwtHeader(new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["SecurityKey"])),
                    SecurityAlgorithms.HmacSha256)),
                new JwtPayload(claims));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}