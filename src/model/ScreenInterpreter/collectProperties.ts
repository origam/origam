import { findGridProps } from "./finders";

import { IPropertyParam } from "../types/ModelParam";
import { parseBoolean } from "src/util/xmlValues";
import { IGridProperty } from "src/common/types/IScreenXml";


export function collectProperties(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>,
  nextPhase: Array<() => void>
) {
  findGridProps(node).forEach((n: IGridProperty) => {
    const property: IPropertyParam = {
      id: n.attributes.Id,
      name: n.attributes.Name,
      entity: n.attributes.Entity,
      column: n.attributes.Column,
      isReadOnly: parseBoolean(n.attributes.ReadOnly)
    };
    reprs.set(n, property);
    exhs.add(n);
  });
}
