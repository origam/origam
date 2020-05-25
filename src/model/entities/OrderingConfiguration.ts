import { action, computed, observable, flow } from "mobx";
import { IDataTable } from "./types/IDataTable";
import {
  IOrderByColumnSetting,
  IOrderByDirection, IOrdering,
  IOrderingConfiguration
} from "./types/IOrderingConfiguration";
import _ from "lodash";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getDataViewPropertyById } from "model/selectors/DataView/getDataViewPropertyById";
import {getProperties} from "../selectors/DataView/getProperties";
import {getFormScreen} from "../selectors/FormScreen/getFormScreen";


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
  @observable ordering: IOrdering[] = [];

  getDefaultOrdering(){
    const modelInstanceId = getDataView(this).modelInstanceId;
    return getFormScreen(this).getPanelDefaultOrdering(modelInstanceId);
  }

  @computed get groupChildrenOrdering(): IOrdering | undefined {
    return this.ordering.length === 0 || !this.ordering[0]
      ? undefined
      : this.ordering[0];
  }

  getOrdering(column: string): IOrderByColumnSetting {
    const ordIndex = this.ordering.findIndex(item => item.columnId === column);
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

  private newOrdering(columnId: string, direction: IOrderByDirection): IOrdering{
    const property = getProperties(this)
      .find(prop => prop.id === columnId);
    const lookupId = property &&  property.lookup && property.lookup.lookupId;
    return {
      columnId: columnId,
      direction: direction,
      lookupId: lookupId
    };
  }

  @action.bound
  setOrdering(columnId: string): void {
    const orderingClone = _.cloneDeep(this.ordering);
    const curOrd = this.ordering.find(item => item.columnId === columnId);
    this.ordering.length = 0;
    if (!curOrd) {
      this.ordering.push(this.newOrdering(columnId, IOrderByDirection.ASC));
    } else {
      this.ordering.push(this.newOrdering(columnId, cycleOrdering(curOrd.direction)
      ));
      this.ordering = this.ordering.filter(
        item => item.direction !== IOrderByDirection.NONE
      );
    }

    if (!_.isEqual(orderingClone, this.ordering)) {
      this.maybeApplyOrdering();
    }
  }

  @action.bound
  addOrdering(columnId: string): void {
    const orderingClone = _.cloneDeep(this.ordering);
    const curOrd = this.ordering.find(item => item.columnId === columnId);
    if (!curOrd) {
      this.ordering.push(this.newOrdering(columnId, IOrderByDirection.ASC));
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
          .map(term => getDataViewPropertyById(this, term.columnId)!)
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
