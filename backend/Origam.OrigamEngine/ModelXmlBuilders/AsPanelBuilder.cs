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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using Origam.DA;
using Origam.Gui;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.OrigamEngine.ModelXmlBuilders;

/// <summary>
/// Summary description for AsPanelBuilder.
/// </summary>
public class AsPanelBuilder
{
    public static void Build(
        XmlElement parentNode,
        UIElementRenderData renderData,
        string modelId,
        string controlId,
        DataTable table,
        Hashtable dataSources,
        string primaryKeyColumnName,
        bool showSelectionCheckboxes,
        Guid formId,
        bool isIndependent
    )
    {
        parentNode.SetAttribute(name: "Name", value: renderData.PanelTitle);
        parentNode.SetAttribute(
            localName: "type",
            namespaceURI: "http://www.w3.org/2001/XMLSchema-instance",
            value: "Grid"
        );
        parentNode.SetAttribute(name: "Type", value: "Grid");
        parentNode.SetAttribute(
            name: "HasPanelConfiguration",
            value: XmlConvert.ToString(value: true)
        );
        parentNode.SetAttribute(name: "ModelId", value: modelId);
        OrigamPanelViewMode defaultPanelView = OrigamPanelViewMode.Form;
        if (renderData.IsGridVisible)
        {
            defaultPanelView = OrigamPanelViewMode.Grid;
        }

        if (renderData.IsCalendarVisible)
        {
            defaultPanelView = OrigamPanelViewMode.Calendar;
        }

        if (renderData.IsPipelineVisible)
        {
            defaultPanelView = OrigamPanelViewMode.Pipeline;
        }

        if (renderData.IsMapVisible)
        {
            defaultPanelView = OrigamPanelViewMode.Map;
        }

        if (renderData.IsVisualEditorVisible)
        {
            defaultPanelView = OrigamPanelViewMode.VisualEditor;
        }

        parentNode.SetAttribute(
            name: "DefaultPanelView",
            value: XmlConvert.ToString(value: (int)defaultPanelView)
        );
        if (renderData.NewRecordInDetailView)
        {
            parentNode.SetAttribute(
                name: "NewRecordView",
                value: XmlConvert.ToString(value: (int)OrigamPanelViewMode.Form)
            );
        }

        parentNode.SetAttribute(
            name: "IsHeadless",
            value: XmlConvert.ToString(value: renderData.HideNavigationPanel)
        );
        parentNode.SetAttribute(
            name: "DisableActionButtons",
            value: XmlConvert.ToString(value: renderData.DisableActionButtons)
        );
        parentNode.SetAttribute(
            name: "ShowAddButton",
            value: XmlConvert.ToString(value: renderData.ShowNewButton)
        );
        parentNode.SetAttribute(
            name: "HideCopyButton",
            value: XmlConvert.ToString(value: renderData.HideCopyButton)
        );
        parentNode.SetAttribute(
            name: "ShowDeleteButton",
            value: XmlConvert.ToString(value: renderData.ShowDeleteButton)
        );
        parentNode.SetAttribute(
            name: "ShowSelectionCheckboxes",
            value: XmlConvert.ToString(value: showSelectionCheckboxes)
        );
        parentNode.SetAttribute(
            name: "IsGridHeightDynamic",
            value: XmlConvert.ToString(value: renderData.IsGridHeightDynamic)
        );
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
        if (settings.DisableAttachments)
        {
            parentNode.SetAttribute(name: "SupportsAttachments", value: "false");
        }
        if (renderData.SelectionMember != "")
        {
            parentNode.SetAttribute(name: "SelectionMember", value: renderData.SelectionMember);
        }
        if (renderData.OrderMember != "")
        {
            parentNode.SetAttribute(name: "OrderMember", value: renderData.OrderMember);
        }
        if (renderData.IsGridHeightDynamic)
        {
            parentNode.SetAttribute(
                name: "MaxDynamicGridHeight",
                value: XmlConvert.ToString(value: renderData.MaxDynamicGridHeight)
            );
        }
        parentNode.SetAttribute(
            name: "IsDraggingEnabled",
            value: XmlConvert.ToString(value: renderData.IsDraggingEnabled)
        );
        if (renderData.IsDraggingEnabled)
        {
            parentNode.SetAttribute(
                name: "DraggingLabelMember",
                value: renderData.DraggingLabelMember
            );
        }
        if (isIndependent)
        {
            parentNode.SetAttribute(
                name: "IndependentDataSourceId",
                value: XmlConvert.ToString(value: renderData.IndependentDataSourceId)
            );
            parentNode.SetAttribute(
                name: "IndependentDataSourceFilterId",
                value: XmlConvert.ToString(value: renderData.IndependentDataSourceFilterId)
            );
            parentNode.SetAttribute(
                name: "IndependentDataSourceSortId",
                value: XmlConvert.ToString(value: renderData.IndependentDataSourceSortId)
            );
        }
        // calendar view attributes
        if (renderData.IsCalendarSupported)
        {
            parentNode.SetAttribute(
                name: "CalendarDateDueMember",
                value: renderData.CalendarDateDueMember
            );
            parentNode.SetAttribute(
                name: "CalendarDateFromMember",
                value: renderData.CalendarDateFromMember
            );
            parentNode.SetAttribute(
                name: "CalendarDateToMember",
                value: renderData.CalendarDateToMember
            );
            parentNode.SetAttribute(
                name: "CalendarDescriptionMember",
                value: renderData.CalendarDescriptionMember
            );
            parentNode.SetAttribute(
                name: "CalendarIsFinishedMember",
                value: renderData.CalendarIsFinishedMember
            );
            parentNode.SetAttribute(
                name: "CalendarNameMember",
                value: renderData.CalendarNameMember
            );
            parentNode.SetAttribute(
                name: "CalendarResourceIdMember",
                value: renderData.CalendarResourceIdMember
            );
            parentNode.SetAttribute(
                name: "IsCalendarSupported",
                value: XmlConvert.ToString(value: renderData.IsCalendarSupported)
            );
            parentNode.SetAttribute(
                name: "DefaultCalendarView",
                value: XmlConvert.ToString(value: renderData.DefaultCalendarView)
            );
            parentNode.SetAttribute(
                name: "CalendarResourceNameLookupField",
                value: renderData.CalendarResourceNameLookupField
            );
            parentNode.SetAttribute(
                name: "CalendarCustomSortMember",
                value: renderData.CalendarCustomSortMember
            );
            if (renderData.CalendarRowHeightConstantId != Guid.Empty)
            {
                IParameterService parameterService =
                    ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
                    as IParameterService;
                string value = (string)
                    parameterService.GetParameterValue(
                        id: renderData.CalendarRowHeightConstantId,
                        targetType: OrigamDataType.String
                    );
                parentNode.SetAttribute(name: "CalendarRowHeight", value: value);
            }
            parentNode.SetAttribute(
                name: "CalendarShowAllResources",
                value: XmlConvert.ToString(value: renderData.CalendarShowAllResources)
            );
            if (renderData.CalendarShowAllResources)
            {
                XmlElement calendarResourceListElement = parentNode.OwnerDocument.CreateElement(
                    name: "CalendarResourceList"
                );
                parentNode.AppendChild(newChild: calendarResourceListElement);
                Guid lookupId = (Guid)
                    table.Columns[name: renderData.CalendarResourceIdMember].ExtendedProperties[
                        key: Const.DefaultLookupIdAttribute
                    ];
                ComboBoxBuilder.BuildCommonDropdown(
                    propertyElement: calendarResourceListElement,
                    lookupId: lookupId,
                    bindingMember: renderData.CalendarResourceIdMember,
                    table: table
                );
            }
            if (renderData.CalendarViewStyle != null)
            {
                parentNode.SetAttribute(
                    name: "CalendarViewStyle",
                    value: renderData.CalendarViewStyle.StyleDefinition()
                );
            }
        }
        // pipeline view attributes
        if (renderData.IsPipelineSupported)
        {
            parentNode.SetAttribute(
                name: "IsPipelineSupported",
                value: XmlConvert.ToString(value: renderData.IsPipelineSupported)
            );
            parentNode.SetAttribute(
                name: "PipelineNameMember",
                value: renderData.PipelineNameMember
            );
            parentNode.SetAttribute(
                name: "PipelineDateMember",
                value: renderData.PipelineDateMember
            );
            parentNode.SetAttribute(
                name: "PipelinePriceMember",
                value: renderData.PipelinePriceMember
            );
            parentNode.SetAttribute(
                name: "PipelineStateMember",
                value: renderData.PipelineStateMember
            );
            parentNode.SetAttribute(
                name: "PipelineStateLoookup",
                value: XmlConvert.ToString(value: renderData.PipelineStateLoookup)
            );
        }
        // map view attributes
        if (renderData.IsMapSupported)
        {
            parentNode.SetAttribute(
                name: "IsMapSupported",
                value: XmlConvert.ToString(value: renderData.IsMapSupported)
            );
            parentNode.SetAttribute(name: "MapLocationMember", value: renderData.MapLocationMember);
            parentNode.SetAttribute(name: "MapAzimuthMember", value: renderData.MapAzimuthMember);
            parentNode.SetAttribute(name: "MapColorMember", value: renderData.MapColorMember);
            parentNode.SetAttribute(name: "MapIconMember", value: renderData.MapIconMember);
            parentNode.SetAttribute(name: "MapTextMember", value: renderData.MapTextMember);
            parentNode.SetAttribute(name: "TextColorMember", value: renderData.MapTextColorMember);
            parentNode.SetAttribute(
                name: "TextLocationMember",
                value: renderData.MapTextLocationMember
            );
            parentNode.SetAttribute(
                name: "TextRotationMember",
                value: renderData.MapTextRotationMember
            );
            // MapViewLayers
            DataSet layers = core.DataService.Instance.LoadData(
                dataStructureId: new Guid(g: "29aa47ff-98f6-4dba-8e02-ab5ebad08162"),
                methodId: new Guid(g: "d0556499-6859-4459-a239-04bd6359a862"),
                defaultSetId: Guid.Empty,
                sortSetId: new Guid(g: "368729ca-44af-4f25-8894-44353068ed04"),
                transactionId: null,
                paramName1: "OrigamMap_parReferenceCode",
                paramValue1: renderData.MapLayers
            );

            layers.DataSetName = "MapViewLayers";
            bool isZoomNull = true;
            int zoom = 0;
            string center = null;
            if (layers.Tables[index: 0].Rows.Count > 0)
            {
                DataRow layerRow = layers.Tables[index: 0].Rows[index: 0];
                if (!layerRow.IsNull(columnName: "OrigamMap_InitialZoom"))
                {
                    isZoomNull = false;
                    zoom = (int)layerRow[columnName: "OrigamMap_InitialZoom"];
                }
                if (!layerRow.IsNull(columnName: "OrigamMap_MapCenter"))
                {
                    center = (string)layerRow[columnName: "OrigamMap_MapCenter"];
                }
            }
            if (!isZoomNull && zoom <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "OrigamMap_InitialZoom",
                    actualValue: zoom,
                    message: "Initial zoom has to be greater than zero"
                );
            }
            if (center != null)
            {
                parentNode.SetAttribute(name: "MapCenter", value: center);
            }
            if (!isZoomNull)
            {
                parentNode.SetAttribute(
                    name: "MapResolution",
                    value: XmlConvert.ToString(value: zoom)
                );
            }
            XmlDocumentFragment xdf = parentNode.OwnerDocument.CreateDocumentFragment();
            xdf.InnerXml = layers.GetXml();
            parentNode.AppendChild(newChild: xdf);
        }
        // visual edito view attributes
        if (renderData.IsVisualEditorSupported)
        {
            parentNode.SetAttribute(
                name: "IsVisualEditorSupported",
                value: XmlConvert.ToString(value: renderData.IsVisualEditorSupported)
            );
        }
        string entityName = FormXmlBuilder.AddDataSource(
            dataSources: dataSources,
            table: table,
            controlId: controlId,
            isIndependent: isIndependent
        );
        parentNode.SetAttribute(name: "Entity", value: entityName);
        parentNode.SetAttribute(name: "DataMember", value: renderData.DataMember);
        // Properties
        XmlElement propertiesElement = parentNode.OwnerDocument.CreateElement(name: "Properties");
        parentNode.AppendChild(newChild: propertiesElement);
        // Actions
        XmlElement actionsElement = parentNode.OwnerDocument.CreateElement(name: "Actions");
        parentNode.AppendChild(newChild: actionsElement);
        // Implicit filter
        if (renderData.ImplicitFilter != null && renderData.ImplicitFilter != "")
        {
            XmlElement filterElement = parentNode.OwnerDocument.CreateElement(name: "Filter");
            parentNode.AppendChild(newChild: filterElement);
            XmlDocumentFragment xdf = filterElement.OwnerDocument.CreateDocumentFragment();
            xdf.InnerXml = renderData.ImplicitFilter;
            filterElement.AppendChild(newChild: xdf);
        }
        // ID Property
        XmlElement idPropertyElement = parentNode.OwnerDocument.CreateElement(name: "Property");
        propertiesElement.AppendChild(newChild: idPropertyElement);
        idPropertyElement.SetAttribute(name: "Id", value: primaryKeyColumnName);
        idPropertyElement.SetAttribute(name: "Name", value: primaryKeyColumnName);
        idPropertyElement.SetAttribute(name: "Entity", value: "String");
        idPropertyElement.SetAttribute(name: "Column", value: "Text");
        idPropertyElement.SetAttribute(name: "AlwaysHidden", value: "true");
        XmlElement formRootElement = parentNode.OwnerDocument.CreateElement(name: "FormRoot");
        formRootElement.SetAttribute(name: "Type", value: "Canvas");
        parentNode.AppendChild(newChild: formRootElement);
        SchemaService ss =
            ServiceManager.Services.GetService(serviceType: typeof(SchemaService)) as SchemaService;
        ChartSchemaItemProvider chartProvider =
            ss.GetProvider(type: typeof(ChartSchemaItemProvider)) as ChartSchemaItemProvider;
        List<AbstractChart> charts = chartProvider.Charts(formId: formId, entity: table.TableName);
        if (charts.Count > 0)
        {
            parentNode.SetAttribute(
                name: "IsChartSupported",
                value: XmlConvert.ToString(value: true)
            );
            // Charts
            XmlElement chartsElement = parentNode.OwnerDocument.CreateElement(name: "Charts");
            parentNode.AppendChild(newChild: chartsElement);
            foreach (AbstractChart chart in charts)
            {
                chartsElement.AppendChild(
                    newChild: ChartBuilder.Build(
                        doc: chartsElement.OwnerDocument,
                        chart: chart,
                        table: table
                    )
                );
            }
        }
    }

    public static XmlElement FormRootElement(XmlElement panelNode)
    {
        return panelNode.SelectSingleNode(xpath: "FormRoot") as XmlElement;
    }

    public static XmlElement PropertiesElement(XmlElement panelNode)
    {
        return panelNode.SelectSingleNode(xpath: "Properties") as XmlElement;
    }

    public static XmlElement ActionsElement(XmlElement panelNode)
    {
        return panelNode.SelectSingleNode(xpath: "Actions") as XmlElement;
    }
}
