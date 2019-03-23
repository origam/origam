import { findBoxes } from "./finders";
import { findAllStopping } from "src/util/xmlObj";
import * as ScreenXml from "src/common/types/IScreenXml";
import * as ScreenUIBp from "../../types/IUIScreenBlueprints";

export function collectBoxes(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>,
  nextPhase: Array<() => void>
) {
  findBoxes(node)
    .filter((uiB: any) => !exhs.has(uiB))
    .forEach((uiB: any) => {
      const box: ScreenUIBp.IUIBox = {
        type: "Box",
        props: {},
        children: []
      };
      exhs.add(uiB);
      reprs.set(uiB, box);
      nextPhase.push(() => {
        findAllStopping(uiB, (cn: any) => cn !== uiB && reprs.has(cn)).forEach(
          (cn: any) => {
            (box.children as any).push(reprs.get(cn));
          }
        );
      });
    });
}
