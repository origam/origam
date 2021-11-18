import { Container } from "dic/Container";
import * as PerspectiveModule from "modules/DataView/Perspective/PerspectiveModule";
import * as DataViewModule from "modules/DataView/DataViewModule";
import * as LookupModule from "modules/Lookup/LookupModule";
import * as ScreenModule from "modules/Screen/ScreenModule";

export function registerModules($cont: Container) {
  DataViewModule.register($cont);
  PerspectiveModule.register($cont);
  LookupModule.register($cont);
  ScreenModule.register($cont);
}
