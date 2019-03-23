import { findFormFields } from "./finders";
import * as ScreenXml from "src/common/types/IScreenXml";
import * as ScreenUIBp from "../../types/IUIScreenBlueprints";

export function collectFormFields(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>,
  nextPhase: Array<() => void>
) {
  findFormFields(node)
    .filter((ch: any) => !exhs.has(ch))
    .forEach((ch: any) => {
      const field: ScreenUIBp.IUIFormField  = {
        type: "FormField",
        props: {
          id: ch.elements[0].text
        },
        children: []
      };
      exhs.add(ch);
      reprs.set(ch, field);
    });
}