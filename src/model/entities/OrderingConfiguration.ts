import { action, computed, observable, flow } from "mobx";
import { IDataTable } from "./types/IDataTable";
import {
  IGroupChildrenOrdering,
  IOrderByColumnSetting,
  IOrderByDirection,
  IOrderingConfiguration
} from "./types/IOrderingConfiguration";
import _ from "lodash";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getDataViewPropertyById } from "model/selectors/DataView/getDataViewPropertyById";
import {getProperties} from "../selectors/DataView/getProperties";
import {getFormScreen} from "../selectors/FormScreen/getFormScreen";

interface IDataOrdering {
  column: string;
  direction: IOrderByDirection;
}

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
  @observable ordering: IDataOrdering[] = [];

  getDefaultOrdering(){
    const modelInstanceId = getDataView(this).modelInstanceId;
    return getFormScreen(this).getPanelDefaultOrdering(modelInstanceId);
  }

  @computed get groupChildrenOrdering(): IGroupChildrenOrdering | undefined {
    if(this.ordering.length === 0 || !this.ordering[0]) return undefined;
    const property = getProperties(this)
      .find(prop => prop.id === this.ordering[0].column);
    const lookupId = property &&  property.lookup && property.lookup.lookupId;
    return {
      columnId: this.ordering[0].column,
      direction: this.ordering[0].direction,
      lookupId: lookupId
    };
  }

  getOrdering(column: string): IOrderByColumnSetting {
    const ordIndex = this.ordering.findIndex(item => item.column === column);
    if (ordIndex === -1) {
      return {
        order: 0,
        ordering: IOrderByDirection.NONE
      };
    } else {
      return {
        order: ordIndex,
        ordering: this.ordering[ordIndex].direction
      };
    }
  }

  @action.bound
  setOrdering(column: string): void {
    const orderingClone = _.cloneDeep(this.ordering);
    const curOrd = this.ordering.find(item => item.column === column);
    this.ordering.length = 0;
    if (!curOrd) {
      this.ordering.push({ column, direction: IOrderByDirection.ASC });
    } else {
      this.ordering.push({
        column,
        direction: cycleOrdering(curOrd.direction)
      });
      this.ordering = this.ordering.filter(
        item => item.direction !== IOrderByDirection.NONE
      );
    }

    if (!_.isEqual(orderingClone, this.ordering)) {
      this.maybeApplyOrdering();
    }
  }

  @action.bound
  addOrdering(column: string): void {
    const orderingClone = _.cloneDeep(this.ordering);
    const curOrd = this.ordering.find(item => item.column === column);
    if (!curOrd) {
      this.ordering.push({ column, direction: IOrderByDirection.ASC });
    } else {
      curOrd.direction = cycleOrdering(curOrd.direction);
      /*this.ordering = this.ordering.filter(
        item => item.direction !== IOrderByDirection.NONE
      );*/
    }

    if (!_.isEqual(orderingClone, this.ordering)) {
      this.maybeApplyOrdering();
    }
  }

  maybeApplyOrdering = flow(
    function*(this: OrderingConfiguration) {
      const dataView = getDataView(this);
      const dataTable = getDataTable(dataView);
      if (dataView.isReorderedOnClient) {
        const comboProps = this.ordering
          .map(term => getDataViewPropertyById(this, term.column)!)
          .filter(prop => prop.column === "ComboBox");

        yield Promise.all(
          comboProps.map(prop =>
            prop.lookup!.resolveList(
              new Set(dataTable.getAllValuesOfProp(prop))
            )
          )
        );

        dataTable.setSortingFn(this.orderingFunction);
      }
    }.bind(this)
  );

  get orderingFunction(): (
    dataTable: IDataTable
  ) => (row1: any[], row2: any[]) => number {
    return (dataTable: IDataTable) => (row1: any[], row2: any) => {
      let mul = 10 * this.ordering.length;
      let res = 0;
      for (let term of this.ordering) {
        const prop = dataTable.getPropertyById(term.column)!;
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
              cmpSign =
                dataTable.getCellValue(row1, prop) -
                dataTable.getCellValue(row2, prop);
            }
            break;
          }
        }

        res =
          res +
          mul * (term.direction === IOrderByDirection.DESC ? -1 : 1) * cmpSign;
        mul = mul / 10;
      }
      return res;
    };
  }
}
