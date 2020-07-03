import {IGridDimensions} from "../../../Components/ScreenElements/Table/types";
import {SimpleScrollState} from "../../../Components/ScreenElements/Table/SimpleScrollState";
import {action, autorun, computed, flow, IReactionDisposer, reaction} from "mobx";
import {getApi} from "../../../../model/selectors/getApi";
import {getFormScreenLifecycle} from "../../../../model/selectors/FormScreen/getFormScreenLifecycle";
import {getMenuItemId} from "../../../../model/selectors/getMenuItemId";
import {getSessionId} from "../../../../model/selectors/getSessionId";
import {getDataStructureEntityId} from "../../../../model/selectors/DataView/getDataStructureEntityId";
import {getColumnNamesToLoad} from "../../../../model/selectors/DataView/getColumnNamesToLoad";
import {joinWithAND} from "../../../../model/entities/OrigamApiHelpers";
import {getUserFilters} from "../../../../model/selectors/DataView/getUserFilters";
import {getUserOrdering} from "../../../../model/selectors/DataView/getUserOrdering";
import {IVisibleRowsMonitor, OpenGroupVisibleRowsMonitor} from "./VisibleRowsMonitor";
import {ScrollRowContainer} from "../../../../model/entities/ScrollRowContainer";
import {CancellablePromise} from "mobx/lib/api/flow";


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
}

export class InfiniteScrollLoader implements IInfiniteScrollLoader {
  private debugDisposer: IReactionDisposer | undefined;
  private reactionDisposer: IReactionDisposer | undefined;
  visibleRowsMonitor: IVisibleRowsMonitor;

  constructor(data: IInfiniteScrollLoaderData) {
    Object.assign(this, data);
    this.rowsContainer.registerResetListener(() => this.handleRowContainerReset());
    this.visibleRowsMonitor = new OpenGroupVisibleRowsMonitor(this.ctx, this.gridDimensions, this.scrollState);
    this.requestProcessor.start();
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

  handleRowContainerReset() {
    this.lastRequestedStartOffset = 0;
    this.lastRequestedEndOffset = 0;
  }

  getFilters() {
    const filters = [];
    if (this.groupFilter) {
      filters.push(this.groupFilter);
    }
    const userFilters = getUserFilters(this.ctx);
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
    debugger;
    const data = yield api.getRows({
      MenuId: getMenuItemId(this.ctx),
      SessionFormIdentifier: getSessionId(formScreenLifecycle),
      DataStructureEntityId: getDataStructureEntityId(this.ctx),
      Filter: this.getFilters(),
      Ordering: getUserOrdering(this.ctx),
      RowLimit: SCROLL_ROW_CHUNK,
      RowOffset: this.rowsContainer.nextEndOffset,
      ColumnNames: getColumnNamesToLoad(this.ctx),
      MasterRowId: undefined
    })
    const oldDistanceToStart = this.distanceToStart;
    this.rowsContainer.appendRecords(data)
    if (this.rowsContainer.isFull && !this.rowsContainer.isLastRowLoaded) {
      const newDistanceToStart = oldDistanceToStart - SCROLL_ROW_CHUNK
      const newTop = this.gridDimensions.getRowTop(newDistanceToStart);
      this.scrollState.scrollTo({scrollTop: newTop});
    }
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
    debugger;
    const data = yield api.getRows({
      MenuId: getMenuItemId(this.ctx),
      SessionFormIdentifier: getSessionId(formScreenLifecycle),
      DataStructureEntityId: getDataStructureEntityId(this.ctx),
      Filter: this.getFilters(),
      Ordering: getUserOrdering(this.ctx),
      RowLimit: SCROLL_ROW_CHUNK,
      RowOffset: nextStartOffset,
      ColumnNames: getColumnNamesToLoad(this.ctx),
      MasterRowId: undefined
    })
    const oldDistanceToStart = this.distanceToStart;
    this.rowsContainer.prependRecords(data);
    if (this.rowsContainer.isFull && !this.rowsContainer.isFirstRowLoaded) {
      const newDistanceToStart = oldDistanceToStart + SCROLL_ROW_CHUNK
      const newTop = this.gridDimensions.getRowTop(newDistanceToStart);
      this.scrollState.scrollTo({scrollTop: newTop});
    }
  });

  dispose(): void {
    if (this.reactionDisposer) {
      this.reactionDisposer();
    }
    if (this.debugDisposer) {
      this.debugDisposer();
    }
    this.requestProcessor.dispose();
  }
}

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
