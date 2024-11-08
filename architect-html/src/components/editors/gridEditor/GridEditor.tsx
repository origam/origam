import { useEffect } from "react";
import S from 'src/components/editors/gridEditor/GridEditor.module.scss';
import { PropertyEditor } from "src/components/editors/propertyEditor/PropertyEditor.tsx";
import {
  EditorState
} from "src/components/editors/gridEditor/GridEditorState.ts";
import { flow } from "mobx";
import { observer } from "mobx-react-lite";

export const GridEditor: React.FC<{
  title: string,
  editorState: EditorState
}> = observer( (props) => {
   useEffect(() => {
    flow(props.editorState.initialize.bind(props.editorState))();
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