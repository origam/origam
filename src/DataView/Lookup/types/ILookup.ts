import { ILookupResolver } from "./ILookupResolver";

export interface ILookup {
  lookupId: string;
  lookupResolver: ILookupResolver;
}