export interface IFormDataManagerData {}

export interface IFormDataManager extends IFormDataManagerData {
  $type_IFormDataManager: 1;

  parent?: any;
}

export const isIFormDataManager = (o: any): o is IFormDataManager =>
  o.$type_IFormDataManager;
