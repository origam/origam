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

import { IArchitectApi, IDesignerEditorData, IUpdatePropertiesResult } from '@api/IArchitectApi.ts';
import { EditorProperty } from '@components/editors/gridEditor/EditorProperty.ts';
import { IEditorNode } from '@components/editorTabView/EditorTabViewState.ts';
import { PropertiesState } from '@components/properties/PropertiesState.ts';
import { Component } from '@editors/designerEditor/common/designerComponents/Component.tsx';
import { DesignSurfaceState } from '@editors/designerEditor/common/DesignSurfaceState.tsx';
import { IDesignerEditorState } from '@editors/designerEditor/common/IDesignerEditorState.tsx';
import { ToolboxState } from '@editors/designerEditor/common/ToolboxState.tsx';
import { FlowHandlerInput } from '@errors/runInFlowWithHandler.ts';
import { observable } from 'mobx';
import { CancellablePromise } from 'mobx/dist/api/flow';
import { ReactElement } from 'react';

export abstract class DesignerEditorState implements IDesignerEditorState {
  public surface: DesignSurfaceState;
  public toolbox: ToolboxState;

  @observable accessor isActive: boolean = false;
  @observable accessor isDirty: boolean = false;

  get label() {
    return this.toolbox.name;
  }

  protected constructor(
    public editorId: string,
    protected editorNode: IEditorNode,
    isDirty: boolean,
    editorData: IDesignerEditorData,
    propertiesState: PropertiesState,
    toolbox: ToolboxState,
    protected architectApi: IArchitectApi,
    runGeneratorHandled: (args: FlowHandlerInput) => CancellablePromise<any>,
    loadComponent?: (componentId: string) => Promise<ReactElement>,
  ) {
    this.isDirty = isDirty;
    this.toolbox = toolbox;
    this.surface = new DesignSurfaceState(
      editorData,
      propertiesState,
      this.update.bind(this),
      runGeneratorHandled,
      loadComponent,
    );
    propertiesState.onPropertyUpdated = this.onPropertyUpdated.bind(this);
    toolbox.onNameTopPropertiesChanged = () => (this.isDirty = true);
  }

  *onPropertyUpdated(
    property: EditorProperty,
    value: any,
  ): Generator<Promise<IUpdatePropertiesResult>, void, IUpdatePropertiesResult> {
    property.value = value;
    yield* this.update() as any;
  }

  public abstract delete(components: Component[]): void;

  public abstract create(x: number, y: number): void;

  protected abstract update(): Generator<Promise<any>, void, any>;

  *save(): Generator<Promise<any>, void, any> {
    if (this.editorNode.parent?.parent) {
      yield* this.editorNode.parent.parent.loadChildren();
    }
    this.isDirty = false;
  }
}
