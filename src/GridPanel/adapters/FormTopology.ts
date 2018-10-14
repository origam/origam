import { IFormTopology, IGridTopology } from "src/Grid/types";

export class FormTopology implements IFormTopology {
  constructor(public gridTopology: IGridTopology) {}

  public getPrevRecordId(recordId: string): string | undefined {
    return this.gridTopology.getUpRowId(recordId);
  }

  public getNextRecordId(recordId: string): string | undefined {
    return this.gridTopology.getDownRowId(recordId);
  }

  public getPrevFieldId(fieldId: string): string | undefined {
    return this.gridTopology.getLeftColumnId(fieldId);
  }

  public getNextFieldId(fieldId: string): string | undefined {
    return this.gridTopology.getRightColumnId(fieldId);
  }

  public getFieldIdByIndex(fieldIndex: number): string | undefined {
    return this.gridTopology.getColumnIdByIndex(fieldIndex);
  }

  public getRecordIdByIndex(recordIndex: number): string | undefined {
    return this.gridTopology.getRowIdByIndex(recordIndex);
  }

  public getFieldIndexById(fieldId: string): number {
    return this.gridTopology.getColumnIndexById(fieldId);
  }

  public getRecordIndexById(recordId: string): number {
    return this.gridTopology.getRowIndexById(recordId);
  }
}
