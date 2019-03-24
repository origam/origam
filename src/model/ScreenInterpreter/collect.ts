import { collectDataViews } from "./collectDataViews";
import { collectProperties } from "./collectProperties";
import { findAllStopping } from "src/util/xmlObj";

export function collectElements(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>
) {
  console.time("collect(Model)");
  const nextPhase: Array<() => void> = [];
  collectDataViews(node, reprs, exhs, nextPhase);
  collectProperties(node, reprs, exhs, nextPhase);
  nextPhase.forEach(nph => nph());
  console.timeEnd("collect(Model)");
  console.log(reprs);
  const rootOrigNode = findAllStopping(node, (cn: any) => reprs.has(cn));
  return rootOrigNode.map((ron: any) => reprs.get(ron));
}
