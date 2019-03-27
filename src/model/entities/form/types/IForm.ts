
export interface IForm {
  initialValues: Map<string, any>;
  dirtyValues: Map<string, any>;
  valueMap: Map<string, any>;

  getDirty(id: string): boolean;
  getValue(id: string): any;
  getDirtyValue(id: string): any;
  getInitialValue(id: string): any;

  setDirtyValue(id: string, value: any): void;
}