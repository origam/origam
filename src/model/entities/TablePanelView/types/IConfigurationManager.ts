import {ITablePanelView} from "model/entities/TablePanelView/types/ITablePanelView";
import {AggregationType} from "model/entities/types/AggregationType";
import {GroupingUnit} from "model/entities/types/GroupingUnit";

export interface IConfigurationManager {
  onColumnOrderChnaged(): Generator;
  onColumnWidthChanged(id: string, width: number): Generator;
  deleteActiveTableConfiguration(): Promise<any>;
  saveTableConfigurations(): Promise<any>;
  cloneAndActivate(configuration: ITableConfiguration, newName: string): void;
  activeTableConfiguration: ITableConfiguration;
  customTableConfigurations: ITableConfiguration[],
  defaultTableConfiguration: ITableConfiguration,
  allTableConfigurations: ITableConfiguration[]
  parent: any;
}

export interface ITableConfiguration {
  id: string;
  name: string | undefined
  fixedColumnCount: number;
  columnConfigurations: IColumnConfiguration[];
  isActive: boolean;
  sortColumnConfiguartions(propertyIds: string[]): void;
  updateColumnWidth(propertyId: string, width: number): void;
  apply(tablePanelView: ITablePanelView): void;
  deepClone(): ITableConfiguration;
}

export interface IColumnConfiguration {
  propertyId: string;
  isVisible: boolean;
  groupingIndex: number;
  aggregationType: AggregationType | undefined;
  timeGroupingUnit: GroupingUnit | undefined;
  width: number;
  deepClone(): IColumnConfiguration;
}