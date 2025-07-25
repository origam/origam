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
using Origam.Gui;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Controls;

public abstract class LabeledEditor: ControlBase
{
    [Category("(ORIGAM)")]
    public int CaptionLength { get; set; } = 100;

    [Category("(ORIGAM)")]
    [Description("Column Width (in pixels) to be used in grid-view. If the value is less than then zero, then the column is hidden by default. However, when it's enabled, the abs(configured value) is used.")]
    public int GridColumnWidth { get; set; } = 100;

    [Localizable(true)]
    [Category("(ORIGAM)")]
    public CaptionPosition CaptionPosition { get; set; } = CaptionPosition.Left;
    
    public override void Initialize(ControlSetItem controlSetItem)
    {
        Height = 20;
        Width = 400;
    }
}