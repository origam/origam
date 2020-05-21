import {IGridDimensions} from "../../../Components/ScreenElements/Table/types";
import {SimpleScrollState} from "../../../Components/ScreenElements/Table/SimpleScrollState";
import {IDataView} from "../../../../model/entities/types/IDataView";
import {action, autorun, computed, flow, observable, reaction} from "mobx";
import {BoundingRect} from "react-measure";
import {rangeQuery} from "../../../../utils/arrays";
import {getDataTable} from "../../../../model/selectors/DataView/getDataTable";
import {getApi} from "../../../../model/selectors/getApi";
import {getFormScreenLifecycle} from "../../../../model/selectors/FormScreen/getFormScreenLifecycle";
import {getOrderingConfiguration} from "../../../../model/selectors/DataView/getOrderingConfiguration";
import {getProperties} from "../../../../model/selectors/DataView/getProperties";
import {IOrderByDirection} from "../../../../model/entities/types/IOrderingConfiguration";
import {getMenuItemId} from "../../../../model/selectors/getMenuItemId";
import {getSessionId} from "../../../../model/selectors/getSessionId";
import {getDataStructureEntityId} from "../../../../model/selectors/DataView/getDataStructureEntityId";
import {getColumnNamesToLoad} from "../../../../model/selectors/DataView/getColumnNamesToLoad";

export interface IInfiniteScrollLoaderData{
  gridDimensions: IGridDimensions;
  scrollState: SimpleScrollState;
  dataView: IDataView;
}

export interface  IInfiniteScrollLoader extends IInfiniteScrollLoaderData{
  contentBounds: BoundingRect | undefined;
  start(): any;
}

export const SCROLL_DATA_INCREMENT_SIZE = 100;

export class InfiniteScrollLoader implements IInfiniteScrollLoader {

  constructor(data: IInfiniteScrollLoaderData) {
    Object.assign(this, data);
  }

  lastRequestedStartOffset: number = -1;
  lastRequestedEndOffset: number = 0;
  gridDimensions: IGridDimensions = null as any;
  scrollState: SimpleScrollState = null as any;
  dataView: IDataView = null as any;
  @observable contentBounds: BoundingRect | undefined;

  @computed
  get visibleRowsRange() {
    return rangeQuery(
      (i) => this.gridDimensions.getRowBottom(i),
      (i) => this.gridDimensions.getRowTop(i),
      this.gridDimensions.rowCount,
      this.scrollState.scrollTop,
      this.scrollState.scrollTop + (this.contentBounds && this.contentBounds.height || 0)
    );
  }

  @computed
  get visibleRowsFirstIndex() {
    return this.visibleRowsRange.fgte;
  }

  @computed
  get visibleRowsLastIndex() {
    return this.visibleRowsRange.llte;
  }

  @computed
  get distanceToStart() {
    return this.visibleRowsFirstIndex;
  }

  @computed
  get distanceToEnd() {
    return getDataTable(this.dataView).rows.length - this.visibleRowsLastIndex;
  }

  @computed
  get headLoadingNeeded() {
    return this.distanceToStart <= SCROLL_DATA_INCREMENT_SIZE && !getDataTable(this.dataView).isFirstLoaded;
  }

  @computed
  get tailLoadingNeeded() {
    return this.distanceToEnd <= SCROLL_DATA_INCREMENT_SIZE && !getDataTable(this.dataView).isLastLoaded;
  }

  @computed
  get incrementLoadingNeeded() {
    return this.headLoadingNeeded || this.tailLoadingNeeded;
  }

  @action.bound
  public start() {
    autorun(() => {
      console.log("firstLine: " + this.visibleRowsRange.fgte);
      console.log("lastLine: " + this.visibleRowsRange.llte);
      console.log("headLoadingNeeded(): " + this.headLoadingNeeded);
      console.log("tailLoadingNeeded(): " + this.tailLoadingNeeded);
    });
    return reaction(
      () => {
        return [
          this.visibleRowsFirstIndex,
          this.visibleRowsLastIndex,
          this.headLoadingNeeded,
          this.tailLoadingNeeded
        ];
      },
      () => {
        if (this.headLoadingNeeded) {
          this.prependLines();
        }
        if (this.tailLoadingNeeded) {
          this.appendLines();
        }
      }
    );
  }

  @observable
  public inLoading = 0;

  @computed
  public get isLoading() {
    return this.inLoading > 0;
  }

  private appendLines = flow(function* (
    this: InfiniteScrollLoader
  ) {
    console.log("appendLines!");
    const dataView = this.dataView;
    const api = getApi(dataView);
    const dataTable = getDataTable(dataView);
    const formScreenLifecycle = getFormScreenLifecycle(dataView);
    const orderingConfiguration = getOrderingConfiguration(dataView);
    const firstProperty = getProperties(dataView)[0];
    const ordering = orderingConfiguration.groupChildrenOrdering
      ? [[orderingConfiguration.groupChildrenOrdering.columnId, orderingConfiguration.groupChildrenOrdering.direction]]
      : [[firstProperty.id, IOrderByDirection.ASC]];

    if (this.lastRequestedEndOffset === dataTable.nextEndOffset) {
      return;
    }
    this.lastRequestedEndOffset = dataTable.nextEndOffset;
    this.lastRequestedStartOffset = -1;
    api.getRows({
      MenuId: getMenuItemId(dataView),
      SessionFormIdentifier: getSessionId(formScreenLifecycle),
      DataStructureEntityId: getDataStructureEntityId(dataView),
      Filter: "",
      Ordering: ordering,
      RowLimit: SCROLL_DATA_INCREMENT_SIZE,
      RowOffset: dataTable.nextEndOffset,
      ColumnNames: getColumnNamesToLoad(dataView),
      MasterRowId: undefined
    }).then(data => dataTable.appendRecords(data));
  });

  private prependLines = flow(function* (
    this: InfiniteScrollLoader
  ) {
    console.log("prependLines!");
    const dataView = this.dataView;
    const api = getApi(dataView);
    const dataTable = getDataTable(dataView);
    const formScreenLifecycle = getFormScreenLifecycle(dataView);
    const orderingConfiguration = getOrderingConfiguration(dataView);
    const firstProperty = getProperties(dataView)[0];
    const ordering = orderingConfiguration.groupChildrenOrdering
      ? [[orderingConfiguration.groupChildrenOrdering.columnId, orderingConfiguration.groupChildrenOrdering.direction]]
      : [[firstProperty.id, IOrderByDirection.ASC]];

    if (this.lastRequestedStartOffset === dataTable.nextStartOffset) {
      return;
    }
    this.lastRequestedStartOffset = dataTable.nextStartOffset;
    this.lastRequestedEndOffset = 0;
    api.getRows({
      MenuId: getMenuItemId(dataView),
      SessionFormIdentifier: getSessionId(formScreenLifecycle),
      DataStructureEntityId: getDataStructureEntityId(dataView),
      Filter: "",
      Ordering: ordering,
      RowLimit: SCROLL_DATA_INCREMENT_SIZE,
      RowOffset: dataTable.nextStartOffset,
      ColumnNames: getColumnNamesToLoad(dataView),
      MasterRowId: undefined
    }).then(data => dataTable.prependRecords(data));
  });
}