import { ILookupResolver } from "./types/ILookupResolver";
import { ILookup } from "./types/ILookup";
import { ML } from "../../utils/types";
import { unpack } from "../../utils/objects";

export class Lookup implements ILookup {
  constructor(
    public P: { lookupId: ML<string>; lookupResolver: ML<ILookupResolver> }
  ) {}

  get lookupId() {
    return unpack(this.P.lookupId);
  }

  get lookupResolver() {
    return unpack(this.P.lookupResolver);
  }
}
