import { observable } from "mobx";
import {
  IArchitectApi,
  IEditorField,
  ISectionEditorData,
  ISectionEditorModel
} from "src/API/IArchitectApi.ts";
import {
  ToolboxState
} from "src/components/editors/designerEditor/common/ToolboxState.tsx";

export class SectionToolboxState {
  toolboxState: ToolboxState;
  @observable accessor fields: IEditorField[];
  @observable accessor selectedFieldName: string | undefined;

  constructor(
    sectionEditorData: ISectionEditorData,
    id: string,
    private architectApi: IArchitectApi
  ) {
    this.toolboxState = new ToolboxState(
      sectionEditorData.dataSources,
      sectionEditorData.name,
      sectionEditorData.schemaExtensionId,
      sectionEditorData.selectedDataSourceId,
      id,
      this.updateTopProperties.bind(this))
    this.fields = sectionEditorData.fields;
  }

  private updateTopProperties() {
    return function* (this: SectionToolboxState): Generator<Promise<ISectionEditorModel>, void, ISectionEditorModel> {
      const updateResult = yield this.architectApi.updateScreenEditor({
        schemaItemId: this.toolboxState.id,
        name: this.toolboxState.name,
        selectedDataSourceId: this.toolboxState.selectedDataSourceId,
        modelChanges: []
      });
      const newData = updateResult.data;
      this.toolboxState.name = newData.name;
      this.toolboxState.selectedDataSourceId = newData.selectedDataSourceId;
      this.fields = newData.fields;
    }.bind(this);
  }

}