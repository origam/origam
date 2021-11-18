import {Container} from "dic/Container";
import {DataViewBodyUI, DataViewToolbarUI, IDataViewBodyUI, IDataViewToolbarUI,} from "./DataViewUI";
import * as PerspectiveModule from "./Perspective/PerspectiveModule";

export const SCOPE_DataView = "DataView";

export function register($cont: Container) {
  $cont.registerClass(IDataViewBodyUI, DataViewBodyUI).scopedInstance(SCOPE_DataView);
  $cont.registerClass(IDataViewToolbarUI, DataViewToolbarUI).scopedInstance(SCOPE_DataView);
  // $cont.registerClass(IViewConfiguration, ViewConfiguration);
  PerspectiveModule.register($cont);
}

export function beginScope($cont: Container) {
  return $cont.beginLifetimeScope(SCOPE_DataView);
}
