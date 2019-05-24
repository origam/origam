import { action } from "mobx";
import * as DataViewActions from "./DataViewActions";
import { IADeleteRow } from "./types/IADeleteRow";
import { IDataTable } from "./types/IDataTable";
import { IRecCursor } from "./types/IRecCursor";


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
      switch(action.type) {
        case DataViewActions.DELETE_SELECTED_ROW:
          this.doSelected();
          break;
      }
    });
  }

  @action.bound
  doSelected() {
    const rowId = this.recCursor.selId;
    if (rowId) {
      this.dataTable.markDeletedRow(rowId);
      // TODO: Select closest existing row...
      this.P.dispatch(DataViewActions.requestSaveData());
    }
  }

  get dataTable() {
    return this.P.dataTable;
  }

  get recCursor() {
    return this.P.recCursor;
  }
}
