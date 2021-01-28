import {action, computed, observable } from "mobx";
import FlexSearch from "flexsearch";
import _ from "lodash";
import {ISearcher} from "./types/ISearcher";
import { IServerSearchResult } from "./types/ISearchResult";
import { onMainMenuItemClick } from "model/actions-ui/MainMenu/onMainMenuItemClick";
import { ISearchResultGroup } from "./types/ISearchResultGroup";
import { runInFlowWithHandler } from "utils/runInFlowWithHandler";
import { getApi } from "model/selectors/getApi";
import { IMenuItemIcon } from "gui/Workbench/MainMenu/MainMenu";
import { onSearchResultClick } from "model/actions/Workbench/onSearchResultClick";
import { T } from "utils/translation";
import { openSingleMenuFolder } from "model/selectors/MainMenu/getMainMenuUI";
import { getWorkbench } from "model/selectors/getWorkbench";
import { getMainMenuState } from "model/selectors/MainMenu/getMainMenuState";


export class Searcher implements ISearcher {
  parent?: any;
  index: any;

  @observable
  serverResultGroups: ISearchResultGroup[] = [];
  
  @observable
  menuResultGroup: ISearchResultGroup | undefined = undefined;

  @computed
  get resultGroups(){
    return this.menuResultGroup && this.menuResultGroup.results.length > 0 
      ? [this.menuResultGroup, ...this.serverResultGroups]
      : this.serverResultGroups;
  }

  onItemServerClick(searchResult: IServerSearchResult){
    onSearchResultClick(this)(searchResult.dataSourceLookupId, searchResult.referenceId)
  }

  searchOnServer(){
    if(!this.searchTerm.trim()){
      this.serverResultGroups = [];
      return;
    }
    runInFlowWithHandler({
      ctx: this, 
      action : async ()=> 
      {
        const api = getApi(this);
        const searchResults = await api.search(this.searchTerm);
        for (const searchResult of searchResults) {
          searchResult.icon = IMenuItemIcon.Form;
          searchResult.onClick = ()=> this.onItemServerClick(searchResult);
        }
        const groupMap =  searchResults.groupBy((item: IServerSearchResult) => item.group);  
        this.serverResultGroups = Array.from(groupMap.keys())
                .sort()
                .map(name => { return {name: name, results: groupMap.get(name)!}})
      } 
    });
  }

  searchTerm = "";

  @action.bound onSearchFieldChange(searchTerm: string) {
    this.searchTerm = searchTerm;
    if(searchTerm.trim() === ""){
      this.menuResultGroup = undefined;
      return;
    }
    this.doSearchTerm(searchTerm);
  }

  doSearchTerm = _.throttle(this.doSearchTermImm, 100);

  onCommandClicked(node: any){
    onMainMenuItemClick(this)({
      event: null,
      item: node,
      idParameter: undefined,
    });
    const sidebarState = getWorkbench(this).sidebarState;
    sidebarState.onSearchResultsChange(this.resultGroups);
  }

  onSubMenuClicked(node: any){
    openSingleMenuFolder(node, this);
    const sidebarState = getWorkbench(this).sidebarState;
    sidebarState.activeSection = "Menu";
    getMainMenuState(this).scrollToItem(node.attributes.id);
  }

  @action.bound doSearchTermImm(term: string) {
    if (!this.index) return;
    const searchResults = 
        this.index.search(term).map((node: any) => {
          switch (node.name) {
            case "Submenu":
              return {
                id: node.attributes.id,
                type: "Submenu",
                icon: node.attributes.icon,
                label: node.attributes.label,
                description: getPath(node),
                onClick: ()=>this.onSubMenuClicked(node)
              };
            case "Command":
              return {
                id: node.attributes.id,
                type: "Command",
                icon: node.attributes.icon,
                label: node.attributes.label,
                description: getPath(node),
                onClick: ()=>this.onCommandClicked(node)
              };
          }
        })
    this.menuResultGroup = {name: T("Menu", "menu"), results: searchResults};
  }


  @action.bound
  indexMainMenu(mainMenu: any) {
    this.index = FlexSearch.create({
      encode: "extra",
      doc: {
        id: "attributes:id",
        field: "attributes:label"
      }
    } as any);
    const documents: any[] = [];
    const recursive = (node: any) => {
      if (node.attributes.isHidden === "true") {
        return;
      }
      switch (node.name) {
        case "Submenu":
        case "Command":
          documents.push(node);
      }
      node.elements.forEach((element: any) => recursive(element));
    };
    mainMenu.elements.forEach((element: any) => {
      recursive(element);
    });
    this.index.add(documents);
  }

  @action.bound
  clear(){
    this.serverResultGroups = [];
    this.menuResultGroup = undefined;
  }
}
