import { action, flow } from "mobx";
import axios from "axios";
import { CancellablePromise } from "mobx/lib/api/flow";

export class DataLoader {
  constructor(public tableName: string) {}

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
        action((result: any): string[] => {
          return result.data.map((o: {name: string}) => o.name);
        })
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
  }) {
    return (flow(this.loadDataTableProc.bind(this)) as any)({
      columns,
      limit,
      filter,
      orderBy
    });
  }

  private *loadDataTableProc({
    columns,
    limit,
    filter,
    orderBy
  }: {
    columns?: string[];
    limit?: number;
    filter?: Array<[string, string, string]>;
    orderBy?: Array<[string, string]>;
  }) {
    return axios.get(`http://127.0.0.1:8080/api/${this.tableName}`, {
      params: {
        limit,
        cols: JSON.stringify(columns),
        filter: JSON.stringify(filter),
        odb: (orderBy && orderBy.length > 0) ? JSON.stringify(orderBy) : undefined
      }
    });
  }

  public loadLookup(table: string, label: string, ids: string[]) {
    return axios.get(`http://127.0.0.1:8080/api/${table}`, {
      params: {
        cols: JSON.stringify(["id", label]),
        filter: JSON.stringify([["id", "in", ids]])
      }
    });
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
