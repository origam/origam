import { action, observable } from "mobx";
import { IEditorState } from "src/components/editorTabView/IEditorState.ts";
import {
  IEditorNode
} from "src/components/editorTabView/EditorTabViewState.ts";
import {
  ApiControl,
  IArchitectApi,
  IDataSource,
  IEditorField,
  ISectionEditorData,
} from "src/API/IArchitectApi.ts";
import {
  IComponentData,
} from "src/components/editors/screenSectionEditor/ComponentType.tsx";
import {
  Component, toComponent, toComponentRecursive
} from "src/components/editors/screenSectionEditor/Component.tsx";
import { toChanges } from "src/components/editors/gridEditor/EditorProperty.ts";

// export interface IComponent {
//   id: string;
//   data: IComponentData;
//   left: number;
//   top: number;
//   width: number;
//   height: number;
//   labelWidth: number;
//   parentId: string | null;
//   relativeLeft?: number;
//   relativeTop?: number;
//   labelPosition: LabelPosition;
// }

export enum LabelPosition {Left, Right, Top, Bottom, None}

export function parseLabelPosition(value: string | undefined | null): LabelPosition {
  if (value === undefined || value === null || value === '') {
    return LabelPosition.None;
  }
  const intValue = parseInt(value)
  if (isNaN(intValue)) {
    const validOptions = Object.keys(LabelPosition);
    if (!validOptions.includes(value)) {
      throw new Error(`Invalid LabelPosition: ${value}. Valid values are: ${validOptions.join(', ')}`);
    } else {
      return LabelPosition[value];
    }
  }

  const validOptions = Object.values(LabelPosition);
  if (!validOptions.includes(intValue as LabelPosition)) {
    throw new Error(`Invalid LabelPosition: ${value}. Valid values are: ${validOptions.join(', ')}`);
  }

  return intValue as LabelPosition;
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

export class ComponentDesignerState implements IEditorState {

  public surface: DesignSurfaceState;
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
    this.surface = new DesignSurfaceState(sectionEditorData, architectApi, this.editorNode.origamId);
  }

  onDesignerMouseUp(x: number, y: number) {
    return function* () {
      if (this.surface.isDragging) {
        this.surface.endDragging(x, y);
        yield* this.updateScreenEditor();
      }
      if (this.surface.isResizing) {
        this.surface.endResizing();
        yield* this.updateScreenEditor();
      }
    }.bind(this);
  }

  private* updateScreenEditor() {
    const modelChanges = this.surface.components.map(x => {
        return {
          schemaItemId: x.id,
          changes: toChanges(x.properties)
        }
      }
    )
    const newData = yield this.architectApi.updateScreenEditor({
      schemaItemId: this.toolbox.id,
      name: this.toolbox.name,
      selectedDataSourceId: this.toolbox.selectedDataSourceId,
      modelChanges: modelChanges
    });
    this.toolbox.name = newData.name;
    this.toolbox.selectedDataSourceId = newData.selectedDataSourceId;
    this.toolbox.fields = newData.fields;
    this.surface.loadComponents(newData.rootControl);
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
    return this.updateTopProperties();
  }

  nameChanged(value: string) {
    this.name = value;
    return this.updateTopProperties();
  }

  private updateTopProperties() {
    return function* (this: ToolboxState) {
      const newData = yield this.architectApi.updateScreenEditor({
        schemaItemId: this.id,
        name: this.name,
        selectedDataSourceId: this.selectedDataSourceId,
        modelChanges: []
      });
      this.name = newData.name;
      this.selectedDataSourceId = newData.selectedDataSourceId;
      this.fields = newData.fields;
    }.bind(this);
  }
}

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
    private editorNodeId: string
  ) {
    this.loadComponents(sectionEditorData.rootControl);
  }

  loadComponents(rootControl: ApiControl) {
    this.rootControl = rootControl;
    let components = [];
    for (const child of rootControl.children) {
      components = toComponentRecursive(child, null, components)
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
  startDragging(component: Component, mouseX: number, mouseY: number) {
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
      } else {
        // If we're dropping outside any group box, maintain the absolute position
        draggingComponent.parentId = null;
        draggingComponent.left = mouseX;
        draggingComponent.top = mouseY;
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
    return function* () {
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

  @action
  deleteComponent(id: string) {
    this.components = this.components.filter(component => component.id !== id);
  }
}

