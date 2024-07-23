import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { AppThunk, RootState } from 'src/stores/store.ts';

export interface EditorProperty {
  name: string;
  type: "boolean" | "enum" | "string" | "looukup";
  value: any;
  dropDownValues: DropDownValue[];
  category: string;
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
    initializeEditor: (state, action: PayloadAction<{
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

export const selectActiveEditorState = (state: RootState) => {
  for (const editorState of Object.values(state.editorStates.editors)) {
    if (editorState.isActive) {
      return editorState;
    }
  }
  return null;
}

export const {
  initializeEditor,
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

export default editorSlice.reducer;