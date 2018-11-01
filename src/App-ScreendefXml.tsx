import axios from "axios";
import { observable, action, computed } from "mobx";
import { observer, Provider, inject, Observer } from "mobx-react";
import * as React from "react";
import * as xmlJs from "xml-js";
import "./App.scss";
import "./styles/screenComponents.scss";
import { isArray } from "util";
import { AutoSizer } from "react-virtualized";
import { GridComponent, ColumnHeaders } from "./Grid/GridComponent";
import { GridCursorComponent } from "./Grid/GridCursorComponent";
import { GridEditorMounter } from "./cells/GridEditorMounter";
import { StringGridEditor } from "./cells/string/GridEditor";
import { createGridCellRenderer } from "./Grid/GridCellRenderer";
import { IGridPanelBacking } from "./GridPanel/types";
import {
  IGridPaneView,
  IGridSetup,
  IGridTopology,
  IFormSetup,
  IFormTopology
} from "./Grid/types";
import {
  IDataTableFieldStruct,
  ICellValue,
  IDataTableSelectors,
  IFieldType
} from "./DataTable/types";
import { LookupResolverProvider } from "./DataLoadingStrategy/LookupResolverProvider";
import { GridOrderingState } from "./GridOrdering/GridOrderingState";
import { GridOrderingSelectors } from "./GridOrdering/GridOrderingSelectors";
import { GridOrderingActions } from "./GridOrdering/GridOrderingActions";
import { GridOutlineState } from "./GridOutline/GridOutlineState";
import { GridOutlineSelectors } from "./GridOutline/GridOutlineSelectors";
import { GridOutlineActions } from "./GridOutline/GridOutlineActions";
import { DataLoader } from "./DataLoadingStrategy/DataLoader";
import { DataTableState, DataTableField } from "./DataTable/DataTableState";
import { DataTableSelectors } from "./DataTable/DataTableSelectors";
import { DataTableActions } from "./DataTable/DataTableActions";
import { EventObserver } from "./utils/events";
import { GridState } from "./Grid/GridState";
import { GridSelectors } from "./Grid/GridSelectors";
import { GridActions } from "./Grid/GridActions";
import { GridView } from "./Grid/GridView";
import { GridInteractionState } from "./Grid/GridInteractionState";
import { GridInteractionSelectors } from "./Grid/GridInteractionSelectors";
import { FormState } from "./Grid/FormState";
import { FormActions } from "./Grid/FormActions";
import { GridInteractionActions } from "./Grid/GridInteractionActions";
import { GridCursorView } from "./Grid/GridCursorView";
import { CellScrolling } from "./Grid/CellScrolling";
import { DataLoadingStrategyState } from "./DataLoadingStrategy/DataLoadingStrategyState";
import { DataLoadingStrategySelectors } from "./DataLoadingStrategy/DataLoadingStrategySelectors";
import { DataLoadingStrategyActions } from "./DataLoadingStrategy/DataLoadingStrategyActions";
import { DataSaver } from "./DataLoadingStrategy/DataSaver";
import { DataSavingStrategy } from "./DataLoadingStrategy/DataSavingStrategy";
import { GridToolbarView } from "./GridPanel/GridToolbarView";
import { GridSetup } from "./GridPanel/adapters/GridSetup";
import { GridTopology } from "./GridPanel/adapters/GridTopology";
import { FormView } from "./Grid/FormView";
import { FormTopology } from "./GridPanel/adapters/FormTopology";
import { createColumnHeaderRenderer } from "./Grid/ColumnHeaderRenderer";






function parseScreenDef(o: any) {
  const unhandled: any[] = [];
  const uiStruct: any = { children: [] };

  const DropDownColumns = "DropDownColumns";

  function processNode(node: any, uiStructOpened: any, pathFlags: Set<string>) {
    let newPathFlags: Set<string> = pathFlags;
    let newChild: any = uiStructOpened;
    switch (node.name) {
      case undefined:
        break;
      case "Window":
        newChild = {
          type: node.name,
          props: {
            name: node.attributes.Name,
            id: node.attributes.Id
          },
          children: []
        };
        uiStructOpened.children.push(newChild);
        break;
      case "FormRoot":
        newPathFlags.add("FormRoot");
        newChild = {
          type: "FormRoot",
          props: {},
          children: []
        };
        uiStructOpened.children.push(newChild);
        break;
      case "FormElement": {
        newPathFlags.add("FormElement");
        const location = {
          x: parseInt(node.attributes.X, 10),
          y: parseInt(node.attributes.Y, 10),
          w: parseInt(node.attributes.Width, 10),
          h: parseInt(node.attributes.Height, 10)
        };
        newChild = {
          type: "Panel",
          props: {
            name: node.attributes.Title,
            ...location
          },
          children: []
        };
        uiStructOpened.children.push(newChild);
        break;
      }
      case "string":
        if (pathFlags.has("FormRoot")) {
          newChild = {
            type: "Property",
            props: {
              id: node.elements[0] && node.elements[0].text
            },
            children: []
          };
          uiStructOpened.children.push(newChild);
        }
        break;
      case "UIRoot":
      case "UIElement": {
        const location = {
          x: parseInt(node.attributes.X, 10),
          y: parseInt(node.attributes.Y, 10),
          w: parseInt(node.attributes.Width, 10),
          h: parseInt(node.attributes.Height, 10)
        };
        newChild = {
          type: node.attributes.Type,
          props: {
            name: node.attributes.Name,
            id: node.attributes.Id,
            ...location
          },
          children: []
        };
        if (node.attributes.Type === "Grid") {
          newChild.props.isHeadless = node.attributes.IsHeadless === "true";
          newChild.props.isAddButton = node.attributes.ShowAddButton === "true";
          newChild.props.isCopyButton =
            node.attributes.ShowAddButton === "true";
          newChild.props.isDeleteButton =
            node.attributes.ShowDeleteButton === "true";
          newChild.props.name = node.attributes.Name;
        }
        uiStructOpened.children.push(newChild);
        break;
      }
      case "Property": {
        if (pathFlags.has(DropDownColumns)) {
          break;
        }
        const location = {
          x: parseInt(node.attributes.X, 10),
          y: parseInt(node.attributes.Y, 10),
          w: parseInt(node.attributes.Width, 10),
          h: parseInt(node.attributes.Height, 10)
        };
        newChild = {
          type: "Property",
          props: {
            name: node.attributes.Name,
            id: node.attributes.Id,
            entity: node.attributes.Entity,
            ...location,
            captionLength: parseInt(node.attributes.CaptionLength, 10),
            captionPosition: node.attributes.CaptionPosition
          },
          children: []
        };
        uiStructOpened.children.push(newChild);
        break;
      }
      case "DropDownColumns":
        newPathFlags = new Set(newPathFlags.keys());
        newPathFlags.add(DropDownColumns);
        break;
      case "Properties":
        newChild = {
          type: "Properties",
          props: {},
          children: []
        };
        uiStructOpened.children.push(newChild);
        break;
      default:
        unhandled.push(node);
        break;
    }
    if (node.elements) {
      for (const element of node.elements) {
        processNode(element, newChild, newPathFlags);
      }
    }
    if (newChild.type === "Tab") {
      const handles = [];
      for (const child of newChild.children) {
        handles.push({
          type: "TabHandle",
          props: {
            name: child.props.name,
            id: child.props.id
          },
          children: []
        });
      }
      newChild.props.handles = handles;
      if (handles[0]) {
        newChild.props.firstTabId = handles[0].props.id;
      }
    }
  }

  function moveProperties(node: any): any {
    if (node.type === "Grid") {
      const formRoot = node.children.find((ch: any) => ch.type === "FormRoot");
      const properties = node.children.find(
        (ch: any) => ch.type === "Properties"
      );
      const propertiesMap = new Map(
        properties.children.map((prop: any) => [prop.props.id, prop])
      );
      const formRootChildren = [...formRoot.children];
      for (
        let formRootChildIndex = 0;
        formRootChildIndex < formRootChildren.length;
        formRootChildIndex++
      ) {
        const formRootChild = formRootChildren[formRootChildIndex];
        if (formRootChild.type === "Property") {
          formRoot.children[formRootChildIndex] = propertiesMap.get(
            formRootChild.props.id
          );
        } else if (formRootChild.type === "Panel") {
          const panelChildren = [...formRootChild.children];
          for (
            let panelChildIndex = 0;
            panelChildIndex < formRootChild.children.length;
            panelChildIndex++
          ) {
            const panelChild = formRootChild.children[panelChildIndex];
            if (panelChild.type === "Property") {
              formRootChild.children[panelChildIndex] = propertiesMap.get(
                panelChild.props.id
              );
            }
          }
        }
      }
      node.children = node.children.filter(
        (ch: any) => ch.type !== "Properties"
      );
      return node;
    }
    for (const child of node.children) {
      moveProperties(child);
    }
    return node;
  }

  // console.log(o)
  processNode(o, uiStruct, new Set());
  moveProperties(uiStruct);
  // console.log(unhandled);
  return {
    uiStruct
  };
}

class OUIUnknown extends React.Component<any> {
  public render() {
    return (
      <div className="oui-unknown">
        {`Type: ${this.props.type} Name: ${this.props.name}, Id: ${
          this.props.id
        }`}
        {this.props.children}
      </div>
    );
  }
}

class OUIFormRoot extends React.Component<any> {
  public render() {
    return <div className="oui-form-root">{this.props.children}</div>;
  }
}

class OUIProperty extends React.Component<any> {
  public render() {
    if (!Number.isInteger(this.props.x) || !Number.isInteger(this.props.y)) {
      return null;
    }
    let captionLocation;
    if (this.props.captionPosition === "Left") {
      captionLocation = {
        left: this.props.x - this.props.captionLength,
        top: this.props.y,
        width: this.props.captionLength,
        minHeight: 20 // this.props.h,
      };
    } else if (this.props.captionPosition === "Top") {
      captionLocation = {
        left: this.props.x,
        top: this.props.y - 20,
        width: this.props.captionLength,
        minHeight: 20 // this.props.h,
      };
    } else {
      captionLocation = {
        left:
          this.props.x +
          (this.props.entity === "Boolean" ? this.props.h : this.props.w), // + this.props.captionLength,
        top: this.props.y,
        width: this.props.captionLength,
        minHeight: 20 // this.props.h,
      };
    }
    return (
      <>
        <div
          className="oui-property"
          style={{
            top: this.props.y,
            left: this.props.x,
            width:
              this.props.entity === "Boolean" ? this.props.h : this.props.w,
            height: this.props.h
          }}
        >
          {/*`Type: ${this.props.type} Name: ${this.props.name}, Id: ${
            this.props.id
          }`*/}
          {/*this.props.children*/}
          {/*this.props.name*/}
          {this.props.entity}
        </div>
        {this.props.captionPosition !== "None" && (
          <div className="oui-property-caption" style={{ ...captionLocation }}>
            {this.props.name}
          </div>
        )}
      </>
    );
  }
}

class OUIVSplit extends React.Component<any> {
  public render() {
    let children = React.Children.map(this.props.children, child => (
      <div className="oui-vsplit-panel">{child}</div>
    ));
    children = React.Children.map(children, (child, idx) => (
      <>
        {child}
        {idx < children.length - 1 && (
          <div className="oui-vsplit-handle">
            <div className="knob" />
          </div>
        )}
      </>
    ));
    return <div className="oui-vsplit-container">{children}</div>;
  }
}

class OUIHSplit extends React.Component<any> {
  public render() {
    let children = React.Children.map(this.props.children, child => (
      <div className="oui-hsplit-panel">{child}</div>
    ));
    children = React.Children.map(children, (child, idx) => (
      <>
        {child}
        {idx < children.length - 1 && (
          <div className="oui-hsplit-handle">
            <div className="knob" />
          </div>
        )}
      </>
    ));
    return <div className="oui-hsplit-container">{children}</div>;
  }
}

class OUIPanel extends React.Component<any> {
  public render() {
    return (
      <>
        <div
          className="oui-panel"
          style={{
            top: this.props.y,
            left: this.props.x,
            width: this.props.w,
            height: this.props.h
          }}
        >
          {this.props.children}
        </div>
        <div
          className="oui-panel-label"
          style={{
            top: this.props.y + 5,
            left: this.props.x + 5
          }}
        >
          {this.props.name}
        </div>
      </>
    );
  }
}

class OUIVBox extends React.Component<any> {
  public render() {
    return (
      <div
        className="oui-vbox"
        style={{
          maxWidth: this.props.w,
          maxHeight: this.props.h
        }}
      >
        {this.props.children}
      </div>
    );
  }
}

class OUIWindow extends React.Component<any> {
  public render() {
    return <div className="oui-window">{this.props.children}</div>;
  }
}

class OUIGridToolbar extends React.Component<any> {
  public render() {
    return (
      <div
        className={"oui-grid-toolbar" + (this.props.isHidden ? " hidden" : "")}
      >
        <div className="toolbar-section">
          <span className="toolbar-caption">{this.props.name}</span>
        </div>
        <div className="toolbar-section">
          {this.props.isAddButton && (
            <button className="oui-toolbar-btn">
              <i className="fa fa-plus-circle icon" aria-hidden="true" />
            </button>
          )}
          {this.props.isDeleteButton && (
            <button className="oui-toolbar-btn">
              <i className="fa fa-minus-circle icon" aria-hidden="true" />
            </button>
          )}
          {this.props.isCopyButton && (
            <button className="oui-toolbar-btn">
              <i className="fa fa-copy icon" aria-hidden="true" />
            </button>
          )}
        </div>
        <div className="toolbar-section pusher" />
        <div className="toolbar-section">
          <button className="oui-toolbar-btn">
            <i className="fa fa-step-backward icon" aria-hidden="true" />
          </button>
          <button className="oui-toolbar-btn">
            <i className="fa fa-caret-left icon" aria-hidden="true" />
          </button>
          <button className="oui-toolbar-btn">
            <i className="fa fa-caret-right icon" aria-hidden="true" />
          </button>
          <button className="oui-toolbar-btn">
            <i className="fa fa-step-forward icon" aria-hidden="true" />
          </button>
        </div>
        <div className="toolbar-section">
          <span className="oui-toolbar-text">1/6</span>
        </div>
        <div className="toolbar-section">
          <button className="oui-toolbar-btn">
            <i className="fa fa-table icon" aria-hidden="true" />
          </button>
          <button className="oui-toolbar-btn">
            <i className="fa fa-list-alt icon" aria-hidden="true" />
          </button>
          <button className="oui-toolbar-btn">
            <i className="fa fa-map-o icon" aria-hidden="true" />
          </button>
        </div>
        <div className="toolbar-section">
          <button className="oui-toolbar-btn">
            <i className="fa fa-filter icon" aria-hidden="true" />
          </button>
          <button className="oui-toolbar-btn">
            <i className="fa fa-caret-down icon" aria-hidden="true" />
          </button>
        </div>
        <div className="toolbar-section">
          <button className="oui-toolbar-btn">
            <i className="fa fa-cog icon" aria-hidden="true" />
            <i className="fa fa-caret-down icon" aria-hidden="true" />
          </button>
        </div>
      </div>
    );
  }
}

const personFields = [
  new DataTableField({
    id: "name",
    label: "Name",
    type: IFieldType.string,
    dataIndex: 0,
    isLookedUp: false
  }),
  new DataTableField({
    id: "birth_date",
    label: "Birth date",
    type: IFieldType.date,
    dataIndex: 1,
    isLookedUp: false
  }),
  new DataTableField({
    id: "likes_platypuses",
    label: "Likes platypuses?",
    type: IFieldType.boolean,
    dataIndex: 2,
    isLookedUp: false
  }),
  new DataTableField({
    id: "city_id",
    label: "Lives in",
    type: IFieldType.string,
    dataIndex: 3,
    isLookedUp: true,
    lookupResultFieldId: "name",
    lookupResultTableId: "city"
  }),
  new DataTableField({
    id: "favorite_color",
    label: "Favorite color",
    type: IFieldType.color,
    dataIndex: 4,
    isLookedUp: false
  })
];

const cityFields = [
  new DataTableField({
    id: "name",
    label: "Name",
    type: IFieldType.string,
    dataIndex: 0,
    isLookedUp: false
  }),
  new DataTableField({
    id: "inhabitants",
    label: "Inhabitants",
    type: IFieldType.integer,
    dataIndex: 1,
    isLookedUp: false
  })
];

class GridConfiguration {
  public gridSetup: IGridSetup;
  public gridTopology: IGridTopology;
  public formSetup: IFormSetup;
  public formTopology: IFormTopology;

  @action.bound
  public set(
    gridSetup: IGridSetup,
    gridTopology: IGridTopology,
    formSetup: IFormSetup,
    formTopology: IFormTopology
  ) {
    this.gridSetup = gridSetup;
    this.gridTopology = gridTopology;
    this.formSetup = formSetup;
    this.formTopology = formTopology;
  }
}

class FormSetup implements IFormSetup {
  constructor(public dataTableSelectors: IDataTableSelectors) {}

  public get dimensions() {
    return [
      [200, 30, 100, 20],
      [200, 60, 100, 20],
      [200, 90, 100, 20],
      [200, 120, 100, 20],
      [200, 150, 100, 20],
      [450, 30, 100, 20],
      [450, 60, 100, 20],
      [450, 90, 100, 20],
      [450, 120, 100, 20]
    ].slice(0, this.fieldCount);
  }

  @computed
  public get fieldCount(): number {
    return this.dataTableSelectors.fieldCount;
  }

  public isScrollingEnabled: boolean = true;

  public getCellTop(fieldIndex: number): number {
    return this.dimensions[fieldIndex][1];
  }

  public getCellBottom(fieldIndex: number): number {
    return this.getCellTop(fieldIndex) + this.getCellHeight(fieldIndex);
  }

  public getCellLeft(fieldIndex: number): number {
    return this.dimensions[fieldIndex][0];
  }

  public getCellRight(fieldIndex: number): number {
    return this.getCellLeft(fieldIndex) + this.getCellWidth(fieldIndex);
  }

  public getCellHeight(fieldIndex: number): number {
    return this.dimensions[fieldIndex][3];
  }

  public getCellWidth(fieldIndex: number): number {
    return this.dimensions[fieldIndex][2];
  }

  public getCellValue(
    recordIndex: number,
    fieldIndex: number
  ): ICellValue | undefined {
    const record = this.dataTableSelectors.getRecordByRecordIndex(recordIndex);
    const field = this.dataTableSelectors.getFieldByFieldIndex(fieldIndex);
    if (record && field) {
      return this.dataTableSelectors.getValue(record, field);
    } else {
      return;
    }
  }

  public getFieldLabel(fieldIndex: number): string {
    return `Field label ${fieldIndex}`;
  }

  public getLabelOffset(fieldIndex: number): number {
    return 100;
  }
}

function createGridPaneBacking(
  dataTableName: string,
  dataTableFields: IDataTableFieldStruct[]
) {
  const configuration = new GridConfiguration();

  const lookupResolverProvider = new LookupResolverProvider({
    get dataLoader() {
      return dataLoader;
    }
  });

  const gridOrderingState = new GridOrderingState();
  const gridOrderingSelectors = new GridOrderingSelectors(gridOrderingState);
  const gridOrderingActions = new GridOrderingActions(
    gridOrderingState,
    gridOrderingSelectors
  );

  const gridOutlineState = new GridOutlineState();
  const gridOutlineSelectors = new GridOutlineSelectors(gridOutlineState);
  const gridOutlineActions = new GridOutlineActions(
    gridOutlineState,
    gridOutlineSelectors
  );

  const dataLoader = new DataLoader(dataTableName);

  const dataTableState = new DataTableState();

  dataTableState.fields = dataTableFields;

  const dataTableSelectors = new DataTableSelectors(
    dataTableState,
    lookupResolverProvider,
    dataTableName
  );
  const dataTableActions = new DataTableActions(
    dataTableState,
    dataTableSelectors
  );

  const onStartGrid = EventObserver();
  const onStopGrid = EventObserver();

  const gridState = new GridState();
  const gridSelectors = new GridSelectors(
    gridState,
    configuration,
    configuration
  );
  const gridActions = new GridActions(gridState, gridSelectors, configuration);

  const gridView = new GridView(gridSelectors, gridActions);

  const gridInteractionState = new GridInteractionState();
  const gridInteractionSelectors = new GridInteractionSelectors(
    gridInteractionState,
    configuration,
    configuration
  );

  const formState = new FormState();
  const formActions = new FormActions(formState);

  const gridInteractionActions = new GridInteractionActions(
    gridInteractionState,
    gridInteractionSelectors,
    gridActions,
    gridSelectors,
    formActions,
    configuration
  );
  onStartGrid(() => gridInteractionActions.start());
  onStopGrid(() => gridInteractionActions.stop());

  const gridCursorView = new GridCursorView(
    gridInteractionSelectors,
    gridSelectors,
    dataTableSelectors,
    dataTableActions,
    configuration,
    configuration
  );

  const cellScrolling = new CellScrolling(
    gridSelectors,
    gridActions,
    gridInteractionSelectors,
    configuration,
    configuration
  );
  onStartGrid(() => cellScrolling.start());
  onStopGrid(() => cellScrolling.stop());

  const dataLoadingStrategyState = new DataLoadingStrategyState();
  const dataLoadingStrategySelectors = new DataLoadingStrategySelectors(
    dataLoadingStrategyState,
    gridSelectors,
    dataTableSelectors
  );
  const dataLoadingStrategyActions = new DataLoadingStrategyActions(
    dataLoadingStrategyState,
    dataLoadingStrategySelectors,
    dataTableActions,
    dataTableSelectors,
    dataLoader,
    gridOrderingSelectors,
    gridOrderingActions,
    gridOutlineSelectors,
    gridInteractionActions,
    gridSelectors,
    gridActions
  );
  onStartGrid(() => dataLoadingStrategyActions.start());
  onStopGrid(() => dataLoadingStrategyActions.stop());

  const dataSaver = new DataSaver(
    dataTableName,
    dataTableActions,
    dataTableSelectors
  );
  const dataSavingStrategy = new DataSavingStrategy(
    dataTableSelectors,
    dataTableActions,
    dataSaver
  );

  const gridToolbarView = new GridToolbarView(
    gridInteractionSelectors,
    gridSelectors,
    dataTableSelectors,
    dataTableActions,
    gridInteractionActions,
    configuration
  );

  const gridSetup = new GridSetup(gridInteractionSelectors, dataTableSelectors);
  const gridTopology = new GridTopology(dataTableSelectors);

  /*
  onStartGrid.trigger();

  // gridOrderingActions.setOrdering('name', 'asc');

  dataLoadingStrategyActions.requestLoadFresh();*/

  const formSetup = new FormSetup(dataTableSelectors);
  const formView = new FormView(
    dataTableSelectors,
    gridInteractionSelectors,
    formSetup
  );

  const formTopology = new FormTopology(gridTopology);

  configuration.set(gridSetup, gridTopology, formSetup, formTopology);

  return {
    gridToolbarView,
    gridView,
    gridSetup,
    gridTopology,
    gridCursorView,
    gridInteractionActions,
    gridInteractionSelectors,
    onStartGrid,
    onStopGrid,
    dataLoadingStrategyActions,
    dataTableSelectors,

    formView,
    formSetup,
    formTopology,
    formActions
  };
}

class GridTable extends React.Component<any> {
  constructor(props: any) {
    super(props);
    this.gridPanelBacking = createGridPaneBacking(
      this.props.initialDataTableName,
      this.props.initialFields
    );
  }

  private gridPanelBacking: IGridPanelBacking;

  public componentDidMount() {
    this.gridPanelBacking.onStartGrid.trigger();
    this.gridPanelBacking.dataLoadingStrategyActions
      .requestLoadFresh()
      .then(() => {
        this.gridPanelBacking.gridInteractionActions.selectFirst();
      });
  }

  public render() {
    const {
      gridToolbarView,
      gridView,
      gridSetup,
      gridTopology,
      gridCursorView,
      gridInteractionActions,
      gridInteractionSelectors,
      formView,
      formSetup,
      formTopology,
      formActions
    } = this.gridPanelBacking;
    return (
      <>
        <div
          style={{
            display: "flex",
            flexDirection: "column"
          }}
        >
          <ColumnHeaders
            view={gridView}
            columnHeaderRenderer={createColumnHeaderRenderer({
              gridSetup
            })}
          />
        </div>

        <div
          style={{
            flexDirection: "column",
            height: "100%",
            flex: "1 1",
            display:
              gridInteractionSelectors.activeView === IGridPaneView.Grid
                ? "flex"
                : "none"
          }}
        >
          <AutoSizer>
            {({ width, height }) => (
              <Observer>
                {() => (
                  <GridComponent
                    view={gridView}
                    gridSetup={gridSetup}
                    gridTopology={gridTopology}
                    width={width}
                    height={height}
                    overlayElements={
                      <GridCursorComponent
                        view={gridCursorView}
                        cursorContent={
                          gridInteractionSelectors.activeView ===
                            IGridPaneView.Grid && (
                            <GridEditorMounter cursorView={gridCursorView}>
                              {gridCursorView.isCellEditing && (
                                <StringGridEditor
                                  editingRecordId={gridCursorView.editingRowId!}
                                  editingFieldId={
                                    gridCursorView.editingColumnId!
                                  }
                                  value={
                                    gridCursorView.editingOriginalCellValue
                                  }
                                  onKeyDown={
                                    gridInteractionActions.handleDumbEditorKeyDown
                                  }
                                  onDataCommit={gridCursorView.handleDataCommit}
                                />
                              )}
                            </GridEditorMounter>
                          )
                        }
                      />
                    }
                    cellRenderer={createGridCellRenderer({
                      gridSetup,
                      onClick(event, cellRect, cellInfo) {
                        gridInteractionActions.handleGridCellClick(event, {
                          rowId: gridTopology.getRowIdByIndex(
                            cellInfo.rowIndex
                          )!,
                          columnId: gridTopology.getColumnIdByIndex(
                            cellInfo.columnIndex
                          )!
                        });
                      }
                    })}
                    onKeyDown={gridInteractionActions.handleGridKeyDown}
                    onOutsideClick={
                      gridInteractionActions.handleGridOutsideClick
                    }
                    onNoCellClick={gridInteractionActions.handleGridNoCellClick}
                  />
                )}
              </Observer>
            )}
          </AutoSizer>
        </div>
      </>
    );
  }
}

class OUIGrid extends React.Component<any> {
  public render() {
    return (
      <div
        className="oui-grid"
        style={{
          maxWidth: this.props.w,
          maxHeight: this.props.h
        }}
      >
        <OUIGridToolbar isHidden={this.props.isHeadless} />
        { <GridTable initialDataTableName="person" initialFields={personFields} />}
        {/*this.props.children*/}
      </div>
    );
  }
}

@observer
class OUITab extends React.Component<any> {
  @observable
  public activeTabId: string = this.props.firstTabId;

  @action.bound
  public handleHandleClick(event: any, tabId: string) {
    this.activeTabId = tabId;
    console.log("Handle click", this.activeTabId);
  }

  public render() {
    return (
      <Provider tabParent={this}>
        <div className="oui-tab">
          <div className="oui-tab-handles">{this.props.handles}</div>
          <div className="oui-tab-panels">{this.props.children}</div>
        </div>
      </Provider>
    );
  }
}

@inject(stores => {
  const { tabParent } = stores as any;
  const { activeTabId, handleHandleClick } = tabParent;
  return {
    activeTabId,
    onHandleClick: handleHandleClick
  };
})
@observer
class OUITabHandle extends React.Component<any> {
  public render() {
    return (
      <div
        onClick={event => this.props.onHandleClick(event, this.props.id)}
        className={
          "oui-tab-handle" +
          (this.props.id === this.props.activeTabId ? " active" : "")
        }
      >
        {this.props.name}
      </div>
    );
  }
}

@inject(({ tabParent: { activeTabId } }) => {
  return {
    activeTabId
  };
})
@observer
class OUIBox extends React.Component<any> {
  public render() {
    return (
      <div
        className="oui-box"
        style={{
          display: this.props.activeTabId !== this.props.id ? "none" : undefined
        }}
      >
        {this.props.children}
      </div>
    );
  }
}

class OUILabel extends React.Component<any> {
  public render() {
    return <div className="oui-label">{this.props.name}</div>;
  }
}

class OUITreePanel extends React.Component<any> {
  public render() {
    return (
      <div>
        <img
          style={{ width: 250 }}
          src="https://thegraphicsfairy.com/wp-content/uploads/blogger/-GXi8yHjt0fc/T-zV1MX-VfI/AAAAAAAASgI/uChxO5KV9yE/s1600/tree-Vintage-GraphicsFairy6.jpg"
        />
      </div>
    );
  }
}

const elementTypes = {
  FormRoot: OUIFormRoot,
  Property: OUIProperty,
  Panel: OUIPanel,
  VSplit: OUIVSplit,
  HSplit: OUIHSplit,
  Window: OUIWindow,
  Grid: OUIGrid,
  Tab: OUITab,
  Box: OUIBox,
  VBox: OUIVBox,
  TabHandle: OUITabHandle,
  Label: OUILabel,
  TreePanel: OUITreePanel
};

function createUITree(uiStruct: any): any {
  if (isArray(uiStruct)) {
    console.log(uiStruct);
  }

  if (isArray(uiStruct)) {
    return uiStruct.map(uiStr => createUITree(uiStr));
  }
  const elementClass = elementTypes[uiStruct.type];
  if (!elementClass) {
    return React.createElement(
      OUIUnknown,
      { ...uiStruct.props, type: uiStruct.type },
      uiStruct.children.map((child: any) => createUITree(child))
    );
  } else {
    switch (uiStruct.type) {
      case "Tab":
        return React.createElement(
          elementClass,
          {
            ...uiStruct.props,
            handles: uiStruct.props.handles.map((child: any) =>
              createUITree(child)
            )
          },
          uiStruct.children.map((child: any) => createUITree(child))
        );
        break;
      default:
        return React.createElement(
          elementClass,
          uiStruct.props,
          uiStruct.children.map((child: any) => createUITree(child))
        );
        break;
    }
  }
}

@observer
class App extends React.Component {
  @observable.ref
  public xmlObj: any;
  @observable.ref
  public screenDef: any;

  public async componentDidMount() {
    const xml = (await axios.get("/screen03.xml")).data;
    this.xmlObj = xmlJs.xml2js(xml, { compact: false });

    const xo = this.xmlObj;
    const screenDef = parseScreenDef(xo);
    this.screenDef = screenDef;
    console.log(this.screenDef.uiStruct.children[0]);
  }

  public render() {
    return (
      <>
        {/**/}

        {this.screenDef && createUITree(this.screenDef.uiStruct.children[0])}
        {/*<pre>{JSON.stringify(this.xmlObj, null, 2)}</pre>*/}
        {/*<pre>
          {JSON.stringify(
            this.screenDef && this.screenDef.uiStruct.children[0],
            null,
            2
          )}
        </pre>*/}
      </>
    );
  }
}

export default App;
