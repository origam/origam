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

import { Observer } from "mobx-react";
import React, { useContext } from "react";
import { createPortal } from "react-dom";
import { Grid } from "react-virtualized";
import Highlighter from "react-highlight-words";
import cx from "classnames";
import S from "./MapPerspectiveUI.module.scss";
import { CtxMapRootStore } from "./stores/MapRootStore";

export function MapPerspectiveSearch() {
  const {mapSearchStore, mapSetupStore} = useContext(CtxMapRootStore);
  return (
    <Observer>
      {() => (
        <>
          {mapSetupStore.isReadOnlyView ? (
            <div className={S.mapSearch} ref={mapSearchStore.refSearchField}>
              <input
                className={"input"}
                autoComplete={"off"}
                value={mapSearchStore.searchInputValue}
                onChange={mapSearchStore.handleSearchInputChange}
                onKeyDown={mapSearchStore.handleSearchInputKeyDown}
                onFocus={mapSearchStore.handleSearchInputFocus}
              />
              <div className={"inputActions"}>
                <button
                  className={"inputClear"}
                  onClick={mapSearchStore.handleClearClick}
                  onMouseDown={mapSearchStore.handleClearMouseDown}
                >
                  <i className="fas fa-times"/>
                </button>
                <button className={"inputCaret"} onMouseDown={mapSearchStore.handleCaretMouseDown}>
                  <i className="fas fa-caret-down"/>
                </button>
              </div>
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
          ) : null}
        </>
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
      style={{top: props.top, left: props.left, width: props.width}}
      className={S.mapSearchDropdown}
    >
      <SearchResults width={props.width - 4}/>
    </div>
  );
}

const ROW_HEIGHT = 25;

export function SearchResults(props: { width: number }) {
  const {mapSearchStore} = useContext(CtxMapRootStore);
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
          cellRenderer={({key, style, rowIndex, columnIndex}) => {
            const searchResult = mapSearchStore.searchResults[rowIndex];
            return (
              <Observer key={key}>
                {() => (
                  <div
                    className={cx("cell", {c1: rowIndex % 2 === 0, c2: rowIndex % 2 === 1})}
                    style={style}
                    onClick={(e) => mapSearchStore.handleSearchResultClick(e, searchResult.id)}
                  >
                    <Highlighter
                      searchWords={[mapSearchStore.searchPhrase]}
                      textToHighlight={searchResult.name}
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
