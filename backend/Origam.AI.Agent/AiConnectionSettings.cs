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

namespace Origam.AI.Agent;

public sealed record AiConnectionSettings(string Endpoint, string Model, string ApiKey)
{
    private static readonly Dictionary<string, string> RouterNamesByHost = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ["api.openai.com"] = "OpenAI",
        ["openrouter.ai"] = "OpenRouter",
        ["integrate.api.nvidia.com"] = "NVIDIA",
        ["api.anthropic.com"] = "Anthropic",
        ["generativelanguage.googleapis.com"] = "Google",
        ["api.deepseek.com"] = "DeepSeek",
        ["api.mistral.ai"] = "Mistral",
    };

    public bool HasApiKey => !string.IsNullOrWhiteSpace(ApiKey);

    public string Router
    {
        get
        {
            if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out var endpointUri))
            {
                return Endpoint;
            }
            return RouterNamesByHost.TryGetValue(endpointUri.Host, out var routerName)
                ? routerName
                : endpointUri.Host;
        }
    }

    public static AiConnectionSettings Read(IConfiguration configuration)
    {
        var section = configuration.GetSection("Ai");
        return new AiConnectionSettings(
            section["Endpoint"] ?? "https://api.openai.com/v1",
            section["Model"] ?? "gpt-5.6-luna",
            section["ApiKey"] ?? ""
        );
    }
}
