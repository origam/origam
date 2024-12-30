import {
  IDesignerEditorState
} from "src/components/editors/designerEditor/common/IDesignerEditorState.tsx";
import {
  DesignSurfaceState
} from "src/components/editors/designerEditor/common/DesignSurfaceState.tsx";
import {
  ToolboxState
} from "src/components/editors/designerEditor/common/ToolboxState.tsx";
import { observable } from "mobx";
import {
  IEditorNode
} from "src/components/editorTabView/EditorTabViewState.ts";
import {
  IArchitectApi, IDesignerEditorData,
  IUpdatePropertiesResult
} from "src/API/IArchitectApi.ts";
import { PropertiesState } from "src/components/properties/PropertiesState.ts";
import {
  EditorProperty
} from "src/components/editors/gridEditor/EditorProperty.ts";
import {
  Component
} from "src/components/editors/designerEditor/common/Component.tsx";

export abstract class DesignerEditorState implements IDesignerEditorState {

  public surface: DesignSurfaceState;
  public toolbox: ToolboxState;

  @observable accessor isActive: boolean = false;
  @observable accessor isDirty: boolean = false;

  get label() {
    return this.toolbox.name;
  }

  get schemaItemId() {
    return this.editorNode.origamId;
  }

  protected constructor(
    protected editorNode: IEditorNode,
    isDirty: boolean,
    editorData: IDesignerEditorData,
    propertiesState: PropertiesState,
    toolbox: ToolboxState,
    protected architectApi: IArchitectApi
  ) {
    this.isDirty = isDirty;
    this.toolbox = toolbox;
    this.surface = new DesignSurfaceState(
      editorData,
      propertiesState,
      this.update.bind(this)
    );
    propertiesState.onPropertyUpdated = this.onPropertyUpdated.bind(this);
  }

  * onPropertyUpdated(property: EditorProperty, value: any): Generator<Promise<IUpdatePropertiesResult>, void, IUpdatePropertiesResult> {
    property.value = value;
    yield* this.update() as any
  }

  public abstract delete(component: Component): void;

  public abstract create(x: number, y: number): void;

  protected abstract update(): Generator<Promise<any>, void, any>;

  onDesignerMouseUp(x: number, y: number) {
    return function* (this: DesignerEditorState) {
      if (this.surface.isDragging) {
        const didDrag = this.surface.dragState.didDrag;
        this.surface.endDragging(x, y);
        if (didDrag) {
          yield* this.update();
        }
      }
      if (this.surface.isResizing) {
        this.surface.endResizing();
        yield* this.update();
      }
    }.bind(this);
  }

  * save(): Generator<Promise<any>, void, any> {
    yield this.architectApi.persistChanges(this.editorNode.origamId);
    if (this.editorNode.parent) {
      yield* this.editorNode.parent.loadChildren();
    }
    this.isDirty = false;
  }
}