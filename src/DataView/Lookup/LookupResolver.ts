import { observable, createAtom, IAtom, action, runInAction } from "mobx";
import _ from "lodash";
import { IApi } from "../../Api/IApi";
import { unpack } from "../../utils/objects";
import { ILookupResolver } from "./types/ILookupResolver";

export enum IIdState {
  LOADING = "LOADING",
  ERROR = "ERROR"
}

export class LookupResolver implements ILookupResolver {
  constructor(public P: { lookupId: string; menuItemId: string; api: IApi }) {}

  @observable labelMap: Map<string, any> = new Map();
  @observable idStates: Map<string, IIdState> = new Map();
  visibleIds: Map<string, { atom: IAtom }> = new Map();

  isLoading(key: string) {
    const stateRec = this.idStates.get(key);
    return stateRec ? stateRec === IIdState.LOADING : false;
  }

  isError(key: string) {
    const stateRec = this.idStates.get(key);
    return stateRec ? stateRec === IIdState.ERROR : false;
  }

  isSomethingLoading: boolean = false;
  @action.bound
  triggerloadImm() {
    if (this.isSomethingLoading) {
      return;
    }
    while (true) {
      const idsToLoad = new Set();
      runInAction(() => {
        for (let key of this.visibleIds.keys()) {
          if (key && !this.idStates.has(key) && !this.labelMap.has(key)) {
            idsToLoad.add(key);
            this.idStates.set(key, IIdState.LOADING);
          }
        }
      });
      if (idsToLoad.size === 0) {
        break;
      }
      this.isSomethingLoading = true;
      this.api
        .getLookupLabels({
          LookupId: this.P.lookupId,
          MenuId: this.P.menuItemId,
          LabelIds: Array.from(idsToLoad)
        })
        .then(
          action((labels: { [key: string]: string }) => {
            this.isSomethingLoading = false;
            for (let [key, value] of Object.entries(labels)) {
              this.idStates.delete(key);
              this.labelMap.set(key, value);
              idsToLoad.delete(key);
            }
            for (let key of idsToLoad) {
              this.idStates.set(key, IIdState.ERROR);
            }
          })
        )
        .catch(
          action(error => {
            this.isSomethingLoading = false;
            for (let key of idsToLoad) {
              this.idStates.set(key, IIdState.ERROR);
            }
            console.error(error);
          })
        );
    }
  }

  triggerLoad = _.debounce(this.triggerloadImm, 100);

  getValue(key: string): any {
    if (!this.visibleIds.has(key)) {
      this.visibleIds.set(key, {
        atom: createAtom(
          `Lookup atom [${key}]`,
          () => {
            this.triggerLoad();
          },
          () => {
            this.visibleIds.delete(key);
          }
        )
      });
    }
    this.visibleIds.get(key)!.atom.reportObserved();
    return this.labelMap.get(key);
  }

  get api() {
    return unpack(this.P.api);
  }

  start() {
    return;
  }

  stop() {
    return;
  }
}
