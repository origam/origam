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

  get dataTable(){
    return getDataTable(this.dataView);
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
    return this.dataTable.rows.length - this.visibleRowsLastIndex;
  }

  @computed
  get headLoadingNeeded() {
    return this.distanceToStart <= SCROLL_DATA_INCREMENT_SIZE && !this.isFirstLoaded;
  }

  @computed
  get tailLoadingNeeded() {
    return this.distanceToEnd <= SCROLL_DATA_INCREMENT_SIZE && !this.isLastLoaded;
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

    if (this.lastRequestedEndOffset === this.nextEndOffset) {
      return;
    }
    this.lastRequestedEndOffset = this.nextEndOffset;
    this.lastRequestedStartOffset = -1;
    api.getRows({
      MenuId: getMenuItemId(dataView),
      SessionFormIdentifier: getSessionId(formScreenLifecycle),
      DataStructureEntityId: getDataStructureEntityId(dataView),
      Filter: "",
      Ordering: ordering,
      RowLimit: SCROLL_DATA_INCREMENT_SIZE,
      RowOffset: this.nextEndOffset,
      ColumnNames: getColumnNamesToLoad(dataView),
      MasterRowId: undefined
    }).then(data => this.appendRecords(data));
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

    if (this.lastRequestedStartOffset === this.nextStartOffset) {
      return;
    }
    this.lastRequestedStartOffset = this.nextStartOffset;
    this.lastRequestedEndOffset = 0;
    api.getRows({
      MenuId: getMenuItemId(dataView),
      SessionFormIdentifier: getSessionId(formScreenLifecycle),
      DataStructureEntityId: getDataStructureEntityId(dataView),
      Filter: "",
      Ordering: ordering,
      RowLimit: SCROLL_DATA_INCREMENT_SIZE,
      RowOffset: this.nextStartOffset,
      ColumnNames: getColumnNamesToLoad(dataView),
      MasterRowId: undefined
    }).then(data => this.prependRecords(data));
  });

  @observable
  firstRowOffset: number = 0;
  lastRowOffset: number = 0;
  rowChunkSize: number = SCROLL_DATA_INCREMENT_SIZE;
  maxRowsSize = 3 * SCROLL_DATA_INCREMENT_SIZE;

  @observable
  lastRowChunkSize: number = 0;

  @observable
  isEndLoaded=false;

  get nextEndOffset(){
    return this.lastRowOffset + this.rowChunkSize;
  }

  get nextStartOffset(){
    return this.firstRowOffset - this.rowChunkSize;
  }

  @action.bound
  setRecords(rows: any[][]) {
    this.firstRowOffset = 0;
    this.lastRowOffset = 0;
    this.lastRowChunkSize = rows.length;
    this.dataTable.rowsContainer.set(rows);
  }

  @action.bound
  prependRecords(rows: any[][]) {
    this.firstRowOffset -= this.rowChunkSize;
    this.dataTable.rowsContainer.rows.unshift(...rows);
    if(this.dataTable.rowsContainer.rows.length > this.maxRowsSize){
      this.removeRowsFromEnd();
      this.isEndLoaded = false;
    }
  }

  private removeRowsFromEnd() {
    this.dataTable.rowsContainer.rows.splice(this.dataTable.rowsContainer.rows.length - this.lastRowChunkSize, this.lastRowChunkSize);
    this.lastRowOffset -=  this.lastRowChunkSize;
    this.lastRowChunkSize = this.rowChunkSize;
  }


  @action.bound
  appendRecords(rows: any[][]) {
    this.lastRowOffset += rows.length;
    this.lastRowChunkSize = rows.length;
    this.dataTable.rowsContainer.rows.push(...rows);
    if(this.dataTable.rowsContainer.rows.length > this.maxRowsSize){
      this.removeRowsFromStart();
    }
    if(rows.length < this.rowChunkSize){
      this.isEndLoaded = true;
    }
  }

  private removeRowsFromStart() {
    this.dataTable.rowsContainer.rows.splice(0, this.rowChunkSize);
    this.firstRowOffset += this.rowChunkSize;
  }

  @computed
  get isLastLoaded(){
    return this.isEndLoaded;
  }

  @computed
  get isFirstLoaded(){
    return this.firstRowOffset === 0;
  }
}

// prependRecords(rows: any[][]): void;
// appendRecords(rows: any[][]): void;
// isLastLoaded: boolean;
// isFirstLoaded: boolean;
// nextEndOffset: number;
// nextStartOffset: number;