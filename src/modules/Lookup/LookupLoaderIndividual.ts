import { action } from "mobx";
import { ILookupMultiResultListenerArgs, LookupLoaderMulti, ILookupLoaderMulti } from "./LookupLoaderMulti";
import { PubSub } from "./common";
import { TypeSymbol } from "dic/Container";
import { ILookupId } from "./LookupModule";

export interface ILookupIndividualResultListenerArgs {
  labels: Map<any, any>;
}

export class LookupLoaderIndividual {
  constructor(private lookupId: string, private loader: LookupLoaderMulti) {}

  @action.bound
  handleResultingLabels(args: ILookupMultiResultListenerArgs) {
    for (let [k, v] of args.labels.entries()) {
      if (k === this.lookupId) this.resultListeners.trigger({ labels: v });
    }
  }

  resultListeners = new PubSub<ILookupIndividualResultListenerArgs>();

  setInterrest(key: any) {
    this.loader.setInterrest(this.lookupId, key);
  }

  resetInterrest(key: any) {
    this.loader.resetInterrest(this.lookupId, key);
  }

  async loadList(labelIds: Set<any>) {
    return this.loader.loadList(this.lookupId, labelIds);
  }

  isWorking(key: any) {
    return this.loader.isWorking(this.lookupId, key);
  }
}
export const ILookupLoaderIndividual = TypeSymbol<LookupLoaderIndividual>("ILookupLoaderIndividual");
