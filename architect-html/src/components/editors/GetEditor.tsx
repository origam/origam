import { GridEditor } from "src/components/editors/gridEditor/GridEditor.tsx";
import { XsltEditor } from "src/components/editors/xsltEditor/XsltEditor.tsx";
import {
  EditorProperty,
  GridEditorState
} from "src/components/editors/gridEditor/GridEditorState.ts";
import {
  IApiEditorProperty,
  IArchitectApi,
  ISectionEditorData
} from "src/API/IArchitectApi.ts";
import { IEditorState } from "src/components/editorTabView/IEditorState.ts";
import { EditorData } from "src/components/modelTree/EditorData.ts";
import ComponentDesigner
  from "src/components/editors/screenSectionEditor/ComponentDesigner.tsx";
import {
  ComponentDesignerState
} from "src/components/editors/screenSectionEditor/ComponentDesignerState.tsx";

export function getEditor(
  args: {
    editorData: EditorData
    architectApi: IArchitectApi
  }
) {
  const {editorData, architectApi } = args;
  const {node, data, isPersisted} = editorData;
  if (node.editorType === "GridEditor") {
    const properties = (data as IApiEditorProperty[]).map(property => new EditorProperty(property));
    const editorState = new GridEditorState(node, properties, isPersisted, architectApi);
    return new Editor(
      editorState,
      <GridEditor editorState={editorState}/>
    );
  }
  if (node.editorType === "XslTEditor") {
    const properties = (data as IApiEditorProperty[]).map(property => new EditorProperty(property));
    const editorState = new GridEditorState(node, properties, isPersisted, architectApi);
    return new Editor(
      editorState,
      <XsltEditor editorState={editorState}/>
    );
  }
  if (node.editorType === "ScreenSectionEditor") {
    const sectionData = data as ISectionEditorData;
    const componentDesignerState = new ComponentDesignerState(node, isPersisted, sectionData, architectApi);
    return new Editor(
      componentDesignerState,
      <ComponentDesigner designerState={componentDesignerState}/>
    );
    return null;
  }
  return null;
}


export class Editor {
  constructor(public state: IEditorState, public element: React.ReactElement) {
  }
}