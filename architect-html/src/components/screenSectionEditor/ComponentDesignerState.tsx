import { action, observable } from "mobx";

export interface IComponent {
  id: string;
  type: ComponentType;
  left: number;
  top: number;
  width: number;
  height: number;
  text: string;
  parentId: string | null;
  relativeLeft?: number;
  relativeTop?: number;
}

export type ComponentType = 'Label' | 'GroupBox';
export type ResizeHandle =
  'top'
  | 'right'
  | 'bottom'
  | 'left'
  | 'topLeft'
  | 'topRight'
  | 'bottomRight'
  | 'bottomLeft';

interface DragState {
  component: IComponent | null;
  startX: number;
  startY: number;
  originalLeft: number;
  originalTop: number;
}

interface ResizeState {
  component: IComponent | null;
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

export class ComponentDesignerState {
  @observable accessor components: IComponent[] = [];
  @observable accessor draggedComponentType: ComponentType | null = null;
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

  get isDragging() {
    return !!this.dragState.component;
  }

  get isResizing() {
    return !!this.resizeState.component;
  }

  get draggingComponentId() {
    return this.dragState.component?.id;
  }

  @action
  selectComponent(componentId: string | null) {
    this.selectedComponentId = componentId;
  }

  @action
  startDragging(component: IComponent, mouseX: number, mouseY: number) {
    this.selectComponent(component.id);
    this.dragState = {
      component,
      startX: mouseX,
      startY: mouseY,
      originalLeft: component.left,
      originalTop: component.top
    };
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
  endDragging(mouseX: number, mouseY: number) {
    if (!this.dragState.component) {
      return;
    }

    const draggingComponent = this.dragState.component;

    if (draggingComponent.type !== 'GroupBox') {
      const targetGroupBox = this.components.find(
        comp =>
          comp.type === 'GroupBox' &&
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
  startResizing(component: IComponent, handle: ResizeHandle, mouseX: number, mouseY: number) {
    this.selectComponent(component.id);
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
  updatePosition(component: IComponent, left: number, top: number) {
    component.left = left;
    component.top = top;

    if (component.type === 'GroupBox') {
      for (const comp of this.components) {
        if (comp.parentId === component.id && comp.relativeLeft !== undefined && comp.relativeTop !== undefined) {
          comp.left = comp.relativeLeft + component.left;
          comp.top = comp.relativeTop + component.top;
        }
      }
    }
  }

  @action
  createDraggedComponent(x: number, y: number) {
    const newComponent = new Component({
      id: `component-${Date.now()}`,
      type: this.draggedComponentType!,
      left: x,
      top: y,
      width: this.draggedComponentType === 'Label' ? 100 : 200,
      height: this.draggedComponentType === 'Label' ? 30 : 150,
      text: this.draggedComponentType === 'Label' ? 'New Label' : 'New Group Box'
    });

    this.components.push(newComponent);
    this.draggedComponentType = null;
  }

  private isPointInsideComponent(x: number, y: number, component: IComponent) {
    return (
      x >= component.left &&
      x <= component.left + component.width &&
      y >= component.top &&
      y <= component.top + component.height
    );
  }

  @action
  deleteComponent(id: string) {
    this.components = this.components.filter(component => component.id !== id);
  }
}

export class Component implements IComponent {
  id: string;
  type: ComponentType;
  @observable accessor left: number;
  @observable accessor top: number;
  @observable accessor width: number;
  @observable accessor height: number;
  @observable accessor text: string;
  @observable accessor parentId: string | null = null;
  @observable accessor relativeLeft: number | undefined;
  @observable accessor relativeTop: number | undefined;

  constructor(args: {
    id: string,
    type: ComponentType,
    left: number,
    top: number,
    width: number,
    height: number,
    text: string
  }) {
    this.id = args.id;
    this.type = args.type;
    this.left = args.left;
    this.top = args.top;
    this.width = args.width;
    this.height = args.height;
    this.text = args.text;
  }
}