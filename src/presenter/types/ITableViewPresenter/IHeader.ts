import { IOrderByState } from "./IOrderByState";
import { IHeaderFilter } from './IHeaderFilter';

export interface IHeader {
  label: string;
  orderBy: IOrderByState;
  filter: IHeaderFilter;
}