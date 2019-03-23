import { findHSplits } from "./finders";
import { findAllStopping } from "src/util/xmlObj";
import * as ScreenXml from "src/common/types/IScreenXml";
import * as ScreenUIBp from "../../types/IUIScreenBlueprints";
import * as ScreenInfBp from "../../types/IInfScreenBlueprints";

export function collectHSplits(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>,
  nextPhase: Array<() => void>
) {
  findHSplits(node)
    .filter((uiHS: any) => !exhs.has(uiHS))
    .forEach((uiHS: any) => {
      const hsplit: ScreenUIBp.IUIHSplit = {
        type: "HSplit",
        props: {
          id: uiHS.attributes.Id
        },
        children: []
      };
      exhs.add(uiHS);
      reprs.set(uiHS, hsplit);
      nextPhase.push(() => {
        findAllStopping(
          uiHS,
          (cn: any) => cn !== uiHS && reprs.has(cn)
        ).forEach((cn: any) => {
          hsplit.children.push(reprs.get(cn));
        });
      });
    });
}
