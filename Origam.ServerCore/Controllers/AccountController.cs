#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using System.ComponentModel.DataAnnotations;
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
using Origam.ServerCore.Models.Account;

namespace Origam.ServerCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("internalApi/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly CoreUserManager userManager;
        private readonly SignInManager<IOrigamUser> signInManager;
        private readonly IMailService mailService;
        private readonly AccountConfig accountConfig;
        private readonly IConfiguration configuration;
        private readonly IServiceProvider serviceProvider;

        public AccountController(CoreUserManager userManager, SignInManager<IOrigamUser> signInManager,
            IConfiguration configuration, IServiceProvider serviceProvider,
            IMailService mailService, IOptions<AccountConfig> accountConfig)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.mailService = mailService;
            this.accountConfig = accountConfig.Value;
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateInitialUser([FromBody] NewUserModel userModel)
        {
            if (userModel.Password != userModel.RePassword)
            {
                return BadRequest("Passwords don't match");
            }

            SetOrigamServerAsCurrentUser();
            IdentityServiceAgent.ServiceProvider = serviceProvider;

            if (!userManager.IsInitialSetupNeeded())
            {
                return BadRequest("Initial user already exists");
            }

            var newUser = new User
            {
                FirstName = userModel.FirstName,
                Name = userModel.Name,
                UserName = userModel.UserName,
                Email = userModel.Email,
                RoleId = SecurityManager.BUILTIN_SUPER_USER_ROLE
            };

            var userCreationResult = await userManager.CreateAsync(newUser, userModel.Password);
            if (!userCreationResult.Succeeded)
            {
                return BadRequest(userCreationResult.Errors.ToErrorMessage());
            }

            userManager.SetInitialSetupComplete();
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateNewUser([FromBody] NewUserModel userModel)
        {
            if (userModel.Password != userModel.RePassword)
            {
                return BadRequest("Passwords don't match");
            }
            
            SetOrigamServerAsCurrentUser();

            IdentityServiceAgent.ServiceProvider = serviceProvider;
            
            var newUser = new User 
            {
                FirstName = userModel.FirstName,
                Name = userModel.Name,
                UserName = userModel.UserName,
                Email = userModel.Email,
                RoleId = accountConfig.NewUserRoleId
            };

            var userCreationResult = await userManager.CreateAsync(newUser, userModel.Password);
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
            SetOrigamServerAsCurrentUser();
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
            SetOrigamServerAsCurrentUser();
            
            var user = await userManager.FindByNameAsync(loginData.UserName);
            if (user == null)
            {
                return BadRequest("Invalid login");
            }
            if (!user.EmailConfirmed)
            {
                return BadRequest("Confirm your email first");
            }

            //var passwordSignInResult = await signInManager.PasswordSignInAsync(userName, password, false, false);
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
        public async Task<IActionResult> ForgotPassword([FromBody]ForgotPasswordData passwordData) 
        {
            IdentityServiceAgent.ServiceProvider = serviceProvider;
            SetOrigamServerAsCurrentUser();
            var user = await userManager.FindByEmailAsync(passwordData.Email);
            if (user == null)
                return Content("User with that email was not found");

            var passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            mailService.SendPasswordResetToken( user, passwordResetToken, 24 );           
            return Content("Check your email for a password reset link");
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> ResetPassword([FromBody]ResetPasswordData passwordData)
        {           
            SetOrigamServerAsCurrentUser();
            var user = await userManager.FindByIdAsync(passwordData.Id);
            if (user == null)
                throw new InvalidOperationException();

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

//        [HttpPost("[action]")]
//        public async Task<IActionResult> Logout()
//        {
//            await signInManager.SignOutAsync();
//            return Ok();
//        }  
        
        private static void SetOrigamServerAsCurrentUser()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "origam_server"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("name", "origam_server"),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            Thread.CurrentPrincipal = new ClaimsPrincipal(identity);
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