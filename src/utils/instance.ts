import { IReactionDisposer, onBecomeUnobserved, _getGlobalState, observable } from "mobx";

export class InstanceCache<TKey, TResult> {
  constructor(public keyFn: (args: any[]) => TKey) {}

  @observable
  private instances = new Map<TKey, TResult>();

  public get(args: any[], create: (args: any[]) => TResult): TResult {
    let key: any;
    if (this.keyFn) {
      key = this.keyFn(args);
    } else {
      key = JSON.stringify(args);
    }
    let instance;
    if (!this.instances.has(key)) {
      instance = create(args);

      this.instances.set(key, instance);
    } else {
      instance = this.instances.get(key)!;
    }
    return instance;
  }

}
