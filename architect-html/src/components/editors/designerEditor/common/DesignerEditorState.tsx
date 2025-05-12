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
} from "src/components/editors/designerEditor/common/designerComponents/Component.tsx";
import { ReactElement } from "react";
import { FlowHandlerInput } from "src/errorHandling/runInFlowWithHandler.ts";
import { CancellablePromise } from "mobx/dist/api/flow";

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
    protected architectApi: IArchitectApi,
    runGeneratorHandled: (args: FlowHandlerInput) => CancellablePromise<any>,
    loadComponent?: (componentId: string) => Promise<ReactElement>
  ) {
    this.isDirty = isDirty;
    this.toolbox = toolbox;
    this.surface = new DesignSurfaceState(
      editorData,
      propertiesState,
      this.update.bind(this),
      runGeneratorHandled,
      loadComponent
    );
    propertiesState.onPropertyUpdated = this.onPropertyUpdated.bind(this);
    toolbox.onNameTopPropertiesChanged = ()=> this.isDirty = true;
  }

  * onPropertyUpdated(property: EditorProperty, value: any): Generator<Promise<IUpdatePropertiesResult>, void, IUpdatePropertiesResult> {
    property.value = value;
    yield* this.update() as any
  }

  public abstract delete(components: Component[]): void;

  public abstract create(x: number, y: number): void;

  protected abstract update(): Generator<Promise<any>, void, any>;

  * save(): Generator<Promise<any>, void, any> {
    if (this.editorNode.parent?.parent) {
      yield* this.editorNode.parent.parent.loadChildren();
    }
    this.isDirty = false;
  }
}