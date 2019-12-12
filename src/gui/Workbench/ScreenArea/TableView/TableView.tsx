import bind from "bind-decorator";
import { action, computed, observable } from "mobx";
import { inject, observer, Provider } from "mobx-react";
import { onTableKeyDown } from "model/actions-ui/DataView/TableView/onTableKeyDown";
import React from "react";
import { onColumnHeaderClick } from "../../../../model/actions-ui/DataView/TableView/onColumnHeaderClick";
import { ITablePanelView } from "../../../../model/entities/TablePanelView/types/ITablePanelView";
import { IDataView } from "../../../../model/entities/types/IDataView";
import { IProperty } from "../../../../model/entities/types/IProperty";
import { getColumnHeaders } from "../../../../model/selectors/TablePanelView/getColumnHeaders";
import { getIsEditing } from "../../../../model/selectors/TablePanelView/getIsEditing";
import { getRowCount } from "../../../../model/selectors/TablePanelView/getRowCount";
import { getSelectedColumnIndex } from "../../../../model/selectors/TablePanelView/getSelectedColumnIndex";
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
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { getSelectedRowIndex } from "model/selectors/DataView/getSelectedRowIndex";
import { onNoCellClick } from "model/actions-ui/DataView/TableView/onNoCellClick";
import { onOutsideTableClick } from "model/actions-ui/DataView/TableView/onOutsideTableClick";
import { getFixedColumnsCount } from "model/selectors/TablePanelView/getFixedColumnsCount";
import { getIsSelectionCheckboxesShown } from "model/selectors/DataView/getIsSelectionCheckboxesShown";

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
  refTableDisposer: any;
  refTable = (elmTable: Table | null) => {
    this.elmTable = elmTable;
    if (elmTable) {
      const d1 = this.props.tablePanelView!.subOnFocusTable(
        elmTable.focusTable
      );
      const d2 = this.props.tablePanelView!.subOnScrollToCellShortest(
        elmTable.scrollToCellShortest
      );
      this.refTableDisposer = () => {
        d1();
        d2();
      };
    } else {
      this.refTableDisposer && this.refTableDisposer();
    }
  };
  elmTable: Table | null = null;

  gDim = new GridDimensions({
    getTableViewProperties: () => getTableViewProperties(this.props.dataView),
    getRowCount: () => getRowCount(this.props.dataView),
    getIsSelectionCheckboxes: () =>
      getIsSelectionCheckboxesShown(this.props.tablePanelView)
  });

  headerRenderer = new HeaderRenderer({
    tablePanelView: this.props.tablePanelView!,
    getIsSelectionCheckboxes: () =>
      getIsSelectionCheckboxesShown(this.props.tablePanelView),
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
    const isSelectionCheckboxes = getIsSelectionCheckboxesShown(
      this.props.tablePanelView
    );
    const editingRowIndex = getSelectedRowIndex(this.props.tablePanelView);
    let editingColumnIndex = getSelectedColumnIndex(this.props.tablePanelView);
    editingColumnIndex =
      editingColumnIndex &&
      editingColumnIndex + (isSelectionCheckboxes ? 1 : 0);

    return (
      <Provider tablePanelView={this.props.tablePanelView}>
        <>
          <Table
            gridDimensions={self.gDim}
            scrollState={self.scrollState}
            editingRowIndex={editingRowIndex}
            editingColumnIndex={editingColumnIndex}
            isEditorMounted={getIsEditing(this.props.tablePanelView)}
            fixedColumnCount={
              getFixedColumnsCount(this.props.tablePanelView) +
              (isSelectionCheckboxes ? 1 : 0)
            }
            isLoading={false}
            renderHeader={self.headerRenderer.renderHeader}
            renderCell={self.cellRenderer.renderCell}
            renderEditor={() => (
              <TableViewEditor
                key={`${editingRowIndex}@${editingColumnIndex}`}
              />
            )}
            onNoCellClick={onNoCellClick(this.props.tablePanelView)}
            onOutsideTableClick={onOutsideTableClick(this.props.tablePanelView)}
            refCanvasMovingComponent={this.props.tablePanelView!.setTableCanvas}
            onKeyDown={this.props.onTableKeyDown}
            ref={this.refTable}
          />
        </>
      </Provider>
    );
  }
}

interface IGridDimensionsData {
  getTableViewProperties: () => IProperty[];
  getRowCount: () => number;
  getIsSelectionCheckboxes: () => boolean;
}

class GridDimensions implements IGridDimensions {
  constructor(data: IGridDimensionsData) {
    Object.assign(this, data);
  }

  @observable columnWidths: Map<string, number> = new Map();

  getTableViewProperties: () => IProperty[] = null as any;
  getRowCount: () => number = null as any;
  getIsSelectionCheckboxes: () => boolean = null as any;

  @computed get isSelectionCheckboxes() {
    return this.getIsSelectionCheckboxes();
  }

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
    return (
      this.tableViewProperties.length + (this.isSelectionCheckboxes ? 1 : 0)
    );
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
    let colIdx = columnIndex;
    if (this.isSelectionCheckboxes) {
      if (colIdx === 0) {
        return 30;
      }
      colIdx--;
    }
    const property = this.tableViewProperties[colIdx];
    return this.columnWidths.has(property.id)
      ? this.columnWidths.get(property.id)!
      : 100;
  }

  getColumnRight(columnIndex: number): number {
    if (columnIndex < 0) return 0;
    return this.getColumnLeft(columnIndex) + this.getColumnWidth(columnIndex);
  }

  getRowTop(rowIndex: number): number {
    return rowIndex * 24;
  }

  getRowHeight(rowIndex: number): number {
    return 24;
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
  getIsSelectionCheckboxes: () => boolean;
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
  getIsSelectionCheckboxes: () => boolean = null as any;

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

  @computed get isSelectionCheckboxes() {
    return this.getIsSelectionCheckboxes();
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
    let property;
    let header;
    if (this.isSelectionCheckboxes) {
      if (args.columnIndex === 0) {
        return null;
      } else {
        property = this.tableViewProperties[args.columnIndex - 1];
        header = this.columnHeaders[args.columnIndex - 1];
      }
    } else {
      property = this.tableViewProperties[args.columnIndex];
      header = this.columnHeaders[args.columnIndex];
    }
    return (
      <Provider key={property.id} property={property}>
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
