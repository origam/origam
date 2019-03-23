import { ITableProperty } from "./ITableProperty";

export interface ITableViewParam {
  id: string;
  isHeadless: boolean;
  properties: ITableProperty[];
}