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
        parentNode.SetAttribute("Name", renderData.PanelTitle);
        parentNode.SetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance", "Grid");
        parentNode.SetAttribute("Type", "Grid");
        parentNode.SetAttribute("HasPanelConfiguration", XmlConvert.ToString(true));
        parentNode.SetAttribute("ModelId", modelId);
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

        parentNode.SetAttribute("DefaultPanelView", XmlConvert.ToString((int)defaultPanelView));
        if (renderData.NewRecordInDetailView)
        {
            parentNode.SetAttribute(
                "NewRecordView",
                XmlConvert.ToString((int)OrigamPanelViewMode.Form)
            );
        }

        parentNode.SetAttribute("IsHeadless", XmlConvert.ToString(renderData.HideNavigationPanel));
        parentNode.SetAttribute(
            "DisableActionButtons",
            XmlConvert.ToString(renderData.DisableActionButtons)
        );
        parentNode.SetAttribute("ShowAddButton", XmlConvert.ToString(renderData.ShowNewButton));
        parentNode.SetAttribute("HideCopyButton", XmlConvert.ToString(renderData.HideCopyButton));
        parentNode.SetAttribute(
            "ShowDeleteButton",
            XmlConvert.ToString(renderData.ShowDeleteButton)
        );
        parentNode.SetAttribute(
            "ShowSelectionCheckboxes",
            XmlConvert.ToString(showSelectionCheckboxes)
        );
        parentNode.SetAttribute(
            "IsGridHeightDynamic",
            XmlConvert.ToString(renderData.IsGridHeightDynamic)
        );
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
        if (settings.DisableAttachments)
        {
            parentNode.SetAttribute("SupportsAttachments", "false");
        }
        if (renderData.SelectionMember != "")
        {
            parentNode.SetAttribute("SelectionMember", renderData.SelectionMember);
        }
        if (renderData.OrderMember != "")
        {
            parentNode.SetAttribute("OrderMember", renderData.OrderMember);
        }
        if (renderData.IsGridHeightDynamic)
        {
            parentNode.SetAttribute(
                "MaxDynamicGridHeight",
                XmlConvert.ToString(renderData.MaxDynamicGridHeight)
            );
        }
        parentNode.SetAttribute(
            "IsDraggingEnabled",
            XmlConvert.ToString(renderData.IsDraggingEnabled)
        );
        if (renderData.IsDraggingEnabled)
        {
            parentNode.SetAttribute("DraggingLabelMember", renderData.DraggingLabelMember);
        }
        if (isIndependent)
        {
            parentNode.SetAttribute(
                "IndependentDataSourceId",
                XmlConvert.ToString(renderData.IndependentDataSourceId)
            );
            parentNode.SetAttribute(
                "IndependentDataSourceFilterId",
                XmlConvert.ToString(renderData.IndependentDataSourceFilterId)
            );
            parentNode.SetAttribute(
                "IndependentDataSourceSortId",
                XmlConvert.ToString(renderData.IndependentDataSourceSortId)
            );
        }
        // calendar view attributes
        if (renderData.IsCalendarSupported)
        {
            parentNode.SetAttribute("CalendarDateDueMember", renderData.CalendarDateDueMember);
            parentNode.SetAttribute("CalendarDateFromMember", renderData.CalendarDateFromMember);
            parentNode.SetAttribute("CalendarDateToMember", renderData.CalendarDateToMember);
            parentNode.SetAttribute(
                "CalendarDescriptionMember",
                renderData.CalendarDescriptionMember
            );
            parentNode.SetAttribute(
                "CalendarIsFinishedMember",
                renderData.CalendarIsFinishedMember
            );
            parentNode.SetAttribute("CalendarNameMember", renderData.CalendarNameMember);
            parentNode.SetAttribute(
                "CalendarResourceIdMember",
                renderData.CalendarResourceIdMember
            );
            parentNode.SetAttribute(
                "IsCalendarSupported",
                XmlConvert.ToString(renderData.IsCalendarSupported)
            );
            parentNode.SetAttribute(
                "DefaultCalendarView",
                XmlConvert.ToString(renderData.DefaultCalendarView)
            );
            parentNode.SetAttribute(
                "CalendarResourceNameLookupField",
                renderData.CalendarResourceNameLookupField
            );
            parentNode.SetAttribute(
                "CalendarCustomSortMember",
                renderData.CalendarCustomSortMember
            );
            if (renderData.CalendarRowHeightConstantId != Guid.Empty)
            {
                IParameterService parameterService =
                    ServiceManager.Services.GetService(typeof(IParameterService))
                    as IParameterService;
                string value = (string)
                    parameterService.GetParameterValue(
                        renderData.CalendarRowHeightConstantId,
                        OrigamDataType.String
                    );
                parentNode.SetAttribute("CalendarRowHeight", value);
            }
            parentNode.SetAttribute(
                "CalendarShowAllResources",
                XmlConvert.ToString(renderData.CalendarShowAllResources)
            );
            if (renderData.CalendarShowAllResources)
            {
                XmlElement calendarResourceListElement = parentNode.OwnerDocument.CreateElement(
                    "CalendarResourceList"
                );
                parentNode.AppendChild(calendarResourceListElement);
                Guid lookupId = (Guid)
                    table.Columns[renderData.CalendarResourceIdMember].ExtendedProperties[
                        Const.DefaultLookupIdAttribute
                    ];
                ComboBoxBuilder.BuildCommonDropdown(
                    calendarResourceListElement,
                    lookupId,
                    renderData.CalendarResourceIdMember,
                    table
                );
            }
            if (renderData.CalendarViewStyle != null)
            {
                parentNode.SetAttribute(
                    "CalendarViewStyle",
                    renderData.CalendarViewStyle.StyleDefinition()
                );
            }
        }
        // pipeline view attributes
        if (renderData.IsPipelineSupported)
        {
            parentNode.SetAttribute(
                "IsPipelineSupported",
                XmlConvert.ToString(renderData.IsPipelineSupported)
            );
            parentNode.SetAttribute("PipelineNameMember", renderData.PipelineNameMember);
            parentNode.SetAttribute("PipelineDateMember", renderData.PipelineDateMember);
            parentNode.SetAttribute("PipelinePriceMember", renderData.PipelinePriceMember);
            parentNode.SetAttribute("PipelineStateMember", renderData.PipelineStateMember);
            parentNode.SetAttribute(
                "PipelineStateLoookup",
                XmlConvert.ToString(renderData.PipelineStateLoookup)
            );
        }
        // map view attributes
        if (renderData.IsMapSupported)
        {
            parentNode.SetAttribute(
                "IsMapSupported",
                XmlConvert.ToString(renderData.IsMapSupported)
            );
            parentNode.SetAttribute("MapLocationMember", renderData.MapLocationMember);
            parentNode.SetAttribute("MapAzimuthMember", renderData.MapAzimuthMember);
            parentNode.SetAttribute("MapColorMember", renderData.MapColorMember);
            parentNode.SetAttribute("MapIconMember", renderData.MapIconMember);
            parentNode.SetAttribute("MapTextMember", renderData.MapTextMember);
            parentNode.SetAttribute("TextColorMember", renderData.MapTextColorMember);
            parentNode.SetAttribute("TextLocationMember", renderData.MapTextLocationMember);
            parentNode.SetAttribute("TextRotationMember", renderData.MapTextRotationMember);
            // MapViewLayers
            DataSet layers = core.DataService.Instance.LoadData(
                new Guid("29aa47ff-98f6-4dba-8e02-ab5ebad08162"),
                new Guid("d0556499-6859-4459-a239-04bd6359a862"),
                Guid.Empty,
                new Guid("368729ca-44af-4f25-8894-44353068ed04"),
                null,
                "OrigamMap_parReferenceCode",
                renderData.MapLayers
            );

            layers.DataSetName = "MapViewLayers";
            bool isZoomNull = true;
            int zoom = 0;
            string center = null;
            if (layers.Tables[0].Rows.Count > 0)
            {
                DataRow layerRow = layers.Tables[0].Rows[0];
                if (!layerRow.IsNull("OrigamMap_InitialZoom"))
                {
                    isZoomNull = false;
                    zoom = (int)layerRow["OrigamMap_InitialZoom"];
                }
                if (!layerRow.IsNull("OrigamMap_MapCenter"))
                {
                    center = (string)layerRow["OrigamMap_MapCenter"];
                }
            }
            if (!isZoomNull && zoom <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "OrigamMap_InitialZoom",
                    zoom,
                    "Initial zoom has to be greater than zero"
                );
            }
            if (center != null)
            {
                parentNode.SetAttribute("MapCenter", center);
            }
            if (!isZoomNull)
            {
                parentNode.SetAttribute("MapResolution", XmlConvert.ToString(zoom));
            }
            XmlDocumentFragment xdf = parentNode.OwnerDocument.CreateDocumentFragment();
            xdf.InnerXml = layers.GetXml();
            parentNode.AppendChild(xdf);
        }
        // visual edito view attributes
        if (renderData.IsVisualEditorSupported)
        {
            parentNode.SetAttribute(
                "IsVisualEditorSupported",
                XmlConvert.ToString(renderData.IsVisualEditorSupported)
            );
        }
        string entityName = FormXmlBuilder.AddDataSource(
            dataSources,
            table,
            controlId,
            isIndependent
        );
        parentNode.SetAttribute("Entity", entityName);
        parentNode.SetAttribute("DataMember", renderData.DataMember);
        // Properties
        XmlElement propertiesElement = parentNode.OwnerDocument.CreateElement("Properties");
        parentNode.AppendChild(propertiesElement);
        // Actions
        XmlElement actionsElement = parentNode.OwnerDocument.CreateElement("Actions");
        parentNode.AppendChild(actionsElement);
        // Implicit filter
        if (renderData.ImplicitFilter != null && renderData.ImplicitFilter != "")
        {
            XmlElement filterElement = parentNode.OwnerDocument.CreateElement("Filter");
            parentNode.AppendChild(filterElement);
            XmlDocumentFragment xdf = filterElement.OwnerDocument.CreateDocumentFragment();
            xdf.InnerXml = renderData.ImplicitFilter;
            filterElement.AppendChild(xdf);
        }
        // ID Property
        XmlElement idPropertyElement = parentNode.OwnerDocument.CreateElement("Property");
        propertiesElement.AppendChild(idPropertyElement);
        idPropertyElement.SetAttribute("Id", primaryKeyColumnName);
        idPropertyElement.SetAttribute("Name", primaryKeyColumnName);
        idPropertyElement.SetAttribute("Entity", "String");
        idPropertyElement.SetAttribute("Column", "Text");
        idPropertyElement.SetAttribute("AlwaysHidden", "true");
        XmlElement formRootElement = parentNode.OwnerDocument.CreateElement("FormRoot");
        formRootElement.SetAttribute("Type", "Canvas");
        parentNode.AppendChild(formRootElement);
        SchemaService ss =
            ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
        ChartSchemaItemProvider chartProvider =
            ss.GetProvider(typeof(ChartSchemaItemProvider)) as ChartSchemaItemProvider;
        List<AbstractChart> charts = chartProvider.Charts(formId, table.TableName);
        if (charts.Count > 0)
        {
            parentNode.SetAttribute("IsChartSupported", XmlConvert.ToString(true));
            // Charts
            XmlElement chartsElement = parentNode.OwnerDocument.CreateElement("Charts");
            parentNode.AppendChild(chartsElement);
            foreach (AbstractChart chart in charts)
            {
                chartsElement.AppendChild(
                    ChartBuilder.Build(chartsElement.OwnerDocument, chart, table)
                );
            }
        }
    }

    public static XmlElement FormRootElement(XmlElement panelNode)
    {
        return panelNode.SelectSingleNode("FormRoot") as XmlElement;
    }

    public static XmlElement PropertiesElement(XmlElement panelNode)
    {
        return panelNode.SelectSingleNode("Properties") as XmlElement;
    }

    public static XmlElement ActionsElement(XmlElement panelNode)
    {
        return panelNode.SelectSingleNode("Actions") as XmlElement;
    }
}
