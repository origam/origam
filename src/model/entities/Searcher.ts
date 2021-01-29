import {action, computed, observable } from "mobx";
import _ from "lodash";
import {ISearcher} from "./types/ISearcher";
import { ISearchResult, IServerSearchResult } from "./types/ISearchResult";
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
import { getPath } from "model/selectors/MainMenu/menuNode";
import { latinize } from "utils/string";
import { onWorkQueuesListItemClick } from "model/actions-ui/WorkQueues/onWorkQueuesListItemClick";


export class Searcher implements ISearcher {
  parent?: any;
  nodeIndex: NodeContainer[] = [];
  workQueueIndex: NodeContainer[] = [];

  @observable
  selectedResult: ISearchResult | undefined; 
  
  @observable
  serverResultGroups: ISearchResultGroup[] = [];
  
  @observable
  menuResultGroup: ISearchResultGroup | undefined = undefined;
  
  @observable
  workQueueResultGroup: ISearchResultGroup | undefined = undefined;
  
  @computed
  get resultGroups(){
    const groups = this.serverResultGroups.length > 0 
      ? [...this.serverResultGroups] 
      : [];
    if(this.workQueueResultGroup){
      groups.unshift(this.workQueueResultGroup);
    }
    if(this.menuResultGroup){
      groups.unshift(this.menuResultGroup);
    }
    return groups;
  }

  onItemServerClick(searchResult: IServerSearchResult){
    onSearchResultClick(this)(searchResult.dataSourceLookupId, searchResult.referenceId)
  }

  selectFirst(){
    if(this.resultGroups.length === 0 ||
       this.resultGroups[0].results.length === 0)
    {
      return
    }
    this.selectedResult = this.resultGroups[0].results[0];
  }
  
  selectNextResult() {
    if(!this.selectedResult){
      this.selectFirst();
      return;
    }
    const currentGroup = this.resultGroups
      .find(group => group.results.some(result => result.id === this.selectedResult!.id))!;
    if(!currentGroup){
      this.selectFirst();
      return;
    }
    const currentResultIndex = currentGroup.results
      .findIndex(result => result.id === this.selectedResult!.id);
    if(currentResultIndex < currentGroup.results.length -1){
      this.selectedResult = currentGroup.results[currentResultIndex + 1];
    }else{
      const currentGroupIndex = this.resultGroups.indexOf(currentGroup);
      if(currentGroupIndex < this.resultGroups.length -1){
        const newGroup = this.resultGroups[currentGroupIndex + 1];
        newGroup.isExpanded = true;
        this.selectedResult = newGroup.results[0];
      }
    }
  }

  selectPreviousResult(): void {
    if(!this.selectedResult){
      this.selectFirst();
      return;
    }
    const currentGroup = this.resultGroups
      .find(group => group.results.some(result => result.id === this.selectedResult!.id))!;
    if(!currentGroup){
      this.selectFirst();
      return;
    }
    const currentResultIndex = currentGroup.results
      .findIndex(result => result.id === this.selectedResult!.id);
    if(currentResultIndex > 0){
      this.selectedResult = currentGroup.results[currentResultIndex - 1];
    }else{
      const currentGroupIndex = this.resultGroups.indexOf(currentGroup);
      if(currentGroupIndex > 0){
        const newGroup = this.resultGroups[currentGroupIndex - 1];
        newGroup.isExpanded = true;
        this.selectedResult = newGroup.results[newGroup.results.length - 1];
      }
    }
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
          searchResult.id =  searchResult.referenceId;
          searchResult.onClick = ()=> this.onItemServerClick(searchResult);
        }
        const groupMap =  searchResults.groupBy((item: IServerSearchResult) => item.group);  
        this.serverResultGroups = Array.from(groupMap.keys())
          .sort()
          .map(name => new SearchResultGroup(name, groupMap.get(name)!));
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

  onWorkQueueItemClicked(item: any){
    openSingleMenuFolder(item, this);
    const sidebarState = getWorkbench(this).sidebarState;
    onWorkQueuesListItemClick(this)(null, item);
    sidebarState.activeSection = "WorkQueues";
    // getMainMenuState(this).scrollToItem(item.attributes.id);
  }

  @action.bound doSearchTermImm(term: string) {
    const latinizedTerm = latinize(term.trim()).toLowerCase();
    const searchResults = this.nodeIndex
          .filter(container => {
            return container.latinizedLowerLabel.includes(latinizedTerm);
          })
          .map((container: any) => {
            const node = container.node;
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
            }) as ISearchResult[];
    this.menuResultGroup = new SearchResultGroup(T("Menu", "menu"), searchResults);
    
    const workQueueSearchResults = this.workQueueIndex
    .filter(container => {
      return container.latinizedLowerLabel.includes(latinizedTerm);
    })
    .map((container: any) => {
      const item = container.node;
      return {
        id: item.id,
        type: "",
        icon: IMenuItemIcon.WorkQueue,
        label: item.name,
        description: "",
        onClick: ()=>this.onWorkQueueItemClicked(item)
      };
    }) as ISearchResult[];

    this.workQueueResultGroup = new SearchResultGroup(T("Work Queue", "queue_results"), workQueueSearchResults);
  }


  @action.bound
  indexWorkQueues(items: any[]){
    items
      .map(item => new NodeContainer(latinize(item.name).toLowerCase(), item))
      .forEach(container => this.workQueueIndex.push(container));
  }

  @action.bound
  indexMainMenu(mainMenu: any) {
    const recursive = (node: any) => {
      if (node.attributes.isHidden === "true") {
        return;
      }
      switch (node.name) {
        case "Submenu":
        case "Command":
          this.nodeIndex.push(new NodeContainer(latinize(node.attributes.label).toLowerCase(),node));
      }
      node.elements.forEach((element: any) => recursive(element));
    };
    mainMenu.elements.forEach((element: any) => {
      recursive(element);
    });
  }

  @action.bound
  clear(){
    this.serverResultGroups = [];
    this.menuResultGroup = undefined;
  }
}

class SearchResultGroup implements ISearchResultGroup{
  @observable
  isExpanded: boolean = true;

  constructor(
    public name: string,
    public results: ISearchResult[],
  )
  {}
}

class NodeContainer {
  constructor(
    public latinizedLowerLabel: string,
    public node: any
  ){}
}
