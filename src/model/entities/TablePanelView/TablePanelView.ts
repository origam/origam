import { action, computed, observable } from "mobx";
import { onFieldChangeG } from "model/actions-ui/DataView/TableView/onFieldChange";
import { getSelectedRowIndex } from "model/selectors/DataView/getSelectedRowIndex";
import { getCellValue } from "model/selectors/TablePanelView/getCellValue";
import { getSelectedColumnId } from "model/selectors/TablePanelView/getSelectedColumnId";
import { getSelectedColumnIndex } from "model/selectors/TablePanelView/getSelectedColumnIndex";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getTableViewProperties } from "model/selectors/TablePanelView/getTableViewProperties";
import { getDataTable } from "../../selectors/DataView/getDataTable";
import { getDataView } from "../../selectors/DataView/getDataView";
import { getDataViewPropertyById } from "../../selectors/DataView/getDataViewPropertyById";
import { IFilterConfiguration } from "../types/IFilterConfiguration";
import { IOrderingConfiguration } from "../types/IOrderingConfiguration";
import { IProperty } from "../types/IProperty";
import { IColumnConfigurationDialog } from "./types/IColumnConfigurationDialog";
import {
  ITableCanvas,
  ITablePanelView,
  ITablePanelViewData
} from "./types/ITablePanelView";
import { onMainMenuItemClick } from "model/actions-ui/MainMenu/onMainMenuItemClick";

import selectors from "model/selectors-tree";
import { IGroupingConfiguration } from "../types/IGroupingConfiguration";
import {IAggregationInfo} from "../types/IAggregationInfo";
import {AggregationType} from "../types/AggregationType";
import {ICellRectangle} from "./types/ICellRectangle";

export class TablePanelView implements ITablePanelView {
  $type_ITablePanelView: 1 = 1;

  constructor(data: ITablePanelViewData) {
    Object.assign(this, data);
    this.columnConfigurationDialog.parent = this;
    this.filterConfiguration.parent = this;
    this.orderingConfiguration.parent = this;
    this.groupingConfiguration.parent = this;
  }

  columnConfigurationDialog: IColumnConfigurationDialog = null as any;
  filterConfiguration: IFilterConfiguration = null as any;
  orderingConfiguration: IOrderingConfiguration = null as any;
  groupingConfiguration: IGroupingConfiguration = null as any;
  rowHeight: number = null as any;

  rectangleMap: Map<number, Map<number, ICellRectangle>> = new Map<number, Map<number, ICellRectangle>>();

  @observable isEditing: boolean = false;
  @observable fixedColumnCount: number = 0;
  @observable tablePropertyIds: string[] = [];
  @observable hiddenPropertyIds: Map<string, boolean> = new Map();
  @observable columnOrderChangingTargetId: string | undefined;
  @observable columnOrderChangingSourceId: string | undefined;

  parent?: any;
  aggregations: AggregationContainer = new AggregationContainer();

  @computed get propertyMap() {
    return new Map(
      this.allTableProperties.map(x => [x.id, x] as [string, IProperty])
    );
  }

  @computed get allTableProperties() {
    return this.tablePropertyIds.map(id =>
      getDataTable(this).getPropertyById(id)
    ) as IProperty[];
  }

  @computed get tableProperties() {
    return this.allTableProperties.filter(
      prop => !this.hiddenPropertyIds.get(prop.id)
    );
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
    return getDataView(this).selectedRowIndex;
  }

  @computed get selectedProperty(): IProperty | undefined {
    return this.selectedColumnId
      ? getDataViewPropertyById(this, this.selectedColumnId)
      : undefined;
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

  *onCellClick(event: any, row: any[], columnId: string) {
    const property = this.propertyMap.get(columnId)!;
    if (property.column !== "CheckBox") {
      if (property.isLink && event.ctrlKey) {
        const menuId = selectors.column.getLinkMenuId(property);
        const menuItem = menuId && selectors.mainMenu.getItemById(this, menuId);
        if (menuItem) {
          yield onMainMenuItemClick(this)({
            event: undefined,
            item: menuItem
          });
        }
      } else {
        if (
          this.dataTable.getRowId(row) ===
          this.selectedRowId /*&&
        property.id === this.selectedColumnId*/
        ) {
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
      this.selectCell(this.dataTable.getRowId(row) as string, property.id);
      yield* onFieldChangeG(this)(
        undefined,
        row,
        property,
        !getCellValue(this, row, property)
      );
    }
    this.scrollToCurrentCell();
  }

  *onNoCellClick() {
    if (this.isEditing) {
      this.setEditing(false);
    }
  }

  *onOutsideTableClick() {
    if (this.isEditing) {
      this.setEditing(false);
    }
  }

  @action.bound selectCell(
    rowId: string | undefined,
    columnId: string | undefined
  ) {
    this.selectedColumnId = columnId;
    getDataView(this).selectRowById(rowId);
  }

  @action.bound
  selectNextColumn(nextRowWhenEnd?: boolean): void {
    const properties = getTableViewProperties(this);
    const selPropId = getSelectedColumnId(this);
    if (selPropId) {
      const idx = properties.findIndex(prop => prop.id === selPropId);
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
      const idx = properties.findIndex(prop => prop.id === selPropId);
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
    const idx1 = this.tablePropertyIds.findIndex(id => id === id1);
    const idx2 = this.tablePropertyIds.findIndex(id => id === id2);
    const tmp = this.tablePropertyIds[idx1];
    this.tablePropertyIds[idx1] = this.tablePropertyIds[idx2];
    this.tablePropertyIds[idx2] = tmp;
  }

  @action.bound
  setColumnOrderChangeAttendants(
    idSource: string | undefined,
    idTarget: string | undefined
  ): void {
    this.columnOrderChangingTargetId = idTarget;
    this.columnOrderChangingSourceId = idSource;
  }

  subId = 0;
  onScrollToCurrentCellHandlers: Map<
    number,
    (rowIdx: number, columnIdx: number) => void
  > = new Map();
  subOnScrollToCellShortest(
    fn: (rowIdx: number, columnIdx: number) => void
  ): () => void {
    const myId = this.subId++;
    this.onScrollToCurrentCellHandlers.set(myId, fn);
    return () => this.onScrollToCurrentCellHandlers.delete(myId);
  }

  @action.bound scrollToCurrentCell() {
    const rowIdx = getSelectedRowIndex(this);
    const columnIdx = getSelectedColumnIndex(this);
    if (rowIdx !== undefined && columnIdx !== undefined) {
      this.triggerOnScrollToCellShortest(rowIdx, columnIdx);
    }
  }

  @action.bound triggerOnScrollToCellShortest(
    rowIdx: number,
    columnIdx: number
  ) {
    for (let h of this.onScrollToCurrentCellHandlers.values())
      h(rowIdx, columnIdx);
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

  getCellRectangle(rowIndex: number, columnIndex: number){
    return this.rectangleMap.get(rowIndex)!.get(columnIndex)!;
  }

  setCellRectangle(rowId: number, columnId: number, rectangle: ICellRectangle) {
    if(!this.rectangleMap.has(rowId)){
      this.rectangleMap.set(rowId, new Map<number, ICellRectangle>());
    }
    this.rectangleMap.get(rowId)!.set(columnId, rectangle);
  }
}

export class AggregationContainer{
  @observable aggregationTypes: Map<string, AggregationType | undefined> = new Map<string, AggregationType | undefined>();

  getType(columnId: string){
    return this.aggregationTypes.get(columnId)
  }

  setType(columnId: string, aggregationType: AggregationType | undefined){
    if(!aggregationType && !this.aggregationTypes.has(columnId)) return;
    this.aggregationTypes.set(columnId, aggregationType);
  }

  @computed get aggregationList(): IAggregationInfo[]{
    // @ts-ignore
    return Array.from(this.aggregationTypes.entries())
      .filter(entry => entry[1])
      .map(entry => {
        return  {
          "ColumnName": entry[0],
          "AggregationType": entry[1]
        }
      });
  }
}
