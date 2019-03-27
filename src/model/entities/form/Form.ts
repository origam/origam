import { IForm } from "./types/IForm";
import { computed, action, observable } from "mobx";

export class Form implements IForm {
  constructor(initialValues: Map<string, any> = new Map()) {
    this.initialValues = initialValues;
  }

  @observable initialValues: Map<string, any> = new Map();
  @observable dirtyValues: Map<string, any> = new Map();

  @computed get valueMap(): Map<string, any> {
    return new Map(Array.from(this.initialValues.keys()).map(key => [
      key,
      this.getValue(key)
    ]) as Array<[string, any]>);
  }

  getDirty(id: string): boolean {
    return this.dirtyValues.has(id);
  }

  getValue(id: string) {
    return this.getDirty(id)
      ? this.getDirtyValue(id)
      : this.getInitialValue(id);
  }

  getDirtyValue(id: string) {
    return this.dirtyValues.get(id);
  }

  getInitialValue(id: string) {
    return this.initialValues.get(id);
  }

  @action.bound setDirtyValue(id: string, value: any) {
    this.dirtyValues.set(id, value);
  }
}
