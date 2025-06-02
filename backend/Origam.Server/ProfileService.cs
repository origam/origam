using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using OpenIddict.Server.Events;
using Microsoft.AspNetCore.Identity;
using Origam.Security.Common;

namespace Origam.Server;
public class ProfileService : IOpenIddictServerHandler<ProcessSignInContext>
{
    private readonly IUserClaimsPrincipalFactory<IOrigamUser> _claimsFactory;
    private readonly UserManager<IOrigamUser> _userManager;
    public ProfileService(UserManager<IOrigamUser> userManager, IUserClaimsPrincipalFactory<IOrigamUser> claimsFactory)
    {
        _userManager = userManager;
        _claimsFactory = claimsFactory;
    }
    public ValueTask HandleAsync(ProcessSignInContext context)
    {
        // TODO: migrate custom claims to OpenIddict events
        return ValueTask.CompletedTask;
    }
}
