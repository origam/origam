import { getTablePanelView } from "../selectors/TablePanelView/getTablePanelView";
import { getDataView } from "../selectors/DataView/getDataView";
import { IFocusable } from "./FormFocusManager";
import { getFilterConfiguration } from "model/selectors/DataView/getFilterConfiguration";

export class GridFocusManager {

  constructor(public parent: any) {
  }

  public focusTableOnReload: boolean = true;

  focusTableIfNeeded() {
    const filtersVisible = getFilterConfiguration(this.parent).isFilterControlsDisplayed;
    const filterInputHasFocus = filtersVisible && document.activeElement?.tagName === 'INPUT';
    if (this.focusTableOnReload && !filterInputHasFocus) {
      getTablePanelView(this)?.triggerOnFocusTable();
    } else {
      this.focusTableOnReload = true;
    }
  }

  activeEditor: IFocusable | undefined;

  focusEditor() {
    this.activeEditor?.focus();
  }
}

export function getGridFocusManager(ctx: any) {
  return getDataView(ctx).gridFocusManager;
}