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
import { SimpleScrollState } from "../../../Components/ScreenElements/Table/SimpleScrollState";
import { computed } from "mobx";
import { getDataView } from "../../../../model/selectors/DataView/getDataView";
import { rangeQuery } from "../../../../utils/arrays";
import { IDataTable } from "../../../../model/entities/types/IDataTable";
import { getDataTable } from "../../../../model/selectors/DataView/getDataTable";
import { getGrouper } from "model/selectors/DataView/getGrouper";


export interface IVisibleRowsMonitor {
  firstIndex: number;
  lastIndex: number;
}

export class VisibleRowsMonitor implements IVisibleRowsMonitor {

  ctx: any;
  gridDimensions: IGridDimensions;
  scrollState: SimpleScrollState;

  constructor(ctx: any, gridDimensions: IGridDimensions, scrollState: SimpleScrollState) {
    this.ctx = ctx;
    this.gridDimensions = gridDimensions;
    this.scrollState = scrollState;
  }

  @computed
  get visibleRowsRange() {
    const dataView = getDataView(this.ctx);
    return rangeQuery(
      (i) => this.gridDimensions.getRowBottom(i),
      (i) => this.gridDimensions.getRowTop(i),
      this.gridDimensions.rowCount,
      this.scrollState.scrollTop,
      this.scrollState.scrollTop + (dataView.contentBounds?.height ?? 0)
    );
  }

  @computed
  get firstIndex() {
    return this.visibleRowsRange.firstGreaterThanNumber;
  }

  @computed
  get lastIndex() {
    return this.visibleRowsRange.lastLessThanNumber;
  }
}

export class OpenGroupVisibleRowsMonitor implements IVisibleRowsMonitor {

  ctx: any;
  gridDimensions: IGridDimensions;
  scrollState: SimpleScrollState;
  visibleRowsAll: VisibleRowsMonitor;
  private dataTable: IDataTable;

  constructor(ctx: any, gridDimensions: IGridDimensions, scrollState: SimpleScrollState) {
    this.ctx = ctx;
    this.gridDimensions = gridDimensions;
    this.scrollState = scrollState;
    this.visibleRowsAll = new VisibleRowsMonitor(ctx, gridDimensions, scrollState);
    this.dataTable = getDataTable(ctx);
  }

  @computed
  get firstIndex() {
    const expandedGroupIndex = getGrouper(this.ctx).topLevelGroups
      .findIndex(group => group.isExpanded);
    return expandedGroupIndex > this.visibleRowsAll.firstIndex
      ? 0
      : this.visibleRowsAll.firstIndex - expandedGroupIndex;
  }

  @computed
  get lastIndex() {
    const expandedGroupIndex = getGrouper(this.ctx).topLevelGroups
      .findIndex(group => group.isExpanded);
    return this.visibleRowsAll.lastIndex - expandedGroupIndex;
  }
}
