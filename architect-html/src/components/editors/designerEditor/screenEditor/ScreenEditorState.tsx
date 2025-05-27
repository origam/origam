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
import { IArchitectApi, IScreenEditorData } from 'src/API/IArchitectApi.ts';
import { toChanges } from 'src/components/editors/gridEditor/EditorProperty.ts';
import { PropertiesState } from 'src/components/properties/PropertiesState.ts';
import { Component } from 'src/components/editors/designerEditor/common/designerComponents/Component.tsx';
import { ScreenToolboxState } from 'src/components/editors/designerEditor/screenEditor/ScreenToolboxState.tsx';
import { DesignerEditorState } from 'src/components/editors/designerEditor/common/DesignerEditorState.tsx';
import {
  controlToComponent,
  toComponentRecursive,
} from 'src/components/editors/designerEditor/common/designerComponents/ControlToComponent.tsx';
import { SectionItem } from 'src/components/editors/designerEditor/common/SectionItem.tsx';
import { ReactElement } from 'react';
import { FlowHandlerInput } from 'src/errorHandling/runInFlowWithHandler.ts';
import { CancellablePromise } from 'mobx/dist/api/flow';
import { TabControl } from 'src/components/editors/designerEditor/common/designerComponents/TabControl.tsx';

export class ScreenEditorState extends DesignerEditorState {
  public screenToolbox: ScreenToolboxState;

  constructor(
    editorNode: IEditorNode,
    isDirty: boolean,
    screenEditorData: IScreenEditorData,
    propertiesState: PropertiesState,
    screenToolboxState: ScreenToolboxState,
    architectApi: IArchitectApi,
    runGeneratorHandled: (args: FlowHandlerInput) => CancellablePromise<any>,
  ) {
    super(
      editorNode,
      isDirty,
      screenEditorData,
      propertiesState,
      screenToolboxState.toolboxState,
      architectApi,
      runGeneratorHandled,
      getSectionLoader(architectApi, editorNode.origamId),
    );
    this.screenToolbox = screenToolboxState;
  }

  delete(components: Component[]) {
    return function* (this: ScreenEditorState): Generator<Promise<any>, void, any> {
      const newData = yield this.architectApi.deleteScreenEditorItem({
        editorSchemaItemId: this.toolbox.id,
        schemaItemIds: components.map(x => x.id),
      });
      yield* this.surface.loadComponents(newData.data.rootControl);
      this.isDirty = true;
    }.bind(this);
  }

  createTabPage(tabControl: TabControl) {
    return function* (this: ScreenEditorState): Generator<Promise<any>, void, any> {
      const tabPageControlItemId = '6d13ec20-3b17-456e-ae43-3021cb067a70';
      const screenEditorItem = yield this.architectApi.createScreenEditorItem({
        editorSchemaItemId: this.editorNode.origamId,
        parentControlSetItemId: tabControl.id,
        controlItemId: tabPageControlItemId,
        top: 10,
        left: 10,
      });

      const sectionLoader = getSectionLoader(this.architectApi, this.editorNode.origamId);
      const newComponent = yield controlToComponent(
        screenEditorItem.screenItem,
        tabControl,
        this.surface.getChildren.bind(this.surface),
        sectionLoader,
      );
      newComponent.width = newComponent.width ?? 400;
      newComponent.height = newComponent.height ?? 20;
      this.surface.components.push(newComponent);
      this.surface.draggedComponentData = null;
      this.isDirty = true;

      const panelSizeChanged = this.surface.updatePanelSize(newComponent);
      if (panelSizeChanged) {
        yield* this.update() as any;
      }
    }.bind(this);
  }

  create(x: number, y: number) {
    return function* (this: ScreenEditorState): Generator<Promise<any>, void, any> {
      const parent = this.surface.findComponentAt(x, y);

      let currentParent: Component | null = parent;
      let relativeX = x;
      let relativeY = y;
      while (currentParent !== null) {
        relativeX -= currentParent.relativeLeft;
        relativeY -= currentParent.relativeTop;
        currentParent = currentParent.parent;
      }

      const screenEditorItem = yield this.architectApi.createScreenEditorItem({
        editorSchemaItemId: this.editorNode.origamId,
        parentControlSetItemId: parent.id,
        controlItemId: this.surface.draggedComponentData!.identifier!,
        top: relativeY,
        left: relativeX,
      });

      const sectionLoader = getSectionLoader(this.architectApi, this.editorNode.origamId);

      const components: Component[] = yield toComponentRecursive(
        screenEditorItem.screenItem,
        parent,
        [],
        this.surface.getChildren.bind(this.surface),
        sectionLoader,
      );
      for (const newComponent of components) {
        newComponent.width = newComponent.width ?? 400;
        newComponent.height = newComponent.height ?? 20;
        this.surface.components.push(newComponent);
      }
      const rootComponent = components[0];
      this.surface.draggedComponentData = null;
      this.isDirty = true;

      const panelSizeChanged = this.surface.updatePanelSize(rootComponent);
      if (panelSizeChanged) {
        yield* this.update() as any;
      }
    }.bind(this);
  }

  *save(): Generator<Promise<any>, void, any> {
    yield this.architectApi.persistChanges(this.editorNode.origamId);
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
    const updateResult = yield this.architectApi.updateScreenEditor({
      schemaItemId: this.toolbox.id,
      name: this.toolbox.name,
      selectedDataSourceId: this.toolbox.selectedDataSourceId,
      modelChanges: modelChanges,
    });
    this.isDirty = updateResult.isDirty;
    const newData = updateResult.data;
    this.toolbox.name = newData.name;
    this.toolbox.selectedDataSourceId = newData.selectedDataSourceId;
    this.surface.updateComponents(newData.rootControl);
  }
}

function getSectionLoader(architectApi: IArchitectApi, editorNodeId: string): any {
  return async function loadSection(sectionId: string): Promise<ReactElement> {
    const sectionControls = await architectApi.loadSections(editorNodeId, [sectionId]);
    const sectionComponents = await toComponentRecursive(sectionControls[sectionId], null, []);
    return <SectionItem components={sectionComponents} />;
  };
}
