import {
  IFormView,
  IGridInteractionSelectors,
  IGridSetup,
  IFormSetup
} from "./types";
import {
  IDataTableSelectors,
  IRecordId,
  IFieldId,
  ICellValue
} from "../DataTable/types";

export class FormView implements IFormView {
  constructor(
    public dataTableSelectors: IDataTableSelectors,
    public gridInteractionSelectors: IGridInteractionSelectors,
    public formSetup: IFormSetup
  ) {}

  public getCellValue(fieldIndex: number): ICellValue | undefined {
    if (!this.gridInteractionSelectors.isCellSelected) {
      return;
    }
    const recordIndex = this.dataTableSelectors.getRecordIndexById(
      this.gridInteractionSelectors.selectedRowId!
    );
    return this.formSetup.getCellValue(recordIndex, fieldIndex);
  }

  public getFieldLabel(fieldIndex: number): string {
    const field = this.dataTableSelectors.getFieldByFieldIndex(fieldIndex);
    return field ? field.label : `Field ${fieldIndex}`;
  }
}
