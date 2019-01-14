import { observable, action } from "mobx";
import { IDataTableSelectors, IDataCursorState } from "./types2";



export default class DataCursorState implements IDataCursorState{
  constructor(private dataTableSelectors: IDataTableSelectors) {}

  @observable public selectedRecordId: string | undefined;
  @observable public selectedFieldId: string | undefined;
  @observable public isEditing = false;

  @action.bound
  public finishEditing() {
    // TODO...
    this.isEditing = false;
  }

  @action.bound
  public cancelEditing() {
    // TODO...
    this.isEditing = false;
  }

  @action.bound
  public editSelected() {
    this.isEditing = true;
  }

  @action.bound
  public selectField(fieldId: string) {
    this.selectedFieldId = fieldId;
  }

  public selectRecord(recordId: string) {
    this.selectedRecordId = recordId;
  }

  public selectCell(recordId: string, fieldId: string) {
    this.selectRecord(recordId);
    this.selectField(fieldId);
  }

  @action.bound
  public selectNextRow() {
    const newId =
      this.isCellSelected &&
      this.dataTableSelectors.recordIdAfterId(this.selectedRecordId!);
    newId && this.selectRecord(newId);
  }

  @action.bound
  public selectPrevRow() {
    const newId =
      this.isCellSelected &&
      this.dataTableSelectors.recordIdBeforeId(this.selectedRecordId!);
    newId && this.selectRecord(newId);
  }

  @action.bound
  public selectNextColumn() {
    const newId =
      this.isCellSelected &&
      this.dataTableSelectors.fieldIdAfterId(this.selectedFieldId!);
    newId && this.selectField(newId);
  }

  @action.bound
  public selectPrevColumn() {
    const newId =
      this.isCellSelected &&
      this.dataTableSelectors.fieldIdBeforeId(this.selectedFieldId!);
    newId && this.selectField(newId);
  }

  public isCellSelected(recordId: string, fieldId: string) {
    return (
      this.selectedRecordId === recordId && this.selectedFieldId === fieldId
    );
  }
}