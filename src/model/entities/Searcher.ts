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

import {action, computed, observable } from "mobx";
import _ from "lodash";
import {IResultIndices, ISearcher} from "./types/ISearcher";
import { ISearchResult, IServerSearchResult } from "./types/ISearchResult";
import { onMainMenuItemClick } from "model/actions-ui/MainMenu/onMainMenuItemClick";
import { ISearchResultGroup } from "./types/ISearchResultGroup";
import { runInFlowWithHandler } from "utils/runInFlowWithHandler";
import { getApi } from "model/selectors/getApi";
import { onSearchResultClick } from "model/actions/Workbench/onSearchResultClick";
import { T } from "utils/translation";
import { openSingleMenuFolder } from "model/selectors/MainMenu/getMainMenuUI";
import { getWorkbench } from "model/selectors/getWorkbench";
import { getMainMenuState } from "model/selectors/MainMenu/getMainMenuState";
import { getPath } from "model/selectors/MainMenu/menuNode";
import { onWorkQueuesListItemClick } from "model/actions-ui/WorkQueues/onWorkQueuesListItemClick";
import { onChatroomsListItemClick } from "model/actions/Chatrooms/onChatroomsListItemClick";
import {getCustomAssetsRoute} from "model/selectors/User/getCustomAssetsRoute";
import { getIconUrl } from "gui/getIconUrl";
import {IMenuItemIcon} from "gui/Workbench/MainMenu/IMenuItemIcon";
import {prepareForFilter} from "model/selectors/PortalSettings/getStringFilterConfig";


export class Searcher implements ISearcher {
  parent?: any;
  nodeIndex: NodeContainer[] = [];
  workQueueIndex: NodeContainer[] = [];
  chatsIndex: NodeContainer[] = [];

  searchTerm = "";

  @observable
  selectedResult: ISearchResult | undefined; 
  
  @observable
  serverResultGroups: ISearchResultGroup[] = [];
  
  @observable
  menuResultGroup: ISearchResultGroup | undefined = undefined;
  
  @observable
  workQueueResultGroup: ISearchResultGroup | undefined = undefined;
  
  @observable
  chatResultGroup: ISearchResultGroup | undefined = undefined;
  
  @computed
  get resultGroups(){
    const groups = this.serverResultGroups.length > 0 
      ? [...this.serverResultGroups] 
      : [];
    if(this.workQueueResultGroup && this.workQueueResultGroup.results.length > 0){
      groups.unshift(this.workQueueResultGroup);
    }
    if(this.menuResultGroup && this.menuResultGroup.results.length > 0){
      groups.unshift(this.menuResultGroup);
    }
    if(this.chatResultGroup && this.chatResultGroup.results.length > 0){
      groups.unshift(this.chatResultGroup);
    }
    return groups;
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
    const resultIndices = this.getSelectedResultIndices();
    if(resultIndices.groupIndex === -1){
      this.selectFirst();
      return;
    }
    const currentGroup = this.resultGroups[resultIndices.groupIndex];
    if(resultIndices.indexInGroup < currentGroup.results.length -1){
      this.selectedResult = currentGroup.results[resultIndices.indexInGroup + 1];
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
    const resultIndices = this.getSelectedResultIndices();
    if(resultIndices.groupIndex === -1){
      this.selectFirst();
      return;
    }
    const currentGroup = this.resultGroups[resultIndices.groupIndex];
    if(resultIndices.indexInGroup > 0){
      this.selectedResult = currentGroup.results[resultIndices.indexInGroup - 1];
    }else{
      const currentGroupIndex = this.resultGroups.indexOf(currentGroup);
      if(currentGroupIndex > 0){
        const newGroup = this.resultGroups[currentGroupIndex - 1];
        newGroup.isExpanded = true;
        this.selectedResult = newGroup.results[newGroup.results.length - 1];
      }
    }
  }

  getSelectedResultIndices() :IResultIndices {
    const currentGroupIndex = this.resultGroups
      .findIndex(group => group.results.some(result => result.id === this.selectedResult!.id))!;
    if(currentGroupIndex === -1){
      return {
        groupIndex: -1,
        indexInGroup: -1
      }
    }
    const currentResultIndex = this.resultGroups[currentGroupIndex].results
      .findIndex(result => result.id === this.selectedResult!.id);
    return {
      groupIndex: currentGroupIndex,
      indexInGroup: currentResultIndex
    }
  }

  searchOnServer(){
    if(!this.searchTerm.trim()){
      this.serverResultGroups = [];
      this.menuResultGroup = undefined;
      this.workQueueResultGroup = undefined;
      this.chatResultGroup = undefined;
      return;
    }
    runInFlowWithHandler({
      ctx: this, 
      action : async ()=> 
      {
        const api = getApi(this);
        const searchResults = await api.search(this.searchTerm);
        for (const searchResult of searchResults) {
          searchResult.iconUrl = this.getIconUrl(IMenuItemIcon.Form);
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

  onItemServerClick(searchResult: IServerSearchResult){
    onSearchResultClick(this)(searchResult.dataSourceLookupId, searchResult.referenceId)
    const sidebarState = getWorkbench(this).sidebarState;
    sidebarState.onSearchResultsChange(this.resultGroups);
  }

  @action.bound onSearchFieldChange(searchTerm: string) {
    this.searchTerm = searchTerm;
    if(searchTerm.trim() === ""){
      this.menuResultGroup = undefined;
      return;
    }
    this.doSearchTerm(searchTerm);
  }

  doSearchTerm = _.throttle(this.doSearchTermImm, 100);

  @action.bound doSearchTermImm(term: string) {
    const searchTerm = prepareForFilter(this, term.trim())!;
    this.searchInMenu(searchTerm);
    this.searchInWorkQueues(searchTerm);
    this.searchInChat(searchTerm);
  }

  getIconUrl(icon: string | IMenuItemIcon){
    const customAssetsRoute = getCustomAssetsRoute(this);
    return getIconUrl(icon,customAssetsRoute + "/" + icon)
  }

  private searchInChat(term: string) {
    const chatSearchResults = this.chatsIndex
      .filter(container => {
        return container.latinizedLowerLabel.includes(term);
      })
      .map((container: any) => {
        const item = container.node;
        return {
          id: item.id,
          type: "",
          iconUrl: this.getIconUrl(IMenuItemIcon.Chat),
          label: item.topic,
          description: "",
          onClick: () => this.onChatItemClicked(item)
        };
      }) as ISearchResult[];

    this.chatResultGroup = new SearchResultGroup(T("Chat", "chat"), chatSearchResults);
  }

  onChatItemClicked(item: any){
    openSingleMenuFolder(item, this);
    const sidebarState = getWorkbench(this).sidebarState;
    onChatroomsListItemClick(this)(null, item);
    sidebarState.activeSection = "Chat";
    getMainMenuState(this).scrollToItem(item.id);
    sidebarState.onSearchResultsChange(this.resultGroups);
  }

  private searchInWorkQueues(latinizedTerm: string) {
    const workQueueSearchResults = this.workQueueIndex
      .filter(container => {
        return container.latinizedLowerLabel.includes(latinizedTerm);
      })
      .map((container: any) => {
        const item = container.node;
        return {
          id: item.id,
          type: "",
          iconUrl: this.getIconUrl(IMenuItemIcon.WorkQueue),
          label: item.name,
          description: "",
          onClick: () => this.onWorkQueueItemClicked(item)
        };
      }) as ISearchResult[];

    this.workQueueResultGroup = new SearchResultGroup(T("Work Queue", "queue_results"), workQueueSearchResults);
  }

  onWorkQueueItemClicked(item: any){
    openSingleMenuFolder(item, this);
    const sidebarState = getWorkbench(this).sidebarState;
    onWorkQueuesListItemClick(this)(null, item);
    sidebarState.activeSection = "WorkQueues";
    getMainMenuState(this).scrollToItem(item.id);
    sidebarState.onSearchResultsChange(this.resultGroups);
  }

  private searchInMenu(latinizedTerm: string) {
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
              iconUrl: this.getIconUrl(node.attributes.icon),
              label: node.attributes.label,
              description: getPath(node),
              onClick: () => this.onSubMenuClicked(node)
            };
          case "Command":
            return {
              id: node.attributes.id,
              type: "Command",
              iconUrl: this.getIconUrl(node.attributes.icon),
              label: node.attributes.label,
              description: getPath(node),
              onClick: () => this.onCommandClicked(node)
            };
        }
        return undefined;
      })
        .filter(result => result) as ISearchResult[];
    this.menuResultGroup = new SearchResultGroup(T("Menu", "menu"), searchResults);
  }

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


  @action.bound
  indexWorkQueues(items: any[]){
    this.workQueueIndex = items
      .map(item => new NodeContainer(prepareForFilter(this, item.name)!, item));
  }

  @action.bound
  indexChats(items: any[]){
    this.chatsIndex = items
      .map(item => new NodeContainer(prepareForFilter(this, item.topic)!, item));
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
          this.nodeIndex.push(new NodeContainer(prepareForFilter(this, node.attributes.label)! ,node));
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
    this.workQueueResultGroup = undefined;
    this.chatResultGroup = undefined;
    this.searchTerm = "";
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