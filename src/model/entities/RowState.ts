import _ from "lodash";
import { action, computed, createAtom, flow, observable } from "mobx";
import { handleError } from "model/actions/handleError";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getApi } from "model/selectors/getApi";
import { getSessionId } from "model/selectors/getSessionId";
import { flashColor2htmlColor } from "utils/flashColorFormat";
import { IRowState, IRowStateColumnItem, IRowStateData, IRowStateItem } from "./types/IRowState";
import { FlowBusyMonitor } from "../../utils/flow";

export enum IIdState {
  LOADING = "LOADING",
  ERROR = "ERROR"
}

export class RowState implements IRowState {
  $type_IRowState: 1 = 1;

  constructor(data: IRowStateData) {
    Object.assign(this, data);
  }

  monitor: FlowBusyMonitor =  new FlowBusyMonitor();

  get isWorking() {
    return this.monitor.isWorkingDelayed;
  }

  @observable firstLoadingPerformed = false;
  @computed get mayCauseFlicker() {
    return !this.firstLoadingPerformed;
  }

  @observable resolvedValues: Map<string, IRowStateItem> = new Map();
  @observable temporaryResolvedValues?: Map<string, IRowStateItem>;
  @observable idStates = new Map();
  @observable observedIds = new Map();

  @observable
  isSomethingLoading = false;

  triggerLoadImm = flow(
    function*(this: RowState) {
      if (this.isSomethingLoading) {
        return;
      }
      try {
        while (true) {
          const idsToLoad: Set<string> = new Set();
          try {
            this.monitor.inFlow++;
            for (let key of this.observedIds.keys()) {
              if (key && !this.idStates.has(key) && !this.resolvedValues.has(key)) {
                idsToLoad.add(key);
                this.idStates.set(key, IIdState.LOADING);
              }
            }
            if (idsToLoad.size === 0) {
              break;
            }
            this.isSomethingLoading = true;
            const api = getApi(this);
            const states = yield api.getRowStates({
              SessionFormIdentifier: getSessionId(this),
              Entity: getEntity(this),
              Ids: Array.from(idsToLoad)
            });
            this.isSomethingLoading = false;
            this.firstLoadingPerformed = true;
            for (let state of states) {
              this.putValue(state);
              this.idStates.delete(state.id);
              idsToLoad.delete(state.id);
            }
          } catch (error) {
            this.isSomethingLoading = false;
            this.firstLoadingPerformed = true;
            for (let key of idsToLoad) {
              this.idStates.set(key, IIdState.ERROR);
            }
            console.error(error);
            // TODO: Better error handling.
            yield* handleError(this)(error);
          } finally {
            this.monitor.inFlow--
          }
        }
      } finally {
        // After everything got loaded, here we swith back to provide the values just loaded.
        this.temporaryResolvedValues = undefined;
      }
    }.bind(this)
  );

  triggerLoad = _.debounce(this.triggerLoadImm, 666);

  getValue(key: string) {
    //console.log("getValue", key);
    if (!this.observedIds.has(key)) {
      this.observedIds.set(key, {
        atom: createAtom(
          `RowState atom [${key}]`,
          () =>
            requestAnimationFrame(() => {
              // console.log('trigger load')
              this.triggerLoad();
            }),
          () => {
            this.observedIds.delete(key);
          }
        )
      });
    }
    this.observedIds.get(key)?.atom?.reportObserved?.();
    // Get from temporary value storage when the values are just being refreshed.
    if (this.temporaryResolvedValues && this.temporaryResolvedValues.has(key)) {
      // To keep the values observed while taken from temporary storage.
      this.resolvedValues.get(key);
      return this.temporaryResolvedValues.get(key);
    } else {
      return this.resolvedValues.get(key);
    }
  }

  async loadValues(keys: string[]) {
    for (const key of keys) {
      if (!this.observedIds.has(key)) {
        this.observedIds.set(key, null); 
      }
    }
    await this.triggerLoadImm();
  }

  hasValue(key: string): boolean {
    return this.resolvedValues.has(key);
  }

  @action.bound
  putValue(state: any) {
    this.resolvedValues.set(
      state.id,
      new RowStateItem(
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
        new Set(state.disabledActions),
        state.relations
      )
    );
    this.firstLoadingPerformed = true;
  }

  @action.bound reload() {
    // Deletes old resolved values, which are invisible.
    const keysToDelete: any[] = [];
    for (let [resKey, resVal] of this.resolvedValues.entries()) {
      if (!this.observedIds.has(resKey)) {
        keysToDelete.push(resKey);
      }
    }
    for (let key of keysToDelete) {
      this.resolvedValues.delete(key);
    }
    // Store the rest of values to suppress flickering while reloading.
    this.temporaryResolvedValues = new Map(this.resolvedValues.entries());
    // This actually causes reloading of the values (by views calling getValue(...) )
    this.resolvedValues.clear();
    this.idStates.clear();
    for (let obsvIdVal of this.observedIds.values()) {
      if(obsvIdVal?.atom){
        obsvIdVal.atom.onBecomeUnobservedListeners.clear();
        obsvIdVal.atom.onBecomeObservedListeners.clear();
      }
    }
    this.observedIds.clear();
    this.firstLoadingPerformed = false;
  }

  @action.bound clearAll() {
    this.resolvedValues.clear();
    this.temporaryResolvedValues = undefined;
    this.idStates.clear();
    for (let obsvIdVal of this.observedIds.values()) {
      if(obsvIdVal?.atom){
        obsvIdVal.atom.onBecomeUnobservedListeners.clear();
        obsvIdVal.atom.onBecomeObservedListeners.clear();
      }
    }
    this.observedIds.clear();
    this.firstLoadingPerformed = false;
    // TODO: Wait when something is currently loading.
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
    public disabledActions: Set<string>,
    public relations: any[]
  ) {}
}

export class RowStateColumnItem implements IRowStateColumnItem {
  constructor(
    public name: string,
    public dynamicLabel: string | undefined | null,
    public foregroundColor: string | undefined,
    public backgroundColor: string | undefined,
    public allowRead: boolean,
    public allowUpdate: boolean
  ) {}
}
