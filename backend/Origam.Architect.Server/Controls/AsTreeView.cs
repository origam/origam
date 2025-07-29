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

namespace Origam.Architect.Server.Controls;

public class AsTreeView: ControlBase
{
    [Category("Data")]
    [Description("Identifier of the parent. Note: this member must have the same type as identifier column.")]
    public string ParentIDColumn { get; set; }
    
    [Category("Data")]
    [Description("Identifier member, in most cases this is primary column of the table.")]
    public string IDColumn { get; set; }

    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }
    
    [Editor("System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Windows.Forms.Design.DataMemberListEditor, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    [Category("Data")]
    [Description("Data member of the tree.")]
    public string DataMember { get; set; }
    
    [Category("Data")]
    [Description("Name member. Note: editing of this column available only with types that support converting from string.")]
    public string NameColumn { get; set; }
}
