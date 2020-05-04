import { Container } from "dic/Container";
import { IFormPerspectiveDirector, FormPerspectiveDirector } from "./FormPerspectiveDirector";
import { IFormPerspective, FormPerspective } from "./FormPerspective";

export const SCOPE_FormPerspective = "FormPerspective";

export function register($cont: Container) {
  $cont
    .registerClass(IFormPerspectiveDirector, FormPerspectiveDirector)
    .scopedInstance(SCOPE_FormPerspective);

  $cont.registerClass(IFormPerspective, FormPerspective).scopedInstance(SCOPE_FormPerspective);
}
