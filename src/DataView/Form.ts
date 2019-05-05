import { action, observable } from "mobx";
import { IForm } from "./types/IForm";

export class Form implements IForm {
  @observable initialValues: Map<string, any> = new Map();
  @observable dirtyValues: Map<string, any> = new Map();

  @action.bound setDirtyValue(id: string, value: any) {
    this.dirtyValues.set(id, value);
  }

  @action.bound
  init(values: Map< string, any>) {
    this.initialValues = new Map(values);
    this.dirtyValues = new Map();
  }
  
  @action.bound
  destroy() {
    this.initialValues = new Map();
    this.dirtyValues = new Map();
  }

  isDirtyField(id: string): boolean {
    return this.dirtyValues.has(id);
  }

  getValue(id: string) {
    return this.isDirtyField(id)
      ? this.dirtyValues.get(id)
      : this.initialValues.get(id);
  }
}