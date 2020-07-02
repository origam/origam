import {
  IComponentBinding,
  IComponentBindingData,
  IComponentBindingPair,
  IComponentBindingPairData
} from "./types/IComponentBinding";
import {computed} from "mobx";
import {IDataView} from "./types/IDataView";
import {getFormScreen} from "../selectors/FormScreen/getFormScreen";
import {getDataTable} from "../selectors/DataView/getDataTable";

export class ComponentBindingPair implements IComponentBindingPair {
  constructor(data: IComponentBindingPairData) {
    Object.assign(this, data);
  }

  parent?: any;
  parentPropertyId: string = "";
  childPropertyId: string = "";
}

export class ComponentBinding implements IComponentBinding {
  $type_IComponentBinding: 1 = 1;
  
  constructor(data: IComponentBindingData) {
    Object.assign(this, data);
    this.bindingPairs.forEach(o => (o.parent = this));
  }

  parentId: string = "";
  parentEntity: string = "";
  childId: string = "";
  childEntity: string = "";
  childPropertyType: string = "";
  bindingPairs: IComponentBindingPair[] = [];

  @computed get parentDataView(): IDataView {
    const screen = getFormScreen(this);
    return screen.getDataViewByModelInstanceId(this.parentId)!;
  }

  @computed get childDataView(): IDataView {
    const screen = getFormScreen(this);
    return screen.getDataViewByModelInstanceId(this.childId)!;
  }

  @computed get bindingController() {
    const c: Array<[string, any]> = [];
    const parentDataTable = getDataTable(this.parentDataView);
    for (let pair of this.bindingPairs) {
      const row = this.parentDataView.selectedRow;
      const property = parentDataTable.getPropertyById(pair.parentPropertyId);
      if (row && property) {
        c.push([
          pair.childPropertyId,
          parentDataTable.getCellValue(row, property)
        ]);
      }
    }
    return c;
  }

  @computed get isBindingControllerValid() {
    return this.bindingController.every(pair => pair[0] && pair[1])
  }

  parent?: any;
}
