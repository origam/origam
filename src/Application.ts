import * as presenterCollect from "./presenter/factory/screen/collect";
import { ScreenFactory as PresenterScreenFactory } from "./presenter/factory/screen/ScreenFactory";

import * as modelCollect from "./model/ScreenInterpreter/collect";
import { ScreenFactory as ModelScreenFactory } from "./model/factory/ScreenFactory";

import { IScreenXml } from "./common/types/IScreenXml";
import { IModel } from "./model/types/IModel";
import { Records } from "./model/entities/data/Records";
import { Record } from "./model/entities/data/Record";

export function buildScreenPresenter(screenXml: IScreenXml, model: IModel) {
  const reprs = new Map();
  const exhs = new Set();
  const infReprs = new Map();
  const infExhs = new Set();
  const elements = presenterCollect.collectElements(
    screenXml,
    reprs,
    exhs,
    infReprs,
    infExhs
  );
  console.log(elements);
  const screenFactory = new PresenterScreenFactory(model);
  const screen = screenFactory.getScreen(elements);
  console.log(screen);
  return screen;
}

export function buildScreenModel(screenXml: IScreenXml) {
  const reprs = new Map();
  const exhs = new Set();
  const elements = modelCollect.collectElements(screenXml, reprs, exhs);
  console.log(elements);
  const screenFactory = new ModelScreenFactory();
  const screen = screenFactory.getScreen(elements);
  console.log(screen);
  return screen;
}

export function buildScreen(screenXml: IScreenXml) {
  console.log(screenXml);
  const model = buildScreenModel(screenXml);
  const presenter = buildScreenPresenter(screenXml, model);
  console.log(model);

  presenter.tabPanelsMap.get("AsTabControl1_2")!.activeTabId = "TabPage3_29";

  const dt01 = model.getDataTable({ dataViewId: "AsPanel9_30" })!;

  for (let i = 0; i < 100; i++) {
    const values = [];
    for (let j = 0; j < dt01.properties.count; j++) {
      values.push(`${i} & ${j}`);
    }
    (dt01.records as Records).items.push(new Record({ id: values[0], values }));
  }

  dt01.deleteRecordById("7 & 0");
  dt01.setDirtyValueById("3 & 0", "Length", "DIRTY_VALUE");

  const cursor = model.getCursor({ dataViewId: "AsPanel9_30" })!;
  cursor.selectCell("4 & 0", "Comment");
  // cursor.selectCellByIdx(8, 10)

  const tableView = model.getTableView({ dataViewId: "AsPanel9_30" })!;
  tableView.reorderedProperties.reorderingIds = ["Comment", "From", "Subject"];

  return presenter;
}

export function buildApplication() {
  return;
}
