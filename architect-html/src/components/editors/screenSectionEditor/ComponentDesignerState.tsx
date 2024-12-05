import { observable } from "mobx";
import { IEditorState } from "src/components/editorTabView/IEditorState.ts";
import {
  IEditorNode
} from "src/components/editorTabView/EditorTabViewState.ts";
import {
  IArchitectApi,
  IDeleteResult,
  ISectionEditorData, ISectionEditorModel,
} from "src/API/IArchitectApi.ts";
import { toChanges } from "src/components/editors/gridEditor/EditorProperty.ts";
import {
  DesignSurfaceState
} from "src/components/editors/screenSectionEditor/DesignSurfaceState.tsx";
import {
  ToolboxState
} from "src/components/editors/screenSectionEditor/ToolboxState.tsx";

export class ComponentDesignerState implements IEditorState {

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

  constructor(
    private editorNode: IEditorNode,
    isDirty: boolean,
    sectionEditorData: ISectionEditorData,
    private architectApi: IArchitectApi
  ) {
    this.isDirty = isDirty;
    this.toolbox = new ToolboxState(sectionEditorData, editorNode.origamId, architectApi);
    this.surface = new DesignSurfaceState(
      sectionEditorData, architectApi, this.editorNode.origamId, (value)=> this.isDirty = value);
  }

  deleteComponent(id: string) {
    return function* (this: ComponentDesignerState): Generator<Promise<IDeleteResult>, void, IDeleteResult> {
      const newData = yield this.architectApi.deleteScreenEditorItem({
        editorSchemaItemId: this.toolbox.id,
        schemaItemId: id
      });
      this.surface.loadComponents(newData.rootControl);
      this.isDirty = true;
    }.bind(this);
  }

  onDesignerMouseUp(x: number, y: number) {
    return function* (this: ComponentDesignerState) {
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

  private* updateScreenEditor(): Generator<Promise<ISectionEditorModel>, void, ISectionEditorModel> {
    const modelChanges = this.surface.components.map(x => {
        return {
          schemaItemId: x.id,
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
    this.toolbox.fields = newData.fields;
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
      return LabelPosition[value as any] as any;
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

