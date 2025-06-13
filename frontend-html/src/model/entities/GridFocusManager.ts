/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
    if (this.isTablePerspectiveActive()) {
      requestFocus(this._activeEditor as any);
    }
  }

  setLastFocusedFilter(focusable: IFocusable) {
    this._lastFocusedFilter = focusable;
  }

  refocusLastFilter() {
    if (this.isTablePerspectiveActive()) {
      requestFocus(this._lastFocusedFilter as any);
    }
  }

  private isTablePerspectiveActive(){
    const dataView = getDataView(this.parent);
    return dataView.isTableViewActive();
  }
}

export function getGridFocusManager(ctx: any) {
  return getDataView(ctx).gridFocusManager;
}