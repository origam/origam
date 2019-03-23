import { findVSplits } from "./finders";
import * as ScreenXml from "src/common/types/IScreenXml";
import * as ScreenUIBp from "../../types/IUIScreenBlueprints";
import * as ScreenInfBp from "../../types/IInfScreenBlueprints";
import { findAllStopping } from "src/util/xmlObj";

export function collectVSplits(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>,
  nextPhase: Array<() => void>
) {
  findVSplits(node)
    .filter((uiVS: any) => !exhs.has(uiVS))
    .forEach((uiVS: any) => {
      const vsplit: ScreenUIBp.IUIVSplit = {
        type: "VSplit",
        props: {
          id: uiVS.attributes.Id
        },
        children: []
      };
      exhs.add(uiVS);
      reprs.set(uiVS, vsplit);
      nextPhase.push(() => {
        findAllStopping(
          uiVS,
          (cn: any) => cn !== uiVS && reprs.has(cn)
        ).forEach((cn: any) => {
          vsplit.children.push(reprs.get(cn));
        });
      });
    });
}
