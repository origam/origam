import { getTablePanelView } from "../selectors/TablePanelView/getTablePanelView";
import { getDataView } from "../selectors/DataView/getDataView";
import { IFocusable } from "./FormFocusManager";
import { getFilterConfiguration } from "model/selectors/DataView/getFilterConfiguration";
import { requestFocus } from "utils/focus";

export class GridFocusManager {
  private _activeEditor: IFocusable | undefined;
  public focusTableOnReload: boolean = true;

  constructor(public parent: any) {
  }

  focusTableIfNeeded() {
    const filtersVisible = getFilterConfiguration(this.parent).isFilterControlsDisplayed;
    const filterInputHasFocus = filtersVisible && document.activeElement?.tagName === 'INPUT';
    if (this.focusTableOnReload && !filterInputHasFocus) {
      getTablePanelView(this)?.triggerOnFocusTable();
    } else {
      this.focusTableOnReload = true;
    }
  }

  get activeEditor(): IFocusable | undefined {
    return this._activeEditor;
  }

  set activeEditor(value: IFocusable | undefined) {
    this._activeEditor = value;
    this.focusEditor();
  }

  focusEditor() {
    requestFocus(this._activeEditor as any);
  }
}

export function getGridFocusManager(ctx: any) {
  return getDataView(ctx).gridFocusManager;
}