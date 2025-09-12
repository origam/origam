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

import _ from "lodash";
import { createMachine, interpret, Interpreter } from "xstate";
import { PubSub } from "./APIService";
import { HashtagRootStore } from "../stores/RootStore";
import { WindowsSvc } from "../../../components/Windows/WindowsSvc";
import { renderHashtaggingDialog } from "../HashtaggingApp";
import {
  action,
  computed,
  observable,
  runInAction,
} from "mobx";
import {
  Column,
  Column2TouchMoveControlee,
  DataSourceField,
} from "../stores/DataTableStore";
import { ObjectTouchMover } from "../util/ObjectTouchMover";
import { renderErrorDialog } from "../../../components/Dialogs/ErrorDialog";
import { renderSimpleProgress } from "../../../components/Windows/Windows";
import { T } from "util/translation";

/*
inspect({
  iframe: false, // open in new window
});*/

export class ScreenProcess {
  constructor(public root: HashtagRootStore, public windowsSvc: WindowsSvc) {}

  @observable state: any = undefined;

  get apiService() {
    return this.root.apiService;
  }

  get dataTableCategories() {
    return this.root.dataTableStore.getDataTable("categories");
  }

  get dataTableObjects() {
    return this.root.dataTableStore.getDataTable("objects");
  }

  @computed get isCategoriesLoading() {
    return this.state?.matches({ DIALOG_DISPLAYED: "LOAD_CATEGORIES" });
  }

  @computed get isObjectsLoading() {
    return this.state?.matches({ DIALOG_DISPLAYED: "LOAD_OBJECTS" });
  }

  interpreter?: Interpreter<any>;

  feedChoosenHashtags: (
    items: Array<{
      hashtagCategoryName: string;
      hashtagObjectId: any;
      hashtagLabel: string;
    }>
  ) => void = () => {};

  isLastPageLoaded = false;
  lastPageLoaded = 0;

  start(
    feedChoosenHashtags: (
      items: Array<{
        hashtagCategoryName: string;
        hashtagObjectId: any;
        hashtagLabel: string;
      }>
    ) => void
  ) {
    this.feedChoosenHashtags = feedChoosenHashtags;
    this.interpreter = interpret(this.createMachine() /*, { devTools: true }*/) as any;
    this.interpreter!.onTransition((state, event) => {
      this.state = state;
    });
    this.interpreter!.start();
  }

  createMachine() {
    return createMachine(
      {
        id: "screenProcess",
        initial: "DIALOG_DISPLAYED",
        states: {
          DIALOG_DISPLAYED: {
            invoke: [
              {
                src: "svcHashtagDialog",
              },
              {
                src: "svcReactions",
              },
            ],
            onEntry: ["actClearAllData"],
            on: {
              ERROR: ".ERROR_DIALOG",
            },
            initial: "LOAD_CATEGORIES",
            states: {
              LOAD_CATEGORIES: {
                invoke: {
                  src: "svcLoadCategories",
                },
                on: {
                  SEARCH_CATEGORY_CHANGED: {
                    target: "LOAD_CATEGORIES",
                  },
                  DONE: {
                    target: "LOAD_OBJECTS",
                  },
                },
                onExit: [
                  "actFixCategorySelection",
                  "actSelectedCategoryChanged",
                ],
              },
              LOAD_OBJECTS: {
                invoke: {
                  src: "svcLoadObjects",
                },
                on: {
                  SEARCH_OBJECT_CHANGED: {
                    target: "LOAD_OBJECTS",
                  },
                  SEARCH_CATEGORY_CHANGED: {
                    target: "LOAD_CATEGORIES",
                  },
                  SELECTED_CATEGORY_CHANGED: {
                    target: "LOAD_OBJECTS",
                    actions: ["actSelectedCategoryChanged"],
                  },
                  DONE: {
                    target: "IDLE",
                  },
                },
                onExit: ["actFixObjectSelection"],
              },
              IDLE: {
                on: {
                  SEARCH_OBJECT_CHANGED: {
                    target: "LOAD_OBJECTS",
                    actions: ["actClearObjectsData"],
                  },
                  SEARCH_CATEGORY_CHANGED: {
                    target: "LOAD_CATEGORIES",
                  },
                  SELECTED_CATEGORY_CHANGED: {
                    target: "LOAD_OBJECTS",
                    actions: ["actSelectedCategoryChanged"],
                  },
                  CANCEL: "#screenProcess.FINISHED",
                  OK: "CREATE_HASHTAGS",
                  SCROLLED_NEAR_TABLE_END: "LOAD_OBJECTS",
                },
              },
              CREATE_HASHTAGS: {
                invoke: [
                  { src: "svcCreateHashtags" },
                  { src: "svcProgressDialog" },
                ],
                on: {
                  DONE: "#screenProcess.FINISHED",
                },
              },
              ERROR_DIALOG: {
                invoke: { src: "svcErrorDialog" },
                on: {
                  OK: "IDLE",
                },
              },
            },
          },
          FINISHED: {
            type: "final",
          },
        },
      },
      {
        actions: {
          actClearAllData: (ctx, event) => {
            this.clearCategoriesData();
            this.clearObjectsData();
          },
          actClearObjectsData: (ctx, event) => {
            this.clearObjectsData();
          },
          actFixCategorySelection: (ctx, event) => {
            const dataTable = this.root.dataTableStore.getDataTable(
              "categories"
            );
            if (
              dataTable &&
              dataTable?.rowCount > 0 &&
              !dataTable?.selectedRow
            ) {
              dataTable.tableCursor.setSelectedRowId(
                dataTable.getRowId(dataTable.getRowByDataCellIndex(0))
              );
            }
          },
          actSelectedCategoryChanged: (ctx, event) => {
            runInAction(() => {
              const choosenCategoryRow = this.dataTableCategories?.selectedRow;
              if (
                choosenCategoryRow &&
                this.dataTableObjects &&
                this.dataTableCategories
              ) {
                const tableConfig = choosenCategoryRow[2];
                this.clearObjectsData();
                this.dataTableObjects.dataSource.clearFields();
                this.dataTableObjects.dataSource.fields.push(
                  ...tableConfig.dataSourceFields.map(
                    (item: any) =>
                      new DataSourceField(item.name, item.dataIndex)
                  )
                );
                this.dataTableObjects.clearColumns();
                this.dataTableObjects.columns.push(
                  ...tableConfig.columns.map(
                    (item: any) =>
                      new Column(
                        this.dataTableObjects!,
                        item.name,
                        item.label,
                        item.type,
                        item.formatterPattern
                      )
                  )
                );
                for (let c of this.dataTableObjects.columns) {
                  c.touchMover = new ObjectTouchMover(
                    new Column2TouchMoveControlee(c)
                  );
                }
              }
            });
          },
          /*actFixObjectSelection: (ctx, event) => {
          const dataTable = this.root.dataTableStore.getDataTable("objects");
          if (dataTable && dataTable?.rowCount > 0 && !dataTable?.selectedRow) {
            dataTable.tableCursor.setSelectedRowId(
              dataTable.getRowId(dataTable.getRowByDataCellIndex(0))
            );
          }
        },*/
        },
        services: {
          svcLoadCategories: (ctx, event) => (callback, onReceive) => {
            const chCancel = new PubSub();
            this.apiService
              .getCategories(this.categorySearchTerm, 1, 1000, chCancel)
              .then((items) => {
                this.root.dataTableStore
                  .getDataTable("categories")
                  ?.setRows(items);
                callback("DONE");
              })
              .catch((exception) => {
                callback({ type: "ERROR", payload: { exception } });
                console.error(exception);
              });
            return () => chCancel.trig();
          },
          svcLoadObjects: (ctx, event) => (callback, onReceive) => {
            const chCancel = new PubSub();
            const PAGE_SIZE = 1000;
            this.apiService
              .getObjects(
                this.selectedCategoryId || "",
                this.objectSearchTerm,
                this.lastPageLoaded + 1,
                PAGE_SIZE,
                chCancel
              )
              .then(
                action((items) => {
                  this.lastPageLoaded++;
                  if (items.length < PAGE_SIZE) {
                    this.isLastPageLoaded = true;
                  }
                  this.root.dataTableStore
                    .getDataTable("objects")
                    ?.appendRows(items);
                  callback("DONE");
                })
              )
              .catch((e) => {
                if (e.$isCancellation) return;
                throw e;
              })
              .catch((exception) => {
                callback({ type: "ERROR", payload: { exception } });
                console.error(exception);
              });
            return () => chCancel.trig();
          },
          svcHashtagDialog: (ctx, event) => (callback, onReceive) => {
            const hModal = this.windowsSvc.push(() =>
              renderHashtaggingDialog()
            );
            return () => hModal.close();
          },
          svcErrorDialog: (ctx, event) => (callback, onReceive) => {
            const hModal = this.windowsSvc.push(
              renderErrorDialog(event.payload?.exception)
            );
            hModal.interact().then(() => callback("OK"));
            return () => hModal.close();
          },
          svcProgressDialog: (ctx, event) => (callback, onReceive) => {
            const hModal = this.windowsSvc.push(
              renderSimpleProgress(T("Working...", "working..."))
            );
            return () => hModal.close();
          },
          svcReactions: (ctx, event) => (callback, onReceive) => {
            const _disposers: any[] = [];
            return () => {
              for (let h of _disposers) h();
            };
          },
          svcCreateHashtags: (ctx, event) => (callback, onReceive) => {
            const selectedObjectRowIds = this.dataTableObjects?.selectedRowIds;
            const selectedCategoryId = this.dataTableCategories?.tableCursor
              .selectedRowId;
            if (selectedCategoryId && selectedObjectRowIds) {
              this.apiService
                .getHashtagLabels(
                  selectedCategoryId,
                  Array.from(selectedObjectRowIds.values())
                )
                .then((labels) => {
                  this.feedChoosenHashtags(
                    Array.from(selectedObjectRowIds.values()).map((rowId) => {
                      return {
                        hashtagCategoryName: selectedCategoryId,
                        hashtagObjectId: rowId,
                        hashtagLabel: labels[rowId],
                      };
                    })
                  );
                  callback("DONE");
                })
                .catch((exception) => {
                  callback({ type: "ERROR", payload: { exception } });
                  console.error(exception);
                });
            }
          },
        },
      }
    );
  }

  @action.bound
  clearCategoriesData() {
    this.dataTableCategories?.clearData();
  }

  @action.bound
  clearObjectsData() {
    this.dataTableObjects?.clearData();
    this.dataTableObjects?.clearSelectedRows();
    this.isLastPageLoaded = false;
    this.lastPageLoaded = 0;
  }

  @action.bound
  handleUIInitialized() {
    this.interpreter?.send("UI_INITIALIZED");
  }

  @action.bound
  handleCategorySearchChangeImm(value: string) {
    this.interpreter?.send("SEARCH_CATEGORY_CHANGED");
  }

  @action.bound
  handleScrolledNearTableEnd() {
    if (!this.isLastPageLoaded) {
      this.interpreter?.send("SCROLLED_NEAR_TABLE_END");
    }
  }

  handleCategorySearchChange = _.debounce(
    this.handleCategorySearchChangeImm,
    400
  );

  @action.bound
  handleObjectSearchChangeImm(value: string) {
    this.interpreter?.send("SEARCH_OBJECT_CHANGED");
  }

  handleObjectSearchChange = _.debounce(this.handleObjectSearchChangeImm, 400);

  @action.bound
  handleSelectedCategoryChangeImm(rowId: string) {
    this.interpreter?.send("SELECTED_CATEGORY_CHANGED");
  }

  handleSelectedCategoryChange = _.debounce(
    this.handleSelectedCategoryChangeImm,
    400
  );

  @action.bound
  handleCancelClick() {
    this.interpreter?.send("CANCEL");
  }

  @action.bound
  handleCloseClick() {
    this.interpreter?.send("CANCEL");
  }

  @action.bound
  handleOkClick() {
    this.interpreter?.send("OK");
  }

  get selectedCategoryId() {
    return this.root.dataTableStore.getDataTable("categories")?.tableCursor
      .selectedRowId;
  }

  get categorySearchTerm() {
    return this.root.searchStore.spCategories;
  }

  get objectSearchTerm() {
    return this.root.searchStore.spObjects;
  }
}
