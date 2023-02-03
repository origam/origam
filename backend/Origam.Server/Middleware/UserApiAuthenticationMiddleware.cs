using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Origam.Server.Middleware;

/// <summary>
/// Middleware that performs authentication based on the IdentityServerAccessToken.
/// Based on AuthenticationMiddleware
/// https://github.com/dotnet/dotnet/blob/0fa4e2051eede834ddc4da42848a835aafc2f3da/src/aspnetcore/src/Security/Authentication/Core/src/AuthenticationMiddleware.cs
/// </summary>
public class UserApiAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public UserApiAuthenticationMiddleware(RequestDelegate next,
        IAuthenticationSchemeProvider schemes)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        Schemes = schemes ?? throw new ArgumentNullException(nameof(schemes));
    }

    public IAuthenticationSchemeProvider Schemes { get; set; }
    
    public async Task Invoke(HttpContext context)
    {
        context.Features.Set<IAuthenticationFeature>(new AuthenticationFeature
        {
            OriginalPath = context.Request.Path,
            OriginalPathBase = context.Request.PathBase
        });

        // Give any IAuthenticationRequestHandler schemes a chance to handle the request
        var handlers = context.RequestServices
            .GetRequiredService<IAuthenticationHandlerProvider>();
        foreach (var scheme in await Schemes.GetRequestHandlerSchemesAsync())
        {
            var handler =
                await handlers.GetHandlerAsync(context, scheme.Name) as
                    IAuthenticationRequestHandler;
            if (handler != null && await handler.HandleRequestAsync())
            {
                return;
            }
        }

        // Using the IdentityServerConstants.LocalApi.AuthenticationScheme here
        // causes the authentication to use the IdentityServerAccessToken.
        var result = await context.AuthenticateAsync(IdentityServerConstants.LocalApi.AuthenticationScheme);
        if (result?.Principal != null)
        {
            context.User = result.Principal;
        }

        if (result?.Succeeded ?? false)
        {
            var authFeatures = new OrigamAuthenticationFeatures(result);
            context.Features.Set<IHttpAuthenticationFeature>(authFeatures);
            context.Features.Set<IAuthenticateResultFeature>(authFeatures);
        }
        else
        {
            context.Response.StatusCode = 401;
            return;
        }
        await _next(context);
    }
}

class OrigamAuthenticationFeatures : IAuthenticateResultFeature, IHttpAuthenticationFeature
{
    private ClaimsPrincipal? _user;
    private AuthenticateResult? _result;

    public OrigamAuthenticationFeatures(AuthenticateResult result)
    {
        AuthenticateResult = result;
    }

    public AuthenticateResult? AuthenticateResult
    {
        get => _result;
        set
        {
            _result = value;
            _user = _result?.Principal;
        }
    }

    public ClaimsPrincipal? User
    {
        get => _user;
        set
        {
            _user = value;
            _result = null;
        }
    }
}