import { IProperty } from "./IProperty";
export interface IPropReorder {
  count: number;
  propIds: string[];
  setIds(ids: string[]): void;
  reorderedItems: IProperty[];
  originalItems: IProperty[];
  getNthIdFrom(id: string | undefined, n: number): string | undefined;
  getIdAfterId(id: string | undefined): string | undefined;
  getIdBeforeId(id: string | undefined): string | undefined;
  getById(id: string | undefined): IProperty | undefined;
  getByIndex(idx: number | undefined): IProperty | undefined;
  getIdByIndex(idx: number | undefined): string | undefined;
  getIndexById(id: string): number | undefined;
}
