import { IEventSubscriber } from "../utils/events";
export type ICellValue = string | number | boolean | undefined;

export enum ICellState {
  Loading,
  Loaded
}

export interface IRecord {
  isDirtyDeleted: boolean;
}

export interface IField {}

export interface IDataTableSelectors {
  recordById(id: string): IRecord | undefined;
  fieldById(id: string): IField | undefined;
  recordByIndex(idx: number): IRecord | undefined;
  fieldByIndex(idx: number): IField | undefined;
  recordIndexById(id: string): number | undefined;
  fieldIndexById(id: string): number | undefined;
  recordIdByIndex(idx: number): string | undefined;
  fieldIdByIndex(idx: number): string | undefined;
  recordIdAfterId(id: string): string | undefined;
  fieldIdAfterId(id: string): string | undefined;
  recordIdBeforeId(id: string): string | undefined;
  fieldIdBeforeId(id: string): string | undefined;
}

export interface IDataCursorState {
  selectedRecordId: string | undefined;
  selectedFieldId: string | undefined;
  isEditing: boolean;
  selectField(fieldId: string): void;
  selectRecord(recordId: string): void;
  selectCell(recordId: string, fieldId: string): void;
  selectNextRow(): void;
  selectPrevRow(): void;
  selectNextColumn(): void;
  selectPrevColumn(): void;
  editSelected(): void;
  finishEditing(): void;
  cancelEditing(): void;
  isCellSelected(recordId: string, fieldId: string): boolean;
  refCurrentEditor(elm: IGridEditor | null): void;
  onEditingEnded: IEventSubscriber;
}

export interface IGridTableEvents {
  handleCellClick(event: any, rowIndex: number, columnIndex: number): void;
  handleNoCellClick(event: any): void;
  handleOutsideClick(event: any): void;
  handleGridKeyDown(event: any): void;
  handleDefaultEditorKeyDown(event: any): void;
  onCursorMovementFinished: IEventSubscriber;
}

export interface IGridEditor {
  requestDataCommit(recordId: string, fieldId: string): void;
}
