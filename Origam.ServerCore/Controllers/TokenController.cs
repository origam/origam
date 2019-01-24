using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;

namespace Origam.ServerCore.Controllers
{
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public TokenController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost("[action]")]
        public IActionResult Create(string username, string password)
        {
            if (IsValidUserAndPasswordCombination(username, password))
            {
                return new ObjectResult(GenerateToken(username));
            }
            return BadRequest();
        }

        private bool IsValidUserAndPasswordCombination(string username, string password)
        {
            return !string.IsNullOrEmpty(username) && username == password;
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