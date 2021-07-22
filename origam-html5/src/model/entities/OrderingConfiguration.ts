/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import {action, computed, flow, observable} from "mobx";
import {
  IOrderByColumnSetting,
  IOrderByDirection,
  IOrdering,
  IOrderingConfiguration,
} from "./types/IOrderingConfiguration";
import _ from "lodash";
import {getDataView} from "model/selectors/DataView/getDataView";
import {getDataTable} from "model/selectors/DataView/getDataTable";
import {getDataViewPropertyById} from "model/selectors/DataView/getDataViewPropertyById";
import {getProperties} from "../selectors/DataView/getProperties";
import {compareStrings} from "../../utils/string";

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

function flipOrdering(direction: IOrderByDirection) {
  return direction === IOrderByDirection.DESC
    ? IOrderByDirection.ASC
    : IOrderByDirection.DESC;
}

export class OrderingConfiguration implements IOrderingConfiguration {
  @observable userOrderings: IOrdering[] = [];
  private defaultOrderings: IOrdering[];

  constructor(defaultOrderings: IOrdering[] | undefined) {
    this.defaultOrderings = defaultOrderings ?? [];
    this.userOrderings = this.defaultOrderings;
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
      this.userOrderings.push(this.newOrdering(columnId, flipOrdering(curOrd.direction)));
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
    }

    if (!_.isEqual(orderingClone, this.userOrderings)) {
      this.maybeApplyOrdering();
    }
  }

  maybeApplyOrdering = flow(
    function* (this: OrderingConfiguration) {
      const dataView = getDataView(this);
      const dataTable = getDataTable(dataView);
      if (!dataView.isLazyLoading) {
        const comboProps = this.userOrderings
          .map((term) => getDataViewPropertyById(this, term.columnId)!)
          .filter((prop) => prop.column === "ComboBox");

        yield Promise.all(
          comboProps.map(async (prop) => {
            return prop.lookupEngine?.lookupResolver.resolveList(
              dataTable.getAllValuesOfProp(prop)
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
            const txt1 = dataTable.getOriginalCellText(row1, prop);
            const txt2 = dataTable.getOriginalCellText(row2, prop);
            cmpSign = compareStrings(txt1, txt2);
            break;
          case "CheckBox": {
            const val1 =  dataTable.getOriginalCellValue(row1, prop);
            const val2 = dataTable.getOriginalCellValue(row2, prop);
            cmpSign = compareStrings(`${val1}`, `${val2}`);
            break;
          }
          case "Number": {
            const val1 = dataTable.getOriginalCellValue(row1, prop);
            const val2 = dataTable.getOriginalCellValue(row2, prop);
            if (val1 === null) {
              cmpSign = 1;
            } else if (val2 === null) {
              cmpSign = -1;
            } else {
              cmpSign = Math.sign(val1 - val2);
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
