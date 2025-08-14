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
using Origam.Architect.Server.Attributes;

namespace Origam.Architect.Server.Controls;

public class AsTextBox : LabeledEditor, IAsControl
{
    [Description(
        "Valid only for numeric data types. If specified, it will override default formatting for the given data type."
    )]
    public string CustomNumericFormat { get; set; }

    public bool AllowTab { get; set; } = false;

    [Category("Behavior")]
    public bool ReadOnly { get; set; } = false;

    [Category("(ORIGAM)")]
    public string GridColumnCaption { get; set; }

    [Category("(ORIGAM)")]
    public string Caption { get; set; }

    [Localizable(true)]
    [Category("Behavior")]
    [MergableProperty(false)]
    public int TabIndex { get; set; }

    public bool HideOnForm { get; set; }

    public bool IsRichText { get; set; } = false;

    [Category("Behavior")]
    public bool Multiline { get; set; }

    public bool IsPassword { get; set; }

    public Object Value { get; set; }

    [NotAModelProperty]
    public string DefaultBindableProperty => "Value";
}
