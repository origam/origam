import { RootStoreContext } from "src/main.tsx";
import { useContext } from "react";
import { observer } from "mobx-react-lite";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";

export const SaveButton = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const editorTabViewState = rootStore.editorTabViewState;
  const activeEditor = editorTabViewState.activeEditorState;
  if (!activeEditor) {
    return null;
  }
  const handleSave = () => {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: activeEditor.save.bind(activeEditor),
    });
  };

  return (
    <button
      onClick={handleSave}
      disabled={!activeEditor.isDirty || activeEditor.isSaving}
      style={{backgroundColor: activeEditor.isDirty ? 'red' : 'initial'}}
    >
      {activeEditor.isSaving ? 'Saving...' : 'Save'}
    </button>
  );
});