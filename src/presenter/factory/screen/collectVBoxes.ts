import { findVBoxes } from "./finders";
import { findAllStopping } from "src/util/xmlObj";
import { parseNumber } from "src/util/xmlValues";
import * as ScreenXml from "src/common/types/IScreenXml";
import * as ScreenUIBp from "../../types/IUIScreenBlueprints";
import * as ScreenInfBp from "../../types/IInfScreenBlueprints";

export function collectVBoxes(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>,
  nextPhase: Array<() => void>
) {
  findVBoxes(node)
    .filter((uiVB: any) => !exhs.has(uiVB))
    .forEach((uiVB: any) => {
      const vbox: ScreenUIBp.IUIVBox = {
        type: "VBox",
        props: {
          height: parseNumber(uiVB.attributes.Height)
        },
        children: []
      };
      exhs.add(uiVB);
      reprs.set(uiVB, vbox);
      nextPhase.push(() => {
        findAllStopping(
          uiVB,
          (cn: any) => cn !== uiVB && reprs.has(cn)
        ).forEach((cn: any) => {
          vbox.children.push(reprs.get(cn));
        });
      });
    });
}
