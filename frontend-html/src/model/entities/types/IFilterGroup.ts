import { IFilter } from "model/entities/types/IFilter";

export interface IFilterGroup {
  filters: IFilter[];
  id: string;
  isGlobal: boolean;
  name: string;
}