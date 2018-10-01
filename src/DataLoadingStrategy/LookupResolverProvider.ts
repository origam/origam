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
    ([tableId, fieldId]) => `${tableId}@${fieldId}`
  );

  public get(tableId: ITableId, fieldId: IFieldId) {
    return this.cache.get(
      [tableId, fieldId],
      () => new LookupResolver(this.parentFactory.dataLoader, tableId, fieldId)
    );
  }
}
