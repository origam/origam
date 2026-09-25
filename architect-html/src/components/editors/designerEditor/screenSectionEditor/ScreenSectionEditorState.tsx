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
import { IArchitectApi, ISectionEditorData } from '@api/IArchitectApi';
import { IEditorNode } from '@components/editorTabView/EditorTabViewState';
import { PropertiesState } from '@components/properties/PropertiesState';
import { Component } from '@editors/designerEditor/common/designerComponents/Component';
import { controlToComponent } from '@editors/designerEditor/common/designerComponents/ControlToComponent';
import { DesignerEditorState } from '@editors/designerEditor/common/DesignerEditorState';
import { SectionToolboxState } from '@editors/designerEditor/screenSectionEditor/SectionToolboxState';
import { toChanges } from '@editors/gridEditor/EditorProperty';
import { FlowHandlerInput } from '@errors/runInFlowWithHandler';
import { observable, when } from 'mobx';
import { CancellablePromise } from 'mobx/dist/api/flow';

export class ScreenSectionEditorState extends DesignerEditorState {
  public sectionToolbox: SectionToolboxState;
  @observable private accessor isUpdating = false;
  private isUpdateRequested = false;

  constructor(
    editorId: string,
    editorNode: IEditorNode,
    isDirty: boolean,
    sectionEditorData: ISectionEditorData,
    propertiesState: PropertiesState,
    sectionToolboxState: SectionToolboxState,
    architectApi: IArchitectApi,
    runGeneratorHandled: (args: FlowHandlerInput) => CancellablePromise<any>,
  ) {
    super(
      editorId,
      editorNode,
      isDirty,
      sectionEditorData,
      propertiesState,
      sectionToolboxState.toolboxState,
      architectApi,
      runGeneratorHandled,
    );
    this.sectionToolbox = sectionToolboxState;
  }

  delete(components: Component[]) {
    return function* (this: ScreenSectionEditorState): Generator<Promise<any>, void, any> {
      const newData = yield this.architectApi.deleteSectionEditorItem({
        editorSchemaItemId: this.toolbox.id,
        schemaItemIds: components.map(x => x.id),
      });
      yield* this.surface.loadComponents(newData.data.rootControl);
      this.warnings = newData.data.warnings ?? [];
      this.isDirty = true;
      this.resendRunningUpdate();
    }.bind(this);
  }

  create(x: number, y: number) {
    return function* (this: ScreenSectionEditorState): Generator<Promise<any>, void, any> {
      const parent = this.surface.findComponentAt(x, y);

      let currentParent: Component | null = parent;
      let relativeX = x;
      let relativeY = y;
      while (currentParent !== null) {
        relativeX -= currentParent.relativeLeft;
        relativeY -= currentParent.relativeTop;
        currentParent = currentParent.parent;
      }

      const apiControl = yield this.architectApi.createSectionEditorItem({
        editorSchemaItemId: this.editorNode.origamId,
        parentControlSetItemId: parent.id,
        componentType: this.surface.draggedComponentData!.type,
        fieldName: this.surface.draggedComponentData!.identifier,
        top: Math.max(0, Math.round(relativeY)),
        left: Math.max(0, Math.round(relativeX)),
      });

      const newComponent = yield controlToComponent(apiControl, null);
      newComponent.width = newComponent.width ?? 100;
      newComponent.height = newComponent.height ?? 20;
      newComponent.parent = parent;
      this.surface.components.push(newComponent);
      this.surface.draggedComponentData = null;
      this.isDirty = true;
      this.resendRunningUpdate();

      const panelSizeChanged = this.surface.updatePanelSize(newComponent);
      if (panelSizeChanged) {
        yield* this.update() as any;
      } else {
        yield* this.refreshFields();
      }
    }.bind(this);
  }

  *save(): Generator<Promise<any>, void, any> {
    yield when(() => !this.isUpdating);
    if (this.isUpdateRequested) {
      yield* this.update();
    }
    if (this.warnings.length > 0) {
      throw new Error(T('Fix the warnings to save', 'save_blocked_by_warnings'));
    }
    yield this.architectApi.persistSectionEditorChanges(this.editorNode.origamId);
    yield* super.save();
  }

  *refreshFields(): Generator<Promise<any>, void, any> {
    const updateResult = yield this.architectApi.updateSectionEditor({
      schemaItemId: this.toolbox.id,
      name: this.toolbox.name,
      selectedDataSourceId: this.toolbox.selectedDataSourceId,
      modelChanges: [],
    });
    this.sectionToolbox.fields = updateResult.data.fields;
    this.warnings = updateResult.data.warnings ?? [];
  }

  private resendRunningUpdate() {
    if (this.isUpdating) {
      this.isUpdateRequested = true;
    }
  }

  protected *update(): Generator<Promise<any>, void, any> {
    this.isDirty = true;
    this.isUpdateRequested = true;
    if (this.isUpdating) {
      return;
    }
    this.isUpdating = true;
    try {
      while (this.isUpdateRequested) {
        this.isUpdateRequested = false;
        const modelChanges = this.surface.components.map(x => {
          return {
            schemaItemId: x.id,
            parentSchemaItemId: x.parent?.id,
            changes: toChanges(x.properties),
          };
        });
        const updateResult = yield this.architectApi.updateSectionEditor({
          schemaItemId: this.toolbox.id,
          name: this.toolbox.name,
          selectedDataSourceId: this.toolbox.selectedDataSourceId,
          modelChanges: modelChanges,
        });
        if (this.isUpdateRequested) {
          continue;
        }
        this.isDirty = updateResult.isDirty;
        const newData = updateResult.data;
        this.sectionToolbox.fields = newData.fields;
        this.warnings = newData.warnings ?? [];
        yield* this.surface.loadComponents(newData.rootControl);
      }
    } finally {
      this.isUpdating = false;
    }
  }
}
