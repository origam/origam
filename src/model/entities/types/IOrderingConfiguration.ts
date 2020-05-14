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
  groupChildrenOrdering: IGroupChildrenOrdering | undefined;
  parent?: any;
}

export interface IGroupChildrenOrdering {
  columnId: string;
  direction: IOrderByDirection;
  lookupId: string | undefined;
}
