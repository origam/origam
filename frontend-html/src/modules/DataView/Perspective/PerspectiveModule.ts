import {Container} from "dic/Container";

import * as MapPerspectiveModule from './MapPerspective/MapPerspectiveModule';
import * as FormPerspectiveModule from './FormPerspective/FormPerspectiveModule';
import * as TablePerspectiveModule from './TablePerspective/TablePerspectiveModule';
import {IPerspective, Perspective} from "./Perspective";
import {SCOPE_DataView} from "../DataViewModule";

export function register($cont: Container) {
  $cont.registerClass(IPerspective, Perspective).scopedInstance(SCOPE_DataView);

  FormPerspectiveModule.register($cont);
  MapPerspectiveModule.register($cont);
  TablePerspectiveModule.register($cont);
}