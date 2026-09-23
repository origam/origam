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

namespace Origam.AI.Agent.Tests.Infrastructure.Architect;

public sealed record DesignerWidget(
    string Type,
    string? BoundField,
    IReadOnlyDictionary<string, string> Properties,
    IReadOnlyList<DesignerWidget> Children
)
{
    public string ShortType => Type[(Type.LastIndexOf('.') + 1)..];

    public static DesignerWidget FromJson(JsonElement control)
    {
        var properties = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in control.GetProperty("properties").EnumerateArray())
        {
            if (
                property.TryGetProperty(propertyName: "name", out var name)
                && name.GetString() is { } propertyName
                && property.TryGetProperty(propertyName: "value", out var value)
            )
            {
                properties[propertyName] =
                    value.ValueKind == JsonValueKind.Null ? string.Empty : value.ToString();
            }
        }

        var children = new List<DesignerWidget>();
        if (
            control.TryGetProperty(propertyName: "children", out var childElements)
            && childElements.ValueKind == JsonValueKind.Array
        )
        {
            children.AddRange(childElements.EnumerateArray().Select(FromJson));
        }

        return new DesignerWidget(
            control.GetProperty("type").GetString() ?? string.Empty,
            control.TryGetProperty(propertyName: "boundField", out var boundField)
                ? boundField.GetString()
                : null,
            properties,
            children
        );
    }

    public string Property(string name)
    {
        return Properties.GetValueOrDefault(name, defaultValue: string.Empty);
    }

    public int Number(string name)
    {
        return int.TryParse(Property(name), out var value) ? value : 0;
    }

    public IEnumerable<DesignerWidget> Descendants()
    {
        foreach (var child in Children)
        {
            yield return child;
            foreach (var grandChild in child.Descendants())
            {
                yield return grandChild;
            }
        }
    }

    public IReadOnlyList<DesignerWidget> FindAll(string shortType)
    {
        return Descendants().Where(widget => widget.ShortType == shortType).ToList();
    }

    public IReadOnlyList<DesignerWidget> FindAll(string shortType, string? boundField)
    {
        return Descendants()
            .Where(widget =>
                widget.ShortType == shortType
                && string.Equals(widget.BoundField, boundField, StringComparison.Ordinal)
            )
            .ToList();
    }

    public DesignerWidget? Find(string shortType)
    {
        return FindAll(shortType).FirstOrDefault();
    }

    public DesignerWidget? Find(string shortType, string? boundField)
    {
        return FindAll(shortType, boundField).FirstOrDefault();
    }

    public string Describe()
    {
        var lines = new List<string>();
        Describe(this, indent: 0, lines);
        return string.Join(Environment.NewLine, lines);
    }

    private static void Describe(DesignerWidget widget, int indent, List<string> lines)
    {
        var binding = widget.BoundField is null ? "" : " -> " + widget.BoundField;
        lines.Add(new string(c: ' ', count: indent * 2) + widget.ShortType + binding);
        foreach (var child in widget.Children)
        {
            Describe(child, indent + 1, lines);
        }
    }
}
