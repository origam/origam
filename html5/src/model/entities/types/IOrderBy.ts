export enum IOrderByDirection {
  NONE = "NONE",
  ASC = "ASC",
  DESC = "DESC"
}

export interface IDataOrder {
  column: string;
  ordering: IOrderByDirection;
}