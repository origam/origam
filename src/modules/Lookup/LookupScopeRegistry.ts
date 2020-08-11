import { Container, TypeSymbol } from "dic/Container";
import { ILookupId } from "./LookupModule";


export class LookupScopeRegistry {
  items = new Map<string, Container>();

  addScope($cont: Container) {
    this.items.set($cont.resolve(ILookupId), $cont);
  }

  removeScope($cont: Container) {
    this.items.delete($cont.resolve(ILookupId));
  }

  getScope(lookupId: string) {
    return this.items.get(lookupId)!;
  }

  hasScope(lookupId: string) {
    return this.items.has(lookupId);
  }
}
export const ILookupScopeRegistry = TypeSymbol<LookupScopeRegistry>("ILookupScopeRegistry");
