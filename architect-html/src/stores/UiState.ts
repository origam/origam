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

import { DEFAULT_RESULT_FILTER } from '@editors/DeploymentScriptsGeneratorEditor/DeploymentScriptsGeneratorEditorState';
import { action, observable } from 'mobx';

enum EStorageKeys {
  TREE_EXPANDED_NODES = 'treeExpandedNodes',
  SETTINGS = 'settings',
  DEPLOYMENT_SCRIPTS_GENERATOR_STATE = 'deploymentScriptsGeneratorState',
}

type TSettings = {
  isVimEnabled: boolean;
};

const defaultSettings: TSettings = {
  isVimEnabled: false,
};

export interface IDsGeneratorPersistedState {
  isOpen: boolean;
  selectedPlatform: string | null;
  resultFilter: string;
  selectedDeploymentVersionId: string | null;
}

const defaultDsGeneratorState: IDsGeneratorPersistedState = {
  isOpen: false,
  selectedPlatform: null,
  resultFilter: DEFAULT_RESULT_FILTER,
  selectedDeploymentVersionId: null,
};

const STORAGE_DEFAULTS = {
  [EStorageKeys.TREE_EXPANDED_NODES]: [] as string[],
  [EStorageKeys.SETTINGS]: defaultSettings,
  [EStorageKeys.DEPLOYMENT_SCRIPTS_GENERATOR_STATE]: defaultDsGeneratorState,
} as const;

export class UIState {
  @observable accessor expandedNodes: string[] = [];
  @observable accessor settings: TSettings = { ...defaultSettings };
  @observable accessor dsGeneratorState: IDsGeneratorPersistedState = {
    ...defaultDsGeneratorState,
  };

  constructor() {
    this.expandedNodes = this.loadStateFromLocalStorage(EStorageKeys.TREE_EXPANDED_NODES);

    const loadedSettings = this.loadStateFromLocalStorage(EStorageKeys.SETTINGS);
    this.settings.isVimEnabled = loadedSettings.isVimEnabled;

    const loadedDsGeneratorState = this.loadStateFromLocalStorage(
      EStorageKeys.DEPLOYMENT_SCRIPTS_GENERATOR_STATE,
    );
    this.dsGeneratorState = { ...defaultDsGeneratorState, ...loadedDsGeneratorState };
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

  loadStateFromLocalStorage<K extends EStorageKeys>(key: K): (typeof STORAGE_DEFAULTS)[K] {
    const def = STORAGE_DEFAULTS[key];
    const fallback = () => structuredClone(def) as (typeof STORAGE_DEFAULTS)[K];
    try {
      const serializedState = localStorage.getItem(key);
      if (!serializedState) return fallback();

      const parsedState = JSON.parse(serializedState);

      if (Array.isArray(def)) {
        return (Array.isArray(parsedState) ? parsedState : fallback()) as (typeof STORAGE_DEFAULTS)[K];
      }
      return (typeof parsedState === 'object' && parsedState !== null
        ? parsedState
        : fallback()) as (typeof STORAGE_DEFAULTS)[K];
    } catch (err) {
      console.error('Error loading state from local storage:', err);
      return fallback();
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

  getDsGeneratorState(): IDsGeneratorPersistedState {
    return { ...this.dsGeneratorState };
  }

  @action
  setDsGeneratorState(partial: Partial<IDsGeneratorPersistedState>) {
    this.dsGeneratorState = { ...this.dsGeneratorState, ...partial };
    localStorage.setItem(
      EStorageKeys.DEPLOYMENT_SCRIPTS_GENERATOR_STATE,
      JSON.stringify(this.dsGeneratorState),
    );
  }
}
