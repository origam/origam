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

import { IApiControl, IDesignerEditorData } from '@api/IArchitectApi';
import { PropertiesState } from '@components/properties/PropertiesState';
import { ComponentType, IComponentData } from '@editors/designerEditor/common/ComponentType';
import { Component } from '@editors/designerEditor/common/designerComponents/Component';
import { toComponentRecursive } from '@editors/designerEditor/common/designerComponents/ControlToComponent';
import { FlowHandlerInput } from '@errors/runInFlowWithHandler';
import { action, observable } from 'mobx';
import { CancellablePromise } from 'mobx/dist/api/flow';
import { ReactElement } from 'react';
import {
  IComponentProvider,
  IDesignerVerb,
} from '@editors/designerEditor/common/IComponentProvider.tsx';

export class DesignSurfaceState implements IComponentProvider {
  @observable public accessor components: Component[] = [];
  @observable accessor draggedComponentData: IComponentData | null = null;
  @observable public accessor selectedComponent: Component | null = null;
  @observable accessor dragState: DragState = idleDragState();
  @observable accessor resizeState: ResizeState = idleResizeState();
  panel: Component = null as any; // will be assigned in loadComponents
  panelId: string | undefined;
  getVerbs: (component: Component) => IDesignerVerb[] = () => [];

  get verbs(): IDesignerVerb[] {
    return this.selectedComponent ? this.getVerbs(this.selectedComponent) : [];
  }

  get isDragging() {
    return !!this.dragState.component;
  }

  get isResizing() {
    return !!this.resizeState.component;
  }

  get draggingComponentId() {
    return this.dragState.component?.id;
  }

  constructor(
    editorData: IDesignerEditorData,
    private propertiesState: PropertiesState,
    private updateEditor: () => Generator<Promise<any>, void, any>,
    runGeneratorHandled: (args: FlowHandlerInput) => CancellablePromise<any>,
    private loadComponent?: (componentId: string) => Promise<ReactElement>,
  ) {
    if (editorData.rootControl) {
      this.panelId = editorData.rootControl.id;
      runGeneratorHandled({ generator: this.loadComponents(editorData.rootControl) });
      this.panel = this.components.find(x => x.id === editorData.rootControl.id)!;
    }
  }

  *loadComponents(rootControl: IApiControl) {
    let components: Component[] = [];
    components = yield toComponentRecursive(
      rootControl,
      null,
      components,
      this.getChildren.bind(this),
      this.loadComponent,
    );
    this.components = components;
    this.propertiesState.setComponentProvider(this);
    this.panel = this.components.find(x => x.id === this.panelId)!;
    this.reselectComponent();
    this.restorePointerStates();
  }

  @action
  restorePointerStates() {
    if (this.dragState.component) {
      const draggedId = this.dragState.component.id;
      this.dragState.component = this.components.find(x => x.id === draggedId) ?? null;
      if (this.dragState.didDrag) {
        this.updateDragging(this.dragState.lastX, this.dragState.lastY);
      }
    }
    if (this.resizeState.component) {
      const resizedId = this.resizeState.component.id;
      this.resizeState.component = this.components.find(x => x.id === resizedId) ?? null;
      if (this.resizeState.didResize) {
        this.updateResizing(this.resizeState.lastX, this.resizeState.lastY);
      }
    }
  }

  updateComponents(control: IApiControl) {
    const currentComponent = this.components.find(x => x.id === control.id)!;
    for (const property of currentComponent.properties) {
      const updatedProperty = control.properties.find(x => x.name === property.name)!;
      property.value = updatedProperty.value;
      property.dropDownValues = updatedProperty.dropDownValues;
    }
    for (const childControl of control.children) {
      this.updateComponents(childControl);
    }
  }

  private reselectComponent() {
    const selectedComponentId = this.selectedComponent?.id;
    if (selectedComponentId) {
      const newSelectedInstance = this.components.find(x => x.id === selectedComponentId);
      this.selectComponent(newSelectedInstance);
    }
  }

  @action
  selectComponent(component: Component | null | undefined) {
    this.propertiesState.setComponentProvider(this);
    this.selectedComponent = component ?? null;
  }

  @action
  updateDragging(mouseX: number, mouseY: number) {
    if (!this.dragState.component) return;

    const dx = Math.round(mouseX - this.dragState.startX);
    const dy = Math.round(mouseY - this.dragState.startY);
    if (
      !this.dragState.didDrag &&
      Math.abs(dx) < dragStartDistance &&
      Math.abs(dy) < dragStartDistance
    ) {
      return;
    }
    this.dragState.didDrag = true;
    this.dragState.lastX = mouseX;
    this.dragState.lastY = mouseY;
    this.updatePosition(
      this.dragState.component,
      this.dragState.originalLeft + dx,
      this.dragState.originalTop + dy,
    );
  }

  @action
  startDragging(component: Component, mouseX: number, mouseY: number) {
    if (component.id === this.panelId) {
      return;
    }
    this.selectComponent(component);
    this.dragState = {
      component,
      startX: mouseX,
      startY: mouseY,
      lastX: mouseX,
      lastY: mouseY,
      originalLeft: component.absoluteLeft,
      originalTop: component.absoluteTop,
      didDrag: false,
    };
  }

  @action
  endDragging(mouseX: number, mouseY: number) {
    const draggingComponent = this.dragState.component;
    const didDrag = this.dragState.didDrag;
    this.dragState = idleDragState();
    if (!draggingComponent || !didDrag) {
      return;
    }

    if (
      draggingComponent.data.type !== ComponentType.GroupBox &&
      draggingComponent.data.type !== ComponentType.AsPanel
    ) {
      const targetParent = this.findComponentAt(mouseX, mouseY, draggingComponent);
      const previousParent = draggingComponent.parent;
      if (targetParent && draggingComponent.parent != targetParent) {
        const absoluteLeft = draggingComponent.absoluteLeft;
        const absoluteTop = draggingComponent.absoluteTop;
        draggingComponent.parent = targetParent;
        draggingComponent.absoluteLeft = absoluteLeft;
        draggingComponent.absoluteTop = absoluteTop;
        previousParent?.update();
        targetParent.onChildrenChanged();
      } else {
        targetParent?.update();
      }
    }
    draggingComponent.relativeLeft = Math.max(0, draggingComponent.relativeLeft);
    draggingComponent.relativeTop = Math.max(0, draggingComponent.relativeTop);
    this.updatePanelSize(draggingComponent);
  }

  findComponentAt(mouseX: number, mouseY: number, excludeComponent?: Component) {
    const excludeIds = excludeComponent
      ? [...this.getDescendants(excludeComponent).map(x => x.id), excludeComponent.id]
      : [];
    const componentsUnderPoint = this.components.filter(
      comp =>
        (excludeIds.length == 0 || !excludeIds.includes(comp.id)) &&
        comp.canAcceptChild(excludeComponent) &&
        comp.isActive &&
        comp.isPointInside(mouseX, mouseY),
    );
    const components1 = componentsUnderPoint.sort(
      (comp1, comp2) => comp2.countParents() - comp1.countParents(),
    );
    return components1[0] ?? this.panel;
  }

  onDesignerMouseUp(x: number, y: number) {
    return function* (this: DesignSurfaceState) {
      if (this.isDragging) {
        const didDrag = this.dragState.didDrag;
        this.endDragging(x, y);
        if (didDrag) {
          yield* this.updateEditor();
        }
      }
      if (this.isResizing) {
        const didResize = this.resizeState.didResize;
        this.endResizing();
        if (didResize) {
          yield* this.updateEditor();
        }
      }
    }.bind(this);
  }

  getChildren(component: Component) {
    return this.components.filter(x => x.parent?.id === component.id)!;
  }

  private getDescendants(component: Component): Component[] {
    return this.getChildren(component).flatMap(child => [child, ...this.getDescendants(child)]);
  }

  updatePanelSize(draggingComponent: Component) {
    let didUpdate = false;
    if (draggingComponent.absoluteRight > this.panel.absoluteRight) {
      this.panel.width += draggingComponent.absoluteRight - this.panel.absoluteRight + 20;
      didUpdate = true;
    }
    if (draggingComponent.absoluteBottom > this.panel.absoluteBottom) {
      this.panel.height += draggingComponent.absoluteBottom - this.panel.absoluteBottom + 20;
      didUpdate = true;
    }
    return didUpdate;
  }

  @action
  startResizing(component: Component, handle: ResizeHandle, mouseX: number, mouseY: number) {
    this.selectComponent(component);
    this.resizeState = {
      component,
      handle,
      startX: mouseX,
      startY: mouseY,
      lastX: mouseX,
      lastY: mouseY,
      originalWidth: component.width,
      originalHeight: component.height,
      originalLeft: component.absoluteLeft,
      originalTop: component.absoluteTop,
      didResize: false,
    };
  }

  @action
  updateResizing(mouseX: number, mouseY: number) {
    if (!this.resizeState.component || !this.resizeState.handle) return;

    const component = this.resizeState.component;
    const deltaX = Math.round(mouseX - this.resizeState.startX);
    const deltaY = Math.round(mouseY - this.resizeState.startY);
    if (!this.resizeState.didResize && deltaX === 0 && deltaY === 0) {
      return;
    }
    this.resizeState.didResize = true;
    this.resizeState.lastX = mouseX;
    this.resizeState.lastY = mouseY;
    const { originalWidth, originalHeight, originalLeft, originalTop } = this.resizeState;

    switch (this.resizeState.handle) {
      case 'right':
        component.width = Math.max(minComponentHeight, originalWidth + deltaX);
        break;
      case 'bottom':
        component.height = Math.max(minComponentHeight, originalHeight + deltaY);
        break;
      case 'left': {
        const newWidth = originalWidth - deltaX;
        if (newWidth >= minComponentWidth) {
          component.width = newWidth;
          component.absoluteLeft = originalLeft + deltaX;
        }
        break;
      }
      case 'top': {
        const newHeight = originalHeight - deltaY;
        if (newHeight >= minComponentHeight) {
          component.height = newHeight;
          component.absoluteTop = originalTop + deltaY;
        }
        break;
      }
      case 'topLeft': {
        const newHeightTL = originalHeight - deltaY;
        const newWidthTL = originalWidth - deltaX;
        if (newHeightTL >= minComponentHeight) {
          component.height = newHeightTL;
          component.absoluteTop = originalTop + deltaY;
        }
        if (newWidthTL >= minComponentWidth) {
          component.width = newWidthTL;
          component.absoluteLeft = originalLeft + deltaX;
        }
        break;
      }
      case 'topRight': {
        const newHeightTR = originalHeight - deltaY;
        if (newHeightTR >= minComponentHeight) {
          component.height = newHeightTR;
          component.absoluteTop = originalTop + deltaY;
        }
        component.width = Math.max(minComponentWidth, originalWidth + deltaX);
        break;
      }
      case 'bottomLeft': {
        const newWidthBL = originalWidth - deltaX;
        if (newWidthBL >= minComponentWidth) {
          component.width = newWidthBL;
          component.absoluteLeft = originalLeft + deltaX;
        }
        component.height = Math.max(minComponentHeight, originalHeight + deltaY);
        break;
      }
      case 'bottomRight':
        component.width = Math.max(minComponentWidth, originalWidth + deltaX);
        component.height = Math.max(minComponentHeight, originalHeight + deltaY);
        break;
    }
  }

  @action
  endResizing() {
    const resizedComponent = this.resizeState.component;
    const didResize = this.resizeState.didResize;
    this.resizeState = idleResizeState();
    if (!resizedComponent || !didResize) {
      return;
    }
    if (resizedComponent.relativeLeft < 0) {
      resizedComponent.width = Math.max(
        minComponentWidth,
        resizedComponent.width + resizedComponent.relativeLeft,
      );
      resizedComponent.relativeLeft = 0;
    }
    if (resizedComponent.relativeTop < 0) {
      resizedComponent.height = Math.max(
        minComponentHeight,
        resizedComponent.height + resizedComponent.relativeTop,
      );
      resizedComponent.relativeTop = 0;
    }
    resizedComponent.update();
    resizedComponent.parent?.update();
    this.updatePanelSize(resizedComponent);
  }

  @action
  updatePosition(component: Component, left: number, top: number) {
    component.absoluteLeft = left;
    component.absoluteTop = top;
  }

  @action
  onClose() {
    this.propertiesState.setComponentProvider(undefined);
  }
}

interface DragState {
  component: Component | null;
  startX: number;
  startY: number;
  lastX: number;
  lastY: number;
  originalLeft: number;
  originalTop: number;
  didDrag: boolean;
}

interface ResizeState {
  component: Component | null;
  handle: ResizeHandle | null;
  startX: number;
  startY: number;
  lastX: number;
  lastY: number;
  originalWidth: number;
  originalHeight: number;
  originalLeft: number;
  originalTop: number;
  didResize: boolean;
}

function idleDragState(): DragState {
  return {
    component: null,
    startX: 0,
    startY: 0,
    lastX: 0,
    lastY: 0,
    originalLeft: 0,
    originalTop: 0,
    didDrag: false,
  };
}

function idleResizeState(): ResizeState {
  return {
    component: null,
    handle: null,
    startX: 0,
    startY: 0,
    lastX: 0,
    lastY: 0,
    originalWidth: 0,
    originalHeight: 0,
    originalLeft: 0,
    originalTop: 0,
    didResize: false,
  };
}

const dragStartDistance = 3;

const minComponentHeight = 20;
const minComponentWidth = 20;

export type ResizeHandle =
  'top' | 'right' | 'bottom' | 'left' | 'topLeft' | 'topRight' | 'bottomRight' | 'bottomLeft';
