export enum IOrderByDirection {
  NONE = "NONE",
  ASC = "ASC",
  DESC = "DESC"
}


export interface IOrderByColumnSetting {
  ordering: IOrderByDirection;
  order: number;
}

export interface IOrderingConfiguration {
  getOrdering(column: string): IOrderByColumnSetting;
  setOrdering(column: string): void;
  addOrdering(column: string): void;

  parent?: any;
}
