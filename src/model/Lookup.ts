import { ILookup, ILookupData, IDropDownType } from './types/ILookup';
import { observable, IAtom, action, runInAction, createAtom } from "mobx";
import _ from "lodash";
import { IDropDownColumn } from "./types/IDropDownColumn";
import { getApi } from "./selectors/getApi";
import { getMenuItemId } from './selectors/getMenuItemId';

export enum IIdState {
  LOADING = "LOADING",
  ERROR = "ERROR"
}

export class Lookup implements ILookup {
  constructor(data: ILookupData) {
    Object.assign(this, data);
    this.dropDownColumns.forEach(o => (o.parent = this));
  }
  $type_ILookup: 1 = 1;

  lookupId: string = "";
  dropDownShowUniqueValues: boolean = false;
  identifier: string = "";
  identifierIndex: number = 0;
  dropDownType: IDropDownType = IDropDownType.EagerlyLoadedGrid;
  cached: boolean = false;
  searchByFirstColumnOnly: boolean = false;
  dropDownColumns: IDropDownColumn[] = [];

  parent?: any;

  @observable resolvedValues: Map<string, any> = new Map();
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
      const idsToLoad: Set<string> = new Set();
      runInAction(() => {
        for (let key of this.visibleIds.keys()) {
          if (key && !this.idStates.has(key) && !this.resolvedValues.has(key)) {
            idsToLoad.add(key);
            this.idStates.set(key, IIdState.LOADING);
          }
        }
      });
      if (idsToLoad.size === 0) {
        break;
      }
      this.isSomethingLoading = true;
      const api = getApi(this);
      
      api
        .getLookupLabels({
          LookupId: this.lookupId,
          MenuId: getMenuItemId(this),
          LabelIds: Array.from(idsToLoad)
        })
        .then(
          action((labels: { [key: string]: string }) => {
            this.isSomethingLoading = false;
            for (let [key, value] of Object.entries(labels)) {
              this.idStates.delete(key);
              this.resolvedValues.set(key, value);
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
    return this.resolvedValues.get(key);
  }
}
