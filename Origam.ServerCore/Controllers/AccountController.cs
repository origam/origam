using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
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
using Origam.ServerCore.Models;

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
        public async Task<IActionResult> VerifyEmail(string id, string token)
        {
            SetOrigamServerAsCurrentUser();
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
                return BadRequest();
            
            var emailConfirmationResult = await userManager.ConfirmEmailAsync(user, token);
            if (!emailConfirmationResult.Succeeded)
            {
                return Content(emailConfirmationResult.Errors.ToErrorMessage());
            }

            return Content("Email confirmed, you can now log in");
        }      

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> Login(string userName, string password)
        {
            SetOrigamServerAsCurrentUser();
            
            var user = await userManager.FindByNameAsync(userName);
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
                await signInManager.CheckPasswordSignInAsync(user, password, false);
           
            if (!passwordSignInResult.Succeeded)
            {                
                await userManager.AccessFailedAsync(user);
                return BadRequest("Invalid login");
            }

            return Ok(GenerateToken(userName));
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            IdentityServiceAgent.ServiceProvider = serviceProvider;
            SetOrigamServerAsCurrentUser();
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return Content("Check your email for a password reset link");

            var passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            mailService.SendPasswordResetToken( user, passwordResetToken, 24 );           
            return Content("Check your email for a password reset link");
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> ResetPassword(string id, string token,
            string password, string repassword)
        {           
            SetOrigamServerAsCurrentUser();
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
                throw new InvalidOperationException();

            if (password != repassword)
            {
                return BadRequest("Passwords do not match");
            }

            var resetPasswordResult = 
                await userManager.ResetPasswordAsync(user, token, password);
            if (!resetPasswordResult.Succeeded)
            {
                return BadRequest(resetPasswordResult.Errors.ToErrorMessage());
            }

            return Content("Password updated");
        }
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok();
        }  
        
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