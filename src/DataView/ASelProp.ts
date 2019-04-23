import { L, ML } from "../utils/types";
import { action } from "mobx";
import { IASelProp } from "./types/IASelProp";
import { IPropCursor } from "./types/IPropCursor";
import { IProperties } from "./types/IProperties";
import { unpack } from "../utils/objects";
import { IPropReorder } from "./types/IPropReorder";
import { IProperty } from "./types/IProperty";
import { IDataViewMediator } from "./types/IDataViewMediator";
import { IAReloadChildren } from "./types/IAReloadChildren";

export class ASelProp implements IASelProp {
  constructor(
    public P: {
      propCursor: ML<IPropCursor>;
      properties: ML<IPropReorder>;
      aReloadChildren: ML<IAReloadChildren>;
    }
  ) {}

  @action.bound
  do(id: string | undefined) {
    const propCursor = this.propCursor;
    propCursor.setSelId(id);
    this.aReloadChildren.do();
  }

  @action.bound
  doByIdx(idx: number | undefined) {
    const properties = this.properties;
    const id = idx !== undefined ? properties.getIdByIndex(idx) : undefined;
    this.do(id);
  }

  @action.bound
  doSelFirst() {
    // TODO
    this.doByIdx(0);
  }

  get propCursor() {
    return unpack(this.P.propCursor);
  }

  get properties() {
    return unpack(this.P.properties);
  }

  get aReloadChildren() {
    return unpack(this.P.aReloadChildren);
  }
}
