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

using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Origam.Server;
using Origam.Server.Configuration;

namespace Origam.ServerTests.Localization;

[TestFixture]
public class RequestLocalizationTests
{
    [Test]
    public async Task AcceptLanguageHeaderIsIgnoredWhenNotEnabled()
    {
        RequestLocalizationOptions options = CreateOptions(
            TestLanguageConfig.Create(acceptLanguageEnabled: false)
        );

        RequestCulture requestCulture = await Resolve(options, acceptLanguage: "en-US,en;q=0.9");

        Assert.Multiple(() =>
        {
            Assert.That(options.RequestCultureProviders, Has.Count.EqualTo(1));
            Assert.That(requestCulture.Culture.Name, Is.EqualTo("de-CH"));
        });
    }

    [Test]
    public async Task AcceptLanguageHeaderIsResolvedWhenEnabled()
    {
        RequestLocalizationOptions options = CreateOptions(
            TestLanguageConfig.Create(acceptLanguageEnabled: true)
        );

        RequestCulture requestCulture = await Resolve(options, acceptLanguage: "en-US,en;q=0.9");

        Assert.Multiple(() =>
        {
            Assert.That(options.RequestCultureProviders, Has.Count.EqualTo(2));
            Assert.That(
                options.RequestCultureProviders[0],
                Is.TypeOf<OrigamCookieRequestCultureProvider>()
            );
            Assert.That(
                options.RequestCultureProviders[1],
                Is.TypeOf<OrigamAcceptLanguageRequestCultureProvider>()
            );
            Assert.That(requestCulture.Culture.Name, Is.EqualTo("en"));
        });
    }

    [Test]
    public async Task ChosenLanguageWinsOverTheHeader()
    {
        RequestLocalizationOptions options = CreateOptions(
            TestLanguageConfig.Create(acceptLanguageEnabled: true)
        );

        RequestCulture requestCulture = await Resolve(
            options,
            acceptLanguage: "en-US,en;q=0.9",
            chosenLanguage: "fr-CH"
        );

        Assert.That(requestCulture.Culture.Name, Is.EqualTo("fr-CH"));
    }

    [Test]
    public async Task UnconfiguredRegionOfAnAllowedLanguageResolvesToThatCulture()
    {
        RequestLocalizationOptions options = CreateOptions(
            TestLanguageConfig.Create(acceptLanguageEnabled: true)
        );

        RequestCulture requestCulture = await Resolve(options, acceptLanguage: "fr-CA");

        // the header names a culture nobody configured, so a configured culture of the same language
        // is resolved instead of the header's own culture
        Assert.That(requestCulture.Culture.Name, Is.EqualTo("fr-CH"));
    }

    [Test]
    public async Task AcceptLanguageEntriesWithoutAConfiguredLanguageAreSkipped()
    {
        RequestLocalizationOptions options = CreateOptions(
            TestLanguageConfig.Create(acceptLanguageEnabled: true)
        );

        RequestCulture requestCulture = await Resolve(
            options,
            acceptLanguage: "pl-PL;q=0.9, fr-CA;q=0.5"
        );

        Assert.That(requestCulture.Culture.Name, Is.EqualTo("fr-CH"));
    }

    [Test]
    public async Task AcceptLanguageQualityOrderIsPreserved()
    {
        RequestLocalizationOptions options = CreateOptions(
            TestLanguageConfig.Create(acceptLanguageEnabled: true)
        );

        RequestCulture frenchFirst = await Resolve(
            options,
            acceptLanguage: "fr-CA;q=0.9, en-US;q=0.5"
        );
        RequestCulture englishFirst = await Resolve(
            options,
            acceptLanguage: "en-US;q=0.9, fr-CA;q=0.5"
        );

        Assert.Multiple(() =>
        {
            Assert.That(frenchFirst.Culture.Name, Is.EqualTo("fr-CH"));
            Assert.That(englishFirst.Culture.Name, Is.EqualTo("en"));
        });
    }

    [Test]
    public async Task UnparsableAcceptLanguageValueFallsBackToTheDefaultCulture()
    {
        RequestLocalizationOptions options = CreateOptions(
            TestLanguageConfig.Create(acceptLanguageEnabled: true)
        );

        RequestCulture range = await Resolve(options, acceptLanguage: "*");
        RequestCulture garbage = await Resolve(options, acceptLanguage: "garbage");

        Assert.Multiple(() =>
        {
            Assert.That(range.Culture.Name, Is.EqualTo("de-CH"));
            Assert.That(garbage.Culture.Name, Is.EqualTo("de-CH"));
        });
    }

    [Test]
    public async Task UnsupportedHeaderLanguageFallsBackToTheDefaultCulture()
    {
        RequestLocalizationOptions options = CreateOptions(
            TestLanguageConfig.Create(acceptLanguageEnabled: true)
        );

        RequestCulture requestCulture = await Resolve(options, acceptLanguage: "it-IT,it;q=0.9");

        Assert.That(requestCulture.Culture.Name, Is.EqualTo("de-CH"));
    }

    [Test]
    public async Task UnsupportedCookieFallsBackToTheDefaultCultureWhenTheHeaderIsOff()
    {
        RequestLocalizationOptions options = CreateOptions(
            TestLanguageConfig.Create(acceptLanguageEnabled: false)
        );

        RequestCulture requestCulture = await Resolve(
            options,
            acceptLanguage: "en-US,en;q=0.9",
            rawCookieValue: "c=it-IT|uic=it-IT"
        );

        Assert.That(requestCulture.Culture.Name, Is.EqualTo("de-CH"));
    }

    [Test]
    public async Task UnsupportedCookieFallsThroughToTheHeader()
    {
        RequestLocalizationOptions options = CreateOptions(
            TestLanguageConfig.Create(acceptLanguageEnabled: true)
        );

        RequestCulture requestCulture = await Resolve(
            options,
            acceptLanguage: "en-US,en;q=0.9",
            rawCookieValue: "c=it-IT|uic=it-IT"
        );

        Assert.That(requestCulture.Culture.Name, Is.EqualTo("en"));
    }

    [Test]
    public async Task ResolvedCultureIsAppliedInsideTheRequestPipeline()
    {
        RequestLocalizationOptions options = CreateOptions(
            TestLanguageConfig.Create(acceptLanguageEnabled: true)
        );
        var context = new DefaultHttpContext();
        context.Request.Headers.AcceptLanguage = "fr-CA";
        string currentCultures = null;
        var middleware = new RequestLocalizationMiddleware(
            _ =>
            {
                currentCultures =
                    $"{CultureInfo.CurrentCulture.Name}/{CultureInfo.CurrentUICulture.Name}";
                return Task.CompletedTask;
            },
            Microsoft.Extensions.Options.Options.Create(options),
            NullLoggerFactory.Instance
        );

        await middleware.Invoke(context);

        // the framework applies the resolved culture downstream of UseRequestLocalization
        Assert.That(currentCultures, Is.EqualTo("fr-CH/fr-CH"));
    }

    [Test]
    public async Task CorruptCookieFallsThroughToTheHeader()
    {
        RequestLocalizationOptions options = CreateOptions(
            TestLanguageConfig.Create(acceptLanguageEnabled: true)
        );

        RequestCulture requestCulture = await Resolve(
            options,
            acceptLanguage: "en-US,en;q=0.9",
            rawCookieValue: "fr-CH"
        );

        Assert.That(requestCulture.Culture.Name, Is.EqualTo("en"));
    }

    [Test]
    public async Task CorruptCookieFallsBackToTheDefaultCultureWhenTheHeaderIsOff()
    {
        RequestLocalizationOptions options = CreateOptions(
            TestLanguageConfig.Create(acceptLanguageEnabled: false)
        );

        RequestCulture requestCulture = await Resolve(
            options,
            acceptLanguage: "en-US,en;q=0.9",
            rawCookieValue: "fr-CH"
        );

        Assert.That(requestCulture.Culture.Name, Is.EqualTo("de-CH"));
    }

    [Test]
    public void LocalizationCookieFollowsTheResolvedCulture()
    {
        RequestLocalizationOptions options = CreateOptions(
            TestLanguageConfig.Create(acceptLanguageEnabled: true)
        );
        var context = new DefaultHttpContext();
        context.Features.Set<IRequestCultureFeature>(
            new RequestCultureFeature(new RequestCulture("fr-FR"), provider: null)
        );

        Assert.Multiple(() =>
        {
            Assert.That(
                OrigamRequestLocalization.MakeLocalizationCookieValue(options, context),
                Does.StartWith("c=fr-FR|")
            );
            Assert.That(
                OrigamRequestLocalization.MakeLocalizationCookieValue(
                    options,
                    new DefaultHttpContext()
                ),
                Does.StartWith("c=de-CH|")
            );
        });
    }

    private static RequestLocalizationOptions CreateOptions(LanguageConfig languageConfig)
    {
        var options = new RequestLocalizationOptions();
        OrigamRequestLocalization.Apply(options, languageConfig);
        return options;
    }

    private static async Task<RequestCulture> Resolve(
        RequestLocalizationOptions options,
        string acceptLanguage,
        string chosenLanguage = null,
        string rawCookieValue = null
    )
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.AcceptLanguage = acceptLanguage;
        var cookieProvider = (OrigamCookieRequestCultureProvider)options.RequestCultureProviders[0];
        string cookieValue = rawCookieValue;
        if (cookieValue == null && chosenLanguage != null)
        {
            cookieValue = cookieProvider.MakeCookieValue(new RequestCulture(chosenLanguage));
        }
        if (cookieValue != null)
        {
            var cookieContext = new DefaultHttpContext();
            cookieContext.Response.Cookies.Append(cookieProvider.CookieName, cookieValue);
            context.Request.Headers.Cookie = cookieContext
                .Response.Headers.SetCookie.ToString()
                .Split(';')[0];
        }
        var middleware = new RequestLocalizationMiddleware(
            context => Task.CompletedTask,
            Microsoft.Extensions.Options.Options.Create(options),
            NullLoggerFactory.Instance
        );
        await middleware.Invoke(context);
        return context.Features.Get<IRequestCultureFeature>().RequestCulture;
    }
}
