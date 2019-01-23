import { observable, action, computed } from "mobx";
import { IDataTableSelectors, IDataCursorState, IGridEditor } from "./types2";
import { EventObserver } from '../utils/events';

export default class DataCursorState implements IDataCursorState {
  constructor(private dataTableSelectors: IDataTableSelectors) {}

  @observable public elmCurrentEditor: IGridEditor | null;
  @observable public selectedRecordId: string | undefined;
  @observable public selectedFieldId: string | undefined;
  @observable public isEditing = false;

  public onEditingEnded = EventObserver();

  @action.bound
  public refCurrentEditor(elm: IGridEditor | null) {
    if (elm) {
      if ((elm as any).wrappedInstance) {
        this.elmCurrentEditor = (elm as any).wrappedInstance as IGridEditor;
      }
    } else {
      this.elmCurrentEditor = elm;
    }
  }

  @action.bound
  public finishEditing() {
    // TODO...
    if (this.elmCurrentEditor && this.isSelected) {
      this.elmCurrentEditor!.requestDataCommit(
        this.selectedFieldId!,
        this.selectedRecordId!
      );
      this.isEditing = false;
      this.onEditingEnded.trigger();
      console.log('Editing finished.')
    }
  }

  @action.bound
  public cancelEditing() {
    // TODO...
    this.isEditing = false;
    this.onEditingEnded.trigger();
    console.log('Editing cancelled.')
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

  @computed public get isSelected() {
    return (
      this.selectedRecordId !== undefined && this.selectedFieldId !== undefined
    );
  }
}
