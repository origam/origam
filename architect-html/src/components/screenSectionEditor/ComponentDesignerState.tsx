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

interface DragState {
  component: IComponent | null;
  startX: number;
  startY: number;
  originalLeft: number;
  originalTop: number;
}

interface ResizeState {
  component: IComponent | null;
  handle: 'top' | 'right' | 'bottom' | 'left' | null;
}

export class ComponentDesignerState {
  @observable accessor components: IComponent[] = [];
  @observable accessor draggedComponentType: ComponentType | null = null;
  @observable accessor dragState: DragState = {
    component: null,
    startX: 0,
    startY: 0,
    originalLeft: 0,
    originalTop: 0
  };
  @observable accessor resizeState: ResizeState = {
    component: null,
    handle: null
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
  startDragging(component: IComponent, mouseX: number, mouseY: number) {
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
      }
      else if (!targetGroupBox && draggingComponent.parentId) {
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
  startResizing(component: IComponent, handle: ResizeState['handle']) {
    this.resizeState = { component, handle };
  }

  @action
  updateResizing(mouseX: number, mouseY: number) {
    if (!this.resizeState.component || !this.resizeState.handle) return;

    const component = this.resizeState.component;
    switch (this.resizeState.handle) {
      case 'right':
        component.width = Math.max(50, mouseX - component.left);
        break;
      case 'bottom':
        component.height = Math.max(50, mouseY - component.top);
        break;
      case 'left': {
        const newWidth = component.width + (component.left - mouseX);
        if (newWidth >= 50) {
          component.width = newWidth;
          component.left = mouseX;
        }
        break;
      }
      case 'top': {
        const newHeight = component.height + (component.top - mouseY);
        if (newHeight >= 50) {
          component.height = newHeight;
          component.top = mouseY;
        }
        break;
      }
    }
  }

  @action
  endResizing() {
    this.resizeState = { component: null, handle: null };
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

  constructor(args: { id: string, type: ComponentType, left: number, top: number, width: number, height: number, text: string }) {
    this.id = args.id;
    this.type = args.type;
    this.left = args.left;
    this.top = args.top;
    this.width = args.width;
    this.height = args.height;
    this.text = args.text;
  }
}