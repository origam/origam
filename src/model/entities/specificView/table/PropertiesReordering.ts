import { IProperties } from "../../data/types/IProperties";
import { observable, computed } from "mobx";
import { IProperty } from "../../data/types/IProperty";

export class PropertiesReordering implements IProperties {
  constructor(public origin: IProperties, orderedIds: string[]) {
    this.orderedIds = orderedIds;
  }

  @observable orderedIds: string[] = [];

  @computed get ordProps(): IProperty[] {
    return this.orderedIds
      .map(id => this.origin.byId(id))
      .filter(prop => prop !== undefined) as IProperty[];
  }

  get count(): number {
    return this.ordProps.length;
  }

  byId(id: string): IProperty | undefined {
    return this.ordProps.find(prop => prop.id === id);
  }

  byIndex(idx: number): IProperty | undefined {
    return this.ordProps[idx];
  }

  getIndex(property: IProperty): number | undefined {
    return this.ordProps.findIndex(prop => prop.id === property.id);
  }

  index2Id(idx: number): string | undefined {
    const prop = this.byIndex(idx);
    return prop && prop.id;
  }

  id2Index(id: string): number | undefined {
    const prop = this.byId(id);
    return prop && this.getIndex(prop);
  }

  getPropertyIdAfterId(id: string): string | undefined {
    const idx = this.id2Index(id);
    const resProp = idx !== undefined ? this.byIndex(idx + 1) : undefined;
    return resProp && resProp.id;
  }

  getPropertyIdBeforeId(id: string): string | undefined {
    const idx = this.id2Index(id);
    const resProp = idx !== undefined ? this.byIndex(idx - 1) : undefined;
    return resProp && resProp.id;
  }
}
