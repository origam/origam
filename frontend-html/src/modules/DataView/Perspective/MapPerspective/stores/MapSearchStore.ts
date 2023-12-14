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

import { action, computed, observable } from "mobx";
import { IMapObject } from "./MapObjectsStore";
import { MapRootStore } from "./MapRootStore";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { runGeneratorInFlowWithHandler } from "utils/runInFlowWithHandler";

export class SearchStore {
  constructor(private root: MapRootStore) {
  }

  get dataView() {
    return this.root.dataView;
  }

  get navigationStore() {
    return this.root.mapNavigationStore;
  }

  get allMapObjects() {
    return this.root.mapObjectsStore.mapObjects;
  }

  @computed get searchResults(): IMapObject[] {
    if (!this.searchPhrase) return this.allMapObjects;
    return this.allMapObjects.filter((obj) =>
      obj.name.toLocaleLowerCase().includes(this.searchPhrase.toLocaleLowerCase())
    );
  }

  @observable selectedSearchResult?: IMapObject;

  @observable isDropped = false;

  refSearchField = (elm: any) => (this.elmSearchField = elm);
  elmSearchField: any = null;

  refDropdown = (elm: any) => (this.elmDropdown = elm);
  elmDropdown: any = null;

  @observable searchPhrase = "";

  @observable rect: any = {top: 0, left: 0, right: 0, bottom: 0, height: 0, width: 0};

  @action.bound
  measureSearchField() {
    if (this.elmSearchField) {
      this.rect = this.elmSearchField.getBoundingClientRect();
    }
  }

  @action.bound
  dropDown() {
    this.measureSearchField();
    this.isDropped = true;
    window.addEventListener("mousedown", this.handleWindowMouseDown);
  }

  @action.bound
  dropUp() {
    this.isDropped = false;
    window.removeEventListener("mousedown", this.handleWindowMouseDown);
  }

  @action.bound
  handleSearchInputChange(event: any) {
    this.searchPhrase = event.target.value;
    if (!this.isDropped) {
      this.dropDown();
    }
  }

  @action.bound handleSearchInputFocus(event: any) {
    event.target.select?.();
  }

  @action.bound handleSearchInputBlur(event: any) {
    if (!this.searchPhrase) {
      this.selectedSearchResult = undefined;
      this.navigationStore.fitToSelectedSearchResult();
      this.navigationStore.highlightSelectedSearchResult();
    }
  }

  @action.bound
  handleSearchInputKeyDown(event: any) {
    switch (event.key) {
      case "Escape":
        if (this.isDropped) {
          this.dropUp();
        }
        break;
      case "Enter":
        if (!this.searchPhrase) {
          this.selectedSearchResult = undefined;
          this.navigationStore.fitToSelectedSearchResult();
          this.navigationStore.highlightSelectedSearchResult();
        }
        break;
    }
  }

  get searchInputValue() {
    return this.searchPhrase;
  }

  @action.bound
  handleCaretMouseDown(event: any) {
    event.stopPropagation();
    event.preventDefault();
    if (this.isDropped) {
      this.dropUp();
    } else {
      this.searchPhrase = "";
      this.dropDown();
    }
  }

  @action.bound
  handleClearClick(event: any) {
    this.searchPhrase = "";
    this.selectedSearchResult = undefined;
    this.navigationStore.fitToSelectedSearchResult();
    this.navigationStore.highlightSelectedSearchResult();
  }

  @action.bound
  handleClearMouseDown(event: any) {
  }

  @action.bound
  selectSearchResultById(resultId: string) {
    this.selectedSearchResult = this.allMapObjects.find((item) => item.id === resultId);
    this.searchPhrase = this.selectedSearchResult?.name || "";
  }

  @action.bound
  handleSearchResultClick(event: any, resultId: string) {
    const self = this;
    runGeneratorInFlowWithHandler({
      ctx: this.dataView,
      generator: function* (){
        yield*getDataView(self.dataView).setSelectedRowId(resultId);
        getTablePanelView(self.dataView).scrollToCurrentRow();
        self.selectSearchResultById(resultId);
        self.navigationStore.fitToSelectedSearchResult();
        self.navigationStore.highlightSelectedSearchResult();
        self.dropUp();
      }()
    })
  }

  @action.bound
  handleWindowMouseDown(event: any) {
    if (!this.elmDropdown?.contains(event.target) && !this.elmSearchField?.contains(event.target)) {
      this.dropUp();
    }
  }

  get dropdownTop() {
    return this.rect.bottom;
  }

  get dropdownLeft() {
    return this.rect.left;
  }

  get dropdownWidth() {
    return this.rect.width;
  }
}
