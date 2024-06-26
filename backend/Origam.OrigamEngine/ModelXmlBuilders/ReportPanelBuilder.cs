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
using System.Data;

using Origam.Workbench.Services;
using Origam.Schema.GuiModel;
using Origam.Schema;

namespace Origam.OrigamEngine.ModelXmlBuilders;
/// <summary>
/// Summary description for ReportPanelBuilder.
/// </summary>
public class ReportPanelBuilder
{
	public static void Build(XmlElement parentNode, UIElementRenderData renderData, 
		DataTable table, ControlSetItem control)
	{
		IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService))
			as IPersistenceService;
		AbstractReport report = persistence.SchemaProvider.RetrieveInstance(typeof(AbstractReport), 
			new ModelElementKey(new Guid(renderData.ReportId))) as AbstractReport;
		parentNode.SetAttribute("X", XmlConvert.ToString(renderData.Left));
		parentNode.SetAttribute("Y", XmlConvert.ToString(renderData.Top));
		parentNode.SetAttribute("Width", XmlConvert.ToString(renderData.Width));
		parentNode.SetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance", "UIElement");
		parentNode.SetAttribute("Type", "ReportButton");
		parentNode.SetAttribute("Entity", table.TableName);
		parentNode.SetAttribute("ReportId", renderData.ReportId);
		parentNode.SetAttribute("Text", (report.Caption == null ? report.Name : report.Caption));
		XmlElement reportParametersElement = parentNode.OwnerDocument.CreateElement("ReportParameters");
		parentNode.AppendChild(reportParametersElement);
		foreach(ColumnParameterMapping mapping in control.ChildItemsByType(ColumnParameterMapping.CategoryConst))
		{
			XmlElement reportParamElement = parentNode.OwnerDocument.CreateElement("ReportParameterMapping");
			reportParametersElement.AppendChild(reportParamElement);
			reportParamElement.SetAttribute("ParameterName", mapping.Name);
			reportParamElement.SetAttribute("FieldName", mapping.ColumnName);
		}
	}
}
