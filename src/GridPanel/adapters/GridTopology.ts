import { IGridTopology } from "../../Grid/types";
import { IDataTableSelectors, IRecordId, IFieldId } from "../../DataTable/types";


export class GridTopology implements IGridTopology {
  constructor(public dataTableSelectors: IDataTableSelectors) {}

  public getUpRowId(rowId: IRecordId) {
    const rowIndex = this.dataTableSelectors.getRecordIndexById(rowId);
    if(rowIndex === undefined) {
      return;
    }
    const newRowId = this.getRowIdByIndex(rowIndex - 1);
    return newRowId;
  }

  public getDownRowId(rowId: IRecordId) {
    const rowIndex = this.dataTableSelectors.getRecordIndexById(rowId);
    if(rowIndex === undefined) {
      return;
    }
    const newRowId = this.getRowIdByIndex(rowIndex + 1);
    return newRowId;
  }

  public getLeftColumnId(columnId: IFieldId) {
    const columnIndex = this.dataTableSelectors.getFieldIndexById(columnId);
    if(columnIndex === undefined) {
      return;
    }
    const newColumnId = this.getColumnIdByIndex(columnIndex - 1);
    return newColumnId;
  }

  public getRightColumnId(columnId: IFieldId) {
    const columnIndex = this.dataTableSelectors.getFieldIndexById(columnId);
    if(columnIndex === undefined) {
      return;
    }
    const newColumnId = this.getColumnIdByIndex(columnIndex + 1);
    return newColumnId;
  }

  public getColumnIdByIndex(columnIndex: number) {
    const field = this.dataTableSelectors.getFieldByFieldIndex(columnIndex);
    return field && field.id;
  }

  public getRowIdByIndex(rowIndex: number) {
    const record = this.dataTableSelectors.getRecordByRecordIndex(rowIndex);
    return record && record.id;
  }

  public getColumnIndexById(columnId: IFieldId) {
    const fieldIndex = this.dataTableSelectors.getFieldIndexById(columnId);
    return fieldIndex;
  }

  public getRowIndexById(rowId: IRecordId) {
    const recordIndex = this.dataTableSelectors.getRecordIndexById(rowId);
    return recordIndex;
  }
}
