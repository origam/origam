import S from 'src/components/editors/gridEditor/GridEditor.module.scss';
import { PropertyEditor } from "src/components/editors/propertyEditor/PropertyEditor.tsx";
import {
  GridEditorState
} from "src/components/editors/gridEditor/GridEditorState.ts";
import { observer } from "mobx-react-lite";

export const GridEditor: React.FC<{
  editorState: GridEditorState
}> = observer( (props) => {
  return (
    <div className={S.gridEditor}>
      <h3 className={S.title}>{`Editing: ${props.editorState.label}`}</h3>
      <PropertyEditor
        editorState={props.editorState}
        properties={props.editorState.properties}/>
    </div>
  );
});