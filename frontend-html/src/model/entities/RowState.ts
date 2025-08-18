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

import _ from "lodash";
import { action, computed, createAtom, flow, IAtom, observable } from "mobx";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getApi } from "model/selectors/getApi";
import { getSessionId } from "model/selectors/getSessionId";
import { IRowState, IRowStateColumnItem, IRowStateItem } from "./types/IRowState";
import { FlowBusyMonitor } from "utils/flow";
import { handleError } from "model/actions/handleError";
import { visibleRowsChanged } from "gui/Components/ScreenElements/Table/TableRendering/renderTable";
import { getDataSource } from "model/selectors/DataSources/getDataSource";
import { flashColor2htmlColor } from "utils/flashColorFormat";

const defaultRowStatesToFetch = 100;

export enum IIdState {
  LOADING = "LOADING",
  ERROR = "ERROR"
}

export class RowState implements IRowState {
  $type_IRowState: 1 = 1;
  suppressWorkingStatus: boolean = false;
  dataViewVisibleRows: Map<string,string[]> = new Map();
  disposers: (()=> void)[] = [];

  constructor(debouncingDelayMilliseconds?: number) {
    this.triggerLoadDebounced = _.debounce(
      this.triggerLoadImm,
      debouncingDelayMilliseconds == undefined ? 0 : debouncingDelayMilliseconds);
    const disposer = visibleRowsChanged.subscribe((visibleRows) => {
      const dataSource = getDataSource(this);
      if (!visibleRows || dataSource.identifier !== visibleRows.dataSourceId) {
        return;
      }
      // The event is sometimes raised with no ids, then some ids, then no ids...
      // Ignoring the no ids makes sure that the triggerLoadDebounced will not run with no ids
      // when some are actually visible. This problem was not really observed so may be the
      // "if" statement could be removed if this results in more RowState calls then necessary.
      if(visibleRows.rowIds.length > 0) {
        this.dataViewVisibleRows.set(visibleRows.dataViewModelInstanceId, visibleRows.rowIds);
        this.triggerLoadDebounced();
      }
    });
    this.disposers.push(disposer);
  }

  monitor: FlowBusyMonitor = new FlowBusyMonitor();

  get isWorking() {
    return this.monitor.isWorkingDelayed;
  }

  @observable firstLoadingPerformed = false;
  @observable temporaryRequestsValues?: Map<string, RowStateRequest>;
  @computed get mayCauseFlicker() {
    return !this.firstLoadingPerformed;
  }
 
  @observable
  requests: Map<string, RowStateRequest> = new Map<string, RowStateRequest>();

  @observable
  isSomethingLoading = false;

  triggerLoadImm = flow(() => this.triggerLoad(false));

  private getRequestsToLoad(loadAll: boolean){
    if(loadAll){
      return this.requests.values()
    }
    if (this.dataViewVisibleRows.size === 0) {
      return Array.from(this.requests.values()).slice(-defaultRowStatesToFetch);
    } else {
      let requestForVisibleRows: RowStateRequest[] = [];
      for (let visibleRowIds of this.dataViewVisibleRows.values()) {
        const requestsForDataView = visibleRowIds
            .map(rowId => this.requests.get(rowId))
            .filter(x => x !== undefined) as unknown as IterableIterator<RowStateRequest>
        requestForVisibleRows = [...requestForVisibleRows, ...requestsForDataView];
      }
      return requestForVisibleRows;
    }
  }

  *triggerLoad(loadAll: boolean): any {
    if (this.isSomethingLoading) {
      return;
    }
    let requestsToLoad: Map<string, RowStateRequest> = new Map();
    let reportBusyStatus = true;
    try {
      while (true) {
        try {
          const requests = this.getRequestsToLoad(loadAll);

          for (let request of requests) {
            if (request.rowId && !request.isValid && !request.processingSate){
              requestsToLoad.set(request.rowId, request);
            }
          }
          reportBusyStatus = Array.from(requestsToLoad.values()).every(requests => !requests.suppressWorkingStatus);
          if (reportBusyStatus){
            this.monitor.inFlow++;
          }
          if (requestsToLoad.size === 0) {
            break;
          }
          for (let request of requestsToLoad.values()) {
            request.processingSate = IIdState.LOADING;
          }
          this.isSomethingLoading = true;
          const api = getApi(this);
          const states = yield api.getRowStates({
            SessionFormIdentifier: getSessionId(this),
            Entity: getEntity(this),
            Ids: Array.from(requestsToLoad.values()).map(request => request.rowId)
          });
          this.isSomethingLoading = false;
          this.firstLoadingPerformed = true;
          for (let state of states) {
            this.putValue(state);
            this.requests.get(state.id)!.processingSate = undefined;
          }
        } catch (error) {
          this.isSomethingLoading = false;
          this.firstLoadingPerformed = true;
          for (let request of requestsToLoad.values()) {
            request.processingSate = IIdState.ERROR;
          }
          yield* handleError(this)(error);
        } finally {
          if(reportBusyStatus){
            this.monitor.inFlow--;
          }
          requestsToLoad.forEach(request => request.suppressWorkingStatus = false);
          requestsToLoad.clear();
        }
      }
    } finally {
      // After everything got loaded, here we switch back to provide the values just loaded.
      this.temporaryRequestsValues = undefined;
    }
  }
  triggerLoadDebounced: any;

  getValue(rowId: string) {
    if (!this.requests.has(rowId)) {
      this.requests.set(rowId, new RowStateRequest(rowId));
    }
    let request = this.requests.get(rowId)!;
    if (!request.atom) {
      request.suppressWorkingStatus = this.suppressWorkingStatus;
      request.atom = createAtom(
        `RowState atom [${rowId}]`,
        () =>
          requestAnimationFrame(() => {
            this.triggerLoadDebounced();
          }),
        () => {
        }
      )
    }
    request.atom.reportObserved?.();
    if (this.temporaryRequestsValues && this.temporaryRequestsValues.has(rowId)) {
      return this.temporaryRequestsValues.get(rowId)?.rowStateItem;
    } else {
      return this.requests.get(rowId)?.rowStateItem;
    }
  }

  async loadValues(rowIds: string[]) {
    for (const rowId of rowIds) {
      if (!this.requests.has(rowId)) {
        this.requests.set(rowId, new RowStateRequest(rowId));
      }
    }
    await flow(() => this.triggerLoad(true))();
  }

  hasValue(rowId: string): boolean {
    return this.requests.has(rowId);
  }

  @action.bound
  putValue(state: any) {
    let rowStateItem = new RowStateItem(
      state.id,
      state.allowCreate,
      state.allowDelete,
      flashColor2htmlColor(state.foregroundColor),
      flashColor2htmlColor(state.backgroundColor),
      new Map(
        state.columns.map((column: any) => {
          const rs = new RowStateColumnItem(
            column.name,
            column.dynamicLabel,
            flashColor2htmlColor(column.foregroundColor),
            flashColor2htmlColor(column.backgroundColor),
            column.allowRead,
            column.allowUpdate
          );
          return [column.name, rs];
        })
      ),
      state.disabledActions ? new Set(state.disabledActions) : undefined,
      state.relations
    );
    if (!this.requests.has(state.id)) {
      this.requests.set(state.id, new RowStateRequest(state.id));
    }
    const request = this.requests.get(state.id);
    request!.rowStateItem = rowStateItem;
    request!.isValid = true;
    this.firstLoadingPerformed = true;
  }

  clearValue(rowId: string){
    const rowStateRequest = this.requests.get(rowId);
    if (rowStateRequest) {
      rowStateRequest.dispose();
      this.requests.delete(rowId);
    }
  }

  @action.bound reload() {
   // Store the rest of values to suppress flickering while reloading.
    this.temporaryRequestsValues = new Map(this.requests.entries());
    // This actually causes reloading of the values (by views calling getValue(...) )
    for (let rowStateRequest of this.requests.values()) {
      rowStateRequest.dispose();
    }
  }

  @action.bound clearAll() {
    for (let rowStateRequest of this.requests.values()) {
      rowStateRequest.dispose();
    }
    this.requests.clear();
    this.firstLoadingPerformed = false;
    this.temporaryRequestsValues = undefined;
    // TODO: Wait when something is currently loading.
  }

  dispose(){
    this.disposers.forEach(x => x());
  }

  parent?: any;
}

export class RowStateItem implements IRowStateItem {
  constructor(
    public id: string,
    public allowCreate: boolean,
    public allowDelete: boolean,
    public foregroundColor: string | undefined,
    public backgroundColor: string | undefined,
    public columns: Map<string, IRowStateColumnItem>,
    public disabledActions: Set<string> | undefined,
    public relations: any[]
  ) {
  }
}

export class RowStateColumnItem implements IRowStateColumnItem {
  constructor(
    public name: string,
    public dynamicLabel: string | undefined | null,
    public foregroundColor: string | undefined,
    public backgroundColor: string | undefined,
    public allowRead: boolean,
    public allowUpdate: boolean
  ) {
  }
}

class RowStateRequest {
  public rowId: string;

  @observable
  public rowStateItem: IRowStateItem | undefined;

  @observable
  public isValid: boolean = false;

  public processingSate: IIdState | undefined;
  public suppressWorkingStatus: boolean = false;

  constructor(
    rowId: string,
    public atom?: IAtom
  ) {
    this.rowId = rowId;
  }

  dispose() {
    this.atom?.onBecomeUnobservedListeners?.clear();
    this.atom?.onBecomeObservedListeners?.clear();
    this.atom = undefined;
    this.isValid = false;
    this.processingSate = undefined;
  }
}

