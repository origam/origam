import { IInfoSubsection } from "gui02/connections/types";
import { action, observable } from "mobx";
import { onSidebarAuditSectionExpanded } from "model/actions-ui/RecordInfo/onSidebarAuditSectionExpanded";
import { onSidebarInfoSectionCollapsed } from "model/actions-ui/RecordInfo/onSidebarInfoSectionCollapsed";
import { onSidebarInfoSectionExpanded } from "model/actions-ui/RecordInfo/onSidebarInfoSectionExpanded";
import { getWorkbench } from "model/selectors/getWorkbench";
import { ISearchResultGroup } from "./types/ISearchResultGroup";

export class SidebarState {
  
  @observable 
  searchResultGroups: ISearchResultGroup[]= [];
  
  @observable
  _activeSection = "Menu";

  @observable
  activeInfoSubsection = IInfoSubsection.Info;

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