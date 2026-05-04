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
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.OrigamEngine.ModelXmlBuilders;

/// <summary>
/// Summary description for ReportPanelBuilder.
/// </summary>
public class ReportPanelBuilder
{
    public static void Build(
        XmlElement parentNode,
        UIElementRenderData renderData,
        DataTable table,
        ControlSetItem control
    )
    {
        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        AbstractReport report =
            persistence.SchemaProvider.RetrieveInstance(
                type: typeof(AbstractReport),
                primaryKey: new ModelElementKey(id: new Guid(g: renderData.ReportId))
            ) as AbstractReport;
        parentNode.SetAttribute(name: "X", value: XmlConvert.ToString(value: renderData.Left));
        parentNode.SetAttribute(name: "Y", value: XmlConvert.ToString(value: renderData.Top));
        parentNode.SetAttribute(name: "Width", value: XmlConvert.ToString(value: renderData.Width));
        parentNode.SetAttribute(
            localName: "type",
            namespaceURI: "http://www.w3.org/2001/XMLSchema-instance",
            value: "UIElement"
        );
        parentNode.SetAttribute(name: "Type", value: "ReportButton");
        parentNode.SetAttribute(name: "Entity", value: table.TableName);
        parentNode.SetAttribute(name: "ReportId", value: renderData.ReportId);
        parentNode.SetAttribute(
            name: "Text",
            value: (report.Caption == null ? report.Name : report.Caption)
        );
        XmlElement reportParametersElement = parentNode.OwnerDocument.CreateElement(
            name: "ReportParameters"
        );
        parentNode.AppendChild(newChild: reportParametersElement);
        foreach (
            var mapping in control.ChildItemsByType<ColumnParameterMapping>(
                itemType: ColumnParameterMapping.CategoryConst
            )
        )
        {
            XmlElement reportParamElement = parentNode.OwnerDocument.CreateElement(
                name: "ReportParameterMapping"
            );
            reportParametersElement.AppendChild(newChild: reportParamElement);
            reportParamElement.SetAttribute(name: "ParameterName", value: mapping.Name);
            reportParamElement.SetAttribute(name: "FieldName", value: mapping.ColumnName);
        }
    }
}
