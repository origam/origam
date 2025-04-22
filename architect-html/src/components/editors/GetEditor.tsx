import { GridEditor } from "src/components/editors/gridEditor/GridEditor.tsx";
import { XsltEditor } from "src/components/editors/xsltEditor/XsltEditor.tsx";
import {
  GridEditorState
} from "src/components/editors/gridEditor/GridEditorState.ts";
import {
  IApiEditorProperty,
  IArchitectApi, IScreenEditorData,
  ISectionEditorData
} from "src/API/IArchitectApi.ts";
import { IEditorState } from "src/components/editorTabView/IEditorState.ts";
import { EditorData } from "src/components/modelTree/EditorData.ts";
import ScreenSectionEditor
  from "src/components/editors/designerEditor/screenSectionEditor/ScreenSectionEditor.tsx";
import {
  ScreenSectionEditorState
} from "src/components/editors/designerEditor/screenSectionEditor/ScreenSectionEditorState.tsx";
import {
  EditorProperty
} from "src/components/editors/gridEditor/EditorProperty.ts";
import { PropertiesState } from "src/components/properties/PropertiesState.ts";
import {
  ScreenEditorState
} from "src/components/editors/designerEditor/screenEditor/ScreenEditorState.tsx";
import ScreenEditor from "src/components/editors/designerEditor/screenEditor/ScreenEditor.tsx";
import {
  ScreenToolboxState
} from "src/components/editors/designerEditor/screenEditor/ScreenToolboxState.tsx";
import {
  SectionToolboxState
} from "src/components/editors/designerEditor/screenSectionEditor/SectionToolboxState.tsx";
import { FlowHandlerInput } from "src/errorHandling/runInFlowWithHandler.ts";
import { CancellablePromise } from "mobx/dist/api/flow";

export function getEditor(
  args: {
    editorData: EditorData,
    propertiesState: PropertiesState
    architectApi: IArchitectApi,
    runGeneratorHandled: (args: FlowHandlerInput) => CancellablePromise<any>;
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
    const sectionToolboxState = new SectionToolboxState(sectionData, node.origamId, architectApi);
    const state = new ScreenSectionEditorState(node, isDirty, sectionData,
      propertiesState, sectionToolboxState, architectApi, args.runGeneratorHandled);
    return new Editor(
      state,
      <ScreenSectionEditor designerState={state}/>
    );
  }
  if (node.editorType === "ScreenEditor") {
    const screenData = data as IScreenEditorData;
    const screenToolboxState = new ScreenToolboxState(screenData, node.origamId, architectApi);
    const state = new ScreenEditorState(
      node, isDirty, screenData, propertiesState,
      screenToolboxState, architectApi, args.runGeneratorHandled);
    return new Editor(
      state,
      <ScreenEditor designerState={state}/>
    );
  }
  return null;
}


export class Editor {
  constructor(public state: IEditorState, public element: React.ReactElement) {
  }
}