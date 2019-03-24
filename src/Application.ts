import * as presenterCollect from "./presenter/factory/screen/collect";
import { ScreenFactory as PresenterScreenFactory } from "./presenter/factory/screen/ScreenFactory";

import * as modelCollect from "./model/ScreenInterpreter/collect";
import { ScreenFactory as ModelScreenFactory } from "./model/factory/ScreenFactory";

import { IScreenXml } from "./common/types/IScreenXml";
import { IModel } from "./model/types/IModel";

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
  const screenFactory = new PresenterScreenFactory();
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

  return presenter;
}

export function buildApplication() {
  return;
}
