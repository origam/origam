import { findTabbedPanels, findChildBoxes } from "./finders";
import { findAllStopping } from "src/util/xmlObj";
import * as ScreenXml from "src/common/types/IScreenXml";
import * as ScreenUIBp from "../../types/IUIScreenBlueprints";
import * as ScreenInfBp from "../../types/IInfScreenBlueprints";

export function collectTabbedPanels(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>,
  nextPhase: Array<() => void>
) {
  findTabbedPanels(node)
    .filter((uiTP: any) => !exhs.has(uiTP))
    .forEach((uiTP: any) => {
      const panels: ScreenUIBp.IPanelDef[] = [];
      findChildBoxes(uiTP).forEach((pb: any) => {
        const panel: ScreenUIBp.IPanelDef = {
          id: pb.attributes.Id,
          label: pb.attributes.Name,
          content: []
        };
        panels.push(panel);
        exhs.add(pb);
        reprs.set(pb, panel);
        nextPhase.push(() => {
          findAllStopping(pb, (cn: any) => cn !== pb && reprs.has(cn)).forEach(
            (cn: any) => {
              panel.content.push(reprs.get(cn));
            }
          );
        });
      });
      const resultTP: ScreenUIBp.IUITabbedPanel = {
        type: "TabbedPanel",
        props: {
          id: uiTP.attributes.Id,
          panels
        },
        children: []
      };
      exhs.add(uiTP);
      reprs.set(uiTP, resultTP);
    });
}
