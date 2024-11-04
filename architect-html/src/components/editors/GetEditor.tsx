import { TreeNode } from "src/stores/TreeNode.ts";
import { GridEditor } from "src/components/editors/gridEditor/GridEditor.tsx";
import { XsltEditor } from "src/components/editors/xsltEditor/XsltEditor.tsx";
import { ProjectState } from "src/stores/RootStore.ts";
import { useMemo } from "react";
import {
  EditorState
} from "src/components/editors/gridEditor/GridEditorState.ts";
import { ArchitectApi } from "src/API/ArchitectApi.ts";

export function getEditor(node: TreeNode, architectApi: ArchitectApi) {
  const editorState = new EditorState(node.id, node.origamId, architectApi);
  if (node.editorType === "GridEditor") {
    return new Editor(
      editorState,
      <GridEditor
        node={node}
        editorState={editorState}/>
    );
  }
  if (node.editorType === "XslTEditor") {
    return new Editor(
      editorState,
      <XsltEditor
        node={node}
        editorState={editorState}/>
    );
  }
}

export class Editor {
  constructor(public state: EditorState, public element: React.ReactElement) {
  }
}