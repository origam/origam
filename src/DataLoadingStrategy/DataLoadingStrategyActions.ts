import { action, flow, reaction, IReactionDisposer } from "mobx";
import { DataTableRecord } from "../DataTable/DataTableState";
import { reactionRuntimeInfo } from "../utils/reaction";
import {
  IDataLoadingStategyState,
  IDataLoadingStrategySelectors,
  IDataLoader
} from "./types";
import {
  IDataTableActions,
  IDataTableSelectors,
  ICellValue
} from "../DataTable/types";
import {
  IGridOrderingSelectors,
  IGridOrderingActions
} from "../GridOrdering/types";
import { IGridOutlineSelectors } from "../GridOutline/types";
import {
  IGridInteractionActions,
  IGridSelectors,
  IGridActions
} from "../Grid/types";
import { CancellablePromise } from "mobx/lib/api/flow";

function noCancelException<T>(promise: Promise<void | T>): Promise<void | T> {
  return promise.catch(e => {
    if (e.message !== "FLOW_CANCELLED") {
      throw e;
    }
  });
}

export class DataLoadingStrategyActions {
  constructor(
    public state: IDataLoadingStategyState,
    public selectors: IDataLoadingStrategySelectors,
    public dataTableActions: IDataTableActions,
    public dataTableSelectors: IDataTableSelectors,
    public dataLoader: IDataLoader,
    public gridOrderingSelectors: IGridOrderingSelectors,
    public gridOrderingActions: IGridOrderingActions,
    public gridOutlineSelectors: IGridOutlineSelectors,
    public gridInteractionActions: IGridInteractionActions,
    public gridViewSelectors: IGridSelectors,
    public gridViewActions: IGridActions
  ) {}

  private loadingPromise: CancellablePromise<any> | undefined;
  private loadWaitingPromise: CancellablePromise<any> | undefined;
  private reOrdering: IReactionDisposer | undefined;
  private reOutline: IReactionDisposer | undefined;
  private reIncremental: IReactionDisposer | undefined;

  @action.bound
  public stop() {
    this.reOrdering && this.reOrdering();
    this.reOutline && this.reOutline();
    this.reIncremental && this.reIncremental();
  }

  @action.bound
  public start() {
    this.reOrdering = reaction(
      () => {
        return [
          this.gridOrderingSelectors.ordering.map(o => void (o[0], o[1]))
        ];
      },
      () => {
        if (reactionRuntimeInfo.info.has("LOAD_FRESH_AROUND")) {
          return;
        }
        this.requestLoadFresh();
      }
    );

    this.reOutline = reaction(
      () => {
        return [this.gridOutlineSelectors.lastSelectedItem];
      },
      () => {
        this.gridOutlineSelectors.lastSelectedItem &&
          this.requestLoadFreshAround(
            this.gridOutlineSelectors.lastSelectedItem
          );
      }
    );

    this.reIncremental = reaction(
      () => {
        return [
          this.gridViewSelectors.visibleRowsFirstIndex,
          this.gridViewSelectors.visibleRowsLastIndex,
          this.gridViewSelectors.rowCount,
          this.selectors.headLoadingNeeded,
          this.selectors.tailLoadingNeeded
        ];
      },
      () => {
        /*console.log(
          this.selectors.headLoadingNeeded,
          this.selectors.tailLoadingNeeded,
          this.selectors.incrementLoadingNeeded
        );*/
        if (!this.selectors.headLoadingNeeded) {
          this.state.setHeadLoadingActive(true);
        }
        if (!this.selectors.tailLoadingNeeded) {
          this.state.setTailLoadingActive(true);
        }
        if (this.selectors.incrementLoadingNeeded) {
          this.requestLoadIncrement();
        }
      }
    );
  }

  @action.bound
  public requestLoadIncrement() {
    return noCancelException(flow(this.loadIncrementProc.bind(this))());
  }

  private *loadIncrementProc() {
    if (!this.selectors.incrementLoadingNeeded) {
      return;
    }
    this.cancelLoadWaiting();
    yield this.waitForLoadingFinished();
    if (!this.selectors.incrementLoadingNeeded) {
      return;
    }
    if (this.selectors.headLoadingActive && this.selectors.headLoadingNeeded) {
      const gridHeightBeforeLoad = this.gridViewSelectors.contentHeight;
      const resultInfo = yield this.loadBeforeFirstRecord();
      const gridHeightAfterLoad = this.gridViewSelectors.contentHeight;
      this.gridViewActions.performIncrementScroll({
        scrollTop: gridHeightAfterLoad - gridHeightBeforeLoad
      });
      if (resultInfo.records.length < 5000) {
        this.state.setHeadLoadingActive(false);
      }
      if (this.selectors.recordsNeedTrimming) {
        this.trimTail();
      }
    } else if (
      this.selectors.tailLoadingActive &&
      this.selectors.tailLoadingNeeded
    ) {
      const resultInfo = yield this.loadAfterLastRecord();
      if (resultInfo.records.length < 5000) {
        this.state.setTailLoadingActive(false);
      }
      if (this.selectors.recordsNeedTrimming) {
        const gridHeightBeforeLoad = this.gridViewSelectors.contentHeight;
        this.trimHead();
        const gridHeightAfterLoad = this.gridViewSelectors.contentHeight;
        this.gridViewActions.performIncrementScroll({
          scrollTop: gridHeightAfterLoad - gridHeightBeforeLoad
        });
      }
    }
  }

  public loadAfterLastRecord() {
    return (this.loadingPromise = flow(
      this.loadAfterLastRecordProc.bind(this)
    )());
  }

  private *loadAfterLastRecordProc() {
    const lastRecord = this.dataTableSelectors.lastFullRecord;
    const addedFilters = [];
    let addedOrdering = [];
    if (this.gridOrderingSelectors.ordering.length === 0) {
      addedFilters.push(["id", "gt", lastRecord.id]);
    }
    for (const ordering of this.gridOrderingSelectors.ordering) {
      const field = this.dataTableSelectors.getFieldById(ordering[0]);
      if (!field) {
        continue;
      }
      if (ordering[1] === "asc") {
        addedFilters.push([
          ordering[0],
          "gt",
          this.dataTableSelectors.getResetValue(lastRecord, field)
        ]);
      } else if (ordering[1] === "desc") {
        addedFilters.push([
          ordering[0],
          "lt",
          this.dataTableSelectors.getResetValue(lastRecord, field)
        ]);
      }
    }
    addedOrdering = this.gridOrderingSelectors.ordering.filter(o => o[0] !== 'id');
    addedOrdering.push(['id', 'asc']);
    const apiResult = yield this.dataLoader.loadDataTable({
      limit: 5000,
      orderBy: addedOrdering as Array<[string, string]>,
      filter: addedFilters as Array<[string, string, string]> // TODO!!!
    });
    const records = apiResult.data.result.map((record: any) => {
      const newRecord = new DataTableRecord(
        record.id,
        Array(this.dataTableSelectors.fieldCount)
      );

      for (const kvPair of (Object as any)
        .entries(record)
        .filter((o: [string, string, string]) => o[0] !== "id")) {
        // TODO: typecast?
        const field = this.dataTableSelectors.getFieldById(kvPair[0]);
        if (!field || field === "ID") {
          continue;
        }
        newRecord.values[field.dataIndex] = kvPair[1];
      }

      return newRecord;
    });
    this.dataTableActions.appendRecords(records);
    return {
      records
    };
  }

  public loadBeforeFirstRecord() {
    return (this.loadingPromise = flow(
      this.loadBeforeFirstRecordProc.bind(this)
    )());
  }

  private *loadBeforeFirstRecordProc() {
    const lastRecord = this.dataTableSelectors.firstFullRecord;
    const addedFilters = [];
    let addedOrdering = [];
    if (this.gridOrderingSelectors.ordering.length === 0) {
      addedFilters.push(["id", "lt", lastRecord.id]);
    }
    for (const ordering of this.gridOrderingSelectors.ordering) {
      const field = this.dataTableSelectors.getFieldById(ordering[0]);
      if (!field) {
        return;
      }
      if (ordering[1] === "asc") {
        addedFilters.push([
          ordering[0],
          "lt",
          this.dataTableSelectors.getResetValue(lastRecord, field)
        ]);
      } else if (ordering[1] === "desc") {
        addedFilters.push([
          ordering[0],
          "gt",
          this.dataTableSelectors.getResetValue(lastRecord, field)
        ]);
      }
    }
    addedOrdering = this.gridOrderingSelectors.ordering.filter(o => o[0] !== 'id');
    addedOrdering.push(['id', 'asc']);
    const apiResult = yield this.dataLoader.loadDataTable({
      limit: 5000,
      orderBy: addedOrdering
        .map(o => [...o])
        .map(o => [o[0], { asc: "desc", desc: "asc" }[o[1]]]) as Array<
        [string, string]
      >,
      filter: addedFilters as Array<[string, string, string]>
    });
    const records = apiResult.data.result.map((record: any) => {
      const newRecord = new DataTableRecord(
        record.id,
        Array(this.dataTableSelectors.fieldCount)
      );

      for (const kvPair of (Object as any)
        .entries(record)
        .filter((o: [string, ICellValue]) => o[0] !== "id")) {
        const field = this.dataTableSelectors.getFieldById(kvPair[0]);
        if (!field || field === "ID") {
          continue;
        }
        newRecord.values[field.dataIndex] = kvPair[1];
      }

      return newRecord;
    });
    records.reverse();
    this.dataTableActions.prependRecords(records);
    return {
      records
    };
  }

  @action.bound
  public requestLoadFresh() {
    return noCancelException(
      (this.loadingPromise = flow(this.loadFreshProc.bind(this))())
    );
  }

  private *loadFreshProc() {
    this.cancelLoading();
    yield* this.loadFresh();
    this.state.setHeadLoadingActive(false);
    this.state.setTailLoadingActive(true);
  }

  private *loadFresh() {
    const addedOrdering = this.gridOrderingSelectors.ordering
    .filter(o => o[0] !== 'id');
    addedOrdering.push(['id', 'asc']);
    const apiResult = yield this.dataLoader.loadDataTable({
      limit: 5000,
      orderBy: addedOrdering as Array<
        [string, string]
      >
    });
    const records = apiResult.data.result.map((record: any) => {
      const newRecord = new DataTableRecord(
        record.id,
        Array(this.dataTableSelectors.fieldCount)
      );

      for (const kvPair of (Object as any)
        .entries(record)
        .filter((o: [string, ICellValue]) => o[0] !== "id")) {
        const field = this.dataTableSelectors.getFieldById(kvPair[0]);
        if (!field || field === "ID") {
          continue;
        }
        newRecord.values[field.dataIndex] = kvPair[1];
      }

      return newRecord;
    });
    this.dataTableActions.setRecords(records);
  }

  @action.bound
  public requestLoadFreshAround(phrase: string) {
    this.loadFreshAroundProc.bind(this);
    return noCancelException(
      (this.loadingPromise = flow(this.loadFreshAroundProc.bind(this) as (
        phrase: string
      ) => IterableIterator<any>)(phrase))
    );
  }

  private *loadFreshAroundProc(phrase: string) {
    this.cancelLoading();
    const loaded = yield* this.loadFreshAround(phrase);
    reactionRuntimeInfo.add("DATA_LOADED", "LOAD_FRESH_AROUND");
    this.gridOrderingActions.setOrdering("name", "asc");
    if (loaded.targettedRecordId) {
      console.log("Selecting", loaded.targettedRecordId);
      this.gridInteractionActions.select(loaded.targettedRecordId, "name");
    }
  }

  private *loadFreshAround(phrase: string) {
    const around = phrase;
    const [apiResult1, apiResult2] = yield Promise.all([
      this.dataLoader.loadDataTable({
        limit: 5000,
        orderBy: [["name", "asc"]],
        filter: [["name", "gte", around]]
      }),
      this.dataLoader.loadDataTable({
        limit: 5000,
        orderBy: [["name", "desc"]],
        filter: [["name", "lt", around]]
      })
    ]);
    let targettedRecordId;
    if (apiResult1.data.result.length > 0) {
      targettedRecordId = apiResult1.data.result[0].id;
    } else if (apiResult2.data.result.length > 0) {
      targettedRecordId = apiResult2.data.result[0].id;
    }
    apiResult2.data.result.reverse();
    const records = [...apiResult2.data.result, ...apiResult1.data.result].map(
      record => {
        const newRecord = new DataTableRecord(
          record.id,
          Array(this.dataTableSelectors.fieldCount)
        );

        for (const kvPair of (Object as any)
          .entries(record)
          .filter((o: [string, ICellValue]) => o[0] !== "id")) {
          const field = this.dataTableSelectors.getFieldById(kvPair[0]);
          if (!field || field === "ID") {
            continue;
          }
          newRecord.values[field.dataIndex] = kvPair[1];
        }

        return newRecord;
      }
    );
    this.dataTableActions.setRecords(records);
    return {
      targettedRecordId
    };
  }

  @action.bound
  public trimTail() {
    this.dataTableActions.trimTail(
      this.dataTableSelectors.fullRecordCount - 50000
    );
  }

  @action.bound
  public trimHead() {
    this.dataTableActions.trimHead(
      this.dataTableSelectors.fullRecordCount - 50000
    );
  }

  @action.bound
  public cancelLoading() {
    if (this.loadingPromise) {
      this.loadingPromise.cancel();
      this.loadingPromise = undefined;
    }
    this.cancelLoadWaiting();
  }

  @action.bound
  public cancelLoadWaiting() {
    if (this.loadWaitingPromise) {
      this.loadWaitingPromise.cancel();
      this.loadWaitingPromise = undefined;
    }
  }

  @action.bound
  public waitForLoadingFinished() {
    const self = this;
    this.loadWaitingPromise = flow(function* waitForLoadingFinished() {
      try {
        // Stop propagating cancellation to loadingPromise.
        // Promise.all issues non-cancellable promise object.
        yield Promise.all([self.loadingPromise]);
      } finally {
        self.loadWaitingPromise = undefined;
      }
    })();
    return this.loadWaitingPromise;
  }
}
