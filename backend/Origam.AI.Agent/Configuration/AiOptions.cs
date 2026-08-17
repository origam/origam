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

namespace Origam.AI.Agent.Configuration;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public string Endpoint { get; set; } = "";

    public string Model { get; set; } = "";

    public string ApiKey { get; set; } = "";

    public string? PromptsPath { get; set; }

    public CommunityOptions Community { get; set; } = new();

    public bool HasApiKey => !string.IsNullOrWhiteSpace(ApiKey);

    public bool HasEndpoint => EndpointUri is not null;

    public string Router => EndpointUri?.Host ?? Endpoint;

    private Uri? EndpointUri =>
        Uri.TryCreate(Endpoint, UriKind.Absolute, out var endpointUri) ? endpointUri : null;
}
