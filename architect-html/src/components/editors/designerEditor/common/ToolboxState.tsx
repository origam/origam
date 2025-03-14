import { IDataSource } from "src/API/IArchitectApi.ts";
import { observable } from "mobx";
import { TabViewState } from "src/components/tabView/TabViewState.ts";

export class ToolboxState {
  @observable accessor name: string;
  @observable accessor selectedDataSourceId: string;
  @observable accessor isDirty: boolean = false;
  tabViewState: TabViewState = new TabViewState();
  onNameTopPropertiesChanged: (() => void) | undefined = undefined;

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
    return function*  (this: ToolboxState){
      this.selectedDataSourceId = value;
      yield * this.updateTopProperties()();
      if(this.onNameTopPropertiesChanged){
        this.onNameTopPropertiesChanged();
      }
    }.bind(this);
  }

  nameChanged(value: string) {
    return function*  (this: ToolboxState){
      this.name = value;
      yield * this.updateTopProperties()();
      if(this.onNameTopPropertiesChanged){
        this.onNameTopPropertiesChanged();
      }
    }.bind(this);
  }
}