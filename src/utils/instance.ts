import { getAtom, observable, onBecomeUnobserved } from "mobx";

export class InstanceCache<TKey, TResult> {
  constructor(public keyFn: (args: any[]) => TKey) {}

  @observable
  public instances = new Map<TKey, TResult>();

  public get(args: any[], create: (args: any[]) => TResult): TResult {
    let key: any;
    if (this.keyFn) {
      key = this.keyFn(args);
    } else {
      key = JSON.stringify(args);
    }
    let instance: TResult;
    if (!this.instances.has(key)) {
      instance = create(args);
      this.instances.set(key, instance);
      const hOnBecomeUnobserved = onBecomeUnobserved(
        getAtom(this.instances, key) as any,
        () => {
          this.instances.delete(key);
          hOnBecomeUnobserved();
        }
      );
    } else {
      instance = this.instances.get(key)!;
    }
    return instance;
  }

  /* public release(instance: TResult) {
    const entry = Array.from(this.instances.entries()).find(
      ([k, v]) => v === instance
    );
    if (entry && entry[0]) {
      this.instances.delete(entry[0]);
    }
  }*/
}
