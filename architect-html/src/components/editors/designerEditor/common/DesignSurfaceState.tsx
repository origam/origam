import { action, observable } from "mobx";
import {
  Component,
} from "src/components/editors/designerEditor/common/designerComponents/Component.tsx";
import {
  ComponentType,
  IComponentData
} from "src/components/editors/designerEditor/common/ComponentType.tsx";
import {
  IApiControl, IDesignerEditorData,
} from "src/API/IArchitectApi.ts";
import { PropertiesState } from "src/components/properties/PropertiesState.ts";
import {
  toComponentRecursive
} from "src/components/editors/designerEditor/common/designerComponents/ControlToComponent.tsx";
import { ReactElement } from "react";

export class DesignSurfaceState {
  @observable accessor components: Component[] = [];
  @observable accessor draggedComponentData: IComponentData | null = null;
  @observable accessor selectedComponent: Component | null = null;
  @observable accessor dragState: DragState = {
    component: null,
    startX: 0,
    startY: 0,
    originalLeft: 0,
    originalTop: 0,
    didDrag: false,
    startedAt: undefined
  };
  @observable accessor resizeState: ResizeState = {
    component: null,
    handle: null,
    startX: 0,
    startY: 0,
    originalWidth: 0,
    originalHeight: 0,
    originalLeft: 0,
    originalTop: 0
  };
  panel: Component = null as any; // will be assigned in loadComponents
  panelId: string | undefined;

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
    private loadComponent?: (componentId: string) => Promise<ReactElement>
  ) {
    if (editorData.rootControl) {
      this.panelId = editorData.rootControl.id;
      this.loadComponents(editorData.rootControl);
      this.panel = this.components.find(x => x.id === editorData.rootControl.id)!;
    }
  }

  async loadComponents(rootControl: IApiControl) {
    let components: Component[] = [];
    components = await toComponentRecursive(rootControl, null, components, this.loadComponent)
    this.components = components;
    this.panel = this.components.find(x => x.id === this.panelId)!;
    this.reselectComponent();
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
    if (component) {
      this.selectedComponent = component;
      this.propertiesState.setEdited(
        component.data.identifier ?? component.getProperty("Text")?.value ?? "",
        component.properties)
    } else {
      this.selectedComponent = null;
      this.propertiesState.setEdited("", [])
    }
  }

  @action
  updateDragging(mouseX: number, mouseY: number) {
    if (!this.dragState.component) return;

    const dx = mouseX - this.dragState.startX;
    const dy = mouseY - this.dragState.startY;
    const left = this.dragState.originalLeft + dx;
    const top = this.dragState.originalTop + dy;

    this.updatePosition(this.dragState.component, left, top);
    this.dragState.didDrag = true;
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
      originalLeft: component.absoluteLeft,
      originalTop: component.absoluteTop,
      didDrag: false,
      startedAt: new Date()
    };
  }

  @action
  endDragging(mouseX: number, mouseY: number) {
    if (!this.dragState.component || !this.dragState.startedAt) {
      return;
    }
    const dragTimeMilliSeconds = new Date().getTime() - this.dragState.startedAt.getTime();
    if(dragTimeMilliSeconds < 1000 )
    {
      this.dragState = {
        component: null,
        startX: 0,
        startY: 0,
        originalLeft: 0,
        originalTop: 0,
        didDrag: false,
        startedAt: undefined
      };
      return;
    }

    console.log("dragTimeMilliSeconds: " + dragTimeMilliSeconds);

    const draggingComponent = this.dragState.component;

    if (
      draggingComponent.data.type !== ComponentType.GroupBox &&
      draggingComponent.data.type !== ComponentType.AsPanel
    ) {
      const targetParent = this.findComponentAt(mouseX, mouseY);
      if (targetParent && draggingComponent.parent != targetParent) {
        draggingComponent.relativeLeft = draggingComponent.parent?.absoluteLeft ?? 0 - targetParent.absoluteLeft + draggingComponent.relativeLeft;
        draggingComponent.relativeTop = draggingComponent.parent?.absoluteTop ?? 0 - targetParent.absoluteTop + draggingComponent.relativeTop;
        draggingComponent.parent = targetParent;
      }
      this.updatePanelSize(draggingComponent);
    }

    this.dragState = {
      component: null,
      startX: 0,
      startY: 0,
      originalLeft: 0,
      originalTop: 0,
      didDrag: false,
      startedAt: undefined
    };
  }

  findComponentAt(mouseX: number, mouseY: number) {
    const componentsUnderPoint = this.components.filter(
      comp =>
        comp.canHaveChildren &&
        comp.isPointInside(mouseX, mouseY)
    ) ?? this.panel;
    return componentsUnderPoint
      .sort((comp1, comp2) => comp2.countParents() - comp1.countParents())[0]
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
        this.endResizing();
        yield* this.updateEditor();
      }
    }.bind(this);
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
      originalWidth: component.width,
      originalHeight: component.height,
      originalLeft: component.absoluteLeft,
      originalTop: component.absoluteTop
    };
  }

  @action
  updateResizing(mouseX: number, mouseY: number) {
    if (!this.resizeState.component || !this.resizeState.handle) return;

    const component = this.resizeState.component;
    const deltaX = mouseX - this.resizeState.startX;
    const deltaY = mouseY - this.resizeState.startY;
    const {
      originalWidth,
      originalHeight,
      originalLeft,
      originalTop
    } = this.resizeState;

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
    this.resizeState = {
      component: null,
      handle: null,
      startX: 0,
      startY: 0,
      originalWidth: 0,
      originalHeight: 0,
      originalLeft: 0,
      originalTop: 0
    };
  }

  @action
  updatePosition(component: Component, left: number, top: number) {
    component.absoluteLeft = left;
    component.absoluteTop = top;

    if (component.canHaveChildren) {
      for (const comp of this.components) {
        if (comp.parent?.id === component.id && comp.relativeLeft !== undefined && comp.relativeTop !== undefined) {
          comp.absoluteLeft = comp.relativeLeft + component.absoluteLeft;
          comp.absoluteTop = comp.relativeTop + component.absoluteTop;
        }
      }
    }
  }


  @action
  onClose() {
    this.propertiesState.setEdited("", []);
  }
}

interface DragState {
  component: Component | null;
  startX: number;
  startY: number;
  originalLeft: number;
  originalTop: number;
  didDrag: boolean;
  startedAt: Date | undefined;
}

interface ResizeState {
  component: Component | null;
  handle: ResizeHandle | null;
  startX: number;
  startY: number;
  originalWidth: number;
  originalHeight: number;
  originalLeft: number;
  originalTop: number;
}

const minComponentHeight = 20;
const minComponentWidth = 20;

export type ResizeHandle =
  'top'
  | 'right'
  | 'bottom'
  | 'left'
  | 'topLeft'
  | 'topRight'
  | 'bottomRight'
  | 'bottomLeft';