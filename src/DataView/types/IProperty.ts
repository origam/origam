import { ILookupResolver } from "../Lookup/types/ILookupResolver";

export interface IProperty {
  id: string;
  name: string;
  isReadOnly: boolean;
  dataIndex: number;
  lookupResolver: ILookupResolver | undefined;
}
