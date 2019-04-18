export interface IOrderByState {
  direction: IOrderByDirection;
  order: number;
}

export enum IOrderByDirection {
  ASC = "ASC",
  DESC = "DESC",
  NONE = "NONE"
}