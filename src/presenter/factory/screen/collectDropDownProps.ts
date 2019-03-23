import { findDropDownProps } from "./finders";

import * as ScreenXml from "src/common/types/IScreenXml";
import * as ScreenUIBp from "../../types/IUIScreenBlueprints";
import * as ScreenInfBp from "../../types/IInfScreenBlueprints";
import { parseNumber } from "src/util/xmlValues";

export function collectDropDownProps(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>,
  nextPhase: Array<() => void>
) {
  findDropDownProps(node)
    .filter((ch: any) => !exhs.has(ch))
    .forEach((ch: ScreenXml.IDropDownProperty) => {
      const attrs = ch.attributes;
      const property: ScreenInfBp.IDropDownProperty = {
        column: attrs.Column,
        entity: attrs.Entity,
        id: attrs.Id,
        name: attrs.Name,
        index: parseNumber(attrs.Index)!
      };
      reprs.set(ch, property);
      exhs.add(ch);
    });
}
