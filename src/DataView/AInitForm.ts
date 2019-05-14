import { action } from "mobx";
import { L } from "../utils/types";
import { IAInitForm } from "./types/IAInitForm";
import { IRecCursor } from "./types/IRecCursor";
import { IDataTable } from "./types/IDataTable";
import { IForm } from "./types/IForm";


export class AInitForm implements IAInitForm {
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
      const valueMap = selRowId
        ? dataTable.getRecValueMap(selRowId)
        : undefined;
      valueMap && form.init(valueMap);
    }
  }
}
