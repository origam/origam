import { Properties } from "../../DataView/DataTable";
import { IScreenContentFactory, IFormScreen } from "./types";
import * as xmlFind from "./xmlFind";
import { parseViewType, parseBoolean, parseNumber } from "../../utils/xml";
import { buildProperty } from "../../DataView/Property";
import { DataSource } from "../DataSource";
import { DataSourceField } from "../DataSourceField";
import { ML } from "../../utils/types";
import { IApi } from "../../Api/IApi";
import { unpack } from "../../utils/objects";
import { DataSources } from "../DataSources";
import { buildDataView } from "../../DataView/buildDataView";
import { buildFormView } from "../../DataView/FormView/buildFormView";
import { buildTableView } from "../../DataView/TableView/buildTableView";
import { IDataViewMediator02 } from "../../DataView/DataViewMediator02";

export class ScreenContentFactory implements IScreenContentFactory {
  constructor(public P: { formScreen: IFormScreen}) {}

  create(xmlObj: any) {
    // console.time("xml-processing");
    const win = xmlFind.findWindow(xmlObj);
    const xmlDataSources = xmlFind.findDataSources(win);
    const dataSources = new DataSources({
      sources: xmlDataSources.map((xmlDataSource: any) => {
        const xmlDataSourceFields = xmlFind.findDataSourceFields(xmlDataSource);
        return new DataSource({
          id: xmlDataSource.attributes.Entity,
          dataStructureEntityId: xmlDataSource.attributes.DataStructureEntityId,
          fields: xmlDataSourceFields.map((xmlDataSourceField: any) => {
            return new DataSourceField({
              id: xmlDataSourceField.attributes.Name,
              idx: parseNumber(xmlDataSourceField.attributes.Index)
            });
          })
        });
      })
    });
    // console.log(dataSources);

    const grids = xmlFind.findGrids(win);
    const dataViewByModelInstanceId = new Map<string, IDataViewMediator02>();
    const dataViews = grids.map(grid => {
      const gridProps = xmlFind.findProps(grid);
      for (let gridProp of gridProps) {
        const dropDownProps = xmlFind.findDropDownProps(gridProp);
        if (dropDownProps.length > 0) {
        }
      }
      const gridFormRoot = xmlFind.findFormRoot(grid);
      const formUI = extractFormUI(gridFormRoot, gridProps);
      // --------------------------------------------------------

      const dataSource = dataSources.getByEntityName(grid.attributes.Entity);
      const dataStructureEntityId = dataSource
        ? dataSource.dataStructureEntityId
        : "";

      // const mediator = new DataViewMediator();

      const dataViewMediator02: IDataViewMediator02 = buildDataView(
        grid.attributes.Id,
        grid.attributes.Name,
        parseBoolean(grid.attributes.IsHeadless),
        dataStructureEntityId,
        dataSource!,
        () => availViewItems,
        () => propertyItems,
        parseViewType(grid.attributes.DefaultPanelView)!,
        this.P.formScreen
      );

      const formView = buildFormView(undefined, formUI, dataViewMediator02);
      const tableView = buildTableView(undefined, dataViewMediator02);

      const availViewItems = [formView, tableView];

      /*const dataView: DataView = new DataView({
        id: () => grid.attributes.Id,
        label: grid.attributes.Name,
        menuItemId: this.P.menuItemId,
        dataStructureEntityId,
        initialDataView: parseViewType(grid.attributes.DefaultPanelView)!,
        isHeadless: parseBoolean(grid.attributes.IsHeadless),
        specificDataViews: () => specificDataViews,
        properties: () => properties,
        dataSource: () => dataSource!,
        mediator,
        api: this.P.api
      });*/

      dataViewByModelInstanceId.set(
        grid.attributes.ModelInstanceId,
        dataViewMediator02
      );
      const properties: Properties = new Properties({
        propertyItems: () => propertyItems
      });

      /*const tableView = new TableView({
        dataView: () => dataView,
        mediator
      });
      const formView = new FormView({
        dataView: () => dataView,
        uiStructure: () => formUI
      });*/

      // const specificDataViews = [formView, tableView];

      const propertyItems = gridProps.map((gp, idx) => {
        const dataSourceField = dataSource
          ? dataSource.fieldById(gp.attributes.Id)
          : undefined;

        const dataSourceIndex = dataSourceField
          ? dataSourceField.idx
          : undefined;

        if (dataSourceIndex === undefined) {
          throw new Error("DataSourceIndex is 0");
        }
        const prop = buildProperty(
          gp,
          idx,
          dataSourceIndex,
          unpack(this.P.formScreen.menuItemId),
          unpack(this.P.formScreen.api)
        );
        return prop;
      });
      tableView.propReorder.setIds(
        properties.items.map(prop => prop.id).filter(id => id !== "Id")
      );
      formView.propReorder.setIds(
        properties.items.map(prop => prop.id).filter(id => id !== "Id")
      );
      return dataViewMediator02;
    });

    const screenBindings = xmlFind.findBindings(win);
    for (let screenBinding of screenBindings) {
      const parentDataView = dataViewByModelInstanceId.get(
        screenBinding.attributes.ParentId
      );
      const childDataView = dataViewByModelInstanceId.get(
        screenBinding.attributes.ChildId
      );
      if (parentDataView && childDataView) {
        /*console.log(
          "Connecting parent to child:",
          screenBinding.attributes.ParentId,
          screenBinding.attributes.ChildId
        );*/
        parentDataView.machine.addChild(childDataView.machine);
        childDataView.machine.setParent(parentDataView.machine);
        parentDataView.machine.controllingFieldId =
          screenBinding.attributes.ParentProperty;
        childDataView.machine.controlledFieldId =
          screenBinding.attributes.ChildProperty;
      }
    }

    const uiRoot = xmlFind.findUIRoot(win);
    const screenUI = extractScreenUI(uiRoot);
    const isSessioned = xmlFind.isSessionedScreen(win);
    // -----------------------------------------------------------
    // console.log(dataViews)
    // console.timeEnd("xml-processing");
    return {
      screenUI,
      dataViews,
      isSessioned
    };
  }


}

function extractScreenUI(node: any) {
  function recursive(n: any) {
    const element: IElement = {
      type: n.attributes.Type,
      props: n.attributes,
      children: []
    };
    const lastElement = stack.slice(-1)[0];
    if (lastElement) {
      lastElement.children.push(element);
    }
    stack.push(element);
    const children = xmlFind.findScreenElements(n);
    level++;
    for (let chn of children) {
      recursive(chn);
    }
    level--;
    stack.pop();
  }
  const stack: IElement[] = [{ type: "DUMMY", props: {}, children: [] }];
  let level = 0;
  recursive(node);
  return stack[0].children;
}

function extractFormUI(node: any, grprops: any[]) {
  function recursive(n: any, tarEl: IElement) {
    let type = "";
    let props = {};
    if (n.name === "FormRoot") {
      type = "FormRoot";
      props = n.attributes;
    } else if (n.type === "text" && n.parent.name === "string") {
      type = "Property";
      props = propMap.get(n.text)!.attributes;
    } else if (n.attributes.Type === "FormSection") {
      type = "FormSection";
      props = n.attributes;
    }
    const element: IElement = {
      type,
      props,
      children: []
    };
    tarEl.children.push(element);
    const children = xmlFind.findFormElements(n);
    level++;
    for (let chn of children) {
      recursive(chn, element);
    }
    level--;
  }
  const propMap = new Map(
    grprops.map(p => [p.attributes.Id, p] as [string, any])
  );
  let level = 0;
  const dummyel = { type: "DUMMY", props: {}, children: [] };
  recursive(node, dummyel);
  return dummyel.children;
}

interface IElement {
  type: string;
  props: any;
  children: any[];
}
