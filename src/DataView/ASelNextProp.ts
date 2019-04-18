import { action } from "mobx";
import { L, ML } from "../utils/types";
import { IASelProp } from "./types/IASelProp";
import { IPropCursor } from "./types/IPropCursor";
import { IProperties } from "./types/IProperties";
import { IASelNextProp } from "./types/IASelNextProp";
import { unpack } from "../utils/objects";
import { IPropReorder } from "./types/IPropReorder";
import { IProperty } from "./types/IProperty";

export class ASelNextProp implements IASelNextProp {
  constructor(
    public P: {
      props: ML<IPropReorder>;
      propCursor: ML<IPropCursor>;
      aSelProp: ML<IASelProp>;
    }
  ) {}

  @action.bound do() {
    const {selId} = this.propCursor;
    const id1 = selId && this.props.getIdAfterId(selId);
    if (id1) {
      this.aSelProp.do(id1);
    }
  }

  get props() {
    return unpack(this.P.props);
  }

  get propCursor() {
    return unpack(this.P.propCursor);
  }

  get aSelProp() {
    return unpack(this.P.aSelProp);
  }
}
