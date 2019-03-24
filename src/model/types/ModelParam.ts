
export interface IDataViewParam {
  id: string;
  properties: IPropertyParam[];
}

export interface IPropertyParam {
  id: string;
  name: string;
  entity: string;
  column: string;
  isReadOnly: boolean;
}
