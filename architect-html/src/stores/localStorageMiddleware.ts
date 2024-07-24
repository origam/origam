import { Middleware } from 'redux';
import type { RootState } from './store';

const LOCAL_STORAGE_KEY = 'treeExpandedNodes';

export const localStorageMiddleware: Middleware<
  {}, // eslint-disable-line @typescript-eslint/ban-types
  RootState
> = (storeAPI) => (next) => (action: any) => {
  const result = next(action);
  if ('type' in action && typeof action.type === 'string' && action.type.startsWith('tree/')) {
    const treeState = storeAPI.getState().tree;
    localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(treeState.expandedNodes));
  }
  return result;
};

export const loadStateFromLocalStorage = (): string[] => {
  try {
    const serializedState = localStorage.getItem(LOCAL_STORAGE_KEY);
    if (serializedState === null) {
      return [];
    }
    return JSON.parse(serializedState);
  } catch (err) {
    console.error('Error loading state from local storage:', err);
    return [];
  }
};