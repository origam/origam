import {IGridDimensions} from "../../../Components/ScreenElements/Table/types";
import {SimpleScrollState} from "../../../Components/ScreenElements/Table/SimpleScrollState";
import {action, autorun, computed, flow, IReactionDisposer, reaction} from "mobx";
import {getApi} from "../../../../model/selectors/getApi";
import {getFormScreenLifecycle} from "../../../../model/selectors/FormScreen/getFormScreenLifecycle";
import {getMenuItemId} from "../../../../model/selectors/getMenuItemId";
import {getSessionId} from "../../../../model/selectors/getSessionId";
import {getDataStructureEntityId} from "../../../../model/selectors/DataView/getDataStructureEntityId";
import {getColumnNamesToLoad} from "../../../../model/selectors/DataView/getColumnNamesToLoad";
import {ScrollRowContainer} from "../../../../model/entities/RowsContainer";
import {joinWithAND} from "../../../../model/entities/OrigamApiHelpers";
import {getUserFilters} from "../../../../model/selectors/DataView/getUserFilters";
import {getUserOrdering} from "../../../../model/selectors/DataView/getUserOrdering";
import {IVisibleRowsMonitor, OpenGroupVisibleRowsMonitor} from "./VisibleRowsMonitor";

export interface IInfiniteScrollLoaderData{
  gridDimensions: IGridDimensions;
  scrollState: SimpleScrollState;
  ctx: any;
  rowsContainer: ScrollRowContainer;
  groupFilter: string | undefined;
  visibleRowsMonitor: IVisibleRowsMonitor;
}

export interface  IInfiniteScrollLoader extends IInfiniteScrollLoaderData{
  start(): ()=>void;
  dispose(): void;
}

export const SCROLL_DATA_INCREMENT_SIZE = 100;

export class NullIScrollLoader implements IInfiniteScrollLoader{
  ctx: any = null as any;
  gridDimensions: IGridDimensions = null as any;
  scrollState: SimpleScrollState = null as any;
  rowsContainer: ScrollRowContainer =  null as any;
  start(): ()=>void {
    return ()=>{};
  }

  dispose(): void {
  }

  groupFilter: string | undefined;
  visibleRowsMonitor: IVisibleRowsMonitor = null as any;
}

export class InfiniteScrollLoader implements IInfiniteScrollLoader {
  private debugDisposer: IReactionDisposer | undefined;
  private reactionDisposer: IReactionDisposer | undefined;
  visibleRowsMonitor: IVisibleRowsMonitor;

  constructor(data: IInfiniteScrollLoaderData) {
    Object.assign(this, data);
    this.rowsContainer.registerResetListener(() => this.handleRowContainerReset());
    this.visibleRowsMonitor = new OpenGroupVisibleRowsMonitor(this.ctx, this.gridDimensions, this.scrollState);
  }

  groupFilter: string | undefined;
  lastRequestedStartOffset: number = 0;
  lastRequestedEndOffset: number = 0;
  gridDimensions: IGridDimensions = null as any;
  scrollState: SimpleScrollState = null as any;
  ctx: any = null as any;
  rowsContainer: ScrollRowContainer = null as any;

  @computed
  get distanceToStart() {
    return this.visibleRowsMonitor.firstIndex;
  }

  @computed
  get distanceToEnd() {
    return this.rowsContainer.rows.length - this.visibleRowsMonitor.lastIndex;
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
    this.debugDisposer =  autorun(() => {
      console.log("distanceToStart: " + this.distanceToStart);
      console.log("distanceToEnd: " + this.distanceToEnd);
      console.log("headLoadingNeeded(): " + this.headLoadingNeeded);
      console.log("tailLoadingNeeded(): " + this.tailLoadingNeeded);
    });
    this.reactionDisposer = reaction(
      () => {
        return [
          this.visibleRowsMonitor.firstIndex,
          this.visibleRowsMonitor.lastIndex,
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
    return this.reactionDisposer;
  }

  handleRowContainerReset(){
    this.lastRequestedStartOffset = 0;
    this.lastRequestedEndOffset = 0;
  }

  getFilters(){
    const filters=[];
    if(this.groupFilter){
      filters.push(this.groupFilter);
    }
    const userFilters = getUserFilters(this.ctx);
    if(userFilters) {
      filters.push(userFilters);
    }
    return joinWithAND(filters);
  }

  private appendLines = flow(function* (
    this: InfiniteScrollLoader
  ) {
    console.log("appendLines!");
    const api = getApi(this.ctx);
    const formScreenLifecycle = getFormScreenLifecycle(this.ctx);

    if (this.lastRequestedEndOffset === this.rowsContainer.nextEndOffset) {
      return;
    }
    this.lastRequestedEndOffset = this.rowsContainer.nextEndOffset;
    this.lastRequestedStartOffset = -1;

    api.getRows({
      MenuId: getMenuItemId(this.ctx),
      SessionFormIdentifier: getSessionId(formScreenLifecycle),
      DataStructureEntityId: getDataStructureEntityId(this.ctx),
      Filter: this.getFilters(),
      Ordering: getUserOrdering(this.ctx),
      RowLimit: SCROLL_DATA_INCREMENT_SIZE,
      RowOffset: this.rowsContainer.nextEndOffset,
      ColumnNames: getColumnNamesToLoad(this.ctx),
      MasterRowId: undefined
    }).then(data => this.rowsContainer.appendRecords(data));
  });


  private prependLines = flow(function* (
    this: InfiniteScrollLoader
  ) {
    console.log("prependLines!");
    const api = getApi(this.ctx);
    const formScreenLifecycle = getFormScreenLifecycle(this.ctx);

    if (this.lastRequestedStartOffset === this.rowsContainer.nextStartOffset) {
      return;
    }
    this.lastRequestedStartOffset = this.rowsContainer.nextStartOffset;
    this.lastRequestedEndOffset = 0;

    api.getRows({
      MenuId: getMenuItemId(this.ctx),
      SessionFormIdentifier: getSessionId(formScreenLifecycle),
      DataStructureEntityId: getDataStructureEntityId(this.ctx),
      Filter: this.getFilters(),
      Ordering: getUserOrdering(this.ctx),
      RowLimit: SCROLL_DATA_INCREMENT_SIZE,
      RowOffset: this.rowsContainer.nextStartOffset,
      ColumnNames: getColumnNamesToLoad(this.ctx),
      MasterRowId: undefined
    }).then(data => this.rowsContainer.prependRecords(data));
  });

  dispose(): void {
    if(this.reactionDisposer){
      this.reactionDisposer();
    }
    if(this.debugDisposer){
      this.debugDisposer();
    }
  }
}
