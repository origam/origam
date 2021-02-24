import {ITablePanelView} from "model/entities/TablePanelView/types/ITablePanelView";
import {AggregationType} from "model/entities/types/AggregationType";
import {GroupingUnit} from "model/entities/types/GroupingUnit";

export interface IConfigurationManager {
  activeTableConfiguration: ITableConfiguration;
  customTableConfigurations: ITableConfiguration[],
  defaultTableConfiguration: ITableConfiguration,
  allTableConfigurations: ITableConfiguration[]
}

export interface ITableConfiguration {
  name: string | undefined
  fixedColumnCount: number;
  columnConfigurations: IColumnConfiguration[];
  isActive: boolean;
  sortColumnConfiguartions(propertyIds: string[]): void;
  apply(tablePanelView: ITablePanelView): void;
  cloneAs(name: string): ITableConfiguration;
}

export interface IColumnConfiguration {
  propertyId: string;
  isVisible: boolean;
  groupingIndex: number;
  aggregationType: AggregationType | undefined;
  timeGroupingUnit: GroupingUnit | undefined;
  width: number;
  clone(): IColumnConfiguration;
}