import {
  IComponentBinding,
  IComponentBindingData,
  IComponentBindingPair,
  IComponentBindingPairData
} from "./types/IComponentBinding";

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
}
