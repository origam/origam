import { action } from "mobx";
import { MapRootStore } from "./MapRootStore";

export class MapNavigationStore {
  constructor(private root: MapRootStore) {}

  refMapComponent = (elm: any) => (this.elmMapComponent = elm);
  elmMapComponent: {
    panToCenter(): void;
    panToSelectedObject(): void;
    panToFirstObject(): void;
    highlightSelectedLayer(): void;
    activateRoutingControls(): void;
    activateNormalControls(): void;
  } | null = null;

  get setupStore() {
    return this.root.mapSetupStore;
  }

  get searchStore() {
    return this.root.mapSearchStore;
  }

  @action.bound
  highlightSelectedSearchResult() {
    // We might select some result right before this point in the same action.
    // Wait for React to propagate props.
    setTimeout(() => this.elmMapComponent?.highlightSelectedLayer(), 100);
  }

  @action.bound
  fitToSelectedSearchResult() {
    // We might select some result right before this point in the same action.
    // Wait for React to propagate props.
    setTimeout(() => this.elmMapComponent?.panToSelectedObject(), 100);
  }

  @action.bound
  handleCenterMapClick(event: any) {
    this.elmMapComponent?.panToCenter();
  }

  @action.bound
  activateRoutingControls() {
    this.elmMapComponent?.activateRoutingControls();
  }

  @action.bound
  activateNormalControls() {
    this.elmMapComponent?.activateNormalControls();
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
