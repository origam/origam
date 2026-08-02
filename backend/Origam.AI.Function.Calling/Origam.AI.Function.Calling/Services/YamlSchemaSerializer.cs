using System.Text;
using System.Text.Json;

namespace Origam.AI.Function.Calling.Services;

public class YamlSchemaSerializer
{
    private readonly AliasMappingService _aliasMappingService;

    public YamlSchemaSerializer(AliasMappingService aliasMappingService)
    {
        _aliasMappingService = aliasMappingService;
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

        // Extract key properties we want to show
        var hasOrigamId =
            obj.TryGetProperty(propertyName: "origamId", out var origamIdProp)
            || obj.TryGetProperty(propertyName: "OrigamId", out origamIdProp);
        var origamId = hasOrigamId ? origamIdProp.GetString() : null;

        var hasNodeText =
            obj.TryGetProperty(propertyName: "nodeText", out var nodeTextProp)
            || obj.TryGetProperty(propertyName: "NodeText", out nodeTextProp);
        var nodeText = hasNodeText ? nodeTextProp.GetString() : null;

        var hasItemTypeName =
            obj.TryGetProperty(propertyName: "itemTypeName", out var typeProp)
            || obj.TryGetProperty(propertyName: "ItemTypeName", out typeProp);
        var itemTypeName = hasItemTypeName ? typeProp.GetString() : null;

        if (hasNodeText)
        {
            sb.Append(indentStr).Append("Name: ").AppendLine(nodeText);
        }
        if (hasItemTypeName && !string.IsNullOrWhiteSpace(itemTypeName))
        {
            sb.Append(indentStr).Append("Type: ").AppendLine(itemTypeName);
        }
        if (hasOrigamId && !string.IsNullOrWhiteSpace(origamId))
        {
            var alias = _aliasMappingService.GetOrAddAlias(origamId);
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
