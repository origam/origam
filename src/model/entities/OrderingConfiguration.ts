import { action, computed, flow, observable } from "mobx";
import {
  IOrderByColumnSetting,
  IOrderByDirection,
  IOrdering,
  IOrderingConfiguration,
} from "./types/IOrderingConfiguration";
import _ from "lodash";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getDataViewPropertyById } from "model/selectors/DataView/getDataViewPropertyById";
import { getProperties } from "../selectors/DataView/getProperties";

function cycleOrdering(direction: IOrderByDirection) {
  switch (direction) {
    case IOrderByDirection.ASC:
      return IOrderByDirection.DESC;
    case IOrderByDirection.DESC:
      return IOrderByDirection.NONE;
    case IOrderByDirection.NONE:
    default:
      return IOrderByDirection.ASC;
  }
}

export class OrderingConfiguration implements IOrderingConfiguration {
  @observable userOrderings: IOrdering[] = [];
  private defaultOrderings: IOrdering[];

  constructor(defaultOrderings: IOrdering[] | undefined) {
    this.defaultOrderings = defaultOrderings ?? [];
  }

  getDefaultOrderings() {
    return this.defaultOrderings;
  }

  @computed get orderings(){
    return this.userOrderings.length !== 0 ? this.userOrderings : this.defaultOrderings;
  }

  @computed get groupChildrenOrdering(): IOrdering | undefined {
    return this.userOrderings.length === 0 || !this.userOrderings[0] ? undefined : this.userOrderings[0];
  }

  getOrdering(column: string): IOrderByColumnSetting {
    const ordIndex = this.orderings.findIndex((item) => item.columnId === column);
    if (ordIndex === -1) {
      return {
        order: 0,
        ordering: IOrderByDirection.NONE,
      };
    } else {
      return {
        order: ordIndex,
        ordering: this.orderings[ordIndex].direction,
      };
    }
  }

  private newOrdering(columnId: string, direction: IOrderByDirection): IOrdering {
    const property = getProperties(this).find((prop) => prop.id === columnId);
    const lookupId = property && property.lookup && property.lookup.lookupId;
    return {
      columnId: columnId,
      direction: direction,
      lookupId: lookupId,
    };
  }

  @action.bound
  setOrdering(columnId: string): void {
    const orderingClone = _.cloneDeep(this.userOrderings);
    const curOrd = this.userOrderings.find((item) => item.columnId === columnId);
    this.userOrderings.length = 0;
    if (!curOrd) {
      this.userOrderings.push(this.newOrdering(columnId, IOrderByDirection.ASC));
    } else {
      this.userOrderings.push(this.newOrdering(columnId, cycleOrdering(curOrd.direction)));
      this.userOrderings = this.userOrderings.filter((item) => item.direction !== IOrderByDirection.NONE);
    }

    if (!_.isEqual(orderingClone, this.userOrderings)) {
      this.maybeApplyOrdering();
    }
  }

  @action.bound
  addOrdering(columnId: string): void {
    const orderingClone = _.cloneDeep(this.userOrderings);
    const curOrd = this.userOrderings.find((item) => item.columnId === columnId);
    if (!curOrd) {
      this.userOrderings.push(this.newOrdering(columnId, IOrderByDirection.ASC));
    } else {
      curOrd.direction = cycleOrdering(curOrd.direction);
      /*this.ordering = this.ordering.filter(
        item => item.direction !== IOrderByDirection.NONE
      );*/
    }

    if (!_.isEqual(orderingClone, this.userOrderings)) {
      this.maybeApplyOrdering();
    }
  }

  maybeApplyOrdering = flow(
    function* (this: OrderingConfiguration) {
      const dataView = getDataView(this);
      const dataTable = getDataTable(dataView);
      if (dataView.isReorderedOnClient) {
        const comboProps = this.userOrderings
          .map((term) => getDataViewPropertyById(this, term.columnId)!)
          .filter((prop) => prop.column === "ComboBox");

        yield Promise.all(
          comboProps.map(async (prop) => {
            return prop.lookupEngine?.lookupResolver.resolveList(
              new Set(dataTable.getAllValuesOfProp(prop))
            );
          })
        );
      }
    }.bind(this)
  );

  @computed get orderingFunction(): () => (row1: any[], row2: any[]) => number {
    return () => (row1: any[], row2: any) => {
      const dataTable = getDataTable(this);
      let mul = 10 * this.orderings.length;
      let res = 0;
      for (let term of this.orderings) {
        const prop = dataTable.getPropertyById(term.columnId)!;
        let cmpSign = 0;
        switch (prop.column) {
          case "Text":
          case "Date":
          case "ComboBox":
            const txt1 = dataTable.getCellText(row1, prop);
            const txt2 = dataTable.getCellText(row2, prop);
            if (txt1 === undefined) {
              cmpSign = 1;
            } else if (txt2 === undefined) {
              cmpSign = -1;
            } else if (txt1 === null) {
              cmpSign = 1;
            } else if (txt2 === null) {
              cmpSign = -1;
            } else {
              cmpSign = txt1.localeCompare(txt2);
            }
            break;
          case "CheckBox": {
            const val1 = dataTable.getCellValue(row1, prop);
            const val2 = dataTable.getCellValue(row2, prop);
            if (val1 === null) {
              cmpSign = 1;
            } else if (val2 === null) {
              cmpSign = -1;
            } else {
              cmpSign = `${val1}`.localeCompare(`${val2}`);
            }
            break;
          }
          case "Number": {
            const val1 = dataTable.getCellValue(row1, prop);
            const val2 = dataTable.getCellValue(row2, prop);
            if (val1 === null) {
              cmpSign = 1;
            } else if (val2 === null) {
              cmpSign = -1;
            } else {
              cmpSign = dataTable.getCellValue(row1, prop) - dataTable.getCellValue(row2, prop);
            }
            break;
          }
        }

        res = res + mul * (term.direction === IOrderByDirection.DESC ? -1 : 1) * cmpSign;
        mul = mul / 10;
      }
      return res;
    };
  }
}
