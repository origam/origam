#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

using Origam.Schema;
using Origam.Schema.GuiModel;


//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.1.4322.573
//
//     Changes to this file may cause incorrect behavior and will be lost if 
//     the code is regenerated.
// </autogenerated>
//using System.Xml.Serialization;
namespace Origam.Gui.Designer;
/// <remarks/>
public class FDToolbox 
{
	public Category[] FDToolboxCategories;
}
/// <remarks/>
public class Category 
{
	public FDToolboxItem[] FDToolboxItem;
	public string DisplayName;
}
/// <remarks/>
public class FDToolboxItem 
{
	public string Type;
	public bool IsComplexType;
	public bool IsFieldItem;
	public bool IsExternal;
	public OrigamDataType OrigamDataType;
	public Origam.Schema.GuiModel.PanelControlSet PanelSetItem;
	public string ColumnName;
	public string Name;
	public ControlItem ControlItem { get; set; }
}
