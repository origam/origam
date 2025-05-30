/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o. 

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { GridEditor } from "src/components/editors/gridEditor/GridEditor.tsx";
import { XsltEditor } from "src/components/editors/xsltEditor/XsltEditor.tsx";
import {
  GridEditorState
} from "src/components/editors/gridEditor/GridEditorState.ts";
import {
  EditorType,
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
import ScreenEditor
  from "src/components/editors/designerEditor/screenEditor/ScreenEditor.tsx";
import {
  ScreenToolboxState
} from "src/components/editors/designerEditor/screenEditor/ScreenToolboxState.tsx";
import {
  SectionToolboxState
} from "src/components/editors/designerEditor/screenSectionEditor/SectionToolboxState.tsx";
import { FlowHandlerInput } from "src/errorHandling/runInFlowWithHandler.ts";
import { CancellablePromise } from "mobx/dist/api/flow";
import {
  DocumentationEditorState
} from "src/components/editors/documentationEditor/DocumentationEditorState.ts";

export function getEditor(
  args: {
    editorType: EditorType,
    editorData: EditorData,
    propertiesState: PropertiesState
    architectApi: IArchitectApi,
    runGeneratorHandled: (args: FlowHandlerInput) => CancellablePromise<any>;
  }
) {
  const {editorType, editorData, propertiesState, architectApi} = args;
  const {node, data, isDirty} = editorData;
  if (editorType === "GridEditor") {
    const properties = (data as IApiEditorProperty[]).map(property => new EditorProperty(property));
    const editorState = new GridEditorState(editorData.editorId, node, properties, isDirty, architectApi);
    return new Editor(
      editorState,
      <GridEditor editorState={editorState}/>
    );
  }
  if (editorType === "XslTEditor") {
    const properties = (data as IApiEditorProperty[]).map(property => new EditorProperty(property));
    const editorState = new GridEditorState(editorData.editorId, node, properties, isDirty, architectApi);
    return new Editor(
      editorState,
      <XsltEditor editorState={editorState}/>
    );
  }
  if (editorType === "ScreenSectionEditor") {
    const sectionData = data as ISectionEditorData;
    const sectionToolboxState = new SectionToolboxState(sectionData, node.origamId, architectApi);
    const state = new ScreenSectionEditorState(editorData.editorId, node, isDirty, sectionData,
      propertiesState, sectionToolboxState, architectApi, args.runGeneratorHandled);
    return new Editor(
      state,
      <ScreenSectionEditor designerState={state}/>
    );
  }
  if (editorType === "ScreenEditor") {
    const screenData = data as IScreenEditorData;
    const screenToolboxState = new ScreenToolboxState(screenData, node.origamId, architectApi);
    const state = new ScreenEditorState(
      editorData.editorId, node, isDirty, screenData, propertiesState,
      screenToolboxState, architectApi, args.runGeneratorHandled);
    return new Editor(
      state,
      <ScreenEditor designerState={state}/>
    );
  }
  if (editorType === "DocumentationEditor") {
    const properties = (data as IApiEditorProperty[]).map(property => new EditorProperty(property));
    const editorState = new DocumentationEditorState(editorData.editorId, node, properties, isDirty, architectApi);
    return new Editor(
      editorState,
      <GridEditor editorState={editorState}/>
    );
  }
  return null;
}


export class Editor {
  constructor(public state: IEditorState, public element: React.ReactElement) {
  }
}