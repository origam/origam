import { ValueBox } from "./common/ValueBox";
import { ITableRow, IClickSubsItem } from "./types";
import { Memoized } from "./common/Memoized";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { getTableViewPropertyByIdx } from "model/selectors/TablePanelView/getTableViewPropertyByIdx";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getTableViewRecordByExistingIdx } from "model/selectors/TablePanelView/getTableViewRecordByExistingIdx";
import { IProperty } from "model/entities/types/IProperty";
import { getIsSelectionCheckboxesShown } from "model/selectors/DataView/getIsSelectionCheckboxesShown";

export const scRenderTable: Array<() => void> = [];
export const scRenderRow: Array<() => void> = [];
export const scRenderCell: Array<() => void> = [];

export const context = ValueBox<any>();
scRenderTable.push(() => context.clear());

export const context2d = ValueBox<CanvasRenderingContext2D>();
scRenderTable.push(() => context2d.clear());

export const rowIndex = ValueBox<number>();
scRenderRow.push(() => rowIndex.clear());

export const selectionColumnShown = () =>  getIsSelectionCheckboxesShown(context());

export const columnIndex = ValueBox<number>();
scRenderCell.push(() => columnIndex.clear());

export const tableRows = ValueBox<ITableRow[]>();
scRenderTable.push(() => tableRows.clear());

export const viewportWidth = ValueBox<number>();
scRenderTable.push(() => viewportWidth.clear());

export const viewportHeight = ValueBox<number>();
scRenderTable.push(() => viewportHeight.clear());

export const scrollLeft = ValueBox<number>();
scRenderTable.push(() => scrollLeft.clear());

export const scrollTop = ValueBox<number>();
scRenderTable.push(() => scrollTop.clear());

export const viewportLeft = scrollLeft;

export const viewportTop = scrollTop;

export const viewportRight = () => viewportLeft() + viewportWidth();

export const viewportBottom = () => viewportTop() + viewportHeight();

export const worldWidth = Memoized(() => gridLeadCellDimensions().slice(-1)[0].right)
scRenderTable.push(() => worldWidth.clear());

export const worldHeight = Memoized(() => tableRowsCount() * rowHeight())
scRenderTable.push(() => worldHeight.clear());

export const rowHeight = ValueBox<number>();
scRenderTable.push(() => rowHeight.clear());

export const tableColumnIds = ValueBox<string[]>();
scRenderTable.push(() => tableColumnIds.clear());

export const groupingColumnIds = ValueBox<string[]>();
scRenderTable.push(() => groupingColumnIds.clear());

export const groupingColumnCount = () => groupingColumnIds().length;

export const isGrouping = () => groupingColumnIds().length > 0;

export const isCheckboxedTable = ValueBox<boolean>();
scRenderTable.push(isCheckboxedTable.clear);

export const fixedColumnCount = ValueBox<number>();
scRenderTable.push(fixedColumnCount.clear);

export const columnWidths = ValueBox<Map<string, number>>();
scRenderTable.push(columnWidths.clear);

export const realFixedColumnCount = () =>
  isCheckboxedTable() ? fixedColumnCount() + 1 : fixedColumnCount();

export const gridLeadCellDimensions = ValueBox<{ left: number; width: number; right: number }[]>();
scRenderTable.push(gridLeadCellDimensions.clear);

export const propertyById = ValueBox<Map<string, IProperty>>();
scRenderTable.push(propertyById.clear)

export const tableRowsCount = () => tableRows().length

export const clickSubscriptions = ValueBox<IClickSubsItem[]>();
scRenderTable.push(clickSubscriptions.clear)

export const tablePanelView = () => getTablePanelView(context());
export const property = () => getTableViewPropertyByIdx(tablePanelView(), columnIndex());
export const dataTable = () => getDataTable(tablePanelView());
export const recordId = () => {
  const record = getTableViewRecordByExistingIdx(tablePanelView(), rowIndex());
  return dataTable().getRowId(record);
} 