import { getApi } from "model/selectors/getApi";
import { runInFlowWithHandler } from "utils/runInFlowWithHandler";
import { IApi, IUpdateData } from "../types/IApi";
import { interpret, createMachine } from "xstate";


interface IUpdateObjectData {
  SessionFormIdentifier: string;
  Entity: string;
  UpdateData: IUpdateData[];
}

interface ExtPromise<T> extends Promise<T> {
  resolve(result?: T): void;
  reject(error?: any): void;
}

function ExtPromise<T>() {
  let resolveFn;
  let rejectFn;
  const promise = new Promise<T>((resolve, reject) => {
    resolveFn = resolve;
    rejectFn = reject;
  });
  (promise as any).resolve = resolveFn;
  (promise as any).reject = rejectFn;
  return promise as ExtPromise<T>;
}

export class UpdateRequestAggregator {
  constructor(
    private api: { updateObject(data: IUpdateObjectData): Promise<any> }
  ) {
    this.interpreter
      .onTransition((state, event) => {
        console.log("TR", state, event);
      })
      .start();
  }

  registeredRequests: Array<
    IUpdateObjectData & { promise: ExtPromise<any>; isRunning: boolean }
  > = [];

  itemJustEnqueued?: IUpdateObjectData & { promise: ExtPromise<any>; isRunning: boolean };

  currentRequest?: IUpdateObjectData & { promise: ExtPromise<any> };

  interpreter = interpret(
    createMachine(
      {
        id: "updateObject",
        initial: "IDLE",
        states: {
          IDLE: {
            on: {
              UPDATE_REQUESTED: {
                actions: ["enqueueRequest"],
                target: "UPDATE_DEBOUNCING",
              },
            },
          },
          UPDATE_DEBOUNCING: {
            on: {
              UPDATE_REQUESTED: {
                actions: ["enqueueRequest"],
                target: "UPDATE_DEBOUNCING",
              },
            },
            after: {
              5000: "DEQUEUE",
            },
          },
          DEQUEUE: {
            entry: "dequeueToCurrentRequest",
            always: "UPDATING",
          },
          UPDATING: {
            initial: "API_RUNNING",
            states: {
              API_RUNNING: {
                invoke: {
                  src: "apiUpdateObject",
                  onDone: "API_RESOLVED",
                  onError: "API_REJECTED",
                },
              },
              API_RESOLVED: {
                entry: ["resolveCurrentPromise"],
                type: "final",
              },
              API_REJECTED: {
                entry: ["rejectCurrentPromise"],
                type: "final",
              },
            },
            on: {
              UPDATE_REQUESTED: {
                actions: ["enqueueRequest"],
              },
            },
            onDone: [
              { cond: "hasMoreItems", target: "#updateObject.DEQUEUE" },
              { target: "#updateObject.IDLE" },
            ],
            exit: ["resetCurrentRequest"],
          },
        },
      },
      {
        services: {
          apiUpdateObject: (ctx, event) => {
            console.log("Invoking apiUpdateRequest", this.currentRequest);
            return this.api.updateObject(this.currentRequest!);
          },
        },
        actions: {
          enqueueRequest: (ctx, event) => {
            console.log("Enqueuing: ", event.payload);
            const updateObjectItem = this.mergeToQueue(event.payload);
            this.itemJustEnqueued = updateObjectItem;
            console.log(JSON.stringify(this.registeredRequests, null, 2));
          },
          dequeueToCurrentRequest: (ctx, event) => {
            const item = this.registeredRequests.shift();
            this.currentRequest = item;
            console.log("Dequeued: ", item);
          },
          resolveCurrentPromise: (ctx, event) => {
            this.currentRequest?.promise.resolve(event.data);
          },
          rejectCurrentPromise: (ctx, event) => {
            this.currentRequest?.promise.reject(event.data);
          },
          resetCurrentRequest: (ctx, event) => {
            this.currentRequest = undefined;
          },
        },
        guards: {
          hasMoreItems: (ctx, event) => {
            return this.registeredRequests.length > 0;
          },
        },
      }
    ),
    { devTools: true }
  );

  enqueue(data: IUpdateObjectData): Promise<any> {
    
    this.interpreter.send({
      type: "UPDATE_REQUESTED",
      payload: data,
    });
    const {itemJustEnqueued} = this;
    this.itemJustEnqueued = undefined;
    return itemJustEnqueued!.promise;
  }

  mergeToQueue(data: IUpdateObjectData) {
    let updateObjectItem = this.registeredRequests.find(
      (item) => item.Entity === data.Entity && !item.isRunning
    );
    if (!updateObjectItem) {
      updateObjectItem = {
        SessionFormIdentifier: data.SessionFormIdentifier,
        Entity: data.Entity,
        UpdateData: data.UpdateData,
        promise: ExtPromise(),
        isRunning: false,
      };
      this.registeredRequests.push(updateObjectItem);
    }

    for (let inputRow of data.UpdateData) {
      let existingRow = updateObjectItem.UpdateData.find(
        (item) => item.RowId === inputRow.RowId
      );
      if (!existingRow) {
        existingRow = { RowId: inputRow.RowId, Values: {} };
        updateObjectItem.UpdateData.push(existingRow);
      }
      existingRow.Values = { ...existingRow.Values, ...inputRow.Values };
    }
    return updateObjectItem;
  }
}