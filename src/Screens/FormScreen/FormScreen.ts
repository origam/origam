import axios from "axios";
import xmlJs from "xml-js";
import { ML } from "../../utils/types";
import { unpack } from "../../utils/objects";
import { IScreenType, IMainViews } from "../types";
import { IFormScreen, IFormScreenMachine } from "./types";
import { observable, computed, action, reaction, autorun } from "mobx";
import { IDataViewMediator02 } from "../../DataView/DataViewMediator02";
import { IApi } from "../../Api/IApi";
import { START_SCREEN, STOP_SCREEN } from "../ScreensActions";
import {
  IDispatcher,
  STATE_VARIABLE_CHANGED,
  stateVariableChanged
} from "../../utils/mediator";


function spc(n: number) {
  let result = "";
  for (let i = 0; i < n; i++) {
    result = result + " ";
  }
  return result;
}

export class FormScreen implements IFormScreen {
  constructor(
    public P: {
      menuItemId: string;
      order: number;
      menuItemLabel: string;
      mainViews: ML<IMainViews>;
      machine: ML<IFormScreenMachine>;
      api: ML<IApi>;
    }
  ) {}

  type: IScreenType.FormRef = IScreenType.FormRef;

  getParent(): IDispatcher {
    return this;
  }

  @action.bound dispatch(event: any) {
    /*switch (event.type) {
      default:
        this.getParent().dispatch(event);
    }*/

    console.log("FormScreen received:", event);
    // debugger
    // THIS IS ROOT FOR NOW
    switch (event.type) {
      case STATE_VARIABLE_CHANGED: {
        this.stateVariableChanged = true;
        break;
      }
      default:
        this.downstreamDispatch(event);
    }
  }

  listeners = new Map<number, (event: any) => void>();
  idgen = 0;
  @action.bound
  listen(cb: (event: any) => void): () => void {
    const myId = this.idgen++;
    this.listeners.set(myId, cb);
    return () => this.listeners.delete(myId);
  }

  @observable stateVariableChanged = false;

  disposers: Array<() => void> = [];
  downstreamDispatch(event: any) {
    switch (event.type) {
      case START_SCREEN: {
        this.machine.start();
        this.disposers.push(
          autorun(() => {
            if (this.stateVariableChanged) {
              this.stateVariableChanged = false;
              this.dispatch({ type: "" });
            }
          })
        );
        break;
      }
      case STOP_SCREEN: {
        this.machine.stop();
        this.disposers.forEach(d => d());
        break;
      }
    }
    for (let l of this.listeners.values()) {
      console.log(l)
      l(event);
    }
    this.dataViews.forEach(view => view.downstreamDispatch(event));
  }

  @observable.ref uiStructure: any = undefined;

  @action.bound setUIStructure(uiStructure: any) {
    console.log("setUIStructure", uiStructure);
    this.uiStructure = uiStructure;
  }

  @observable dataViews: IDataViewMediator02[] = [];

  @computed get dataViewMap(): Map<string, IDataViewMediator02> {
    return new Map(
      this.dataViews.map(dv => [dv.id, dv] as [string, IDataViewMediator02])
    );
  }

  @action.bound setDataViews(views: IDataViewMediator02[]) {
    console.log("setDataViews", views);
    this.dataViews = views;
  }

  @action.bound
  activate(): void {
    console.log("Activating FormScreen", this.menuItemId, this.order);
  }

  @action.bound
  deactivate() {
    console.log("Deactivating FormScreen", this.menuItemId, this.order);
  }

  @action.bound
  open(): void {
    console.log("Opening FormScreen", this.menuItemId, this.order);
    /*this.machine.start();
    for (let dv of this.dataViews) {
      dv.aStartView.do();
    }*/
  }

  @action.bound
  activateDataViews() {
    /*for (let dv of this.dataViews) {
      dv.aStartView.do();
    }*/
  }

  @action.bound
  close(): void {
    console.log("Closing FormScreen", this.menuItemId, this.order);
    for (let dv of this.dataViews) {
      dv.aStopView.do();
    }
    this.machine.stop();
  }

  isLoading: boolean = false;

  @computed get isVisible(): boolean {
    return this.mainViews.isActiveView(this);
  }

  get menuItemLabel(): string | undefined {
    return this.P.menuItemLabel;
  }

  @observable screenLabel: string | undefined;

  @computed get label() {
    return (this.screenLabel ? this.screenLabel : this.menuItemLabel) || "";
  }

  get title() {
    return this.label;
  }

  get mainViews() {
    return unpack(this.P.mainViews);
  }

  get menuItemId(): string {
    return this.P.menuItemId;
  }

  get order(): number {
    return this.P.order;
  }

  get machine() {
    return unpack(this.P.machine);
  }

  get api() {
    return unpack(this.P.api);
  }
}
