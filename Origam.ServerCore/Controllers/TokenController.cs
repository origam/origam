using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Origam.Security.Common;

namespace Origam.ServerCore.Controllers
{
    [Authorize]
    [Route("internalApi/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly UserService userService;

        public TokenController(IConfiguration configuration, UserService userService)
        {
            this.configuration = configuration;
            this.userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult Create(string username, string password)
        {
            Result<IOrigamUser> userResult = userService.Authenticate(username, password);
            if (userResult.IsSuccess)
            {
                return Ok(GenerateToken(username));
            }
            return BadRequest(userResult.Error);
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