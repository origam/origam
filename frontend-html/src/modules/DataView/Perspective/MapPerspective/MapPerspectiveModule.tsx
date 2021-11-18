import {Container} from "dic/Container";
import {IMapPerspectiveDirector, MapPerspectiveDirector} from "./MapPerspectiveDirector";
import {IMapPerspective, MapPerspective} from "./MapPerspective";

export const SCOPE_MapPerspective = "MapPerspective";

export function register($cont: Container) {
  $cont
    .registerClass(IMapPerspectiveDirector, MapPerspectiveDirector)
    .scopedInstance(SCOPE_MapPerspective);

  $cont.registerClass(IMapPerspective, MapPerspective).scopedInstance(SCOPE_MapPerspective);
}
