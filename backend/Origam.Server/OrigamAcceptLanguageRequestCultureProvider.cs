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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Primitives;
using Origam.Server.Configuration;

namespace Origam.Server;

// the framework falls back from a culture to its parent only ("fr-CA" to "fr"), so without this a browser
// whose language is configured only in another region would end at the default culture
public class OrigamAcceptLanguageRequestCultureProvider : AcceptLanguageHeaderRequestCultureProvider
{
    private readonly LanguageConfig languageConfig;

    public OrigamAcceptLanguageRequestCultureProvider(LanguageConfig languageConfig)
    {
        this.languageConfig = languageConfig;
    }

    public override async Task<ProviderCultureResult> DetermineProviderCultureResult(
        HttpContext httpContext
    )
    {
        ProviderCultureResult headerCultures = await base.DetermineProviderCultureResult(
            httpContext
        );
        if (headerCultures == null)
        {
            return null;
        }
        string[] cultureNames = headerCultures
            .Cultures.Select(culture => languageConfig.TryFindCultureItem(culture.Value))
            .Where(item => item != null)
            .Select(item => item.CultureName)
            .Distinct()
            .ToArray();
        return cultureNames.Length == 0
            ? null
            : new ProviderCultureResult(
                cultureNames.Select(name => new StringSegment(name)).ToArray()
            );
    }
}
