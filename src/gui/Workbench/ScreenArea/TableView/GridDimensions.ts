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

import { IGridDimensions } from "../../../Components/ScreenElements/Table/types";
import { action, computed } from "mobx";
import { IProperty } from "../../../../model/entities/types/IProperty";
import { getLeadingColumnCount } from "../../../../model/selectors/TablePanelView/getLeadingColumnCount";
import { getIsSelectionCheckboxesShown } from "../../../../model/selectors/DataView/getIsSelectionCheckboxesShown";
import { getGroupingConfiguration } from "../../../../model/selectors/TablePanelView/getGroupingConfiguration";
import { getTableViewProperties } from "model/selectors/TablePanelView/getTableViewProperties";

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

  @computed get imageProperty() {
    for (let prop of this.tableViewProperties) {
      if (prop.column === "Image") {
        return prop;
      }
    }
    return undefined;
  }

  @computed get rowHeight(): number {
    if (this.imageProperty) return this.imageProperty.height;
    return this.defaultRowHeight;
  }


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
    return this.tableViewProperties.length;
  }

  get contentWidth() {
    if (this.columnCount === 0) return 0;
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
    return this.rowHeight;
  }

  getRowBottom(rowIndex: number): number {
    return this.getRowTop(rowIndex) + this.getRowHeight(rowIndex);
  }

  @action.bound setColumnWidth(columnId: string, newWidth: number) {
    this.columnWidths.set(columnId, Math.max(newWidth, 20));
  }

  @computed get displayedColumnDimensionsCom(): { left: number; width: number; right: number }[] {
    const isCheckBoxedTable = getIsSelectionCheckboxesShown(this.ctx);
    const groupedColumnIds = getGroupingConfiguration(this.ctx).orderedGroupingColumnSettings;
    const tableColumnIds = getTableViewProperties(this.ctx).map((prop) => prop.id);
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
