import { createAtom, IAtom, action } from "mobx";
import _ from "lodash";
import { LookupCacheIndividual, ILookupCacheIndividual } from "./LookupCacheIndividual";
import {
  LookupLoaderIndividual,
  ILookupIndividualResultListenerArgs,
  ILookupLoaderIndividual,
} from "./LookupLoaderIndividual";
import { TypeSymbol } from "dic/Container";

export class LookupResolver {
  constructor(
    private cache: LookupCacheIndividual,
    private loader: LookupLoaderIndividual
  ) {}

  resolved = new Map<any, any>();
  atoms = new Map<any, IAtom>();

  globalAtom = createAtom(
    "LookupGlobal",
    () => {},
    () => {}
  );

  handleAtomObserved(key: any, atom: IAtom) {
    this.atoms.set(key, atom);
    if (!this.resolved.has(key)) {
      this.loader.setInterrest(key);
    }
  }

  handleAtomUnobserved(key: any, atom: IAtom) {
    this.atoms.delete(key);
    this.loader.resetInterrest(key);
  }

  @action.bound
  cleanAndReload() {
    const keysToDelete: any[] = [];
    for (let [k, v] of this.resolved.entries()) {
      if (!this.atoms.has(k)) {
        keysToDelete.push(k);
      } else {
        this.loader.setInterrest(k);
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

    if (!this.atoms.has(key)) {
      throw new Error("Not in a tracking context.");
    }

    return value;
  }

  async resolveList(keys: Set<any>) {
    keys = new Set(keys);
    const cachedLabels = this.cache.getLookupLabels();
    for(let labelId of Array.from(keys.keys())) {
      if(this.resolved.has(labelId)) keys.delete(labelId);
      if(cachedLabels.has(labelId)) keys.delete(labelId);
    }
    const result = await this.loader.loadList(keys);
    this.cache.addLookupLabels(result);
    return result;
  }

  isEmptyAndLoading(key: any) {
    if (!this.resolved.has(key)) {
      return this.loader.isWorking(key);
    } else return false;
  }
}
export const ILookupResolver = TypeSymbol<LookupResolver>("ILookupResolver");
