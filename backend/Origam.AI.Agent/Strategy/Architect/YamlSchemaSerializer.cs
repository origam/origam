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
using Origam.AI.Agent.Extensions;
using Origam.AI.Agent.Services;

namespace Origam.AI.Agent.Strategy.Architect;

public class YamlSchemaSerializer
{
    private readonly AliasMappingService aliasMappingService;

    public YamlSchemaSerializer(AliasMappingService aliasMappingService)
    {
        this.aliasMappingService = aliasMappingService;
    }

    public string SerializeFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        try
        {
            var element = JsonSerializer.Deserialize<JsonElement>(json);
            var sb = new StringBuilder();
            SerializeElement(element, sb, indent: 0);
            return sb.ToString();
        }
        catch
        {
            return json;
        }
    }

    private void SerializeElement(JsonElement element, StringBuilder sb, int indent)
    {
        string indentStr = new string(c: ' ', count: indent * 2);

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object)
                {
                    sb.Append(indentStr).AppendLine("-");
                    SerializeObjectProperties(item, sb, indent + 1);
                }
                else
                {
                    sb.Append(indentStr).Append("- ").AppendLine(item.ToString());
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Object)
        {
            SerializeObjectProperties(element, sb, indent);
        }
    }

    private void SerializeObjectProperties(JsonElement obj, StringBuilder sb, int indent)
    {
        string indentStr = new string(c: ' ', count: indent * 2);

        var origamId = obj.GetStringOrNullIgnoreCase(propertyName: "origamId");
        var nodeText = obj.GetStringOrNullIgnoreCase(propertyName: "nodeText");
        var itemTypeName = obj.GetStringOrNullIgnoreCase(propertyName: "itemTypeName");

        if (nodeText is not null)
        {
            sb.Append(indentStr).Append("Name: ").AppendLine(nodeText);
        }
        if (!string.IsNullOrWhiteSpace(itemTypeName))
        {
            sb.Append(indentStr).Append("Type: ").AppendLine(itemTypeName);
        }
        if (!string.IsNullOrWhiteSpace(origamId))
        {
            var alias = aliasMappingService.GetOrAddAlias(origamId);
            sb.Append(indentStr).Append("Id: ").AppendLine(alias);
        }

        var hasChildren =
            obj.TryGetProperty(propertyName: "children", out var childrenProp)
            || obj.TryGetProperty(propertyName: "Children", out childrenProp);
        if (
            hasChildren
            && childrenProp.ValueKind == JsonValueKind.Array
            && childrenProp.GetArrayLength() > 0
        )
        {
            sb.Append(indentStr).AppendLine("Children:");
            SerializeElement(childrenProp, sb, indent + 1);
        }
    }
}
