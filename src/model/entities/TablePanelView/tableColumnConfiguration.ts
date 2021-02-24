import {ITableColumnConf} from "model/entities/TablePanelView/types/IConfigurationManager";
import {observable} from "mobx";
import {AggregationType} from "model/entities/types/AggregationType";
import {GroupingUnit} from "model/entities/types/GroupingUnit";

export class TableColumnConfiguration implements ITableColumnConf {

  constructor(public id: string) {
  }

  @observable
  aggregationType: AggregationType | undefined;
  canAggregate: boolean = false;
  canGroup: boolean = true;
  entity: string = "";
  @observable
  groupingIndex: number = 0;
  @observable
  isVisible: boolean = true;
  name: string = "";
  @observable
  timeGroupingUnit: GroupingUnit | undefined;
  width = 0;
}