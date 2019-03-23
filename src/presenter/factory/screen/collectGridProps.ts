import { findGridProps } from "./finders";
import * as ScreenXml from "src/common/types/IScreenXml";
import * as ScreenUIBp from "../../types/IUIScreenBlueprints";
import * as ScreenInfBp from "../../types/IInfScreenBlueprints";
import { parseBoolean, parseNumber } from "src/util/xmlValues";

export function collectGridProps(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>,
  infReprs: Map<any, any>,
  infExhs: Set<any>,
  nextPhase: Array<() => void>
) {
  findGridProps(node)
    .filter((ch: any) => !infExhs.has(ch))
    .forEach((ch: ScreenXml.IGridProperty) => {
      const attrs = ch.attributes;
      const property: ScreenInfBp.IGridProperty = {
        allowReturnToForm: parseBoolean(attrs.AllowReturnToForm),
        autoSort: parseBoolean(attrs.AutoSort),
        cached: parseBoolean(attrs.Cached),
        captionLength: parseNumber(attrs.CaptionLength),
        captionPosition: attrs.CaptionPosition as any, // TODO: Check the value?
        column: attrs.Column,
        entity: attrs.Entity,
        entityName: attrs.EntityName,
        height: parseNumber(attrs.Height),
        id: attrs.Id,
        identifier: attrs.Identifier,
        identifierIndex: parseNumber(attrs.IdentifierIndex),
        isTree: parseBoolean(attrs.IsTree),
        lookupId: attrs.LookupId,
        modelInstanceId: attrs.ModelInstanceId,
        name: attrs.Name,
        readOnly: parseBoolean(attrs.ReadOnly),
        searchByFirstColumnOnly: parseBoolean(attrs.SearchByFirstColumnOnly),
        width: parseNumber(attrs.Width),
        top: parseNumber(attrs.Y),
        left: parseNumber(attrs.X)
      };
      infReprs.set(ch, property);
      infExhs.add(ch);
    });
}
