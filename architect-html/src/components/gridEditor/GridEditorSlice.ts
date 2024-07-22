import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { EditorProperty } from "src/components/gridEditor/GridEditor.tsx";

export interface EditorState {
  id: string;
  properties: EditorProperty[];
}

export interface EditorSliceState {
  editors: EditorState[];
}

const editorSlice = createSlice({
  name: 'editor',
  initialState: {
    editors: [] as EditorState[]
  } as EditorSliceState,
  reducers: {
    add: (state,action: PayloadAction<{ id: string; properties: EditorProperty[] }>) => {
      state.editors.push({ id: action.payload.id, properties: action.payload.properties });
    },
    updatePropertyValue: (state, action: PayloadAction<{ id: string; propertyName: string, value: any }>) => {
      const editor = state.editors.find(e => e.id === action.payload.id);
      if (editor) {
        const property = editor.properties.find(p => p.name === action.payload.propertyName);
        if (property) {
          property.value = action.payload.value;
        }
      }
    }
  }
});

// Ensure the inferred type matches your explicit type
type EditorSlice = typeof editorSlice;
// @ts-ignore
const typedEditorSlice: EditorSlice = editorSlice;

export const selectEditorById = (state: { editor: EditorSliceState }, id: string): EditorState | undefined =>
  state.editor.editors.find(editor => editor.id === id);


export const { add, updatePropertyValue } = editorSlice.actions;
export default editorSlice.reducer;