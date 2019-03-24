import { IProperties } from "./types/IProperties";
import { IProperty } from "./types/IProperty";
import { IPropertyId } from "../values/types/IPropertyId";

export class Properties implements IProperties {
  
  constructor(items: IProperty[]) {
    this.items = items;
  }

  items: IProperty[] = [];

  get count(): number {
    return this.items.length;
  }

  byId(id: string): IProperty | undefined {
    return this.items.find(item => item.id === id);
  }

  byIndex(idx: number): IProperty | undefined {
    return this.items[idx];
  }

  getIndex(property: IProperty): number | undefined {
    const index = this.items.findIndex(item => item.id === property.id);
    return index > -1 ? index : undefined;
  }

  index2Id(idx: number): string | undefined {
    const property = this.byIndex(idx);
    return property ? property.id : undefined;
  }

  id2Index(id: IPropertyId): number | undefined {
    const property = this.byId(id);
    return property ? this.getIndex(property) : undefined;
  }

  getPropertyIdAfterId(id: string): string {
    throw new Error("Method not implemented.");
  }

  getPropertyIdBeforeId(id: string): string {
    throw new Error("Method not implemented.");
  }
}
