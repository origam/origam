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

import { IEditorNode } from 'src/components/editorTabView/EditorTabViewState.ts';
import { IArchitectApi, ISectionEditorData } from 'src/API/IArchitectApi.ts';
import { toChanges } from 'src/components/editors/gridEditor/EditorProperty.ts';
import { PropertiesState } from 'src/components/properties/PropertiesState.ts';
import { Component } from 'src/components/editors/designerEditor/common/designerComponents/Component.tsx';
import { SectionToolboxState } from 'src/components/editors/designerEditor/screenSectionEditor/SectionToolboxState.tsx';
import { DesignerEditorState } from 'src/components/editors/designerEditor/common/DesignerEditorState.tsx';
import { controlToComponent } from 'src/components/editors/designerEditor/common/designerComponents/ControlToComponent.tsx';
import { FlowHandlerInput } from 'src/errorHandling/runInFlowWithHandler.ts';
import { CancellablePromise } from 'mobx/dist/api/flow';

export class ScreenSectionEditorState extends DesignerEditorState {
  public sectionToolbox: SectionToolboxState;

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
      this.isDirty = true;
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
        top: relativeY,
        left: relativeX,
      });

      const newComponent = yield controlToComponent(apiControl, null);
      newComponent.width = newComponent.width ?? 400;
      newComponent.height = newComponent.height ?? 20;
      newComponent.parent = parent;
      this.surface.components.push(newComponent);
      this.surface.draggedComponentData = null;
      this.isDirty = true;

      const panelSizeChanged = this.surface.updatePanelSize(newComponent);
      if (panelSizeChanged) {
        yield* this.update() as any;
      }
    }.bind(this);
  }

  *save(): Generator<Promise<any>, void, any> {
    yield this.architectApi.persistSectionEditorChanges(this.editorNode.origamId);
    yield* super.save();
  }

  protected update(): Generator<Promise<any>, void, any> {
    return this.updateGenerator();
  }

  protected *updateGenerator(): Generator<Promise<any>, void, any> {
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
    this.isDirty = updateResult.isDirty;
    const newData = updateResult.data;
    this.toolbox.name = newData.name;
    this.toolbox.selectedDataSourceId = newData.selectedDataSourceId;
    this.sectionToolbox.fields = newData.fields;
    yield* this.surface.loadComponents(newData.rootControl);
  }
}
