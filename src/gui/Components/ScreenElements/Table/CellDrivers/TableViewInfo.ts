import { ITableViewInfo } from "./types";
import { IGroupingConfiguration } from "model/entities/types/IGroupingConfiguration";
import { IProperty } from "model/entities/types/IProperty";

export class TableViewInfo implements ITableViewInfo {
  constructor(
    private groupingConfiguration: IGroupingConfiguration,
    private getIsCheckboxes: () => boolean,
    private getTableViewProperties: () => IProperty[],
    private getTableViewColumnCount: () => number
  ) {}

  get isCheckboxes(): boolean {
    return this.getIsCheckboxes();
  }

  get isGrouping(): boolean {
    return this.groupingConfiguration.isGrouping;
  }

  get groupingColumnsCount(): number {
    return this.groupingConfiguration.groupingColumnCount;
  }

  get dataColumnsCount(): number {
    return this.getTableViewColumnCount();
  }

  getGroupedColumnIdByLevel(level: number): string {
    return this.groupingConfiguration.orderedGroupingColumnIds[level - 1];
  }

  getDataColumnIdByIndex(index: number): string {
    return this.getTableViewProperties()[index].id;
  }
}
