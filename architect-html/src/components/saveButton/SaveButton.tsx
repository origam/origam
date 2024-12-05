import { RootStoreContext } from "src/main.tsx";
import { useContext } from "react";
import { observer } from "mobx-react-lite";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";

export const SaveButton = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const progressBarState = rootStore.progressBarState;
  const editorTabViewState = rootStore.editorTabViewState;
  const activeEditor = editorTabViewState.activeEditorState;
  if (!activeEditor) {
    return null;
  }
  const handleSave = () => {
    runInFlowWithHandler(rootStore.errorDialogController)({
        generator: function* () {
          progressBarState.isWorking = true;
          try {
            yield* activeEditor.save();
          }
          finally {
            progressBarState.isWorking = false;
          }
        }
    });
  };

  return (
    <button
      onClick={handleSave}
      disabled={!activeEditor.isDirty }
      style={{backgroundColor: activeEditor.isDirty ? 'red' : 'initial'}}
    >
      {'Save'}
    </button>
  );
});