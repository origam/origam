import { action } from "mobx";
import { IACreateRow } from "./types/IACreateRow";
import { ML } from "../utils/types";
import { IRecCursor } from "./types/IRecCursor";
import { IDataViewMediator } from "./types/IDataViewMediator";
import * as DataViewActions from "./DataViewActions";
import { unpack } from "../utils/objects";
import { isType } from "ts-action";

export class ACreateRow implements IACreateRow {
  constructor(
    public P: {
      recCursor: ML<IRecCursor>;
      mediator: ML<IDataViewMediator>;
    }
  ) {
    this.subscribeMediator();
  }

  subscribeMediator() {
    this.mediator.listen((action: any) => {
      if (isType(action, DataViewActions.createRow)) {
        this.do();
      }
    });
  }

  @action.bound
  do() {
    this.mediator.dispatch(DataViewActions.requestCreateRow());
  }

  get mediator() {
    return unpack(this.P.mediator);
  }
}
