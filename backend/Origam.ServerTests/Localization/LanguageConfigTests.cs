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

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Origam.Server.Configuration;

namespace Origam.ServerTests.Localization;

[TestFixture]
public class LanguageConfigTests
{
    [Test]
    public void AcceptLanguageIsDisabledByDefault()
    {
        LanguageConfig languageConfig = TestLanguageConfig.CreateWithoutAcceptLanguageSection();

        Assert.Multiple(() =>
        {
            Assert.That(languageConfig.AcceptLanguageHeaderEnabled, Is.False);
            Assert.That(
                languageConfig.SupportedCultures.Select(culture => culture.Name),
                Is.EquivalentTo(new[] { "de-CH", "fr-CH", "en", "fr-FR" })
            );
        });
    }

    [Test]
    public void MatchingCulturesBecomeSupportedButAreNotOffered()
    {
        LanguageConfig languageConfig = TestLanguageConfig.Create(
            acceptLanguageEnabled: true,
            matchingCultures: "fr"
        );

        Assert.Multiple(() =>
        {
            Assert.That(
                languageConfig.SupportedCultures.Select(culture => culture.Name),
                Is.EquivalentTo(new[] { "de-CH", "fr-CH", "en", "fr-FR", "fr" })
            );
            Assert.That(
                languageConfig.CultureItems.Select(item => item.CultureName),
                Is.EquivalentTo(new[] { "de-CH", "fr-CH", "en", "fr-FR" })
            );
        });
    }

    [Test]
    public void MatchingCulturesDoNotApplyWhenDisabled()
    {
        LanguageConfig languageConfig = TestLanguageConfig.Create(
            acceptLanguageEnabled: false,
            matchingCultures: "fr"
        );

        Assert.That(
            languageConfig.SupportedCultures.Select(culture => culture.Name),
            Has.No.Member("fr")
        );
    }

    [Test]
    public void CultureNamesFromConfigurationAreNormalized()
    {
        var settings = new List<KeyValuePair<string, string>>
        {
            new("LanguageConfig:Default", "de-ch"),
            new("LanguageConfig:Allowed:0:Culture", "de-ch"),
            new("LanguageConfig:Allowed:0:Caption", "Deutsch"),
            new("LanguageConfig:Allowed:1:Culture", "fr-ch"),
            new("LanguageConfig:Allowed:1:Caption", "Français"),
        };
        var languageConfig = new LanguageConfig(
            new ConfigurationBuilder().AddInMemoryCollection(settings).Build()
        );

        Assert.Multiple(() =>
        {
            Assert.That(
                languageConfig.CultureItems.Select(item => item.CultureName),
                Is.EquivalentTo(new[] { "de-CH", "fr-CH" })
            );
            // the default's item is found however the configuration spells it
            Assert.That(languageConfig.FindCultureItem("it-IT").CultureName, Is.EqualTo("de-CH"));
        });
    }

    [Test]
    public void CultureItemIsMatchedByExactName()
    {
        LanguageConfig languageConfig = TestLanguageConfig.Create(acceptLanguageEnabled: false);

        Assert.That(languageConfig.FindCultureItem("fr-FR").CultureName, Is.EqualTo("fr-FR"));
    }

    [Test]
    public void CultureItemIsMatchedByLanguage()
    {
        LanguageConfig languageConfig = TestLanguageConfig.Create(
            acceptLanguageEnabled: true,
            matchingCultures: "fr"
        );

        Assert.Multiple(() =>
        {
            Assert.That(languageConfig.FindCultureItem("fr").CultureName, Is.EqualTo("fr-CH"));
            Assert.That(languageConfig.FindCultureItem("de-DE").CultureName, Is.EqualTo("de-CH"));
        });
    }

    [Test]
    public void CultureItemFallsBackToTheDefaultCulture()
    {
        LanguageConfig languageConfig = TestLanguageConfig.Create(acceptLanguageEnabled: false);

        Assert.Multiple(() =>
        {
            Assert.That(languageConfig.FindCultureItem("it-IT").CultureName, Is.EqualTo("de-CH"));
            Assert.That(languageConfig.TryFindCultureItem("it-IT"), Is.Null);
        });
    }
}

internal static class TestLanguageConfig
{
    public static LanguageConfig Create(
        bool acceptLanguageEnabled,
        params string[] matchingCultures
    )
    {
        List<KeyValuePair<string, string>> settings = CultureSettings();
        settings.Add(
            new KeyValuePair<string, string>(
                "LanguageConfig:AcceptLanguage:Enabled",
                acceptLanguageEnabled ? "true" : "false"
            )
        );
        for (int i = 0; i < matchingCultures.Length; i++)
        {
            settings.Add(
                new KeyValuePair<string, string>(
                    $"LanguageConfig:AcceptLanguage:MatchingCultures:{i}",
                    matchingCultures[i]
                )
            );
        }
        return Build(settings);
    }

    public static LanguageConfig CreateWithoutAcceptLanguageSection()
    {
        return Build(CultureSettings());
    }

    private static LanguageConfig Build(List<KeyValuePair<string, string>> settings)
    {
        return new LanguageConfig(
            new ConfigurationBuilder().AddInMemoryCollection(settings).Build()
        );
    }

    private static List<KeyValuePair<string, string>> CultureSettings()
    {
        return new List<KeyValuePair<string, string>>
        {
            new("LanguageConfig:Default", "de-CH"),
            new("LanguageConfig:Allowed:0:Culture", "de-CH"),
            new("LanguageConfig:Allowed:0:Caption", "Deutsch"),
            new("LanguageConfig:Allowed:0:ResetPasswordMailSubject", "Passwort"),
            new("LanguageConfig:Allowed:1:Culture", "fr-CH"),
            new("LanguageConfig:Allowed:1:Caption", "Français"),
            new("LanguageConfig:Allowed:1:ResetPasswordMailSubject", "Mot de passe"),
            new("LanguageConfig:Allowed:2:Culture", "en"),
            new("LanguageConfig:Allowed:2:Caption", "English"),
            new("LanguageConfig:Allowed:2:ResetPasswordMailSubject", "Password"),
            new("LanguageConfig:Allowed:3:Culture", "fr-FR"),
            new("LanguageConfig:Allowed:3:Caption", "Français (FR)"),
            new("LanguageConfig:Allowed:3:ResetPasswordMailSubject", "Mot de passe"),
        };
    }
}
