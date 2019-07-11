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
import { computed } from "mobx";
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
    getColumnHeaders: () => getColumnHeaders(this.props.dataView)
  });
  scrollState = new SimpleScrollState(0, 0);
  cellRenderer = new CellRenderer({
    tablePanelView: getTablePanelView(this.props.dataView)
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
          isEditorMounted={true}
          fixedColumnCount={0}
          isLoading={false}
          renderHeader={self.headerRenderer.renderHeader}
          renderCell={self.cellRenderer.renderCell}
          renderEditor={() => <TableViewEditor />}
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
    return columnIndex * 100;
  }

  getColumnWidth(columnIndex: number): number {
    return 100;
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
}

interface IHeaderRendererData {
  getColumnHeaders: () => IColumnHeader[];
}

class HeaderRenderer {
  constructor(data: IHeaderRendererData) {
    Object.assign(this, data);
  }

  getColumnHeaders: () => IColumnHeader[] = null as any;

  @computed get columnHeaders() {
    return this.getColumnHeaders();
  }

  @bind
  renderHeader(args: { columnIndex: number; columnWidth: number }) {
    return (
      <Header
        key={this.columnHeaders[args.columnIndex].id}
        width={args.columnWidth}
        label={this.columnHeaders[args.columnIndex].label}
        orderingDirection={this.columnHeaders[args.columnIndex].ordering}
        orderingOrder={0}
      />
    );
  }
}
