import { collectFormFields } from "./collectFormFields";
import { collectGridProps } from "./collectGridProps";
import { collectDropDownProps } from "./collectDropDownProps";
import { collectFormRoots } from "./collectFormRoots";
import { collectFormSections } from "./collectFormSections";
import { collectTabbedPanels } from "./collectTabbedPanels";
import { collectBoxes } from "./collectBoxes";
import { collectVBoxes } from "./collectVBoxes";
import { collectLabels } from "./collectLabels";
import { collectHSplits } from "./collectHSplits";
import { collectVSplits } from "./collectVSplits";
import { collectDataViews } from "./collectDataViews";
import { collectUIRoots } from "./collectUIRoots";
import { collectWindows } from "./collectWindows";
import { findFirstBFS, findAll } from "src/util/xmlObj";


export function collectElements(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>,
  infReprs: Map<any, any>,
  infExhs: Set<any>
) {
  const nextPhase: Array<() => void> = [];
  collectFormFields(node, reprs, exhs, nextPhase);
  collectGridProps(node, reprs, exhs, infReprs, infExhs, nextPhase);
  collectDropDownProps(node, reprs, exhs, nextPhase);
  collectFormRoots(node, reprs, exhs, nextPhase);
  collectFormSections(node, reprs, exhs, nextPhase);
  collectTabbedPanels(node, reprs, exhs, nextPhase);
  collectBoxes(node, reprs, exhs, nextPhase);
  collectVBoxes(node, reprs, exhs, nextPhase);
  collectLabels(node, reprs, exhs, nextPhase);
  collectHSplits(node, reprs, exhs, nextPhase);
  collectVSplits(node, reprs, exhs, nextPhase);
  collectDataViews(node, reprs, exhs, infReprs, infExhs, nextPhase);
  collectUIRoots(node, reprs, exhs, nextPhase);
  collectWindows(node, reprs, exhs, infReprs, infExhs, nextPhase);
  nextPhase.forEach(nph => nph());
  nextPhase.length = 0;
  showUncollectedUIElements(node, reprs, exhs);

  const rootOrigNode = findFirstBFS(node, (cn: any) => reprs.has(cn));
  return reprs.get(rootOrigNode);
}

function showUncollectedUIElements(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>
) {
  const nodes = findAll(node, (ch: any) => !exhs.has(ch)).filter(
    (ch: any) => ch.name === "UIElement"
  );
  console.log(nodes);
}


