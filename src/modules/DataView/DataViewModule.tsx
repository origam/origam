import React from "react";

import { Container } from "dic/Container";
import {
  IDataViewBodyUI,
  DataViewBodyUI,
  IDataViewToolbarUI,
  DataViewToolbarUI,
} from "./DataViewUI";
import * as PerspectiveModule from "./Perspective/PerspectiveModule";
import { IViewConfiguration, ViewConfiguration } from "./ViewConfiguration";

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
