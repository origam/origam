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

import { IInfoSubsection } from "gui/connections/types";
import { action, observable } from "mobx";
import { onSidebarAuditSectionExpanded } from "model/actions-ui/RecordInfo/onSidebarAuditSectionExpanded";
import { onSidebarInfoSectionCollapsed } from "model/actions-ui/RecordInfo/onSidebarInfoSectionCollapsed";
import { onSidebarInfoSectionExpanded } from "model/actions-ui/RecordInfo/onSidebarInfoSectionExpanded";
import { getWorkbench } from "model/selectors/getWorkbench";
import { RefObject } from "react";
import { IMainMenuState } from "./types/IMainMenu";
import { ISearchResultGroup } from "./types/ISearchResultGroup";

export interface ISidebarState {
  activeSection: string;
  resultCount: number;
}

export class SidebarState implements ISidebarState{

  @observable
  searchResultGroups: ISearchResultGroup[] = [];

  @observable
  _activeSection = "Menu";

  @observable
  activeInfoSubsection = IInfoSubsection.Info;

  mainMenuState: IMainMenuState = new MainMenuState();

  parent: any;

  get resultCount() {
    return this.searchResultGroups.length === 0
      ? 0
      : this.searchResultGroups.map(group => group.results.length).reduce((x, y) => x + y);
  }

  get activeSection() {
    return this._activeSection;
  }

  set activeSection(value) {
    const workbench = getWorkbench(this);
    if (this._activeSection === "Info" && value !== this._activeSection) {
      onSidebarInfoSectionCollapsed(workbench)();
    }
    if (this._activeSection !== "Info" && value === "Info") {
      if (this.activeInfoSubsection === IInfoSubsection.Info) {
        onSidebarInfoSectionExpanded(workbench)();
      }
      if (this.activeInfoSubsection === IInfoSubsection.Audit) {
        onSidebarAuditSectionExpanded(workbench)();
      }
    }
    this._activeSection = value;
  }

  @action
  onSearchResultsChange(results: ISearchResultGroup[]) {
    this.searchResultGroups = results;
    this.activeSection = "Search";
  }
}


export class MainMenuState implements IMainMenuState {

  private readonly folderStateKey = "folderState";

  constructor() {
    this.folderStateMap = this.restoreFolderState();
  }

  flipEditEnabled(): void {
    this.editingEnabled = !this.editingEnabled;
  }

  @observable
  editingEnabled = false;

  @observable
  private readonly folderStateMap: Map<string, boolean>;

  refMap: Map<string, RefObject<HTMLElement>> = new Map();

  @observable
  private _highLightedItemId: string | undefined;

  private restoreFolderState(): Map<string, boolean> {
    try
    {
      const folderStateJson = localStorage.getItem(this.folderStateKey);
      return folderStateJson
        ? new Map(JSON.parse(folderStateJson))
        : new Map();
    }
    catch (error)
    {
      console.warn(error);
      return new Map();
    }
  }

  private persistFolderState(){
    const folderStateJson = JSON.stringify(Array.from(this.folderStateMap.entries()));
    try
    {
      localStorage.setItem(this.folderStateKey, folderStateJson);
    }
    catch (error)
    {
      console.warn(error);
    }
  }

  closeAll() {
    this.folderStateMap.clear();
  }

  isOpen(menuId: string): boolean {
    return this.folderStateMap.get(menuId) ?? false;
  }

  setIsOpen(menuId: string, state: boolean) {
    this.folderStateMap.set(menuId, state);
    this.persistFolderState();
  }

  flipIsOpen(menuId: string) {
    const newState = !this.isOpen(menuId);
    this.setIsOpen(menuId, newState);
  }

  setReference(id: string, ref: RefObject<HTMLElement>): void {
    this.refMap.set(id, ref);
  }

  scrollToItem(id: string) {
    this.refMap.get(id)?.current?.scrollIntoView();
  }

  public get highLightedItemId() {
    return this._highLightedItemId;
  }

  highlightItem(itemId: string) {
    this._highLightedItemId = itemId;
    setTimeout(() => this._highLightedItemId = undefined, 3000)
  }
}