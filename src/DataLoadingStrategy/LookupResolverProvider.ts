import { ILookupResolverDR } from "./types";
import { InstanceCache } from "../utils/instance";
import {
  IFieldId,
  ILookupResolverProvider,
  ITableId,
  ILookupResolver
} from "../DataTable/types";
import { LookupResolver } from "./LookupResolver";

export class LookupResolverProvider implements ILookupResolverProvider {
  constructor(public parentFactory: ILookupResolverDR) {}

  private cache = new InstanceCache<string, ILookupResolver>(
    ([lookupId]) => `${lookupId}`
  );

  public get(lookupId: string) {
    return this.cache.get(
      [lookupId],
      () => new LookupResolver(this.parentFactory.dataLoader, lookupId)
    );
  }
}
