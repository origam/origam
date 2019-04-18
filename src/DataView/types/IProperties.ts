import { IProperty } from "./IProperty";
export interface IProperties {
  count: number;
  items: IProperty[];
  ids: string[];
  getByIndex(idx: number): IProperty | undefined;
  getById(id: string): IProperty | undefined;
  getIdByIndex(idx: number): string | undefined;
  getIndexById(id: string): number | undefined;
  getIdAfterId(id: string): string | undefined ;
  getIdBeforeId(id: string): string | undefined;
}
