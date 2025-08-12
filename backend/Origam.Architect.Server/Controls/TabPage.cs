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
using Origam.Architect.Server.Controls;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Controls;

public class TabPage : IControl
{
    [Localizable(true)]
    [Browsable(true)]
    public string Text { get; set; }

    [Category("Layout")]
    [Browsable(false)]
    public int Top { get; set; }

    [Category("Layout")]
    [Browsable(false)]
    public int Left { get; set; }

    [Category("Layout")]
    [Browsable(false)]
    public int Height { get; set; } = 200;

    [Category("Layout")]
    [Browsable(false)]
    public int Width { get; set; } = 200;

    public void Initialize(ControlSetItem controlSetItem)
    {
        Regex tabPageNumberRegex = new Regex(@"TabPage(\d*)");
        var tabs = controlSetItem.ParentItem.ChildItems.OfType<ControlSetItem>().ToList();
        var labelTexts = tabs.Select(tab => tab.GetPropertyOrNull("Text")?.Value)
            .Where(labelText => labelText != null);

        int maxTabPageNumber = labelTexts
            .Where(labelText => labelText.StartsWith("TabPage"))
            .Select(labelText =>
            {
                var match = tabPageNumberRegex.Match(labelText);
                return match.Groups[1].Value == "" ? 0 : int.Parse(match.Groups[1].Value);
            })
            .DefaultIfEmpty(0)
            .Max();

        Text = $"TabPage{maxTabPageNumber + 1}";
        string height = tabs.First().GetPropertyOrNull("Height")?.Value;
        if (!string.IsNullOrEmpty(height))
        {
            Height = int.Parse(height);
        }
        string width = tabs.First().GetPropertyOrNull("Width")?.Value;
        if (!string.IsNullOrEmpty(width))
        {
            Width = int.Parse(width);
        }
    }
}
