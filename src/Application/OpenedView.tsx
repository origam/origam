import { observable, action } from "mobx";
import { IOpenedView } from "./types";
import { ILoadingGate, IAPI } from "src/DataLoadingStrategy/types";
import { IComponentBindingsModel } from "src/componentBindings/types";
import { getToken } from "src/DataLoadingStrategy/api";
import * as xmlJs from "xml-js";
import { parseScreenDef } from "src/screenInterpreter/interpreter";
import { buildReactTree } from "src/screenInterpreter/uiBuilder";
import { ComponentBindingsModel } from "src/componentBindings/ComponentBindingsModel";

export class OpenedView implements IOpenedView, ILoadingGate {
  @observable public isLoadingAllowed: boolean = false;

  constructor(
    public id: string,
    public subid: string,
    public label: string,
    public api: IAPI
  ) {}

  @observable.ref public reactTree: React.ReactNode = null;

  public componentBindingsModel: IComponentBindingsModel;

  @action.bound
  public start() {
    this.api.loadScreen({ id: this.id, token: getToken() }).then(
      action((response: any) => {
        const { data } = response;
        const xmlObj = xmlJs.xml2js(data, { compact: false });
        const interpretedResult = parseScreenDef(xmlObj);
        const reactTree = buildReactTree(interpretedResult.uiNode);
        this.reactTree = reactTree;
        this.componentBindingsModel = new ComponentBindingsModel(
          interpretedResult.collectComponentBindings
        );
      })
    );
  }

  @action.bound public unlockLoading() {
    this.isLoadingAllowed = true;
  }

  @action.bound
  public stop() {
    return;
  }
}
