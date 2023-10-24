import { getTablePanelView } from "../selectors/TablePanelView/getTablePanelView";
import { getDataView } from "../selectors/DataView/getDataView";
import { IFocusable } from "./FormFocusManager";
import { getFilterConfiguration } from "model/selectors/DataView/getFilterConfiguration";
import { requestFocus } from "utils/focus";
import { IDataView } from "model/entities/types/IDataView";

export class GridFocusManager {
  private _activeEditor: IFocusable | undefined;
  private _lastFocusedFilter: IFocusable | undefined;
  public focusTableOnReload: boolean = true;
  private readonly _dataViewModelInstanceId: string;
  get canFocusTable(){
    return !this._activeEditor;
  }

  get dataViewModelInstanceId(): string {
    return this._dataViewModelInstanceId;
  }

  constructor(public parent: IDataView) {
    this._dataViewModelInstanceId = parent.modelInstanceId;
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

  editorBlur?: (event?: any) => Promise<void>

  async activeEditorCloses(){
    if(this.editorBlur){
      await this.editorBlur();
      this.editorBlur = undefined;
    }
  }
  focusEditor() {
    requestFocus(this._activeEditor as any);
  }

  setLastFocusedFilter(focusable: IFocusable) {
    this._lastFocusedFilter = focusable;
  }

  refocusLastFilter() {
    requestFocus(this._lastFocusedFilter as any);
  }
}

export function getGridFocusManager(ctx: any) {
  return getDataView(ctx).gridFocusManager;
}