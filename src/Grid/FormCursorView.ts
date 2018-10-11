import { IFormCursorView, IGridInteractionSelectors } from "./types";
import { computed } from "mobx";
import { IDataTableActions, IDataTableSelectors, IRecordId, IFieldId, ICellValue } from "src/DataTable/types";

export class FormCursorView implements IFormCursorView {
  constructor(
    public gridInteractionSelectors: IGridInteractionSelectors,
    public dataTableActions: IDataTableActions,
    public dataTableSelectors: IDataTableSelectors
  ) {}

  @computed
  public get isCellEditing(): boolean {
    return this.gridInteractionSelectors.isCellEditing;
  }

  @computed
  public get editingFieldId(): string | undefined {
    return this.gridInteractionSelectors.editingColumnId;
  }

  @computed
  public get editingOriginalCellValue(): string | undefined {
    return
  }



  public handleDataCommit(
    dirtyValue: string,
    editingRecordId: string,
    editingFieldId: string
  ): void {
    throw new Error("Method not implemented.");
  }
}
