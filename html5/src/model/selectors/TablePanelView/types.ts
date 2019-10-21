export enum IOrderByDirection {
  NONE = "NONE",
  ASC = "ASC",
  DESC = "DESC"
}

export interface IColumnHeader {
  id: string;
  label: string;
  ordering: IOrderByDirection;
  order: number;
}