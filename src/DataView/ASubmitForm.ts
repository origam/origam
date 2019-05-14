import { action } from "mobx";
import { L } from "../utils/types";
import { IASubmitForm } from "./types/IASubmitForm";
import { IRecCursor } from "./types/IRecCursor";
import { IDataTable } from "./types/IDataTable";
import { IForm } from "./types/IForm";

export class ASubmitForm implements IASubmitForm {
  constructor(
    public P: {
      recCursor: IRecCursor;
      dataTable: IDataTable;
      form: IForm;
    }
  ) {}

  @action.bound
  do() {
    const recCursor = this.P.recCursor;
    const dataTable = this.P.dataTable;
    const form = this.P.form;
    // -------------------------------------------------------
    if (recCursor.isSelected) {
      const selRowId = recCursor.selId;
      if (form.dirtyValues.size > 0) {
        selRowId && dataTable.addDirtyValues(selRowId, form.dirtyValues);
      }
      form.destroy();
    }
  }
}
