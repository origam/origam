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

import {
  IApiControl,
  IDesignerEditorData,
  IUpdatePropertiesResult,
  PropertyValue,
} from '@api/IArchitectApi';
import { ComponentType, IComponentData } from '@editors/designerEditor/common/ComponentType';
import { Component } from '@editors/designerEditor/common/designerComponents/Component';
import { toComponentRecursive } from '@editors/designerEditor/common/designerComponents/ControlToComponent';
import {
  allEdges,
  Bounds,
  Edge,
  findSnapLines,
  findSnapOffset,
  noSnapTargets,
  SnapLine,
  SnapTargets,
} from '@editors/designerEditor/common/Snapping';
import { EditorProperty } from '@editors/gridEditor/EditorProperty';
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
  @observable.ref accessor snapLines: SnapLine[] = [];
  @observable accessor isTabOrderMode = false;
  @observable private accessor tabOrderedIds: string[] = [];
  private snapTargetsCache: { container: Component | null; targets: SnapTargets } | null = null;
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
    private updateEditor: () => Generator<Promise<any>, void, any>,
    runGeneratorHandled: (args: FlowHandlerInput) => CancellablePromise<any>,
    private loadComponent?: (
      componentId: string,
      properties: EditorProperty[],
    ) => Promise<ReactElement>,
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
    this.panel = this.components.find(x => x.id === this.panelId)!;
    this.reselectComponent();
    this.restorePointerStates();
  }

  @action
  restorePointerStates() {
    this.snapLines = [];
    if (this.dragState.component) {
      const draggedId = this.dragState.component.id;
      this.dragState.component = this.components.find(x => x.id === draggedId) ?? null;
      if (this.dragState.didDrag) {
        this.updateDragging(
          this.dragState.lastX,
          this.dragState.lastY,
          this.dragState.isSnapEnabled,
        );
      }
    }
    if (this.resizeState.component) {
      const resizedId = this.resizeState.component.id;
      this.resizeState.component = this.components.find(x => x.id === resizedId) ?? null;
      if (this.resizeState.didResize) {
        this.updateResizing(
          this.resizeState.lastX,
          this.resizeState.lastY,
          this.resizeState.isSnapEnabled,
        );
      }
    }
  }

  @action
  setSnapEnabled(isSnapEnabled: boolean) {
    if (this.dragState.didDrag) {
      this.updateDragging(this.dragState.lastX, this.dragState.lastY, isSnapEnabled);
    }
    if (this.resizeState.didResize) {
      this.updateResizing(this.resizeState.lastX, this.resizeState.lastY, isSnapEnabled);
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
    this.selectedComponent = component ?? null;
  }

  *onPropertyUpdated(
    property: EditorProperty,
    value: PropertyValue,
  ): Generator<Promise<IUpdatePropertiesResult>, void, IUpdatePropertiesResult> {
    property.value = value;
    yield* this.updateEditor() as any;
  }

  @action
  toggleTabOrderMode() {
    this.isTabOrderMode = !this.isTabOrderMode;
    this.tabOrderedIds = [];
    this.selectComponent(null);
  }

  showsTabOrder(component: Component) {
    return (
      component.id !== this.panelId &&
      component.data.type !== ComponentType.TabPage &&
      !!component.getProperty('TabIndex')
    );
  }

  isTabOrderFixed(component: Component) {
    return component.parent?.data.type === ComponentType.SplitPanel;
  }

  isTabOrderAssigned(component: Component) {
    return this.tabOrderedIds.includes(component.id);
  }

  tabOrderLabel(component: Component) {
    const tabIndices: number[] = [];
    for (let current: Component | null = component; current; current = current.parent) {
      if (this.showsTabOrder(current)) {
        tabIndices.unshift(current.tabIndex);
      }
    }
    return tabIndices.join('.');
  }

  *assignTabIndex(component: Component, continueFrom: boolean): Generator<Promise<any>, void, any> {
    const parent = component.parent;
    if (!parent || this.isTabOrderFixed(component) || !this.showsTabOrder(component)) {
      return;
    }
    const siblings = this.getChildren(parent)
      .filter(sibling => this.showsTabOrder(sibling))
      .sort((siblingA, siblingB) => siblingA.tabIndex - siblingB.tabIndex);
    const assigned = continueFrom
      ? siblings.slice(0, siblings.indexOf(component) + 1)
      : [
          ...siblings.filter(sibling => sibling !== component && this.isTabOrderAssigned(sibling)),
          component,
        ];
    const ordered = [...assigned, ...siblings.filter(sibling => !assigned.includes(sibling))];
    this.tabOrderedIds = [
      ...this.tabOrderedIds.filter(id => !siblings.some(sibling => sibling.id === id)),
      ...assigned.map(sibling => sibling.id),
    ];
    let isChanged = false;
    ordered.forEach((sibling, index) => {
      isChanged ||= sibling.tabIndex !== index;
      sibling.tabIndex = index;
    });
    if (isChanged) {
      yield* this.updateEditor();
    }
  }

  @action
  updateDragging(mouseX: number, mouseY: number, isSnapEnabled: boolean) {
    const component = this.dragState.component;
    if (!component) return;

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
    this.dragState.isSnapEnabled = isSnapEnabled;
    const left = this.dragState.originalLeft + dx;
    const top = this.dragState.originalTop + dy;
    const container = this.canChangeParent(component)
      ? this.findComponentAt(mouseX, mouseY, component)
      : component.parent;
    const targets = isSnapEnabled ? this.getSnapTargets(component, container) : noSnapTargets;
    const offset = findSnapOffset(boundsAt(component, left, top), targets, allEdges);
    this.updatePosition(component, left + offset.x, top + offset.y);
    this.snapLines = findSnapLines(boundsOf(component), targets, allEdges);
  }

  private getSnapTargets(component: Component, container: Component | null): SnapTargets {
    let cache = this.snapTargetsCache;
    if (!cache || cache.container !== container) {
      cache = { container, targets: this.collectSnapTargets(component, container) };
      this.snapTargetsCache = cache;
    }
    return cache.targets;
  }

  private collectSnapTargets(component: Component, container: Component | null): SnapTargets {
    if (!container) {
      return noSnapTargets;
    }
    const siblings = this.getChildren(container).filter(
      child => child.id !== component.id && child.isActive,
    );
    return { container: container.contentBounds, siblings: siblings.map(boundsOf) };
  }

  private canChangeParent(component: Component) {
    return (
      component.data.type !== ComponentType.GroupBox &&
      component.data.type !== ComponentType.AsPanel
    );
  }

  @action
  startDragging(component: Component, mouseX: number, mouseY: number) {
    if (component.id === this.panelId) {
      return;
    }
    this.selectComponent(component);
    this.snapTargetsCache = null;
    this.dragState = {
      component,
      startX: mouseX,
      startY: mouseY,
      lastX: mouseX,
      lastY: mouseY,
      originalLeft: component.absoluteLeft,
      originalTop: component.absoluteTop,
      didDrag: false,
      isSnapEnabled: true,
    };
  }

  @action
  endDragging(mouseX: number, mouseY: number) {
    const draggingComponent = this.dragState.component;
    const didDrag = this.dragState.didDrag;
    this.dragState = idleDragState();
    this.snapLines = [];
    if (!draggingComponent || !didDrag) {
      return;
    }

    if (this.canChangeParent(draggingComponent)) {
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
    this.snapTargetsCache = null;
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
      isSnapEnabled: true,
    };
  }

  @action
  updateResizing(mouseX: number, mouseY: number, isSnapEnabled: boolean) {
    if (!this.resizeState.component || !this.resizeState.handle) return;

    const component = this.resizeState.component;
    let deltaX = Math.round(mouseX - this.resizeState.startX);
    let deltaY = Math.round(mouseY - this.resizeState.startY);
    if (!this.resizeState.didResize && deltaX === 0 && deltaY === 0) {
      return;
    }
    this.resizeState.didResize = true;
    this.resizeState.lastX = mouseX;
    this.resizeState.lastY = mouseY;
    this.resizeState.isSnapEnabled = isSnapEnabled;
    const { originalWidth, originalHeight, originalLeft, originalTop } = this.resizeState;
    const edges = resizedEdges[this.resizeState.handle];
    const targets = isSnapEnabled
      ? this.getSnapTargets(component, component.parent)
      : noSnapTargets;
    const offset = findSnapOffset(
      {
        left: originalLeft + (edges.includes('left') ? deltaX : 0),
        top: originalTop + (edges.includes('top') ? deltaY : 0),
        right: originalLeft + originalWidth + (edges.includes('right') ? deltaX : 0),
        bottom: originalTop + originalHeight + (edges.includes('bottom') ? deltaY : 0),
        baseline: null,
      },
      targets,
      edges,
    );
    deltaX += offset.x;
    deltaY += offset.y;

    if (edges.includes('left')) {
      component.width = Math.max(minComponentWidth, originalWidth - deltaX);
      component.absoluteLeft = originalLeft + originalWidth - component.width;
    }
    if (edges.includes('right')) {
      component.width = Math.max(minComponentWidth, originalWidth + deltaX);
    }
    if (edges.includes('top')) {
      component.height = Math.max(minComponentHeight, originalHeight - deltaY);
      component.absoluteTop = originalTop + originalHeight - component.height;
    }
    if (edges.includes('bottom')) {
      component.height = Math.max(minComponentHeight, originalHeight + deltaY);
    }
    this.snapLines = findSnapLines(boundsOf(component), targets, edges);
  }

  @action
  endResizing() {
    const resizedComponent = this.resizeState.component;
    const didResize = this.resizeState.didResize;
    this.resizeState = idleResizeState();
    this.snapLines = [];
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
  isSnapEnabled: boolean;
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
  isSnapEnabled: boolean;
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
    isSnapEnabled: true,
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
    isSnapEnabled: true,
  };
}

function boundsOf(component: Component): Bounds {
  return boundsAt(component, component.absoluteLeft, component.absoluteTop);
}

function boundsAt(component: Component, left: number, top: number): Bounds {
  const baseline = component.textBaseline;
  return {
    left,
    top,
    right: left + component.width,
    bottom: top + component.height,
    baseline: baseline === null ? null : baseline - component.absoluteTop + top,
  };
}

const dragStartDistance = 3;

const minComponentHeight = 20;
const minComponentWidth = 20;

export type ResizeHandle =
  'top' | 'right' | 'bottom' | 'left' | 'topLeft' | 'topRight' | 'bottomRight' | 'bottomLeft';

const resizedEdges: Record<ResizeHandle, Edge[]> = {
  top: ['top'],
  right: ['right'],
  bottom: ['bottom'],
  left: ['left'],
  topLeft: ['top', 'left'],
  topRight: ['top', 'right'],
  bottomRight: ['bottom', 'right'],
  bottomLeft: ['bottom', 'left'],
};
