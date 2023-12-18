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

import { action } from "mobx";
import { ILookupMultiResultListenerArgs, LookupLoaderMulti } from "./LookupLoaderMulti";
import { PubSub } from "./common";
import { TypeSymbol } from "dic/Container";

export interface ILookupIndividualResultListenerArgs {
  labels: Map<any, any>;
}

export class LookupLoaderIndividual {
  constructor(public lookupId: string, private loader: LookupLoaderMulti) {
  }

  @action.bound
  handleResultingLabels(args: ILookupMultiResultListenerArgs) {
    for (let [k, v] of args.labels.entries()) {
      if (k === this.lookupId) this.resultListeners.trigger({labels: v});
    }
  }

  resultListeners = new PubSub<ILookupIndividualResultListenerArgs>();

  setInterest(key: any) {
    this.loader.setInterest(this.lookupId, key);
  }

  resetInterest(key: any) {
    this.loader.resetInterest(this.lookupId, key);
  }

  async loadList(labelIds: Set<any>) {
    return this.loader.loadList(this.lookupId, labelIds);
  }

  isWorking(key: any) {
    return this.loader.isWorking(this.lookupId, key);
  }
}

export const ILookupLoaderIndividual = TypeSymbol<LookupLoaderIndividual>("ILookupLoaderIndividual");
