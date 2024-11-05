import { TabViewState } from "src/components/tabView/TabViewState.ts";
import { observable } from "mobx";

export class UiStore {
  public sideBarTabViewState = new TabViewState();
  public treeViewUiState = new TreeViewUiState();

  showModelTree() {
    this.sideBarTabViewState.activeTabIndex = 1;
  }
}

export class TreeViewUiState {

  storageKey = 'treeExpandedNodes';

  @observable accessor expandedNodes: string[] = [];

  constructor() {
    this.expandedNodes = this.loadStateFromLocalStorage();
  }

  setExpanded(nodeId: string, expanded: boolean) {
    if (expanded) {
      if (!this.expandedNodes.includes(nodeId)) {
        this.expandedNodes = [...this.expandedNodes, nodeId];
      }
    } else {
      this.expandedNodes = this.expandedNodes.filter(x => x !== nodeId);
    }
    localStorage.setItem(this.storageKey, JSON.stringify(this.expandedNodes));
  }

  isExpanded(nodeId: string) {
    return this.expandedNodes.includes(nodeId);
  }

  loadStateFromLocalStorage() {
    try {
      const serializedState = localStorage.getItem(this.storageKey) ?? "";
      return JSON.parse(serializedState) ?? [];
    } catch (err) {
      console.error('Error loading state from local storage:', err);
      return [];
    }
  }
}

