import { action, observable } from "mobx";
import { Observer } from "mobx-react";
import React, { useContext, useState } from "react";
import { createPortal } from "react-dom";
import { AutoSizer, Grid } from "react-virtualized";
import Highlighter from "react-highlight-words";
import cx from "classnames";
import S from "./MapPerspectiveUI.module.scss";
import { CtxMapRootStore } from "./stores/MapRootStore";

export function MapPerspectiveSearch() {
  const { mapSearchStore } = useContext(CtxMapRootStore);
  return (
    <Observer>
      {() => (
        <div className={S.mapSearch} ref={mapSearchStore.refSearchField}>
          <input
            className={"input"}
            value={mapSearchStore.searchInputValue}
            onChange={mapSearchStore.handleSearchInputChange}
            onKeyDown={mapSearchStore.handleSearchInputKeyDown}
          />
          <button className={"inputCaret"} onMouseDown={mapSearchStore.handleCaretMouseDown}>
            <i className="fas fa-caret-down" />
          </button>
          {mapSearchStore.isDropped &&
            createPortal(
              <MapPerspectiveSearchDropdown
                domRef={mapSearchStore.refDropdown}
                top={mapSearchStore.dropdownTop + 2}
                left={mapSearchStore.dropdownLeft}
                width={mapSearchStore.dropdownWidth}
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

const ROW_HEIGHT = 25;

export function SearchResults(props: { width: number }) {
  const { mapSearchStore } = useContext(CtxMapRootStore);
  const rowCount = mapSearchStore.searchResults.length;
  return (
    <Observer>
      {() => (
        <Grid
          width={props.width}
          height={Math.min(rowCount * ROW_HEIGHT, 10 * ROW_HEIGHT)}
          rowHeight={ROW_HEIGHT}
          columnWidth={props.width}
          rowCount={rowCount}
          columnCount={1}
          cellRenderer={({ key, style, rowIndex, columnIndex }) => {
            return (
              <Observer key={key}>
                {() => (
                  <div
                    className={cx("cell", { c1: rowIndex % 2 === 0, c2: rowIndex % 2 === 1 })}
                    style={style}
                  >
                    <Highlighter
                      searchWords={[mapSearchStore.searchPhrase]}
                      textToHighlight={mapSearchStore.searchResults[rowIndex].name}
                    />
                  </div>
                )}
              </Observer>
            );
          }}
        />
      )}
    </Observer>
  );
}
