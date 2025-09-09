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

import { T } from '@/main';
import {
  DocumentationEditorData,
  EditorType,
  IApiEditorProperty,
  IArchitectApi,
  IScreenEditorData,
  ISectionEditorData,
} from '@api/IArchitectApi';
import { IEditorState } from '@components/editorTabView/IEditorState';
import { EditorData } from '@components/modelTree/EditorData';
import { PropertiesState } from '@components/properties/PropertiesState';
import ScreenEditor from '@editors/designerEditor/screenEditor/ScreenEditor';
import { ScreenEditorState } from '@editors/designerEditor/screenEditor/ScreenEditorState';
import { ScreenToolboxState } from '@editors/designerEditor/screenEditor/ScreenToolboxState';
import ScreenSectionEditor from '@editors/designerEditor/screenSectionEditor/ScreenSectionEditor';
import { ScreenSectionEditorState } from '@editors/designerEditor/screenSectionEditor/ScreenSectionEditorState';
import { SectionToolboxState } from '@editors/designerEditor/screenSectionEditor/SectionToolboxState';
import { DocumentationEditorState } from '@editors/documentationEditor/DocumentationEditorState';
import { EditorProperty } from '@editors/gridEditor/EditorProperty';
import GridEditor from '@editors/gridEditor/GridEditor';
import { GridEditorState } from '@editors/gridEditor/GridEditorState';
import XsltEditor from '@editors/xsltEditor/XsltEditor';
import { FlowHandlerInput } from '@errors/runInFlowWithHandler';
import { CancellablePromise } from 'mobx/dist/api/flow';
import React from 'react';

export function getEditor(args: {
  editorType: EditorType;
  editorData: EditorData;
  propertiesState: PropertiesState;
  architectApi: IArchitectApi;
  runGeneratorHandled: (args: FlowHandlerInput) => CancellablePromise<any>;
}) {
  const { editorType, editorData, propertiesState, architectApi } = args;
  const { node, data, isDirty } = editorData;

  if (editorType === 'GridEditor') {
    const properties = (data as IApiEditorProperty[]).map(property => new EditorProperty(property));
    const editorState = new GridEditorState(
      editorData.editorId,
      node,
      properties,
      isDirty,
      architectApi,
    );
    return new Editor(
      editorState,
      (
        <GridEditor
          editorState={editorState}
          title={T('Editing: {0}', 'grid_editor_title', editorState.label)}
        />
      ),
    );
  }

  if (editorType === 'XsltEditor') {
    const properties = (data as IApiEditorProperty[]).map(property => new EditorProperty(property));
    const editorState = new GridEditorState(
      editorData.editorId,
      node,
      properties,
      isDirty,
      architectApi,
    );
    return new Editor(editorState, <XsltEditor editorState={editorState} />);
  }

  if (editorType === 'ScreenSectionEditor') {
    const sectionData = data as ISectionEditorData;
    const sectionToolboxState = new SectionToolboxState(sectionData, node.origamId, architectApi);
    const state = new ScreenSectionEditorState(
      editorData.editorId,
      node,
      isDirty,
      sectionData,
      propertiesState,
      sectionToolboxState,
      architectApi,
      args.runGeneratorHandled,
    );
    return new Editor(state, <ScreenSectionEditor designerState={state} />);
  }

  if (editorType === 'ScreenEditor') {
    const screenData = data as IScreenEditorData;
    const screenToolboxState = new ScreenToolboxState(screenData, node.origamId, architectApi);
    const state = new ScreenEditorState(
      editorData.editorId,
      node,
      isDirty,
      screenData,
      propertiesState,
      screenToolboxState,
      architectApi,
      args.runGeneratorHandled,
    );
    return new Editor(state, <ScreenEditor designerState={state} />);
  }

  if (editorType === 'DocumentationEditor') {
    const documentationData = data as DocumentationEditorData;
    const editorState = new DocumentationEditorState(
      editorData.editorId,
      node,
      documentationData,
      isDirty,
      architectApi,
    );
    return new Editor(
      editorState,
      (
        <GridEditor
          editorState={editorState}
          title={T('Documentation: {0}', 'documentation_editor_title', documentationData.label)}
        />
      ),
    );
  }

  return null;
}

export class Editor {
  constructor(
    public state: IEditorState,
    public element: React.ReactElement,
  ) {}
}
