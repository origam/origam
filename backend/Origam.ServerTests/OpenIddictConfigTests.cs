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

using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Origam.Server.Configuration;

namespace Origam.ServerTests;

public class OpenIddictConfigTests
{
    [TestCase(null, AuthenticationMethod.Cookie)]
    [TestCase("Cookie", AuthenticationMethod.Cookie)]
    [TestCase("Token", AuthenticationMethod.Token)]
    public void MissingAdditionalMethodsPreservesDefault(
        string defaultMethod,
        AuthenticationMethod expected
    )
    {
        var config = CreatePrivateApiConfiguration(defaultMethod);

        Assert.That(
            config.GetPrivateApiAuthenticationMethods("/api/legacy"),
            Is.EqualTo(new[] { expected })
        );
        Assert.That(config.PrivateApiAdditionalAuthenticationMethods, Is.Empty);
    }

    [TestCase("Token", "Cookie", AuthenticationMethod.Token, AuthenticationMethod.Cookie)]
    [TestCase("Cookie", "Token", AuthenticationMethod.Cookie, AuthenticationMethod.Token)]
    [TestCase("Token", "cookie", AuthenticationMethod.Token, AuthenticationMethod.Cookie)]
    [TestCase("Cookie", "token", AuthenticationMethod.Cookie, AuthenticationMethod.Token)]
    public void AdditionalMethodsExtendDefaultOnlyForExactEndpoint(
        string defaultMethod,
        string additionalMethod,
        AuthenticationMethod expectedDefault,
        AuthenticationMethod expectedAdditional
    )
    {
        var config = CreatePrivateApiConfiguration(defaultMethod, additionalMethod);

        Assert.That(
            config.GetPrivateApiAuthenticationMethods("/api/legacy"),
            Is.EqualTo(new[] { expectedDefault, expectedAdditional })
        );
        foreach (
            string path in new[]
            {
                "/api/other",
                "/api/legacy/child",
                "/api/legacy2",
                "/api/legacy/",
            }
        )
        {
            Assert.That(
                config.GetPrivateApiAuthenticationMethods(path),
                Is.EqualTo(new[] { expectedDefault })
            );
        }
    }

    [Test]
    public void RepeatedMethodsAreAcceptedOnlyOnce()
    {
        var settings = new Dictionary<string, string>
        {
            ["OpenIddictConfig:ClientApplicationTemplates:Configured"] = "true",
            ["OpenIddictConfig:PrivateApiAuthentication"] = "Token",
            ["OpenIddictConfig:PrivateApiAdditionalAuthenticationMethods:/api/legacy:0"] = "Token",
            ["OpenIddictConfig:PrivateApiAdditionalAuthenticationMethods:/api/legacy:1"] = "Cookie",
            ["OpenIddictConfig:PrivateApiAdditionalAuthenticationMethods:/api/legacy:2"] = "Cookie",
        };
        var config = new OpenIddictConfig(
            new ConfigurationBuilder().AddInMemoryCollection(settings).Build()
        );

        Assert.That(
            config.GetPrivateApiAuthenticationMethods("/api/legacy"),
            Is.EqualTo(new[] { AuthenticationMethod.Token, AuthenticationMethod.Cookie })
        );
    }

    [TestCase("Unknown")]
    [TestCase("42")]
    [TestCase("-1")]
    [TestCase("2147483648")]
    [TestCase("")]
    public void InvalidAdditionalMethodIsRejectedWithDescriptiveMessage(string suppliedValue)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreatePrivateApiConfiguration("Token", suppliedValue)
        );

        Assert.That(
            exception.Message,
            Does.Contain(
                $"Invalid authentication method '{suppliedValue}' for endpoint '/api/legacy'. Valid values are: Cookie, Token."
            )
        );
        Assert.That(
            exception.ParamName,
            Is.EqualTo(nameof(OpenIddictConfig.PrivateApiAdditionalAuthenticationMethods))
        );
        Assert.That(exception.ActualValue, Is.EqualTo(suppliedValue));
    }

    [Test]
    public void AdditionalEndpointPathMustStartWithSlash()
    {
        Assert.Throws<ArgumentException>(() =>
            CreatePrivateApiConfiguration("Token", "Cookie", "api/legacy")
        );
    }

    [TestCase(null, 60)]
    [TestCase("15", 15)]
    [TestCase("120", 120)]
    public void AccessTokenExpirationDefaultsToOneHourAndCanBeConfigured(
        string configuredMinutes,
        int expectedMinutes
    )
    {
        var config = new OpenIddictConfig(CreateConfiguration(configuredMinutes));

        Assert.That(config.AccessTokenExpirationMinutes, Is.EqualTo(expectedMinutes));
    }

    [TestCase("0")]
    [TestCase("-1")]
    public void AccessTokenExpirationMustBePositive(string configuredMinutes)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OpenIddictConfig(CreateConfiguration(configuredMinutes))
        );
    }

    private static IConfiguration CreateConfiguration(string configuredMinutes)
    {
        var settings = new Dictionary<string, string>
        {
            ["OpenIddictConfig:ClientApplicationTemplates:Configured"] = "true",
        };
        if (configuredMinutes != null)
        {
            settings["OpenIddictConfig:AccessTokenExpirationMinutes"] = configuredMinutes;
        }
        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }

    private static OpenIddictConfig CreatePrivateApiConfiguration(
        string defaultMethod,
        string additionalMethod = null,
        string path = "/api/legacy"
    )
    {
        var settings = new Dictionary<string, string>
        {
            ["OpenIddictConfig:ClientApplicationTemplates:Configured"] = "true",
        };
        if (defaultMethod != null)
        {
            settings["OpenIddictConfig:PrivateApiAuthentication"] = defaultMethod;
        }
        if (additionalMethod != null)
        {
            settings[$"OpenIddictConfig:PrivateApiAdditionalAuthenticationMethods:{path}:0"] =
                additionalMethod;
        }
        return new OpenIddictConfig(
            new ConfigurationBuilder().AddInMemoryCollection(settings).Build()
        );
    }
}
