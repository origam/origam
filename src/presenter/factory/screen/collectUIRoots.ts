import { findUIRoots } from "./finders";
import { findAllStopping } from "src/util/xmlObj";
import * as ScreenXml from "src/common/types/IScreenXml";
import * as ScreenUIBp from "../../types/IUIScreenBlueprints";
import * as ScreenInfBp from "../../types/IInfScreenBlueprints";

export function collectUIRoots(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>,
  nextPhase: Array<() => void>
) {
  findUIRoots(node)
    .filter((uiR: any) => !exhs.has(uiR))
    .forEach((uiR: any) => {
      const root: ScreenUIBp.IUIRoot = {
        type: "Root",
        props: {},
        children: []
      };
      exhs.add(uiR);
      reprs.set(uiR, root);
      nextPhase.push(() => {
        findAllStopping(uiR, (cn: any) => cn !== uiR && reprs.has(cn)).forEach(
          (cn: any) => {
            root.children.push(reprs.get(cn));
          }
        );
      });
    });
}
