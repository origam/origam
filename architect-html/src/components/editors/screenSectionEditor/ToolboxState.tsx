import {
  IArchitectApi,
  IDataSource,
  IEditorField,
  ISectionEditorData, ISectionEditorModel
} from "src/API/IArchitectApi.ts";
import { observable } from "mobx";
import { TabViewState } from "src/components/tabView/TabViewState.ts";

export class ToolboxState {
  dataSources: IDataSource[];
  @observable accessor name: string;
  id: string;
  schemaExtensionId: string;
  @observable accessor selectedDataSourceId: string;
  @observable accessor fields: IEditorField[];
  @observable accessor isDirty: boolean = false;
  tabViewState: TabViewState = new TabViewState();

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
    return function* (this: ToolboxState): Generator<Promise<ISectionEditorModel>, void, ISectionEditorModel> {
      const updateResult = yield this.architectApi.updateScreenEditor({
        schemaItemId: this.id,
        name: this.name,
        selectedDataSourceId: this.selectedDataSourceId,
        modelChanges: []
      });
      const newData = updateResult.data;
      this.name = newData.name;
      this.selectedDataSourceId = newData.selectedDataSourceId;
      this.fields = newData.fields;
    }.bind(this);
  }
}