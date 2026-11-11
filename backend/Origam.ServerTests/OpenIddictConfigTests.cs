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

using System.Text;
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

    [TestCase("Token", AuthenticationMethod.Token)]
    [TestCase("Cookie", AuthenticationMethod.Cookie)]
    public void AdditionalMethodMatchingDefaultIsAcceptedOnlyOnce(
        string method,
        AuthenticationMethod expected
    )
    {
        var config = CreatePrivateApiConfiguration(method, method);

        Assert.That(
            config.GetPrivateApiAuthenticationMethods("/api/legacy"),
            Is.EqualTo(new[] { expected })
        );
    }

    [Test]
    public void AdditionalMethodsAreReadFromJsonList()
    {
        using var stream = new MemoryStream(
            Encoding.UTF8.GetBytes(
                """
                {
                  "OpenIddictConfig": {
                    "ClientApplicationTemplates": { "Configured": "true" },
                    "PrivateApiAuthentication": "Token",
                    "PrivateApiAdditionalAuthenticationMethods": [
                      { "Path": "/api/order/{orderId}/status/{statusId}", "Method": "cookie" },
                      { "Path": "/api/order/1/status/current", "Method": "Token" },
                      { "Path": "/api/handover/{handoverId}/view", "Method": "Cookie" }
                    ]
                  }
                }
                """
            )
        );
        var config = new OpenIddictConfig(new ConfigurationBuilder().AddJsonStream(stream).Build());

        Assert.That(config.PrivateApiAdditionalAuthenticationMethods, Has.Count.EqualTo(3));
        foreach (string path in new[] { "/api/order/1/status/2", "/api/handover/3/view" })
        {
            Assert.That(
                config.GetPrivateApiAuthenticationMethods(path),
                Is.EqualTo(new[] { AuthenticationMethod.Token, AuthenticationMethod.Cookie })
            );
        }
        foreach (
            string path in new[]
            {
                "/api/order/1/status/current",
                "/api/order/1/status/2/child",
                "/api/handover//view",
                "/api/handover/3/View",
                "/api/handover/3/view/",
            }
        )
        {
            Assert.That(
                config.GetPrivateApiAuthenticationMethods(path),
                Is.EqualTo(new[] { AuthenticationMethod.Token })
            );
        }
    }

    [Test]
    [NonParallelizable]
    public void AdditionalMethodsAreReadFromPortableEnvironmentVariables()
    {
        string prefix = "OrigamAuthTest_" + Guid.NewGuid().ToString("N") + "__";
        var environment = new Dictionary<string, string>
        {
            [prefix + "OpenIddictConfig__PrivateApiAuthentication"] = "Token",
            [prefix + "OpenIddictConfig__PrivateApiAdditionalAuthenticationMethods__0__Path"] =
                "/api/order/{orderId}/status/{statusId}",
            [prefix + "OpenIddictConfig__PrivateApiAdditionalAuthenticationMethods__0__Method"] =
                "Cookie",
            [prefix + "OpenIddictConfig__PrivateApiAdditionalAuthenticationMethods__1__Path"] =
                "/api/handover/{handoverId}/view",
            [prefix + "OpenIddictConfig__PrivateApiAdditionalAuthenticationMethods__1__Method"] =
                "Cookie",
        };
        try
        {
            foreach (var variable in environment)
            {
                Environment.SetEnvironmentVariable(variable.Key, variable.Value);
            }
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string>
                    {
                        ["OpenIddictConfig:ClientApplicationTemplates:Configured"] = "true",
                    }
                )
                .AddEnvironmentVariables(prefix)
                .Build();
            var config = new OpenIddictConfig(configuration);

            Assert.That(config.PrivateApiAdditionalAuthenticationMethods, Has.Count.EqualTo(2));
            Assert.That(
                config.GetPrivateApiAuthenticationMethods("/api/order/123/status/456"),
                Is.EqualTo(new[] { AuthenticationMethod.Token, AuthenticationMethod.Cookie })
            );
            Assert.That(
                config.GetPrivateApiAuthenticationMethods("/api/handover/123/view"),
                Is.EqualTo(new[] { AuthenticationMethod.Token, AuthenticationMethod.Cookie })
            );
            Assert.That(
                config.GetPrivateApiAuthenticationMethods("/api/other"),
                Is.EqualTo(new[] { AuthenticationMethod.Token })
            );
        }
        finally
        {
            foreach (string key in environment.Keys)
            {
                Environment.SetEnvironmentVariable(key, null);
            }
        }
    }

    [TestCase(null)]
    [TestCase("Unknown")]
    [TestCase("42")]
    [TestCase("")]
    public void InvalidListMethodIsRejected(string method)
    {
        var settings = CreateListSettings("/api/legacy", method);
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OpenIddictConfig(new ConfigurationBuilder().AddInMemoryCollection(settings).Build())
        );

        Assert.That(exception.Message, Does.Contain("/api/legacy"));
        Assert.That(exception.Message, Does.Contain("Cookie, Token"));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("api/legacy")]
    public void InvalidListPathIsRejected(string path)
    {
        Assert.Catch<ArgumentException>(() =>
            new OpenIddictConfig(
                new ConfigurationBuilder()
                    .AddInMemoryCollection(CreateListSettings(path, "Cookie"))
                    .Build()
            )
        );
    }

    [Test]
    public void DuplicateListPathsAreRejected()
    {
        var settings = CreateListSettings("/api/legacy", "Cookie");
        settings["OpenIddictConfig:PrivateApiAdditionalAuthenticationMethods:1:Path"] =
            "/api/legacy";
        settings["OpenIddictConfig:PrivateApiAdditionalAuthenticationMethods:1:Method"] = "Token";

        Assert.Throws<ArgumentException>(() =>
            new OpenIddictConfig(new ConfigurationBuilder().AddInMemoryCollection(settings).Build())
        );
    }

    private static Dictionary<string, string> CreateListSettings(string path, string method)
    {
        return new Dictionary<string, string>
        {
            ["OpenIddictConfig:ClientApplicationTemplates:Configured"] = "true",
            ["OpenIddictConfig:PrivateApiAuthentication"] = "Token",
            ["OpenIddictConfig:PrivateApiAdditionalAuthenticationMethods:0:Path"] = path,
            ["OpenIddictConfig:PrivateApiAdditionalAuthenticationMethods:0:Method"] = method,
        };
    }

    [TestCase("/api/legacy/address-1", true)]
    [TestCase("/api/legacy/address-2", true)]
    [TestCase("/api/legacy/{addressId}", true)]
    [TestCase("/api/legacy", false)]
    [TestCase("/api/legacy/", false)]
    [TestCase("/api/legacy/address-1/", false)]
    [TestCase("/api/legacy/address-1/delete", false)]
    [TestCase("/api/legacy2/address-1", false)]
    [TestCase("/api/Legacy/address-1", false)]
    [TestCase("/api/legacy//address-1", false)]
    public void ParameterizedExceptionMatchesOnlyItsCompletePath(string path, bool matches)
    {
        var config = CreatePrivateApiConfiguration("Token", "Cookie", "/api/legacy/{addressId}");

        Assert.That(
            config.GetPrivateApiAuthenticationMethods(path).Contains(AuthenticationMethod.Cookie),
            Is.EqualTo(matches)
        );
    }

    [TestCase("/api/private/some-path/test/test-1/view", true)]
    [TestCase("/api/private/some-path/test/test-2/view", true)]
    [TestCase("/api/private/some-path/test/test-1/edit", false)]
    [TestCase("/api/private/some-path/test/test-1/View", false)]
    [TestCase("/api/private/some-path/test/test-1", false)]
    [TestCase("/api/private/some-path/test//view", false)]
    [TestCase("/api/private/some-path/test/test-1/view/child", false)]
    [TestCase("/api/private/some-path/test/test-1/view/", false)]
    public void ParameterizedExceptionMatchesParameterBeforeFinalLiteralSegment(
        string path,
        bool matches
    )
    {
        var config = CreatePrivateApiConfiguration(
            "Token",
            "Cookie",
            "/api/private/some-path/test/{testId}/view"
        );

        Assert.That(
            config.GetPrivateApiAuthenticationMethods(path).Contains(AuthenticationMethod.Cookie),
            Is.EqualTo(matches)
        );
    }

    [TestCase("/api/order/order-1/status/status-1", true)]
    [TestCase("/api/order/order-1/other/status-1", false)]
    [TestCase("/api/order//status/status-1", false)]
    [TestCase("/api/order/order-1/status", false)]
    [TestCase("/api/order/order-1/status/", false)]
    [TestCase("/api/order/order-1/status/status-1/child", false)]
    public void ParameterizedExceptionSupportsMultipleParameters(string path, bool matches)
    {
        var config = CreatePrivateApiConfiguration(
            "Token",
            "Cookie",
            "/api/order/{orderId}/status/{statusId}"
        );

        Assert.That(
            config.GetPrivateApiAuthenticationMethods(path).Contains(AuthenticationMethod.Cookie),
            Is.EqualTo(matches)
        );
    }

    [TestCase("/api/report.v1/items+/item-1", true)]
    [TestCase("/api/reportXv1/items+/item-1", false)]
    [TestCase("/api/report.v1/items/item-1", false)]
    public void ParameterizedExceptionPreservesLiteralCharacters(string path, bool matches)
    {
        var config = CreatePrivateApiConfiguration("Token", "Cookie", "/api/report.v1/items+/{id}");

        Assert.That(
            config.GetPrivateApiAuthenticationMethods(path).Contains(AuthenticationMethod.Cookie),
            Is.EqualTo(matches)
        );
    }

    [TestCase("/api/legacy/{}")]
    [TestCase("/api/legacy/{id")]
    [TestCase("/api/legacy/{id}{other}")]
    [TestCase("/api/legacy/prefix-{id}")]
    public void OnlyWholeWellFormedParameterSegmentsAreExpanded(string endpoint)
    {
        var config = CreatePrivateApiConfiguration("Token", "Cookie", endpoint);

        Assert.That(
            config.GetPrivateApiAuthenticationMethods("/api/legacy/value"),
            Is.EqualTo(new[] { AuthenticationMethod.Token })
        );
    }

    [Test]
    public void ExactExceptionTakesPrecedenceOverParameterizedException()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string>
                {
                    ["OpenIddictConfig:ClientApplicationTemplates:Configured"] = "true",
                    ["OpenIddictConfig:PrivateApiAuthentication"] = "Token",
                    ["OpenIddictConfig:PrivateApiAdditionalAuthenticationMethods:0:Path"] =
                        "/api/order/{orderId}",
                    ["OpenIddictConfig:PrivateApiAdditionalAuthenticationMethods:0:Method"] =
                        "Cookie",
                    ["OpenIddictConfig:PrivateApiAdditionalAuthenticationMethods:1:Path"] =
                        "/api/order/submit",
                    ["OpenIddictConfig:PrivateApiAdditionalAuthenticationMethods:1:Method"] =
                        "Token",
                }
            )
            .Build();
        var config = new OpenIddictConfig(configuration);

        Assert.That(
            config.GetPrivateApiAuthenticationMethods("/api/order/submit"),
            Is.EqualTo(new[] { AuthenticationMethod.Token })
        );
        Assert.That(
            config.GetPrivateApiAuthenticationMethods("/api/order/order-1"),
            Is.EqualTo(new[] { AuthenticationMethod.Token, AuthenticationMethod.Cookie })
        );
    }

    [TestCase("Token", "Cookie", AuthenticationMethod.Token, AuthenticationMethod.Cookie)]
    [TestCase("Cookie", "Token", AuthenticationMethod.Cookie, AuthenticationMethod.Token)]
    [TestCase("Token", "Token", AuthenticationMethod.Token, AuthenticationMethod.Token)]
    public void ParameterizedExceptionPreservesDefaultAndAvoidsDuplicates(
        string defaultMethod,
        string additionalMethod,
        AuthenticationMethod expectedDefault,
        AuthenticationMethod expectedAdditional
    )
    {
        var config = CreatePrivateApiConfiguration(
            defaultMethod,
            additionalMethod,
            "/api/legacy/{id}"
        );
        var methods = config.GetPrivateApiAuthenticationMethods("/api/legacy/value").ToArray();

        Assert.That(methods[0], Is.EqualTo(expectedDefault));
        Assert.That(
            methods,
            Is.EquivalentTo(new[] { expectedDefault, expectedAdditional }.Distinct())
        );
    }

    [Test]
    public void DictionaryOfAdditionalMethodsIsRejected()
    {
        var settings = new Dictionary<string, string>
        {
            ["OpenIddictConfig:ClientApplicationTemplates:Configured"] = "true",
            ["OpenIddictConfig:PrivateApiAuthentication"] = "Token",
            ["OpenIddictConfig:PrivateApiAdditionalAuthenticationMethods:/api/legacy"] = "Cookie",
        };

        var exception = Assert.Throws<ArgumentNullException>(() =>
            new OpenIddictConfig(new ConfigurationBuilder().AddInMemoryCollection(settings).Build())
        );

        Assert.That(exception.ParamName, Does.EndWith(":Path"));
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

    [TestCase(null, 20_160)]
    [TestCase("1", 1)]
    [TestCase("1440", 1440)]
    [TestCase("2880", 2880)]
    public void RefreshTokenExpirationDefaultsToFourteenDaysAndCanBeConfigured(
        string configuredMinutes,
        int expectedMinutes
    )
    {
        var config = new OpenIddictConfig(
            CreateConfiguration(refreshTokenExpirationMinutes: configuredMinutes)
        );

        Assert.That(config.RefreshTokenExpirationMinutes, Is.EqualTo(expectedMinutes));
        Assert.That(config.AccessTokenExpirationMinutes, Is.EqualTo(60));
    }

    [TestCase("0")]
    [TestCase("-1")]
    public void RefreshTokenExpirationMustBePositive(string configuredMinutes)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OpenIddictConfig(
                CreateConfiguration(refreshTokenExpirationMinutes: configuredMinutes)
            )
        );

        Assert.That(
            exception.ParamName,
            Is.EqualTo(nameof(OpenIddictConfig.RefreshTokenExpirationMinutes))
        );
    }

    private static IConfiguration CreateConfiguration(
        string configuredMinutes = null,
        string refreshTokenExpirationMinutes = null
    )
    {
        var settings = new Dictionary<string, string>
        {
            ["OpenIddictConfig:ClientApplicationTemplates:Configured"] = "true",
        };
        if (configuredMinutes != null)
        {
            settings["OpenIddictConfig:AccessTokenExpirationMinutes"] = configuredMinutes;
        }
        if (refreshTokenExpirationMinutes != null)
        {
            settings["OpenIddictConfig:RefreshTokenExpirationMinutes"] =
                refreshTokenExpirationMinutes;
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
            settings["OpenIddictConfig:PrivateApiAdditionalAuthenticationMethods:0:Path"] = path;
            settings["OpenIddictConfig:PrivateApiAdditionalAuthenticationMethods:0:Method"] =
                additionalMethod;
        }
        return new OpenIddictConfig(
            new ConfigurationBuilder().AddInMemoryCollection(settings).Build()
        );
    }
}
