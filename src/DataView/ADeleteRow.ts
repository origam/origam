import { action } from "mobx";
import { ML } from "../utils/types";
import { IDataTable } from "./types/IDataTable";
import { IDataViewMachine } from "./types/IDataViewMachine";
import { unpack } from "../utils/objects";
import { IDataViewMediator } from "./types/IDataViewMediator";
import * as DataViewActions from "./DataViewActions";
import { IADeleteRow } from "./types/IADeleteRow";
import { IRecCursor } from "./types/IRecCursor";
import { isType } from "ts-action";

export class ADeleteRow implements IADeleteRow {
  constructor(
    public P: {
      dataTable: ML<IDataTable>;
      mediator: ML<IDataViewMediator>;
      recCursor: ML<IRecCursor>;
    }
  ) {
    this.subscribeMediator();
  }

  subscribeMediator() {
    this.mediator.listen((action: any) => {
      if(isType(action, DataViewActions.deleteSelectedRow)) {
        this.doSelected();
      }
    })
  }

  @action.bound
  doSelected() {
    const rowId = this.recCursor.selId;
    if (rowId) {
      this.dataTable.markDeletedRow(rowId);
      // TODO: Select closest existing row...
      this.mediator.dispatch(DataViewActions.requestSaveData);
    }
  }

  get dataTable() {
    return unpack(this.P.dataTable);
  }

  get mediator() {
    return unpack(this.P.mediator);
  }

  get recCursor() {
    return unpack(this.P.recCursor);
  }
}
