import {
  IRowStateData,
  IRowState,
  IRowStateItem,
  IRowStateColumnItem
} from "./types/IRowState";
import { observable, action, createAtom, flow } from "mobx";
import _ from "lodash";
import { getApi } from "model/selectors/getApi";
import { getSessionId } from "model/selectors/getSessionId";
import { getEntity } from "model/selectors/DataView/getEntity";
import { flashColor2htmlColor } from "utils/flashColorFormat";

export enum IIdState {
  LOADING = "LOADING",
  ERROR = "ERROR"
}

export class RowState implements IRowState {
  $type_IRowState: 1 = 1;

  constructor(data: IRowStateData) {
    Object.assign(this, data);
  }

  @observable resolvedValues: Map<string, IRowStateItem> = new Map();
  @observable idStates = new Map();
  @observable observedIds = new Map();

  isSomethingLoading = false;

  triggerLoadImm = flow(
    function*(this: RowState) {
      if (this.isSomethingLoading) {
        return;
      }
      while (true) {
        const idsToLoad: Set<string> = new Set();
        try {
          for (let key of this.observedIds.keys()) {
            if (
              key &&
              !this.idStates.has(key) &&
              !this.resolvedValues.has(key)
            ) {
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
          for (let state of states) {
            this.putValue(state);
            this.idStates.delete(state.id);
            idsToLoad.delete(state.id);
          }
          console.log(this.resolvedValues);
        } catch (error) {
          this.isSomethingLoading = false;
          for (let key of idsToLoad) {
            this.idStates.set(key, IIdState.ERROR);
          }
          console.error(error);
        }
      }
    }.bind(this)
  );

  triggerLoad = _.debounce(this.triggerLoadImm, 100);

  getValue(key: string) {
    if (!this.observedIds.has(key)) {
      this.observedIds.set(key, {
        atom: createAtom(
          `RowState atom [${key}]`,
          () => {
            // console.log('trigger load')
            this.triggerLoad();
          },
          () => {
            this.observedIds.delete(key);
          }
        )
      });
    }
    this.observedIds.get(key)!.atom.reportObserved();
    return this.resolvedValues.get(key);
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
              flashColor2htmlColor(column.foregroundColor),
              flashColor2htmlColor(column.backgroundColor),
              column.allowRead,
              column.allowUpdate
            );
            return [column.name, rs];
          })
        ),
        new Set(state.disabledActions)
      )
    );
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
    public disabledActions: Set<string>
  ) {}
}

export class RowStateColumnItem implements IRowStateColumnItem {
  constructor(
    public name: string,
    public foregroundColor: string | undefined,
    public backgroundColor: string | undefined,
    public allowRead: boolean,
    public allowUpdate: boolean
  ) {}
}
