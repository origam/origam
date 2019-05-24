import { action } from "mobx";
import { IACreateRow } from "./types/IACreateRow";
import { ML } from "../utils/types";
import { IRecCursor } from "./types/IRecCursor";
import { IDataViewMediator } from "./types/IDataViewMediator";
import * as DataViewActions from "./DataViewActions";
import { unpack } from "../utils/objects";

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
      switch (action.type) {
        case DataViewActions.CREATE_ROW:
          this.do();
          break;
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
