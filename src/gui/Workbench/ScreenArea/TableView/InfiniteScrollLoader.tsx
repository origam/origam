import {IGridDimensions} from "../../../Components/ScreenElements/Table/types";
import {SimpleScrollState} from "../../../Components/ScreenElements/Table/SimpleScrollState";
import {IDataView} from "../../../../model/entities/types/IDataView";
import {action, autorun, computed, flow, observable, reaction} from "mobx";
import {BoundingRect} from "react-measure";
import {rangeQuery} from "../../../../utils/arrays";
import {getApi} from "../../../../model/selectors/getApi";
import {getFormScreenLifecycle} from "../../../../model/selectors/FormScreen/getFormScreenLifecycle";
import {getOrderingConfiguration} from "../../../../model/selectors/DataView/getOrderingConfiguration";
import {getMenuItemId} from "../../../../model/selectors/getMenuItemId";
import {getSessionId} from "../../../../model/selectors/getSessionId";
import {getDataStructureEntityId} from "../../../../model/selectors/DataView/getDataStructureEntityId";
import {getColumnNamesToLoad} from "../../../../model/selectors/DataView/getColumnNamesToLoad";
import {ScrollRowContainer} from "../../../../model/entities/RowsContainer";

export interface IInfiniteScrollLoaderData{
  gridDimensions: IGridDimensions;
  scrollState: SimpleScrollState;
  dataView: IDataView;
  rowsContainer: ScrollRowContainer;
}

export interface  IInfiniteScrollLoader extends IInfiniteScrollLoaderData{
  contentBounds: BoundingRect | undefined;
  start(): ()=>void;
}

export const SCROLL_DATA_INCREMENT_SIZE = 100;

export class NullIScrollLoader implements IInfiniteScrollLoader{
  contentBounds: BoundingRect | undefined;
  dataView: IDataView = null as any;
  gridDimensions: IGridDimensions = null as any;
  scrollState: SimpleScrollState = null as any;
  rowsContainer: ScrollRowContainer =  null as any;
  start(): ()=>void {
    return ()=>{};
  }
}

export class InfiniteScrollLoader implements IInfiniteScrollLoader {

  constructor(data: IInfiniteScrollLoaderData) {
    Object.assign(this, data);
    this.rowsContainer.registerResetListener(() => this.handleRowContainerReset());
  }

  lastRequestedStartOffset: number = 0;
  lastRequestedEndOffset: number = 0;
  gridDimensions: IGridDimensions = null as any;
  scrollState: SimpleScrollState = null as any;
  dataView: IDataView = null as any;
  rowsContainer: ScrollRowContainer = null as any;
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
    return this.visibleRowsRange.firstGreaterThanNumber;
  }

  @computed
  get visibleRowsLastIndex() {
    return this.visibleRowsRange.lastLessThanNumber;
  }

  @computed
  get distanceToStart() {
    return this.visibleRowsFirstIndex;
  }

  @computed
  get distanceToEnd() {
    return this.rowsContainer.rows.length - this.visibleRowsLastIndex;
  }

  @computed
  get headLoadingNeeded() {
    return this.distanceToStart <= SCROLL_DATA_INCREMENT_SIZE && !this.rowsContainer.isFirstLoaded;
  }

  @computed
  get tailLoadingNeeded() {
    return this.distanceToEnd <= SCROLL_DATA_INCREMENT_SIZE && !this.rowsContainer.isLastLoaded;
  }

  @computed
  get incrementLoadingNeeded() {
    return this.headLoadingNeeded || this.tailLoadingNeeded;
  }

  @action.bound
  public start() {
    autorun(() => {
      console.log("firstLine: " + this.visibleRowsRange.firstGreaterThanNumber);
      console.log("lastLine: " + this.visibleRowsRange.lastLessThanNumber);
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

  handleRowContainerReset(){
    this.lastRequestedStartOffset = 0;
    this.lastRequestedEndOffset = 0;
  }

  private appendLines = flow(function* (
    this: InfiniteScrollLoader
  ) {
    console.log("appendLines!");
    const api = getApi(this.dataView);
    const formScreenLifecycle = getFormScreenLifecycle(this.dataView);

    if (this.lastRequestedEndOffset === this.rowsContainer.nextEndOffset) {
      return;
    }
    this.lastRequestedEndOffset = this.rowsContainer.nextEndOffset;
    this.lastRequestedStartOffset = -1;

    api.getRows({
      MenuId: getMenuItemId(this.dataView),
      SessionFormIdentifier: getSessionId(formScreenLifecycle),
      DataStructureEntityId: getDataStructureEntityId(this.dataView),
      Filter: "",
      Ordering: this.getOrdering(this.dataView),
      RowLimit: SCROLL_DATA_INCREMENT_SIZE,
      RowOffset: this.rowsContainer.nextEndOffset,
      ColumnNames: getColumnNamesToLoad(this.dataView),
      MasterRowId: undefined
    }).then(data => this.rowsContainer.appendRecords(data));
  });


  private prependLines = flow(function* (
    this: InfiniteScrollLoader
  ) {
    console.log("prependLines!");
    const api = getApi(this.dataView);
    const formScreenLifecycle = getFormScreenLifecycle(this.dataView);

    if (this.lastRequestedStartOffset === this.rowsContainer.nextStartOffset) {
      return;
    }
    this.lastRequestedStartOffset = this.rowsContainer.nextStartOffset;
    this.lastRequestedEndOffset = 0;

    api.getRows({
      MenuId: getMenuItemId(this.dataView),
      SessionFormIdentifier: getSessionId(formScreenLifecycle),
      DataStructureEntityId: getDataStructureEntityId(this.dataView),
      Filter: "",
      Ordering: this.getOrdering(this.dataView),
      RowLimit: SCROLL_DATA_INCREMENT_SIZE,
      RowOffset: this.rowsContainer.nextStartOffset,
      ColumnNames: getColumnNamesToLoad(this.dataView),
      MasterRowId: undefined
    }).then(data => this.rowsContainer.prependRecords(data));
  });

  private getOrdering(dataView: IDataView) {
    const orderingConfiguration = getOrderingConfiguration(dataView);
    const defaultOrdering = orderingConfiguration.getDefaultOrdering();
    if(!defaultOrdering){
      const dataStructureEntityId = getDataStructureEntityId(this.dataView);
      throw new Error(`Cannot infinitely scroll on dataStructureEntity: ${dataStructureEntityId} because it has no default ordering. Is there a SortSet in it's parent DataStructure?`)
    }
    return orderingConfiguration.groupChildrenOrdering
      ? [orderingConfiguration.groupChildrenOrdering]
      : [defaultOrdering];
  }
}
