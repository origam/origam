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

import { action, createAtom, IAtom, observable } from "mobx";
import _ from "lodash";
import { LookupCacheIndividual } from "./LookupCacheIndividual";
import { ILookupIndividualResultListenerArgs, LookupLoaderIndividual, } from "./LookupLoaderIndividual";
import { TypeSymbol } from "dic/Container";

export class LookupResolver {
  constructor(private cache: LookupCacheIndividual, private loader: LookupLoaderIndividual) {
  }

  @observable
  resolved = new Map<any, any>();
  atoms = new Map<any, IAtom>();

  globalAtom = createAtom(
    "LookupGlobal",
    () => {
    },
    () => {
    }
  );

  handleAtomObserved(key: any, atom: IAtom) {
    this.atoms.set(key, atom);
    if (!this.resolved.has(key)) {
      this.loader.setInterest(key);
    }
  }

  handleAtomUnobserved(key: any, atom: IAtom) {
    this.atoms.delete(key);
    this.loader.resetInterest(key);
  }

  @action.bound
  cleanAndReload() {
    const keysToDelete: any[] = [];
    for (let k of this.resolved.keys()) {
      if (!this.atoms.has(k)) {
        keysToDelete.push(k);
      } else {
        this.loader.setInterest(k);
      }
    }
    for (let k of keysToDelete) {
      this.resolved.delete(k);
    }
    // This one might not be needed?
    this.globalAtom.reportChanged();
  }

  @action.bound
  handleResultingLabels(args: ILookupIndividualResultListenerArgs) {
    for (let [k, v] of args.labels) {
      this.resolved.set(k, v);
    }
    this.cache.addLookupLabels(this.resolved);
    this.globalAtom.reportChanged();
  }

  resolveValue(key: any) {
    // This runs in COMPUTED scope
    //debugger
    if (_.isString(key)) key = String(key).toLowerCase();

    let value: any = null;

    this.globalAtom.reportObserved();

    if (this.resolved.has(key)) value = this.resolved.get(key)!;

    if (value === null) {
      const cachedLabels = this.cache.getLookupLabels();
      if (cachedLabels.has(key)) {
        this.resolved.set(key, cachedLabels.get(key!));
        value = this.resolved.get(key)!;
      }
    }

    if (!this.atoms.has(key)) {
      const atom = createAtom(
        `ALookup@${key}`,
        () => this.handleAtomObserved(key, atom),
        () => this.handleAtomUnobserved(key, atom)
      );
      atom.reportObserved();
    } else {
      const atom = this.atoms.get(key)!;
      atom.reportObserved();
    }


    return value;
  }

  async resolveList(keys: Set<any>): Promise<Map<any, any>> {
    const missingKeys = new Set(keys);
    const cachedLabels = this.cache.getLookupLabels();
    const cachedResultMap = new Map<any, any>();
    for (let labelId of Array.from(missingKeys.keys())) {
      if (this.resolved.has(labelId)) {
        missingKeys.delete(labelId);
        cachedResultMap.set(labelId, this.resolved.get(labelId));
      }
      if (cachedLabels.has(labelId)) {
        missingKeys.delete(labelId);
        cachedResultMap.set(labelId, cachedLabels.get(labelId));
      }
    }

    const loadedResultMap = await this.loader.loadList(missingKeys);
    const entryArray = Array.from(loadedResultMap);
    if (entryArray.length === 1) {
      this.cache.addLookupLabels(loadedResultMap);
      const innerLoadedMap = entryArray[0][1];
      this.resolved = new Map([...innerLoadedMap, ...this.resolved]);
      return new Map([...innerLoadedMap, ...cachedResultMap]);
    }
    if (entryArray.length === 0) {
      return cachedResultMap;
    }
    throw new Error("More that one lookup result maps");
  }

  isEmptyAndLoading(key: any) {
    if (_.isString(key)) key = String(key).toLowerCase();

    if (!this.resolved.has(key)) {
      return this.loader.isWorking(key);
    } else return false;
  }
}

export const ILookupResolver = TypeSymbol<LookupResolver>("ILookupResolver");
