#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Origam.Server.Configuration;

namespace Origam.Server;

public class OrigamCookieRequestCultureProvider : RequestCultureProvider
{
    private static log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    private readonly LanguageConfig languageConfig;
    private static readonly char cookieSeparator = '|';
    private static readonly string culturePrefix = "c=";
    private static readonly string uiCulturePrefix = "uic=";
    public string CookieName { get; } = "origamCurrentLocale";

    public OrigamCookieRequestCultureProvider(LanguageConfig languageConfig)
    {
        this.languageConfig = languageConfig;
    }

    public override Task<ProviderCultureResult> DetermineProviderCultureResult(
        HttpContext httpContext
    )
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(paramName: nameof(httpContext));
        }
        var cookie = httpContext.Request.Cookies[key: CookieName];
        if (string.IsNullOrEmpty(value: cookie))
        {
            return NullProviderCultureResult;
        }
        var providerResultCulture = ParseCookieValue(value: cookie);
        return Task.FromResult(result: providerResultCulture);
    }

    /// <summary>
    /// Creates a string representation of a <see cref="RequestCulture"/> for placement in a cookie.
    /// </summary>
    /// <param name="requestCulture">The <see cref="RequestCulture"/>.</param>
    /// <returns>The cookie value.</returns>
    public string MakeCookieValue(RequestCulture requestCulture)
    {
        if (requestCulture == null)
        {
            throw new ArgumentNullException(paramName: nameof(requestCulture));
        }
        var cultureItem = languageConfig.CultureItems.FirstOrDefault(predicate: items =>
            items.CultureName == requestCulture.Culture.Name
        );
        if (cultureItem == null)
        {
            throw new Exception(
                message: $"The culture \"{requestCulture.Culture.Name}\" was not found among the allowed cultures in the LanguageConfig."
            );
        }
        return string.Join(
            separator: cookieSeparator,
            value: new[]
            {
                $"{culturePrefix}{requestCulture.Culture.Name}",
                $"{uiCulturePrefix}{requestCulture.UICulture.Name}",
                "defaultDateSeparator=" + cultureItem.DateCompleterConfig.DateSeparator,
                "defaultTimeSeparator=" + cultureItem.DateCompleterConfig.TimeSeparator,
                "defaultDateTimeSeparator=" + cultureItem.DateCompleterConfig.DateTimeSeparator,
                "defaultDateSequence=" + cultureItem.DateCompleterConfig.DateSequence,
                "defaultLongDateFormat=" + cultureItem.DefaultDateFormats.Long,
                "defaultShortDateFormat=" + cultureItem.DefaultDateFormats.Short,
                "defaultTimeFormat=" + cultureItem.DefaultDateFormats.Time,
            }
        );
    }

    /// <summary>
    /// Parses a <see cref="RequestCulture"/> from the specified cookie value.
    /// Returns <c>null</c> if parsing fails.
    /// </summary>
    /// <param name="value">The cookie value to parse.</param>
    /// <returns>The <see cref="RequestCulture"/> or <c>null</c> if parsing fails.</returns>
    private ProviderCultureResult ParseCookieValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value: value))
        {
            return null;
        }
        try
        {
            var parts = value.Split(
                separator: cookieSeparator,
                options: StringSplitOptions.RemoveEmptyEntries
            );

            var potentialCultureName = parts[0];
            var potentialUICultureName = parts[1];
            if (
                !potentialCultureName.StartsWith(value: culturePrefix)
                || !potentialUICultureName.StartsWith(value: uiCulturePrefix)
            )
            {
                return null;
            }
            var cultureName = potentialCultureName.Substring(startIndex: culturePrefix.Length);
            var uiCultureName = potentialUICultureName.Substring(
                startIndex: uiCulturePrefix.Length
            );
            return new ProviderCultureResult(culture: cultureName, uiCulture: uiCultureName);
        }
        catch (Exception ex)
        {
            log.Warn(message: ex);
            return new ProviderCultureResult(
                culture: languageConfig.DefaultCulture.Culture.Name,
                uiCulture: languageConfig.DefaultCulture.UICulture.Name
            );
        }
    }
}
