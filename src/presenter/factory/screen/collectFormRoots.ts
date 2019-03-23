import { findFormRoots, getNearestReprs } from "./finders";
import * as ScreenXml from "src/common/types/IScreenXml";
import * as ScreenUIBp from "../../types/IUIScreenBlueprints";
import * as ScreenInfBp from "../../types/IInfScreenBlueprints";

export function collectFormRoots(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>,
  nextPhase: Array<() => void>
) {
  findFormRoots(node)
    .filter((ch: any) => !exhs.has(ch))
    .forEach((ch: any) => {
      const formRoot: ScreenUIBp.IUIFormRoot = {
        type: "FormRoot",
        props: {},
        children: []
      };
      reprs.set(ch, formRoot);
      exhs.add(ch);
      nextPhase.push(() => {
        formRoot.children.push(...getNearestReprs(ch, reprs));
      });
    });
}
