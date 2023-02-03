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

import { createMachine, interpret } from "xstate";
import { IUpdateData } from "../types/IApi";


interface IUpdateObjectData {
  SessionFormIdentifier: string;
  Entity: string;
  UpdateData: IUpdateData[];
}

interface IExtPromise<T> extends Promise<T> {
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
  return promise as IExtPromise<T>;
}

export class UpdateRequestAggregator {
  constructor(
    private api: { updateObject(data: IUpdateObjectData): Promise<any> }
  ) {
    this.interpreter.start();
  }

  registeredRequests: Array<IUpdateObjectData & { promise: IExtPromise<any>; isRunning: boolean }> = [];

  itemJustEnqueued?: IUpdateObjectData & { promise: IExtPromise<any>; isRunning: boolean };

  currentRequest?: IUpdateObjectData & { promise: IExtPromise<any> };

  interpreter = interpret(
    createMachine(
      {
        predictableActionArguments: true,
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
              170: "DEQUEUE",
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
              {cond: "hasMoreItems", target: "#updateObject.DEQUEUE"},
              {target: "#updateObject.IDLE"},
            ],
            exit: ["resetCurrentRequest"],
          },
        },
      },
      {
        services: {
          apiUpdateObject: (ctx, event) => {
            return this.api.updateObject(this.currentRequest!);
          },
        },
        actions: {
          enqueueRequest: (ctx, event) => {
            const updateObjectItem = this.mergeToQueue(event.payload);
            this.itemJustEnqueued = updateObjectItem;
          },
          dequeueToCurrentRequest: (ctx, event) => {
            const item = this.registeredRequests.shift();
            this.currentRequest = item;
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
    {devTools: true}
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
        existingRow = {RowId: inputRow.RowId, Values: {}};
        updateObjectItem.UpdateData.push(existingRow);
      }
      existingRow.Values = {...existingRow.Values, ...inputRow.Values};
    }
    return updateObjectItem;
  }
}