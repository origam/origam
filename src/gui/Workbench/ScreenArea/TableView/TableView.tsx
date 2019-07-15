import React from "react";
import { observer, inject, Provider } from "mobx-react";
import { SimpleScrollState } from "../../../Components/ScreenElements/Table/SimpleScrollState";
import { CellRenderer } from "./CellRenderer";
import { Table } from "../../../Components/ScreenElements/Table/Table";
import {
  IGridDimensions,
  IOrderByDirection
} from "../../../Components/ScreenElements/Table/types";
import bind from "bind-decorator";
import { Header } from "../../../Components/ScreenElements/Table/Header";
import { IProperty } from "../../../../model/types/IProperty";
import { computed, observable, action } from "mobx";
import { IDataView } from "../../../../model/types/IDataView";
import { getTableViewProperties } from "../../../../model/selectors/TablePanelView/getTableViewProperties";
import { getColumnHeaders } from "../../../../model/selectors/TablePanelView/getColumnHeaders";
import { IColumnHeader } from "../../../../model/selectors/TablePanelView/types";
import { getCellValueByIdx } from "../../../../model/selectors/TablePanelView/getCellValue";
import { getRowCount } from "../../../../model/selectors/TablePanelView/getRowCount";
import { getTablePanelView } from "../../../../model/selectors/TablePanelView/getTablePanelView";
import { DateTimeEditor } from "../../../Components/ScreenElements/Editors/DateTimeEditor";
import moment from "moment";
import { TableViewEditor } from "./TableViewEditor";
import { ITablePanelView } from "../../../../model/TablePanelView/types/ITablePanelView";
import { getSelectedRowIndex } from "../../../../model/selectors/TablePanelView/getSelectedRowIndex";
import { getSelectedColumnIndex } from "../../../../model/selectors/TablePanelView/getSelectedColumnIndex";
import { getIsEditing } from "../../../../model/selectors/TablePanelView/getIsEditing";
import { TablePanelView } from "../../../../model/TablePanelView/TablePanelView";

@inject(({ dataView }) => {
  return {
    dataView,
    tablePanelView: dataView.tablePanelView
  };
})
@observer
export class TableView extends React.Component<{
  dataView?: IDataView;
  tablePanelView?: ITablePanelView;
}> {
  gDim = new GridDimensions({
    getTableViewProperties: () => getTableViewProperties(this.props.dataView),
    getRowCount: () => getRowCount(this.props.dataView)
  });
  headerRenderer = new HeaderRenderer({
    getColumnHeaders: () => getColumnHeaders(this.props.dataView),
    onColumnWidthChange: (cid, nw) => this.gDim.setColumnWidth(cid, nw)
  });
  scrollState = new SimpleScrollState(0, 0);
  cellRenderer = new CellRenderer({
    tablePanelView: this.props.tablePanelView!
  });

  render() {
    const self = this;
    return (
      <Provider tablePanelView={this.props.tablePanelView}>
        <Table
          gridDimensions={self.gDim}
          scrollState={self.scrollState}
          editingRowIndex={getSelectedRowIndex(this.props.tablePanelView)}
          editingColumnIndex={getSelectedColumnIndex(this.props.tablePanelView)}
          isEditorMounted={getIsEditing(this.props.tablePanelView)}
          fixedColumnCount={0}
          isLoading={false}
          renderHeader={self.headerRenderer.renderHeader}
          renderCell={self.cellRenderer.renderCell}
          renderEditor={() => <TableViewEditor />}
          onNoCellClick={this.props.tablePanelView!.onNoCellClick}
          onOutsideTableClick={this.props.tablePanelView!.onOutsideTableClick}
        />
      </Provider>
    );
  }
}

interface IGridDimensionsData {
  getTableViewProperties: () => IProperty[];
  getRowCount: () => number;
}

class GridDimensions implements IGridDimensions {
  constructor(data: IGridDimensionsData) {
    Object.assign(this, data);
  }

  @observable columnWidths: Map<string, number> = new Map();
  @observable columnReordering: string[] = [];

  getTableViewProperties: () => IProperty[] = null as any;
  getRowCount: () => number = null as any;

  @computed get tableViewProperties() {
    return this.getTableViewProperties();
  }

  @computed get rowCount() {
    return this.getRowCount();
  }

  @computed get columnCount() {
    return this.tableViewProperties.length;
  }

  get contentWidth() {
    return this.getColumnRight(this.columnCount - 1);
  }

  get contentHeight() {
    return this.getRowBottom(this.rowCount - 1);
  }

  getColumnLeft(columnIndex: number): number {
    return columnIndex === 0 ? 0 : this.getColumnRight(columnIndex - 1);
  }

  getColumnWidth(columnIndex: number): number {
    const property = this.tableViewProperties[columnIndex];
    return this.columnWidths.has(property.id)
      ? this.columnWidths.get(property.id)!
      : 100;
  }

  getColumnRight(columnIndex: number): number {
    return this.getColumnLeft(columnIndex) + this.getColumnWidth(columnIndex);
  }

  getRowTop(rowIndex: number): number {
    return rowIndex * 20;
  }

  getRowHeight(rowIndex: number): number {
    return 20;
  }

  getRowBottom(rowIndex: number): number {
    return this.getRowTop(rowIndex) + this.getRowHeight(rowIndex);
  }

  @action.bound setColumnWidth(columnId: string, newWidth: number) {
    this.columnWidths.set(columnId, Math.max(newWidth, 20));
  }
}

interface IHeaderRendererData {
  getColumnHeaders: () => IColumnHeader[];
  onColumnWidthChange: (id: string, newWidth: number) => void;
}

class HeaderRenderer implements IHeaderRendererData {
  constructor(data: IHeaderRendererData) {
    Object.assign(this, data);
  }

  getColumnHeaders: () => IColumnHeader[] = null as any;
  onColumnWidthChange: (id: string, newWidth: number) => void = null as any;

  @computed get columnHeaders() {
    return this.getColumnHeaders();
  }

  @bind
  renderHeader(args: { columnIndex: number; columnWidth: number }) {
    return (
      <Header
        key={this.columnHeaders[args.columnIndex].id}
        id={this.columnHeaders[args.columnIndex].id}
        width={args.columnWidth}
        label={this.columnHeaders[args.columnIndex].label}
        orderingDirection={this.columnHeaders[args.columnIndex].ordering}
        orderingOrder={0}
        onColumnWidthChange={this.onColumnWidthChange}
      />
    );
  }
}
