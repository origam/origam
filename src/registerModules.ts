import { Container } from "dic/Container";
import * as PerspectiveModule from "modules/DataView/Perspective/PerspectiveModule";
import * as DataViewModule from "modules/DataView/DataViewModule";

export function registerModules($cont: Container) {
  DataViewModule.register($cont);
  PerspectiveModule.register($cont);
  
}
