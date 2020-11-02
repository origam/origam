import { action, observable } from "mobx";
import { Observer } from "mobx-react";
import React, { useState } from "react";
import { createPortal } from "react-dom";
import { AutoSizer, Grid } from "react-virtualized";
import cx from "classnames";
import S from "./MapPerspectiveUI.module.scss";

class MapSearchState {
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

export function MapPerspectiveSearch(props: {
  value: any;
  onSearchPhraseChange?(value: string): void;
  on
}) {
  const [state] = useState(() => new MapSearchState());
  return (
    <Observer>
      {() => (
        <div className={S.mapSearch} ref={state.refSearchField}>
          <input
            className={"input"}
            value={state.searchInputValue}
            onChange={state.handleSearchInputChange}
            onKeyDown={state.handleSearchInputKeyDown}
          />
          <button className={"inputCaret"} onMouseDown={state.handleCaretMouseDown}>
            <i className="fas fa-caret-down" />
          </button>
          {state.isDropped &&
            createPortal(
              <MapPerspectiveSearchDropdown
                domRef={state.refDropdown}
                top={state.dropdownTop + 2}
                left={state.dropdownLeft}
                width={state.dropdownWidth}
              />,
              document.getElementById("dropdown-portal")!
            )}
        </div>
      )}
    </Observer>
  );
}

export function MapPerspectiveSearchDropdown(props: {
  top: number;
  left: number;
  width: number;
  domRef: any;
}) {
  return (
    <div
      ref={props.domRef}
      style={{ top: props.top, left: props.left, width: props.width }}
      className={S.mapSearchDropdown}
    >
      <SearchResults width={props.width - 4} />
    </div>
  );
}

const rowCount = 50;
const ROW_HEIGHT = 25;

export function SearchResults(props: { width: number }) {
  return (
    <Grid
      width={props.width}
      height={Math.min(rowCount * ROW_HEIGHT, 10 * ROW_HEIGHT)}
      rowHeight={ROW_HEIGHT}
      columnWidth={props.width}
      rowCount={rowCount}
      columnCount={1}
      cellRenderer={({ key, style, rowIndex, columnIndex }) => {
        return (
          <div
            className={cx("cell", { c1: rowIndex % 2 === 0, c2: rowIndex % 2 === 1 })}
            key={key}
            style={style}
          >
            {rowIndex}-{columnIndex}
          </div>
        );
      }}
    />
  );
}
