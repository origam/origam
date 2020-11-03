import { action } from "mobx";
import { MapRootStore } from "./MapRootStore";

export class MapNavigationStore {
  constructor(private root: MapRootStore) {}

  refMapComponent = (elm: any) => (this.elmMapComponent = elm);
  elmMapComponent: {
    panToCenter(): void;
    panToSelectedObject(): void;
    panToFirstObject(): void;
  } | null = null;

  get setupStore() {
    return this.root.mapSetupStore;
  }

  get searchStore() {
    return this.root.mapSearchStore;
  }

  @action.bound
  handleCenterMapClick(event: any) {
    this.elmMapComponent?.panToCenter();
  }

  @action.bound
  handleLookupObjectClick(event: any) {
    if (this.setupStore.isReadOnlyView) {
      if (this.searchStore.selectedSearchResult) {
        this.elmMapComponent?.panToSelectedObject();
      }
    } else {
      this.elmMapComponent?.panToFirstObject();
    }
  }
}
