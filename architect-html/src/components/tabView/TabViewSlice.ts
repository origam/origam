import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { TabViewId } from './TabView';

import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { TabViewId } from './TabView';

export interface TabState {
  [instanceId: string]: TabViewId | undefined;
}

const initialState: TabState = {};

const tabSlice = createSlice({
  name: 'tab',
  initialState,
  reducers: {
    setActiveTab: (state, action: PayloadAction<{ instanceId: string; tabId: TabViewId }>) => {
      const { instanceId, tabId } = action.payload;
      state[instanceId] = tabId;
    },
    resetTabState: (state, action: PayloadAction<string>) => {
      delete state[action.payload];
    },
  },
});

export const { setActiveTab, resetTabState } = tabSlice.actions;
export const tabReducer = tabSlice.reducer;
export const selectTabState = (state: { tab: TabState }, instanceId: string): TabViewId | undefined =>
  state.tab[instanceId];