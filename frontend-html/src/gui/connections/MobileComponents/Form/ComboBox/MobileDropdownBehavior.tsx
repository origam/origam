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


import { IDropdownEditorApi } from "modules/Editors/DropdownEditor/DropdownEditorApi";
import { IDropdownEditorData } from "modules/Editors/DropdownEditor/DropdownEditorData";
import { DropdownDataTable } from "modules/Editors/DropdownEditor/DropdownTableModel";
import { DropdownEditorLookupListCache } from "modules/Editors/DropdownEditor/DropdownEditorLookupListCache";
import { action, computed, flow, observable } from "mobx";
import {
  CancellablePromise,
  EagerlyLoadedGrid,
  LazilyLoadedGrid
} from "modules/Editors/DropdownEditor/DropdownEditorCommon";
import _ from "lodash";
import {
  compareLookUpItems,
  dropdownPageSize,
  IDropdownEditorBehavior
} from "modules/Editors/DropdownEditor/DropdownEditorBehavior";
import { DropdownEditorSetup } from "modules/Editors/DropdownEditor/DropdownEditorSetup";
import { requestFocus } from "utils/focus";

export interface IMobileBehaviorData {
  api: IDropdownEditorApi,
  data: IDropdownEditorData,
  dataTable: DropdownDataTable,
  setup: () => DropdownEditorSetup,
  cache: DropdownEditorLookupListCache,
  onValueSelected?: () => void,
  autoSort?: boolean,
  onTextOverflowChanged?: (tooltip: string | null | undefined) => void,
}

export class MobileDropdownBehavior implements IDropdownEditorBehavior{

  private api: IDropdownEditorApi;
  private data: IDropdownEditorData;
  private dataTable: DropdownDataTable;
  private setup: () => DropdownEditorSetup;
  private cache: DropdownEditorLookupListCache;
  public onValueSelected?: () => void;
  private autoSort?: boolean;
  private onTextOverflowChanged?: (tooltip: string | null | undefined) => void;

  constructor(args: IMobileBehaviorData) {
    this.api = args.api;
    this.data = args.data;
    this.dataTable = args.dataTable;
    this.setup = args.setup;
    this.cache = args.cache;
    this.onValueSelected = args.onValueSelected;
    this.autoSort = args.autoSort;
    this.onTextOverflowChanged = args.onTextOverflowChanged;
  }

  @observable isWorking = false;
  @observable userEnteredValue: string | undefined = undefined;
  @observable scrollToRowIndex: number | undefined = undefined;
  dontClearScrollToRow = true;
  hasNewScreenButton = false;

  @observable cursorRowId = "";

  willLoadPage = 1;
  willLoadNextPage = true;

  @computed get chosenRowId() {
    return this.data.value;
  }

  @computed get inputValue() {
    return this.userEnteredValue ?? "";
  }

  makeFocused() {
    if (this.elmInputElement) {
      requestFocus(this.elmInputElement);
    }
  }

  @action.bound
  private scrollToChosenRowIfPossible() {
    if (this.chosenRowId && !_.isArray(this.chosenRowId)) {
      const index = this.dataTable.getRowIndexById(this.chosenRowId);
      if (index > -1) {
        this.dontClearScrollToRow = true;
        this.scrollToRowIndex = index + 1;
      }
    }
  }

  handleInputChangeDeb = _.debounce(this.handleInputChangeImm, 300);

  @action.bound
  handleInputChange(event: any) {
    this.userEnteredValue = event.target.value;

    this.dataTable.setFilterPhrase(this.userEnteredValue || "");
    if (this.setup().dropdownType === EagerlyLoadedGrid) {
      if (this.setup().cached && this.cache.hasCachedListRows()) {
        this.dataTable.setData(this.cache.getCachedListRows());
      } else {
        this.ensureRequestRunning();
      }
      this.trySelectFirstRow();
    } else if (this.setup().dropdownType === LazilyLoadedGrid) {
      this.handleInputChangeDeb();
    }
    this.updateTextOverflowState();
  }

  @action.bound
  private handleInputChangeImm() {
    if (this.setup().dropdownType === LazilyLoadedGrid) {
      this.willLoadNextPage = true;
      this.willLoadPage = 1;
      this.dataTable.clearData();
    }
    this.ensureRequestCancelled();
    this.ensureRequestRunning();
  }

  @action.bound
  async handleTableCellClicked(event: any, visibleRowIndex: any) {
    const id = this.dataTable.getRowIdentifierByIndex(visibleRowIndex);
    await this.data.chooseNewValue(id);

    this.ensureRequestCancelled();
    this.userEnteredValue = "";
    this.dataTable.setFilterPhrase("");
    this.willLoadPage = 1;
    this.willLoadNextPage = true;
    this.scrollToRowIndex = 0;

    this.onValueSelected?.();
  }

  @action.bound
  handleScroll(args: {
    clientHeight: number;
    clientWidth: number;
    scrollHeight: number;
    scrollLeft: number;
    scrollTop: number;
    scrollWidth: number;
  }) {
    const setup = this.setup();
    if (setup.dropdownType === LazilyLoadedGrid) {
      if (
        this.willLoadNextPage &&
        !this.isWorking &&
        args.scrollTop > args.scrollHeight - args.clientHeight - 500
      ) {
        this.willLoadPage++;
        this.ensureRequestRunning();
      }
    }
    if (this.dontClearScrollToRow) {
      this.dontClearScrollToRow = false;
    } else {
      this.scrollToRowIndex = undefined;
    }
  }

  @action.bound
  private ensureRequestRunning() {
    if (!this.isWorking) {
      this.runGetLookupList(
        this.setup().dropdownType === EagerlyLoadedGrid ? "" : this.userEnteredValue || ""
      );
    }
  }

  @action.bound
  private ensureRequestCancelled() {
    if (this.runningPromise) this.runningPromise.cancel();
  }

  runningPromise: CancellablePromise<any> | undefined;

  @action.bound
  private trySelectFirstRow() {
    if (
      !this.dataTable.getRowById(this.cursorRowId) &&
      this.userEnteredValue &&
      this.dataTable.rows.length > 0
    ) {
      this.cursorRowId = this.dataTable.getRowIdentifierByIndex(0);
    }
  }

  @action.bound
  private selectChosenRow() {
    if (
      this.dataTable.rows.length > 0 &&
      this.chosenRowId &&
      !_.isArray(this.chosenRowId) &&
      this.dataTable.getRowById(this.chosenRowId)
    ) {
      this.cursorRowId = this.chosenRowId;
    }
  }

  updateTextOverflowState() {
    if (!this.elmInputElement) {
      return;
    }
    const textOverflow = this.elmInputElement.offsetWidth < this.elmInputElement.scrollWidth
    this.onTextOverflowChanged?.(textOverflow ? this.inputValue : undefined);
  }

  @action.bound
  private runGetLookupList(searchTerm: string) {
    const self = this;
    this.runningPromise = flow(function*() {
      try {
        self.isWorking = true;
        const setup = self.setup();
        const items = yield*self.api.getLookupList(searchTerm);
        if (self.autoSort) {
          items.sort((i1: any[], i2: any[]) => compareLookUpItems(i1[1], i2[1]))
        }
        if (setup.dropdownType === EagerlyLoadedGrid) {
          self.dataTable.setData(items);
          if (setup.cached) {
            self.cache.setCachedListRows(items);
          }
        } else if (setup.dropdownType === LazilyLoadedGrid) {
          if (items.length < dropdownPageSize) {
            self.willLoadNextPage = false;
          }
          self.scrollToRowIndex = undefined;
          self.dataTable.appendData(items);
        }
        self.trySelectFirstRow();
        self.scrollToChosenRowIfPossible();
        self.selectChosenRow();
      } finally {
        self.isWorking = false;
        self.runningPromise = undefined;
      }
    })();
  }

  refInputElement = (elm: any) => {
    this.elmInputElement = elm;
  };

  elmInputElement: any;
}