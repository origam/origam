using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Origam.Security.Common;

namespace Origam.Server.Authorization;

public class ClaimsFactory : UserClaimsPrincipalFactory<IOrigamUser>
{
    public ClaimsFactory(UserManager<IOrigamUser> userManager, IOptions<IdentityOptions> options)
        : base(userManager, options) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IOrigamUser user)
    {
        var id = await base.GenerateClaimsAsync(user);
        id.RemoveClaim(id.FindFirst(OpenIddictConstants.Claims.Subject));
        id.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, user.UserName));
        id.AddClaim(new Claim(OpenIddictConstants.Claims.Name, user.UserName ?? ""));
        if (!string.IsNullOrEmpty(user.Email))
        {
            id.AddClaim(new Claim(OpenIddictConstants.Claims.Email, user.Email));
        }

        return id;
    }
}
