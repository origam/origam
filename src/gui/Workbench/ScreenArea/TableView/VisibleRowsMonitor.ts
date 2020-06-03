import {IGridDimensions} from "../../../Components/ScreenElements/Table/types";
import {SimpleScrollState} from "../../../Components/ScreenElements/Table/SimpleScrollState";
import {computed} from "mobx";
import {getDataView} from "../../../../model/selectors/DataView/getDataView";
import {rangeQuery} from "../../../../utils/arrays";
import {IDataTable} from "../../../../model/entities/types/IDataTable";
import {getDataTable} from "../../../../model/selectors/DataView/getDataTable";


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
      this.scrollState.scrollTop + (dataView.contentBounds && dataView.contentBounds.height || 0)
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

export class OpenGroupVisibleRowsMonitor implements IVisibleRowsMonitor{

  ctx: any;
  gridDimensions: IGridDimensions;
  scrollState: SimpleScrollState;
  visibleRowsAll: VisibleRowsMonitor;
  private dataTable: IDataTable;

  constructor(ctx: any, gridDimensions: IGridDimensions, scrollState: SimpleScrollState){
    this.ctx = ctx;
    this.gridDimensions = gridDimensions;
    this.scrollState = scrollState;
    this.visibleRowsAll = new VisibleRowsMonitor(ctx, gridDimensions, scrollState);
    this.dataTable = getDataTable(ctx);
  }

  @computed
  get firstIndex() {
    const expandedGroupIndex = this.dataTable.groups
      .findIndex(group => group.isExpanded);
    return expandedGroupIndex > this.visibleRowsAll.firstIndex
      ? 0
      : this.visibleRowsAll.firstIndex - expandedGroupIndex;
  }

  @computed
  get lastIndex() {
    const expandedGroupIndex = this.dataTable.groups
      .findIndex(group => group.isExpanded);
    return this.visibleRowsAll.lastIndex - expandedGroupIndex;
  }
}
