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