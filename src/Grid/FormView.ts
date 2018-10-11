import { IFormView, IGridInteractionSelectors } from "./types";
import {
  IDataTableSelectors,
  IRecordId,
  IFieldId,
  ICellValue
} from "../DataTable/types";

export class FormView implements IFormView {

  constructor(
    public dataTableSelectors: IDataTableSelectors,
    public gridInteractionSelectors: IGridInteractionSelectors
  ) {}

  public getOriginalFieldValue(fieldIndex: number): ICellValue | undefined {
    if(!this.gridInteractionSelectors.isCellSelected) {
      return;
    }
    const record = this.dataTableSelectors.getRecordById(
      this.gridInteractionSelectors.selectedRowId!
    );
    const field = this.dataTableSelectors.getFieldByFieldIndex(fieldIndex);
    return this.dataTableSelectors.getOriginalValue(record!, field!);
  }


  public getFieldLabel(fieldIndex: number): string {
    const field = this.dataTableSelectors.getFieldByFieldIndex(fieldIndex);
    return field ? field.label : `Field ${fieldIndex}`;
  }
}
