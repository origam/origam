import { action, observable } from "mobx";
import {
  Component,
  toComponent,
  toComponentRecursive
} from "src/components/editors/screenSectionEditor/Component.tsx";
import {
  IComponentData
} from "src/components/editors/screenSectionEditor/ComponentType.tsx";
import {
  ApiControl,
  IArchitectApi,
  ISectionEditorData
} from "src/API/IArchitectApi.ts";
import {
  ResizeHandle
} from "src/components/editors/screenSectionEditor/ComponentDesignerState.tsx";
import { PropertiesState } from "src/components/properties/PropertiesState.ts";

export class DesignSurfaceState {
  @observable accessor components: Component[] = [];
  @observable accessor draggedComponentData: IComponentData | null = null;
  @observable accessor selectedComponentId: string | null = null;
  @observable accessor dragState: DragState = {
    component: null,
    startX: 0,
    startY: 0,
    originalLeft: 0,
    originalTop: 0
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
  rootControl: ApiControl;

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
    sectionEditorData: ISectionEditorData,
    private architectApi: IArchitectApi,
    private propertiesState: PropertiesState,
    private editorNodeId: string,
    private setDirty: (isDirty: boolean) => void
  ) {
    this.rootControl = sectionEditorData.rootControl;
    this.loadComponents(sectionEditorData.rootControl);
  }

  loadComponents(rootControl: ApiControl) {
    let components: Component[] = [];
    for (const child of rootControl.children) {
      components = toComponentRecursive(child, null, components)
    }
    this.components = components;
  }

  @action
  selectComponent(componentA: Component | null) {
    const component = this.components.find(x =>x.id === componentA?.id);
    if (component) {
      this.selectedComponentId = component.id;
      this.propertiesState.setEdited(component.data.fieldName, component.properties)
    } else {
      this.selectedComponentId = null;
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
  }

  @action
  startDragging(component: Component, mouseX: number, mouseY: number) {
    this.selectComponent(component);
    this.dragState = {
      component,
      startX: mouseX,
      startY: mouseY,
      originalLeft: component.left,
      originalTop: component.top
    };
  }

  @action
  endDragging(mouseX: number, mouseY: number) {
    if (!this.dragState.component) {
      return;
    }

    const draggingComponent = this.dragState.component;

    if (draggingComponent.data.type !== 'GroupBox') {
      const targetGroupBox = this.components.find(
        comp =>
          comp.data.type === 'GroupBox' &&
          this.isPointInsideComponent(mouseX, mouseY, comp)
      );

      if (targetGroupBox) {
        // Calculate relative position based on the component's current position
        draggingComponent.parentId = targetGroupBox.id;
        draggingComponent.relativeLeft = draggingComponent.left - targetGroupBox.left;
        draggingComponent.relativeTop = draggingComponent.top - targetGroupBox.top;

        // Update the absolute position to ensure it stays in the correct place
        draggingComponent.left = targetGroupBox.left + draggingComponent.relativeLeft;
        draggingComponent.top = targetGroupBox.top + draggingComponent.relativeTop;
      } else if (!targetGroupBox && draggingComponent.parentId) {
        // If we're dropping outside any group box, maintain the absolute position
        draggingComponent.parentId = null;
        draggingComponent.relativeLeft = undefined;
        draggingComponent.relativeTop = undefined;
      }
    }

    this.dragState = {
      component: null,
      startX: 0,
      startY: 0,
      originalLeft: 0,
      originalTop: 0
    };
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
      originalLeft: component.left,
      originalTop: component.top
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
          component.left = originalLeft + deltaX;
        }
        break;
      }
      case 'top': {
        const newHeight = originalHeight - deltaY;
        if (newHeight >= minComponentHeight) {
          component.height = newHeight;
          component.top = originalTop + deltaY;
        }
        break;
      }
      case 'topLeft': {
        const newHeightTL = originalHeight - deltaY;
        const newWidthTL = originalWidth - deltaX;
        if (newHeightTL >= minComponentHeight) {
          component.height = newHeightTL;
          component.top = originalTop + deltaY;
        }
        if (newWidthTL >= minComponentWidth) {
          component.width = newWidthTL;
          component.left = originalLeft + deltaX;
        }
        break;
      }
      case 'topRight': {
        const newHeightTR = originalHeight - deltaY;
        if (newHeightTR >= minComponentHeight) {
          component.height = newHeightTR;
          component.top = originalTop + deltaY;
        }
        component.width = Math.max(minComponentWidth, originalWidth + deltaX);
        break;
      }
      case 'bottomLeft': {
        const newWidthBL = originalWidth - deltaX;
        if (newWidthBL >= minComponentWidth) {
          component.width = newWidthBL;
          component.left = originalLeft + deltaX;
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
    component.left = left;
    component.top = top;

    if (component.data.type === 'GroupBox') {
      for (const comp of this.components) {
        if (comp.parentId === component.id && comp.relativeLeft !== undefined && comp.relativeTop !== undefined) {
          comp.left = comp.relativeLeft + component.left;
          comp.top = comp.relativeTop + component.top;
        }
      }
    }
  }

  createDraggedComponent(x: number, y: number) {
    return function* (this: DesignSurfaceState): Generator<Promise<ApiControl>, void, ApiControl> {
      const apiControl = yield this.architectApi.createScreenEditorItem({
        editorSchemaItemId: this.editorNodeId,
        parentControlSetItemId: this.rootControl.id,
        componentType: this.draggedComponentData!.type,
        fieldName: this.draggedComponentData!.fieldName,
        top: y,
        left: x
      });

      const newComponent = toComponent(apiControl, null);
      newComponent.width = newComponent.width ?? 400;
      newComponent.height = newComponent.height ?? 20;
      this.components.push(newComponent);
      this.draggedComponentData = null;
      this.setDirty(true);
    }.bind(this);
  }

  private isPointInsideComponent(x: number, y: number, component: Component) {
    return (
      x >= component.left &&
      x <= component.left + component.width &&
      y >= component.top &&
      y <= component.top + component.height
    );
  }
}

interface DragState {
  component: Component | null;
  startX: number;
  startY: number;
  originalLeft: number;
  originalTop: number;
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