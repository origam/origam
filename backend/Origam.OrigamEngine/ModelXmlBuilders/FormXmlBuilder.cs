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
using System.Linq;
using System.Xml;
using Origam.DA;
using Origam.DA.Service;
using Origam.Gui;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;
using Origam.Workflow;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.OrigamEngine.ModelXmlBuilders;

public class XmlOutput
{
    public XmlDocument Document { get; set; }
    public HashSet<Guid> ContainedLookups { get; set; } = new HashSet<Guid>();
}

public class FormXmlBuilder
{
    public const string WORKFLOW_FINISHED_FORMID = "C4E5DE43-69A4-40e6-9F7F-ED2AF8692429";
    private const int GENERIC_FIELD_HEIGHT = 20;
    private const int GENERIC_FIELD_VERTICAL_SPACE = 2;
    private static readonly string Entity_WorkQueueEntry = "WorkQueueEntry";
    private static readonly string RootGridXPath =
        "//*[(@Type='Grid' or @Type='TreePanel' or @Type='ReportButton') and @IsRootGrid = 'true']";

    public static XmlOutput GetXml(Guid menuId)
    {
        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        FormReferenceMenuItem menuItem =
            persistence.SchemaProvider.RetrieveInstance(
                type: typeof(FormReferenceMenuItem),
                primaryKey: new ModelElementKey(id: menuId)
            ) as FormReferenceMenuItem;
        bool readOnly = FormTools.IsFormMenuReadOnly(formRef: menuItem);
        return GetXml(
            item: menuItem.Screen,
            name: menuItem.DisplayName,
            isPreloaded: menuItem.ListDataStructure == null,
            menuId: menuItem.Id,
            structure: menuItem.Screen.DataStructure,
            forceReadOnly: readOnly,
            confirmSelectionChangeEntity: menuItem.SelectionChangeEntity
        );
    }

    public static XmlDocument GetXml(
        Guid formId,
        string name,
        bool isPreloaded,
        Guid menuId,
        string message,
        Guid structureId,
        bool forceReadOnly,
        string confirmSelectionChangeEntity
    )
    {
        if (formId == new Guid(g: WORKFLOW_FINISHED_FORMID))
        {
            return GetWorkflowFinishedXml(name: name, menuId: menuId, message: message);
        }
        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        FormControlSet item =
            persistence.SchemaProvider.RetrieveInstance(
                type: typeof(FormControlSet),
                primaryKey: new ModelElementKey(id: formId)
            ) as FormControlSet;
        DataStructure structure =
            persistence.SchemaProvider.RetrieveInstance(
                type: typeof(DataStructure),
                primaryKey: new ModelElementKey(id: structureId)
            ) as DataStructure;

        return GetXml(
            item: item,
            name: name,
            isPreloaded: isPreloaded,
            menuId: menuId,
            structure: structure,
            forceReadOnly: forceReadOnly,
            confirmSelectionChangeEntity: confirmSelectionChangeEntity
        ).Document;
    }

    public static XmlDocument GetXmlFromPanel(Guid panelId, string name, Guid menuId)
    {
        return GetXmlFromPanel(
            panelId: panelId,
            name: name,
            menuId: menuId,
            instanceId: panelId,
            forceHideNavigationPanel: true
        );
    }

    public static XmlDocument GetXmlFromPanel(
        Guid panelId,
        string name,
        Guid menuId,
        Guid instanceId,
        bool forceHideNavigationPanel
    )
    {
        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        PanelControlSet panel =
            persistence.SchemaProvider.RetrieveInstance(
                type: typeof(PanelControlSet),
                primaryKey: new ModelElementKey(id: panelId)
            ) as PanelControlSet;
        FormControlSet form = new FormControlSet();
        form.PersistenceProvider = persistence.SchemaProvider;
        ControlSetItem control = form.NewItem<ControlSetItem>(
            schemaExtensionId: form.SchemaExtensionId,
            group: form.Group
        );
        control.PrimaryKey = new ModelElementKey(id: instanceId);
        control.ControlItem = panel.PanelControl;
        foreach (
            var panelProperty in FormTools
                .GetItemFromControlSet(controlSet: panel)
                .ChildItemsByType<PropertyValueItem>(itemType: PropertyValueItem.CategoryConst)
        )
        {
            PropertyValueItem copy = panelProperty.Clone() as PropertyValueItem;

            if (forceHideNavigationPanel && copy.ControlPropertyItem.Name == "HideNavigationPanel")
            {
                copy.BoolValue = true;
            }
            copy.ParentItem = control;
            control.ChildItems.Add(item: copy);
        }
        PropertyValueItem dataMemberProperty = control.NewItem<PropertyValueItem>(
            schemaExtensionId: control.SchemaExtensionId,
            group: null
        );
        dataMemberProperty.Name = "DataMember";
        dataMemberProperty.Value = panel.DataEntity.Name;
        dataMemberProperty.ControlPropertyItem =
            control.ControlItem.GetChildByName(name: "DataMember") as ControlPropertyItem;
        DatasetGenerator gen = new DatasetGenerator(userDefinedParameters: true);
        return GetXml(
            item: form,
            dataset: gen.CreateDataSet(entity: panel.DataEntity),
            name: name,
            isPreloaded: true,
            menuId: menuId,
            forceReadOnly: false,
            confirmSelectionChangeEntity: ""
        ).Document;
    }

    internal static XmlDocument GetWindowBaseXml(
        string name,
        Guid menuId,
        int autoRefreshInterval,
        bool refreshOnFocus,
        bool autoSaveOnListRecordChange,
        bool requestSaveAfterUpdate
    )
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(
            xml: "<Window xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"/>"
        );
        // <Window>
        XmlElement windowElement = doc.FirstChild as XmlElement;
        doc.AppendChild(newChild: windowElement);
        windowElement.SetAttribute(name: "Title", value: name);
        windowElement.SetAttribute(name: "MenuId", value: menuId.ToString());
        windowElement.SetAttribute(name: "ShowInfoPanel", value: "false");
        windowElement.SetAttribute(
            name: "AutoRefreshInterval",
            value: XmlConvert.ToString(value: autoRefreshInterval)
        );
        windowElement.SetAttribute(name: "CacheOnClient", value: "true");
        if (refreshOnFocus || (autoRefreshInterval > 0))
        {
            windowElement.SetAttribute(name: "RefreshOnFocus", value: "true");
        }
        windowElement.SetAttribute(
            name: "AutoSaveOnListRecordChange",
            value: XmlConvert.ToString(value: autoSaveOnListRecordChange)
        );
        windowElement.SetAttribute(
            name: "RequestSaveAfterUpdate",
            value: XmlConvert.ToString(value: requestSaveAfterUpdate)
        );
        IPersistenceService persistenceService =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        FormReferenceMenuItem formReferenceMenuItem =
            persistenceService.SchemaProvider.RetrieveInstance(
                type: typeof(FormReferenceMenuItem),
                primaryKey: new ModelElementKey(id: menuId)
            ) as FormReferenceMenuItem;
        if (
            (formReferenceMenuItem != null)
            && (formReferenceMenuItem.DynamicFormLabelEntity != null)
            && !string.IsNullOrEmpty(value: formReferenceMenuItem.DynamicFormLabelField)
        )
        {
            windowElement.SetAttribute(
                name: "DynamicFormLabelSource",
                value: formReferenceMenuItem.DynamicFormLabelEntity.Name
                    + "."
                    + formReferenceMenuItem.DynamicFormLabelField
            );
        }
        // <Window><DataSources>
        XmlElement dataSourcesElement = doc.CreateElement(name: "DataSources");
        windowElement.AppendChild(newChild: dataSourcesElement);
        // <Window><ComponentBindings>
        XmlElement bindingsElement = doc.CreateElement(name: "ComponentBindings");
        windowElement.AppendChild(newChild: bindingsElement);
        // <Window><UIRoot>
        XmlElement uiRootElement = doc.CreateElement(name: "UIRoot");
        windowElement.AppendChild(newChild: uiRootElement);
        return doc;
    }

    private static XmlDocument GetWorkflowFinishedXml(string name, Guid menuId, string message)
    {
        XmlDocument doc = GetWindowBaseXml(
            name: name,
            menuId: menuId,
            autoRefreshInterval: 0,
            refreshOnFocus: false,
            autoSaveOnListRecordChange: false,
            requestSaveAfterUpdate: false
        );
        XmlElement windowElement = WindowElement(doc: doc);
        XmlElement uiRootElement = UIRootElement(windowElement: windowElement);
        windowElement.SetAttribute(name: "CacheOnClient", value: "false");
        uiRootElement.SetAttribute(
            localName: "type",
            namespaceURI: "UIElement",
            value: "http://www.w3.org/2001/XMLSchema-instance"
        );
        uiRootElement.SetAttribute(name: "Type", value: "WorkflowFinishedPanel");
        uiRootElement.SetAttribute(name: "Message", value: message);
        uiRootElement.SetAttribute(name: "showWorkflowCloseButton", value: "true");
        uiRootElement.SetAttribute(name: "showWorkflowRepeatButton", value: "true");
        return doc;
    }

    public static XmlOutput GetXml(
        FormControlSet item,
        string name,
        bool isPreloaded,
        Guid menuId,
        DataStructure structure,
        bool forceReadOnly,
        string confirmSelectionChangeEntity
    )
    {
        DatasetGenerator gen = new DatasetGenerator(userDefinedParameters: true);
        return GetXml(
            item: item,
            dataset: gen.CreateDataSet(ds: structure),
            name: name,
            isPreloaded: isPreloaded,
            menuId: menuId,
            forceReadOnly: forceReadOnly,
            confirmSelectionChangeEntity: confirmSelectionChangeEntity,
            structure: structure
        );
    }

    public static XmlElement CreateDataSourceField(XmlDocument doc, string name, int index)
    {
        XmlElement dataSourceFieldElement = doc.CreateElement(name: "Field");
        dataSourceFieldElement.SetAttribute(name: "Name", value: name);
        dataSourceFieldElement.SetAttribute(name: "Index", value: index.ToString());
        return dataSourceFieldElement;
    }

    private static void RenderDataSources(XmlElement windowElement, Hashtable dataSources)
    {
        XmlElement dataSourcesElement = DataSourcesElement(windowElement: windowElement);
        foreach (DictionaryEntry entry in dataSources)
        {
            DataTable table = (DataTable)entry.Value;
            string entityName = (string)entry.Key;
            if (table.PrimaryKey.Length == 0)
            {
                throw new ArgumentException(
                    message: $"Cannot render data source into xml, because the source table \"{table.TableName}\" does not have a primary key."
                );
            }
            string identifier = table.PrimaryKey[0].ColumnName;
            string lookupCacheKey = DatabaseTableName(table: table);
            string dataStructureEntityId = table.ExtendedProperties[key: "Id"]?.ToString();
            XmlElement dataSourceElement = AddDataSourceElement(
                dataSourcesElement: dataSourcesElement,
                entity: entityName,
                identifier: identifier,
                lookupCacheKey: lookupCacheKey,
                dataStructureEntityId: dataStructureEntityId
            );
            foreach (DataColumn c in table.Columns)
            {
                dataSourceElement.AppendChild(
                    newChild: CreateDataSourceField(
                        doc: dataSourceElement.OwnerDocument,
                        name: c.ColumnName,
                        index: c.Ordinal
                    )
                );
            }
            dataSourceElement.AppendChild(
                newChild: CreateDataSourceField(
                    doc: dataSourceElement.OwnerDocument,
                    name: "__Errors",
                    index: table.Columns.Count
                )
            );
        }
    }

    public static string DatabaseTableName(DataTable table)
    {
        IPersistenceService ps =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        TableMappingItem tableMapping =
            ps.SchemaProvider.RetrieveInstance(
                type: typeof(TableMappingItem),
                primaryKey: new ModelElementKey(id: (Guid)table.ExtendedProperties[key: "EntityId"])
            ) as TableMappingItem;
        if (tableMapping != null)
        {
            return tableMapping.MappedObjectName;
        }

        return null;
    }

    public static XmlElement AddDataSourceElement(
        XmlElement dataSourcesElement,
        string entity,
        string identifier,
        string lookupCacheKey,
        string dataStructureEntityId
    )
    {
        XmlElement dataSourceElement = dataSourcesElement.OwnerDocument.CreateElement(
            name: "DataSource"
        );
        dataSourcesElement.AppendChild(newChild: dataSourceElement);
        dataSourceElement.SetAttribute(name: "Entity", value: entity);
        if (!string.IsNullOrEmpty(value: dataStructureEntityId))
        {
            dataSourceElement.SetAttribute(
                name: "DataStructureEntityId",
                value: dataStructureEntityId
            );
        }
        dataSourceElement.SetAttribute(name: "Identifier", value: identifier);
        if (lookupCacheKey != null)
        {
            dataSourceElement.SetAttribute(name: "LookupCacheKey", value: lookupCacheKey);
        }
        return dataSourceElement;
    }

    internal static XmlElement WindowElement(XmlDocument doc)
    {
        return doc.FirstChild as XmlElement;
    }

    internal static XmlElement UIRootElement(XmlElement windowElement)
    {
        return windowElement.SelectSingleNode(xpath: "UIRoot") as XmlElement;
    }

    internal static XmlElement DataSourcesElement(XmlElement windowElement)
    {
        return windowElement.SelectSingleNode(xpath: "DataSources") as XmlElement;
    }

    internal static XmlElement ComponentBindingsElement(XmlElement windowElement)
    {
        return windowElement.SelectSingleNode(xpath: "ComponentBindings") as XmlElement;
    }

    public static XmlDocument GetXml(
        WorkQueueClass workQueueClass,
        DataSet dataset,
        string screenTitle,
        WorkQueueCustomScreen customScreen,
        Guid queueId
    )
    {
        var dataSources = new Hashtable();
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        IOrigamAuthorizationProvider authorizationProvider =
            SecurityManager.GetAuthorizationProvider();
        var parameterService = ServiceManager.Services.GetService<IParameterService>();
        DataSet workQueueCommands = core.DataService.Instance.LoadData(
            dataStructureId: new Guid(g: "1d33b667-ca76-4aaa-a47d-0e404ed6f8a6"),
            methodId: new Guid(g: "421aec03-1eec-43f9-b0bb-17cfc24510a0"),
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            parameters: new QueryParameterCollection
            {
                new QueryParameter(
                    _parameterName: "WorkQueueCommand_parWorkQueueId",
                    value: queueId
                ),
            }
        );
        List<DataRow> authorizedCommands = workQueueCommands
            .Tables[name: "WorkQueueCommand"]
            .Rows.Cast<DataRow>()
            .Where(predicate: dataRow =>
                !dataRow.IsNull(columnName: "Roles")
                && authorizationProvider.Authorize(
                    principal: SecurityManager.CurrentPrincipal,
                    context: (string)dataRow[columnName: "Roles"]
                )
            )
            .ToList();
        bool showCheckboxes = authorizedCommands.Any(predicate: command =>
        {
            if (
                (Guid)command[columnName: "refWorkQueueCommandTypeId"]
                != (Guid)
                    parameterService.GetParameterValue(
                        parameterName: "WorkQueueCommandType_WorkQueueClassCommand"
                    )
            )
            {
                return true;
            }
            WorkQueueWorkflowCommand commandDefinition = workQueueClass.GetCommand(
                name: (string)command[columnName: "Command"]
            );
            return commandDefinition.Mode == PanelActionMode.MultipleCheckboxes;
        });
        // base element
        XmlDocument xmlDocument = GetWindowBaseXml(
            name: screenTitle,
            menuId: Guid.Empty,
            autoRefreshInterval: settings.WorkQueueListRefreshPeriod,
            refreshOnFocus: false,
            autoSaveOnListRecordChange: false,
            requestSaveAfterUpdate: false
        );
        XmlElement windowElement = WindowElement(doc: xmlDocument);
        windowElement.SetAttribute(name: "SuppressSave", value: "true");
        windowElement.SetAttribute(name: "CacheOnClient", value: "false");
        XmlElement uiRootElement = UIRootElement(windowElement: windowElement);
        XmlElement bindingsElement = ComponentBindingsElement(windowElement: windowElement);
        XmlElement actionsElement = customScreen is null
            ? BuildGeneratedWorkQueueScreen(
                xml: xmlDocument,
                uiRoot: uiRootElement,
                bindings: bindingsElement,
                dataSources: dataSources,
                dataset: dataset,
                workQueueClass: workQueueClass,
                queueId: queueId,
                showCheckboxes: showCheckboxes
            )
            : BuildCustomWorkQueueScreen(
                xml: xmlDocument,
                windowElement: windowElement,
                uiRoot: uiRootElement,
                dataSources: dataSources,
                dataset: dataset,
                customScreen: customScreen,
                showCheckboxes: showCheckboxes
            );
        foreach (
            DataRow authorizedCommand in authorizedCommands.OrderBy(keySelector: x =>
                x[columnName: "SortOrder"]
            )
        )
        {
            Hashtable cmdParams = new Hashtable();
            string confirmationMessage = null;
            if (
                (Guid)authorizedCommand[columnName: "refWorkQueueCommandTypeId"]
                == (Guid)
                    parameterService.GetParameterValue(
                        parameterName: "WorkQueueCommandType_WorkQueueClassCommand"
                    )
            )
            {
                WorkQueueWorkflowCommand workQueueWorkflowCommand = workQueueClass.GetCommand(
                    name: (string)authorizedCommand[columnName: "Command"]
                );
                var config = new ActionConfiguration
                {
                    Type = PanelActionType.QueueAction,
                    Mode = workQueueWorkflowCommand.Mode,
                    Placement = workQueueWorkflowCommand.Placement,
                    ActionId = authorizedCommand[columnName: "Id"].ToString(),
                    GroupId = "",
                    Caption = (string)authorizedCommand[columnName: "Text"],
                    IconUrl = workQueueWorkflowCommand.ButtonIcon?.Name,
                    IsDefault = (bool)authorizedCommand[columnName: "IsDefault"],
                    ConfirmationMessage = null,
                    Parameters = cmdParams,
                };
                AsPanelActionButtonBuilder.Build(actionsElement: actionsElement, config: config);
            }
            else
            {
                string iconName = "";
                if (
                    (Guid)authorizedCommand[columnName: "refWorkQueueCommandTypeId"]
                    == (Guid)
                        parameterService.GetParameterValue(
                            parameterName: "WorkQueueCommandType_Remove"
                        )
                )
                {
                    iconName = "queue_remove.png";
                    confirmationMessage = ResourceUtils.GetString(
                        key: "WorkQueueRemoveConfirmationMessage"
                    );
                }
                else if (
                    (Guid)authorizedCommand[columnName: "refWorkQueueCommandTypeId"]
                    == (Guid)
                        parameterService.GetParameterValue(
                            parameterName: "WorkQueueCommandType_StateChange"
                        )
                )
                {
                    iconName = "queue_statechange.png";
                }
                var config = new ActionConfiguration
                {
                    Type = PanelActionType.QueueAction,
                    Mode = PanelActionMode.MultipleCheckboxes,
                    Placement = ActionButtonPlacement.Toolbar,
                    ActionId = authorizedCommand[columnName: "Id"].ToString(),
                    GroupId = "",
                    Caption = (string)authorizedCommand[columnName: "Text"],
                    IconUrl = iconName,
                    IsDefault = (bool)authorizedCommand[columnName: "IsDefault"],
                    Parameters = cmdParams,
                    ConfirmationMessage = confirmationMessage,
                    ShowAlways = true,
                };
                AsPanelActionButtonBuilder.Build(actionsElement: actionsElement, config: config);
            }
        }
        RenderDataSources(windowElement: windowElement, dataSources: dataSources);
        return xmlDocument;
    }

    private static XmlElement BuildCustomWorkQueueScreen(
        XmlDocument xml,
        XmlElement windowElement,
        XmlElement uiRoot,
        Hashtable dataSources,
        DataSet dataset,
        WorkQueueCustomScreen customScreen,
        bool showCheckboxes
    )
    {
        var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        var structure = persistenceService.SchemaProvider.RetrieveInstance<DataStructure>(
            instanceId: customScreen.Screen.DataStructure.Id
        );
        var xmlOutput = new XmlOutput { Document = xml };
        int controlCounter = 0;
        RenderUIElement(
            xmlOutput: xmlOutput,
            parentNode: uiRoot,
            item: FormTools.GetItemFromControlSet(controlSet: customScreen.Screen),
            dataset: dataset,
            dataSources: dataSources,
            controlCounter: ref controlCounter,
            isPreloaded: false,
            formId: customScreen.Screen.Id,
            menuWorkflowId: Guid.Empty,
            forceReadOnly: true,
            confirmSelectionChangeEntity: string.Empty,
            structure: structure
        );
        RenderDataSources(windowElement: windowElement, dataSources: dataSources);
        PostProcessScreenXml(
            doc: xml,
            dataset: dataset,
            windowElement: windowElement,
            confirmSelectionChangeEntity: string.Empty
        );
        if (showCheckboxes)
        {
            var rootGrid = (XmlElement)xml.SelectSingleNode(xpath: RootGridXPath);
            rootGrid?.SetAttribute(
                name: "ShowSelectionCheckboxes",
                value: XmlConvert.ToString(value: true)
            );
        }
        var actionsElement = (XmlElement)xml.SelectSingleNode(xpath: $"{RootGridXPath}/Actions");
        return actionsElement;
    }

    private static XmlElement BuildGeneratedWorkQueueScreen(
        XmlDocument xml,
        XmlElement uiRoot,
        XmlElement bindings,
        Hashtable dataSources,
        DataSet dataset,
        WorkQueueClass workQueueClass,
        Guid queueId,
        bool showCheckboxes
    )
    {
        var table = dataset.Tables[name: Entity_WorkQueueEntry];
        SplitPanelBuilder.Build(
            parentNode: uiRoot,
            orientation: SplitPanelOrientation.Horizontal,
            fixedSize: false
        );
        uiRoot.SetAttribute(name: "ModelInstanceId", value: "52DEFCEA-587C-47e0-97F5-3590B6AC492F");
        XmlElement children = xml.CreateElement(name: "UIChildren");
        uiRoot.AppendChild(newChild: children);
        XmlElement listElement = xml.CreateElement(name: "UIElement");
        children.AppendChild(newChild: listElement);
        var panelData = new UIElementRenderData
        {
            PanelTitle = ResourceUtils.GetString(key: "WorkQueueFromTitle"),
            IsGridVisible = true,
            DataMember = Entity_WorkQueueEntry,
        };
        AsPanelBuilder.Build(
            parentNode: listElement,
            renderData: panelData,
            modelId: queueId.ToString(),
            controlId: "queuePanel1",
            table: table,
            dataSources: dataSources,
            primaryKeyColumnName: table.PrimaryKey[0].ColumnName,
            showSelectionCheckboxes: showCheckboxes,
            formId: Guid.Empty,
            isIndependent: false
        );
        listElement.SetAttribute(name: "Id", value: "queuePanel1");
        listElement.SetAttribute(name: "ModelInstanceId", value: queueId.ToString());
        listElement.SetAttribute(name: "IsRootGrid", value: XmlConvert.ToString(value: true));
        listElement.SetAttribute(name: "IsRootEntity", value: XmlConvert.ToString(value: true));
        listElement.SetAttribute(name: "IsPreloaded", value: XmlConvert.ToString(value: true));
        XmlElement formRoot = AsPanelBuilder.FormRootElement(panelNode: listElement);
        XmlElement properties = AsPanelBuilder.PropertiesElement(panelNode: listElement);
        XmlElement actions = AsPanelBuilder.ActionsElement(panelNode: listElement);
        XmlElement propertyNames = xml.CreateElement(name: "PropertyNames");
        formRoot.AppendChild(newChild: propertyNames);
        int lastPos = 5;
        DataStructureColumn memoColumn = null;
        var mappedColumns = workQueueClass.ChildItemsByType<WorkQueueClassEntityMapping>(
            itemType: WorkQueueClassEntityMapping.CategoryConst
        );
        mappedColumns.Sort();
        var entity = workQueueClass.WorkQueueStructure.Entities[index: 0];
        foreach (var mapping in mappedColumns)
        {
            // don't add RecordCreated twice
            if (mapping.Name == "RecordCreated")
            {
                continue;
            }
            AddColumn(
                entity: entity,
                columnName: mapping.Name,
                memoColumn: ref memoColumn,
                lastPos: ref lastPos,
                propertiesElement: properties,
                propertyNamesElement: propertyNames,
                table: table,
                formatPattern: mapping.FormatPattern
            );
        }
        AddColumn(
            entity: entity,
            columnName: "IsLocked",
            memoColumn: ref memoColumn,
            lastPos: ref lastPos,
            propertiesElement: properties,
            propertyNamesElement: propertyNames,
            table: table,
            formatPattern: null
        );
        AddColumn(
            entity: entity,
            columnName: "refLockedByBusinessPartnerId",
            memoColumn: ref memoColumn,
            lastPos: ref lastPos,
            propertiesElement: properties,
            propertyNamesElement: propertyNames,
            table: table,
            formatPattern: null
        );
        AddColumn(
            entity: entity,
            columnName: "ErrorText",
            memoColumn: ref memoColumn,
            lastPos: ref lastPos,
            propertiesElement: properties,
            propertyNamesElement: propertyNames,
            table: table,
            formatPattern: null
        );
        AddColumn(
            entity: entity,
            columnName: "RecordCreated",
            memoColumn: ref memoColumn,
            lastPos: ref lastPos,
            propertiesElement: properties,
            propertyNamesElement: propertyNames,
            table: table,
            formatPattern: null
        );
        SetUserConfig(
            doc: xml,
            parentNode: listElement,
            defaultConfiguration: workQueueClass.DefaultPanelConfiguration,
            objectId: queueId,
            workflowId: Guid.Empty
        );
        if (memoColumn is not null)
        {
            BuildMemoPanel(
                xml: xml,
                children: children,
                bindings: bindings,
                dataSources: dataSources,
                table: table,
                queueId: queueId,
                memoColumn: memoColumn
            );
        }
        return actions;
    }

    private static void BuildMemoPanel(
        XmlDocument xml,
        XmlElement children,
        XmlElement bindings,
        Hashtable dataSources,
        DataTable table,
        Guid queueId,
        DataStructureColumn memoColumn
    )
    {
        XmlElement memoElement = xml.CreateElement(name: "UIElement");
        children.AppendChild(newChild: memoElement);
        var memoRenderData = new UIElementRenderData
        {
            DataMember = Entity_WorkQueueEntry,
            HideNavigationPanel = true,
            PanelTitle = memoColumn.Caption,
        };
        AsPanelBuilder.Build(
            parentNode: memoElement,
            renderData: memoRenderData,
            modelId: queueId.ToString(),
            controlId: "memoPanel1",
            table: table,
            dataSources: dataSources,
            primaryKeyColumnName: table.PrimaryKey[0].ColumnName,
            showSelectionCheckboxes: false,
            formId: Guid.Empty,
            isIndependent: false
        );
        memoElement.SetAttribute(name: "Id", value: "memoPanel");
        memoElement.SetAttribute(
            name: "ModelInstanceId",
            value: "65DF44F9-C050-4554-AD9A-896445314279"
        );
        memoElement.SetAttribute(name: "IsRootGrid", value: XmlConvert.ToString(value: false));
        memoElement.SetAttribute(name: "IsRootEntity", value: XmlConvert.ToString(value: true));
        memoElement.SetAttribute(name: "IsPreloaded", value: XmlConvert.ToString(value: true));
        memoElement.SetAttribute(name: "ParentId", value: queueId.ToString());
        memoElement.SetAttribute(name: "ParentEntityName", value: Entity_WorkQueueEntry);
        var filterExpressions = xml.CreateElement(name: "FilterExpressions");
        memoElement.AppendChild(newChild: filterExpressions);
        foreach (DataColumn dataColumn in table.PrimaryKey)
        {
            XmlElement filter = xml.CreateElement(name: "FilterExpression");
            filterExpressions.AppendChild(newChild: filter);
            filter.SetAttribute(name: "ParentProperty", value: dataColumn.ColumnName);
            filter.SetAttribute(name: "ItemProperty", value: dataColumn.ColumnName);
        }
        XmlElement memoFormRoot = AsPanelBuilder.FormRootElement(panelNode: memoElement);
        XmlElement memoProperties = AsPanelBuilder.PropertiesElement(panelNode: memoElement);
        XmlElement memoPropertyNames = xml.CreateElement(name: "PropertyNames");
        memoFormRoot.AppendChild(newChild: memoPropertyNames);
        var propertyElement = AsPanelPropertyBuilder.CreateProperty(
            propertiesElement: memoProperties,
            propertyNamesElement: memoPropertyNames,
            modelId: memoColumn.Id,
            bindingMember: memoColumn.Name,
            caption: string.Empty,
            gridCaption: null,
            table: table,
            readOnly: true,
            left: 0,
            top: 0,
            width: 100,
            height: 16,
            captionLength: 100,
            captionPosition: "None",
            gridColumnWidth: "500",
            style: null,
            tabIndex: null,
            fieldType: memoColumn.Field.FieldType
        );
        var buildDefinition = new TextBoxBuildDefinition(type: OrigamDataType.Memo)
        {
            Dock = "Fill",
            Multiline = true,
        };
        TextBoxBuilder.Build(propertyElement: propertyElement, buildDefinition: buildDefinition);
        // binding from the parent grid to the memo grid (same entity)
        CreateComponentBinding(
            doc: xml,
            bindingsElement: bindings,
            parentId: queueId.ToString(),
            parentProperty: "Id",
            parentEntity: Entity_WorkQueueEntry,
            childId: "65DF44F9-C050-4554-AD9A-896445314279",
            childProperty: "Id",
            childEntity: Entity_WorkQueueEntry,
            isChildParameter: false
        );
    }

    internal static void AddColumn(
        DataStructureEntity entity,
        string columnName,
        ref DataStructureColumn memoColumn,
        ref int lastPos,
        XmlElement propertiesElement,
        XmlElement propertyNamesElement,
        DataTable table,
        string formatPattern
    )
    {
        AddColumn(
            entity: entity,
            columnName: columnName,
            memoColumn: ref memoColumn,
            lastPos: ref lastPos,
            propertiesElement: propertiesElement,
            propertyNamesElement: propertyNamesElement,
            table: table,
            formatPattern: formatPattern,
            label: "",
            readOnly: true,
            lookupParameterName: null,
            lookupParameterValue: null
        );
    }

    private static void AddColumn(
        DataStructureEntity entity,
        string columnName,
        ref DataStructureColumn memoColumn,
        ref int lastPos,
        XmlElement propertiesElement,
        XmlElement propertyNamesElement,
        DataTable table,
        string formatPattern,
        string label,
        bool readOnly,
        string lookupParameterName,
        string lookupParameterValue
    )
    {
        UIStyle style = null;
        SchemaService schema =
            ServiceManager.Services.GetService(serviceType: typeof(SchemaService)) as SchemaService;
        StylesSchemaItemProvider styles =
            schema.GetProvider(type: typeof(StylesSchemaItemProvider)) as StylesSchemaItemProvider;
        int height = 0;
        DataStructureColumn col = entity.Column(name: columnName);
        if (col == null)
        {
            throw new ArgumentOutOfRangeException(
                paramName: "columnName",
                actualValue: columnName,
                message: "Column not found in the work queue data structure."
            );
        }
        if (col.Field.DataType != OrigamDataType.Blob && col.Name != "Id")
        {
            if (col.Field.Name == "s1")
            {
                style =
                    styles.GetChildByName(name: "bold", itemType: UIStyle.CategoryConst) as UIStyle;
            }
            if (col.Field.Name == "m1")
            {
                memoColumn = col;
            }
            else if (
                !(col.Field.DataType == OrigamDataType.UniqueIdentifier && col.FinalLookup == null)
            )
            {
                string caption =
                    label == "" ? (col.Caption == "" ? col.Field.Caption : col.Caption) : label;
                height = GENERIC_FIELD_HEIGHT * (col.Field.DataType == OrigamDataType.Memo ? 6 : 1);
                XmlElement propertyElement = AsPanelPropertyBuilder.CreateProperty(
                    propertiesElement: propertiesElement,
                    propertyNamesElement: propertyNamesElement,
                    modelId: col.Id,
                    bindingMember: col.Name,
                    caption: caption,
                    gridCaption: null,
                    table: table,
                    readOnly: readOnly,
                    left: 160,
                    top: lastPos,
                    width: 600,
                    height: height,
                    captionLength: 150,
                    captionPosition: "Left",
                    gridColumnWidth: "100",
                    style: style,
                    tabIndex: null,
                    fieldType: col.Field.FieldType
                );
                switch (col.Field.DataType)
                {
                    case OrigamDataType.Float:
                    case OrigamDataType.Integer:
                    case OrigamDataType.Currency:
                    case OrigamDataType.Memo:
                    case OrigamDataType.String:
                    {
                        var buildDefinition = new TextBoxBuildDefinition(type: col.Field.DataType)
                        {
                            Multiline = col.Field.DataType == OrigamDataType.Memo,
                        };
                        if (!string.IsNullOrEmpty(value: formatPattern))
                        {
                            buildDefinition.CustomNumberFormat = formatPattern;
                        }
                        TextBoxBuilder.Build(
                            propertyElement: propertyElement,
                            buildDefinition: buildDefinition
                        );
                        break;
                    }

                    case OrigamDataType.UniqueIdentifier:
                    {
                        ComboBoxBuilder.Build(
                            propertyElement: propertyElement,
                            lookupId: (Guid)col.FinalLookup.PrimaryKey[key: "Id"],
                            showUniqueValues: false,
                            bindingMember: col.Name,
                            table: table
                        );
                        if (lookupParameterName != null)
                        {
                            XmlDocument doc = propertyElement.OwnerDocument;
                            XmlElement comboParametersElement = doc.CreateElement(
                                name: "DropDownParameters"
                            );
                            propertyElement.AppendChild(newChild: comboParametersElement);
                            XmlElement comboParamElement = doc.CreateElement(
                                name: "ComboBoxParameterMapping"
                            );
                            comboParametersElement.AppendChild(newChild: comboParamElement);
                            comboParamElement.SetAttribute(
                                name: "ParameterName",
                                value: lookupParameterName
                            );
                            comboParamElement.SetAttribute(
                                name: "FieldName",
                                value: lookupParameterValue
                            );
                            propertyElement.SetAttribute(
                                name: "Cached",
                                value: XmlConvert.ToString(value: false)
                            );
                        }
                        break;
                    }

                    case OrigamDataType.Date:
                    {
                        if (string.IsNullOrEmpty(value: formatPattern))
                        {
                            formatPattern = "dd. MM. yyyy HH:mm:ss";
                        }
                        DateBoxBuilder.Build(
                            propertyElement: propertyElement,
                            format: "Custom",
                            customFormat: formatPattern
                        );
                        break;
                    }

                    case OrigamDataType.Boolean:
                    {
                        CheckBoxBuilder.Build(propertyElement: propertyElement, text: caption);
                        break;
                    }
                }
            }
        }
        lastPos += height + GENERIC_FIELD_VERTICAL_SPACE;
    }

    public static XmlDocument GetXml(SimpleModelData simpleModel, DataSet dataset, string name)
    {
        Hashtable dataSources = new Hashtable();
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
        DataTable table = dataset.Tables[name: "OrigamRecord"];
        SimpleModelData.OrigamEntityRow entityRow =
            simpleModel.OrigamEntity.Rows[index: 0] as SimpleModelData.OrigamEntityRow;
        // Window
        XmlDocument doc = GetWindowBaseXml(
            name: name,
            menuId: Guid.Empty,
            autoRefreshInterval: 0,
            refreshOnFocus: false,
            autoSaveOnListRecordChange: true,
            requestSaveAfterUpdate: false
        );
        XmlElement windowElement = WindowElement(doc: doc);
        XmlElement uiRootElement = UIRootElement(windowElement: windowElement);
        XmlElement bindingsElement = ComponentBindingsElement(windowElement: windowElement);
        windowElement.SetAttribute(name: "SuppressSave", value: "false");
        windowElement.SetAttribute(name: "CacheOnClient", value: "false");
        SplitPanelBuilder.Build(
            parentNode: uiRootElement,
            orientation: SplitPanelOrientation.Horizontal,
            fixedSize: false
        );
        uiRootElement.SetAttribute(
            name: "ModelInstanceId",
            value: "52DEFCEA-587C-47e0-97F5-3590B6AC492F"
        );
        XmlElement children = doc.CreateElement(name: "UIChildren");
        uiRootElement.AppendChild(newChild: children);
        XmlElement listElement = doc.CreateElement(name: "UIElement");
        children.AppendChild(newChild: listElement);
        // Panel
        UIElementRenderData renderData = new UIElementRenderData();
        renderData.PanelTitle = entityRow.Label;
        renderData.IsGridVisible = true;
        renderData.DataMember = "OrigamRecord";
        renderData.ShowNewButton = true;
        renderData.ShowDeleteButton = true;
        if (
            entityRow.IsCalendarEnabled
            && !entityRow.IsrefCalendarDateStartOrigamFieldIdNull()
            && !entityRow.IsrefCalendarResourceOrigamFieldIdNull()
            && !entityRow.IsrefCalendarNameOrigamFieldIdNull()
        )
        {
            renderData.IsCalendarSupported = true;
            renderData.CalendarDateFromMember = GetMember(
                entityRow: entityRow,
                memberId: entityRow.refCalendarDateStartOrigamFieldId
            );
            if (!entityRow.IsrefCalendarDateEndOrigamFieldIdNull())
            {
                renderData.CalendarDateToMember = GetMember(
                    entityRow: entityRow,
                    memberId: entityRow.refCalendarDateEndOrigamFieldId
                );
            }
            renderData.CalendarNameMember = GetMember(
                entityRow: entityRow,
                memberId: entityRow.refCalendarNameOrigamFieldId
            );
            if (!entityRow.IsrefCalendarResourceOrigamFieldIdNull())
            {
                renderData.CalendarResourceIdMember = GetMember(
                    entityRow: entityRow,
                    memberId: entityRow.refCalendarResourceOrigamFieldId
                );
            }
            if (!entityRow.IsrefCalendarDescriptionOrigamFieldIdNull())
            {
                renderData.CalendarDescriptionMember = GetMember(
                    entityRow: entityRow,
                    memberId: entityRow.refCalendarDescriptionOrigamFieldId
                );
            }
        }
        AsPanelBuilder.Build(
            parentNode: listElement,
            renderData: renderData,
            modelId: entityRow.Id.ToString(),
            controlId: "recordPanel1",
            table: table,
            dataSources: dataSources,
            primaryKeyColumnName: table.PrimaryKey[0].ColumnName,
            showSelectionCheckboxes: false,
            formId: Guid.Empty,
            isIndependent: false
        );
        listElement.SetAttribute(name: "Id", value: "recordPanel1");
        listElement.SetAttribute(name: "ModelInstanceId", value: entityRow.Id.ToString());
        listElement.SetAttribute(name: "IsRootGrid", value: XmlConvert.ToString(value: true));
        listElement.SetAttribute(name: "IsRootEntity", value: XmlConvert.ToString(value: true));
        listElement.SetAttribute(name: "IsPreloaded", value: XmlConvert.ToString(value: true));
        listElement.SetAttribute(
            name: "RequestDataAfterSelectionChange",
            value: XmlConvert.ToString(value: true)
        );
        listElement.SetAttribute(
            name: "ConfirmSelectionChange",
            value: XmlConvert.ToString(value: true)
        );
        XmlElement formRootElement = AsPanelBuilder.FormRootElement(panelNode: listElement);
        XmlElement propertiesElement = AsPanelBuilder.PropertiesElement(panelNode: listElement);
        XmlElement actionsElement = AsPanelBuilder.ActionsElement(panelNode: listElement);
        XmlElement configElement = doc.CreateElement(name: "Configuration");
        listElement.AppendChild(newChild: configElement);
        XmlElement propertyNamesElement = doc.CreateElement(name: "PropertyNames");
        formRootElement.AppendChild(newChild: propertyNamesElement);
        DataStructureColumn memoColumn = null;
        // Panel controls
        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        Guid dsId = (Guid)dataset.ExtendedProperties[key: "Id"];
        DataStructure ds =
            persistence.SchemaProvider.RetrieveInstance(
                type: typeof(DataStructure),
                primaryKey: new ModelElementKey(id: dsId)
            ) as DataStructure;
        DataStructureEntity entity = ds.Entities[index: 0] as DataStructureEntity;
        int lastPos = 5;
        if (entityRow.WorkflowCount > 0)
        {
            AddColumn(
                entity: entity,
                columnName: "refOrigamWorkflowId",
                memoColumn: ref memoColumn,
                lastPos: ref lastPos,
                propertiesElement: propertiesElement,
                propertyNamesElement: propertyNamesElement,
                table: table,
                formatPattern: null,
                label: dataset
                    .Tables[name: "OrigamRecord"]
                    .Columns[name: "refOrigamWorkflowId"]
                    .Caption,
                readOnly: false,
                lookupParameterName: "OrigamWorkflow_parOrigamEntityId",
                lookupParameterValue: "'" + entityRow.Id.ToString() + "'"
            );
            AddColumn(
                entity: entity,
                columnName: "refOrigamStateId",
                memoColumn: ref memoColumn,
                lastPos: ref lastPos,
                propertiesElement: propertiesElement,
                propertyNamesElement: propertyNamesElement,
                table: table,
                formatPattern: null,
                label: dataset
                    .Tables[name: "OrigamRecord"]
                    .Columns[name: "refOrigamStateId"]
                    .Caption,
                readOnly: false,
                lookupParameterName: "OrigamState_parOrigamWorkflowId",
                lookupParameterValue: "refOrigamWorkflowId"
            );
        }
        foreach (var column in entityRow.GetOrigamFieldRows())
        {
            AddColumn(
                entity: entity,
                columnName: column.MappedColumn,
                memoColumn: ref memoColumn,
                lastPos: ref lastPos,
                propertiesElement: propertiesElement,
                propertyNamesElement: propertyNamesElement,
                table: table,
                formatPattern: null,
                label: column.Label,
                readOnly: false,
                lookupParameterName: "OrigamRecord_parOrigamEntityId",
                lookupParameterValue: column.IsrefLookupOrigamEntityIdNull()
                    ? null
                    : "'" + column.refLookupOrigamEntityId.ToString() + "'"
            );
        }
        var validActions = new List<EntityUIAction>();
        UIActionTools.GetValidActions(
            formId: entityRow.Id,
            panelId: entityRow.Id,
            disableActionButtons: renderData.DisableActionButtons,
            entityId: table.ExtendedProperties.Contains(key: "EntityId")
                ? (Guid)table.ExtendedProperties[key: "EntityId"]
                : Guid.Empty,
            validActions: validActions
        );
        UserProfile profile = SecurityManager.CurrentUserProfile();
        EntityUIAction actionToRemove = null;
        foreach (EntityUIAction action in validActions)
        {
            if (action.Name == "Design" && !entityRow.RecordCreatedBy.Equals(g: profile.Id))
            {
                actionToRemove = action;
                break;
            }
        }
        if (actionToRemove != null)
        {
            validActions.Remove(item: actionToRemove);
        }
        Hashtable designButtonParams = new Hashtable();
        designButtonParams.Add(
            key: "OrigamEntity_parId",
            value: "'" + entityRow.Id.ToString() + "'"
        );
        IParameterService parameterService =
            ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
            as IParameterService;
        RenderActions(
            parameterService: parameterService,
            validActions: validActions,
            actionsElement: actionsElement,
            inputParameters: designButtonParams
        );
        SetUserConfig(
            doc: doc,
            parentNode: listElement,
            defaultConfiguration: null,
            objectId: entityRow.Id,
            workflowId: Guid.Empty
        );
        RenderDataSources(windowElement: windowElement, dataSources: dataSources);
        return doc;
    }

    private static string GetMember(SimpleModelData.OrigamEntityRow entityRow, Guid memberId)
    {
        foreach (var item in entityRow.GetOrigamFieldRows())
        {
            if (item.Id.Equals(g: memberId))
            {
                return item.MappedColumn;
            }
        }
        throw new ArgumentOutOfRangeException(
            paramName: "memberId",
            actualValue: memberId,
            message: "Member not found"
        );
    }

    public static XmlDocument GetXml(
        string dashboardViewConfig,
        string name,
        Guid menuId,
        XmlDocument dashboardViews
    )
    {
        XmlDocument configXml = new XmlDocument();
        configXml.LoadXml(xml: dashboardViewConfig);
        DashboardConfiguration config = DashboardConfiguration.Deserialize(doc: configXml);
        XmlDocument doc = GetWindowBaseXml(
            name: name,
            menuId: menuId,
            autoRefreshInterval: 0,
            refreshOnFocus: false,
            autoSaveOnListRecordChange: false,
            requestSaveAfterUpdate: false
        );
        XmlElement windowElement = WindowElement(doc: doc);
        XmlElement uiRootElement = UIRootElement(windowElement: windowElement);
        XmlElement bindingsElement = ComponentBindingsElement(windowElement: windowElement);
        XmlElement dataSourcesElement = DataSourcesElement(windowElement: windowElement);
        XmlElement views = doc.CreateElement(name: "DashboardViews");
        windowElement.AppendChild(newChild: views);
        if (dashboardViews != null)
        {
            views.InnerXml = dashboardViews.FirstChild.InnerXml;
        }
        GridLayoutPanelBuilder.Build(parentNode: uiRootElement);
        XmlElement children = doc.CreateElement(name: "UIChildren");
        uiRootElement.AppendChild(newChild: children);
        if (config.Items != null)
        {
            foreach (DashboardConfigurationItem item in config.Items)
            {
                DashboardConfigurationItemBuilder.Build(
                    doc: doc,
                    children: children,
                    menuId: menuId,
                    item: item,
                    dataSourcesElement: dataSourcesElement
                );
            }
            // component bindings (after all grids are configured)
            foreach (DashboardConfigurationItem item in config.Items)
            {
                if (item.Parameters != null)
                {
                    foreach (DashboardConfigurationItemParameter param in item.Parameters)
                    {
                        if (param.IsBound)
                        {
                            string parentId = param.BoundItemId.ToString();
                            string parentProperty = param.BoundItemProperty;
                            string parentEntity = EntityByGridInstanceId(
                                doc: doc,
                                instanceId: parentId
                            );
                            string childEntity = EntityByGridInstanceId(
                                doc: doc,
                                instanceId: item.Id.ToString()
                            );
                            CreateComponentBinding(
                                doc: doc,
                                bindingsElement: bindingsElement,
                                parentId: parentId,
                                parentProperty: parentProperty,
                                parentEntity: parentEntity,
                                childId: item.Id.ToString(),
                                childProperty: param.Name,
                                childEntity: childEntity,
                                isChildParameter: true
                            );
                        }
                    }
                }
            }
        }
        return doc;
    }

    public static XmlOutput GetXml(
        FormControlSet item,
        DataSet dataset,
        string name,
        bool isPreloaded,
        Guid menuId,
        bool forceReadOnly,
        string confirmSelectionChangeEntity,
        DataStructure structure = null
    )
    {
        int controlCounter = 0;
        IPersistenceService ps =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        WorkflowReferenceMenuItem wfmi =
            ps.SchemaProvider.RetrieveInstance(
                type: typeof(WorkflowReferenceMenuItem),
                primaryKey: new ModelElementKey(id: menuId)
            ) as WorkflowReferenceMenuItem;
        FormReferenceMenuItem frmi =
            ps.SchemaProvider.RetrieveInstance(
                type: typeof(FormReferenceMenuItem),
                primaryKey: new ModelElementKey(id: menuId)
            ) as FormReferenceMenuItem;
        Guid workflowId = (wfmi == null ? Guid.Empty : wfmi.WorkflowId);
        int autoRefreshInterval = 0;
        Hashtable dataSources = new Hashtable();
        if (frmi != null && frmi.AutoRefreshInterval != null)
        {
            IParameterService parameterService =
                ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
                as IParameterService;
            autoRefreshInterval = (int)
                parameterService.GetParameterValue(
                    id: frmi.AutoRefreshIntervalConstantId,
                    targetType: OrigamDataType.Integer
                );
        }
        // Window
        XmlDocument doc = GetWindowBaseXml(
            name: name,
            menuId: menuId,
            autoRefreshInterval: autoRefreshInterval,
            refreshOnFocus: (frmi != null) ? frmi.RefreshOnFocus : false,
            autoSaveOnListRecordChange: (frmi != null) ? frmi.AutoSaveOnListRecordChange : false,
            requestSaveAfterUpdate: (frmi != null) ? frmi.RequestSaveAfterUpdate : false
        );
        XmlElement windowElement = WindowElement(doc: doc);
        XmlElement uiRootElement = UIRootElement(windowElement: windowElement);
        if (forceReadOnly)
        {
            windowElement.SetAttribute(name: "SuppressSave", value: "true");
        }
        if (frmi?.ListDataStructure != null)
        {
            windowElement.SetAttribute(name: "UseSession", value: "true");
        }
        XmlOutput xmlOutput = new XmlOutput { Document = doc };
        RenderUIElement(
            xmlOutput: xmlOutput,
            parentNode: uiRootElement,
            item: FormTools.GetItemFromControlSet(controlSet: item),
            dataset: dataset,
            dataSources: dataSources,
            controlCounter: ref controlCounter,
            isPreloaded: isPreloaded,
            formId: item.Id,
            menuWorkflowId: workflowId,
            forceReadOnly: forceReadOnly,
            confirmSelectionChangeEntity: confirmSelectionChangeEntity,
            structure: structure
        );
        RenderDataSources(windowElement: windowElement, dataSources: dataSources);
        PostProcessScreenXml(
            doc: doc,
            dataset: dataset,
            windowElement: windowElement,
            confirmSelectionChangeEntity: confirmSelectionChangeEntity
        );
        return xmlOutput;
    }

    private static XmlElement FindComponentByInstanceId(XmlDocument doc, string instanceId)
    {
        XmlElement result = (XmlElement)
            doc.SelectSingleNode(xpath: "//*[@ModelInstanceId='" + instanceId + "']");
        if (result == null)
        {
            throw new ArgumentOutOfRangeException(
                paramName: "instanceId",
                actualValue: instanceId,
                message: "Component with specified model instance id not found."
            );
        }
        return result;
    }

    private static string DataMemberByGridInstanceId(XmlDocument doc, string instanceId)
    {
        XmlElement parentGrid = FindComponentByInstanceId(doc: doc, instanceId: instanceId);
        return parentGrid.GetAttribute(name: "DataMember");
    }

    private static string EntityByGridInstanceId(XmlDocument doc, string instanceId)
    {
        XmlElement parentGrid = FindComponentByInstanceId(doc: doc, instanceId: instanceId);
        return parentGrid.GetAttribute(name: "Entity");
    }

    private static void CreateComponentBinding(
        XmlDocument doc,
        XmlElement bindingsElement,
        string parentId,
        string parentProperty,
        string parentEntity,
        string childId,
        string childProperty,
        string childEntity,
        bool isChildParameter
    )
    {
        XmlElement binding = doc.CreateElement(name: "Binding");
        binding.SetAttribute(name: "ParentId", value: parentId);
        binding.SetAttribute(name: "ParentProperty", value: parentProperty);
        if (parentEntity != null)
        {
            binding.SetAttribute(name: "ParentEntity", value: parentEntity);
        }
        binding.SetAttribute(name: "ChildId", value: childId);
        binding.SetAttribute(name: "ChildProperty", value: childProperty);
        if (childEntity != null)
        {
            binding.SetAttribute(name: "ChildEntity", value: childEntity);
        }
        if (isChildParameter)
        {
            binding.SetAttribute(name: "ChildPropertyType", value: "Parameter");
        }
        else
        {
            binding.SetAttribute(name: "ChildPropertyType", value: "Field");
        }
        bindingsElement.AppendChild(newChild: binding);
    }

    private static XmlElement FindParentGridInParentEntity(
        DataSet dataset,
        string dataMember,
        XmlNodeList grids
    )
    {
        int lastDot = dataMember.LastIndexOf(value: ".");
        if (lastDot > 0)
        {
            string parentMember = dataMember.Substring(startIndex: 0, length: lastDot);
            // find parent grid
            foreach (XmlElement parentGrid in grids)
            {
                if (
                    parentGrid.GetAttribute(name: "Type") != "ReportButton"
                    && parentGrid.GetAttribute(name: "DataMember").ToLower()
                        == parentMember.ToLower()
                )
                {
                    return parentGrid;
                }
            }
        }
        return null;
    }

    private static bool RenderUIElement(
        XmlOutput xmlOutput,
        XmlElement parentNode,
        ISchemaItem item,
        DataSet dataset,
        Hashtable dataSources,
        ref int controlCounter,
        bool isPreloaded,
        Guid formId,
        Guid menuWorkflowId,
        bool forceReadOnly,
        string confirmSelectionChangeEntity,
        DataStructure structure = null,
        string parentTabIndex = null
    )
    {
        ControlSetItem control = item as ControlSetItem;
        IParameterService parameterService =
            ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
            as IParameterService;
        if (control.ControlItem.Name == "AsForm")
        {
            // for the Form we find its root control and we continue
            if (
                control
                    .ChildItemsByType<ControlSetItem>(itemType: ControlSetItem.CategoryConst)
                    .Count == 0
            )
            {
                return false;
            }
            item = control.ChildItemsByType<ControlSetItem>(itemType: ControlSetItem.CategoryConst)[
                index: 0
            ];
            control = item as ControlSetItem;
        }
        if (!RenderTools.ShouldRender(control: control))
        {
            return false;
        }
        forceReadOnly = FormTools.GetReadOnlyStatus(
            cntrlSet: control,
            currentReadOnlyStatus: forceReadOnly
        );
        UIElementRenderData renderData = UIElementRenderData.GetRenderData(
            control: control,
            forceReadOnly: forceReadOnly
        );
        if (renderData.Style != null)
        {
            parentNode.SetAttribute(name: "Style", value: renderData.Style.StyleDefinition());
        }

        DataTable table;

        if (renderData.IndependentDataSourceId == Guid.Empty)
        {
            table = dataset.Tables[
                name: FormTools.FindTableByDataMember(ds: dataset, member: renderData.DataMember)
            ];
        }
        else
        {
            IPersistenceService ps =
                ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
                as IPersistenceService;
            DataStructure ds = (DataStructure)
                ps.SchemaProvider.RetrieveInstance(
                    type: typeof(DataStructure),
                    primaryKey: new ModelElementKey(id: renderData.IndependentDataSourceId)
                );
            DatasetGenerator dg = new DatasetGenerator(userDefinedParameters: true);
            DataSet independentDataset = dg.CreateDataSet(ds: ds);
            table = independentDataset.Tables[index: 0];
        }
        parentNode.SetAttribute(name: "Id", value: control.Name + "_" + controlCounter.ToString());
        controlCounter++;
        parentNode.SetAttribute(name: "ModelInstanceId", value: control.Id.ToString());
        // for the first control in the fixed sized splitter we must set the height of the control
        // the second control in the splitter will have 100% size
        XmlElement parentControlElement = parentNode.ParentNode.ParentNode as XmlElement;
        if (
            renderData.TabIndex == 0
            && parentControlElement != null
            && (
                parentControlElement.GetAttribute(name: "Type") == "VBox"
                || parentControlElement.GetAttribute(name: "Type") == "CollapsiblePanel"
            )
        )
        {
            parentNode.SetAttribute(
                name: "Height",
                value: XmlConvert.ToString(value: renderData.Height)
            );
        }

        if (
            renderData.TabIndex == 0
            && parentControlElement != null
            && parentControlElement.GetAttribute(name: "Type") == "HBox"
        )
        {
            parentNode.SetAttribute(
                name: "Width",
                value: XmlConvert.ToString(value: renderData.Width)
            );
        }
        string tabIndex = string.IsNullOrEmpty(value: parentTabIndex)
            ? renderData.TabIndex.ToString()
            : $"{parentTabIndex}.{renderData.TabIndex}";
        bool isIndependent = renderData.IndependentDataSourceId != Guid.Empty;
        if (!control.ControlItem.IsComplexType)
        {
            var controlItem = GentControlItem(control: control);
            switch (controlItem.Name)
            {
                case "Panel":
                {
                    PanelBuilder.Build(parentNode: parentNode);
                    break;
                }

                case "AsReportPanel":
                {
                    ReportPanelBuilder.Build(
                        parentNode: parentNode,
                        renderData: renderData,
                        table: table,
                        control: control
                    );
                    break;
                }

                case "TabControl":
                {
                    TabControlBuilder.Build(parentNode: parentNode);
                    break;
                }

                case "TabPage":
                {
                    TabBuilder.Build(parentNode: parentNode, text: renderData.Text);
                    break;
                }

                case "CollapsibleContainer":
                {
                    CollapsibleContainerBuilder.Build(parentNode: parentNode);
                    break;
                }

                case "CollapsiblePanel":
                {
                    CollapsiblePanelBuilder.Build(parentNode: parentNode, renderData: renderData);
                    break;
                }

                case "GridLayoutPanel":
                {
                    GridLayoutPanelBuilder.Build(parentNode: parentNode);
                    break;
                }

                case "GridLayoutPanelItem":
                {
                    GridLayoutPanelItemBuilder.Build(
                        parentNode: parentNode,
                        renderData: renderData
                    );
                    break;
                }

                case "SplitPanel":
                {
                    SplitPanelBuilder.Build(
                        parentNode: parentNode,
                        orientation: (SplitPanelOrientation)renderData.Orientation,
                        fixedSize: renderData.FixedSize
                    );
                    break;
                }

                case "AsTree":
                {
                    TreeControlBuilder.Build(
                        parentNode: parentNode,
                        renderData: renderData,
                        table: table,
                        controlId: control.Id.ToString(),
                        dataSources: dataSources,
                        isIndependent: false
                    );
                    break;
                }

                case "AsTree2":
                {
                    TreeControlBuilder.Build2(
                        parentNode: parentNode,
                        formParameterName: renderData.FormParameterName,
                        treeId: renderData.TreeId
                    );
                    break;
                }

                case "ScreenLevelPlugin":
                {
                    ScreenLevelPluginBuilder.Build(
                        parentNode: parentNode,
                        text: renderData.Text,
                        dataSources: dataSources,
                        dataset: dataset,
                        dataStructure: structure,
                        dataMember: renderData.DataMember
                    );
                    break;
                }

                case "SectionLevelPlugin":
                {
                    SectionLevelPluginBuilder.Build(
                        parentNode: parentNode,
                        text: renderData.Text,
                        table: table,
                        dataStructure: structure,
                        isPreloaded: isPreloaded,
                        isIndependent: isIndependent,
                        dataSources: dataSources,
                        modelId: control.Id.ToString(),
                        dataMember: renderData.DataMember
                    );
                    break;
                }

                case "Label":
                {
                    FormLabelBuilder.Build(parentNode: parentNode, text: renderData.Text);
                    break;
                }

                default:
                {
                    parentNode.SetAttribute(
                        localName: "type",
                        namespaceURI: "http://www.w3.org/2001/XMLSchema-instance",
                        value: "UIElement"
                    );
                    parentNode.SetAttribute(name: "Type", value: "Box");
                    parentNode.SetAttribute(
                        name: "Title",
                        value: "UNKNOWN CONTROL:" + control.Name
                    );
                    break;
                }
            }
            AddDynamicProperties(parentNode: parentNode, renderData: renderData);
        }
        else // complex type = screen section
        {
            if (table == null)
            {
                if (string.IsNullOrWhiteSpace(value: renderData.DataMember))
                {
                    throw new NullReferenceException(
                        message: "DataMember not set for a screen section inside a screen. Cannot render a screen section. "
                            + control.Path
                    );
                }
                throw new Exception(
                    message: $"DataMember {renderData.DataMember} was not found in data source. Cannot render a screen section. {control.Path}"
                );
            }
            if (table.PrimaryKey.Length == 0)
            {
                throw new Exception(
                    message: "Panel's data source has no primary key. Cannot render panel. "
                        + control.Path
                );
            }
            // get list of valid actions and set the panel multi-select-checkbox column visibility
            var validActions = new List<EntityUIAction>();
            bool hasMultipleSelection = UIActionTools.GetValidActions(
                formId: formId,
                panelId: control.ControlItem.PanelControlSet.Id,
                disableActionButtons: renderData.DisableActionButtons,
                entityId: table.ExtendedProperties.Contains(key: "EntityId")
                    ? (Guid)table.ExtendedProperties[key: "EntityId"]
                    : Guid.Empty,
                validActions: validActions
            );
            AsPanelBuilder.Build(
                parentNode: parentNode,
                renderData: renderData,
                modelId: FormTools
                    .GetItemFromControlSet(controlSet: control.ControlItem.PanelControlSet)
                    .Id.ToString(),
                controlId: control.Id.ToString(),
                table: table,
                dataSources: dataSources,
                primaryKeyColumnName: table.PrimaryKey[0].ColumnName,
                showSelectionCheckboxes: hasMultipleSelection,
                formId: formId,
                isIndependent: isIndependent
            );
            XmlElement formRootElement = AsPanelBuilder.FormRootElement(panelNode: parentNode);
            XmlElement propertiesElement = AsPanelBuilder.PropertiesElement(panelNode: parentNode);
            XmlElement actionsElement = AsPanelBuilder.ActionsElement(panelNode: parentNode);
            RenderActions(
                parameterService: parameterService,
                validActions: validActions,
                actionsElement: actionsElement,
                inputParameters: new Hashtable()
            );
            // render controls (both directly placed edit controls and containers)
            RenderPanel(
                panel: control,
                xmlOutput: xmlOutput,
                table: table,
                parentElement: formRootElement,
                propertiesElement: propertiesElement,
                item: FormTools.GetItemFromControlSet(
                    controlSet: control.ControlItem.PanelControlSet
                ),
                processContainers: true,
                processEditControls: true,
                forceReadOnly: forceReadOnly,
                parentTabIndex: tabIndex
            );
        }
        // add config
        SetUserConfig(
            doc: xmlOutput.Document,
            parentNode: parentNode,
            defaultConfiguration: renderData.DefaultConfiguration,
            objectId: control.Id,
            workflowId: menuWorkflowId
        );
        var sortedChildren = new List<ControlSetItem>(
            collection: item.ChildItemsByType<ControlSetItem>(
                itemType: ControlSetItem.CategoryConst
            )
        );
        if (sortedChildren.Count > 0)
        {
            sortedChildren.Sort(comparer: new ControlSetItemComparer());
            XmlElement children = xmlOutput.Document.CreateElement(name: "UIChildren");
            parentNode.AppendChild(newChild: children);
            foreach (ControlSetItem child in sortedChildren)
            {
                XmlElement el = xmlOutput.Document.CreateElement(name: "UIElement");
                children.AppendChild(newChild: el);
                if (
                    !RenderUIElement(
                        xmlOutput: xmlOutput,
                        parentNode: el,
                        item: child,
                        dataset: dataset,
                        dataSources: dataSources,
                        controlCounter: ref controlCounter,
                        isPreloaded: isPreloaded,
                        formId: formId,
                        menuWorkflowId: menuWorkflowId,
                        forceReadOnly: forceReadOnly,
                        confirmSelectionChangeEntity: confirmSelectionChangeEntity,
                        structure: structure,
                        parentTabIndex: tabIndex
                    )
                )
                {
                    children.RemoveChild(oldChild: el);
                }
            }
        }
        // child grid filter expressions
        bool hasParentTables = false;
        if (table != null && renderData.DataMember != null)
        {
            hasParentTables =
                table.ParentRelations.Count > 0 & renderData.DataMember.IndexOf(value: ".") >= 0;
        }
        if (isIndependent)
        {
            parentNode.SetAttribute(name: "IsRootGrid", value: XmlConvert.ToString(value: true));
            parentNode.SetAttribute(name: "IsRootEntity", value: XmlConvert.ToString(value: true));
            parentNode.SetAttribute(name: "IsPreloaded", value: XmlConvert.ToString(value: false));
        }
        else if (control.ControlItem.Name == "AsReportPanel")
        {
            parentNode.SetAttribute(name: "IsRootGrid", value: XmlConvert.ToString(value: false));
            parentNode.SetAttribute(
                name: "IsRootEntity",
                value: XmlConvert.ToString(value: !hasParentTables)
            );
        }
        // grid or tree
        else if (hasParentTables)
        {
            DataRelation relation = table.ParentRelations[index: 0];
            parentNode.SetAttribute(name: "IsRootGrid", value: XmlConvert.ToString(value: false));
            parentNode.SetAttribute(name: "IsRootEntity", value: XmlConvert.ToString(value: false));
            if (renderData.HideNavigationPanel)
            {
                parentNode.SetAttribute(
                    name: "IsPreloaded",
                    value: XmlConvert.ToString(value: true)
                );
            }
            else
            {
                parentNode.SetAttribute(
                    name: "IsPreloaded",
                    value: XmlConvert.ToString(value: isPreloaded)
                );
            }
        }
        else if (control.ControlItem.Name == "SectionLevelPlugin")
        {
            if (renderData.AllowNavigation)
            {
                parentNode.SetAttribute(
                    name: "IsRootGrid",
                    value: XmlConvert.ToString(value: true)
                );
                parentNode.SetAttribute(
                    name: "IsRootEntity",
                    value: XmlConvert.ToString(value: true)
                );
                parentNode.SetAttribute(
                    name: "IsPreloaded",
                    value: XmlConvert.ToString(value: false)
                );
            }
            else
            {
                parentNode.SetAttribute(
                    name: "IsRootGrid",
                    value: XmlConvert.ToString(value: false)
                );
                parentNode.SetAttribute(
                    name: "IsRootEntity",
                    value: XmlConvert.ToString(value: true)
                );
                parentNode.SetAttribute(
                    name: "IsPreloaded",
                    value: XmlConvert.ToString(value: true)
                );
            }
        }
        // grid without navigation on a root level but without an implicit filter
        // if there is an implicit filter it must be an independent panel
        else if (
            (
                renderData.ImplicitFilter == null
                || (
                    renderData.ImplicitFilter == ""
                    && (
                        renderData.HideNavigationPanel
                        || (
                            renderData.ShowDeleteButton == false
                            && renderData.ShowNewButton == false
                        )
                    )
                )
            )
        )
        {
            parentNode.SetAttribute(name: "IsRootGrid", value: XmlConvert.ToString(value: false));
            parentNode.SetAttribute(name: "IsRootEntity", value: XmlConvert.ToString(value: true));
            parentNode.SetAttribute(name: "IsPreloaded", value: XmlConvert.ToString(value: true));
        }
        // root grid with navigation or SectionLevelPlugin
        else
        {
            parentNode.SetAttribute(name: "IsRootGrid", value: XmlConvert.ToString(value: true));
            parentNode.SetAttribute(name: "IsRootEntity", value: XmlConvert.ToString(value: true));
            parentNode.SetAttribute(name: "IsPreloaded", value: XmlConvert.ToString(value: true));
        }
        return true;
    }

    private static ISchemaItem GentControlItem(ControlSetItem control)
    {
        if (control.ControlItem.Ancestors.Count > 1)
        {
            throw new Exception(
                message: $"Could not find control for {control.ControlItem.Name} because it has more than one ancestor."
            );
        }
        return control.ControlItem.Ancestors.Count == 1
            ? control.ControlItem.Ancestors[index: 0].Ancestor
            : control.ControlItem;
    }

    private static void AddDynamicProperties(XmlElement parentNode, UIElementRenderData renderData)
    {
        foreach (var pair in renderData.DynamicProperties)
        {
            parentNode.SetAttribute(name: pair.Key, value: pair.Value);
        }
    }

    private static void RenderActions(
        IParameterService parameterService,
        List<EntityUIAction> validActions,
        XmlElement actionsElement,
        Hashtable inputParameters
    )
    {
        // render action buttons
        foreach (EntityUIAction action in validActions)
        {
            Hashtable parameters = new Hashtable(d: inputParameters);
            foreach (
                var mapping in action.ChildItemsByType<EntityUIActionParameterMapping>(
                    itemType: EntityUIActionParameterMapping.CategoryConst
                )
            )
            {
                parameters.Add(key: mapping.Name, value: mapping.Field);
            }
            string groupId = "";
            if (action.ParentItem is EntityDropdownAction)
            {
                groupId = action.ParentItemId.ToString();
            }
            bool shShift = false;
            bool shAlt = false;
            bool shCtrl = false;
            int shKey = 0;
            KeyboardShortcut sh = action.KeyboardShortcut;
            int terminator = 0;
            if (sh != null)
            {
                shShift = sh.IsShift;
                shAlt = sh.IsAlt;
                shCtrl = sh.IsControl;
                shKey = sh.KeyCode;
            }
            if ((action.ScannerTerminator != "") && (action.ScannerTerminator != null))
            {
                terminator = int.Parse(s: action.ScannerTerminator);
            }
            string confirmationMessage = null;
            if (action.ConfirmationMessage != null)
            {
                confirmationMessage = parameterService.GetString(
                    name: action.ConfirmationMessage.Name
                );
            }
            var builderConfiguration = new ActionConfiguration
            {
                Type = action.ActionType,
                Mode = action.Mode,
                Placement = action.Placement,
                ActionId = action.Id.ToString(),
                GroupId = groupId,
                Caption = action.Caption,
                IconUrl = action.ButtonIcon == null ? "" : action.ButtonIcon.Name,
                IsDefault = action.IsDefault,
                ConfirmationMessage = confirmationMessage,
                Parameters = parameters,
                Shortcut = new KeyShortcut
                {
                    IsShift = shShift,
                    IsControl = shCtrl,
                    IsAlt = shAlt,
                    KeyCode = shKey,
                },
                Scanner = new ScannerSettings
                {
                    Parameter = action.ScannerInputParameter,
                    TerminatorCharCode = terminator,
                },
            };
            AsPanelActionButtonBuilder.Build(
                actionsElement: actionsElement,
                config: builderConfiguration
            );
        }
    }

    private static void SetUserConfig(
        XmlDocument doc,
        XmlNode parentNode,
        string defaultConfiguration,
        Guid objectId,
        Guid workflowId
    )
    {
        UserProfile profile = SecurityManager.CurrentUserProfile();
        DataSet userConfig = OrigamPanelConfigDA.LoadConfigData(
            panelInstanceId: objectId,
            workflowId: workflowId,
            profileId: profile.Id
        );

        if (userConfig.Tables[name: "OrigamFormPanelConfig"].Rows.Count == 0)
        {
            if (defaultConfiguration != null && defaultConfiguration != "")
            {
                XmlDocumentFragment configElement = doc.CreateDocumentFragment();
                configElement.InnerXml = defaultConfiguration;
                parentNode.AppendChild(newChild: configElement);
            }
            else
            {
                XmlElement configElement = doc.CreateElement(name: "Configuration");
                parentNode.AppendChild(newChild: configElement);
            }
        }
        else
        {
            XmlDocumentFragment configElement = doc.CreateDocumentFragment();
            object data = userConfig.Tables[name: "OrigamFormPanelConfig"].Rows[index: 0][
                columnName: "SettingsData"
            ];
            if (data is String)
            {
                configElement.InnerXml = (string)data;
            }
            parentNode.AppendChild(newChild: configElement);
        }
    }

    private static void RenderPanel(
        ControlSetItem panel,
        XmlOutput xmlOutput,
        DataTable table,
        XmlElement parentElement,
        XmlElement propertiesElement,
        ISchemaItem item,
        bool processContainers,
        bool processEditControls,
        bool forceReadOnly,
        string parentTabIndex = null
    )
    {
        if (string.IsNullOrWhiteSpace(value: parentTabIndex))
        {
            int itemTabIndex =
                item.ChildItemsByType<PropertyValueItem>(itemType: PropertyValueItem.CategoryConst)
                    .FirstOrDefault(predicate: prop => prop.ControlPropertyItem.Name == "TabIndex")
                    ?.IntValue
                ?? -1;
            parentTabIndex = itemTabIndex.ToString();
        }
        XmlElement childrenElement = xmlOutput.Document.CreateElement(name: "Children");
        parentElement.AppendChild(newChild: childrenElement);
        XmlElement propertyNamesElement = xmlOutput.Document.CreateElement(name: "PropertyNames");
        parentElement.AppendChild(newChild: propertyNamesElement);
        XmlElement formExclusiveControlsElement = xmlOutput.Document.CreateElement(
            name: "FormExclusiveControls"
        );
        parentElement.AppendChild(newChild: formExclusiveControlsElement);
        // other properties
        var childItems = item.ChildItemsByType<ControlSetItem>(
                itemType: ControlSetItem.CategoryConst
            )
            .ToList();
        childItems.Sort(comparer: new ControlSetItemComparer());
        foreach (ControlSetItem csi in childItems)
        {
            if (RenderTools.ShouldRender(control: csi))
            {
                string caption = "";
                string gridCaption = "";
                string bindingMember = "";
                string tabIndex = "0";
                Guid lookupId = Guid.Empty;
                bool readOnly = forceReadOnly;
                if (!forceReadOnly)
                {
                    FormTools.GetReadOnlyStatus(cntrlSet: csi, currentReadOnlyStatus: false);
                }
                string text = "";
                bool showUniqueValues = false;
                int top = 0;
                int left = 0;
                int width = 0;
                int height = 0;
                int captionLength = 100;
                string captionPosition = "Left";
                string dock = "None";
                bool multiline = false;
                bool isPassword = false;
                string gridColumnWidth = null;
                bool isRichText = false;
                bool allowTab = false;
                bool hideOnForm = false;
                Guid styleId = Guid.Empty;
                UIStyle style = null;
                string format = null;
                string customFormat = null;
                string sourceType = null;
                int columnWidth = 100;
                Guid dataConstantId = Guid.Empty;
                string controlMember = "";
                string customNumericFormat = "";
                foreach (
                    var property in csi.ChildItemsByType<PropertyValueItem>(
                        itemType: PropertyValueItem.CategoryConst
                    )
                )
                {
                    string stringValue = property.Value;
                    if (
                        stringValue != null
                        && DatasetGenerator.IsCaptionExpression(caption: stringValue)
                    )
                    {
                        stringValue = DatasetGenerator.EvaluateCaptionExpression(
                            caption: stringValue
                        );
                    }
                    switch (property.ControlPropertyItem.Name)
                    {
                        case "TabIndex":
                        {
                            tabIndex = property.IntValue.ToString();
                            break;
                        }

                        case "Text":
                        {
                            text = stringValue;
                            break;
                        }

                        case "Caption":
                        {
                            caption = stringValue;
                            break;
                        }

                        case "GridColumnCaption":
                        {
                            gridCaption = stringValue;
                            break;
                        }

                        case "LookupId":
                        {
                            lookupId = property.GuidValue;
                            break;
                        }

                        case "ReadOnly":
                        {
                            readOnly = (readOnly ? true : property.BoolValue);
                            break;
                        }

                        case "ShowUniqueValues":
                        {
                            showUniqueValues = property.BoolValue;
                            break;
                        }

                        case "Top":
                        {
                            top = property.IntValue;
                            break;
                        }

                        case "Left":
                        {
                            left = property.IntValue;
                            break;
                        }

                        case "Width":
                        {
                            width = property.IntValue;
                            break;
                        }

                        case "Height":
                        {
                            height = property.IntValue;
                            break;
                        }

                        case "CaptionLength":
                        {
                            captionLength = property.IntValue;
                            break;
                        }

                        case "CaptionPosition":
                        {
                            captionPosition = ((CaptionPosition)property.IntValue).ToString();
                            break;
                        }

                        case "SourceType":
                        {
                            sourceType = ((ImageBoxSourceType)property.IntValue).ToString();
                            break;
                        }

                        case "Dock":
                        {
                            dock = ToWinFormsDockStyle(intVal: property.IntValue);
                            break;
                        }

                        case "Multiline":
                        {
                            multiline = property.BoolValue;
                            break;
                        }

                        case "IsPassword":
                        {
                            isPassword = property.BoolValue;
                            break;
                        }

                        case "GridColumnWidth":
                        {
                            gridColumnWidth = property.IntValue.ToString();
                            break;
                        }

                        case "IsRichText":
                        {
                            isRichText = property.BoolValue;
                            break;
                        }

                        case "AllowTab":
                        {
                            allowTab = property.BoolValue;
                            break;
                        }

                        case "HideOnForm":
                        {
                            hideOnForm = property.BoolValue;
                            break;
                        }

                        case "StyleId":
                        {
                            styleId = property.GuidValue;
                            break;
                        }

                        case "Format":
                        {
                            if (property.Value == null)
                            {
                                format = "Long";
                            }
                            else
                            {
                                XmlDocument formatDoc = new XmlDocument();
                                formatDoc.LoadXml(xml: property.Value);
                                format = formatDoc
                                    .SelectSingleNode(xpath: "DateTimePickerFormat")
                                    .InnerText;
                            }
                            break;
                        }

                        case "CustomFormat":
                        {
                            customFormat = stringValue;
                            break;
                        }

                        case "ColumnWidth":
                        {
                            columnWidth = property.IntValue;
                            break;
                        }

                        case "DataConstantId":
                        {
                            dataConstantId = property.GuidValue;
                            break;
                        }

                        case "ControlMember":
                        {
                            controlMember = property.Value;
                            break;
                        }

                        case "CustomNumericFormat":
                        {
                            customNumericFormat = property.Value;
                            break;
                        }
                    }
                }
                if (!styleId.Equals(g: Guid.Empty))
                {
                    IPersistenceService persistence =
                        ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
                        as IPersistenceService;
                    style =
                        persistence.SchemaProvider.RetrieveInstance(
                            type: typeof(UIStyle),
                            primaryKey: new ModelElementKey(id: styleId)
                        ) as UIStyle;
                }
                ControlSetItem parentPanel = csi.ParentItem as ControlSetItem;
                if (parentPanel != null && parentPanel.ControlItem.Name == "AsPanel")
                {
                    PropertyValueItem hideProperty =
                        parentPanel.GetChildByName(
                            name: "HideNavigationPanel",
                            itemType: PropertyValueItem.CategoryConst
                        ) as PropertyValueItem;
                    if (hideProperty != null)
                    {
                        if (!hideProperty.BoolValue)
                        {
                            top -= 20;
                        }
                    }
                }
                foreach (
                    var bindItem in csi.ChildItemsByType<PropertyBindingInfo>(
                        itemType: PropertyBindingInfo.CategoryConst
                    )
                )
                {
                    bindingMember = bindItem.Value;
                }
                var fieldType =
                    csi.FirstParentOfType<PanelControlSet>()
                        ?.DataEntity?.ChildItems?.OfType<IDataEntityColumn>()
                        ?.FirstOrDefault(predicate: child => child.Name == bindingMember)
                        ?.FieldType ?? "";

                if (int.Parse(s: tabIndex) >= 0)
                {
                    tabIndex = parentTabIndex + "." + tabIndex;
                }
                if (csi.ControlItem.Name == "Label" && processContainers)
                {
                    PanelLabelBuilder.Build(
                        childrenElement: childrenElement,
                        text: text,
                        top: top,
                        left: left,
                        height: height,
                        width: width
                    );
                }
                else if (csi.ControlItem.Name == "RadioButton" && processEditControls)
                {
                    if (!table.Columns.Contains(name: bindingMember))
                    {
                        throw new Exception(
                            message: "Field '"
                                + bindingMember
                                + "' not found in a data structure for the form '"
                                + panel.RootItem.Path
                                + "'"
                        );
                    }

                    XmlElement controlElement = AsPanelPropertyBuilder.CreateProperty(
                        category: "Control",
                        propertiesElement: formExclusiveControlsElement,
                        propertyNamesElement: null,
                        modelId: csi.Id,
                        bindingMember: bindingMember,
                        caption: caption,
                        gridCaption: gridCaption,
                        table: table,
                        readOnly: readOnly,
                        left: left,
                        top: top,
                        width: width,
                        height: height,
                        captionLength: captionLength,
                        captionPosition: captionPosition,
                        gridColumnWidth: gridColumnWidth == "0" ? "" : gridColumnWidth,
                        style: style,
                        tabIndex: tabIndex,
                        fieldType: fieldType
                    );
                    IParameterService parameterService =
                        ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
                        as IParameterService;
                    string value = (string)
                        parameterService.GetParameterValue(
                            id: dataConstantId,
                            targetType: OrigamDataType.String
                        );
                    RadioButtonBuilder.Build(
                        propertyElement: controlElement,
                        text: text,
                        value: value
                    );
                }
                else if (bindingMember == "" && processContainers) // visual element
                {
                    XmlElement formElement = xmlOutput.Document.CreateElement(name: "FormElement");
                    childrenElement.AppendChild(newChild: formElement);
                    formElement.SetAttribute(name: "Type", value: "FormSection");
                    formElement.SetAttribute(name: "Title", value: text);
                    formElement.SetAttribute(name: "TabIndex", value: tabIndex);
                    formElement.SetAttribute(name: "X", value: XmlConvert.ToString(value: left));
                    formElement.SetAttribute(name: "Y", value: XmlConvert.ToString(value: top));
                    formElement.SetAttribute(
                        name: "Width",
                        value: XmlConvert.ToString(value: width)
                    );
                    formElement.SetAttribute(
                        name: "Height",
                        value: XmlConvert.ToString(value: height)
                    );
                    RenderPanel(
                        panel: panel,
                        xmlOutput: xmlOutput,
                        table: table,
                        parentElement: formElement,
                        propertiesElement: propertiesElement,
                        item: csi,
                        processContainers: true,
                        processEditControls: true,
                        forceReadOnly: readOnly,
                        parentTabIndex: tabIndex
                    );
                }
                else if (bindingMember != "" & processEditControls) // property (entry field)
                {
                    if (!table.Columns.Contains(name: bindingMember))
                    {
                        throw new Exception(
                            message: "Field '"
                                + table.TableName
                                + "."
                                + bindingMember
                                + "' not found in a data structure for the form '"
                                + panel.RootItem.Path
                                + "'"
                        );
                    }
                    XmlElement propertyElement = AsPanelPropertyBuilder.CreateProperty(
                        propertiesElement: propertiesElement,
                        propertyNamesElement: hideOnForm ? null : propertyNamesElement,
                        modelId: csi.Id,
                        bindingMember: bindingMember,
                        caption: caption,
                        gridCaption: gridCaption,
                        table: table,
                        readOnly: readOnly,
                        left: left,
                        top: top,
                        width: width,
                        height: height,
                        captionLength: captionLength,
                        captionPosition: captionPosition,
                        gridColumnWidth: gridColumnWidth == "0" ? "" : gridColumnWidth,
                        style: style,
                        tabIndex: tabIndex,
                        fieldType: fieldType
                    );
                    switch (csi.ControlItem.Name)
                    {
                        case "ColorPicker":
                        {
                            var bindingColumn = table.Columns[name: bindingMember];
                            if (bindingColumn.DataType != typeof(int))
                            {
                                throw new Exception(
                                    message: "Field '"
                                        + table.TableName
                                        + "."
                                        + bindingMember
                                        + "' must be of type int because it si bound to a color editor"
                                );
                            }
                            ColorPickerBuilder.Build(propertyElement: propertyElement);
                            break;
                        }

                        case "BlobControl":
                        {
                            BlobControlBuilder.Build(
                                propertyElement: propertyElement,
                                control: csi
                            );
                            break;
                        }

                        case "ImageBox":
                        {
                            ImageBoxBuilder.Build(
                                propertyElement: propertyElement,
                                sourceType: sourceType
                            );
                            break;
                        }

                        case "AsTextBox":
                        {
                            TextBoxBuildDefinition buildDefinition = new TextBoxBuildDefinition(
                                type: (OrigamDataType)
                                    table.Columns[name: bindingMember].ExtendedProperties[
                                        key: "OrigamDataType"
                                    ]
                            );
                            buildDefinition.Dock = dock;
                            buildDefinition.Multiline = multiline;
                            buildDefinition.IsPassword = isPassword;
                            buildDefinition.IsRichText = isRichText;
                            buildDefinition.AllowTab = allowTab;
                            buildDefinition.MaxLength = table
                                .Columns[name: bindingMember]
                                .MaxLength;
                            buildDefinition.CustomNumberFormat = customNumericFormat;
                            TextBoxBuilder.Build(
                                propertyElement: propertyElement,
                                buildDefinition: buildDefinition
                            );
                            break;
                        }

                        case "AsDateBox":
                        {
                            DateBoxBuilder.Build(
                                propertyElement: propertyElement,
                                format: format,
                                customFormat: customFormat
                            );
                            break;
                        }
                        case "AsCheckBox":
                        {
                            CheckBoxBuilder.Build(propertyElement: propertyElement, text: text);
                            break;
                        }
                        case "AsCombo":
                        case "TagInput":
                        case "Checklist":
                        {
                            if (csi.ControlItem.Name == "AsCombo")
                            {
                                ComboBoxBuilder.Build(
                                    propertyElement: propertyElement,
                                    lookupId: lookupId,
                                    showUniqueValues: showUniqueValues,
                                    bindingMember: bindingMember,
                                    table: table
                                );
                            }
                            else if (csi.ControlItem.Name == "TagInput")
                            {
                                TagInputBuilder.BuildTagInput(
                                    propertyElement: propertyElement,
                                    lookupId: lookupId,
                                    bindingMember: bindingMember,
                                    table: table
                                );
                            }
                            else
                            {
                                TagInputBuilder.BuildChecklist(
                                    propertyElement: propertyElement,
                                    lookupId: lookupId,
                                    bindingMember: bindingMember,
                                    columnWidth: columnWidth,
                                    table: table
                                );
                            }
                            // set parameters
                            XmlElement comboParametersElement = xmlOutput.Document.CreateElement(
                                name: "DropDownParameters"
                            );
                            propertyElement.AppendChild(newChild: comboParametersElement);
                            foreach (
                                var mapping in csi.ChildItemsByType<ColumnParameterMapping>(
                                    itemType: ColumnParameterMapping.CategoryConst
                                )
                            )
                            {
                                XmlElement comboParamElement = xmlOutput.Document.CreateElement(
                                    name: "ComboBoxParameterMapping"
                                );
                                comboParametersElement.AppendChild(newChild: comboParamElement);
                                comboParamElement.SetAttribute(
                                    name: "ParameterName",
                                    value: mapping.Name
                                );
                                comboParamElement.SetAttribute(
                                    name: "FieldName",
                                    value: mapping.ColumnName
                                );
                                propertyElement.SetAttribute(
                                    name: "Cached",
                                    value: XmlConvert.ToString(value: false)
                                );
                            }
                            break;
                        }
                        case "MultiColumnAdapterFieldWrapper":
                        {
                            MultiColumnAdapterFieldWrapperBuilder.Build(
                                propertyElement: propertyElement,
                                control: csi,
                                controlMember: controlMember
                            );
                            RenderPanel(
                                panel: panel,
                                xmlOutput: xmlOutput,
                                table: table,
                                parentElement: propertyElement,
                                propertiesElement: propertyElement,
                                item: csi,
                                processContainers: false,
                                processEditControls: true,
                                forceReadOnly: readOnly
                            );
                            XmlNode propertyNames = propertyElement.SelectSingleNode(
                                xpath: "PropertyNames"
                            );
                            if (propertyNames == null)
                            {
                                throw new Exception(
                                    message: "There are no widgets defined in MultiColumnAdapterFieldWrapper "
                                        + csi.Path
                                );
                            }
                            propertyElement.RemoveChild(oldChild: propertyNames);
                            break;
                        }

                        default: // fallback: TextBox
                        {
                            TextBoxBuilder.Build(
                                propertyElement: propertyElement,
                                buildDefinition: new TextBoxBuildDefinition(
                                    type: (OrigamDataType)
                                        table.Columns[name: bindingMember].ExtendedProperties[
                                            key: "OrigamDataType"
                                        ]
                                )
                            );
                            break;
                        }
                    }
                    // The Id column was created earlier in AsPanelBuilder.cs
                    // and is meant to be invisible.
                    // If the Id column was added to the model in the Architect
                    // and thus should be the invisible, the original invisible
                    // Id column is removed. Duplicated Id column would create
                    // problems in the client
                    XmlElement zeroColumn = propertiesElement.FirstChild as XmlElement;
                    if (
                        propertyElement.GetAttribute(name: "Id")
                        == zeroColumn.GetAttribute(name: "Id")
                    )
                    {
                        propertiesElement.RemoveChild(oldChild: zeroColumn);
                    }
                    if (csi.MultiColumnAdapterFieldCondition != Guid.Empty)
                    {
                        IParameterService parameterService =
                            ServiceManager.Services.GetService(
                                serviceType: typeof(IParameterService)
                            ) as IParameterService;
                        propertyElement.SetAttribute(
                            name: "ControlPropertyValue",
                            value: parameterService.GetParameterValue(
                                id: csi.MultiColumnAdapterFieldCondition,
                                targetType: OrigamDataType.String
                            ) as string
                        );
                    }
                    if (csi.RequestSaveAfterChange)
                    {
                        propertyElement.SetAttribute(name: "RequestSaveAfterChange", value: "true");
                    }
                }
                if (lookupId != Guid.Empty)
                {
                    xmlOutput.ContainedLookups.Add(item: lookupId);
                }
            }
        }
        if (childrenElement.InnerXml == "")
        {
            parentElement.RemoveChild(oldChild: childrenElement);
        }
        if (propertyNamesElement.InnerXml == "")
        {
            parentElement.RemoveChild(oldChild: propertyNamesElement);
        }
        if (formExclusiveControlsElement.InnerXml == "")
        {
            parentElement.RemoveChild(oldChild: formExclusiveControlsElement);
        }
    }

    /// <summary>
    /// https://docs.microsoft.com/cs-cz/dotnet/api/system.windows.forms.dockstyle?view=netframework-4.7.2
    /// </summary>
    /// <param name="intVal"></param>
    /// <returns></returns>
    private static string ToWinFormsDockStyle(int intVal)
    {
        switch (intVal)
        {
            case 2:
            {
                return "Bottom";
            }
            case 5:
            {
                return "Fill";
            }
            case 3:
            {
                return "Left";
            }
            case 0:
            {
                return "None";
            }
            case 4:
            {
                return "Right";
            }
            case 1:
            {
                return "Top";
            }
            default:
            {
                throw new Exception(
                    message: "Cannot convert value " + intVal + " to System.Windows.Forms string"
                );
            }
        }
    }

    internal static string AddDataSource(
        Hashtable dataSources,
        DataTable table,
        string controlId,
        bool isIndependent
    )
    {
        string entityName = table.TableName;
        if (isIndependent)
        {
            entityName = controlId + "_" + entityName;
        }
        dataSources[key: entityName] = table;
        return entityName;
    }

    private static void PostProcessScreenXml(
        XmlDocument doc,
        DataSet dataset,
        XmlElement windowElement,
        string confirmSelectionChangeEntity
    )
    {
        // check if there is no root grid
        XmlNodeList grids = doc.SelectNodes(xpath: RootGridXPath);
        if (grids.Count == 0)
        {
            grids = doc.SelectNodes(
                xpath: "//*[(@Type='Grid' or @Type='TreePanel' or @Type='ReportButton') and @IsRootEntity = 'true']"
            );
            if (grids.Count > 0)
            {
                (grids[i: 0] as XmlElement).SetAttribute(name: "IsRootGrid", value: "true");
            }
        }
        grids = doc.SelectNodes(
            xpath: "//*[@Type='Grid' or @Type='TreePanel' or @Type='ReportButton' or @Type='SectionLevelPlugin']"
        );
        foreach (XmlElement g in grids)
        {
            if (
                g.GetAttribute(name: "IsRootGrid") == "false"
                && g.GetAttribute(name: "IsRootEntity") == "false"
            )
            {
                // if this panel has no navigation and displays form (detail) view, so we think it displays
                // only a single record so we try to find a panel on the same entity that has navigation
                // and connect them 1=1.
                if (
                    (
                        g.GetAttribute(name: "IsHeadless") == "true"
                        || g.GetAttribute(name: "DisableActionButtons") == "true"
                    )
                    && (
                        g.GetAttribute(name: "DefaultPanelView")
                            == XmlConvert.ToString(value: (int)OrigamPanelViewMode.Form)
                        || g.GetAttribute(name: "DefaultPanelView")
                            == XmlConvert.ToString(value: (int)OrigamPanelViewMode.Map)
                    )
                )
                {
                    bool found = false;
                    // try finding parent grid in the same entity - with head
                    foreach (XmlElement parentGrid in grids)
                    {
                        if (
                            (
                                parentGrid.GetAttribute(name: "Entity")
                                == g.GetAttribute(name: "Entity")
                            )
                            && (
                                (parentGrid.GetAttribute(name: "Type") == "TreePanel")
                                || (
                                    (parentGrid.GetAttribute(name: "IsHeadless") == "false")
                                    && (
                                        parentGrid.GetAttribute(name: "DisableActionButtons")
                                        == "false"
                                    )
                                )
                            )
                        )
                        {
                            g.SetAttribute(
                                name: "ParentId",
                                value: parentGrid.GetAttribute(name: "ModelInstanceId")
                            );
                            g.SetAttribute(
                                name: "ParentEntityName",
                                value: parentGrid.GetAttribute(name: "Entity")
                            );
                            g.SetAttribute(
                                name: "IsPreloaded",
                                value: XmlConvert.ToString(value: true)
                            );
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        // there is no grid in the same entity, we find one in the parent entity
                        XmlElement parentGrid = FindParentGridInParentEntity(
                            dataset: dataset,
                            dataMember: g.GetAttribute(name: "DataMember"),
                            grids: grids
                        );
                        if (parentGrid != null)
                        {
                            g.SetAttribute(
                                name: "ParentId",
                                value: parentGrid.GetAttribute(name: "ModelInstanceId")
                            );
                            g.SetAttribute(
                                name: "ParentEntityName",
                                value: parentGrid.GetAttribute(name: "Entity")
                            );
                        }
                    }
                }
                else
                {
                    XmlElement parentGrid = FindParentGridInParentEntity(
                        dataset: dataset,
                        dataMember: g.GetAttribute(name: "DataMember"),
                        grids: grids
                    );
                    if (parentGrid != null)
                    {
                        g.SetAttribute(
                            name: "ParentId",
                            value: parentGrid.GetAttribute(name: "ModelInstanceId")
                        );
                        g.SetAttribute(
                            name: "ParentEntityName",
                            value: parentGrid.GetAttribute(name: "Entity")
                        );
                    }
                }
            }
            else if (
                g.GetAttribute(name: "IsRootGrid") == "false"
                & g.GetAttribute(name: "IsRootEntity") == "true"
            )
            {
                // we lookup a root grid
                foreach (XmlElement parentGrid in grids)
                {
                    if (
                        parentGrid.GetAttribute(name: "IsRootGrid") == "true"
                        && parentGrid.GetAttribute(name: "Entity") == g.GetAttribute(name: "Entity")
                    )
                    {
                        g.SetAttribute(
                            name: "ParentId",
                            value: parentGrid.GetAttribute(name: "ModelInstanceId")
                        );
                        g.SetAttribute(
                            name: "ParentEntityName",
                            value: parentGrid.GetAttribute(name: "Entity")
                        );
                        break;
                    }
                }
            }
            if (
                g.GetAttribute(name: "IsRootGrid") == "false"
                && (
                    g.GetAttribute(name: "ParentId") == null
                    || g.GetAttribute(name: "ParentId") == ""
                )
            )
            {
                g.SetAttribute(name: "IsRootGrid", value: "true");
            }
        }
        XmlElement bindingsElement = ComponentBindingsElement(windowElement: windowElement);
        // set filters between related grids & lazy loading parameters
        foreach (XmlElement g in grids)
        {
            // set lazy loading parameters
            if (
                g.GetAttribute(name: "Entity") == confirmSelectionChangeEntity
                && g.GetAttribute(name: "IsRootGrid") == "true"
            )
            {
                g.SetAttribute(
                    name: "RequestDataAfterSelectionChange",
                    value: XmlConvert.ToString(value: true)
                );
                g.SetAttribute(
                    name: "ConfirmSelectionChange",
                    value: XmlConvert.ToString(value: true)
                );
            }
            // the grid references some parent grid
            if (g.GetAttribute(name: "ParentId").Length > 0)
            {
                string entity = g.GetAttribute(name: "Entity");
                string parentEntity = g.GetAttribute(name: "ParentEntityName");
                string dataMember = g.GetAttribute(name: "DataMember");
                string parentId = g.GetAttribute(name: "ParentId");
                if (dataMember == "")
                {
                    dataMember = entity;
                }

                string parentDataMember = DataMemberByGridInstanceId(
                    doc: doc,
                    instanceId: parentId
                );
                DataTable t = dataset.Tables[name: entity];
                if (entity == parentEntity && dataMember == parentDataMember) // references to the same entity
                {
                    XmlElement filterExpressionsElement = doc.CreateElement(
                        name: "FilterExpressions"
                    );
                    g.AppendChild(newChild: filterExpressionsElement);
                    foreach (DataColumn col in t.PrimaryKey)
                    {
                        CreateComponentBinding(
                            doc: doc,
                            bindingsElement: bindingsElement,
                            parentId: parentId,
                            parentProperty: col.ColumnName,
                            parentEntity: parentEntity,
                            childId: g.GetAttribute(name: "ModelInstanceId"),
                            childProperty: col.ColumnName,
                            childEntity: entity,
                            isChildParameter: false
                        );
                    }
                }
                else // references to the parent entity
                {
                    DataRelation relation = null;

                    foreach (DataRelation r in t.ParentRelations)
                    {
                        if (r.ParentTable.TableName == parentEntity)
                        {
                            relation = r;
                            break;
                        }
                    }
                    if (relation == null)
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "ParentEntityName",
                            actualValue: parentEntity,
                            message: "Parent entity not found for entity '"
                                + entity
                                + "'. Cannot generate filters."
                        );
                    }

                    XmlElement filterExpressionsElement = doc.CreateElement(
                        name: "FilterExpressions"
                    );
                    g.AppendChild(newChild: filterExpressionsElement);
                    for (int i = 0; i < relation.ChildColumns.Length; i++)
                    {
                        CreateComponentBinding(
                            doc: doc,
                            bindingsElement: bindingsElement,
                            parentId: parentId,
                            parentProperty: relation.ParentColumns[i].ColumnName,
                            parentEntity: parentEntity,
                            childId: g.GetAttribute(name: "ModelInstanceId"),
                            childProperty: relation.ChildColumns[i].ColumnName,
                            childEntity: entity,
                            isChildParameter: false
                        );
                    }
                }
            }
        }
    }
}
