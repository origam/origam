import { findLabels } from "./finders";
import { parseNumber } from "src/util/xmlValues";
import * as ScreenXml from "src/common/types/IScreenXml";
import * as ScreenUIBp from "../../types/IUIScreenBlueprints";
import * as ScreenInfBp from "../../types/IInfScreenBlueprints";

export function collectLabels(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>,
  nextPhase: Array<() => void>
) {
  findLabels(node)
    .filter((uiL: any) => !exhs.has(uiL))
    .forEach((uiL: any) => {
      const label: ScreenUIBp.IUILabel = {
        type: "Label",
        props: {
          height: parseNumber(uiL.attributes.Height),
          text: uiL.attributes.Name
        },
        children: []
      };
      exhs.add(uiL);
      reprs.set(uiL, label);
    });
}
