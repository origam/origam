import {ITablePanelView} from "model/entities/TablePanelView/types/ITablePanelView";
import {AggregationType} from "model/entities/types/AggregationType";
import {GroupingUnit} from "model/entities/types/GroupingUnit";

export interface IConfigurationManager {
  setAsCurrent(newConfig: any): void;
  customTableConfigurations: ITableConfiguration[],
  defaultTableConfiguration: ITableConfiguration,
  allTableConfigurations: ITableConfiguration[]
}

export interface ITableConfiguration {
  name: string | undefined
  fixedColumnCount: number;
  columnConfiguration: IColumnConfiguration[];
  sortedColumnConfigurations: IColumnConfiguration[];
  isActive: boolean;
  tablePropertyIds: string[];
  apply(tablePanelView: ITablePanelView): void;
  cloneAs(name: string): ITableConfiguration;
}

export interface IColumnConfiguration {
  propertyId: string;
  name: string;
  isVisible: boolean;
  groupingIndex: number;
  aggregationType: AggregationType | undefined;
  timeGroupingUnit: GroupingUnit | undefined;
  entity: string;
  canGroup: boolean;
  canAggregate: boolean;
  width: number;
  clone(): IColumnConfiguration;
}