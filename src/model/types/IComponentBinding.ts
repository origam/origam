import { IDataView } from "./IDataView";

export interface IComponentBindingData {
  parentId: string;
  parentEntity: string;
  childId: string;
  childEntity: string;
  childPropertyType: string;
  bindingPairs: IComponentBindingPair[];
}

export interface IComponentBinding extends IComponentBindingData {
  bindingController: Array<[string, any]>;
  parentDataView: IDataView;
  childDataView: IDataView;
  parent?: any;
}

export interface IComponentBindingPairData {
  parentPropertyId: string;
  childPropertyId: string;
}

export interface IComponentBindingPair extends IComponentBindingPairData {
  parent?: any;
}
