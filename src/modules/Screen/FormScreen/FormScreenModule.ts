import { Container } from "dic/Container";
import { IScreenEvents, ScreenEvents } from "./ScreenEvents";

export const SCOPE_FormScreen = "FormScreen";

export function register($cont: Container) {
  $cont.registerClass(IScreenEvents, ScreenEvents).scopedInstance(SCOPE_FormScreen);
}