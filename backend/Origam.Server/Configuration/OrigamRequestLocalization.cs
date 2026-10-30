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

using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace Origam.Server.Configuration;

public static class OrigamRequestLocalization
{
    public static void Apply(RequestLocalizationOptions options, LanguageConfig languageConfig)
    {
        options.DefaultRequestCulture = languageConfig.DefaultCulture;
        options.SupportedCultures = languageConfig.SupportedCultures;
        options.SupportedUICultures = languageConfig.SupportedCultures;
        options.RequestCultureProviders.Clear();
        options.RequestCultureProviders.Add(new OrigamCookieRequestCultureProvider(languageConfig));
        if (languageConfig.AcceptLanguageHeaderEnabled)
        {
            options.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
        }
    }

    // the client persists this cookie as its language, so it must carry the culture the request
    // resolved and, for a matching-only culture, the item whose settings apply to it
    public static RequestCulture ResolveCultureForCookie(
        LanguageConfig languageConfig,
        RequestCulture resolvedCulture
    )
    {
        CultureItem cultureItem = languageConfig.TryFindCultureItem(resolvedCulture.Culture.Name);
        return cultureItem == null
            ? languageConfig.DefaultCulture
            : new RequestCulture(cultureItem.CultureName);
    }

    public static string MakeLocalizationCookieValue(
        RequestLocalizationOptions options,
        LanguageConfig languageConfig,
        HttpContext httpContext
    )
    {
        var cultureProvider = options
            .RequestCultureProviders.OfType<OrigamCookieRequestCultureProvider>()
            .First();
        RequestCulture resolvedCulture =
            httpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture
            ?? options.DefaultRequestCulture;
        return cultureProvider.MakeCookieValue(
            ResolveCultureForCookie(languageConfig, resolvedCulture)
        );
    }
}
