import { IDataSource } from "src/API/IArchitectApi.ts";
import { observable } from "mobx";
import { TabViewState } from "src/components/tabView/TabViewState.ts";

export class ToolboxState {
  @observable accessor name: string;
  @observable accessor selectedDataSourceId: string;
  @observable accessor isDirty: boolean = false;
  tabViewState: TabViewState = new TabViewState();

  constructor(
    public dataSources: IDataSource[],
    name: string,
    public schemaExtensionId: string,
    selectedDataSourceId: string,
    public id: string,
    private updateTopProperties: ()=>()=>Generator<Promise<any>, void, any>,
  ) {
    this.name = name;
    this.selectedDataSourceId = selectedDataSourceId;
  }

  selectedDataSourceIdChanged(value: string) {
    this.selectedDataSourceId = value;
    return this.updateTopProperties();
  }

  nameChanged(value: string) {
    this.name = value;
    return this.updateTopProperties();
  }
}