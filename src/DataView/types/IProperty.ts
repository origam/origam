import { ILookupResolver } from "../Lookup/types/ILookupResolver";

export interface IProperty {
  id: string;
  name: string;
  column: string;
  entity: string;
  formatterPattern: string;
  isReadOnly: boolean;
  dataIndex: number;
  dataSourceIndex: number;
  lookupResolver: ILookupResolver | undefined;
  lookupColumns: string[];
}
