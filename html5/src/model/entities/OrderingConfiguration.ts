import {
  IOrderingConfiguration,
  IOrderByDirection,
  IOrderByColumnSetting
} from "./types/IOrderingConfiguration";
import { action, observable } from "mobx";

class DataOrdering {
  constructor(public column: string, public direction: IOrderByDirection) {}
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
  @observable ordering: DataOrdering[] = [];

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
    const curOrd = this.ordering.find(item => item.column === column);
    this.ordering.length = 0;
    if (!curOrd) {
      this.ordering.push(new DataOrdering(column, IOrderByDirection.ASC));
    } else {
      this.ordering.push(
        new DataOrdering(column, cycleOrdering(curOrd.direction))
      );
    }
  }

  @action.bound
  addOrdering(column: string): void {
    const curOrd = this.ordering.find(item => item.column === column);
    if (!curOrd) {
      this.ordering.push(new DataOrdering(column, IOrderByDirection.ASC));
    } else {
      this.ordering.push(
        new DataOrdering(column, cycleOrdering(curOrd.direction))
      );
    }
  }
}
