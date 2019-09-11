import bind from "bind-decorator";
import { action, computed, observable } from "mobx";
import { inject, observer, Provider } from "mobx-react";
import { onTableKeyDown } from "model/actions/DataView/TableView/onTableKeyDown";
import React from "react";
import { onColumnHeaderClick } from "../../../../model/actions/DataView/TableView/onColumnHeaderClick";
import { ITablePanelView } from "../../../../model/entities/TablePanelView/types/ITablePanelView";
import { IDataView } from "../../../../model/entities/types/IDataView";
import { IProperty } from "../../../../model/entities/types/IProperty";
import { getColumnHeaders } from "../../../../model/selectors/TablePanelView/getColumnHeaders";
import { getIsEditing } from "../../../../model/selectors/TablePanelView/getIsEditing";
import { getRowCount } from "../../../../model/selectors/TablePanelView/getRowCount";
import { getSelectedColumnIndex } from "../../../../model/selectors/TablePanelView/getSelectedColumnIndex";
import { getSelectedRowIndex } from "../../../../model/selectors/TablePanelView/getSelectedRowIndex";
import { getTableViewProperties } from "../../../../model/selectors/TablePanelView/getTableViewProperties";
import { IColumnHeader } from "../../../../model/selectors/TablePanelView/types";
import { ITableColumnsConf } from "../../../Components/Dialogs/ColumnsDialog";
import { FilterSettings } from "../../../Components/ScreenElements/Table/FilterSettings/FilterSettings";
import { Header } from "../../../Components/ScreenElements/Table/Header";
import { SimpleScrollState } from "../../../Components/ScreenElements/Table/SimpleScrollState";
import { Table } from "../../../Components/ScreenElements/Table/Table";
import { IGridDimensions } from "../../../Components/ScreenElements/Table/types";
import { CellRenderer } from "./CellRenderer";
import { TableViewEditor } from "./TableViewEditor";
import { getPropertyOrdering } from "../../../../model/selectors/DataView/getPropertyOrdering";

@inject(({ dataView }) => {
  return {
    dataView,
    tablePanelView: dataView.tablePanelView,
    onColumnDialogCancel: dataView.tablePanelView.onColumnConfCancel,
    onColumnDialogOk: dataView.tablePanelView.onColumnConfSubmit,

    onTableKeyDown: onTableKeyDown(dataView)
  };
})
@observer
export class TableView extends React.Component<{
  dataView?: IDataView;
  tablePanelView?: ITablePanelView;
  onColumnDialogCancel?: (event: any) => void;
  onColumnDialogOk?: (event: any, configuration: ITableColumnsConf) => void;
  onTableKeyDown?: (event: any) => void;
}> {
  gDim = new GridDimensions({
    getTableViewProperties: () => getTableViewProperties(this.props.dataView),
    getRowCount: () => getRowCount(this.props.dataView)
  });
  headerRenderer = new HeaderRenderer({
    tablePanelView: this.props.tablePanelView!,
    getColumnHeaders: () => getColumnHeaders(this.props.dataView),
    getTableViewProperties: () => getTableViewProperties(this.props.dataView),
    onColumnWidthChange: (cid, nw) => this.gDim.setColumnWidth(cid, nw),
    onColumnOrderChange: (id1, id2) =>
      this.props.tablePanelView!.swapColumns(id1, id2),
    onColumnOrderAttendantsChange: (
      idSource: string | undefined,
      idTarget: string | undefined
    ) =>
      this.props.tablePanelView!.setColumnOrderChangeAttendants(
        idSource,
        idTarget
      )
  });
  scrollState = new SimpleScrollState(0, 0);
  cellRenderer = new CellRenderer({
    tablePanelView: this.props.tablePanelView!
  });

  render() {
    const self = this;
    const editingRowIndex = getSelectedRowIndex(this.props.tablePanelView);
    const editingColumnIndex = getSelectedColumnIndex(
      this.props.tablePanelView
    );
    return (
      <Provider tablePanelView={this.props.tablePanelView}>
        <>
          <Table
            gridDimensions={self.gDim}
            scrollState={self.scrollState}
            editingRowIndex={editingRowIndex}
            editingColumnIndex={editingColumnIndex}
            isEditorMounted={getIsEditing(this.props.tablePanelView)}
            fixedColumnCount={0}
            isLoading={false}
            renderHeader={self.headerRenderer.renderHeader}
            renderCell={self.cellRenderer.renderCell}
            renderEditor={() => (
              <TableViewEditor
                key={`${editingRowIndex}@${editingColumnIndex}`}
              />
            )}
            onNoCellClick={this.props.tablePanelView!.onNoCellClick}
            onOutsideTableClick={this.props.tablePanelView!.onOutsideTableClick}
            refCanvasMovingComponent={this.props.tablePanelView!.setTableCanvas}
            onKeyDown={this.props.onTableKeyDown}
          />
        </>
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

  getTableViewProperties: () => IProperty[] = null as any;
  getRowCount: () => number = null as any;

  @computed get tableViewPropertiesOriginal() {
    return this.getTableViewProperties();
  }

  @computed get tableViewProperties() {
    return this.tableViewPropertiesOriginal;
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
    return columnIndex <= 0 ? 0 : this.getColumnRight(columnIndex - 1);
  }

  getColumnWidth(columnIndex: number): number {
    const property = this.tableViewProperties[columnIndex];
    return this.columnWidths.has(property.id)
      ? this.columnWidths.get(property.id)!
      : 100;
  }

  getColumnRight(columnIndex: number): number {
    if (columnIndex < 0) return 0;
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
  getTableViewProperties: () => IProperty[];

  tablePanelView: ITablePanelView;
  getColumnHeaders: () => IColumnHeader[];
  onColumnWidthChange: (id: string, newWidth: number) => void;
  onColumnOrderChange: (id: string, targetId: string) => void;
  onColumnOrderAttendantsChange: (
    idSource: string | undefined,
    idTarget: string | undefined
  ) => void;
}

class HeaderRenderer implements IHeaderRendererData {
  constructor(data: IHeaderRendererData) {
    Object.assign(this, data);
  }

  getTableViewProperties: () => IProperty[] = null as any;

  @computed get tableViewPropertiesOriginal() {
    return this.getTableViewProperties();
  }

  @computed get tableViewProperties() {
    return this.tableViewPropertiesOriginal;
  }

  getColumnHeaders: () => IColumnHeader[] = null as any;
  onColumnWidthChange: (id: string, newWidth: number) => void = null as any;
  onColumnOrderChange: (id: string, targetId: string) => void = null as any;
  onColumnOrderAttendantsChange: (
    idSource: string | undefined,
    idTarget: string | undefined
  ) => void = null as any;
  tablePanelView: ITablePanelView = null as any;

  columnOrderChangeSourceId: string | undefined;
  columnOrderChangeTargetId: string | undefined;

  @computed get columnHeaders() {
    return this.getColumnHeaders();
  }

  @observable isColumnOrderChanging = false;

  @action.bound handleStartColumnOrderChanging(id: string) {
    this.isColumnOrderChanging = true;
    this.columnOrderChangeSourceId = id;
    this.onColumnOrderAttendantsChange(
      this.columnOrderChangeSourceId,
      this.columnOrderChangeTargetId
    );
  }

  @action.bound handleStopColumnOrderChanging(id: string) {
    this.isColumnOrderChanging = false;
    this.columnOrderChangeSourceId = undefined;
    this.columnOrderChangeTargetId = undefined;
    this.onColumnOrderAttendantsChange(
      this.columnOrderChangeSourceId,
      this.columnOrderChangeTargetId
    );
  }

  @action.bound handlePossibleColumnOrderChange(targetId: string | undefined) {
    this.columnOrderChangeTargetId = targetId;
    this.onColumnOrderAttendantsChange(
      this.columnOrderChangeSourceId,
      this.columnOrderChangeTargetId
    );
  }

  @action.bound handleColumnOrderDrop(targetId: string) {
    this.onColumnOrderChange(this.columnOrderChangeSourceId!, targetId);
    this.onColumnOrderAttendantsChange(
      this.columnOrderChangeSourceId,
      this.columnOrderChangeTargetId
    );
  }

  @bind
  renderHeader(args: { columnIndex: number; columnWidth: number }) {
    const property = this.tableViewProperties[args.columnIndex];
    const header = this.columnHeaders[args.columnIndex];
    return (
      <Provider property={property} key={property.id}>
        <Header
          key={header.id}
          id={header.id}
          width={args.columnWidth}
          label={header.label}
          orderingDirection={header.ordering}
          orderingOrder={header.order}
          onColumnWidthChange={this.onColumnWidthChange}
          isColumnOrderChanging={this.isColumnOrderChanging}
          onColumnOrderDrop={this.handleColumnOrderDrop}
          onStartColumnOrderChanging={this.handleStartColumnOrderChanging}
          onStopColumnOrderChanging={this.handleStopColumnOrderChanging}
          onPossibleColumnOrderChange={this.handlePossibleColumnOrderChange}
          onClick={onColumnHeaderClick(this.tablePanelView)}
          additionalHeaderContent={
            this.tablePanelView.filterConfiguration.isFilterControlsDisplayed
              ? () => <FilterSettings />
              : undefined
          }
        />
      </Provider>
    );
  }
}
