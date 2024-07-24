import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { RootState } from 'src/stores/store.ts'; // Assuming you have a store.ts file

interface TreeState {
  expandedNodes: string[];
}

const initialState: TreeState = {
  expandedNodes: [],
};

export const treeSlice = createSlice({
  name: 'tree',
  initialState,
  reducers: {
    toggleNode: (state, action: PayloadAction<string>) => {
      const nodeId = action.payload;
      const index = state.expandedNodes.indexOf(nodeId);
      if (index > -1) {
        state.expandedNodes.splice(index, 1);
      } else {
        state.expandedNodes.push(nodeId);
      }
    },
    expandNodes: (state, action: PayloadAction<string[]>) => {
      const newNodes = action.payload.filter(
        nodeId => !state.expandedNodes.includes(nodeId)
      );
      state.expandedNodes.push(...newNodes);
    },
    setExpandedNodes: (state, action: PayloadAction<string[]>) => {
      state.expandedNodes = action.payload;
    },
  },
});

export const { toggleNode, expandNodes, setExpandedNodes } = treeSlice.actions;

// Selector to get all currently expanded tree nodes
export const selectExpandedNodes = (state: RootState) => state.tree.expandedNodes;

export default treeSlice.reducer;