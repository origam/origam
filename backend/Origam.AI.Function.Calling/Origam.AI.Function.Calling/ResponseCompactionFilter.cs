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

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;

namespace Origam.AI.Function.Calling;

public class ResponseCompactionFilter : IToolInvocationFilter
{
    private const int MaxValueLength = 120;
    private const int MaxPropertiesLength = 1500;

    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web
    );

    public async ValueTask<object?> OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        ToolInvocation next,
        CancellationToken cancellationToken
    )
    {
        object? result = await next(context, cancellationToken);
        if (result is not string content)
        {
            return result;
        }

        try
        {
            return Compact(content) ?? result;
        }
        catch (JsonException)
        {
            return result;
        }
    }

    private static string? Compact(string content)
    {
        string trimmed = content.TrimStart();
        if (trimmed.Length == 0 || (trimmed[0] != '{' && trimmed[0] != '['))
        {
            return null;
        }

        using var document = JsonDocument.Parse(content);
        JsonElement root = document.RootElement;

        if (root.ValueKind == JsonValueKind.Object)
        {
            return CompactObject(root);
        }

        if (root.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var compactedItems = new List<string>();
        foreach (JsonElement item in root.EnumerateArray())
        {
            string? compactedItem =
                item.ValueKind == JsonValueKind.Object ? CompactObject(item) : null;
            if (compactedItem is null)
            {
                return null;
            }

            compactedItems.Add(compactedItem);
        }

        return compactedItems.Count == 0
            ? null
            : "[" + string.Join(separator: ",", compactedItems) + "]";
    }

    private static string? CompactObject(JsonElement root)
    {
        if (root.TryGetProperty(propertyName: "propertyUpdates", out JsonElement propertyUpdates))
        {
            return CompactPropertyUpdates(root, propertyUpdates);
        }

        if (root.TryGetProperty(propertyName: "node", out JsonElement node))
        {
            return CompactEditorTab(root, node);
        }

        return null;
    }

    private static string? CompactEditorTab(JsonElement root, JsonElement node)
    {
        if (
            node.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty(propertyName: "data", out JsonElement data)
            || !TryReadProperties(data, out string values, out List<string> errors)
        )
        {
            return null;
        }

        var summary = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["id"] = GetString(node, propertyName: "origamId"),
            ["name"] = GetString(node, propertyName: "nodeText"),
            ["type"] = GetString(node, propertyName: "itemTypeName"),
            ["saved"] = GetBoolean(root, propertyName: "isPersisted"),
            ["props"] = values,
            ["errors"] = errors,
        };

        return JsonSerializer.Serialize(summary, SerializerOptions);
    }

    private static string? CompactPropertyUpdates(JsonElement root, JsonElement propertyUpdates)
    {
        if (!TryReadProperties(propertyUpdates, out string values, out List<string> errors))
        {
            return null;
        }

        var summary = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["isDirty"] = GetBoolean(root, propertyName: "isDirty"),
            ["props"] = values,
            ["errors"] = errors,
        };

        return JsonSerializer.Serialize(summary, SerializerOptions);
    }

    private static bool TryReadProperties(
        JsonElement properties,
        out string values,
        out List<string> errors
    )
    {
        values = string.Empty;
        errors = new List<string>();

        if (properties.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        var builder = new StringBuilder();
        bool truncated = false;

        foreach (JsonElement property in properties.EnumerateArray())
        {
            if (property.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            string? name =
                GetString(property, propertyName: "name")
                ?? GetString(property, propertyName: "propertyName");
            if (name is null)
            {
                return false;
            }

            AppendErrors(errors, name, property);

            if (truncated || GetBoolean(property, propertyName: "readOnly") == true)
            {
                continue;
            }

            if (builder.Length >= MaxPropertiesLength)
            {
                builder.Append("; ...");
                truncated = true;
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append("; ");
            }

            builder.Append(name).Append('=').Append(DescribeValue(property));
        }

        values = builder.ToString();
        return true;
    }

    private static void AppendErrors(List<string> errors, string name, JsonElement property)
    {
        if (
            !property.TryGetProperty(propertyName: "errors", out JsonElement propertyErrors)
            || propertyErrors.ValueKind != JsonValueKind.Array
        )
        {
            return;
        }

        foreach (JsonElement error in propertyErrors.EnumerateArray())
        {
            if (error.ValueKind == JsonValueKind.String)
            {
                errors.Add($"{name}: {error.GetString()}");
            }
        }
    }

    private static string DescribeValue(JsonElement property)
    {
        if (!property.TryGetProperty(propertyName: "value", out JsonElement value))
        {
            return "null";
        }

        string text = value.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => "null",
            JsonValueKind.String => value.GetString() ?? string.Empty,
            _ => value.GetRawText(),
        };

        text = text.ReplaceLineEndings(replacementText: " ");
        return text.Length <= MaxValueLength
            ? text
            : text.Substring(startIndex: 0, length: MaxValueLength) + "...";
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        return
            element.TryGetProperty(propertyName, out JsonElement property)
            && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static bool? GetBoolean(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null,
        };
    }
}
