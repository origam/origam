import { IProperties } from "./types/IProperties";
import { IProperty } from "./types/IProperty";
import { IPropertyId } from "../values/types/IPropertyId";
import { computed } from "mobx";

export class Properties implements IProperties {
  constructor(items: IProperty[], reorderingIds?: string[]) {
    this.originalItems = items;
    this.reorderingIds = reorderingIds;
  }

  reorderingIds?: string[];
  originalItems: IProperty[] = [];

  @computed get items() {
    if (!this.reorderingIds) {
      return this.originalItems;
    } else {
      return this.reorderingIds
        .map(id => this.originalItems.find(item => item.id === id))
        .filter(item => item !== undefined) as IProperty[];
    }
  }

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

  getPropertyIdAfterId(id: string): string | undefined {
    const idx = this.id2Index(id);
    const newIdx = idx !== undefined ? idx + 1 : undefined;
    return newIdx !== undefined ? this.index2Id(newIdx) : undefined;
  }

  getPropertyIdBeforeId(id: string): string | undefined {
    const idx = this.id2Index(id);
    const newIdx = idx !== undefined ? idx - 1 : undefined;
    return newIdx !== undefined ? this.index2Id(newIdx) : undefined;
  }
}
