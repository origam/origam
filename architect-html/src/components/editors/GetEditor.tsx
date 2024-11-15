import { GridEditor } from "src/components/editors/gridEditor/GridEditor.tsx";
import { XsltEditor } from "src/components/editors/xsltEditor/XsltEditor.tsx";
import {
  EditorProperty,
  GridEditorState
} from "src/components/editors/gridEditor/GridEditorState.ts";
import { IArchitectApi } from "src/API/IArchitectApi.ts";
import { IEditorState } from "src/components/editorTabView/IEditorState.ts";
import { EditorData } from "src/components/modelTree/EditorData.ts";

export function getEditor(
  args: {
    editorData: EditorData
    architectApi: IArchitectApi
  }
) {
  const {editorData, architectApi } = args;
  const {node, data, isPersisted} = editorData;
  if (node.editorType === "GridEditor") {
    const properties = data.map(property => new EditorProperty(property));
    const editorState = new GridEditorState(node, properties, isPersisted, architectApi);
    return new Editor(
      editorState,
      <GridEditor editorState={editorState}/>
    );
  }
  if (node.editorType === "XslTEditor") {
    const properties = data.map(property => new EditorProperty(property));
    const editorState = new GridEditorState(node, properties, isPersisted, architectApi);
    return new Editor(
      editorState,
      <XsltEditor
        editorState={editorState}/>
    );
  }
  if (node.editorType === "ScreenSectionEditor") {
    // return new Editor(
    //   new GridEditorState(node, data, isPersisted, architectApi),
    //   <GridEditor editorState={new GridEditorState(node, data, isPersisted, architectApi)}/>
    // );
    return null;
  }
  return null;
}


export class Editor {
  constructor(public state: IEditorState, public element: React.ReactElement) {
  }
}