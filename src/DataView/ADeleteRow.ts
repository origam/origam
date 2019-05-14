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
      dataTable: IDataTable;
      recCursor: IRecCursor;
      dispatch: (action: any) => void;
      listen: (cb: (action: any) => void) => void;
    }
  ) {
    this.subscribeMediator();
  }

  subscribeMediator() {
    this.P.listen((action: any) => {
      if (isType(action, DataViewActions.deleteSelectedRow)) {
        this.doSelected();
      }
    });
  }

  @action.bound
  doSelected() {
    const rowId = this.recCursor.selId;
    if (rowId) {
      this.dataTable.markDeletedRow(rowId);
      // TODO: Select closest existing row...
      this.P.dispatch(DataViewActions.requestSaveData);
    }
  }

  get dataTable() {
    return this.P.dataTable;
  }

  get recCursor() {
    return this.P.recCursor;
  }
}
