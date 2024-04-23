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

using Origam.Schema;
using Origam.Schema.GuiModel;

namespace Origam.OrigamEngine.ModelXmlBuilders;

/// <summary>
/// Summary description for DashboardSimpleWidgetBuilder.
/// </summary>
public class DashboardSimpleWidgetBuilder
{
	public static void Build(XmlDocument doc, AbstractSimpleDashboardWidget simpleWidget,
		Guid dashboardItemId, string caption, XmlElement itemChildren,
		XmlElement dataSourcesElement)
	{
			string modelInstanceId = dashboardItemId.ToString();
			XmlElement element = itemChildren.AppendChild(doc.CreateElement("UIElement")) as XmlElement;

			element.SetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance", "UIElement");
			element.SetAttribute("Type", "SimplePanel");

			element.SetAttribute("Id", simpleWidget.Id.ToString());
			element.SetAttribute("ModelInstanceId", modelInstanceId);
			element.SetAttribute("Entity", modelInstanceId);
			element.SetAttribute("Height", "20");

			XmlElement componentElement = element.AppendChild(doc.CreateElement("Component")) as XmlElement;
			componentElement.SetAttribute("Name", caption);
			componentElement.SetAttribute("CaptionLength", "100");
			componentElement.SetAttribute("CaptionPosition", "Left");
			componentElement.SetAttribute("DataMember", "Value");

			LookupDashboardWidget lookupWidget = simpleWidget as LookupDashboardWidget;
			CurrencyDashboardWidget currencyWidget = simpleWidget as CurrencyDashboardWidget;
			TextDashboardWidget textWidget = simpleWidget as TextDashboardWidget;
			DateDashboardWidget dateWidget = simpleWidget as DateDashboardWidget;
			CheckBoxDashboardWidget checkWidghet = simpleWidget as CheckBoxDashboardWidget;

			if(lookupWidget != null)
			{
				ComboBoxBuilder.Build(componentElement, lookupWidget.LookupId, false, null, null);
			}
			else if(textWidget != null)
			{
				TextBoxBuilder.Build(componentElement, 
					new TextBoxBuildDefinition(OrigamDataType.String));
			}
			else if(currencyWidget != null)
			{
				TextBoxBuilder.Build(componentElement, 
					new TextBoxBuildDefinition(OrigamDataType.Currency));
			}
			else if(dateWidget != null)
			{
				DateBoxBuilder.Build(componentElement, null, null);
			}
			else if(checkWidghet != null)
			{
				CheckBoxBuilder.Build(componentElement, caption);
			}

			XmlElement newDataSourceElement = FormXmlBuilder.AddDataSourceElement(dataSourcesElement, modelInstanceId, "Id", null, null);
			newDataSourceElement.AppendChild(FormXmlBuilder.CreateDataSourceField(doc, "Id", 0));
			newDataSourceElement.AppendChild(FormXmlBuilder.CreateDataSourceField(doc, "Value", 1));
		}
}