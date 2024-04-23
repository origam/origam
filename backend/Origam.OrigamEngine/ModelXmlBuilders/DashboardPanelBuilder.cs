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
using System.Xml;

using Origam.Schema.GuiModel;

namespace Origam.OrigamEngine.ModelXmlBuilders;

/// <summary>
/// Summary description for DashboardPanelBuilder.
/// </summary>
public class DashboardPanelBuilder
{
	public static void Build(PanelDashboardWidget panelWidget,
		Guid menuId, Guid dashboardItemId, XmlElement itemChildren,
		XmlElement dataSourcesElement)
	{
			XmlDocument panelDoc = FormXmlBuilder.GetXmlFromPanel(panelWidget.PanelId, panelWidget.Caption, menuId, dashboardItemId, false);

			// clone panel definition
			itemChildren.InnerXml = panelDoc["Window"]["UIRoot"].OuterXml.Replace("UIRoot", "UIElement");

			XmlElement panelElement = itemChildren.FirstChild as XmlElement;

			// clone data source
			XmlElement originalDataSourceElement = panelDoc["Window"]["DataSources"].FirstChild as XmlElement;
			string dataSourceName = originalDataSourceElement.GetAttribute("Entity");
			string dataSourceIdentifier = originalDataSourceElement.GetAttribute("Identifier");

			XmlElement newDataSourceElement = FormXmlBuilder.AddDataSourceElement(dataSourcesElement, dataSourceName, dataSourceIdentifier, null, null);

			newDataSourceElement.InnerXml = originalDataSourceElement.InnerXml;
		}
}