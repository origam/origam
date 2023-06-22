export interface IDriverState {
  chosenRowId: string | string[] | null;
  cursorRowId: string | undefined;

  handleTableCellClicked(event: any, visibleRowIndex: any): void;
}