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

    /// <summary>
    /// Converts a JSON string representing a TreeNode (or list of TreeNodes) into compact YAML,
    /// substituting OrigamId UUIDs with short aliases.
    /// </summary>
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
            return json; // Fallback
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

        // Recursively serialize children if present
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
