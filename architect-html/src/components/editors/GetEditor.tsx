import { GridEditor } from "src/components/editors/gridEditor/GridEditor.tsx";
import { XsltEditor } from "src/components/editors/xsltEditor/XsltEditor.tsx";
import {
  GridEditorState
} from "src/components/editors/gridEditor/GridEditorState.ts";
import { IArchitectApi } from "src/API/IArchitectApi.ts";
import { IEditorState } from "src/components/editorTabView/IEditorState.ts";
import { NewEditorData } from "src/components/modelTree/NewEditorNode.ts";

export function getEditor(
  args: {
    editorData: NewEditorData
    architectApi: IArchitectApi
  }
) {
  const {editorData, architectApi } = args;
  const {node, properties, isPersisted} = editorData;
  if (node.editorType === "GridEditor") {
    const editorState = new GridEditorState(node, properties, isPersisted, architectApi);
    return new Editor(
      editorState,
      <GridEditor editorState={editorState}/>
    );
  }
  if (node.editorType === "XslTEditor") {
    const editorState = new GridEditorState(node, properties, isPersisted, architectApi);
    return new Editor(
      editorState,
      <XsltEditor
        editorState={editorState}/>
    );
  }
  if (node.editorType === "ScreenSectionEditor") {
    return new Editor(
      new GridEditorState(node, properties, isPersisted, architectApi),
      <GridEditor editorState={new GridEditorState(node, properties, isPersisted, architectApi)}/>
    );
  }
  return null;
}


export class Editor {
  constructor(public state: IEditorState, public element: React.ReactElement) {
  }
}