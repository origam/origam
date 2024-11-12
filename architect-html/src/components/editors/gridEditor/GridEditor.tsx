import { useContext, useEffect } from "react";
import S from 'src/components/editors/gridEditor/GridEditor.module.scss';
import { PropertyEditor } from "src/components/editors/propertyEditor/PropertyEditor.tsx";
import {
  EditorState
} from "src/components/editors/gridEditor/GridEditorState.ts";
import { observer } from "mobx-react-lite";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";
import { RootStoreContext } from "src/main.tsx";

export const GridEditor: React.FC<{
  editorState: EditorState
}> = observer( (props) => {
   const rootStore = useContext(RootStoreContext);

   useEffect(() => {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: props.editorState.initialize.bind(props.editorState),
    });
  }, [props.editorState]);

  return (
    <div className={S.gridEditor}>
      <h3 className={S.title}>{`Editing: ${props.editorState.label}`}</h3>
      <PropertyEditor
        editorState={props.editorState}
        properties={props.editorState.properties}/>
    </div>
  );
});