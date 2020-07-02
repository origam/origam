import _ from "lodash";
import {action, flow, observable, when} from "mobx";
import {handleError} from "model/actions/handleError";
import {getApi} from "model/selectors/getApi";
import {ILookupLoader} from "./types/ILookupLoader";

export class LookupLoader implements ILookupLoader {
  collectedQuery: {
    LookupId: string;
    MenuId: string | undefined;
    LabelIds: string[];
  }[] = [];

  @observable loadingScheduled = true;
  @observable loadingDone = false;
  @observable loadingError: undefined | any;
  willLoad = false;
  isLoading = false;

  loadedItems: { [key: string]: { [key: string]: any } } = {};
  waitingRequesters = 0;

  async getLookupLabels(query: {
    LookupId: string;
    MenuId: string | undefined;
    LabelIds: string[];
  }) {
    try {
      const existingItem = this.collectedQuery.find(
        q => q.LookupId === query.LookupId && q.MenuId === query.MenuId
      );
      if (existingItem) {
        const existingIds = new Set(existingItem.LabelIds);
        query.LabelIds.forEach(id => existingIds.add(id));
        existingItem.LabelIds = Array.from(existingIds);
      } else {
        this.collectedQuery.push(query);
      }
      this.ensureScheduleLoad();
      this.waitingRequesters++;
      await when(() => this.loadingDone || this.loadingError);
      if (this.loadingError) {
        throw this.loadingError;
      }
      const result = this.loadedItems[query.LookupId] || {};
      return result;
    } finally {
      this.waitingRequesters--;
      if (this.waitingRequesters === 0) {
        this.loadedItems = {};
        this.loadingError = undefined;
      }
    }
  }

  async triggerLoadImm() {
    if (this.isLoading) {
      return;
    }
    this.isLoading = true;
    try {
      while (this.willLoad) {
        const api = getApi(this);
        this.willLoad = false;
        const query = this.collectedQuery;
        this.collectedQuery = [];
        this.loadedItems = {
          ...this.loadedItems,
          ...(await api.getLookupLabelsEx(query))
        };
        if (!this.willLoad) {
          this.loadingDone = true;
        }
      }
    } catch (error) {
      console.error(error);
      // TODO: Better error handling.
      // TODO: Refactor to use generators
      await flow(handleError(this))(error);
    } finally {
      this.isLoading = false;
    }
  }

  triggerLoad = _.throttle(this.triggerLoadImm, 100);

  @action.bound ensureScheduleLoad() {
    this.loadingDone = false;
    this.willLoad = true;
    this.triggerLoad();
  }

  parent?: any;
}
