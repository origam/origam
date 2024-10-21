import { useDispatch, useSelector } from 'react-redux';
import {
  EditorState,
  saveEditorContent,
  selectActiveEditorState
} from 'src/components/editors/gridEditor/GrirEditorSlice.ts';

export const SaveButton = () => {
  const dispatch = useDispatch();
  const activeEditor = useSelector<any, EditorState | null>(selectActiveEditorState);

  if (!activeEditor) return null;

  const { isDirty, isSaving } = activeEditor;

  const handleSave = () => {
    dispatch(saveEditorContent(activeEditor.id) as any);
  };

  return (
    <button
      onClick={handleSave}
      disabled={!isDirty || isSaving}
      style={{ backgroundColor: isDirty ? 'red' : 'initial' }}
    >
      {isSaving ? 'Saving...' : 'Save'}
    </button>
  );
};