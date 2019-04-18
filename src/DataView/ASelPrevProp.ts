import { action } from "mobx";
import { L, ML } from "../utils/types";
import { IASelPrevProp } from "./types/IASelPrevProp";
import { IProperties } from "./types/IProperties";
import { IPropCursor } from "./types/IPropCursor";
import { IASelProp } from "./types/IASelProp";
import { unpack } from "../utils/objects";
import { IPropReorder } from "./types/IPropReorder";
import { IProperty } from "./types/IProperty";

export class ASelPrevProp implements IASelPrevProp {
  constructor(
    public P: {
      props: ML<IPropReorder>;
      propCursor: ML<IPropCursor>;
      aSelProp: ML<IASelProp>;
    }
  ) {}

  @action.bound do() {
    const { selId } = this.propCursor;
    const id1 = selId && this.props.getIdBeforeId(selId);
    if (id1) {
      this.aSelProp.do(id1);
    }
  }

  get propCursor() {
    return unpack(this.P.propCursor);
  }

  get props() {
    return unpack(this.P.props);
  }

  get aSelProp() {
    return unpack(this.P.aSelProp);
  }
}
