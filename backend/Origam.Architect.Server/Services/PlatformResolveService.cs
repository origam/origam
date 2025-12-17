#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using Origam.Architect.Server.Interfaces.Services;

namespace Origam.Architect.Server.Services;

public class PlatformResolveService : IPlatformResolveService
{
    public Platform Resolve(string requestedPlatformName)
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        var platforms = settings.GetAllPlatforms();

        if (string.IsNullOrWhiteSpace(requestedPlatformName))
        {
            Platform platform = platforms.FirstOrDefault();
            return platform ?? throw new InvalidOperationException("No platforms are configured.");
        }

        var requested = requestedPlatformName.Trim();
        Platform match = platforms.FirstOrDefault(p =>
            string.Equals(p.Name, requested, StringComparison.OrdinalIgnoreCase)
        );

        if (match != null)
        {
            return match;
        }

        var available = string.Join(", ", platforms.Select(p => p.Name));
        throw new InvalidOperationException(
            $"Unknown platform '{requested}'. Available platforms: {available}."
        );
    }
}
