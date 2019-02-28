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

namespace Origam.ServerCore.Controllers
{
  [Authorize]
  [Route("internalApi/[controller]")]
  public class AccountController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IMessageService messageService;
        private readonly IConfiguration configuration;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, 
            IMessageService messageService, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.messageService = messageService;
            this.configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> Register(string email, string password, string repassword)
        {
            if (password != repassword)
            {
                return BadRequest("Passwords don't match");
            }

            var newUser = new User 
            {
                UserName = email,
                Email = email
            };

            var userCreationResult = await userManager.CreateAsync(newUser, password);
            if (!userCreationResult.Succeeded)
            {
                foreach(var error in userCreationResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return BadRequest();
            }

            await userManager.AddClaimAsync(newUser, new Claim(ClaimTypes.Role, "Administrator"));
            
            
            var emailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(newUser);
            var tokenVerificationUrl = Url.Action("VerifyEmail", "Account", new {id = newUser.Id, token = emailConfirmationToken}, Request.Scheme);

            await messageService.Send(email, "Verify your email", $"Click <a href=\"{tokenVerificationUrl}\">here</a> to verify your email");

            return Content("Check your email for a verification link");
        }


        public async Task<IActionResult> VerifyEmail(string id, string token)
        {
            var user = await userManager.FindByIdAsync(id);
            if(user == null)
                throw new InvalidOperationException();
            
            var emailConfirmationResult = await userManager.ConfirmEmailAsync(user, token);
            if (!emailConfirmationResult.Succeeded)            
                return Content(emailConfirmationResult.Errors.Select(error => error.Description).Aggregate((allErrors, error) => allErrors += ", " + error));                            

            return Content("Email confirmed, you can now log in");
        }      

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> Login(string userName, string password)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "origam_server"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("name", "origam_server"),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            Thread.CurrentPrincipal = new ClaimsPrincipal(identity);
            
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
            var passwordSignInResult = await signInManager.CheckPasswordSignInAsync(user, password, false);
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
            var passwordResetUrl = Url.Action("ResetPassword", "Account", new {id = user.Id, token = passwordResetToken}, Request.Scheme);

            await messageService.Send(email, "Password reset", $"Click <a href=\"" + passwordResetUrl + "\">here</a> to reset your password");

            return Content("Check your email for a password reset link");
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ResetPassword(string id, string token, string password, string repassword)
        {           
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
                throw new InvalidOperationException();

            if (password != repassword)
            {
                return BadRequest("Passwords do not match");
            }

            var resetPasswordResult = await userManager.ResetPasswordAsync(user, token, password);
            if (!resetPasswordResult.Succeeded)
            {
                return BadRequest(resetPasswordResult.Errors.Aggregate("",(x, y) => x + y.Description));
            }

            return Content("Password updated");
        }
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