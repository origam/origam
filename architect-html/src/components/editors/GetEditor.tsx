import { GridEditor } from "src/components/editors/gridEditor/GridEditor.tsx";
import { XsltEditor } from "src/components/editors/xsltEditor/XsltEditor.tsx";
import {
  EditorProperty,
  EditorState
} from "src/components/editors/gridEditor/GridEditorState.ts";
import { IArchitectApi } from "src/API/IArchitectApi.ts";
import {
  IEditorNode
} from "src/components/editorTabView/EditorTabViewState.ts";

export function getEditor(
  editorNode: IEditorNode,
  properties: EditorProperty[] | undefined,
  architectApi: IArchitectApi
) {
  const editorState = new EditorState(editorNode, properties, architectApi);
  if (editorNode.editorType === "GridEditor") {
    return new Editor(
      editorState,
      <GridEditor
        title={editorNode.nodeText}
        editorState={editorState}/>
    );
  }
  if (editorNode.editorType === "XslTEditor") {
    return new Editor(
      editorState,
      <XsltEditor
        editorState={editorState}/>
    );
  }
  return null;
}

export class Editor {
  constructor(public state: EditorState, public element: React.ReactElement) {
  }
}