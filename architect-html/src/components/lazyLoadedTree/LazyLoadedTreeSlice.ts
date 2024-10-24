import { createSelector, createSlice, PayloadAction } from '@reduxjs/toolkit';
import { RootState } from 'src/stores/store.ts';

export interface TreeNode {
  id: string;
  origamId: string;
  nodeText: string;
  hasChildNodes: boolean;
  isNonPersistentItem: boolean;
  editorType: null | "GridEditor";
  children?: TreeNode[];
  childrenIds: string[];
  parentId: string | null;
}

interface TreeState {
  expandedNodes: string[];
  nodes: {[id:string] : TreeNode}
}

const initialState: TreeState = {
  expandedNodes: [],
  nodes: {}
};

export const treeSlice = createSlice({
  name: 'tree',
  initialState,
  reducers: {
    addTopNodes: (state, action: PayloadAction<TreeNode[]>) => {
      for (const node of action.payload) {
        node.childrenIds = [];
        node.parentId = null;
        for (const childNode of node.children) {
          node.childrenIds.push(childNode.id);
          childNode.childrenIds = [];
          childNode.parentId = node.id;
          state.nodes[childNode.id] = childNode;
        }
        state.nodes[node.id] = node;
      }
    },
    setChildNodes: (state, action: PayloadAction<{nodeId: string, children: TreeNode[]}>) => {
       const {nodeId, children} = action.payload;
       const childNodeIds = []
       for (const childNode of children) {
         childNode.childrenIds = [];
         state.nodes[childNode.id] = childNode;
         childNodeIds.push(childNode.id)
      }
      state.nodes[nodeId].childrenIds = childNodeIds;
    },
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

export const { toggleNode, addTopNodes, setChildNodes, expandNodes, setExpandedNodes } = treeSlice.actions;

export const selectExpandedNodes = (state: RootState) => state.tree.expandedNodes;

export const selectTopNodesInternal = (state: RootState) => state.tree.nodes;

export const selectTopNodes = createSelector(
  [selectTopNodesInternal],
  (nodes) => Object.values(nodes).filter(x => x.parentId === null)
);

export const makeSelectChildNodes = () => {
  return createSelector(
    [
      selectTopNodesInternal,
      (_state: RootState, nodeId: string) => nodeId
    ],
    (nodes, nodeId) => {
      return nodes[nodeId].childrenIds.map(childId => nodes[childId]);
    }
  );
};

export default treeSlice.reducer;