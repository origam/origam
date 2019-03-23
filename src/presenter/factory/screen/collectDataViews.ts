import {
  findDataViews,
  findGridPropsStopping,
  findFormFields,
  findFormRootsStopping
} from "./finders";
import * as ScreenXml from "src/common/types/IScreenXml";
import * as ScreenUIBp from "../../types/IUIScreenBlueprints";
import * as ScreenInfBp from "../../types/IInfScreenBlueprints";
import { parseBoolean } from "src/util/xmlValues";

export function collectDataViews(
  node: any,
  reprs: Map<any, any>,
  exhs: Set<any>,
  infReprs: Map<any, any>,
  infExhs: Set<any>,
  nextPhase: Array<() => void>
) {
  findDataViews(node)
    .filter((uiDV: any) => !exhs.has(uiDV))
    .forEach((uiDV: ScreenXml.IGrid) => {
      const dataView: ScreenUIBp.IUIDataView = {
        type: "DataView",
        props: {
          id: uiDV.attributes.Id,
          height: uiDV.attributes.Height
            ? parseInt(uiDV.attributes.Height, 10)
            : undefined
        },
        children: []
      };
      exhs.add(uiDV);
      reprs.set(uiDV, dataView);
      const infDataView: ScreenInfBp.IDataView = {
        id: uiDV.attributes.Id,
        isHeadless: parseBoolean(uiDV.attributes.IsHeadless),
        initialView: uiDV.attributes.DefaultPanelView,
        availableViews: [],
        properties: [],
        propertiesMap: new Map()
      };
      infExhs.add(uiDV);
      infReprs.set(uiDV, infDataView);
      nextPhase.push(() => {
        findGridPropsStopping(uiDV).forEach((ch: any) => {
          const repr = infReprs.get(ch)! as ScreenInfBp.IGridProperty;
          infDataView.properties.push(repr);
          infDataView.propertiesMap.set(repr.id, repr);
        });
        findFormFields(uiDV).forEach((ch: any) => {
          const formField = reprs.get(ch)! as ScreenUIBp.IUIFormField;
          const property = infDataView.propertiesMap.get(formField.props.id)!;
          formField.props.name = property.name;
          formField.props.captionLength = property.captionLength;
          formField.props.captionPosition = property.captionPosition;
          formField.props.column = property.column;
          formField.props.entity = property.entity;
          formField.props.height = property.height;
          formField.props.width = property.width;
          formField.props.top = property.top;
          formField.props.left = property.left;
        });
        findFormRootsStopping(uiDV).forEach((ch: any) => {
          const repr = reprs.get(ch)!;
          const formView: ScreenInfBp.IFormView = {
            type: "FormView",
            uiStructure: repr,
            isHeadless: parseBoolean(uiDV.attributes.IsHeadless)
          };
          // TODO: Has to be driven by grid
          infDataView.availableViews.push(formView);
          const tableView: ScreenInfBp.ITableView = {
            type: "TableView",
            properties: infDataView.properties
          };
          infDataView.availableViews.push(tableView);
        });
      });
    });
}
