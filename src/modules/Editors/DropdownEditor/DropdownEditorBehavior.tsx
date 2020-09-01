import {TypeSymbol} from "dic/Container";
import _ from "lodash";
import {action, computed, flow, observable, reaction} from "mobx";
import {DropdownEditorSetup} from "./DropdownEditor";
import {DropdownEditorApi} from "./DropdownEditorApi";
import {CancellablePromise, EagerlyLoadedGrid, LazilyLoadedGrid} from "./DropdownEditorCommon";
import {IDropdownEditorData} from "./DropdownEditorData";
import {DropdownEditorLookupListCache} from "./DropdownEditorLookupListCache";
import {DropdownDataTable} from "./DropdownTableModel";
import {IFocusable} from "../../../model/entities/FocusManager";

export class DropdownEditorBehavior {
  constructor(
    private api: DropdownEditorApi,
    private data: IDropdownEditorData,
    private dataTable: DropdownDataTable,
    private setup: () => DropdownEditorSetup,
    private cache: DropdownEditorLookupListCache,
    public isReadOnly: boolean,
    public onDoubleClick?: (event: any) => void,
    public subscribeToFocusManager?: (obj: IFocusable) => () => void,
    public tabIndex?: number
  ) {
  }

  @observable isDropped = false;
  @observable isWorking = false;
  @observable userEnteredValue = undefined;
  @observable scrollToRowIndex: number | undefined = undefined;

  @observable cursorRowId = "";

  willLoadPage = 1;
  willLoadNextPage = true;
  pageSize = 100;
  unsubscribeFromFocusManager?: () => void;

  @computed get choosenRowId() {
    return this.data.value;
  }

  @computed get inputValue() {
    return this.userEnteredValue !== undefined ? this.userEnteredValue : this.data.text;
  }

  @action.bound dropUp() {
    if (this.isDropped) {
      this.ensureRequestCancelled();
      this.dataTable.clearData();
      this.userEnteredValue = undefined;
      this.isDropped = false;
      this.willLoadPage = 1;
      this.willLoadNextPage = true;
      this.scrollToRowIndex = 0;
    }
  }

  @action.bound dropDown() {
    const setup = this.setup();
    if (!this.isDropped) {
      switch (setup.dropdownType) {
        case EagerlyLoadedGrid: {
          this.dataTable.setFilterPhrase(this.userEnteredValue || "");
          if (this.cache.hasCachedListRows()) {
            this.dataTable.setData(this.cache.getCachedListRows());
          } else {
            this.ensureRequestRunning();
          }
          break;
        }
        case LazilyLoadedGrid: {
          this.ensureRequestRunning();
        }
      }
      this.isDropped = true;
      this.makeFocused();
    }
  }

  makeFocused() {
    if (this.elmInputElement) {
      this.elmInputElement.focus();
    }
  }

  @action.bound
  scrollToCursoredRowIfNeeded() {
    const index = this.dataTable.getRowIndexById(this.cursorRowId);
    if (index > -1) {
      this.scrollToRowIndex = index + 1;
    }
  }

  @action.bound handleInputFocus(event: any) {
    const {target} = event;
    setTimeout(() => {
      if (target) {
        target.select();
        target.scrollLeft = 0;
      }
    }, 10);
  }

  @action.bound handleInputBlur(event: any) {
    if (
      this.userEnteredValue !== undefined &&
      this.isDropped &&
      !this.isWorking &&
      this.cursorRowId
    ) {
      this.data.chooseNewValue(this.cursorRowId);
    }
    this.dropUp();
  }

  @action.bound
  handleInputBtnClick(event: any) {
    if (!this.isDropped) {
      this.dropDown();
    } else {
      this.dropUp();
    }
  }

  @action.bound
  handleInputKeyDown(event: any) {
    switch (event.key) {
      case "Escape":
        this.dropUp();
        this.userEnteredValue = undefined;
        break;
      case "Enter":
        if (this.isDropped && !this.isWorking && this.cursorRowId) {
          this.data.chooseNewValue(this.cursorRowId);
          this.dropUp();
        }
        break;
      case "ArrowUp":
        if (this.isDropped && this.cursorRowId) {
          event.preventDefault();
          const prevRowId = this.dataTable.getRowIdBeforeId(this.cursorRowId);
          if (prevRowId) {
            this.cursorRowId = prevRowId;
          }
          this.scrollToCursoredRowIfNeeded();
        }
        break;
      case "ArrowDown":
        if (this.isDropped && this.cursorRowId) {
          event.preventDefault();
          if (this.cursorRowId) {
            const nextRowId = this.dataTable.getRowIdAfterId(this.cursorRowId);
            if (nextRowId) {
              this.cursorRowId = nextRowId;
            }
          }
          this.scrollToCursoredRowIfNeeded();
        } else {
          this.dropDown();
        }
        break;
    }
  }

  handleInputChangeDeb = _.debounce(this.handleInputChangeImm, 300);

  @action.bound
  handleInputChange(event: any) {
    this.userEnteredValue = event.target.value;
    this.isDropped = true;

    if (this.setup().dropdownType === EagerlyLoadedGrid) {
      if (this.cache.hasCachedListRows()) {
        this.dataTable.setData(this.cache.getCachedListRows());
      } else {
        this.ensureRequestRunning();
      }
      this.dataTable.setFilterPhrase(this.userEnteredValue || "");
      this.trySelectFirstRow();
    } else if (this.setup().dropdownType === LazilyLoadedGrid) {
      this.handleInputChangeDeb();
    }
  }

  @action.bound handleInputChangeImm() {
    if (this.setup().dropdownType === LazilyLoadedGrid) {
      this.willLoadNextPage = true;
      this.willLoadPage = 1;
      this.dataTable.clearData();
    }
    this.ensureRequestCancelled();
    this.ensureRequestRunning();
  }

  @action.bound handleTableCellClicked(event: any, visibleRowIndex: any) {
    const id = this.dataTable.getRowIdentifierByIndex(visibleRowIndex);
    this.data.chooseNewValue(id);
    this.dropUp();
  }

  @action.bound
  handleControlMouseDown(event: any) {
    if (this.isDropped) event.stopPropagation();
  }

  @action.bound
  handleBodyMouseDown(event: any) {
    if (this.isDropped) {
      event.stopPropagation();
      event.preventDefault();
    }
  }

  @action.bound
  handleWindowMouseDown(event: any) {
    if (this.isDropped) {
      this.dropUp();
    }
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
    console.log(args);
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
  }

  @action.bound
  ensureRequestRunning() {
    if (!this.isWorking) {
      this.runGetLookupList(
        this.setup().dropdownType === EagerlyLoadedGrid ? "" : this.userEnteredValue || ""
      );
    }
  }

  @action.bound ensureRequestCancelled() {
    if (this.runningPromise) this.runningPromise.cancel();
  }

  runningPromise: CancellablePromise<any> | undefined;

  @action.bound trySelectFirstRow() {
    if (this.dataTable.rows.length > 0) {
      this.cursorRowId = this.dataTable.getRowIdentifierByIndex(0);
    }
  }

  @action.bound runGetLookupList(searchTerm: string) {
    const self = this;
    this.runningPromise = flow(function* () {
      try {
        self.isWorking = true;
        const setup = self.setup();
        const items = yield* self.api.getLookupList(searchTerm);
        items.sort(compareLookupItems);
        switch (setup.dropdownType) {
          case EagerlyLoadedGrid: {
            self.dataTable.setData(items);
            self.cache.setCachedListRows(items);
            break;
          }
          case LazilyLoadedGrid: {
            if (items.length < self.pageSize) {
              self.willLoadNextPage = false;
            }
            self.scrollToRowIndex = undefined;
            self.dataTable.appendData(items);
            break;
          }
        }
        if (!self.cursorRowId) self.trySelectFirstRow();
      } finally {
        self.isWorking = false;
        self.runningPromise = undefined;
      }
    })();
  }

  @action.bound handleUseEffect() {
    return reaction(
      () => this.data.value,
      () => {
        this.userEnteredValue = undefined;
      }
    );
  }

  _refInputDisposer: any;
  refInputElement = (elm: any) => {
    this.elmInputElement = elm;
    if (this.elmInputElement && this.subscribeToFocusManager) {
      this.unsubscribeFromFocusManager = this.subscribeToFocusManager(this.elmInputElement);
    }
  };

  elmInputElement: any;
  refDropdownBody = (elm: any) => (this.elmDropdownBody = elm);
  elmDropdownBody: any;
}

function compareLookupItems( a: any[], b: any[] ) {
  if ( a[1] < b[1] ){
    return -1;
  }
  if ( a[1] > b[1] ){
    return 1;
  }
  return 0;
}

export const IDropdownEditorBehavior = TypeSymbol<DropdownEditorBehavior>(
  "IDropdownEditorBehavior"
);
