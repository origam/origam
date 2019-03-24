import { findWindows, findDataViewsStopping, findTabbedPanels } from "./finders";
import * as ScreenXml from "src/common/types/IScreenXml";
import * as ScreenUIBp from "../../types/IUIScreenBlueprints";
import * as ScreenInfBp from "../../types/IInfScreenBlueprints";
import { findFirstDFS } from "src/util/xmlObj";

export function collectWindows(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>,
  infReprs: Map<any, any>,
  infExhs: Set<any>,
  nextPhase: Array<() => void>
) {
  findWindows(node)
    .filter((ch: any) => !exhs.has(ch))
    .forEach((ch: any) => {
      const window: ScreenInfBp.IScreen = {
        cardTitle: ch.attributes.Title,
        screenTitle: ch.attributes.Title,
        uiStructure: [],
        dataViewsMap: new Map(),
        tabPanelsMap: new Map()
      };
      exhs.add(ch);
      reprs.set(ch, window);
      nextPhase.push(() => {
        const root = findFirstDFS(ch, (n: any) => n.name === "UIRoot");
        const rootRepr = reprs.get(root)!;
        window.uiStructure.push(rootRepr);
        findDataViewsStopping(ch).forEach((dv: any) => {
          const repr = infReprs.get(dv)!;
          window.dataViewsMap.set(repr.id, repr);
        });
        findTabbedPanels(ch).forEach((tp: any) => {
          const repr = reprs.get(tp)!;
          const tabbedPanel: ScreenInfBp.ITabbedPanel = {
            activeTabId: repr.props.panels[0].id
          }
          window.tabPanelsMap.set(repr.props.id, tabbedPanel)
        })
      });
    });
}