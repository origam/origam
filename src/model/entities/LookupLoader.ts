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
import { action, computed, flow, observable, when } from "mobx";
import { handleError } from "model/actions/handleError";
import { getApi } from "model/selectors/getApi";
import { ILookupLoader } from "./types/ILookupLoader";

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

  @observable inFlow = 0;

  @computed get isWorking() {
    return this.inFlow > 0;
  }

  async getLookupLabels(query: {
    LookupId: string;
    MenuId: string | undefined;
    LabelIds: string[];
  }) {
    try {
      const existingItem = this.collectedQuery.find(
        (q) => q.LookupId === query.LookupId && q.MenuId === query.MenuId
      );
      if (existingItem) {
        const existingIds = new Set(existingItem.LabelIds);
        query.LabelIds.forEach((id) => existingIds.add(id));
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
    const self = this;
    flow(function*() {
      if (self.isLoading) {
        return;
      }
      self.isLoading = true;
      try {
        self.inFlow++;
        while (self.willLoad) {
          const api = getApi(self);
          self.willLoad = false;
          const query = self.collectedQuery;
          self.collectedQuery = [];
          self.loadedItems = {
            ...self.loadedItems,
            ...(yield api.getLookupLabelsEx(query)),
          };
          if (!self.willLoad) {
            self.loadingDone = true;
          }
        }
      } catch (error) {
        // TODO: Better error handling.
        // TODO: Refactor to use generators
        yield*handleError(self)(error);
      } finally {
        self.inFlow--;
        self.isLoading = false;
      }
    })();
  }

  triggerLoad = _.throttle(this.triggerLoadImm.bind(this), 100);

  @action.bound ensureScheduleLoad() {
    this.loadingDone = false;
    this.willLoad = true;
    this.triggerLoad();
  }

  parent?: any;
}
