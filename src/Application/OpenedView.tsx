import { observable, action, computed } from "mobx";
import { IOpenedView, IOpenedViewCollection } from "./types";
import { ILoadingGate, IAPI } from "src/DataLoadingStrategy/types";
import { IComponentBindingsModel } from "src/componentBindings/types";
import { getToken } from "src/DataLoadingStrategy/api";
import * as xmlJs from "xml-js";
import { reactProcessNode } from "src/screenInterpreter/ScreenInterpreter";

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
  public livesIn: IOpenedViewCollection;

  @computed
  public get isActive(): boolean {
    return this.livesIn.isActiveView(this);
  }

  @computed get order(): number {
    return this.livesIn.getViewOrder(this);
  }

  @action.bound
  public start() {
    this.api.loadScreen({ id: this.id, token: getToken() }).then(
      action((response: any) => {
        const { data } = response;
        const xmlObj = xmlJs.xml2js(data, { compact: false });
        const reactTree = reactProcessNode(xmlObj, []);
        this.reactTree = reactTree;
        /* this.componentBindingsModel = new ComponentBindingsModel(
          interpretedResult.collectComponentBindings
        );*/
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
