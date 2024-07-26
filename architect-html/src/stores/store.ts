import { configureStore, ThunkAction, Action, ConfigureStoreOptions } from '@reduxjs/toolkit';
import editorsReducer from 'src/components/editors/gridEditor/GrirEditorSlice';
import { IArchitectApi } from 'src/API/IArchitectApi';
import { ArchitectApi } from "src/API/ArchitectApi";
import treeReducer from 'src/components/lazyLoadedTree/LazyLoadedTreeSlice';
import {
  loadStateFromLocalStorage, localStorageMiddleware
} from "src/stores/localStorageMiddleware";
import { tabReducer } from "src/components/tabView/TabViewSlice.ts";

const architectApi = new ArchitectApi();

const preloadedState = {
  tree: {
    expandedNodes: loadStateFromLocalStorage(),
  },
};

const storeOptions: ConfigureStoreOptions = {
  reducer: {
    editorStates: editorsReducer,
    tree: treeReducer,
    tab: tabReducer,
  },
  preloadedState,
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      thunk: {
        extraArgument: { architectApi }
      }
    }).concat(localStorageMiddleware)
};

export const store = configureStore(storeOptions);

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

export type AppThunk<ReturnType = void> = ThunkAction<
  ReturnType,
  RootState,
  { architectApi: IArchitectApi },
  Action<string>
>;