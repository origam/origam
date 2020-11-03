import { action, computed, observable } from "mobx";
import { IMapObject } from "./MapObjectsStore";
import { MapRootStore } from "./MapRootStore";

export class SearchStore {
  constructor(private root: MapRootStore) {}

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

  @observable rect: any = { top: 0, left: 0, right: 0, bottom: 0, height: 0, width: 0 };

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

  @action.bound
  handleSearchInputKeyDown(event: any) {
    switch (event.key) {
      case "Escape":
        if (this.isDropped) {
          this.dropUp();
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
    if (this.isDropped) {
      this.dropUp();
    } else {
      this.dropDown();
    }
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
