import { observable, computed, action } from "mobx";
import { start } from "repl";
import * as React from "react";
import axios from "axios";
import * as xmlJs from "xml-js";
import { parseScreenDef } from "src/screenInterpreter/interpreter";
import { buildReactTree } from "src/screenInterpreter/uiBuilder";
import { interpretMenu } from "src/MainMenu/MainMenuComponent";
import { IAPI } from "../DataLoadingStrategy/types";
import { getToken } from "../DataLoadingStrategy/api";
import { ComponentBindingsModel } from "src/componentBindings/ComponentBindingsModel";

export interface IOpenedView {
  id: string;
  subid: string;
  label: string;
  reactTree: React.ReactNode;
  start(): void;
  stop(): void;
  componentBindingsModel: ComponentBindingsModel;
}

class OpenedView implements IOpenedView {
  constructor(
    public id: string,
    public subid: string,
    public label: string,
    public api: IAPI
  ) {}

  @observable.ref public reactTree: React.ReactNode = null;

  public componentBindingsModel: ComponentBindingsModel;

  @action.bound
  public start() {
    this.api.loadScreen({ id: this.id, token: getToken() }).then(
      action((response: any) => {
        const { data } = response;
        const xmlObj = xmlJs.xml2js(data, { compact: false });
        const interpretedResult = parseScreenDef(xmlObj);
        const reactTree = buildReactTree(interpretedResult.uiNode);
        this.reactTree = reactTree;
        this.componentBindingsModel = new ComponentBindingsModel(interpretedResult.collectComponentBindings);
      })
    );
  }

  @action.bound
  public stop() {
    return;
  }
}

let subidGen = 1;

export class MainViewEngine {
  constructor(public api: IAPI) {}

  @observable public activeView: IOpenedView | undefined = undefined;
  @observable public openedViewsState: IOpenedView[] = [];

  @observable.ref public reactMenu: React.ReactNode = null;

  @action.bound
  public start() {
    this.api.loadMenu({ token: getToken() }).then(
      action((response: any) => {
        const { data } = response;
        const xmlObj = xmlJs.xml2js(data, { compact: false });
        const reactMenu = interpretMenu(xmlObj);
        this.reactMenu = reactMenu;
      })
    );
  }

  @computed
  public get openedViews(): Array<{ order: number; view: IOpenedView }> {
    const viewMap = {};
    const result = [];
    for (const view of this.openedViewsState) {
      if (viewMap[view.id] === undefined) {
        viewMap[view.id] = 0;
      } else {
        viewMap[view.id]++;
      }
      result.push({ order: viewMap[view.id] as number, view });
    }
    return result;
  }

  @action.bound
  public closeView(id: string, subid: string) {
    const index = this.openedViewsState.findIndex(
      o => o.id === id && o.subid === subid
    );

    if (index > -1) {
      const oldView = this.openedViewsState[index];
      oldView.stop();
      if (
        !(
          this.activeView &&
          this.activeView.id === oldView.id &&
          this.activeView.subid === oldView.subid
        )
      ) {
        this.openedViewsState.splice(index, 1);
      } else if (this.openedViewsState.length === 1) {
        this.openedViewsState.splice(index, 1);
        this.activateView(undefined, undefined);
      } else {
        this.openedViewsState.splice(index, 1);
        const view = this.openedViewsState[Math.max(0, index - 1)];
        this.activateView(view.id, view.subid);
      }
    }
  }

  @action.bound
  public openOrActivateView(id: string, label: string) {
    let view = this.openedViewsState.find(o => o.id === id);
    if (!view) {
      view = this.openView(id, label);
    }
    this.activateView(id, view.subid);
  }

  @action.bound
  public activateView(id: string | undefined, subid: string | undefined) {
    if (!id || !subid) {
      this.activeView = undefined;
      return;
    }
    this.activeView = this.openedViewsState.find(
      o => o.id === id && o.subid === subid
    );
  }

  @action.bound
  public openView(id: string, label: string) {
    const view = new OpenedView(id, `${subidGen++}`, label, this.api);
    view.start();
    this.openedViewsState.push(view);
    this.activateView(view.id, view.subid);
    return view;
  }

  @action.bound
  public handleMenuFormItemClick(event: any, id: string, label: string) {
    console.log("click", label);
    if (event.ctrlKey) {
      this.openView(id, label);
    } else {
      this.openOrActivateView(id, label);
    }
  }
}
