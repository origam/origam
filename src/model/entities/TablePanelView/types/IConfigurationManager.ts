import {ITablePanelView} from "model/entities/TablePanelView/types/ITablePanelView";
import {AggregationType} from "model/entities/types/AggregationType";
import {GroupingUnit} from "model/entities/types/GroupingUnit";

export interface IConfigurationManager {
  tableConfigurations: ITableColumnsConf[],
  defaultTableConfiguration: ITableColumnsConf
}

export interface ITableColumnsConf {
  name: string | undefined
  fixedColumnCount: number;
  columnConf: ITableColumnConf[];
  tablePropertyIds: string[];
  apply(tablePanelView: ITablePanelView): void;
}

export interface ITableColumnConf {
  id: string;
  name: string;
  isVisible: boolean;
  groupingIndex: number;
  aggregationType: AggregationType | undefined;
  timeGroupingUnit: GroupingUnit | undefined;
  entity: string;
  canGroup: boolean;
  canAggregate: boolean;
  width: number;
}