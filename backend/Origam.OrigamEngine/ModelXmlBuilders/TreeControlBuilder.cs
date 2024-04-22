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

using System;
using System.Data;
using System.Xml;
using System.Collections;

namespace Origam.OrigamEngine.ModelXmlBuilders;

/// <summary>
/// Summary description for TreeControlBuilder.
/// </summary>
public class TreeControlBuilder
{
	public static void Build(XmlElement parentNode, UIElementRenderData  renderData, 
		DataTable table, string controlId, Hashtable dataSources, bool isIndependent)
	{
			parentNode.SetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance", "UIElement");
			parentNode.SetAttribute("Type", "TreePanel");
			parentNode.SetAttribute("Entity", table.TableName);
			parentNode.SetAttribute("IdProperty", renderData.IdColumn);
			parentNode.SetAttribute("ParentIdProperty", renderData.ParentIdColumn);
			parentNode.SetAttribute("NameProperty", renderData.NameColumn);
			parentNode.SetAttribute("DataMember", renderData.DataMember);
//			parentNode.SetAttribute("Width", XmlConvert.ToString(width));
//			parentNode.SetAttribute("Height", XmlConvert.ToString(height));

			FormXmlBuilder.AddDataSource(dataSources, table, controlId, isIndependent);
		}

	public static void Build2(XmlElement parentNode, string formParameterName, Guid treeId)
	{
			parentNode.SetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance", "UIElement");
			parentNode.SetAttribute("Type", "TreePanelEx");

			parentNode.SetAttribute("TreeId", treeId.ToString());
			parentNode.SetAttribute("FormParameterName", formParameterName);
		}
}