import { action, flow } from "mobx";
import axios from "axios";
import { CancellablePromise } from "mobx/lib/api/flow";
import { IAPI, IDataLoader } from "./types";
import { getToken } from "./api";
import { ISubscriber, IEventSubscriber } from '../utils/events';

function tidyUpFilter(filter: any) {
  if (!filter) {
    return;
  }
  for (let i = 0; i < filter.length; i++) {
    const term = filter[i];
    if (Array.isArray(term)) {
      filter[i] = tidyUpFilter(term);
    }
  }

  if (!Array.isArray(filter[0])) {
    // It is a conj/disj term
    if (filter.length === 2) {
      // Delete and/or
      filter = filter.slice(1);
    }
  }
  if (filter.length === 1 && Array.isArray(filter[0])) {
    filter = filter[0];
  }

  return filter;
}

export class DataLoader implements IDataLoader {
  constructor(
    public tableName: string,
    public api: IAPI,
    public menuItemId: string
  ) {}

  private loadingPromise: CancellablePromise<any> | undefined;
  private loadWaitingPromise: CancellablePromise<any> | undefined;

  @action.bound
  public loadOutline() {
    return axios
      .get(`http://127.0.0.1:8080/api/${this.tableName}/outline`, {
        params: {
          method: "string",
          col: "name"
        }
      })
      .then(
        action(
          (result: any): string[] => {
            return result.data.map((o: { name: string }) => o.name);
          }
        )
      );
  }

  @action.bound
  public loadDataTable({
    columns,
    limit,
    filter,
    orderBy
  }: {
    columns?: string[];
    limit?: number;
    filter?: Array<[string, string, string]>;
    orderBy?: Array<[string, string]>;
  }, canceller?: IEventSubscriber) {
    return this.api.loadDataTable({
      tableId: this.tableName,
      columns,
      token: getToken(),
      limit,
      filter: tidyUpFilter(filter),
      orderBy,
      menuId: this.menuItemId
    }, canceller);

    /*return axios.get(`http://127.0.0.1:8080/api/${this.tableName}`, {
      params: {
        limit,
        cols: JSON.stringify(columns),
        filter: JSON.stringify(filter),
        odb: (orderBy && orderBy.length > 0) ? JSON.stringify(orderBy) : undefined
      }
    });*/
  }

  public async loadLookup(lookupId: string, labelIds: string[]) {
    /*return axios.get(`http://127.0.0.1:8080/api/${table}`, {
      params: {
        cols: JSON.stringify(["id", label]),
        filter: JSON.stringify([["id", "in", ids]])
      }
    });*/
    return await axios.post(
      `/api/Data/GetLookupLabels`,
      { lookupId, labelIds, menuId: this.menuItemId },
      { headers: { Authorization: `Bearer ${getToken()}` } }
    );
  }

  public async loadLokupOptions(
    dataStructureEntityId: string,
    property: string,
    rowId: string,
    lookupId: string,
    searchText: string,
    pageSize: number,
    pageNumber: number,
    columnNames: string[],
  ): Promise<any> {
    return await axios.post(
      `/api/Data/GetLookupListEx`,
      {
        dataStructureEntityId,
        property,
        id: rowId,
        lookupId,
        showUniqueValues: "false",
        searchText,
        pageSize,
        pageNumber,
        columnNames,
        menuId: this.menuItemId
      },
      { headers: { Authorization: `Bearer ${getToken()}` } }
    );
  }

  @action.bound
  public cancelLoading() {
    if (this.loadingPromise) {
      this.loadingPromise.cancel();
      this.loadingPromise = undefined;
    }
    this.cancelLoadWaiting();
  }

  @action.bound
  public waitForLoadingFinished() {
    const self = this;
    this.loadWaitingPromise = flow(
      function* waitForLoadingFinished() {
        try {
          // Stop propagating cancellation to loadingPromise.
          // Promise.all issues non-cancellable promise object.
          yield Promise.all([self.loadingPromise]);
        } finally {
          self.loadWaitingPromise = undefined;
        }
      }.bind(this)
    )();
    return this.loadWaitingPromise;
  }

  @action.bound
  public cancelLoadWaiting() {
    if (this.loadWaitingPromise) {
      this.loadWaitingPromise.cancel();
      this.loadWaitingPromise = undefined;
    }
  }
}
