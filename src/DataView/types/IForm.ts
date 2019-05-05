export interface IForm {
  initialValues: Map<string, any>;
  dirtyValues: Map<string, any>;
  setDirtyValue(id: string, value: any): void;
  init(values: Map<string, any>): void;
  destroy(): void;
  isDirtyField(id: string): boolean;
  getValue(id: string): any;
}
