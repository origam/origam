import { GridEditor } from "src/components/editors/gridEditor/GridEditor.tsx";
import { XsltEditor } from "src/components/editors/xsltEditor/XsltEditor.tsx";
import {
  EditorProperty,
  GridEditorState
} from "src/components/editors/gridEditor/GridEditorState.ts";
import { IArchitectApi } from "src/API/IArchitectApi.ts";
import {
  IEditorNode
} from "src/components/editorTabView/EditorTabViewState.ts";
import { IEditorState } from "src/components/editorTabView/IEditorState.ts";

export function getEditor(
  args: {
    editorNode: IEditorNode,
    properties: EditorProperty[] | undefined,
    isPersisted: boolean,
    architectApi: IArchitectApi
  }
) {

  const { editorNode, properties, isPersisted, architectApi } = args;
  if (editorNode.editorType === "GridEditor") {
    const editorState = new GridEditorState(editorNode, properties, isPersisted, architectApi);
    return new Editor(
      editorState,
      <GridEditor editorState={editorState}/>
    );
  }
  if (editorNode.editorType === "XslTEditor") {
    const editorState = new GridEditorState(editorNode, properties, isPersisted, architectApi);
    return new Editor(
      editorState,
      <XsltEditor
        editorState={editorState}/>
    );
  }
  if (editorNode.editorType === "ScreenSectionEditor") {
    return new Editor(
      new GridEditorState(editorNode, properties, isPersisted, architectApi),
      <GridEditor editorState={new GridEditorState(editorNode, properties, isPersisted, architectApi)}/>
    );
  }
  return null;
}


export class Editor {
  constructor(public state: IEditorState, public element: React.ReactElement) {
  }
}