import { RootStoreContext } from "src/main.tsx";
import { useContext } from "react";
import { flow } from "mobx";
import { observer } from "mobx-react-lite";

export const SaveButton = observer( () => {
  const projectState = useContext(RootStoreContext).projectState;
  const activeEditor = projectState.activeEditorState;
  if (!activeEditor) {
    return null;
  }
  const handleSave = () => {
    flow(activeEditor.save.bind(activeEditor))();
  };

  return (
    <button
      onClick={handleSave}
      disabled={!activeEditor.isDirty || activeEditor.isSaving}
      style={{ backgroundColor: activeEditor.isDirty ? 'red' : 'initial' }}
    >
      {activeEditor.isSaving ? 'Saving...' : 'Save'}
    </button>
  );
});