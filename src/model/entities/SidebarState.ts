import { IInfoSubsection } from "gui/connections/types";
import { action, observable } from "mobx";
import { onSidebarAuditSectionExpanded } from "model/actions-ui/RecordInfo/onSidebarAuditSectionExpanded";
import { onSidebarInfoSectionCollapsed } from "model/actions-ui/RecordInfo/onSidebarInfoSectionCollapsed";
import { onSidebarInfoSectionExpanded } from "model/actions-ui/RecordInfo/onSidebarInfoSectionExpanded";
import { getWorkbench } from "model/selectors/getWorkbench";
import { RefObject } from "react";
import { IMainMenuState } from "./types/IMainMenu";
import { ISearchResultGroup } from "./types/ISearchResultGroup";

export class SidebarState {
  
  @observable 
  searchResultGroups: ISearchResultGroup[]= [];
  
  @observable
  _activeSection = "Menu";

  @observable
  activeInfoSubsection = IInfoSubsection.Info;

  mainMenuState: IMainMenuState = new MainMenuState();

  parent: any;

  get resultCount(){ 
    return this.searchResultGroups.length === 0 
      ? 0 
      : this.searchResultGroups.map(group => group.results.length).reduce((x,y) => x + y);
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
  
  @observable
  folderStateMap: Map<string, boolean> = new Map();

  refMap: Map<string, RefObject<HTMLElement>> = new Map();

  @observable
  private _highLightedItemId: string | undefined;
  
  closeAll(){
    this.folderStateMap.clear();
  }

  isOpen(menuId: string): boolean {
    return this.folderStateMap.get(menuId) ?? false;
  }

  setIsOpen(menuId: string, state: boolean){
    this.folderStateMap.set(menuId, state);
  }

  flipIsOpen(menuId: string){
    const newState = !this.isOpen(menuId);
    this.setIsOpen(menuId, newState);
  }

  setReference(id: string, ref: RefObject<HTMLElement>): void{
    this.refMap.set(id, ref);
  }

  scrollToItem(id: string){
    this.refMap.get(id)?.current?.scrollIntoView();
  }

  public get highLightedItemId() {
    return this._highLightedItemId;
  }

  highlightItem(itemId: string){
    this._highLightedItemId = itemId;
    setTimeout(() => this._highLightedItemId = undefined, 3000)
  }
}