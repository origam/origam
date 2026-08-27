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

using System;
using System.Collections.Generic;
using System.Linq;
using Origam.Schema.GuiModel;
using Origam.Server.Configuration;

namespace Origam.Server.OpenApi;

public class ModeledOpenApiPagePolicy(StartUpConfiguration startUpConfiguration)
{
    private const string TagPrefix = "Modeled API — ";

    public bool IsDocumented(AbstractPage page)
    {
        return !page.IsAbstract
            && IsRuntimeCompatibleUrl(page.Url)
            && (
                IsRouteIn(page.Url, startUpConfiguration.UserApiPublicRoutes)
                || IsRouteIn(page.Url, startUpConfiguration.UserApiRestrictedRoutes)
            );
    }

    public bool RequiresAuthentication(AbstractPage page)
    {
        return !string.Equals(a: page.Roles, b: "*", comparisonType: StringComparison.Ordinal)
            || IsRouteIn(page.Url, startUpConfiguration.UserApiRestrictedRoutes);
    }

    public string GetTagName(AbstractPage page)
    {
        string groupName =
            page.Group == null
                ? Resources.ModeledApiUncategorized
                : string.Join(
                    separator: " / ",
                    values: page.Group.Path.Replace(oldValue: "\\", newValue: "/")
                        .Split(separator: '/', options: StringSplitOptions.RemoveEmptyEntries)
                        .AsEnumerable()
                );
        return TagPrefix + groupName;
    }

    private static bool IsRouteIn(string pageUrl, IEnumerable<string> configuredRoutes)
    {
        string normalizedPageUrl = "/" + pageUrl.TrimStart('/');
        return configuredRoutes
            .Where(route => !string.IsNullOrWhiteSpace(route))
            .Any(route =>
                normalizedPageUrl.StartsWith(
                    "/" + route.TrimStart('/'),
                    StringComparison.OrdinalIgnoreCase
                )
            );
    }

    private static bool IsRuntimeCompatibleUrl(string url)
    {
        return !string.IsNullOrWhiteSpace(url) && !url.StartsWith('/');
    }
}
