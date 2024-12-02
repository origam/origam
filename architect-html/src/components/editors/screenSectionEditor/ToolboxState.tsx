import {
  IArchitectApi,
  IDataSource,
  IEditorField,
  ISectionEditorData
} from "src/API/IArchitectApi.ts";
import { observable } from "mobx";

export class ToolboxState {
  dataSources: IDataSource[];
  @observable accessor name: string;
  id: string;
  schemaExtensionId: string;
  @observable accessor selectedDataSourceId: string;
  @observable accessor fields: IEditorField[];
  @observable accessor isDirty: boolean = false;

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
    return function* (this: ToolboxState): Generator<Promise<ISectionEditorData>, void, ISectionEditorData> {
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