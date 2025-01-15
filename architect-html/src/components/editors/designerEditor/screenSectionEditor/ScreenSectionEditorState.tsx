import {
  IEditorNode
} from "src/components/editorTabView/EditorTabViewState.ts";
import {
  IApiControl,
  IArchitectApi,
  ISectionEditorData,
  ISectionEditorModel,
} from "src/API/IArchitectApi.ts";
import {
  toChanges
} from "src/components/editors/gridEditor/EditorProperty.ts";
import { PropertiesState } from "src/components/properties/PropertiesState.ts";
import {
  Component
} from "src/components/editors/designerEditor/common/designerComponents/Component.tsx";
import {
  SectionToolboxState
} from "src/components/editors/designerEditor/screenSectionEditor/SectionToolboxState.tsx";
import {
  DesignerEditorState
} from "src/components/editors/designerEditor/common/DesignerEditorState.tsx";
import {
  controlToComponent
} from "src/components/editors/designerEditor/common/designerComponents/ControlToComponent.tsx";

export class ScreenSectionEditorState extends DesignerEditorState {

  public sectionToolbox: SectionToolboxState;

  constructor(
    editorNode: IEditorNode,
    isDirty: boolean,
    sectionEditorData: ISectionEditorData,
    propertiesState: PropertiesState,
    sectionToolboxState: SectionToolboxState,
    architectApi: IArchitectApi
  ) {
    super(editorNode, isDirty, sectionEditorData, propertiesState, sectionToolboxState.toolboxState, architectApi);
    this.sectionToolbox = sectionToolboxState;
  }

  delete(component: Component) {
    return function* (this: ScreenSectionEditorState): Generator<Promise<ISectionEditorModel>, void, ISectionEditorModel> {
      const newData = yield this.architectApi.deleteSectionEditorItem({
        editorSchemaItemId: this.toolbox.id,
        schemaItemId: component.id
      });
      this.surface.loadComponents(newData.data.rootControl);
      this.isDirty = true;
    }.bind(this);
  }

  create(x: number, y: number) {
    return function* (this: ScreenSectionEditorState): Generator<Promise<IApiControl>, void, IApiControl> {
      const parent = this.surface.findComponentAt(x, y);

      let currentParent: Component | null = parent;
      let relativeX = x;
      let relativeY = y;
      while (currentParent !== null) {
        relativeX -= currentParent.relativeLeft;
        relativeY -= currentParent.relativeTop;
        currentParent = currentParent.parent
      }

      const apiControl = yield this.architectApi.createSectionEditorItem({
        editorSchemaItemId: this.editorNode.origamId,
        parentControlSetItemId: parent.id,
        componentType: this.surface.draggedComponentData!.type,
        fieldName: this.surface.draggedComponentData!.identifier,
        top: relativeY,
        left: relativeX
      });

      const newComponent = controlToComponent(apiControl, null);
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
    const updateResult = yield this.architectApi.updateSectionEditor({
      schemaItemId: this.toolbox.id,
      name: this.toolbox.name,
      selectedDataSourceId: this.toolbox.selectedDataSourceId,
      modelChanges: modelChanges
    });
    this.isDirty = updateResult.isDirty;
    const newData = updateResult.data;
    this.toolbox.name = newData.name;
    this.toolbox.selectedDataSourceId = newData.selectedDataSourceId;
    this.sectionToolbox.fields = newData.fields;
    this.surface.loadComponents(newData.rootControl);
  }
}

