import { observable, computed, action } from "mobx";
import {
  ITablePanelView,
  CTablePanelView,
  ITablePanelViewData
} from "./types/ITablePanelView";
import { getDataView } from "../selectors/DataView/getDataView";
import { getDataTable } from "../selectors/DataView/getDataTable";
import { IProperty } from "../types/IProperty";

export class TablePanelView implements ITablePanelView {

  $type: typeof CTablePanelView = "CTablePanelView";

  constructor(data: ITablePanelViewData) {
    Object.assign(this, data);
  }

  @observable tablePropertyIds = [];
  @computed get tableProperties() {
    return this.tablePropertyIds.map(id =>
      getDataTable(this).getPropertyById(id)
    ) as IProperty[];
  }
  @observable selectedColumnId: string | undefined;
  @computed get selectedRowId(): string | undefined {
    return getDataView(this).selectedRowId;
  }

  @computed get selectedColumnIndex(): number | undefined {
    const idx = this.tableProperties.findIndex(
      prop => prop.id === this.selectedColumnId
    );
    return idx > -1 ? idx : undefined;
  }
  @computed get selectedRowIndex(): number | undefined {
    return this.selectedRowId
      ? this.dataTable.getExistingRowIdxById(this.selectedRowId)
      : undefined;
  }

  getCellValueByIdx(rowIdx: number, columnIdx: number) {
    const property = this.tableProperties[columnIdx]!;
    const row = this.dataTable.getRowByExistingIdx(rowIdx);
    return this.dataTable.getCellValue(row, property);
  }

  @action.bound
  onCellClick(rowIndex: number, columnIndex: number): void {
    console.log("CellClicked:", rowIndex, columnIndex);
    const row = this.dataTable.getRowByExistingIdx(rowIndex);
    const property = this.tableProperties[columnIndex];
    this.selectedColumnId = property.id;
    getDataView(this).setSelectedRowId(row[0] as string);
  }
  
  @action.bound
  setSelectedColumnId(id: string | undefined): void {
    this.selectedColumnId = id;
  }

  @computed get dataTable() {
    return getDataTable(this);
  }

  parent?: any;
}
