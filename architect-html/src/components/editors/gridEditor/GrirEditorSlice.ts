import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { AppThunk, RootState } from 'src/stores/store.ts';
import { TreeNode } from "src/components/lazyLoadedTree/LazyLoadedTreeSlice.ts";

export interface EditorProperty {
  name: string;
  type: "boolean" | "enum" | "string" | "looukup";
  value: any;
  dropDownValues: DropDownValue[];
  category: string | null;
  description: string;
  readOnly: boolean;
}

export interface DropDownValue {
    name: string;
    value: any;
}

export interface EditorStates {
  editors: { [editorId: string]: EditorState };
}

export interface EditorState {
  id: string;
  schemaItemId: string;
  properties: EditorProperty[];
  isDirty: boolean;
  isSaving: boolean;
  isActive: boolean;
}

const initialState: EditorStates = {editors: {}};

const editorSlice = createSlice({
  name: 'editorStates',
  initialState,
  reducers: {
    initEditor: (state, action: PayloadAction<{
      editorId: string;
      schemaItemId: string;
      properties: EditorProperty[]
    }>) => {
      state.editors[action.payload.editorId] =
        {
          id: action.payload.editorId,
          schemaItemId: action.payload.schemaItemId,
          properties: action.payload.properties,
          isDirty: false,
          isSaving: false,
          isActive: true,
        };
    },
    updateProperty: (state, action: PayloadAction<{
      editorId: string;
      propertyName: string;
      value: any
    }>) => {
      const {editorId, propertyName, value} = action.payload;
      const editor = state.editors[editorId];
      if (editor) {
        const property = editor.properties.find(p => p.name === propertyName);
        if (property) {
          property.value = value;
          editor.isDirty = true;
        }
      }
    },
    setSaving: (state, action: PayloadAction<{
      editorId: string;
      isSaving: boolean
    }>) => {
      const {editorId, isSaving} = action.payload;
      const editorState = state.editors[editorId];
      if (editorState) {
        editorState.isSaving = isSaving;
      }
    },
    setDirty: (state, action: PayloadAction<{
      editorId: string;
      isDirty: boolean
    }>) => {
      const {editorId, isDirty} = action.payload;
      const editorState = state.editors[editorId];
      if (editorState) {
        editorState.isDirty = isDirty;
      }
    },
  },
});

export const selectActiveEditorState = (state: RootState): EditorState | null => {
  for (const editorState of Object.values(state.editorStates.editors)) {
    const state = editorState as EditorState;
    if (state.isActive) {
      return state;
    }
  }
  return null;
}

export const {
  initEditor,
  updateProperty,
  setSaving,
  setDirty
} = editorSlice.actions;

export const saveEditorContent = (editorId: string): AppThunk =>
  async (dispatch, getState, {architectApi}) => {
    const editor = getState().editorStates.editors[editorId];
    if (!editor) return;

    dispatch(setSaving({editorId, isSaving: true}));
    try {
      await architectApi.persistChanges(editor.schemaItemId, editor.properties);
      dispatch(setDirty({editorId, isDirty: false}));
    } catch (error) {
      console.error('Failed to save content', error);
    } finally {
      dispatch(setSaving({editorId, isSaving: false}));
    }
  };

export const initializeEditor = (node: TreeNode): AppThunk =>
  async (dispatch, getState, {architectApi}) => {
    const editorId = getEditorId(node)
    try {
      const newProperties = await architectApi.getProperties(node.origamId);
      dispatch(initEditor({
        editorId,
        schemaItemId: node.id,
        properties: newProperties
      }));
    } catch (error) {
      console.error("Error fetching properties:", error);
    }
  };


export function getEditorId(node: TreeNode): string {
  return node.nodeText + "_" + node.id;
}
export default editorSlice.reducer;