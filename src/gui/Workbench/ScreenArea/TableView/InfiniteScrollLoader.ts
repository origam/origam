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

import {IGridDimensions} from "gui/Components/ScreenElements/Table/types";
import {SimpleScrollState} from "gui/Components/ScreenElements/Table/SimpleScrollState";
import {action, autorun, computed, flow, IReactionDisposer, reaction} from "mobx";
import {getApi} from "model/selectors/getApi";
import {getFormScreenLifecycle} from "model/selectors/FormScreen/getFormScreenLifecycle";
import {getMenuItemId} from "model/selectors/getMenuItemId";
import {getSessionId} from "model/selectors/getSessionId";
import {getDataStructureEntityId} from "model/selectors/DataView/getDataStructureEntityId";
import {getColumnNamesToLoad} from "model/selectors/DataView/getColumnNamesToLoad";
import {joinWithAND} from "model/entities/OrigamApiHelpers";
import {getUserFilters} from "model/selectors/DataView/getUserFilters";
import {getUserOrdering} from "model/selectors/DataView/getUserOrdering";
import {IVisibleRowsMonitor, OpenGroupVisibleRowsMonitor} from "./VisibleRowsMonitor";
import {ScrollRowContainer} from "model/entities/ScrollRowContainer";
import {CancellablePromise} from "mobx/lib/api/flow";
import { getUserFilterLookups } from "model/selectors/DataView/getUserFilterLookups";
import {getDataView} from "../../../../model/selectors/DataView/getDataView";

export interface IInfiniteScrollLoaderData {
  gridDimensions: IGridDimensions;
  scrollState: SimpleScrollState;
  ctx: any;
  rowsContainer: ScrollRowContainer;
  groupFilter: string | undefined;
  visibleRowsMonitor: IVisibleRowsMonitor;
}

export interface IInfiniteScrollLoader extends IInfiniteScrollLoaderData {
  start(): () => void;
  dispose(): void;
  loadLastPage(): Generator;
  loadFirstPage(): Generator;
}

export const SCROLL_ROW_CHUNK = 1000;
export const MAX_CHUNKS_TO_HOLD = 10;

export class NullIScrollLoader implements IInfiniteScrollLoader {
  ctx: any = null as any;
  gridDimensions: IGridDimensions = null as any;
  scrollState: SimpleScrollState = null as any;
  rowsContainer: ScrollRowContainer = null as any;

  start(): () => void {
    return () => {
    };
  }

  dispose(): void {
  }

  groupFilter: string | undefined;
  visibleRowsMonitor: IVisibleRowsMonitor = null as any;

  *loadLastPage(): Generator {
  }

  *loadFirstPage(): Generator {
  }
}

export class InfiniteScrollLoader implements IInfiniteScrollLoader {
  $type_InfiniteScrollLoader: 1 = 1;
  private debugDisposer: IReactionDisposer | undefined;
  private reactionDisposer: IReactionDisposer | undefined;
  visibleRowsMonitor: IVisibleRowsMonitor;

  constructor(data: IInfiniteScrollLoaderData) {
    Object.assign(this, data);
    this.rowsContainer.registerResetListener(() => this.handleRowContainerReset());
    this.visibleRowsMonitor = new OpenGroupVisibleRowsMonitor(this.ctx, this.gridDimensions, this.scrollState);
    this.requestProcessor.start();
  }

  *loadLastPage(){
    const api = getApi(this.ctx);
    const formScreenLifecycle = getFormScreenLifecycle(this.ctx);
    let dataView = getDataView(this.ctx);
    yield formScreenLifecycle.updateTotalRowCount(dataView);
    if(dataView.totalRowCount === undefined){
      return;
    }
    const rowsInLastChunk = (dataView.totalRowCount! % SCROLL_ROW_CHUNK) + SCROLL_ROW_CHUNK;
    const lastStartOffset =  dataView.totalRowCount! - rowsInLastChunk;

    const data = yield api.getRows({
      MenuId: getMenuItemId(this.ctx),
      SessionFormIdentifier: getSessionId(formScreenLifecycle),
      DataStructureEntityId: getDataStructureEntityId(this.ctx),
      Filter: this.getFilters(),
      FilterLookups: getUserFilterLookups(this.ctx),
      Ordering: getUserOrdering(this.ctx),
      RowLimit: SCROLL_ROW_CHUNK * 2,
      RowOffset: lastStartOffset,
      Parameters: {},
      MasterRowId: undefined,
      ColumnNames: getColumnNamesToLoad(this.ctx),
    })
    this.rowsContainer.set(data, lastStartOffset, true)

    this.reactionDisposer?.();
    setTimeout(()=>{
      const newDistanceToStart = this.distanceToStart + SCROLL_ROW_CHUNK
      const newTop = this.gridDimensions.getRowTop(newDistanceToStart);
      this.scrollState.scrollTo({scrollTop: newTop});
      this.start();
    });
  }

  *loadFirstPage(){
    const api = getApi(this.ctx);
    const formScreenLifecycle = getFormScreenLifecycle(this.ctx);

    const data = yield api.getRows({
      MenuId: getMenuItemId(this.ctx),
      SessionFormIdentifier: getSessionId(formScreenLifecycle),
      DataStructureEntityId: getDataStructureEntityId(this.ctx),
      Filter: this.getFilters(),
      FilterLookups: getUserFilterLookups(this.ctx),
      Ordering: getUserOrdering(this.ctx),
      RowLimit: SCROLL_ROW_CHUNK ,
      RowOffset: 0,
      Parameters: {},
      MasterRowId: undefined,
      ColumnNames: getColumnNamesToLoad(this.ctx),
    })
    this.rowsContainer.set(data)

    this.reactionDisposer?.();
    setTimeout(()=>{
      const newTop = this.gridDimensions.getRowTop(0);
      this.scrollState.scrollTo({scrollTop: newTop});
      this.start();
    });
  }

  groupFilter: string | undefined;
  lastRequestedStartOffset: number = 0;
  lastRequestedEndOffset: number = 0;
  gridDimensions: IGridDimensions = null as any;
  scrollState: SimpleScrollState = null as any;
  ctx: any = null as any;
  rowsContainer: ScrollRowContainer = null as any;
  requestProcessor: FlowQueueProcessor = new FlowQueueProcessor(100, 100);

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
    return this.distanceToStart <= SCROLL_ROW_CHUNK * 0.9 && !this.rowsContainer.isFirstRowLoaded;
  }

  @computed
  get tailLoadingNeeded() {
    return this.distanceToEnd <= SCROLL_ROW_CHUNK * 0.9 && !this.rowsContainer.isLastRowLoaded;
  }

  @computed
  get incrementLoadingNeeded() {
    return this.headLoadingNeeded || this.tailLoadingNeeded;
  }

  @action.bound
  public start() {
    this.debugDisposer = autorun(() => {
      //console.log("distanceToStart: " + this.distanceToStart);
      //console.log("distanceToEnd: " + this.distanceToEnd);
      //console.log("headLoadingNeeded(): " + this.headLoadingNeeded);
      //console.log("tailLoadingNeeded(): " + this.tailLoadingNeeded);
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
          this.requestProcessor.enqueue(() => this.prependLines());
        }
        if (this.tailLoadingNeeded) {
          this.requestProcessor.enqueue(() => this.appendLines());
        }
      }
    );
    return this.reactionDisposer;
  }

  prependListeners: ((data: any[][]) => void)[] = [];

  registerPrependListener(listener: (data: any[][])=> void): void{
    this.prependListeners.push(listener);
  }

  appendListeners: ((data: any[][]) => void)[] = [];

  registerAppendListener(listener: (data: any[][])=> void): void{
    this.appendListeners.push(listener);
  }

  handleRowContainerReset() {
    this.lastRequestedStartOffset = 0;
    this.lastRequestedEndOffset = 0;
  }

  getFilters() {
    const filters = [];
    if (this.groupFilter) {
      filters.push(this.groupFilter);
    }
    const userFilters = getUserFilters({ctx: this.ctx});
    if (userFilters) {
      filters.push(userFilters);
    }
    return joinWithAND(filters);
  }

  private appendLines = flow(function* (
    this: InfiniteScrollLoader
  ) {
    if (!this.tailLoadingNeeded) {
      return;
    }
    if (this.lastRequestedEndOffset === this.rowsContainer.nextEndOffset) {
      return;
    }
    this.lastRequestedEndOffset = this.rowsContainer.nextEndOffset;
    this.lastRequestedStartOffset = -1;

    const api = getApi(this.ctx);
    const formScreenLifecycle = getFormScreenLifecycle(this.ctx);

    const data = yield api.getRows({
      MenuId: getMenuItemId(this.ctx),
      SessionFormIdentifier: getSessionId(formScreenLifecycle),
      DataStructureEntityId: getDataStructureEntityId(this.ctx),
      Filter: this.getFilters(),
      FilterLookups: getUserFilterLookups(this.ctx),
      Ordering: getUserOrdering(this.ctx),
      RowLimit: SCROLL_ROW_CHUNK,
      MasterRowId: undefined,
      Parameters: {},
      RowOffset: this.rowsContainer.nextEndOffset,
      ColumnNames: getColumnNamesToLoad(this.ctx),
    })
    const oldDistanceToStart = this.distanceToStart;
    this.rowsContainer.appendRecords(data)
    if (this.rowsContainer.isFull && !this.rowsContainer.isLastRowLoaded) {
      const newDistanceToStart = oldDistanceToStart - SCROLL_ROW_CHUNK
      const newTop = this.gridDimensions.getRowTop(newDistanceToStart);
      this.scrollState.scrollTo({scrollTop: newTop});
    }
    this.appendListeners.forEach(listener => listener(data));
  });


  private prependLines = flow(function* (
    this: InfiniteScrollLoader
  ) {
    if (!this.headLoadingNeeded) {
      return;
    }
    const nextStartOffset = this.rowsContainer.nextStartOffset;
    if (this.lastRequestedStartOffset === nextStartOffset || nextStartOffset < 0) {
      return;
    }
    this.lastRequestedStartOffset = nextStartOffset;
    this.lastRequestedEndOffset = 0;

    const api = getApi(this.ctx);
    const formScreenLifecycle = getFormScreenLifecycle(this.ctx);
    const data = yield api.getRows({
      MenuId: getMenuItemId(this.ctx),
      SessionFormIdentifier: getSessionId(formScreenLifecycle),
      DataStructureEntityId: getDataStructureEntityId(this.ctx),
      Filter: this.getFilters(),
      FilterLookups: getUserFilterLookups(this.ctx),
      Ordering: getUserOrdering(this.ctx),
      RowLimit: SCROLL_ROW_CHUNK,
      RowOffset: nextStartOffset,
      Parameters: {},
      MasterRowId: undefined,
      ColumnNames: getColumnNamesToLoad(this.ctx),
    })
    const oldDistanceToStart = this.distanceToStart;
    this.rowsContainer.prependRecords(data);
    if (!this.rowsContainer.isFirstRowLoaded) {
      const newDistanceToStart = oldDistanceToStart + SCROLL_ROW_CHUNK
      const newTop = this.gridDimensions.getRowTop(newDistanceToStart);
      this.scrollState.scrollTo({scrollTop: newTop});
    }
    this.prependListeners.forEach(listener => listener(data));
  });

  dispose(): void {
    if (this.reactionDisposer) {
      this.reactionDisposer();
    }
    if (this.debugDisposer) {
      this.debugDisposer();
    }
    this.requestProcessor.dispose();
    this.prependListeners.length = 0;
    this.appendListeners.length = 0;
  }
}

// A better implementation:
// https://medium.com/@karenmarkosyan/how-to-manage-promises-into-dynamic-queue-with-vanilla-javascript-9d0d1f8d4df5
class FlowQueueProcessor {

  private flowQueue: (() => CancellablePromise<void>)[] = [];
  private stopRequested: boolean = false;
  private readonly delayBetweenCallsMillis: number;
  private readonly mainLoopDelayMills: number;
  private timeoutHandle: any;

  constructor(mainLoopDelayMilliseconds: number, delayBetweenCallsMilliseconds: number) {
    this.mainLoopDelayMills = mainLoopDelayMilliseconds;
    this.delayBetweenCallsMillis = delayBetweenCallsMilliseconds;
  }

  private sleep(milliseconds: number) {
    return new Promise(resolve => {
      this.timeoutHandle = setTimeout(resolve, milliseconds);
    });
  }

  enqueue(flow: () => CancellablePromise<void>) {
    this.flowQueue.push(flow);
  }

  dispose() {
    this.stopRequested = true;
    clearTimeout(this.timeoutHandle);
  }

  async start() {
    while (true) {
      while (this.flowQueue.length > 0) {
        const currentFlow = this.flowQueue.shift()!;
        await currentFlow();
        await this.sleep(this.delayBetweenCallsMillis);
      }
      await this.sleep(this.mainLoopDelayMills);
      if (this.stopRequested) {
        return;
      }
    }
  };
}

export const isInfiniteScrollLoader = (o: any): o is InfiniteScrollLoader =>
  o?.$type_InfiniteScrollLoader;

