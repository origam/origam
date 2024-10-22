import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface TabState {
  [instanceId: string]: number;
}

const initialState: TabState = {};

const tabSlice = createSlice({
  name: 'tab',
  initialState,
  reducers: {
    setActiveTab: (state, action: PayloadAction<{ instanceId: string; index: number }>) => {
      const { instanceId, index } = action.payload;
      state[instanceId] = index;
    },
    resetTabState: (state, action: PayloadAction<string>) => {
      delete state[action.payload];
    },
  },
});

export const { setActiveTab, resetTabState } = tabSlice.actions;
export const tabReducer = tabSlice.reducer;
export const selectTabState = (state: { tab: TabState }, instanceId: string): number | undefined =>
  state.tab[instanceId];