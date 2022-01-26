using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Origam.Security.Common;

namespace Origam.Server
{
    public class ProfileService : IProfileService
    {
        private readonly IUserClaimsPrincipalFactory<IOrigamUser> _claimsFactory;
        private readonly UserManager<IOrigamUser> _userManager;
 
        public ProfileService(UserManager<IOrigamUser> userManager, IUserClaimsPrincipalFactory<IOrigamUser> claimsFactory)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
        }
 
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            var principal = await _claimsFactory.CreateAsync(user);
 
            var claims = principal.Claims.ToList();
            claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();
 
            // // Add custom claims in token here based on user properties or any other source
            claims.Add(new Claim("name", user.UserName ?? string.Empty));
 
            context.IssuedClaims = claims;
        }
 
        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }
    }
}