import { action, computed, autorun, observable } from "mobx";
import { observer, Provider, inject } from "mobx-react";
import * as React from "react";

import { GridToolbar } from "../controls/GridToolbar";

import { IDataViewProps, IDataViewType, IDataViewState } from "./types";


import {
  IDataTableSelectors,
  IRecord,
  IField,
  IGridTableEvents
} from "src/Grid/types2";
import { IDataCursorState } from "../../Grid/types2";
import DataCursorState from "src/Grid/DataCursorState";


/*class GridConfiguration {
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
}*/

/*
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


function createGridPaneBacking(
  dataStructureEntityId: string,
  dataTableFields: IDataTableFieldStruct[],
  defaultView: GridViewType,
  menuItemId: string,
  modelInstanceId: string
) {
  // console.log(defaultView)
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

  const dataLoader = new DataLoader(dataStructureEntityId, api, menuItemId);

  const dataTable = new DataTable(
    lookupResolverProvider,
    dataStructureEntityId
  );

  dataTable.setFields(dataTableFields);

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
    configuration,
    dataTable
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
  gridInteractionActions.setActiveView(defaultView);
  onStartGrid(() => gridInteractionActions.start());
  onStopGrid(() => gridInteractionActions.stop());

  const gridCursorView = new GridCursorView(
    gridInteractionSelectors,
    gridInteractionActions,
    gridSelectors,
    dataTable,
    dataTable,
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
    dataTable
  );
  const dataLoadingStrategyActions = new DataLoadingStrategyActions(
    dataLoadingStrategyState,
    dataLoadingStrategySelectors,
    dataTable,
    dataTable,
    dataLoader,
    gridOrderingSelectors,
    gridOrderingActions,
    gridOutlineSelectors,
    gridInteractionActions,
    gridSelectors,
    gridActions,
    gridCursorView
  );
  onStartGrid(() => dataLoadingStrategyActions.start());
  onStopGrid(() => dataLoadingStrategyActions.stop());

  const dataSaver = new DataSaver(
    dataStructureEntityId,
    dataTable,
    dataTable,
    menuItemId
  );
  const dataSavingStrategy = new DataSavingStrategy(
    dataTable,
    dataTable,
    dataSaver,
    dataLoadingStrategyActions
  );

  const gridToolbarView = new GridToolbarView(
    gridInteractionSelectors,
    gridSelectors,
    dataTable,
    dataTable,
    gridInteractionActions,
    configuration
  );

  const gridSetup = new GridSetup(gridInteractionSelectors, dataTable);
  const gridTopology = new GridTopology(dataTable);

  /*
  onStartGrid.trigger();

  // gridOrderingActions.setOrdering('name', 'asc');

  dataLoadingStrategyActions.requestLoadFresh();*/

/*
  const formSetup = new FormSetup(dataTable);
  const formView = new FormView(dataTable, gridInteractionSelectors, formSetup);

  const formTopology = new FormTopology(gridTopology);

  configuration.set(gridSetup, gridTopology, formSetup, formTopology);
*/
/*return {
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
    dataLoadingStrategySelectors,
    dataTable,
    gridOrderingActions,
    gridOrderingSelectors,
    dataLoader,
    dataStructureEntityId,
    modelInstanceId,

    formView,
    formSetup,
    formTopology,
    formActions
  };
}*/
/*
function fieldsFromProperties(properties: any[]) {
  return properties.map((property: any, idx: number) => {
    return new DataTableField({
      id: property.id,
      label: property.name,
      type: IFieldType.string,
      dataIndex: idx,
      recvDataIndex: property.recvDataIndex,
      isPrimaryKey: property.isPrimaryKey,
      isLookedUp: Boolean(property.lookupId),
      lookupId: property.lookupId,
      lookupIdentifier: property.lookupIdentifier,
      dropdownColumns: property.dropdownColumns
    });
  });
}
*/

class DataTableSelectors implements IDataTableSelectors {
  public recordById(id: string): IRecord | undefined {
    throw new Error("Method not implemented.");
  }

  public fieldById(id: string): IField | undefined {
    throw new Error("Method not implemented.");
  }

  public recordByIndex(idx: number): IRecord | undefined {
    throw new Error("Method not implemented.");
  }

  public fieldByIndex(idx: number): IField | undefined {
    throw new Error("Method not implemented.");
  }

  public recordIndexById(id: string): number | undefined {
    return parseInt(id, 10);
  }

  public fieldIndexById(id: string): number | undefined {
    return parseInt(id, 10);
  }

  public recordIdByIndex(idx: number): string | undefined {
    return `${idx}`;
  }

  public fieldIdByIndex(idx: number): string | undefined {
    return `${idx}`;
  }

  public recordIdAfterId(id: string): string | undefined {
    return `${parseInt(id, 10) + 1}`;
  }
  public fieldIdAfterId(id: string): string | undefined {
    return `${parseInt(id, 10) + 1}`;
  }

  public recordIdBeforeId(id: string): string | undefined {
    return `${parseInt(id, 10) - 1}`;
  }

  public fieldIdBeforeId(id: string): string | undefined {
    return `${parseInt(id, 10) - 1}`;
  }
}

@inject("mainView")
@observer
export class DataView extends React.Component<IDataViewProps>
  implements IDataViewState {
  constructor(props: any) {
    super(props);
    /*const { mainView } = props as { mainView: IMainView & ILoadingGate };
    const fields = fieldsFromProperties(props.properties);
    this.gridPaneBacking = createGridPaneBacking(
      props.dataSource.dataStructureEntityId,
      fields,
      props.initialView,
      mainView.id,
      props.modelInstanceId
    );
    mainView.componentBindingsModel.registerGridPaneBacking(
      this.gridPaneBacking
    );
    this.gridPaneBacking.dataLoadingStrategyActions.addLoadingGate(mainView);*/

    this.dataTableSelectors = new DataTableSelectors();
    this.dataCursorState = new DataCursorState(this.dataTableSelectors);

    this.setActiveView(this.props.initialView);
    this.dataCursorState.selectCell("5", "9");
  }

  private dataTableSelectors: IDataTableSelectors;
  private dataCursorState: IDataCursorState;

  @observable public activeView: IDataViewType;

  @action.bound public setActiveView(view: IDataViewType) {
    console.log(view);
    this.activeView = view;
  }

  public componentDidMount() {
    /*this.gridPaneBacking.onStartGrid.trigger();
    this.gridPaneBacking.dataLoadingStrategyActions
      .requestLoadFresh()
      .then(() => {
        this.gridPaneBacking.gridInteractionActions.selectFirst();
      });*/
  }

  public render() {
    return (
      <Provider
        dataTableSelectors={this.dataTableSelectors}
        dataCursorState={this.dataCursorState}
        dataViewState={this}
      >
        <div className="data-view-container">
          <GridToolbar />
          {this.props.children}
        </div>
      </Provider>
    );
  }
}
