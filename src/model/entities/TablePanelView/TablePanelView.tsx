/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { action, computed, observable, flow } from "mobx";
import { onFieldChangeG } from "model/actions-ui/DataView/TableView/onFieldChange";
import { getSelectedRowIndex } from "model/selectors/DataView/getSelectedRowIndex";
import { getCellValue } from "model/selectors/TablePanelView/getCellValue";
import { getSelectedColumnId } from "model/selectors/TablePanelView/getSelectedColumnId";
import { getSelectedColumnIndex } from "model/selectors/TablePanelView/getSelectedColumnIndex";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getTableViewProperties } from "model/selectors/TablePanelView/getTableViewProperties";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getDataViewPropertyById } from "model/selectors/DataView/getDataViewPropertyById";
import { IFilterConfiguration } from "model/entities/types/IFilterConfiguration";
import { IOrderingConfiguration } from "model/entities/types/IOrderingConfiguration";
import { IProperty } from "model/entities/types/IProperty";
import { IColumnConfigurationDialog } from "model/entities/TablePanelView/types/IColumnConfigurationDialog";
import { ITableCanvas, ITablePanelView, ITablePanelViewData } from "model/entities/TablePanelView/types/ITablePanelView";
import { IGroupingConfiguration } from "model/entities/types/IGroupingConfiguration";
import { IAggregationInfo } from "model/entities/types/IAggregationInfo";
import { AggregationType } from "model/entities/types/AggregationType";
import { ICellRectangle } from "model/entities/TablePanelView/types/ICellRectangle";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { flushCurrentRowData } from "model/actions/DataView/TableView/flushCurrentRowData";
import { isReadOnly } from "model/selectors/RowState/isReadOnly";
import { FilterGroupManager } from "model/entities/FilterGroupManager";
import { handleUserInputOnChangingRow } from "../FormScreenLifecycle/questionSaveDataAfterRecordChange";
import { getGroupingConfiguration } from "model/selectors/TablePanelView/getGroupingConfiguration";
import { getGrouper } from "model/selectors/DataView/getGrouper";
import { IConfigurationManager } from "model/entities/TablePanelView/types/IConfigurationManager";
import {getGridFocusManager} from "model/entities/GridFocusManager";
import {isLazyLoading} from "model/selectors/isLazyLoading";

export class TablePanelView implements ITablePanelView {
  $type_ITablePanelView: 1 = 1;

  constructor(data: ITablePanelViewData) {
    Object.assign(this, data);
    this.columnConfigurationDialog.parent = this;
    this.filterConfiguration.parent = this;
    this.filterGroupManager.parent = this;
    this.orderingConfiguration.parent = this;
    this.groupingConfiguration.parent = this;
  }

  columnConfigurationDialog: IColumnConfigurationDialog = null as any;
  configurationManager: IConfigurationManager = null as any;
  filterConfiguration: IFilterConfiguration = null as any;
  filterGroupManager: FilterGroupManager = null as any;
  orderingConfiguration: IOrderingConfiguration = null as any;
  groupingConfiguration: IGroupingConfiguration = null as any;
  rowHeight: number = null as any;
  firstColumn: IProperty | undefined;
  handleScrolling = true;

  @observable rectangleMap: Map<number, Map<number, ICellRectangle>> = new Map<
    number,
    Map<number, ICellRectangle>
  >();

  @observable isEditing: boolean = false;
  @observable fixedColumnCount: number = 0;
  @observable tablePropertyIds: string[] = [];
  @observable hiddenPropertyIds: Map<string, boolean> = new Map();
  @observable columnOrderChangingTargetId: string | undefined;
  @observable columnOrderChangingSourceId: string | undefined;

  @observable _currentTooltipText?: string = undefined;
  @computed get currentTooltipText() {
    return this._currentTooltipText;
  }
  set currentTooltipText(value: string | undefined) {
    this._currentTooltipText = value;
  }

  parent?: any;
  aggregations: AggregationContainer = new AggregationContainer();

  @computed get propertyMap() {
    return new Map(this.allTableProperties.map((x) => [x.id, x] as [string, IProperty]));
  }

  @computed get allTableProperties() {
    return this.tablePropertyIds
      .map((id) => getDataTable(this).getPropertyById(id))
      .filter((prop) => prop) as IProperty[];
  }

  @computed get tableProperties() {
    return this.allTableProperties.filter((prop) => !this.hiddenPropertyIds.get(prop.id));
  }

  @observable selectedColumnId: string | undefined;

  @computed get selectedRowId(): string | undefined {
    return getDataView(this).selectedRowId;
  }

  @computed get selectedColumnIndex(): number | undefined {
    const idx = this.tableProperties.findIndex((prop) => prop.id === this.selectedColumnId);
    return idx > -1 ? idx : undefined;
  }

  @computed get selectedRowIndex(): number | undefined {
    return getDataView(this).selectedRowIndex;
  }

  @computed get selectedProperty(): IProperty | undefined {
    return this.selectedColumnId ? getDataViewPropertyById(this, this.selectedColumnId) : undefined;
  }

  @observable.ref tableCanvas: ITableCanvas | null = null;

  @action.bound
  setTableCanvas(tableCanvas: ITableCanvas | null): void {
    this.tableCanvas = tableCanvas;
  }

  get firstVisibleRowIndex(): number {
    return this.tableCanvas ? this.tableCanvas.firstVisibleRowIndex : 0;
  }

  get lastVisibleRowIndex(): number {
    return this.tableCanvas ? this.tableCanvas.lastVisibleRowIndex : 0;
  }

  getCellValueByIdx(rowIdx: number, columnIdx: number) {
    const property = this.tableProperties[columnIdx]!;
    const row = this.dataTable.getRowByExistingIdx(rowIdx);
    return this.dataTable.getCellValue(row, property);
  }

  getCellTextByIdx(rowIdx: number, columnIdx: number) {
    const property = this.tableProperties[columnIdx]!;
    const row = this.dataTable.getRowByExistingIdx(rowIdx);
    return this.dataTable.getCellText(row, property);
  }

  clearCurrentCellEditData() {
    const row = getDataView(this).selectedRow;
    const propertyId = this.selectedProperty?.id;
    if (row && propertyId) {
      getDataView(this).dataTable.deleteAdditionalCellData(row, propertyId);
    }
  }

  *onCellClick(event: any, row: any[], columnId: string, isControlInteraction: boolean): any {
    const dataView = getDataView(this);
    const rowId = this.dataTable.getRowId(row);

    getTablePanelView(this).setEditing(false);
    yield* flushCurrentRowData(this)();
    const isDirty = getFormScreen(dataView).isDirty;

    if (isDirty && dataView.selectedRowId !== rowId) {
      const shouldProceedToSelectRow = yield handleUserInputOnChangingRow(dataView);
      if (!shouldProceedToSelectRow) {
        return;
      }
    }
    yield* this.onCellClickInternal(event, row, columnId, isControlInteraction);
  }

  *onCellClickInternal(event: any, row: any[], columnId: string, isControlInteraction: boolean) {
    const property = this.propertyMap.get(columnId)!;
    if (property.column !== "CheckBox" || !isControlInteraction) {
      if (property.isLink && property.column !== "TagInput" && (event.ctrlKey || event.metaKey)) {
        yield* getDataView(this).navigateLookupLink(property, row);
      } else {
        if (this.dataTable.getRowId(row) === this.selectedRowId) {
          this.selectCell(this.dataTable.getRowId(row) as string, property.id);
          this.setEditing(true);
        } else {
          const { isEditing } = this;
          if (isEditing) {
            this.setEditing(false);
          }
          this.selectCell(this.dataTable.getRowId(row) as string, property.id);
          if (isEditing) {
            this.setEditing(true);
          }
        }
      }
    } else {
      const rowId = this.dataTable.getRowId(row);
      yield* this.selectCellAsync(columnId, rowId);

      if (!isReadOnly(property!, rowId)) {
        yield* onFieldChangeG(this)({
          event: undefined,
          row: row,
          property: property,
          value: !getCellValue(this, row, property),
        });
      }
    }
    if (!getGroupingConfiguration(this).isGrouping) {
      this.scrollToCurrentCell();
    }
    if(!isLazyLoading(this)){
      setTimeout(()=>{
        getGridFocusManager(this).focusTableIfNeeded();
      });
    }
  }

  private *selectCellAsync(columnId: string, rowId: string) {
    this.selectedColumnId = columnId;
    const dataView = getDataView(this);
    if (dataView.selectedRowId === rowId) {
      return;
    }
    yield dataView.lifecycle.runRecordChangedReaction(function* () {
      yield dataView.setSelectedRowId(rowId);
    });
  }

  *onNoCellClick() {
    if (this.isEditing) {
      this.setEditing(false);
      yield* flushCurrentRowData(this)();
    }
  }

  @action.bound
  handleTableScroll(event: any, scrollTop: number, scrollLeft: number) {
    if (!this.handleScrolling) {
      this.handleScrolling = true;
      return;
    }

    const _this = this;
    flow(function* () {
      if (_this.isEditing) {
        _this.setEditing(false);
        yield* flushCurrentRowData(_this)();
      }
    })();
  }

  dontHandleNextScroll() {
    this.handleScrolling = false;
  }

  *onOutsideTableClick() {
    if (this.isEditing) {
      this.setEditing(false);
      yield* flushCurrentRowData(this)();
    }
  }

  @action.bound selectCell(rowId: string | undefined, columnId: string | undefined) {
    this.selectedColumnId = columnId;
    getDataView(this).setSelectedRowId(rowId);
  }

  isFirstColumnSelected(): boolean {
    const properties = getTableViewProperties(this);
    const selPropId = getSelectedColumnId(this);
    if (!selPropId) {
      return false;
    }
    const idx = properties.findIndex((prop) => prop.id === selPropId);
    return idx === 0;
  }

  isLastColumnSelected(): boolean {
    const properties = getTableViewProperties(this);
    const selPropId = getSelectedColumnId(this);
    if (!selPropId) {
      return false;
    }
    const idx = properties.findIndex((prop) => prop.id === selPropId);
    return idx === properties.length - 1;
  }

  @action.bound
  selectNextColumn(nextRowWhenEnd?: boolean): void {
    const properties = getTableViewProperties(this);
    const selPropId = getSelectedColumnId(this);
    if (selPropId) {
      const idx = properties.findIndex((prop) => prop.id === selPropId);
      if (idx < properties.length - 1) {
        const newProp = properties[idx + 1];
        this.setSelectedColumnId(newProp.id);
      } else if (nextRowWhenEnd && properties.length > 1) {
        const rowId = getSelectedRowId(this);
        getDataView(this).selectNextRow();
        if (rowId !== getSelectedRowId(this)) {
          this.selectFirstColumn();
        }
      }
    }
  }

  @action.bound
  selectPrevColumn(prevRowWhenStart?: boolean): void {
    const properties = getTableViewProperties(this);
    const selPropId = getSelectedColumnId(this);
    if (selPropId) {
      const idx = properties.findIndex((prop) => prop.id === selPropId);
      if (idx > 0) {
        const newProp = properties[idx - 1];
        this.setSelectedColumnId(newProp.id);
      } else if (prevRowWhenStart && properties.length > 1) {
        const rowId = getSelectedRowId(this);
        getDataView(this).selectPrevRow();
        if (rowId !== getSelectedRowId(this)) {
          this.selectLastColumn();
        }
      }
    }
  }

  @action.bound selectFirstColumn(): void {
    const properties = getTableViewProperties(this);
    const newProp = properties[0];
    this.setSelectedColumnId(newProp.id);
  }

  @action.bound selectLastColumn(): void {
    const properties = getTableViewProperties(this);
    const newProp = properties[properties.length - 1];
    this.setSelectedColumnId(newProp.id);
  }

  @action.bound
  setSelectedColumnId(id: string | undefined): void {
    this.selectedColumnId = id;
  }

  @action.bound
  setEditing(state: boolean): void {
    this.isEditing = state;
  }

  @action.bound
  swapColumns(id1: string, id2: string): void {
    const idx1 = this.tablePropertyIds.findIndex((id) => id === id1);
    const idx2 = this.tablePropertyIds.findIndex((id) => id === id2);
    const tmp = this.tablePropertyIds[idx1];
    this.tablePropertyIds[idx1] = this.tablePropertyIds[idx2];
    this.tablePropertyIds[idx2] = tmp;
  }

  @action.bound
  moveColumnBehind(id1: string, id2: string): void {
    const idx1 = this.tablePropertyIds.findIndex((id) => id === id1);
    const tmp = this.tablePropertyIds[idx1];
    this.tablePropertyIds.splice(idx1, 1);
    const idx2 = this.tablePropertyIds.findIndex((id) => id === id2);
    this.tablePropertyIds.splice(idx2 + 1, 0, tmp);
  }

  @action.bound
  setColumnOrderChangeAttendants(idSource: string | undefined, idTarget: string | undefined): void {
    this.columnOrderChangingTargetId = idTarget;
    this.columnOrderChangingSourceId = idSource;
  }

  subId = 0;
  onScrollToCurrentCellHandlers: Map<
    number,
    (rowIdx: number, columnIdx: number) => void
  > = new Map();

  subOnScrollToCellShortest(fn: (rowIdx: number, columnIdx: number) => void): () => void {
    const myId = this.subId++;
    this.onScrollToCurrentCellHandlers.set(myId, fn);
    return () => this.onScrollToCurrentCellHandlers.delete(myId);
  }

  @action.bound scrollToCurrentRow() {
    const rowIdx = getSelectedRowIndex(this);
    if (rowIdx !== undefined) {
      this.triggerOnScrollToCellShortest(rowIdx, 0);
    }
  }

  @action.bound scrollToCurrentCell() {
    const rowIdx = getSelectedRowIndex(this);
    const columnIdx = getSelectedColumnIndex(this);
    if (rowIdx !== undefined && columnIdx !== undefined) {
      this.triggerOnScrollToCellShortest(rowIdx, columnIdx);
    }
  }

  @action.bound triggerOnScrollToCellShortest(rowIdx: number, columnIdx: number) {
    for (let h of this.onScrollToCurrentCellHandlers.values()) h(rowIdx, columnIdx);
  }

  onFocusTableHandlers: Map<number, () => void> = new Map();

  subOnFocusTable(fn: () => void): () => void {
    const myId = this.subId++;
    this.onFocusTableHandlers.set(myId, fn);
    return () => this.onFocusTableHandlers.delete(myId);
  }

  @action.bound
  setPropertyHidden(propertyId: string, state: boolean): void {
    if (state) {
      this.hiddenPropertyIds.set(propertyId, true);
    } else {
      this.hiddenPropertyIds.delete(propertyId);
    }
  }

  @action.bound triggerOnFocusTable() {
    for (let h of this.onFocusTableHandlers.values()) h();
  }

  @computed get dataTable() {
    return getDataTable(this);
  }

  getCellRectangle(rowIndex: number, columnIndex: number) {
    const groupingConfig = getGroupingConfiguration(this);
    let cellOffset = { row: 0, column: 0 };
    if (groupingConfig.isGrouping) {
      const rowId = getDataView(this).selectedRowId;
      cellOffset = getGrouper(this).getCellOffset(rowId!);
    }
    if (!this.rectangleMap.has(rowIndex + cellOffset.row)) {
      return {
        columnLeft: 0,
        columnWidth: 0,
        rowTop: 0,
        rowHeight: 0,
      };
    }
    return this.rectangleMap.get(rowIndex + cellOffset.row)!.get(columnIndex + cellOffset.column)!;
  }

  setCellRectangle(rowId: number, columnId: number, rectangle: ICellRectangle) {
    if (!this.rectangleMap.has(rowId)) {
      this.rectangleMap.set(rowId, new Map<number, ICellRectangle>());
    }
    this.rectangleMap.get(rowId)!.set(columnId, rectangle);
  }
}

export class AggregationContainer {
  @observable aggregationTypes: Map<string, AggregationType | undefined> = new Map<
    string,
    AggregationType | undefined
  >();

  getType(columnId: string) {
    return this.aggregationTypes.get(columnId);
  }

  setType(columnId: string, aggregationType: AggregationType | undefined) {
    if(!aggregationType && this.aggregationTypes.has(columnId)){
      this.aggregationTypes.delete(columnId);
      return;
    }
    if (!aggregationType && !this.aggregationTypes.has(columnId)) return;

    this.aggregationTypes.set(columnId, aggregationType);
  }

  @computed get aggregationList(): IAggregationInfo[] {
    // @ts-ignore
    return Array.from(this.aggregationTypes.entries())
      .filter((entry) => entry[1])
      .map((entry) => {
        return {
          ColumnName: entry[0],
          AggregationType: entry[1],
        };
      });
  }
}
