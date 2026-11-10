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
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using OpenIddict.Validation.AspNetCore;
using Origam.Server;
using Origam.Server.Configuration;
using SoapCore;

namespace Origam.ServerTests.Middleware;

public class SoapApiAuthenticationMiddlewareTests
{
    [TestCase(null, "true", "valid", null, 200, "Token")]
    [TestCase(null, "true", null, "valid", 200, "Cookie")]
    [TestCase(null, "true", "valid", "valid", 200, "Token")]
    [TestCase(null, "true", "invalid", "valid", 200, "Cookie")]
    [TestCase(null, "true", "valid", "invalid", 200, "Token")]
    [TestCase(null, "true", "invalid", "invalid", 401, null)]
    [TestCase(null, "true", null, null, 401, null)]
    [TestCase("CookieAndToken", "true", "valid", null, 200, "Token")]
    [TestCase("CookieAndToken", "true", null, "valid", 200, "Cookie")]
    [TestCase("CookieAndToken", "true", null, null, 401, null)]
    [TestCase("Cookie", "true", "valid", null, 401, null)]
    [TestCase("Cookie", "true", null, "valid", 200, "Cookie")]
    [TestCase("Cookie", "true", "valid", "valid", 200, "Cookie")]
    [TestCase("Cookie", "true", null, null, 401, null)]
    [TestCase("cookie", "true", "valid", null, 401, null)]
    [TestCase("Token", "true", "valid", null, 200, "Token")]
    [TestCase("Token", "true", null, "valid", 401, null)]
    [TestCase("Token", "true", "valid", "valid", 200, "Token")]
    [TestCase("Token", "true", "invalid", "valid", 401, null)]
    [TestCase("Token", "true", null, null, 401, null)]
    [TestCase("token", "true", null, "valid", 401, null)]
    [TestCase(null, "false", null, null, 200, null)]
    [TestCase("Cookie", "false", "valid", null, 200, null)]
    [TestCase("Token", "false", null, "valid", 200, null)]
    [TestCase("Token", "false", "valid", null, 200, "Token")]
    public async Task SoapApiAcceptsOnlyConfiguredAuthenticationMethods(
        string authenticationMethod,
        string requiresAuthentication,
        string token,
        string cookie,
        int expectedStatus,
        string expectedIdentity
    )
    {
        StartUpConfiguration configuration = CreateConfiguration(
            authenticationMethod,
            requiresAuthentication
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
        services.AddSoapCore();
        services
            .AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ApplicationScheme);
        services.AddSingleton(authenticationService.Object);
        using ServiceProvider provider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(provider);
        app.UseAuthentication();
        app.UseSoapApi(
            configuration.SoapInterfaceRequiresAuthentication,
            configuration.SoapInterfaceAuthenticationMethod,
            expectAndReturnOldDotNetAssemblyReferences: false
        );
        var requestContext = new DefaultHttpContext { RequestServices = provider };
        requestContext.Request.Method = HttpMethods.Get;
        requestContext.Request.Scheme = "https";
        requestContext.Request.Host = new HostString("localhost");
        requestContext.Request.Path = "/soap/DataService";
        requestContext.Response.Body = new MemoryStream();
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
        if (configuration.SoapInterfaceAuthenticationMethod == SoapApiAuthenticationMethod.Cookie)
        {
            authenticationService.Verify(
                service =>
                    service.AuthenticateAsync(
                        requestContext,
                        OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
                    ),
                Times.Never
            );
        }
    }

    [Test]
    public void UnknownAuthenticationMethodIsRejected()
    {
        StartUpConfiguration configuration = CreateConfiguration(
            authenticationMethod: "Basic",
            requiresAuthentication: "true"
        );

        Assert.That(
            () => configuration.SoapInterfaceAuthenticationMethod,
            Throws.InvalidOperationException
        );
    }

    private static StartUpConfiguration CreateConfiguration(
        string authenticationMethod,
        string requiresAuthentication
    )
    {
        var settings = new Dictionary<string, string>
        {
            ["SoapAPI:RequiresAuthentication"] = requiresAuthentication,
        };
        if (authenticationMethod != null)
        {
            settings["SoapAPI:AuthenticationMethod"] = authenticationMethod;
        }
        return new StartUpConfiguration(
            new ConfigurationBuilder().AddInMemoryCollection(settings).Build()
        );
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
