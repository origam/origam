import { action, observable } from "mobx";
import { IEditorState } from "src/components/editorTabView/IEditorState.ts";
import {
  IEditorNode
} from "src/components/editorTabView/EditorTabViewState.ts";
import {
  IArchitectApi,
  IDataSource,
  IEditorField,
  ISectionEditorData,
} from "src/API/IArchitectApi.ts";
import {
  IComponentData,
} from "src/components/editors/screenSectionEditor/ComponentType.tsx";
import {
  Component, toComponent
} from "src/components/editors/screenSectionEditor/Component.tsx";


export interface IComponent {
  id: string;
  data: IComponentData;
  left: number;
  top: number;
  width: number;
  height: number;
  labelWidth: number;
  parentId: string | null;
  relativeLeft?: number;
  relativeTop?: number;
}

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

export class ComponentDesignerState implements IEditorState {

  public surface ;
  public toolbox: ToolboxState;

  @observable accessor isActive: boolean;
  @observable accessor isDirty: boolean;
  @observable accessor isPersisted: boolean;

  get label() {
    return this.toolbox.name;
  }

  get schemaItemId() {
    return this.editorNode.origamId;
  }

  constructor(
    private editorNode: IEditorNode,
    isPersisted: boolean,
    sectionEditorData: ISectionEditorData,
    private architectApi: IArchitectApi
  ) {
    this.isPersisted = isPersisted;
    this.toolbox = new ToolboxState(sectionEditorData, editorNode.origamId, architectApi);
    this.surface = new DesignSurfaceState(sectionEditorData);
  }

  * save(): Generator<Promise<any>, void, any> {

  }
}

export class ToolboxState {
  dataSources: IDataSource[];
  @observable accessor name: string;
  id: string;
  schemaExtensionId: string;
  @observable accessor selectedDataSourceId: string;
  @observable accessor fields: IEditorField[];

  constructor(
    sectionEditorData: ISectionEditorData,
    id: string,
    private architectApi: IArchitectApi
  ) {
    this.dataSources = sectionEditorData.dataSources;
    this.name = sectionEditorData.name;
    this.schemaExtensionId = sectionEditorData.schemaExtensionId;
    this.selectedDataSourceId = sectionEditorData.selectedDataSourceId;
    this.fields = sectionEditorData.fields;
    this.id = id;
  }

  selectedDataSourceIdChanged(value: string) {
    this.selectedDataSourceId = value;
    return this.update();
  }

  nameChanged(value: string) {
    this.name = value;
    return this.update();
  }

  private update() {
    return function* (this: ToolboxState) {
      const newData = yield this.architectApi.updateScreenEditor(
        this.id,
        this.name,
        this.selectedDataSourceId
      );
      this.name = newData.name;
      this.selectedDataSourceId = newData.selectedDataSourceId;
      this.fields = newData.fields;
    }.bind(this);
  }
}


export class DesignSurfaceState {
  @observable accessor components: IComponent[] = [];
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

  get isDragging() {
    return !!this.dragState.component;
  }

  get isResizing() {
    return !!this.resizeState.component;
  }

  get draggingComponentId() {
    return this.dragState.component?.id;
  }

  constructor(sectionEditorData: ISectionEditorData) {
    let components = [];
    for (const child of sectionEditorData.rootControl.children) {
      components = toComponent(child, null, components)
    }
    this.components = components;
  }

  @action
  selectComponent(componentId: string | null) {
    this.selectedComponentId = componentId;
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

    if (component.data.type === 'GroupBox') {
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
      data: this.draggedComponentData!,
      left: x,
      top: y,
      width: this.draggedComponentData?.type === 'GroupBox' ? 200 : 300,
      height: this.draggedComponentData?.type === 'GroupBox' ? 150 : 20,
      labelWidth: 100,
    });

    this.components.push(newComponent);
    this.draggedComponentData = null;
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

