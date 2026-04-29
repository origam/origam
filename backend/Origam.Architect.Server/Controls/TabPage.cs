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

using System.ComponentModel;
using System.Text.RegularExpressions;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Controls;

public class TabPage : IControl
{
    [Localizable(isLocalizable: true)]
    [Browsable(browsable: true)]
    public string Text { get; set; }

    [Category(category: "Layout")]
    [Browsable(browsable: false)]
    public int Top { get; set; }

    [Category(category: "Layout")]
    [Browsable(browsable: false)]
    public int Left { get; set; }

    [Category(category: "Layout")]
    [Browsable(browsable: false)]
    public int Height { get; set; } = 200;

    [Category(category: "Layout")]
    [Browsable(browsable: false)]
    public int Width { get; set; } = 200;

    public void Initialize(ControlSetItem controlSetItem)
    {
        var tabPageNumberRegex = new Regex(pattern: @"TabPage(\d*)");
        var tabs = controlSetItem.ParentItem.ChildItems.OfType<ControlSetItem>().ToList();
        var labelTexts = tabs.Select(selector: tab =>
                tab.GetPropertyOrNull(propertyName: "Text")?.Value
            )
            .Where(predicate: labelText => labelText != null);

        int maxTabPageNumber = labelTexts
            .Where(predicate: labelText => labelText.StartsWith(value: "TabPage"))
            .Select(selector: labelText =>
            {
                var match = tabPageNumberRegex.Match(input: labelText);
                return match.Groups[groupnum: 1].Value == ""
                    ? 0
                    : int.Parse(s: match.Groups[groupnum: 1].Value);
            })
            .DefaultIfEmpty(defaultValue: 0)
            .Max();

        Text = $"TabPage{maxTabPageNumber + 1}";
        string height = tabs.First().GetPropertyOrNull(propertyName: "Height")?.Value;
        if (!string.IsNullOrEmpty(value: height))
        {
            Height = int.Parse(s: height);
        }
        string width = tabs.First().GetPropertyOrNull(propertyName: "Width")?.Value;
        if (!string.IsNullOrEmpty(value: width))
        {
            Width = int.Parse(s: width);
        }
    }
}
