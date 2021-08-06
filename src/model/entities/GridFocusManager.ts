import {getTablePanelView} from "../selectors/TablePanelView/getTablePanelView";
import {getDataView} from "../selectors/DataView/getDataView";
import {IFocusable} from "./FormFocusManager";

export class GridFocusManager {

  constructor(public parent: any) {
  }

  public focusTableOnReload: boolean = true;

  focusTableIfNeeded() {
    if (this.focusTableOnReload) {
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