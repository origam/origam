import { observable, computed, action } from "mobx";
import { start } from "repl";
import * as React from "react";
import axios from "axios";
import * as xmlJs from "xml-js";
import { parseScreenDef } from "src/screenInterpreter/interpreter";
import { buildReactTree } from "src/screenInterpreter/uiBuilder";
import { interpretMenu } from "src/MainMenu/MainMenuComponent";

interface IOpenedView {
  id: string;
  subid: string;
  label: string;
  reactTree: React.ReactNode;
}

class OpenedView implements IOpenedView {
  constructor(public id: string, public subid: string, public label: string) {}

  @observable.ref public reactTree: React.ReactNode = null;

  @action.bound
  public start() {
    axios
      /*.get(
        {
          "b2a2194c-c2a5-4236-837a-08c1b720fc96": "/screen01.xml",
          "f96ad0ca-7be8-4f11-a397-358d02abd0b2": "/screen02.xml",
          "8713c618-b4fb-4749-ab28-0811b85481b0": "/screen03.xml",
          "3272db8a-172f-48ee-9e50-ef65219089c2": "/screen04.xml"
        }[this.id]
      )*/
      .get(`/api/MetaData/GetScreeSection`, {
        headers: { Authorization: `Bearer ${TOKEN}` },
        params: {
          id: this.id
        }
      })
      .then(
        action((response: any) => {
          const { data } = response;
          const xmlObj = xmlJs.xml2js(data, { compact: false });
          const interpretedResult = parseScreenDef(xmlObj);
          const reactTree = buildReactTree(interpretedResult.uiNode);
          // console.log(interpretedResult);
          // console.log(reactTree);
          this.reactTree = reactTree;
        })
      );
  }

  @action.bound
  public stop() {
    return;
  }
}

let subidGen = 1;

const TOKEN =
  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUFRMRTU0MFxccGF2ZWwiLCJuYmYiOiIxNTQyODMwNTc3IiwiZXhwIjoiMTU0MjkxNjk3NyJ9.YVxGKmYrvZdTkenY26uaT6toMRl8b30dJmNpH1xjgbE";

export class MainViewEngine {
  @observable public activeView: IOpenedView | undefined = undefined;
  @observable public openedViewsState: IOpenedView[] = [];

  @observable.ref public reactMenu: React.ReactNode = null;

  @action.bound
  public start() {
    axios
      .get("/api/MetaData/GetMenu", {
        headers: { Authorization: `Bearer ${TOKEN}` }
      })
      .then(
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
    const view = new OpenedView(id, `${subidGen++}`, label);
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
