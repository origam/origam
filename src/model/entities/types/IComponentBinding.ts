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
  $type_IComponentBinding: 1;

  bindingController: Array<[string, any]>;
  parentDataView: IDataView;
  childDataView: IDataView;
  isBindingControllerValid: boolean;
  parent?: any;
}

export interface IComponentBindingPairData {
  parentPropertyId: string;
  childPropertyId: string;
}

export interface IComponentBindingPair extends IComponentBindingPairData {
  parent?: any;
}

export const isIComponentBinding = (o: any): o is IComponentBinding =>
  o.$type_IComponentBinding;
