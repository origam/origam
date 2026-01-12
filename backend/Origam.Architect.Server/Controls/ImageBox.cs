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

public class ImageBox : IControl
{
    [Category("(ORIGAM)")]
    [Description(
        "Column Width (in pixels) to be used in grid-view. If the value is less than then zero, then the column is hidden by default. However, when it's enabled, the abs(configured value) is used."
    )]
    public int GridColumnWidth { get; set; } = 100;

    [Category("(ORIGAM)")]
    public string GridColumnCaption { get; set; }

    [Browsable(true)]
    public ImageBoxSourceType SourceType { get; set; }

    public Object ImageData { get; set; }

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

    [Category("Behavior")]
    public int TabIndex { get; set; }

    public virtual void Initialize(ControlSetItem controlSetItem) { }
}
