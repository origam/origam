#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using OpenIddict.Validation.AspNetCore;
using Origam.Server.Middleware;

namespace Origam.ServerTests.Middleware;

[TestFixture]
public class UserApiTokenAuthenticationMiddlewareTests
{
    private const string CookieUserName = "cookieUser";
    private const string TokenUserName = "tokenUser";

    [TestCase("iframe", "same-origin", CookieUserName)]
    [TestCase("iframe", "cross-site", null)]
    [TestCase("iframe", "same-site", null)]
    [TestCase("document", "same-origin", null)]
    [TestCase(null, null, null)]
    public async Task CookieAuthenticatesOnlySameOriginFrameRequestWithoutToken(
        string fetchDestination,
        string fetchSite,
        string expectedUserName
    )
    {
        DefaultHttpContext context = CreateContext(tokenUserName: null);
        context.Request.Headers["Sec-Fetch-Dest"] = fetchDestination;
        context.Request.Headers["Sec-Fetch-Site"] = fetchSite;

        bool nextCalled = await InvokeMiddleware(context);

        Assert.That(nextCalled, Is.True);
        Assert.That(context.User.Identity?.Name, Is.EqualTo(expectedUserName));
    }

    [Test]
    public async Task TokenTakesPrecedenceOverCookieInSameOriginFrameRequest()
    {
        DefaultHttpContext context = CreateContext(TokenUserName);
        context.Request.Headers["Sec-Fetch-Dest"] = "iframe";
        context.Request.Headers["Sec-Fetch-Site"] = "same-origin";

        await InvokeMiddleware(context);

        Assert.That(context.User.Identity?.Name, Is.EqualTo(TokenUserName));
    }

    private static async Task<bool> InvokeMiddleware(HttpContext context)
    {
        bool nextCalled = false;
        var schemeProvider = new Mock<IAuthenticationSchemeProvider>();
        schemeProvider
            .Setup(provider => provider.GetRequestHandlerSchemesAsync())
            .ReturnsAsync(Enumerable.Empty<AuthenticationScheme>());
        var middleware = new UserApiTokenAuthenticationMiddleware(
            nextContext =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            schemeProvider.Object
        );
        await middleware.Invoke(context);
        return nextCalled;
    }

    private static DefaultHttpContext CreateContext(string tokenUserName)
    {
        var authenticationService = new Mock<IAuthenticationService>();
        authenticationService
            .Setup(service =>
                service.AuthenticateAsync(
                    It.IsAny<HttpContext>(),
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
                )
            )
            .ReturnsAsync(
                tokenUserName == null
                    ? AuthenticateResult.NoResult()
                    : CreateSuccess(
                        tokenUserName,
                        OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
                    )
            );
        authenticationService
            .Setup(service =>
                service.AuthenticateAsync(
                    It.IsAny<HttpContext>(),
                    IdentityConstants.ApplicationScheme
                )
            )
            .ReturnsAsync(CreateSuccess(CookieUserName, IdentityConstants.ApplicationScheme));
        ServiceProvider services = new ServiceCollection()
            .AddSingleton(authenticationService.Object)
            .AddSingleton(Mock.Of<IAuthenticationHandlerProvider>())
            .BuildServiceProvider();
        return new DefaultHttpContext { RequestServices = services };
    }

    private static AuthenticateResult CreateSuccess(string userName, string scheme)
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, userName)], scheme);
        return AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identity), scheme)
        );
    }
}
