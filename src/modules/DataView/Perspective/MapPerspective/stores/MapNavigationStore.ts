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
