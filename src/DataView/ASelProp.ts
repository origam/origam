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
import { IASelCell } from "./types/IASelCell";

export class ASelProp implements IASelProp {
  constructor(
    public P: {
      aSelCell: ML<IASelCell>;
    }
  ) {}

  @action.bound
  do(id: string | undefined) {
    this.aSelCell.do(undefined, id);
  }

  @action.bound
  doByIdx(idx: number | undefined) {
    this.aSelCell.doByIdx(undefined, idx);
  }

  @action.bound
  doSelFirst() {
    this.doByIdx(0);
  }

  get aSelCell() {
    return unpack(this.P.aSelCell);
  }
}
