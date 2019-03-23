import { findFormSections, getNearestReprs } from "./finders";
import * as ScreenXml from "src/common/types/IScreenXml";
import * as ScreenUIBp from "../../types/IUIScreenBlueprints";
import * as ScreenInfBp from "../../types/IInfScreenBlueprints";
import { parseNumber } from "src/util/xmlValues";

export function collectFormSections(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>,
  nextPhase: Array<() => void>
) {
  findFormSections(node)
    .filter((ch: any) => !exhs.has(ch))
    .forEach((ch: any) => {
      const formSection: ScreenUIBp.IUIFormSection = {
        type: "FormSection",
        props: {
          top: parseNumber(ch.attributes.Y)!,
          left: parseNumber(ch.attributes.X)!,
          width: parseNumber(ch.attributes.Width)!,
          height: parseNumber(ch.attributes.Height)!,
          title: ch.attributes.Title
        },
        children: []
      };
      reprs.set(ch, formSection);
      exhs.add(ch);
      nextPhase.push(() => {
        formSection.children.push(...getNearestReprs(ch, reprs));
      });
    });
}
