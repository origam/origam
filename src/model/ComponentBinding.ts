import {
  IComponentBinding,
  IComponentBindingData,
  IComponentBindingPair,
  IComponentBindingPairData
} from "./types/IComponentBinding";
import { computed } from "mobx";
import { IDataView } from "./types/IDataView";
import { getFormScreen } from './selectors/FormScreen/getFormScreen';
import { getDataView } from './selectors/DataView/getDataView';

export class ComponentBindingPair implements IComponentBindingPair {
  constructor(data: IComponentBindingPairData) {
    Object.assign(this, data);
  }
  
  parent?: any;
  parentPropertyId: string = "";
  childPropertyId: string = "";
}

export class ComponentBinding implements IComponentBinding {

  constructor(data: IComponentBindingData) {
    Object.assign(this, data);
    this.bindingPairs.forEach(o => (o.parent = this));
  }
  parent?: any;

  parentId: string = "";
  parentEntity: string = "";
  childId: string = "";
  childEntity: string = "";
  childPropertyType: string = "";
  bindingPairs: IComponentBindingPair[] = [];

  @computed get parentDataView(): IDataView {
    const screen = getFormScreen(this);
    return screen.getDataViewByModelInstanceId(this.parentId)!
  }

  @computed get childDataView(): IDataView {
    const screen = getFormScreen(this);
    return screen.getDataViewByModelInstanceId(this.childId)!
  }
}
