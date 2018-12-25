import { action, computed, autorun } from "mobx";
import { observer, Provider, inject } from "mobx-react";
import * as React from "react";
import { DataLoader } from "src/DataLoadingStrategy/DataLoader";
import { DataLoadingStrategyActions } from "src/DataLoadingStrategy/DataLoadingStrategyActions";
import { DataLoadingStrategySelectors } from "src/DataLoadingStrategy/DataLoadingStrategySelectors";
import { DataLoadingStrategyState } from "src/DataLoadingStrategy/DataLoadingStrategyState";
import { DataSaver } from "src/DataLoadingStrategy/DataSaver";
import { DataSavingStrategy } from "src/DataLoadingStrategy/DataSavingStrategy";
import { LookupResolverProvider } from "src/DataLoadingStrategy/LookupResolverProvider";
import { DataTableActions } from "src/DataTable/DataTableActions";
import { DataTableSelectors } from "src/DataTable/DataTableSelectors";
import { DataTableField, DataTableState } from "src/DataTable/DataTableState";
import {
  ICellValue,
  IDataTableFieldStruct,
  IDataTableSelectors,
  IFieldType
} from "src/DataTable/types";
import { CellScrolling } from "src/Grid/CellScrolling";
import { FormActions } from "src/Grid/FormActions";
import { FormState } from "src/Grid/FormState";
import { FormView } from "src/Grid/FormView";
import { GridActions } from "src/Grid/GridActions";
import { GridCursorView } from "src/Grid/GridCursorView";
import { GridInteractionActions } from "src/Grid/GridInteractionActions";
import { GridInteractionSelectors } from "src/Grid/GridInteractionSelectors";
import { GridInteractionState } from "src/Grid/GridInteractionState";
import { GridSelectors } from "src/Grid/GridSelectors";
import { GridState } from "src/Grid/GridState";
import { GridView } from "src/Grid/GridView";
import {
  IFormSetup,
  IFormTopology,
  IGridSetup,
  IGridTopology,
  GridViewType
} from "src/Grid/types";
import { GridOrderingActions } from "src/GridOrdering/GridOrderingActions";
import { GridOrderingSelectors } from "src/GridOrdering/GridOrderingSelectors";
import { GridOrderingState } from "src/GridOrdering/GridOrderingState";
import { GridOutlineActions } from "src/GridOutline/GridOutlineActions";
import { GridOutlineSelectors } from "src/GridOutline/GridOutlineSelectors";
import { GridOutlineState } from "src/GridOutline/GridOutlineState";
import { FormTopology } from "src/GridPanel/adapters/FormTopology";
import { GridSetup } from "src/GridPanel/adapters/GridSetup";
import { GridTopology } from "src/GridPanel/adapters/GridTopology";
import { GridToolbarView } from "src/GridPanel/GridToolbarView";
import { EventObserver } from "src/utils/events";
import { GridTable } from "../controls/GridTable";
import { GridToolbar } from "../controls/GridToolbar";
import { IGridPanelBacking } from "src/GridPanel/types";
import { GridForm } from "../controls/GridForm";
import { GridMap } from "../controls/GridMap";
import { api } from "src/DataLoadingStrategy/api";

import { ILoadingGate } from '../../DataLoadingStrategy/types';

import { IProperty } from "src/screenInterpreter/types";
import { IDataViewProps } from "./types";
import { IMainView } from "src/Application/types";

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
  dataStructureEntityId: string,
  dataTableFields: IDataTableFieldStruct[],
  defaultView: GridViewType,
  menuItemId: string,
  modelInstanceId: string,
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

  const dataTableState = new DataTableState();

  dataTableState.fields = dataTableFields;

  const dataTableSelectors = new DataTableSelectors(
    dataTableState,
    lookupResolverProvider,
    dataStructureEntityId
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
    configuration,
    dataTableSelectors
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
    gridActions,
    gridCursorView
  );
  onStartGrid(() => dataLoadingStrategyActions.start());
  onStopGrid(() => dataLoadingStrategyActions.stop());

  const dataSaver = new DataSaver(
    dataStructureEntityId,
    dataTableActions,
    dataTableSelectors,
    menuItemId
  );
  const dataSavingStrategy = new DataSavingStrategy(
    dataTableSelectors,
    dataTableActions,
    dataSaver,
    dataLoadingStrategyActions
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
    dataLoadingStrategySelectors,
    dataTableActions,
    dataTableSelectors,
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
}

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





@inject("mainView")
@observer
export class DataView extends React.Component<IDataViewProps> {
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
  }

  private gridPaneBacking: IGridPanelBacking;

  public componentDidMount() {
    /*this.gridPaneBacking.onStartGrid.trigger();
    this.gridPaneBacking.dataLoadingStrategyActions
      .requestLoadFresh()
      .then(() => {
        this.gridPaneBacking.gridInteractionActions.selectFirst();
      });*/
  }

  public render() {
    /*const { gridPaneBacking } = this;*/
    return (
      <Provider /*gridPaneBacking={this.gridPaneBacking}*/>
        <></>
       {/*
        <div
          className="oui-grid"
          style={{
            minWidth: this.props.w,
            maxWidth: this.props.w,
            maxHeight: this.props.h,
            minHeight: this.props.h
          }}
        >
          <GridToolbar
            name={this.props.name}
            isHidden={this.props.isHeadless}
            isAddButton={this.props.isShowAddButton}
            isDeleteButton={this.props.isShowDeleteButton}
            isCopyButton={this.props.isShowAddButton}
          />
          {this.props.table}
          {this.props.form}
          <GridMap />
          {/*<GridForm isActiveView={gridPaneBacking.gridInteractionSelectors.activeView === IGridPaneView.Form} reactTree={this.props.form} />*/}
          {/*this.props.children
        </div>*/}
      </Provider>
    );
  }
}
