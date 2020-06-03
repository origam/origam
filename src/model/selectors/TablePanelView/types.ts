import {IOrderByDirection} from "../../entities/types/IOrderingConfiguration";

export interface IColumnHeader {
  id: string;
  label: string;
  ordering: IOrderByDirection;
  order: number;
}