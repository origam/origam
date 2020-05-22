import { observable } from "mobx";

let currentIdent = 1;

export function getIdent() {
  return currentIdent++;
}

export interface IIId {
  $iid: number;
}

export class ContribArray<T> {
  @observable items = new Map<number, T>();

  put(item: T & IIId) {
    this.items.set(item.$iid, item);
    return () => this.items.delete(item.$iid);
  }

  del(item: IIId) {
    this.items.delete(item.$iid);
  }

  [Symbol.iterator]() {
    return this.items.values()[Symbol.iterator]();
  }

  asArray() {
    return Array.from(this);
  }
}
