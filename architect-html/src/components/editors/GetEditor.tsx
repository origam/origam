import { GridEditor } from "src/components/editors/gridEditor/GridEditor.tsx";
import { XsltEditor } from "src/components/editors/xsltEditor/XsltEditor.tsx";
import {
  GridEditorState
} from "src/components/editors/gridEditor/GridEditorState.ts";
import {
  IApiEditorProperty,
  IArchitectApi,
  ISectionEditorData
} from "src/API/IArchitectApi.ts";
import { IEditorState } from "src/components/editorTabView/IEditorState.ts";
import { EditorData } from "src/components/modelTree/EditorData.ts";
import ScreenSectionEditor
  from "src/components/editors/screenSectionEditor/ScreenSectionEditor.tsx";
import {
  ScreenSectionEditorState
} from "src/components/editors/screenSectionEditor/ScreenSectionEditorState.tsx";
import {
  EditorProperty
} from "src/components/editors/gridEditor/EditorProperty.ts";
import { PropertiesState } from "src/components/properties/PropertiesState.ts";

export function getEditor(
  args: {
    editorData: EditorData,
    propertiesState: PropertiesState
    architectApi: IArchitectApi
  }
) {
  const {editorData, propertiesState, architectApi } = args;
  const {node, data, isDirty} = editorData;
  if (node.editorType === "GridEditor") {
    const properties = (data as IApiEditorProperty[]).map(property => new EditorProperty(property));
    const editorState = new GridEditorState(node, properties, isDirty, architectApi);
    return new Editor(
      editorState,
      <GridEditor editorState={editorState}/>
    );
  }
  if (node.editorType === "XslTEditor") {
    const properties = (data as IApiEditorProperty[]).map(property => new EditorProperty(property));
    const editorState = new GridEditorState(node, properties, isDirty, architectApi);
    return new Editor(
      editorState,
      <XsltEditor editorState={editorState}/>
    );
  }
  if (node.editorType === "ScreenSectionEditor") {
    const sectionData = data as ISectionEditorData;
    const componentDesignerState = new ScreenSectionEditorState(node, isDirty, sectionData, propertiesState, architectApi);
    return new Editor(
      componentDesignerState,
      <ScreenSectionEditor designerState={componentDesignerState}/>
    );
  }
  return null;
}


export class Editor {
  constructor(public state: IEditorState, public element: React.ReactElement) {
  }
}