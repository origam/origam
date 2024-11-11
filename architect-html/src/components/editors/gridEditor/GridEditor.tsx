import { useContext, useEffect } from "react";
import S from 'src/components/editors/gridEditor/GridEditor.module.scss';
import { PropertyEditor } from "src/components/editors/propertyEditor/PropertyEditor.tsx";
import {
  EditorState
} from "src/components/editors/gridEditor/GridEditorState.ts";
import { observer } from "mobx-react-lite";
import {
  runGeneratorInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";
import { RootStoreContext } from "src/main.tsx";

export const GridEditor: React.FC<{
  title: string,
  editorState: EditorState
}> = observer( (props) => {
   const rootStore = useContext(RootStoreContext);

   useEffect(() => {
    runGeneratorInFlowWithHandler({
      controller: rootStore.errorDialogController,
      generator: props.editorState.initialize.bind(props.editorState),
    });
  }, [props.editorState]);

  return (
    <div className={S.gridEditor}>
      <h3 className={S.title}>{`Editing: ${props.title}`}</h3>
      <PropertyEditor
        editorState={props.editorState}
        properties={props.editorState.properties}/>
    </div>
  );
});