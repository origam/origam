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

import { action, observable } from 'mobx';

enum EStorageKeys {
  TREE_EXPANDED_NODES = 'treeExpandedNodes',
  SETTINGS = 'settings',
}

type TSettings = {
  isVimEnabled: boolean;
};

const defaultSettings: TSettings = {
  isVimEnabled: false,
};

export class UIState {
  @observable accessor expandedNodes: string[] = [];
  @observable accessor settings: TSettings = { ...defaultSettings };

  constructor() {
    this.expandedNodes = this.loadStateFromLocalStorage(EStorageKeys.TREE_EXPANDED_NODES);

    const loadedSettings = this.loadStateFromLocalStorage(EStorageKeys.SETTINGS);
    this.settings.isVimEnabled = loadedSettings.isVimEnabled;
  }

  setExpanded(nodeId: string, expanded: boolean) {
    if (expanded) {
      if (!this.expandedNodes.includes(nodeId)) {
        this.expandedNodes = [...this.expandedNodes, nodeId];
      }
    } else {
      this.expandedNodes = this.expandedNodes.filter(x => x !== nodeId);
    }
    localStorage.setItem(EStorageKeys.TREE_EXPANDED_NODES, JSON.stringify(this.expandedNodes));
  }

  isExpanded(nodeId: string) {
    return this.expandedNodes.includes(nodeId);
  }

  loadStateFromLocalStorage(key: EStorageKeys) {
    try {
      if (key === EStorageKeys.TREE_EXPANDED_NODES) {
        const serializedState = localStorage.getItem(key) ?? '';
        return JSON.parse(serializedState) ?? [];
      }
      if (key === EStorageKeys.SETTINGS) {
        const serializedState = localStorage.getItem(key) ?? '';
        return JSON.parse(serializedState) ?? { ...defaultSettings };
      }
    } catch (err) {
      console.error('Error loading state from local storage:', err);
      if (key === EStorageKeys.SETTINGS) {
        return { ...defaultSettings };
      }
      return [];
    }
  }

  clearExpandedNodes() {
    this.expandedNodes = [];
    localStorage.setItem(EStorageKeys.TREE_EXPANDED_NODES, JSON.stringify(this.expandedNodes));
  }

  @action
  toggleVimEnabled() {
    this.settings.isVimEnabled = !this.settings.isVimEnabled;
    localStorage.setItem(EStorageKeys.SETTINGS, JSON.stringify(this.settings));
  }
}
