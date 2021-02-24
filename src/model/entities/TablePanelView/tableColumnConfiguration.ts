import {IColumnConfiguration} from "model/entities/TablePanelView/types/IConfigurationManager";
import {observable} from "mobx";
import {AggregationType} from "model/entities/types/AggregationType";
import {GroupingUnit} from "model/entities/types/GroupingUnit";
import {getGroupingConfiguration} from "model/selectors/TablePanelView/getGroupingConfiguration";
import {isLazyLoading} from "model/selectors/isLazyLoading";

export class TableColumnConfiguration implements IColumnConfiguration {

  constructor(
    public propertyId: string,
    public name: string
  ) {
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
  @observable
  timeGroupingUnit: GroupingUnit | undefined;
  width = 0;

  clone(): IColumnConfiguration {
    return Object.assign(Object.create(Object.getPrototypeOf(this)), this)
  }
}