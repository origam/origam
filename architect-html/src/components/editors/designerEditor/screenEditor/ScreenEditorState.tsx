import { observable } from "mobx";
import {
  IEditorNode
} from "src/components/editorTabView/EditorTabViewState.ts";
import {
  ApiControl,
  IArchitectApi, IScreenEditorData, IScreenEditorModel,
  IUpdatePropertiesResult,
} from "src/API/IArchitectApi.ts";
import {
  EditorProperty,
  toChanges
} from "src/components/editors/gridEditor/EditorProperty.ts";
import {
  DesignSurfaceState
} from "src/components/editors/designerEditor/common/DesignSurfaceState.tsx";
import {
  ToolboxState
} from "src/components/editors/designerEditor/common/ToolboxState.tsx";
import { PropertiesState } from "src/components/properties/PropertiesState.ts";
import {
  Component,
  toComponent
} from "src/components/editors/designerEditor/common/Component.tsx";
import {
  IDesignerEditorState
} from "src/components/editors/designerEditor/common/IDesignerEditorState.tsx";
import {
  ScreenToolboxState
} from "src/components/editors/designerEditor/screenEditor/ScreenToolboxState.tsx";

export class ScreenEditorState implements IDesignerEditorState {

  public surface: DesignSurfaceState;
  public screenToolbox: ScreenToolboxState;
  public toolbox: ToolboxState;

  @observable accessor isActive: boolean = false;
  @observable accessor isDirty: boolean = false;

  get label() {
    return this.toolbox.name;
  }

  get schemaItemId() {
    return this.editorNode.origamId;
  }

  constructor(
    private editorNode: IEditorNode,
    isDirty: boolean,
    screenEditorData: IScreenEditorData,
    propertiesState: PropertiesState,
    private architectApi: IArchitectApi
  ) {
    this.isDirty = isDirty;
    this.screenToolbox = new ScreenToolboxState(screenEditorData, editorNode.origamId, architectApi);
    this.toolbox = this.screenToolbox.toolboxState;
    this.surface = new DesignSurfaceState(
      screenEditorData,
      propertiesState,
      this.updateScreenEditor.bind(this)
    );
    propertiesState.onPropertyUpdated = this.onPropertyUpdated.bind(this);
  }

  * onPropertyUpdated(property: EditorProperty, value: any): Generator<Promise<IUpdatePropertiesResult>, void, IUpdatePropertiesResult> {
    property.value = value;
    yield* this.updateScreenEditor() as any
  }

  deleteComponent(component: Component) {
    return function* (this: ScreenEditorState): Generator<Promise<IScreenEditorModel>, void, IScreenEditorModel> {
      const newData = yield this.architectApi.deleteScreenEditorItem({
        editorSchemaItemId: this.toolbox.id,
        schemaItemId: component.id
      });
      this.surface.loadComponents(newData.data.rootControl);
      this.isDirty = true;
    }.bind(this);
  }

  onDesignerMouseUp(x: number, y: number) {
    return function* (this: ScreenEditorState) {
      if (this.surface.isDragging) {
        const didDrag = this.surface.dragState.didDrag;
        this.surface.endDragging(x, y);
        if (didDrag) {
          yield* this.updateScreenEditor();
        }
      }
      if (this.surface.isResizing) {
        this.surface.endResizing();
        yield* this.updateScreenEditor();
      }
    }.bind(this);
  }

  createDraggedComponent(x: number, y: number) {
    return function* (this: ScreenEditorState): Generator<Promise<ApiControl>, void, ApiControl> {
      const parent = this.surface.findComponentAt(x, y);

      let currentParent: Component | null = parent;
      let relativeX = x;
      let relativeY = y;
      while (currentParent !== null) {
        relativeX -= currentParent.relativeLeft;
        relativeY -= currentParent.relativeTop;
        currentParent = currentParent.parent
      }

      const apiControl = yield this.architectApi.createScreenEditorItem({
        editorSchemaItemId: this.editorNode.origamId,
        parentControlSetItemId: parent.id,
        controlItemId: this.surface.draggedComponentData!.identifier!,
        top: relativeY,
        left: relativeX
      });

      const newComponent = toComponent(apiControl, null);
      newComponent.width = newComponent.width ?? 400;
      newComponent.height = newComponent.height ?? 20;
      newComponent.parent = parent;
      this.surface.components.push(newComponent);
      this.surface.draggedComponentData = null;
      this.isDirty = true;

      const panelSizeChanged = this.surface.updatePanelSize(newComponent);
      if (panelSizeChanged) {
        yield* this.updateScreenEditor() as any;
      }

    }.bind(this);
  }

  private* updateScreenEditor(): Generator<Promise<IScreenEditorModel>, void, IScreenEditorModel> {
    const modelChanges = this.surface.components.map(x => {
        return {
          schemaItemId: x.id,
          parentSchemaItemId: x.parent?.id,
          changes: toChanges(x.properties)
        }
      }
    )
    const updateResult = yield this.architectApi.updateScreenEditor({
      schemaItemId: this.toolbox.id,
      name: this.toolbox.name,
      selectedDataSourceId: this.toolbox.selectedDataSourceId,
      modelChanges: modelChanges
    });
    this.isDirty = updateResult.isDirty;
    const newData = updateResult.data;
    this.toolbox.name = newData.name;
    this.toolbox.selectedDataSourceId = newData.selectedDataSourceId;
    this.surface.loadComponents(newData.rootControl);
  }

  * save(): Generator<Promise<any>, void, any> {
    yield this.architectApi.persistChanges(this.editorNode.origamId);
    if (this.editorNode.parent) {
      yield* this.editorNode.parent.loadChildren();
    }
    this.isDirty = false;
  }
}
