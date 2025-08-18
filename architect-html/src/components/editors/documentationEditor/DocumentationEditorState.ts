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

import { EditorProperty, toChanges } from 'src/components/editors/gridEditor/EditorProperty.ts';
import {
  DocumentationEditorData,
  IArchitectApi,
  IUpdatePropertiesResult,
} from 'src/API/IArchitectApi.ts';
import { GridEditorState } from 'src/components/editors/gridEditor/GridEditorState.ts';
import { IEditorNode } from 'src/components/editorTabView/EditorTabViewState.ts';

export class DocumentationEditorState extends GridEditorState {
  constructor(
    editorId: string,
    editorNode: IEditorNode,
    private documentationData: DocumentationEditorData,
    isDirty: boolean,
    architectApi: IArchitectApi,
  ) {
    const properties = documentationData.properties.map(property => new EditorProperty(property));
    super(editorId, editorNode, properties, isDirty, architectApi);
  }

  get label() {
    return `[${this.documentationData.label}]`;
  }

  *save(): Generator<Promise<any>, void, any> {
    try {
      this.isSaving = true;
      yield this.architectApi.persistDocumentationChanges(this.editorNode.origamId);
      if (this.editorNode.parent) {
        yield* this.editorNode.parent.loadChildren();
      }
      this._isDirty = false;
    } finally {
      this.isSaving = false;
    }
  }

  *onPropertyUpdated(
    property: EditorProperty,
    value: any,
  ): Generator<Promise<IUpdatePropertiesResult>, void, IUpdatePropertiesResult> {
    property.value = value;
    const changes = toChanges(this.properties);
    const updateResult = (yield this.architectApi.updateDocumentationProperties(
      this.editorNode.origamId,
      changes,
    )) as IUpdatePropertiesResult;
    this._isDirty = updateResult.isDirty;
  }
}
