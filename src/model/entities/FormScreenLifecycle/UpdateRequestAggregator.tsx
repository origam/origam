import { getApi } from "model/selectors/getApi";
import { runInFlowWithHandler } from "utils/runInFlowWithHandler";
import { IApi, IUpdateData } from "../types/IApi";

interface IUpdateObjectData {
  SessionFormIdentifier: string;
  Entity: string;
  UpdateData: IUpdateData[];
}

interface IAggregatedRequest {
  id: string,
  data: IUpdateData[];
  sessionFormIdentifier: string;
  entity: string;
  promise: Promise<any> | undefined;
  timeout: NodeJS.Timeout | undefined;
  promiseResolver: ((value: any | PromiseLike<void>) => void) | undefined;
}

export class UpdateRequestAggregator {
  dataMap: Map<string, IAggregatedRequest> = new Map();
  origamAPI: IApi;
  ctx: any;
  previousRequestPromise: Promise<any> | undefined;

  constructor(ctx: any) {
    this.ctx = ctx;
    this.origamAPI = getApi(ctx);
  }

  enqueue(data: IUpdateObjectData): Promise<any> {
    const aggregatedRequest = this.getAggregateRequest(data);

    if (!aggregatedRequest.timeout) {
      aggregatedRequest.timeout = setTimeout(async () => {
        runInFlowWithHandler({
          ctx: this.ctx,
          action: async () => {
            await this.waitForRunningRequest(aggregatedRequest);
            await this.resolveAggregateRequest(
              aggregatedRequest,
              data
            );
          },
        });
      }, 100);
    }

    return aggregatedRequest.promise!;
  }

  private async resolveAggregateRequest(
    aggregatedRequest: IAggregatedRequest,
    data: IUpdateObjectData
  ) {
    aggregatedRequest.timeout = undefined;
    const resolver = aggregatedRequest.promiseResolver;
    const updateData = aggregatedRequest.data!;
    this.dataMap.delete(aggregatedRequest.id);

    const resultData = await this.origamAPI.updateObject({
      SessionFormIdentifier: data.SessionFormIdentifier,
      Entity: data.Entity,
      UpdateData: updateData,
    });
    resolver?.(resultData);
  }

  private async waitForRunningRequest(aggregatedRequest: IAggregatedRequest) {
    if (this.previousRequestPromise) {
      const previousCall = this.previousRequestPromise;
      this.previousRequestPromise = aggregatedRequest.promise;
      await previousCall;
    } else {
      this.previousRequestPromise = aggregatedRequest.promise;
    }
    this.previousRequestPromise = aggregatedRequest.promise;
  }

  private getAggregateRequest(data: IUpdateObjectData) {
    const requestId = data.Entity + data.SessionFormIdentifier;
    if (!this.dataMap.has(requestId)) {
      let promiseResolver:
        | ((value: any | PromiseLike<void>) => void)
        | undefined;
      const promise = new Promise<any>(
        (resolve, reject) => (promiseResolver = resolve)
      );
      this.dataMap.set(
        requestId,
        {
          id: requestId,
          sessionFormIdentifier: data.SessionFormIdentifier,
          entity: data.Entity,
          data: [
            ...(this.dataMap.get(requestId)?.data ?? []),
            ...data.UpdateData,
          ],
          promise: promise,
          promiseResolver: promiseResolver,
          timeout: undefined,
        }
      );
    } else {
      this.dataMap.get(requestId)!.data = [
        ...this.dataMap.get(requestId)!.data,
        ...data.UpdateData,
      ];
    }
    return this.dataMap.get(requestId)!;
  }
}
