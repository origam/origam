import { IInfoSubsection } from "gui02/connections/types";
import { action, observable } from "mobx";
import { onSidebarAuditSectionExpanded } from "model/actions-ui/RecordInfo/onSidebarAuditSectionExpanded";
import { onSidebarInfoSectionCollapsed } from "model/actions-ui/RecordInfo/onSidebarInfoSectionCollapsed";
import { onSidebarInfoSectionExpanded } from "model/actions-ui/RecordInfo/onSidebarInfoSectionExpanded";
import { getWorkbench } from "model/selectors/getWorkbench";
import { ISearchResult } from "./types/ISearchResult";

export class SidebarState {
  
  @observable 
  searchResults: ISearchResult[] = [];
  
  @observable
   _activeSection = "Menu";

   @observable
   activeInfoSubsection = IInfoSubsection.Info;

   parent: any;

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
  onSearchResultsChange(results: ISearchResult[]) {
    this.searchResults = results;
    this.activeSection = "Search";
  }
}