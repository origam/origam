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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using OpenIddict.Validation.AspNetCore;
using Origam.Server.Configuration;
using Origam.Server.Middleware;

namespace Origam.ServerTests.Middleware;

public class UserApiAdditionalAuthenticationMiddlewareTests
{
    [TestCase("Token", "Cookie", "/api/legacy", "valid", null, 204, "Token")]
    [TestCase("Token", "Cookie", "/api/legacy", null, "valid", 204, "Cookie")]
    [TestCase("Token", "Cookie", "/api/legacy", "invalid", "valid", 204, "Cookie")]
    [TestCase("Token", "Cookie", "/api/legacy", "valid", "invalid", 204, "Token")]
    [TestCase("Token", "Cookie", "/api/legacy", "valid", "valid", 204, "Token")]
    [TestCase("Token", "Cookie", "/api/legacy", "invalid", "invalid", 401, null)]
    [TestCase("Token", "Cookie", "/api/legacy", null, null, 401, null)]
    [TestCase("Token", "Cookie", "/api/legacy2", null, "valid", 401, null)]
    [TestCase("Token", "Cookie", "/api/legacy/child", null, "valid", 401, null)]
    [TestCase("Token", null, "/api/legacy", null, "valid", 401, null)]
    [TestCase("Token", "Token", "/api/legacy", null, "valid", 401, null)]
    [TestCase("Cookie", "Token", "/api/legacy", "valid", null, 204, "Token")]
    [TestCase("Cookie", "Token", "/api/legacy", null, "valid", 204, "Cookie")]
    [TestCase("Cookie", "Token", "/api/legacy", "valid", "invalid", 204, "Token")]
    [TestCase("Cookie", "Token", "/api/legacy", "valid", "valid", 204, "Cookie")]
    [TestCase("Cookie", "Token", "/api/legacy", "invalid", "invalid", 401, null)]
    [TestCase("Cookie", "Token", "/api/legacy", null, null, 401, null)]
    [TestCase("Cookie", "Token", "/api/legacy2", "valid", null, 401, null)]
    [TestCase("Cookie", null, "/api/legacy", "valid", null, 401, null)]
    public async Task PipelineAcceptsOnlyConfiguredMethodsAndPreservesDefaultIdentity(
        string defaultMethod,
        string additionalMethod,
        string path,
        string token,
        string cookie,
        int expectedStatus,
        string expectedIdentity
    )
    {
        var settings = new Dictionary<string, string>
        {
            ["OpenIddictConfig:ClientApplicationTemplates:Configured"] = "true",
            ["OpenIddictConfig:PrivateApiAuthentication"] = defaultMethod,
        };
        if (additionalMethod != null)
        {
            settings["OpenIddictConfig:PrivateApiAdditionalAuthenticationMethods:/api/legacy:0"] =
                additionalMethod;
        }
        var configuration = new OpenIddictConfig(
            new ConfigurationBuilder().AddInMemoryCollection(settings).Build()
        );
        var authenticationService = new Mock<IAuthenticationService>();
        authenticationService
            .Setup(service =>
                service.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>())
            )
            .Returns<HttpContext, string>(
                (context, scheme) => Task.FromResult(Authenticate(context, scheme))
            );
        var services = new ServiceCollection();
        services.AddLogging();
        services
            .AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ApplicationScheme);
        services.AddSingleton(authenticationService.Object);
        using ServiceProvider provider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(provider);
        if (defaultMethod == "Token")
        {
            app.UseMiddleware<UserApiTokenAuthenticationMiddleware>();
        }
        else
        {
            app.UseAuthentication();
        }
        app.UseMiddleware<UserApiAdditionalAuthenticationMiddleware>(configuration);
        app.Run(context =>
        {
            context.Response.StatusCode =
                context.User.Identity?.IsAuthenticated == true ? 204 : 401;
            return Task.CompletedTask;
        });
        var requestContext = new DefaultHttpContext { RequestServices = provider };
        requestContext.Request.Path = path;
        if (token != null)
        {
            requestContext.Request.Headers.Authorization = "Bearer " + token;
        }
        if (cookie != null)
        {
            requestContext.Request.Headers.Cookie = cookie;
        }

        await app.Build()(requestContext);

        Assert.That(requestContext.Response.StatusCode, Is.EqualTo(expectedStatus));
        Assert.That(requestContext.User.Identity?.Name, Is.EqualTo(expectedIdentity));
        if (expectedIdentity != null)
        {
            Assert.That(
                requestContext
                    .Features.Get<IAuthenticateResultFeature>()
                    ?.AuthenticateResult?.Principal,
                Is.SameAs(requestContext.User)
            );
            Assert.That(
                requestContext.Features.Get<IHttpAuthenticationFeature>()?.User,
                Is.SameAs(requestContext.User)
            );
        }
        string defaultScheme =
            defaultMethod == "Token"
                ? OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
                : IdentityConstants.ApplicationScheme;
        authenticationService.Verify(
            service => service.AuthenticateAsync(requestContext, defaultScheme),
            Times.Once
        );
        if (
            expectedIdentity == defaultMethod
            || additionalMethod == null
            || !string.Equals(path, "/api/legacy", StringComparison.Ordinal)
        )
        {
            authenticationService.Verify(
                service =>
                    service.AuthenticateAsync(
                        requestContext,
                        It.Is<string>(scheme => scheme != defaultScheme)
                    ),
                Times.Never
            );
        }
    }

    private static AuthenticateResult Authenticate(HttpContext context, string scheme)
    {
        bool isCookie = scheme == IdentityConstants.ApplicationScheme;
        string credential = isCookie
            ? context.Request.Headers.Cookie.ToString()
            : context
                .Request.Headers.Authorization.ToString()
                .Replace("Bearer ", "", StringComparison.Ordinal);
        if (string.IsNullOrEmpty(credential))
        {
            return AuthenticateResult.NoResult();
        }
        if (credential != "valid")
        {
            return AuthenticateResult.Fail("Invalid test credential.");
        }
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, isCookie ? "Cookie" : "Token") },
            scheme
        );
        return AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identity), scheme)
        );
    }
}
