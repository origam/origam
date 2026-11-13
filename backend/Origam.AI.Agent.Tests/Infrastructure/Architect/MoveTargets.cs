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

namespace Origam.AI.Agent.Tests.Infrastructure.Architect;

public sealed record MoveTargets(
    bool IsSourceInActivePackage,
    IReadOnlyList<MoveTarget> Targets,
    IReadOnlyList<MovePackage> Packages
)
{
    public MoveTarget? CurrentLocation =>
        Targets.FirstOrDefault(target => target.IsCurrentLocation);

    public string Describe()
    {
        var location = CurrentLocation is { } current
            ? $"{current.Path} ({current.PackageName})"
            : "(not among the targets)";
        return $"in the active package: {IsSourceInActivePackage}, now in: {location}";
    }
}
