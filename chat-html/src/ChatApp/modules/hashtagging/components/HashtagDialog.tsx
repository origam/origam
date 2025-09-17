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

import React, { useEffect } from "react";
import { CategoryTable } from "./CategoryTable";
import { ObjectTable } from "./ObjectTable";
import { CtxEntityId } from "./DataTableCommon";
import { useRootStore } from "./Common";
import { observer } from "mobx-react";

import Split from "react-split";
import { BigSpinner } from "../../../components/BigSpinner";
import { T } from "util/translation";

export const HashtagDialogContent = observer(function HashtagDialogContent() {
  const rootStore = useRootStore();
  useEffect(() => {
    rootStore.screenProcess.handleUIInitialized();
  }, [rootStore.screenProcess]);
  return (
    <div className="hashtagDialogContent">
      <div className="searchPanes">
        <Split
          direction="horizontal"
          sizes={[30, 70]}
          className="splitter horizontal"
          style={{ height: "100%" }}
        >
          <div className="splitPaneH">
            <div className="formSection">
              <div className="label">{T("Choose a category:", "choose_category")}</div>
            </div>
            <CtxEntityId.Provider value="categories">
              <div className="dataTable">
                <CategoryTable />
                {rootStore.screenProcess.isCategoriesLoading && (
                  <div className="dataTableOverlay">
                    <BigSpinner />
                  </div>
                )}
              </div>
            </CtxEntityId.Provider>
          </div>
          <div className="splitPaneH">
            <div className="formSection">
              <div className="label">{T("Search object:", "search_object")}</div>
              <ObjectSearch />
            </div>
            <CtxEntityId.Provider value="objects">
              <div className="dataTable">
                <ObjectTable />
                {rootStore.screenProcess.isObjectsLoading  && (
                  <div className="dataTableOverlay">
                    <BigSpinner />
                  </div>
                )}
              </div>
            </CtxEntityId.Provider>
          </div>
        </Split>
      </div>
    </div>
  );
});

export const CategorySearch = observer(function CategorySearch() {
  const root = useRootStore();
  return (
    <div className="inputField">
      <input
        className="categorySearch__input"
        value={root.searchStore.spCategories}
        onChange={root.searchStore.handleSPCategoriesChange}
      />
    </div>
  );
});

export const ObjectSearch = observer(function ObjectSearch() {
  const root = useRootStore();
  return (
    <div className="inputField">
      <input
        className="objectSearch__input"
        value={root.searchStore.spObjects}
        onChange={root.searchStore.handleSPObjectsChange}
      />
    </div>
  );
});

export const ConfirmSelectionBtn = observer(function ConfirmSelectionBtn() {
  const rootStore = useRootStore();
  const selRowCount = rootStore.dataTableStore.getDataTable("objects")
    ?.selectedRowCount;
  if (!selRowCount) return null;
  return (
    <button className="button__isPrimary">
      {T("Ok, create links", "ok_create_links")}&nbsp;<span>({selRowCount})</span>
    </button>
  );
});

export const ClearSelectionBtn = observer(function ConfirmSelectionBtn() {
  const rootStore = useRootStore();
  const dtObjects = rootStore.dataTableStore.getDataTable("objects");
  const selRowCount = dtObjects?.selectedRowCount;
  if (!selRowCount) return null;
  return (
    <button
      className="button__isPrimary"
      onClick={() => dtObjects?.clearSelectedRows()}
    >
      {T("Clear selected items", "clear_selected_items")}
    </button>
  );
});
