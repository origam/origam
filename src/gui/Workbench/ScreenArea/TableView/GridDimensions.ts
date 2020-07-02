import {IGridDimensions} from "../../../Components/ScreenElements/Table/types";
import {action, computed} from "mobx";
import {IProperty} from "../../../../model/entities/types/IProperty";
import {getLeadingColumnCount} from "../../../../model/selectors/TablePanelView/getLeadingColumnCount";
import {getIsSelectionCheckboxesShown} from "../../../../model/selectors/DataView/getIsSelectionCheckboxesShown";
import {getGroupingConfiguration} from "../../../../model/selectors/TablePanelView/getGroupingConfiguration";
import {getTableViewProperties} from "model/selectors/TablePanelView/getTableViewProperties";

export interface IGridDimensionsData {
  getTableViewProperties: () => IProperty[];
  getRowCount: () => number;
  getIsSelectionCheckboxes: () => boolean;
  ctx: any;
  defaultRowHeight: number;
}

export class GridDimensions implements IGridDimensions {
  constructor(data: IGridDimensionsData) {
    Object.assign(this, data);
  }

  @computed get columnWidths(): Map<string, number> {
    return new Map(this.tableViewProperties.map((prop) => [prop.id, prop.columnWidth]));
  }

  getTableViewProperties: () => IProperty[] = null as any;
  getRowCount: () => number = null as any;
  getIsSelectionCheckboxes: () => boolean = null as any;
  ctx: any;
  defaultRowHeight: number = null as any;

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
      this.tableViewProperties.length
    );
  }

  get contentWidth() {
    return this.getColumnRight(this.columnCount - 1);
  }

  get contentHeight() {
    return this.getRowBottom(this.rowCount - 1);
  }

  getColumnLeft(dataColumnIndex: number): number {
    const displayedColumnIndex = this.dataColumnIndexToDisplayedIndex(dataColumnIndex);
    return this.displayedColumnDimensionsCom[displayedColumnIndex].left;
  }

  getColumnRight(dataColumnIndex: number): number {
    const displayedColumnIndex = this.dataColumnIndexToDisplayedIndex(dataColumnIndex);
    return this.displayedColumnDimensionsCom[displayedColumnIndex].right;
  }

  dataColumnIndexToDisplayedIndex(dataColumnIndex: number) {
    return dataColumnIndex + getLeadingColumnCount(this.ctx);
  }

  getRowTop(rowIndex: number): number {
    return rowIndex * this.getRowHeight(rowIndex);
  }

  getRowHeight(rowIndex: number): number {
    return this.defaultRowHeight;
  }

  getRowBottom(rowIndex: number): number {
    return this.getRowTop(rowIndex) + this.getRowHeight(rowIndex);
  }

  @action.bound setColumnWidth(columnId: string, newWidth: number) {
    this.columnWidths.set(columnId, Math.max(newWidth, 20));
  }

  @computed get displayedColumnDimensionsCom(): { left: number; width: number; right: number; }[] {
    const isCheckBoxedTable = getIsSelectionCheckboxesShown(this.ctx);
    const groupedColumnIds = getGroupingConfiguration(this.ctx).orderedGroupingColumnIds;
    const tableColumnIds = getTableViewProperties(this.ctx).map(prop => prop.id)
    const columnWidths = this.columnWidths;

    const widths = Array.from(
      (function* () {
        if (isCheckBoxedTable) yield 20;
        yield* groupedColumnIds.map((id) => 20);
        yield* tableColumnIds
          .map((id) => columnWidths.get(id))
          .filter((width) => width !== undefined) as number[];
      })()
    );
    let acc = 0;
    return Array.from(
      (function* () {
        for (let w of widths) {
          yield {
            left: acc,
            width: w,
            right: acc + w,
          };
          acc = acc + w;
        }
      })()
    );
  }

}