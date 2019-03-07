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
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Origam.Security.Common;
using Origam.Security.Identity;
using Origam.ServerCore.Extensions;

namespace Origam.ServerCore.Controllers
{
  [Authorize]
  [Route("internalApi/[controller]")]
  public class AccountController : Controller
    {
        private readonly CoreUserManager userManager;
        private readonly SignInManager<IOrigamUser> signInManager;
        private readonly IMessageService messageService;
        private readonly IConfiguration configuration;
        private readonly IServiceProvider serviceProvioder;
        private readonly InternalUserManager internalUserManager;

        public AccountController(CoreUserManager userManager, SignInManager<IOrigamUser> signInManager, 
            IMessageService messageService, IConfiguration configuration, IServiceProvider serviceProvioder)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.messageService = messageService;
            this.configuration = configuration;
            this.serviceProvioder = serviceProvioder;
            //internalUserManager = new InternalUserManager(name=>new User(name),3, new NullTokenSender() );
            internalUserManager = new InternalUserManager(
                userName => new User(userName),
                numberOfInvalidPasswordAttempts: 3,
                frameworkSpecificManager: userManager,
                mailTemplateDirectoryPath: "",
                mailQueueName: "",
                portalBaseUrl:"",
                registerNewUserFilename:"",
                fromAddress:"",
                registerNewUserSubject: "",
                resetPwdSubject: "",
                resetPwdBodyFilename: "",
                userUnlockNotificationBodyFilename: "",
                userUnlockNotificationSubject:""
            );
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateInitialUser(string userName, string email,
            string password, string repassword)
        {
            if (password != repassword)
            {
                return BadRequest("Passwords don't match");
            }
            
            SetOrigamServerAsCurrentUser();
            
          
           //internalUserManager.CreateInitialUser(userName,password,"",userName,email);
//            if (!internalUserManager.IsInitialSetupNeeded())
//            {
//                return BadRequest("Initial user already exists");
//            }
            var newUser = new User 
            {
                UserName = userName,
                Email = email,
                RoleId = SecurityManager.BUILTIN_SUPER_USER_ROLE
            };

            var userCreationResult = await userManager.CreateAsync(newUser, password);
            if (!userCreationResult.Succeeded)
            {
                return BadRequest(userCreationResult.Errors.ToErrorMessage());
            }

            await userManager.AddClaimAsync(
                newUser, 
                new Claim(ClaimTypes.Role, "Administrator"));
            
            
            await SendMailWithVerificationToken(newUser);
//            return Content("Check your email for a verification link");
            internalUserManager.SetInitialSetupComplete();
            return Ok();
        }
        
        
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateNewUser(string userName, string email,
            string password, string repassword)
        {
            if (password != repassword)
            {
                return BadRequest("Passwords don't match");
            }
            
            SetOrigamServerAsCurrentUser();

            IdentityServiceAgent.ServiceProvider = serviceProvioder;
            
            var newUser = new User 
            {
                UserName = userName,
                Email = email,
            };

            var userCreationResult = await userManager.CreateAsync(newUser, password);
            if (!userCreationResult.Succeeded)
            {
                return BadRequest(userCreationResult.Errors.ToErrorMessage());
            }

//            await userManager.AddClaimAsync(
//                newUser, 
//                new Claim(ClaimTypes.Role, "Administrator"));
            
            
            return Ok();
        }

        private async Task SendMailWithVerificationToken(User user)
        {
            var emailConfirmationToken =
                await userManager.GenerateEmailConfirmationTokenAsync(user);
            var tokenVerificationUrl = Url.Action(
                "VerifyEmail",
                "Account",
                new
                {
                    id = user.Id,
                    token = emailConfirmationToken
                },
                Request.Scheme);

            await messageService.Send(user.Email, "Verify your email",
                $"Click <a href=\"{tokenVerificationUrl}\">here</a> to verify your email");
        }


        public async Task<IActionResult> VerifyEmail(string id, string token)
        {
            SetOrigamServerAsCurrentUser();
            var user = await userManager.FindByIdAsync(id);
            if(user == null)
                throw new InvalidOperationException();
            
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
//            if (!user.EmailConfirmed)
//            {
//                return BadRequest("Confirm your email first");
//            }

            //var passwordSignInResult = await signInManager.PasswordSignInAsync(userName, password, false, false);
            var passwordSignInResult =
                await signInManager.CheckPasswordSignInAsync(user, password, false);
//            
            if (!passwordSignInResult.Succeeded)
            {                
                await userManager.AccessFailedAsync(user);
                return BadRequest("Invalid login");
            }

            return Ok(GenerateToken(userName));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return Content("Check your email for a password reset link");

            var passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResetUrl = Url.Action(
                "ResetPassword", 
                "Account", 
                new {id = user.Id, token = passwordResetToken},
                Request.Scheme);

            await messageService.Send(email, "Password reset", $"Click <a href=\"" + passwordResetUrl + "\">here</a> to reset your password");

            return Content("Check your email for a password reset link");
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ResetPassword(string id, string token,
            string password, string repassword)
        {           
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