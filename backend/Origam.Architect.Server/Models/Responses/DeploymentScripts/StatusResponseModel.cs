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

using Origam.Architect.Server.Enums;

namespace Origam.Architect.Server.Models.Responses.DeploymentScripts;

public class StatusResponseModel
{
    public List<PackageStatusDto> Packages { get; set; } = new();
}

public class PackageStatusDto
{
    public Guid PackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string PackageModelVersion { get; set; } = string.Empty;
    public string DeployedVersion { get; set; } = string.Empty;
    public List<DeploymentVersionStatusDto> Versions { get; set; } = new();
}

public class DeploymentVersionStatusDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DeploymentActivityStatus Status { get; set; }
    public bool IsCurrentVersion { get; set; }
    public List<ActivityStatusDto> Activities { get; set; } = new();
}

public class ActivityStatusDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public int ActivityOrder { get; set; }
    public DeploymentActivityStatus Status { get; set; }
}
