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

using System.Text.Json;
using Microsoft.Extensions.AI;
using Origam.AI.Agent.Strategy.Architect.Api;

namespace Origam.AI.Agent.Services.OpenApi;

public class OpenApiFunction : AIFunction
{
    private const string EmptySuccessBody = "{\"success\":true}";

    private readonly string functionName;
    private readonly string functionDescription;
    private readonly JsonElement schema;
    private readonly HttpMethod httpMethod;
    private readonly string pathTemplate;
    private readonly IReadOnlyList<OpenApiParameter> parameters;
    private readonly ArchitectApiClient architectApi;

    public OpenApiFunction(
        string functionName,
        string functionDescription,
        JsonElement schema,
        HttpMethod httpMethod,
        string pathTemplate,
        IReadOnlyList<OpenApiParameter> parameters,
        ArchitectApiClient architectApi
    )
    {
        this.functionName = functionName;
        this.functionDescription = functionDescription;
        this.schema = schema;
        this.httpMethod = httpMethod;
        this.pathTemplate = pathTemplate;
        this.parameters = parameters;
        this.architectApi = architectApi;
    }

    public override string Name => functionName;

    public override string Description => functionDescription;

    public override JsonElement JsonSchema => schema;

    protected override async ValueTask<object?> InvokeCoreAsync(
        AIFunctionArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var relativePath = pathTemplate;
        var queryParts = new List<string>();
        var body = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var parameter in parameters)
        {
            if (!arguments.TryGetValue(parameter.Name, out var value) || value is null)
            {
                continue;
            }

            if (parameter.Location == OpenApiParameterLocation.Path)
            {
                relativePath = relativePath.Replace(
                    oldValue: "{" + parameter.Name + "}",
                    newValue: Uri.EscapeDataString(Stringify(value))
                );
            }
            else if (parameter.Location == OpenApiParameterLocation.Query)
            {
                queryParts.Add(
                    $"{Uri.EscapeDataString(parameter.Name)}={Uri.EscapeDataString(Stringify(value))}"
                );
            }
            else
            {
                body[parameter.Name] = value;
            }
        }

        var relativeUrl = relativePath;
        if (queryParts.Count > 0)
        {
            relativeUrl += "?" + string.Join(separator: "&", queryParts);
        }

        var payload = body.Count > 0 ? JsonSerializer.Serialize(body) : string.Empty;

        var response = await architectApi.SendAsync(
            httpMethod,
            relativeUrl,
            payload,
            cancellationToken
        );

        if (!response.IsSuccess)
        {
            throw new OpenApiInvocationException(
                functionName,
                (int)response.StatusCode,
                response.Body,
                response.RequestUrl,
                payload
            );
        }

        return string.IsNullOrWhiteSpace(response.Body) ? EmptySuccessBody : response.Body;
    }

    private static string Stringify(object value)
    {
        if (value is JsonElement element)
        {
            return element.ValueKind == JsonValueKind.String
                ? element.GetString() ?? string.Empty
                : element.GetRawText();
        }

        return value.ToString() ?? string.Empty;
    }
}
