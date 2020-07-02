import {Container} from "dic/Container";
import {ITablePerspectiveDirector, TablePerspectiveDirector} from "./TablePerspectiveDirector";
import {ITablePerspective, TablePerspective} from "./TablePerspective";

export const SCOPE_TablePerspective = "TablePerspective";

export function register($cont: Container) {
  $cont
    .registerClass(ITablePerspectiveDirector, TablePerspectiveDirector)
    .scopedInstance(SCOPE_TablePerspective);

  $cont.registerClass(ITablePerspective, TablePerspective).scopedInstance(SCOPE_TablePerspective);
}
