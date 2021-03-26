import {IColumnConfiguration} from "model/entities/TablePanelView/types/IConfigurationManager";
import {observable} from "mobx";
import {AggregationType} from "model/entities/types/AggregationType";
import {GroupingUnit} from "model/entities/types/GroupingUnit";

export class TableColumnConfiguration implements IColumnConfiguration {

  constructor(
    public propertyId: string
  ) {
  }

  @observable
  aggregationType: AggregationType | undefined;
  @observable
  groupingIndex: number = 0;
  @observable
  isVisible: boolean = true;
  @observable
  timeGroupingUnit: GroupingUnit | undefined;
  width = 0;

  deepClone(): IColumnConfiguration {
    return Object.assign(Object.create(Object.getPrototypeOf(this)), this)
  }
}