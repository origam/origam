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

using Origam.Schema.DeploymentModel;

namespace Origam.Architect.Server.Models.Responses.DeploymentScripts;

public class ListResponseModel
{
    public List<SchemaDbCompareResultDto> Results { get; set; } = [];
    public string SelectedDeploymentVersionId { get; set; }
    public List<DeploymentVersion> DeploymentVersions { get; set; }
}

public class SchemaDbCompareResultDto
{
    public string ResultType { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public string Script { get; set; } = string.Empty;
    public string Script2 { get; set; } = string.Empty;
    public string SchemaItemId { get; set; } = string.Empty;
    public string SchemaItemType { get; set; } = string.Empty;
    public string PlatformName { get; set; } = string.Empty;
}
