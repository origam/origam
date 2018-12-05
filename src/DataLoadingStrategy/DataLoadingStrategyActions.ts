import {
  action,
  flow,
  reaction,
  IReactionDisposer,
  when,
  observable,
  computed,
  comparer
} from "mobx";
import { DataTableRecord } from "../DataTable/DataTableState";
import { reactionRuntimeInfo } from "../utils/reaction";
import {
  IDataLoadingStategyState,
  IDataLoadingStrategySelectors,
  IDataLoader,
  IDataLoadingStrategyActions,
  ILoadingGate,
  IGridFilter
} from "./types";
import {
  IDataTableActions,
  IDataTableSelectors,
  ICellValue,
  IFieldId
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
import { IDataTableFieldStruct, IRecordId } from "../DataTable/types";
import { EventObserver } from "../utils/events";
import { IGridCursorView } from "../Grid/types";

function noCancelException<T>(promise: Promise<void | T>): Promise<void | T> {
  return promise.catch(e => {
    if (e.message !== "FLOW_CANCELLED") {
      throw e;
    }
  });
}

function constructPaginationFilter(
  direction: "after" | "before",
  cursorValues: ICellValue[],
  ordering: Array<[IFieldId, string]>
): any {
  const orderingMap = {};
  const columnIds: IFieldId[] = [];
  for (const orderingItem of ordering) {
    orderingMap[orderingItem[0]] = orderingItem[1];
    columnIds.push(orderingItem[0]);
  }

  const pagSlug: any = ["$OR"];
  for (let compOrd = 0; compOrd < columnIds.length; compOrd++) {
    const ordTerm: any = ["$AND"];
    for (let termNum = 0; termNum <= compOrd; termNum++) {
      ordTerm.push([
        columnIds[termNum],
        termNum < compOrd
          ? "eq"
          : direction === "after" && orderingMap[columnIds[termNum]] === "asc"
          ? "gt"
          : "lt",
        cursorValues[termNum]
      ]);
    }
    pagSlug.push(ordTerm);
  }

  return pagSlug;
}

export class DataLoadingStrategyActions implements IDataLoadingStrategyActions {
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
    public gridViewActions: IGridActions,
    public gridCursorView: IGridCursorView
  ) {}

  private onCancelLoading = EventObserver();
  private onCancelLoadWaiting = EventObserver();

  private loadingPromise: CancellablePromise<any> | undefined;
  private loadWaitingPromise: CancellablePromise<any> | undefined;
  private reOrdering: IReactionDisposer | undefined;
  private reOutline: IReactionDisposer | undefined;
  private reIncremental: IReactionDisposer | undefined;
  private reFilter: IReactionDisposer | undefined;

  @observable
  public inLoading = 0;

  @computed
  public get isLoading() {
    return this.inLoading > 0;
  }

  @action.bound
  public stop() {
    this.reOrdering && this.reOrdering();
    this.reOutline && this.reOutline();
    this.reIncremental && this.reIncremental();
    this.reFilter && this.reFilter();
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

    this.reFilter = reaction(
      () => {
        return [
          this.selectors.bondFilters
            .map(filter => filter.gridFilter)
            .map(filter => filter)
        ];
      },
      () => {
        // console.log("Reload due to filter change");
        if (reactionRuntimeInfo.info.has("LOAD_FRESH_AROUND")) {
          return;
        }
        this.requestLoadFresh();
      },
      {
        equals: comparer.structural
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
  public addLoadingGate(gate: ILoadingGate): () => void {
    return this.state.addLoadingGate(gate);
  }

  @action.bound
  public addBondFilter(filter: IGridFilter): () => void {
    return this.state.addBondFilter(filter);
  }

  @action.bound
  public setLoadingActive(state: boolean): void {
    this.state.setLoadingActive(state);
  }

  private isLoadIncrementWaitingOnGate = false;
  private isLoadIncrementWaitingForLoading = false;

  private requestLoadIncrement = flow(function*(
    this: DataLoadingStrategyActions
  ) {
    if (this.isLoadIncrementWaitingOnGate) {
      return;
    }
    try {
      this.isLoadIncrementWaitingOnGate = true;
      yield when(() => {
        return this.selectors.loadingGatesOpen;
      });
    } finally {
      this.isLoadIncrementWaitingOnGate = false;
    }

    if (!this.selectors.incrementLoadingNeeded) {
      return;
    }

    if (this.isLoadIncrementWaitingForLoading) {
      return;
    }
    try {
      this.isLoadIncrementWaitingForLoading = true;
      const waitingPromise = when(() => !this.isLoading);
      yield waitingPromise;
    } finally {
      this.isLoadIncrementWaitingForLoading = false;
    }
    if (!this.selectors.incrementLoadingNeeded) {
      return;
    }
    try {
      this.inLoading++;
      if (
        this.selectors.headLoadingActive &&
        this.selectors.headLoadingNeeded
      ) {
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
    } catch (e) {
      if (e.message !== "CANCEL") {
        throw e;
      }
    } finally {
      this.inLoading--;
    }
  });

  private async loadAfterLastRecord() {
    try {
      this.inLoading++;
      const lastRecord = this.dataTableSelectors.lastFullRecord;
      const addedFilters = [];
      const addedOrdering = this.gridOrderingSelectors.ordering.filter(
        o => o[0] !== "Id"
      );
      addedOrdering.push(["Id", "asc"]);
      const cursorValues = addedOrdering.map(ord => {
        const field = this.dataTableSelectors.getFieldById(ord[0]);
        const value = this.dataTableSelectors.getResetValue(lastRecord, field!);
        return value;
      });
      addedFilters.push(
        ...constructPaginationFilter("after", cursorValues, addedOrdering)
      );
      const fields = [...this.dataTableSelectors.fields];
      fields.sort((a, b) => a.recvDataIndex - b.recvDataIndex);
      const columns = this.dataTableSelectors.fields.map(field => field.id);
      const idFieldIndex = fields.findIndex(field => field.isPrimaryKey);

      const apiResult = await this.dataLoader.loadDataTable(
        {
          limit: 5000,
          orderBy: addedOrdering as Array<[string, string]>,
          filter: addedFilters as Array<[string, string, string]>, // TODO!!!
          columns
        },
        this.onCancelLoading
      );

      const records = apiResult.data.map((record: any) => {
        const newRecord = new DataTableRecord(
          record[idFieldIndex],
          Array(this.dataTableSelectors.fieldCount)
        );

        for (let fieldIdx = 0; fieldIdx < record.length; fieldIdx++) {
          const field = fields[fieldIdx];
          newRecord.values[field.dataIndex] = record[fieldIdx];
        }

        return newRecord;
      });
      this.dataTableActions.appendRecords(records);
      return {
        records
      };
    } finally {
      this.inLoading--;
    }
  }

  private async loadBeforeFirstRecord() {
    try {
      this.inLoading++;
      const firstRecord = this.dataTableSelectors.firstFullRecord;
      const addedFilters = [];
      let addedOrdering = [];
      addedOrdering = this.gridOrderingSelectors.ordering.filter(
        o => o[0] !== "Id"
      );
      addedOrdering.push(["Id", "asc"]);
      const cursorValues = addedOrdering.map(ord => {
        const field = this.dataTableSelectors.getFieldById(ord[0]);
        const value = this.dataTableSelectors.getResetValue(
          firstRecord,
          field!
        );
        return value;
      });
      addedFilters.push(
        ...constructPaginationFilter("before", cursorValues, addedOrdering)
      );

      const fields = [...this.dataTableSelectors.fields];
      fields.sort((a, b) => a.recvDataIndex - b.recvDataIndex);
      const columns = this.dataTableSelectors.fields.map(field => field.id);
      const idFieldIndex = fields.findIndex(field => field.isPrimaryKey);
      const apiResult = await this.dataLoader.loadDataTable(
        {
          limit: 5000,
          orderBy: addedOrdering as Array<[string, string]>,
          filter: addedFilters as Array<[string, string, string]>, // TODO!!!
          columns
        },
        this.onCancelLoading
      );
      const records = apiResult.data.map((record: any) => {
        const newRecord = new DataTableRecord(
          record[idFieldIndex],
          Array(this.dataTableSelectors.fieldCount)
        );

        for (let fieldIdx = 0; fieldIdx < record.length; fieldIdx++) {
          const field = fields[fieldIdx];
          newRecord.values[field.dataIndex] = record[fieldIdx];
        }

        return newRecord;
      });
      records.reverse();
      this.dataTableActions.prependRecords(records);
      return {
        records
      };
    } finally {
      this.inLoading--;
    }
  }

  private isLoadFreshWaitingOnGate = false;

  public requestLoadFresh = flow(
    function*(this: DataLoadingStrategyActions) {
      if (this.isLoadFreshWaitingOnGate) {
        return;
      }
      try {
        this.isLoadFreshWaitingOnGate = true;
        yield when(() => {
          return this.selectors.loadingGatesOpen;
        });
        // TODO: add waiting cancellation here?
      } finally {
        this.isLoadFreshWaitingOnGate = false;
      }
      console.log("#*#*#*#*#*#*#");
      try {
        this.inLoading++;
        this.cancelLoading();
        this.state.setHeadLoadingActive(false);
        this.state.setTailLoadingActive(false);
        yield this.loadFresh();
        if (this.dataTableSelectors.recordCount >= 5000) {
          this.state.setTailLoadingActive(true);
        }
      } catch (e) {
        if (e.message !== "CANCEL") {
          throw e;
        }
      } finally {
        this.gridCursorView.fixGridSelection();
        this.inLoading--;
      }
    }.bind(this)
  );

  private loadFresh = flow(function*(this: DataLoadingStrategyActions) {
    try {
      this.inLoading++;
      const addedOrdering = this.gridOrderingSelectors.ordering.filter(
        o => o[0] !== "Id"
      );
      addedOrdering.push(["Id", "asc"]);
      const fields = [...this.dataTableSelectors.fields];
      fields.sort((a, b) => a.recvDataIndex - b.recvDataIndex);
      const columns = this.dataTableSelectors.fields.map(field => field.id);
      const idFieldIndex = fields.findIndex(field => field.isPrimaryKey);
      const filters = this.selectors.bondFilters
        .map(filt => filt.gridFilter)
        .filter(filt => filt.length > 0);
      console.log("A");
      const apiResult = yield this.dataLoader.loadDataTable(
        {
          limit: 5000,
          orderBy: addedOrdering as Array<[string, string]>,
          columns,
          filter: filters.length > 0 ? ["$AND", ...filters] : undefined
        },
        this.onCancelLoading
      );
      console.log("B");
      const records = apiResult.data.map((record: any) => {
        const newRecord = new DataTableRecord(
          record[idFieldIndex],
          Array(this.dataTableSelectors.fieldCount)
        );

        for (let fieldIdx = 0; fieldIdx < record.length; fieldIdx++) {
          const field = fields[fieldIdx];
          newRecord.values[field.dataIndex] = record[fieldIdx];
        }

        return newRecord;
      });
      this.dataTableActions.setRecords(records);
    } finally {
      this.inLoading--;
    }
  });

  @action.bound
  public async reloadRow(id: IRecordId) {
    try {
      this.inLoading++;
      const fields = [...this.dataTableSelectors.fields];
      fields.sort((a, b) => a.recvDataIndex - b.recvDataIndex);
      const columns = this.dataTableSelectors.fields.map(field => field.id);
      const idFieldIndex = fields.findIndex(field => field.isPrimaryKey);
      const apiResult = await this.dataLoader.loadDataTable(
        {
          limit: 1,
          orderBy: [],
          columns,
          filter: ["Id", "eq", id]
        },
        this.onCancelLoading
      );
      const records = apiResult.data.map((record: any) => {
        const newRecord = new DataTableRecord(
          record[idFieldIndex],
          Array(this.dataTableSelectors.fieldCount)
        );

        for (let fieldIdx = 0; fieldIdx < record.length; fieldIdx++) {
          const field = fields[fieldIdx];
          newRecord.values[field.dataIndex] = record[fieldIdx];
        }

        return newRecord;
      });

      if (records.length === 1) {
        this.dataTableActions.replaceUpdatedRecord(records[0]);
      }
    } catch (e) {
      if (e.message !== "CANCEL") {
        throw e;
      }
    } finally {
      this.inLoading--;
    }
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
      // console.log("Selecting", loaded.targettedRecordId);
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
    this.onCancelLoading.trigger();
    this.onCancelLoadWaiting.trigger();
  }

  @action.bound
  public cancelLoadWaiting() {
    this.onCancelLoadWaiting.trigger();
  }
}
