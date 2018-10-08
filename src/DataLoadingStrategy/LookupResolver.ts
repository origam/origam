import { action, flow, observable } from "mobx";
import * as _ from "lodash";
import { IDataLoader } from "./types";

export class LookupResolver {
  constructor(
    public dataLoader: IDataLoader,
    public dataTableName: string,
    public resultFieldId: string
  ) {}

  @observable
  private lookupCache = new Map();
  private idsAskedFor = new Set();

  public getLookedUpValue(id: string) {
    if (this.lookupCache.has(id)) {
      return this.lookupCache.get(id);
    }
    if (!this.idsAskedFor.has(id)) {
      this.idsAskedFor.add(id);
      this.requestLookupLoad();
    }
  }

  private requestLookupLoad = _.throttle(this.requestLookupLoadImm, 500, {
    leading: false
  });

  @action.bound
  private requestLookupLoadImm() {
    const self = this;
    return flow(
      function* requestLookupLoadImm() {
        while (self.idsAskedFor.size > 0) {
          const ids = Array.from(self.idsAskedFor.values());
          const result = yield self.dataLoader.loadLookup(
            self.dataTableName,
            self.resultFieldId,
            ids
          );
          for (const item of result.data.result) {
            self.idsAskedFor.delete(item.id);
            ids.splice(ids.findIndex(o => o === item.id), 1);
            self.lookupCache.set(item.id, item[self.resultFieldId]);
          }
          for (const id of ids) {
            self.idsAskedFor.delete(id);
            self.lookupCache.set(id, undefined);
          }
        }
      }.bind(this)
    )();
  }
}
