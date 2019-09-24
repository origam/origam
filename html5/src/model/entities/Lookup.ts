import _ from "lodash";
import { computed, createAtom, flow, IAtom, observable, when } from "mobx";
import { getApi } from "../selectors/getApi";
import { IDropDownColumn } from "./types/IDropDownColumn";
import { IDropDownParameter, IDropDownType, ILookup, ILookupData } from "./types/ILookup";

export enum IIdState {
  LOADING = "LOADING",
  ERROR = "ERROR"
}

export class Lookup implements ILookup {
  constructor(data: ILookupData) {
    Object.assign(this, data);
    this.dropDownColumns.forEach(o => (o.parent = this));
    console.log(this.dropDownParameters);
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
  dropDownParameters: IDropDownParameter[] = [];

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

  triggerloadImm = flow(
    function*(this: Lookup) {
      if (this.isSomethingLoading) {
        return;
      }
      while (true) {
        const idsToLoad: Set<string> = new Set();
        try {
          for (let key of this.visibleIds.keys()) {
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

          const labels = yield api.getLookupLabels({
            LookupId: this.lookupId,
            MenuId: undefined, // getMenuItemId(this),
            LabelIds: Array.from(idsToLoad)
          }) as { [key: string]: any };

          this.isSomethingLoading = false;
          for (let [key, value] of Object.entries(labels)) {
            this.idStates.delete(key);
            this.resolvedValues.set(key, value);
            idsToLoad.delete(key);
          }
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

  triggerLoad = _.debounce(this.triggerloadImm, 100);

  resolveList = flow(
    function*(this: Lookup, idsToLoad: Set<string>) {
      yield when(() => !this.isSomethingLoading);
      try {
        this.isSomethingLoading = true;
        for (let key of idsToLoad.keys()) {
          if (key && !this.idStates.has(key) && !this.resolvedValues.has(key)) {
            this.idStates.set(key, IIdState.LOADING);
          } else {
            idsToLoad.delete(key);
          }
        }
        const api = getApi(this);
        if (idsToLoad.size === 0) {
          return;
        }
        const labels = yield api.getLookupLabels({
          LookupId: this.lookupId,
          MenuId: undefined, // getMenuItemId(this),
          LabelIds: Array.from(idsToLoad)
        });
        this.isSomethingLoading = false;
        for (let [key, value] of Object.entries(labels)) {
          this.idStates.delete(key);
          this.resolvedValues.set(key, value);
          idsToLoad.delete(key);
        }
      } catch (error) {
        this.isSomethingLoading = false;
        for (let key of idsToLoad) {
          this.idStates.set(key, IIdState.ERROR);
        }
        console.error(error);
      }
    }.bind(this)
  );

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

  @computed
  get parameters() {
    const parameters: { [key: string]: any } = {};
    for (let param of this.dropDownParameters) {
      parameters[param.parameterName] = param.fieldName;
    }
    return parameters;
  }
}
