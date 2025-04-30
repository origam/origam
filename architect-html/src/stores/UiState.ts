import { observable } from "mobx";

export class UiState {

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

  clear() {
    this.expandedNodes = [];
    localStorage.setItem(this.storageKey, JSON.stringify(this.expandedNodes));
  }
}