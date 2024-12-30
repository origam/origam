import {
  IEditorNode
} from "src/components/editorTabView/EditorTabViewState.ts";
import {
  ApiControl,
  IArchitectApi,
  IScreenEditorData,
  IScreenEditorModel,
} from "src/API/IArchitectApi.ts";
import { toChanges } from "src/components/editors/gridEditor/EditorProperty.ts";
import { PropertiesState } from "src/components/properties/PropertiesState.ts";
import {
  Component,
  toComponent
} from "src/components/editors/designerEditor/common/Component.tsx";
import {
  ScreenToolboxState
} from "src/components/editors/designerEditor/screenEditor/ScreenToolboxState.tsx";
import {
  DesignerEditorState
} from "src/components/editors/designerEditor/common/DesignerEditorState.tsx";


export class ScreenEditorState extends DesignerEditorState {
  public screenToolbox: ScreenToolboxState;

  constructor(
    editorNode: IEditorNode,
    isDirty: boolean,
    screenEditorData: IScreenEditorData,
    propertiesState: PropertiesState,
    screenToolboxState: ScreenToolboxState,
    architectApi: IArchitectApi
  ) {
    super(editorNode, isDirty, screenEditorData, propertiesState,screenToolboxState.toolboxState, architectApi );
    this.screenToolbox = screenToolboxState;
  }

  delete(component: Component) {
    return function* (this: ScreenEditorState): Generator<Promise<IScreenEditorModel>, void, IScreenEditorModel> {
      const newData = yield this.architectApi.deleteScreenEditorItem({
        editorSchemaItemId: this.toolbox.id,
        schemaItemId: component.id
      });
      this.surface.loadComponents(newData.data.rootControl);
      this.isDirty = true;
    }.bind(this);
  }

  create(x: number, y: number) {
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
        yield* this.update() as any;
      }

    }.bind(this);
  }

  protected update(): Generator<Promise<any>, void, any> {
    return this.updateGenerator();
  }

  protected* updateGenerator(): Generator<Promise<any>, void, any> {
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
}
