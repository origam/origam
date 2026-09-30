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

namespace Origam.AI.Agent.Tests.Infrastructure.Architect;

public sealed record SectionWidget(
    string Type,
    string? BoundField,
    IReadOnlyDictionary<string, string> Properties,
    IReadOnlyList<SectionWidget> Children
)
{
    public string ShortType => Type[(Type.LastIndexOf('.') + 1)..];

    public string Property(string name)
    {
        return Properties.GetValueOrDefault(name, defaultValue: string.Empty);
    }

    public IEnumerable<SectionWidget> Descendants()
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

    public IReadOnlyList<SectionWidget> FindAll(string shortType, string? boundField)
    {
        return Descendants()
            .Where(widget =>
                widget.ShortType == shortType
                && string.Equals(widget.BoundField, boundField, StringComparison.Ordinal)
            )
            .ToList();
    }

    public SectionWidget? Find(string shortType, string? boundField)
    {
        return FindAll(shortType, boundField).FirstOrDefault();
    }

    public string Describe()
    {
        var lines = new List<string>();
        Describe(this, indent: 0, lines);
        return string.Join(Environment.NewLine, lines);
    }

    private static void Describe(SectionWidget widget, int indent, List<string> lines)
    {
        var binding = widget.BoundField is null ? "" : " -> " + widget.BoundField;
        lines.Add(new string(c: ' ', count: indent * 2) + widget.ShortType + binding);
        foreach (var child in widget.Children)
        {
            Describe(child, indent + 1, lines);
        }
    }
}
