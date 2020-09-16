import { DataViewLifecycle } from "model/entities/DataViewLifecycle/DataViewLifecycle";
import { LookupLoader } from "model/entities/LookupLoader";
import { RowState } from "model/entities/RowState";
import { Action } from "model/entities/Action";
import { ActionParameter } from "model/entities/ActionParameter";
import { ComponentBinding, ComponentBindingPair } from "model/entities/ComponentBinding";
import { DataSource } from "model/entities/DataSource";
import { DataSourceField } from "model/entities/DataSourceField";
import { DataTable } from "model/entities/DataTable";
import { DataView } from "model/entities/DataView";
import { DropDownColumn } from "model/entities/DropDownColumn";
import { FilterConfiguration } from "model/entities/FilterConfiguration";
import { FormPanelView } from "model/entities/FormPanelView/FormPanelView";
import { FormScreen } from "model/entities/FormScreen";
import { Lookup } from "model/entities/Lookup";
import { OrderingConfiguration } from "model/entities/OrderingConfiguration";
import { Property, ILookupIndividualEngine } from "model/entities/Property";
import { ColumnConfigurationDialog } from "model/entities/TablePanelView/ColumnConfigurationDialog";
import { TablePanelView } from "model/entities/TablePanelView/TablePanelView";
import { IComponentBinding } from "model/entities/types/IComponentBinding";
import { IFormScreenLifecycle02 } from "model/entities/types/IFormScreenLifecycle";
import { IPanelViewType } from "model/entities/types/IPanelViewType";
import { flf2mof } from "utils/flashDateFormat";
import { findStopping } from "./xmlUtils";
import { GroupingConfiguration } from "model/entities/GroupingConfiguration";
import { ServerSideGrouper } from "model/entities/ServerSideGrouper";
import { ClientSideGrouper } from "model/entities/ClientSideGrouper";
import $root from "rootContainer";
import { SCOPE_Screen } from "modules/Screen/ScreenModule";
import { SCOPE_DataView } from "modules/DataView/DataViewModule";
import { scopeFor, TypeSymbol } from "dic/Container";
import { SCOPE_FormPerspective } from "modules/DataView/Perspective/FormPerspective/FormPerspectiveModule";
import { IFormPerspectiveDirector } from "modules/DataView/Perspective/FormPerspective/FormPerspectiveDirector";
import { SCOPE_TablePerspective } from "modules/DataView/Perspective/TablePerspective/TablePerspectiveModule";
import { ITablePerspectiveDirector } from "modules/DataView/Perspective/TablePerspective/TablePerspectiveDirector";
import { IPerspective } from "modules/DataView/Perspective/Perspective";
import { flow } from "mobx";
import { IViewConfiguration, ViewConfiguration } from "modules/DataView/ViewConfiguration";
import { saveColumnConfigurations } from "model/actions/DataView/TableView/saveColumnConfigurations";
import { IPanelConfiguration } from "model/entities/types/IPanelConfiguration";
import { parseToOrdering } from "model/entities/types/IOrderingConfiguration";
import { isInfiniteScrollingActive } from "model/selectors/isInfiniteScrollingActive";
import { cssString2Object } from "utils/objects";
import { TreeDataTable } from "model/entities/TreeDataTable";
import { parseAggregationType } from "model/entities/types/AggregationType";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getEntity } from "model/selectors/DataView/getEntity";
import { DataViewAPI } from "modules/DataView/DataViewAPI";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { IRowCursor, RowCursor } from "modules/DataView/TableCursor";
import { getDataViewPropertyById } from "model/selectors/DataView/getDataViewPropertyById";
import { DataViewData } from "modules/DataView/DataViewData";
import { ScreenAPI } from "modules/Screen/ScreenAPI";
import { getMenuItemId } from "model/selectors/getMenuItemId";
import { getSessionId } from "model/selectors/getSessionId";
import { getApi } from "model/selectors/getApi";
import { getWorkbench } from "model/selectors/getWorkbench";
import { SCOPE_FormScreen } from "modules/Screen/FormScreen/FormScreenModule";
import { IOrigamAPI, OrigamAPI } from "model/entities/OrigamAPI";
import { IDataView } from "modules/DataView/DataViewTypes";
import { createIndividualLookupEngine } from "modules/Lookup/LookupModule";

export const findUIRoot = (node: any) => findStopping(node, (n) => n.name === "UIRoot")[0];

export const findUIChildren = (node: any) =>
  findStopping(node, (n) => n.parent.name === "UIChildren");

export const findBoxes = (node: any) =>
  findStopping(node, (n) => n.attributes && n.attributes.Type === "Box");

export const findChildren = (node: any) => findStopping(node, (n) => n.name === "Children")[0];

export const findActions = (node: any) =>
  findStopping(node, (n) => n.parent.name === "Actions" && n.name === "Action");

export const findParameters = (node: any) => findStopping(node, (n) => n.name === "Parameter");

export const findStrings = (node: any) =>
  findStopping(node, (n) => n.name === "string").map(
    (n) => findStopping(n, (n2) => n2.type === "text")[0].text
  );

export const findFormPropertyIds = (node: any) =>
  findStopping(node, (n) => n.name === "string" && n.parent.name === "PropertyNames").map(
    (n) => findStopping(n, (n2) => n2.type === "text")[0].text
  );

export const findFormRoot = (node: any) => findStopping(node, (n) => n.name === "FormRoot")[0];

export function interpretScreenXml(
  screenDoc: any,
  formScreenLifecycle: IFormScreenLifecycle02,
  panelConfigurationsRaw: any,
  lookupMenuMappings: any,
  sessionId: string
) {
  const workbench = getWorkbench(formScreenLifecycle);
  const $workbench = scopeFor(workbench);
  const panelConfigurations = new Map<string, IPanelConfiguration>(
    panelConfigurationsRaw.map((pcr: any) => [
      pcr.panel.instanceId,
      {
        position: pcr.position,
        defaultOrdering: parseToOrdering(pcr.defaultSort),
      },
    ])
  );

  const dataSourcesXml = findStopping(screenDoc, (n) => n.name === "DataSources")[0];

  const windowXml = findStopping(screenDoc, (n) => n.name === "Window")[0];

  const dataViews = findStopping(
    screenDoc,
    (n) =>
      (n.name === "UIElement" || n.name === "UIRoot") &&
      (n.attributes.Type === "Grid" || n.attributes.Type === "TreePanel")
  );

  checkInfiniteScrollWillWork(dataViews, formScreenLifecycle, panelConfigurations);

  function panelViewFromNumber(pvn: number) {
    switch (pvn) {
      case 1:
      default:
        return IPanelViewType.Table;
      case 0:
        return IPanelViewType.Form;
    }
  }

  function getPropertyParameters(node: any) {
    const parameters = findParameters(node);
    const result: { [key: string]: any } = {};
    for (let p of parameters) {
      result[p.attributes.Name] = p.attributes.Value;
    }
    return result;
  }

  const xmlComponentBindings = findStopping(
    screenDoc,
    (n) => n.name === "Binding" && n.parent.name === "ComponentBindings"
  );

  const screenAPI = new ScreenAPI(
    () => getSessionId(scr),
    () => getMenuItemId(scr),
    () => getApi(scr)
  );

  const componentBindings: IComponentBinding[] = [];

  for (let xmlBinding of xmlComponentBindings) {
    let existingBinding = componentBindings.find(
      (item) =>
        item.parentId === xmlBinding.attributes.ParentId &&
        item.childId === xmlBinding.attributes.ChildId
    );
    const componentBindingPair = new ComponentBindingPair({
      parentPropertyId: xmlBinding.attributes.ParentProperty,
      childPropertyId: xmlBinding.attributes.ChildProperty,
    });
    if (existingBinding) {
      existingBinding.bindingPairs.push(componentBindingPair);
    } else {
      componentBindings.push(
        new ComponentBinding({
          parentId: xmlBinding.attributes.ParentId,
          childId: xmlBinding.attributes.ChildId,
          parentEntity: xmlBinding.attributes.ParentEntity,
          childEntity: xmlBinding.attributes.ChildEntity,
          bindingPairs: [componentBindingPair],
          childPropertyType: xmlBinding.attributes.ChildPropertyType,
        })
      );
    }
  }
  const scr = new FormScreen({
    title: windowXml.attributes.Title,
    menuId: windowXml.attributes.MenuId,
    dynamicTitleSource: screenDoc.elements[0].attributes.DynamicFormLabelSource,
    sessionId,
    autoWorkflowNext: screenDoc.elements[0].attributes.AutoWorkflowNext === "true",
    openingOrder: 0,
    suppressSave: windowXml.attributes.SuppressSave === "true",
    showInfoPanel: windowXml.attributes.ShowInfoPanel === "true",
    showWorkflowNextButton: windowXml.attributes.ShowWorkflowNextButton === "true",
    showWorkflowCancelButton: windowXml.attributes.ShowWorkflowCancelButton === "true",
    autoRefreshInterval: parseInt(windowXml.attributes.AutoRefreshInterval, 10),
    refreshOnFocus: windowXml.attributes.RefreshOnFocus === "true",
    cacheOnClient: windowXml.attributes.CacheOnClient === "true",
    autoSaveOnListRecordChange: windowXml.attributes.AutoSaveOnListRecordChange === "true",
    requestSaveAfterUpdate: windowXml.attributes.RequestSaveAfterUpdate === "true",
    screenUI: screenDoc,
    panelConfigurations,
    formScreenLifecycle,
    // isSessioned: windowXml.attributes.UseSession,
    dataSources: dataSourcesXml.elements.map((dataSource: any) => {
      return new DataSource({
        rowState: new RowState({}),
        entity: dataSource.attributes.Entity,
        dataStructureEntityId: dataSource.attributes.DataStructureEntityId,
        identifier: dataSource.attributes.Identifier,
        lookupCacheKey: dataSource.attributes.LookupCacheKey,
        fields: findStopping(dataSource, (n) => n.name === "Field").map((field) => {
          return new DataSourceField({
            index: parseInt(field.attributes.Index, 10),
            name: field.attributes.Name,
          });
        }),
      });
    }),

    dataViews: dataViews.map((dataView, i) => {
      const configuration = findStopping(dataView, (n) => n.name === "Configuration");

      const properties = findStopping(dataView, (n) => n.name === "Property").map(
        (property, idx) => {
          return new Property({
            xmlNode: property,
            id: property.attributes.Id,
            modelInstanceId: property.attributes.ModelInstanceId || "",
            name: property.attributes.Name,
            readOnly: property.attributes.ReadOnly === "true",
            x: parseInt(property.attributes.X, 10),
            y: parseInt(property.attributes.Y, 10),
            width: parseInt(property.attributes.Width, 10),
            height: parseInt(property.attributes.Height, 10),
            captionLength: parseInt(property.attributes.CaptionLength, 10),
            captionPosition: property.attributes.CaptionPosition,
            entity: property.attributes.Entity,
            column: property.attributes.Column,
            parameters: getPropertyParameters(property),
            dock: property.attributes.Dock,
            multiline: property.attributes.Multiline === "true",
            isPassword: property.attributes.IsPassword === "true",
            isRichText: property.attributes.IsRichText === "true",
            maxLength: parseInt(property.attributes.MaxLength, 10),
            formatterPattern: property.attributes.FormatterPattern
              ? flf2mof(property.attributes.FormatterPattern)
              : "",
            customNumericFormat: property.attributes.CustomNumericFormat,
            identifier: property.attributes.Identifier,
            gridColumnWidth: property.attributes.GridColumnWidth
              ? parseInt(property.attributes.GridColumnWidth)
              : 100,
            columnWidth: property.attributes.GridColumnWidth
              ? parseInt(property.attributes.GridColumnWidth)
              : 100,
            lookupId: property.attributes.LookupId,
            lookup: !property.attributes.LookupId
              ? undefined
              : new Lookup({
                  dropDownShowUniqueValues: property.attributes.DropDownShowUniqueValues === "true",
                  lookupId: property.attributes.LookupId,
                  identifier: property.attributes.Identifier,
                  identifierIndex: parseInt(property.attributes.IdentifierIndex, 10),
                  dropDownType: property.attributes.DropDownType,
                  cached: property.attributes.Cached === "true",
                  searchByFirstColumnOnly: property.attributes.SearchByFirstColumnOnly === "true",
                  dropDownColumns: findStopping(property, (n) => n.name === "Property").map(
                    (ddProperty) => {
                      return new DropDownColumn({
                        id: ddProperty.attributes.Id,
                        name: ddProperty.attributes.Name,
                        column: ddProperty.attributes.Column,
                        entity: ddProperty.attributes.Entity,
                        index: parseInt(ddProperty.attributes.Index, 10),
                      });
                    }
                  ),
                  dropDownParameters: findStopping(
                    property,
                    (n) => n.name === "ComboBoxParameterMapping"
                  ).map((ddParam) => {
                    return {
                      parameterName: ddParam.attributes.ParameterName,
                      fieldName: ddParam.attributes.FieldName,
                    };
                  }),
                }),

            allowReturnToForm: property.attributes.AllowReturnToForm === "true",
            isTree: property.attributes.IsTree === "true",
            isAggregatedColumn: property.attributes.Aggregated || false,
            isLookupColumn: property.attributes.IsLookupColumn || false,
            style: cssString2Object(property.attributes.Style),
          });
        }
      );

      const formPropertyIds = new Set(findFormPropertyIds(dataView));
      for (let prop of properties) {
        if (formPropertyIds.has(prop.id)) {
          prop.isFormField = true;
        }
      }

      const actions = findActions(dataView).map(
        (action) =>
          new Action({
            id: action.attributes.Id,
            caption: action.attributes.Caption,
            groupId: action.attributes.GroupId,
            type: action.attributes.Type,
            iconUrl: action.attributes.IconUrl,
            mode: action.attributes.Mode,
            isDefault: action.attributes.IsDefault === "true",
            placement: action.attributes.Placement,
            confirmationMessage: action.attributes.ConfirmationMessage,
            parameters: findParameters(action).map(
              (parameter) =>
                new ActionParameter({
                  name: parameter.attributes.Name,
                  fieldName: parameter.attributes.FieldName,
                })
            ),
          })
      );
      const defaultOrderings = panelConfigurations.get(dataView.attributes.ModelInstanceId)
        ?.defaultOrdering;
      if (defaultOrderings) {
        for (let ordering of defaultOrderings) {
          ordering.lookupId = properties.find((prop) => prop.id === ordering.columnId)?.lookupId;
        }
      }

      const orderingConfiguration = new OrderingConfiguration(defaultOrderings);
      const implicitFilters = getImplicitFilters(dataView);
      const filterConfiguration = new FilterConfiguration(implicitFilters);
      const dataViewInstance: DataView = new DataView({
        isFirst: i === 0,
        id: dataView.attributes.Id,
        attributes: dataView.attributes,
        type: dataView.attributes.Type,
        modelInstanceId: dataView.attributes.ModelInstanceId,
        name: dataView.attributes.Name,
        modelId: dataView.attributes.ModelId,
        defaultPanelView: panelViewFromNumber(parseInt(dataView.attributes.DefaultPanelView)),
        activePanelView: panelViewFromNumber(parseInt(dataView.attributes.DefaultPanelView)),
        isHeadless: dataView.attributes.IsHeadless === "true",
        disableActionButtons: dataView.attributes.DisableActionButtons === "true",
        showAddButton: dataView.attributes.ShowAddButton === "true",
        showDeleteButton: dataView.attributes.ShowDeleteButton === "true",
        showSelectionCheckboxesSetting: dataView.attributes.ShowSelectionCheckboxes === "true",
        isGridHeightDynamic: dataView.attributes.IsGridHeightDynamic === "true",
        selectionMember: dataView.attributes.SelectionMember,
        orderMember: dataView.attributes.OrderMember,
        isDraggingEnabled: dataView.attributes.IsDraggingEnabled === "true",
        entity: dataView.attributes.Entity,
        dataMember: dataView.attributes.DataMember,
        isRootGrid: dataView.attributes.IsRootGrid === "true",
        isRootEntity: dataView.attributes.IsRootEntity === "true",
        isPreloaded: dataView.attributes.IsPreloaded === "true",
        requestDataAfterSelectionChange:
          dataView.attributes.RequestDataAfterSelectionChange === "true",
        confirmSelectionChange: dataView.attributes.ConfirmSelectionChange === "true",
        formViewUI: findFormRoot(dataView),
        dataTable:
          dataView.attributes.Type === "TreePanel"
            ? new TreeDataTable(
                dataView.attributes.IdProperty,
                dataView.attributes.ParentIdProperty
              )
            : new DataTable({
                formScreenLifecycle: formScreenLifecycle,
                dataViewAttributes: dataView.attributes,
                orderingConfiguration: orderingConfiguration,
                filterConfiguration: filterConfiguration,
              }),
        serverSideGrouper: new ServerSideGrouper(),
        lifecycle: new DataViewLifecycle(),
        tablePanelView: new TablePanelView({
          tablePropertyIds: properties.slice(1).map((prop) => prop.id),
          columnConfigurationDialog: new ColumnConfigurationDialog(),
          filterConfiguration: filterConfiguration,
          orderingConfiguration: orderingConfiguration,
          groupingConfiguration: new GroupingConfiguration(),
          rowHeight: 25,
        }),
        formPanelView: new FormPanelView(),
        lookupLoader: new LookupLoader(),
        properties,
        actions,
        clientSideGrouper: new ClientSideGrouper(),
        dataViewData: new DataViewData(
          () => dataViewInstance.dataTable,
          (propId) => getDataViewPropertyById(dataViewInstance, propId)
        ),
        dataViewRowCursor: new RowCursor(() => getSelectedRowId(dataViewInstance)),
        dataViewApi: new DataViewAPI(
          () => getDataStructureEntityId(dataViewInstance),
          () => getEntity(dataViewInstance),
          () => screenAPI
        ),
      });

      let groupingColumnCounter = 1;
      configuration.forEach((conf) => {
        const defaultColumnConfigurations = findStopping(conf, (n) => n.name === "columnWidths");
        defaultColumnConfigurations.forEach((defaultColumnConfiguration) => {
          const columns = findStopping(defaultColumnConfiguration, (n) => n.name === "column");
          columns.forEach((column, columnIndex) => {
            if (column.attributes.property) {
              // COLUMN WIDTH
              const prop = properties.find((prop) => prop.id === column.attributes.property);
              prop && prop.setColumnWidth(+column.attributes.width);

              // COLUMN HIDING
              if (column.attributes.isHidden === "true") {
                dataViewInstance.tablePanelView.setPropertyHidden(column.attributes.property, true);
              }
              if (column.attributes.aggregationType !== "0") {
                const aggregationType = parseAggregationType(column.attributes.aggregationType);
                dataViewInstance.tablePanelView.aggregations.setType(
                  column.attributes.property,
                  aggregationType
                );
              }
            } else if (column.attributes.groupingField) {
              const property = properties.find(
                (prop) => prop.id === column.attributes.groupingField
              );
              if (!property?.isLookupColumn) {
                dataViewInstance.tablePanelView.groupingConfiguration.setGrouping(
                  column.attributes.groupingField,
                  groupingColumnCounter
                );
                groupingColumnCounter++;
              }
            }
          });
          dataViewInstance.tablePanelView.tablePropertyIds = dataViewInstance.tablePanelView.tablePropertyIds
            .slice()
            .sort((a, b) => {
              const colIdxA = columns.findIndex((column) => column.attributes.property === a);
              if (colIdxA === -1) return 0;
              const colIdxB = columns.findIndex((column) => column.attributes.property === b);
              if (colIdxB === -1) return 0;
              return colIdxA - colIdxB;
            });
        });
        const defaultView = findStopping(
          conf,
          (n) => n.name === "view" && n.parent.name === "defaultView"
        );
        defaultView.forEach((element) => {
          if (!dataViewInstance.isHeadless && element.attributes.id.length <= 2) {
            dataViewInstance.activePanelView = element.attributes.id;
          }
        });
      });

      lookupMenuMappings.forEach((mapping: any) => {
        if (mapping.lookupId && mapping.menuId) {
          properties.forEach((property) => {
            if (property.lookup && property.lookup.lookupId === mapping.lookupId) {
              property.linkToMenuId = mapping.menuId;
            }
          });
        }
      });

      // COLUMN ORDER

      return dataViewInstance;
    }),
    componentBindings: [],
  });

  for (let xmlBinding of xmlComponentBindings) {
    let existingBinding = scr.componentBindings.find(
      (item) =>
        item.parentId === xmlBinding.attributes.ParentId &&
        item.childId === xmlBinding.attributes.ChildId
    );
    const componentBindingPair = new ComponentBindingPair({
      parentPropertyId: xmlBinding.attributes.ParentProperty,
      childPropertyId: xmlBinding.attributes.ChildProperty,
    });
    if (existingBinding) {
      existingBinding.bindingPairs.push(componentBindingPair);
    } else {
      const cb = new ComponentBinding({
        parentId: xmlBinding.attributes.ParentId,
        childId: xmlBinding.attributes.ChildId,
        parentEntity: xmlBinding.attributes.ParentEntity,
        childEntity: xmlBinding.attributes.ChildEntity,
        bindingPairs: [componentBindingPair],
        childPropertyType: xmlBinding.attributes.ChildPropertyType,
      });
      scr.componentBindings.push(cb);
      cb.parent = scr;
    }
  }

  const $screen = $workbench!.beginLifetimeScope(SCOPE_Screen);
  const $formScreen = $screen.beginLifetimeScope(SCOPE_FormScreen);
  $root.register(IOrigamAPI, () => getApi(scr) as OrigamAPI).scopedInstance(SCOPE_Screen);

  const IFormScreen = TypeSymbol<FormScreen>("IFormScreen");

  $formScreen.register(IFormScreen, () => scr).scopedInstance(SCOPE_Screen);

  for (let dv of scr.dataViews) {
    const $dataView = $formScreen.beginLifetimeScope(SCOPE_DataView);
    $dataView.register(IDataView, () => dv).scopedInstance(SCOPE_DataView);

    $dataView
      .register(IRowCursor, () => new RowCursor(() => getSelectedRowId(dv)))
      .scopedInstance(SCOPE_DataView);
    $dataView
      .register(
        IViewConfiguration,
        () =>
          new ViewConfiguration(
            function* (perspectiveTag) {
              dv.activePanelView = perspectiveTag as any;
              yield* saveColumnConfigurations(dv)();
            },
            () => dv.activePanelView
          )
      )
      .scopedInstance(SCOPE_DataView);

    $dataView.resolve(IDataView);

    const $tablePerspective = $dataView.beginLifetimeScope(SCOPE_TablePerspective);
    $tablePerspective.resolve(ITablePerspectiveDirector).setup();

    const $formPerspective = $dataView.beginLifetimeScope(SCOPE_FormPerspective);
    $formPerspective.resolve(IFormPerspectiveDirector).setup();

    /*const $mapPerspective = $dataView.beginLifetimeScope(SCOPE_MapPerspective);
    $mapPerspective.resolve(IMapPerspectiveDirector).setup();*/

    //***************** */

    const { lookupMultiEngine } = workbench;

    const lookupEngineById = new Map<string, ILookupIndividualEngine>();

    for (let property of dv.properties) {
      if (property.isLookup && property.lookupId) {
        const { lookupId } = property;
        if (!lookupEngineById.has(lookupId)) {
          const lookupIndividualEngine = createIndividualLookupEngine(lookupId, lookupMultiEngine);
          lookupMultiEngine.lookupCleanerReloaderById.set(
            lookupId,
            lookupIndividualEngine.lookupCleanerReloader
          );
          lookupIndividualEngine.startup();
          lookupEngineById.set(lookupId, lookupIndividualEngine);
        }
        const lookupIndividualEngine = lookupEngineById.get(lookupId)!;
        property.lookupEngine = lookupIndividualEngine;
      }
    }

    flow($dataView.resolve(IPerspective).activateDefault)();
  }

  const rscr = $formScreen.resolve(IFormScreen); // Hack to associate FormScreen with its scope to dispose it later.
  return scr;
}

function getImplicitFilters(dataViewXml: any) {
  const filterNodes = findStopping(dataViewXml, (node) => node.name === "Filter");
  if (filterNodes.length === 0) {
    return [];
  }
  return filterNodes.flatMap((node) =>
    node.elements.map((element: any) => {
      return {
        propertyId: element.attributes.Property,
        operatorCode: element.attributes.Operator,
        value: element.attributes.RightSideValue,
      };
    })
  );
}

function checkInfiniteScrollWillWork(
  dataViews: any[],
  formScreenLifecycle: IFormScreenLifecycle02,
  panelConfigurations: Map<string, IPanelConfiguration>
) {
  for (let dataView of dataViews) {
    const id = dataView.attributes.ModelInstanceId;
    const panelConfig = panelConfigurations.get(id);
    if (isInfiniteScrollingActive(formScreenLifecycle, dataView.attributes) && !panelConfig) {
      throw new Error(
        `Table in: ${id} has undefined default ordering while infinite scrolling is on. Make sure the underlying DataStructure has a SortSet defined.`
      );
    }
  }
}
