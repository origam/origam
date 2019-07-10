import { observable, computed } from "mobx";
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
    return;
  }
  @computed get selectedRowIndex(): number | undefined {
    return;
  }

  getCellValueByIdx(rowIdx: number, columnIdx: number) {
    const property = this.tableProperties[columnIdx]!;
    const row = this.dataTable.getRowByExistingIdx(rowIdx);
    return this.dataTable.getCellValue(row, property);
  }

  @computed get dataTable() {
    return getDataTable(this);
  }

  parent?: any;
}
