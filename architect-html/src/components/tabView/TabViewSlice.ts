import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { TabViewId } from './TabView';

interface TabState {
  activeTab: TabViewId | undefined;
}

const initialState: TabState = {
  activeTab: undefined,
};

const tabSlice = createSlice({
  name: 'tab',
  initialState,
  reducers: {
    setActiveTab: (state, action: PayloadAction<TabViewId>) => {
      state.activeTab = action.payload;
    },
  },
});

export const { setActiveTab } = tabSlice.actions;
export const tabReducer = tabSlice.reducer;