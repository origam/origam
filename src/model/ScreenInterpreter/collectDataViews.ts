import { findDataViews, findGridPropsStopping } from "./finders";

import { IPropertyParam, IDataViewParam } from "../types/ModelParam";
import { IGrid } from "src/common/types/IScreenXml";

export function collectDataViews(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>,
  nextPhase: Array<() => void>
) {
  findDataViews(node).forEach((n: IGrid) => {
    const dataView: IDataViewParam = {
      id: n.attributes.Id,
      properties: []
    };
    reprs.set(n, dataView);
    exhs.add(n);
    nextPhase.push(() => {
      findGridPropsStopping(n).forEach((p: any) => {
        const repr: IPropertyParam = reprs.get(p);
        dataView.properties.push(repr);
      });
    });
  });
}
