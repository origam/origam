import { action, observable, computed } from "mobx";
import { L, ML } from "../utils/types";
import { IProperty } from "./types/IProperty";
import { IPropReorder } from "./types/IPropReorder";
import { IProperties } from "./types/IProperties";
import { unpack } from "../utils/objects";

export class PropReorder implements IPropReorder {
  constructor(
    public P: { properties: ML<IProperties>; initPropIds: string[] | undefined }
  ) {
    const initPropIds = P.initPropIds;
    if (initPropIds) {
      this.propIds = initPropIds;
    }
  }

  get count(): number {
    return this.reorderedItems.length;
  }

  @observable propIds: string[] = [];

  @action.bound setIds(ids: string[]) {
    this.propIds = ids;
  }

  @computed get reorderedItems() {
    return this.propIds
      .map(id => this.props.getById(id))
      .filter(item => item !== undefined) as Array<IProperty>;
  }

  @computed get originalItems() {
    return this.props.items;
  }

  getNthIdFrom(id: string | undefined, n: number) {
    const { reorderedItems } = this;
    let idx0 = id ? reorderedItems.findIndex(item => item.id === id) : -1;
    let newItem = idx0 > -1 ? reorderedItems[idx0 + n] : undefined;
    return newItem ? newItem.id : undefined;
  }

  getIdAfterId(id: string | undefined): string | undefined {
    return this.getNthIdFrom(id, 1);
  }

  getIdBeforeId(id: string | undefined): string | undefined {
    return this.getNthIdFrom(id, -1);
  }

  getById(id: string | undefined): IProperty | undefined {
    return id ? this.reorderedItems.find(item => item.id === id) : undefined;
  }

  getByIndex(idx: number | undefined): IProperty | undefined {
    return idx !== undefined ? this.reorderedItems[idx] : undefined;
  }

  getIdByIndex(idx: number | undefined): string | undefined {
    const prop = this.getByIndex(idx);
    return prop ? prop.id : undefined;
  }

  getIndexById(id: string): number | undefined {
    const idx = id ? this.reorderedItems.findIndex(item => item.id === id) : -1;
    return idx > -1 ? idx : undefined;
  }

  /* WIRING */
  get props() {
    return unpack(this.P.properties);
  }
}
